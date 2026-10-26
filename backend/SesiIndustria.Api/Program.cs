using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SesiIndustria.Api.Contracts;
using SesiIndustria.Api.Courses;
using SesiIndustria.Api.Data;

// Ponto de entrada da API. A inicialização segue a ordem abaixo:
// configuração -> serviços -> aplicação -> banco -> endpoints -> servidor.
// As regras de consulta ficam em Courses/; a persistência, em Data/;
// os formatos enviados ao Angular, em Contracts/.

// 1. Preparar a configuração e registrar os serviços disponíveis por injeção de dependência.
// CreateBuilder lê appsettings, ambiente e argumentos como --urls http://localhost:5080.
var builder = WebApplication.CreateBuilder(args);
// Padroniza corpos de erro; o documento OpenAPI descreve o contrato dos endpoints.
builder.Services.AddProblemDetails();
builder.Services.AddOpenApi();

// 2. Configurar a persistência em um arquivo SQLite.
// Database:Path permite separar o banco de desenvolvimento, publicação e testes.
// Um caminho relativo parte da pasta de conteúdo da API, não da pasta do terminal.
var configuredPath = builder.Configuration["Database:Path"] ?? "App_Data/catalog.db";
var databasePath = Path.GetFullPath(configuredPath, builder.Environment.ContentRootPath);
Directory.CreateDirectory(Path.GetDirectoryName(databasePath)!);
var connectionString = new SqliteConnectionStringBuilder
{
    DataSource = databasePath,
    ForeignKeys = true
}.ToString();
// CatalogDbContext traduz as consultas de objetos para SQL e configura os relacionamentos.
// DbContext e repositório têm escopo de requisição: pedidos diferentes não compartilham contexto.
builder.Services.AddDbContext<CatalogDbContext>(options => options.UseSqlite(connectionString));
builder.Services.AddScoped<CourseRepository>();

// 3. Construir a aplicação e definir o tratamento comum de erros HTTP.
var app = builder.Build();
app.UseExceptionHandler();
app.UseStatusCodePages();

// 4. Preparar o banco antes de começar a receber requisições.
// A inicialização acontece fora de uma requisição; por isso cria seu próprio escopo descartável.
// A migration aplica o esquema pendente. O seeder importa o JSON apenas se não houver cursos.
await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    await db.Database.MigrateAsync();
    await CatalogSeeder.SeedAsync(db, Path.Combine(app.Environment.ContentRootPath, "Data", "Seed", "courses.json"));
}

// 5. Registrar os endpoints. Nesta Minimal API, cada MapGet liga uma URL à função que a atende.
// O OpenAPI fica em /openapi/v1.json, conforme a configuração padrão de AddOpenApi.
app.MapOpenApi();

// Listagem: os parâmetros opcionais vêm da URL (?search=...&category=...).
// O ASP.NET fornece o repositório registrado acima e o sinal de cancelamento da requisição.
// A função valida a entrada; CourseRepository monta e executa a consulta no banco.
app.MapGet("/api/courses", async (string? search, string? category, string? modality, string? status,
    bool? featured, CourseRepository repository, CancellationToken cancellationToken) =>
{
    var filter = new CourseQuery(search, category, modality, status, featured);
    var errors = filter.Validate();
    // Consulta inválida retorna 400; consulta válida retorna 200, inclusive se items estiver vazio.
    return errors.Count > 0
        ? Results.ValidationProblem(errors, title: "Parâmetros de consulta inválidos")
        : Results.Ok(await repository.ListAsync(filter, cancellationToken));
}).WithName("ListCourses").WithSummary("Consulta cursos com busca e filtros combinados.")
    .Produces<CoursePageDto>().ProducesValidationProblem();

// Detalhes: {slug} é o identificador legível da URL, como matematica-para-a-precisao.
// O DTO inclui módulos, aulas e textos necessários à página do curso e à AVA.
app.MapGet("/api/courses/{slug}", async (string slug, CourseRepository repository, CancellationToken cancellationToken) =>
{
    var course = await repository.FindAsync(slug, cancellationToken);
    return course is null
        ? Results.Problem(statusCode: 404, title: "Curso não encontrado")
        : Results.Ok(course);
}).WithName("GetCourse").WithSummary("Retorna detalhes, módulos, aulas e conteúdos de um curso.")
    .Produces<CourseDto>().ProducesProblem(404);

// Verifica a conexão com o banco sem transferir a listagem de cursos: 200 ou 503.
app.MapGet("/api/health", async (CatalogDbContext db, CancellationToken cancellationToken) =>
    await db.Database.CanConnectAsync(cancellationToken)
        ? Results.Ok(new { status = "ok" })
        : Results.Problem(statusCode: 503, title: "Catálogo temporariamente indisponível"));

// Um GET de API inexistente responde 404 em JSON, sem receber a página HTML do Angular.
app.MapGet("/api/{**path}", () => Results.Problem(statusCode: 404, title: "Endpoint não encontrado"))
    .ExcludeFromDescription();

// 6. Servir o frontend quando o pacote publicado contém wwwroot/index.html.
// No desenvolvimento, o Angular roda separado e encaminha /api pelo proxy.conf.json.
// O fallback permite abrir diretamente /cursos ou /ava/{slug}; a rota é resolvida pelo Angular.
if (File.Exists(Path.Combine(app.Environment.WebRootPath ?? Path.Combine(app.Environment.ContentRootPath, "wwwroot"), "index.html")))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
    app.MapFallbackToFile("index.html");
}

// 7. Iniciar o servidor e manter o processo atendendo até seu encerramento.
app.Run();

// WebApplicationFactory usa o mesmo ponto de entrada nos testes de integração.
public partial class Program;

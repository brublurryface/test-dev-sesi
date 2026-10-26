using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using SesiIndustria.Api.Contracts;
using SesiIndustria.Api.Data;

namespace SesiIndustria.Api.Tests;

/// <summary>
/// Testes de integração: iniciam a API real com SQLite temporário e verificam respostas HTTP.
/// O banco não é substituído por uma coleção em memória, preservando as restrições relacionais.
/// </summary>
public sealed class CatalogApiTests
{
    // Confere a variedade exigida e o contrato resumido, que não deve transferir todo o conteúdo.
    [Fact]
    public async Task CatalogHasRequiredVarietyAndDoesNotExposeDetailInTheList()
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        var page = await client.GetFromJsonAsync<CoursePageDto>("/api/courses");
        Assert.NotNull(page);
        Assert.Equal(7, page.Total);
        Assert.Equal(page.Total, page.Items.Length);
        Assert.Equal(3, page.Items.Select(c => c.Category).Distinct().Count());
        Assert.Equal(2, page.Items.Select(c => c.Modality).Distinct().Count());
        Assert.Equal(3, page.Items.Select(c => c.Status).Distinct().Count());
        Assert.All(page.Items, course =>
        {
            Assert.True(course.Hours > 0);
            Assert.NotEmpty(course.Audience);
            Assert.StartsWith("assets/", course.Image);
        });
        var json = await client.GetStringAsync("/api/courses");
        Assert.DoesNotContain("\"modules\"", json);
        Assert.DoesNotContain("\"content\"", json);
    }

    // Variações do termo percorrem a mesma normalização usada na carga inicial do banco.
    [Theory]
    [InlineData("%20MATEMATICA%20", "matematica-para-a-precisao")]
    [InlineData("ci%C3%AAncia%20dos%20materiais", "ciencia-dos-materiais")]
    [InlineData("residuos", "gestao-de-residuos")]
    public async Task SearchIgnoresCaseAccentsAndOuterSpaces(string search, string expectedSlug)
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        var page = await client.GetFromJsonAsync<CoursePageDto>($"/api/courses?search={search}");
        Assert.NotNull(page);
        Assert.Equal(expectedSlug, Assert.Single(page.Items).Slug);
        Assert.Equal(1, page.Total);
    }

    [Fact]
    public async Task AllFiltersAreCombinedAndClearingReturnsTheWholeCatalog()
    {
        // Usa combinações compatível e incompatível para verificar o AND entre os critérios.
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        const string query = "search=MATEMATICA&category=Matem%C3%A1tica&modality=Online&status=Dispon%C3%ADvel";
        var result = await client.GetFromJsonAsync<CoursePageDto>($"/api/courses?{query}");
        Assert.Equal("matematica-para-a-precisao", Assert.Single(result!.Items).Slug);
        var empty = await client.GetFromJsonAsync<CoursePageDto>($"/api/courses?{query.Replace("modality=Online", "modality=Presencial")}");
        Assert.Empty(empty!.Items);
        Assert.Equal(0, empty.Total);
        Assert.Equal(7, (await client.GetFromJsonAsync<CoursePageDto>("/api/courses"))!.Total);
    }

    [Theory]
    [InlineData("search=inexistente")]
    [InlineData("search=%27%20OR%201%3D1%20--")]
    public async Task UnmatchedOrSqlLikeTextIsOnlyASearchTerm(string query)
    {
        // Texto semelhante a um comando SQL continua sendo apenas um valor de busca.
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        var result = await client.GetFromJsonAsync<CoursePageDto>($"/api/courses?{query}");
        Assert.NotNull(result);
        Assert.Empty(result.Items);
        Assert.Equal(0, result.Total);
    }

    [Theory]
    [InlineData("category=Desconhecida")]
    [InlineData("modality=Hibrido")]
    [InlineData("status=Invalido")]
    public async Task InvalidFilterReturnsValidationProblem(string query)
    {
        // A API precisa recusar a entrada inválida mesmo quando o cliente não é o Angular.
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        using var response = await client.GetAsync($"/api/courses?{query}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
        var problem = await response.Content.ReadFromJsonAsync<ValidationProblemDetails>();
        Assert.NotNull(problem);
        Assert.NotEmpty(problem.Errors);
    }

    [Fact]
    public async Task LongSearchIsRejectedAndHighlightsAreFilteredByTheServer()
    {
        // Verifica o limite de busca e o filtro usado pela landing page diretamente pelo HTTP.
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        using var invalid = await client.GetAsync($"/api/courses?search={new string('a', 121)}");
        Assert.Equal(HttpStatusCode.BadRequest, invalid.StatusCode);
        var featured = await client.GetFromJsonAsync<CoursePageDto>("/api/courses?featured=true");
        Assert.Equal(6, featured!.Total);
        Assert.All(featured.Items, course => Assert.True(course.Featured));
    }

    [Fact]
    public async Task DetailContainsOrderedModulesLessonsAndReferenceTextsFromTheDatabase()
    {
        // O curso de referência preserva os textos fornecidos; descrições fictícias são identificadas.
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        var course = await client.GetFromJsonAsync<CourseDto>("/api/courses/matematica-para-a-precisao");
        Assert.NotNull(course);
        Assert.Equal(10, course.Modules.Length);
        Assert.Equal(10, course.Faqs.Length);
        Assert.All(course.Modules, module =>
        {
            var lesson = Assert.Single(module.Lessons);
            Assert.Equal("Aula 1", lesson.Title);
            Assert.StartsWith("Exemplo fictício:", lesson.Description);
            Assert.NotEmpty(module.Topics);
        });
        Assert.Equal(["introducao", "boas-praticas", "extras"], course.Introduction.Select(l => l.Id).ToArray());
        var guide = course.Introduction.Single(l => l.Id == "boas-praticas");
        Assert.Equal(8, guide.Content.Length);
        Assert.Contains(guide.Content, block => block.Kind == "heading" && block.Text == "Recomendações");
        Assert.Contains(course.OnlineInformation, block => block.Kind == "list" && block.Items.Length > 0);
        var all = await client.GetFromJsonAsync<CoursePageDto>("/api/courses");
        foreach (var summary in all!.Items)
        {
            var detail = await client.GetFromJsonAsync<CourseDto>($"/api/courses/{summary.Slug}");
            Assert.NotEmpty(detail!.Modules);
            Assert.All(detail.Modules, module => Assert.StartsWith("Exemplo fictício:", Assert.Single(module.Lessons).Description));
            // Itens sem texto de referência também têm uma descrição demonstrativa identificada.
            Assert.All(detail.Introduction.Where(lesson => lesson.Content.Length == 0),
                lesson => Assert.StartsWith("Exemplo fictício:", lesson.Description));
        }
    }

    [Theory]
    [InlineData("/api/courses/nao-existe")]
    [InlineData("/api/endpoint-inexistente")]
    public async Task UnknownApiResourcesReturn404InsteadOfAnHtmlPage(string path)
    {
        // Impede que o fallback das rotas Angular esconda recursos inexistentes da API.
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        using var response = await client.GetAsync(path);
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    [Fact]
    public async Task ExistingDatabaseReceivesMissingDescriptionsWithoutReplacingReferenceOrEditedContent()
    {
        // Recria o esquema anterior e seus itens introdutórios vazios para testar uma atualização real.
        var directory = Path.Combine(Path.GetTempPath(), $"sesi-upgrade-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "catalog.db");
        Directory.CreateDirectory(directory);
        try
        {
            var options = new DbContextOptionsBuilder<CatalogDbContext>().UseSqlite($"Data Source={path};Foreign Keys=True").Options;
            await using (var db = new CatalogDbContext(options))
            {
                await db.GetService<IMigrator>().MigrateAsync("20261023215623_InitialCatalog");
                var contentRoot = Path.GetFullPath("../../../../SesiIndustria.Api", AppContext.BaseDirectory);
                await CatalogSeeder.SeedAsync(db, Path.Combine(contentRoot, "Data", "Seed", "courses.json"));
                await db.Database.ExecuteSqlRawAsync("UPDATE Lessons SET Description = '' WHERE ModuleId IS NULL");
                await db.Database.ExecuteSqlRawAsync("UPDATE Lessons SET Description = 'Apresentação preservada no banco.' WHERE CourseId = 2 AND Key = 'introducao'");
                const string preservedContent = """[{"kind":"paragraph","text":"Material preservado no banco.","items":[]}]""";
                await db.Database.ExecuteSqlInterpolatedAsync($"UPDATE Lessons SET ContentJson = {preservedContent} WHERE CourseId = 2 AND Key = 'extras'");
            }

            // O início normal da API aplica a atualização antes de atender; o seeder preserva o banco.
            using var app = new CatalogApplication(path);
            using var client = app.CreateClient();
            var math = await client.GetFromJsonAsync<CourseDto>("/api/courses/matematica-para-a-precisao");
            Assert.StartsWith("Exemplo fictício:", math!.Introduction.Single(lesson => lesson.Id == "introducao").Description);
            Assert.StartsWith("Exemplo fictício:", math.Introduction.Single(lesson => lesson.Id == "extras").Description);
            Assert.Equal(8, math.Introduction.Single(lesson => lesson.Id == "boas-praticas").Content.Length);
            var geometry = await client.GetFromJsonAsync<CourseDto>("/api/courses/geometria-e-modelagem");
            Assert.Equal("Apresentação preservada no banco.", geometry!.Introduction.Single(lesson => lesson.Id == "introducao").Description);
            Assert.Equal("Material preservado no banco.", Assert.Single(geometry.Introduction.Single(lesson => lesson.Id == "extras").Content).Text);
            Assert.StartsWith("Exemplo fictício:", geometry.Introduction.Single(lesson => lesson.Id == "boas-praticas").Description);
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task DatabaseChangesSurviveRestartAndSeedDoesNotOverwriteThem()
    {
        // Duas instâncias da API usam o mesmo arquivo, simulando encerramento e reinício.
        // Uma alteração direta permite verificar que o seeder não recarrega o JSON sobre o banco.
        var directory = Path.Combine(Path.GetTempPath(), $"sesi-persistence-{Guid.NewGuid():N}");
        var path = Path.Combine(directory, "catalog.db");
        try
        {
            using (var first = new CatalogApplication(path))
            using (var client = first.CreateClient())
                Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/health")).StatusCode);

            await using (var connection = new SqliteConnection($"Data Source={path};Foreign Keys=True"))
            {
                await connection.OpenAsync();
                await using var command = connection.CreateCommand();
                command.CommandText = "UPDATE Lessons SET Description = $description WHERE CourseId = 1 AND ModuleId IS NOT NULL";
                command.Parameters.AddWithValue("$description", "Exemplo fictício: texto persistido no banco para o teste.");
                Assert.Equal(10, await command.ExecuteNonQueryAsync());
            }
            using var second = new CatalogApplication(path);
            using var restartedClient = second.CreateClient();
            var course = await restartedClient.GetFromJsonAsync<CourseDto>("/api/courses/matematica-para-a-precisao");
            Assert.All(course!.Modules, module => Assert.Equal("Exemplo fictício: texto persistido no banco para o teste.", Assert.Single(module.Lessons).Description));
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(directory, true);
        }
    }

    [Fact]
    public async Task DatabaseRejectsALessonAssignedToAnotherCoursesModule()
    {
        // A tentativa ocorre por SQL para confirmar a proteção da FK, fora das regras da aplicação.
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        await client.GetAsync("/api/health");
        await using var connection = new SqliteConnection($"Data Source={app.DatabasePath};Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "UPDATE Lessons SET ModuleId = (SELECT Id FROM Modules WHERE CourseId = 2 LIMIT 1) WHERE CourseId = 1 AND ModuleId IS NOT NULL";
        var exception = await Assert.ThrowsAsync<SqliteException>(() => command.ExecuteNonQueryAsync());
        Assert.Equal(19, exception.SqliteErrorCode);
    }
}

/// <summary>
/// Hospeda o mesmo Program.cs da aplicação e troca apenas configuração de banco e logs.
/// Cada instância recebe um arquivo isolado, salvo quando o teste fornece um caminho compartilhado.
/// </summary>
internal sealed class CatalogApplication : WebApplicationFactory<Program>
{
    private readonly bool ownsDirectory;
    public string DatabasePath { get; }

    public CatalogApplication(string? databasePath = null)
    {
        ownsDirectory = databasePath is null;
        DatabasePath = databasePath ?? Path.Combine(Path.GetTempPath(), $"sesi-api-{Guid.NewGuid():N}", "catalog.db");
    }

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // O conteúdo da API contém o JSON inicial; Database:Path redireciona a persistência do teste.
        builder.UseContentRoot(Path.GetFullPath("../../../../SesiIndustria.Api", AppContext.BaseDirectory));
        builder.UseSetting("Database:Path", DatabasePath);
        builder.UseSetting("Logging:LogLevel:Default", "Warning");
    }

    protected override void Dispose(bool disposing)
    {
        base.Dispose(disposing);
        // Só remove diretórios criados por esta instância; o teste de reinício gerencia o seu próprio.
        // Libera conexões do pool antes de apagar o arquivo, inclusive em sistemas como Windows.
        if (disposing && ownsDirectory && Directory.Exists(Path.GetDirectoryName(DatabasePath)))
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(Path.GetDirectoryName(DatabasePath)!, true);
        }
    }
}

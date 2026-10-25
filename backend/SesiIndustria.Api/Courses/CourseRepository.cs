using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SesiIndustria.Api.Contracts;
using SesiIndustria.Api.Data;

namespace SesiIndustria.Api.Courses;

/// <summary>
/// Executa as consultas de leitura do catálogo e converte entidades do banco em contratos HTTP.
/// O contexto recebido no construtor é fornecido pela injeção de dependência em Program.cs.
/// As respostas retornam DTOs; o endpoint decide o status HTTP.
/// </summary>
public sealed class CourseRepository(CatalogDbContext db)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Combina todos os critérios preenchidos e retorna apenas os campos usados nos cartões.
    /// Sem paginação neste catálogo: Total corresponde ao tamanho da lista filtrada.
    /// </summary>
    public async Task<CoursePageDto> ListAsync(CourseQuery filter, CancellationToken cancellationToken)
    {
        // A consulta só lê dados; AsNoTracking evita manter objetos para futuras atualizações.
        var query = db.Courses.AsNoTracking();
        var term = CourseQuery.Normalize(filter.Search ?? "");
        // Os Where acumulam condições AND. O SQL ainda não é executado nesta montagem.
        if (term.Length > 0) query = query.Where(c => c.NormalizedName.Contains(term));
        if (!string.IsNullOrWhiteSpace(filter.Category)) query = query.Where(c => c.Category == filter.Category);
        if (!string.IsNullOrWhiteSpace(filter.Modality)) query = query.Where(c => c.Modality == filter.Modality);
        if (!string.IsNullOrWhiteSpace(filter.Status)) query = query.Where(c => c.Status == filter.Status);
        if (filter.Featured.HasValue) query = query.Where(c => c.Featured == filter.Featured.Value);
        // Select projeta o resumo sem carregar módulos/aulas. ToArrayAsync executa a consulta.
        // O EF parametriza os valores; a busca não é concatenada ao texto de um comando SQL.
        // O token permite sinalizar cancelamento se a requisição HTTP for interrompida.
        var items = await query.OrderBy(c => c.Id).Select(c => new CourseSummaryDto
        {
            Id = c.Id,
            Slug = c.Slug,
            Name = c.Name,
            ShortName = c.ShortName,
            Description = c.Description,
            Category = c.Category,
            Modality = c.Modality,
            Hours = c.Hours,
            Audience = c.Audience,
            Status = c.Status,
            Image = c.Image,
            ImageAlt = c.ImageAlt,
            AvailableAt = c.AvailableAt,
            Featured = c.Featured
        }).ToArrayAsync(cancellationToken);
        return new CoursePageDto(items, items.Length);
    }

    /// <summary>
    /// Carrega um curso e seus relacionamentos; retorna null quando o slug não existe.
    /// O chamador transforma essa ausência em 404, sem confundi-la com uma lista vazia.
    /// </summary>
    public async Task<CourseDto?> FindAsync(string slug, CancellationToken cancellationToken)
    {
        // Include solicita os relacionamentos que serão usados no DTO.
        // AsSplitQuery evita multiplicar linhas ao combinar várias coleções em um único JOIN.
        // Aulas de introdução pertencem ao curso diretamente e têm ModuleId nulo.
        var course = await db.Courses.AsNoTracking().AsSplitQuery()
            .Include(c => c.Modules).ThenInclude(m => m.Topics)
            .Include(c => c.Modules).ThenInclude(m => m.Lessons)
            .Include(c => c.Lessons.Where(l => l.ModuleId == null))
            .Include(c => c.Faqs)
            .SingleOrDefaultAsync(c => c.Slug == slug, cancellationToken);
        if (course is null) return null;
        // Position preserva a ordem da referência, independentemente da ordem física das tabelas.
        return new CourseDto
        {
            Id = course.Id,
            Slug = course.Slug,
            Name = course.Name,
            ShortName = course.ShortName,
            Description = course.Description,
            About = course.About,
            Category = course.Category,
            Modality = course.Modality,
            Hours = course.Hours,
            Audience = course.Audience,
            Status = course.Status,
            Image = course.Image,
            ImageAlt = course.ImageAlt,
            AvailableAt = course.AvailableAt,
            Featured = course.Featured,
            Certification = course.Certification,
            Modules = course.Modules.OrderBy(m => m.Position).Select(m => new CourseModuleDto(
                m.Title, m.Description,
                m.Topics.OrderBy(t => t.Position).Select(t => t.Text).ToArray(),
                m.Lessons.OrderBy(l => l.Position).Select(ToLesson).ToArray())).ToArray(),
            Introduction = course.Lessons.Where(l => l.ModuleId == null).OrderBy(l => l.Position).Select(ToLesson).ToArray(),
            Faqs = course.Faqs.OrderBy(f => f.Position).Select(f => new FaqDto(f.Question, f.Answer)).ToArray(),
            OnlineInformation = ReadBlocks(course.OnlineInformationJson)
        };
    }

    // Envia a chave estável da aula ao navegador, onde ela identifica a conclusão no progresso local.
    private static LessonDto ToLesson(LessonRecord lesson) => new(lesson.Key, lesson.Title, lesson.Description)
    {
        Content = ReadBlocks(lesson.ContentJson)
    };
    // Os blocos persistidos como JSON voltam como dados estruturados; o frontend renderiza texto.
    private static ContentBlockDto[] ReadBlocks(string json) =>
        JsonSerializer.Deserialize<ContentBlockDto[]>(json, JsonOptions) ?? [];
}

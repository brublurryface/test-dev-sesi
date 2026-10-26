using System.Globalization;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using SesiIndustria.Api.Contracts;
using SesiIndustria.Api.Courses;
using SesiIndustria.Api.Data;

namespace SesiIndustria.Api.Moodle;

/// <summary>Diagnóstico público sem URL privada, token, permissões ou mensagens do provedor.</summary>
public sealed class MoodleImportStatus
{
    public string State { get; internal set; } = "disabled";
    public string? CourseSlug { get; internal set; }
    public DateTimeOffset? LastSuccessAt { get; internal set; }
}

/// <summary>
/// Importação opcional e somente de leitura, executada uma vez a cada inicialização.
/// Valida toda a estrutura antes de gravar; mantém IDs locais das aulas para preservar progresso.
/// Metadados específicos do desafio (carga horária, imagens, FAQ e tópicos) permanecem no catálogo.
/// </summary>
public sealed class MoodleImporter(MoodleClient client, CatalogDbContext db,
    IOptions<MoodleOptions> configuration, MoodleImportStatus status, ILogger<MoodleImporter> logger)
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    public async Task ImportAsync(CancellationToken stoppingToken = default)
    {
        var options = configuration.Value;
        if (!options.Enabled) return;
        status.CourseSlug = options.CourseSlug;
        status.State = "unavailable";
        if (!options.IsValid())
        {
            status.State = "invalid_configuration";
            logger.LogWarning("Integração Moodle sem configuração válida; catálogo preservado.");
            return;
        }
        // Limite total das três chamadas, independentemente dos timeouts individuais do cliente.
        using var deadline = CancellationTokenSource.CreateLinkedTokenSource(stoppingToken);
        deadline.CancelAfter(TimeSpan.FromSeconds(20));
        try
        {
            await ImportCourseAsync(options, deadline.Token);
            status.LastSuccessAt = DateTimeOffset.UtcNow;
            status.State = "synchronized";
            logger.LogInformation("Conteúdo Moodle importado para o catálogo local.");
        }
        catch (Exception error) when (error is HttpRequestException or OperationCanceledException or
            JsonException or InvalidDataException or InvalidOperationException or FormatException or
            KeyNotFoundException or ArgumentException or DbUpdateException or Microsoft.Data.Sqlite.SqliteException or
            System.Text.RegularExpressions.RegexMatchTimeoutException)
        {
            // Não registrar exceção/corpo externo: podem conter dados ou credenciais do provedor.
            db.ChangeTracker.Clear();
            logger.LogWarning("Importação Moodle não concluída; catálogo local preservado.");
        }
    }

    private async Task ImportCourseAsync(MoodleOptions options, CancellationToken cancellationToken)
    {
        var courseId = options.CourseId.ToString(CultureInfo.InvariantCulture);
        var courses = await client.CallAsync(options, "core_course_get_courses_by_field",
            new() { ["field"] = "id", ["value"] = courseId }, cancellationToken);
        var source = courses.GetProperty("courses").EnumerateArray().Single();
        // Confere o identificador do curso, evitando importar outro conteúdo por engano.
        if (source.GetProperty("id").GetInt32() != options.CourseId ||
            source.GetProperty("idnumber").GetString() != options.CourseSlug)
            throw new InvalidDataException("Curso Moodle não corresponde ao catálogo.");
        var sections = await client.CallAsync(options, "core_course_get_contents",
            new() { ["courseid"] = courseId }, cancellationToken);
        var pages = await client.CallAsync(options, "mod_page_get_pages_by_courses",
            new() { ["courseids[0]"] = courseId }, cancellationToken);
        var pageMap = pages.GetProperty("pages").EnumerateArray()
            .Where(page => page.GetProperty("course").GetInt32() == options.CourseId)
            .ToDictionary(page => page.GetProperty("coursemodule").GetInt32());
        var sectionMap = sections.EnumerateArray().ToDictionary(section => section.GetProperty("section").GetInt32());
        var course = await db.Courses.Include(c => c.Modules).ThenInclude(m => m.Lessons)
            .Include(c => c.Lessons).SingleAsync(c => c.Slug == options.CourseSlug, cancellationToken);
        var updates = new List<(LessonRecord Lesson, string Title, string Description, ContentBlockDto[] Content)>();
        var moduleUpdates = new List<(CourseModuleRecord Module, string Title)>();
        if (sectionMap.Count != course.Modules.Count + 1)
            throw new InvalidDataException("Quantidade de módulos diferente do curso de demonstração.");
        foreach (var module in course.Modules.OrderBy(m => m.Position))
        {
            var section = sectionMap[module.Position + 1];
            var page = SectionPages(section, pageMap).Single();
            moduleUpdates.Add((module, RequiredText(section, "name", 250)));
            updates.Add(ReadLesson(module.Lessons.Single(), page));
        }
        var introductions = SectionPages(sectionMap[0], pageMap)
            .ToDictionary(page => CourseQuery.Normalize(RequiredText(page, "name", 250)));
        foreach (var lesson in course.Lessons.Where(lesson => lesson.ModuleId is null))
            updates.Add(ReadLesson(lesson, introductions[CourseQuery.Normalize(lesson.Title)]));
        if (introductions.Count != 3 || updates.Count != 13 || pageMap.Count != 13)
            throw new InvalidDataException("Páginas incompletas ou inesperadas no curso de demonstração.");
        var name = RequiredText(source, "fullname", 250);
        // Só altera entidades depois de validar todas as páginas. Uma única transação grava o conjunto.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        course.Name = name;
        course.NormalizedName = CourseQuery.Normalize($"{name} {course.ShortName}");
        foreach (var (module, title) in moduleUpdates) module.Title = title;
        foreach (var update in updates)
        {
            update.Lesson.Title = update.Title;
            update.Lesson.Description = update.Description;
            update.Lesson.ContentJson = JsonSerializer.Serialize(update.Content, JsonOptions);
        }
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    private static IEnumerable<JsonElement> SectionPages(JsonElement section, Dictionary<int, JsonElement> pages) =>
        section.GetProperty("modules").EnumerateArray()
            .Where(module => module.GetProperty("modname").GetString() == "page")
            .Select(module => pages[module.GetProperty("id").GetInt32()]);

    private static (LessonRecord, string, string, ContentBlockDto[]) ReadLesson(LessonRecord lesson, JsonElement page)
    {
        var title = RequiredText(page, "name", 250);
        var description = MoodleText.Plain(page.GetProperty("intro").GetString() ?? "");
        var blocks = MoodleText.Blocks(page.GetProperty("content").GetString() ?? "");
        if (blocks.Length == 0) throw new InvalidDataException("Página sem conteúdo.");
        // Preserva a hierarquia de títulos já fornecida no protótipo quando o texto é o mesmo.
        var headings = (JsonSerializer.Deserialize<ContentBlockDto[]>(lesson.ContentJson, JsonOptions) ?? [])
            .Where(block => block.Kind == "heading").Select(block => block.Text).ToHashSet();
        blocks = blocks.Select(block => headings.Contains(block.Text) ? block with { Kind = "heading" } : block).ToArray();
        // Nas aulas demonstrativas, o editor contém a mesma descrição: evita repeti-la na tela.
        if (blocks.Length == 1 && blocks[0].Text == description) blocks = [];
        return (lesson, title, description, blocks);
    }

    private static string RequiredText(JsonElement source, string property, int maximum)
    {
        var text = MoodleText.Plain(source.GetProperty(property).GetString() ?? "");
        return !string.IsNullOrWhiteSpace(text) && text.Length <= maximum ? text :
            throw new InvalidDataException("Texto obrigatório ausente ou extenso.");
    }
}

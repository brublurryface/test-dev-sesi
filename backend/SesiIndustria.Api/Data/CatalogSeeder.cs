using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using SesiIndustria.Api.Contracts;
using SesiIndustria.Api.Courses;

namespace SesiIndustria.Api.Data;

/// <summary>
/// Converte o catálogo de Data/Seed/courses.json em registros SQLite na primeira inicialização.
/// Depois da carga, as requisições consultam o banco por CourseRepository.
/// </summary>
public static class CatalogSeeder
{
    private static readonly JsonSerializerOptions JsonOptions = new(JsonSerializerDefaults.Web);

    /// <summary>
    /// Valida a fonte e importa todos os cursos em uma transação, preservando um catálogo existente.
    /// É chamado em Program.cs depois da aplicação das migrations e antes de iniciar o servidor.
    /// </summary>
    public static async Task SeedAsync(CatalogDbContext db, string sourcePath, CancellationToken cancellationToken = default)
    {
        // O catálogo existente é preservado em cada reinício; o JSON só cria a carga inicial.
        if (await db.Courses.AnyAsync(cancellationToken)) return;
        // O arquivo distribuído com a API é a fonte inicial; os DTOs reaproveitam seu formato de dados.
        await using var stream = File.OpenRead(sourcePath);
        var courses = await JsonSerializer.DeserializeAsync<CourseDto[]>(stream, JsonOptions, cancellationToken)
            ?? throw new InvalidDataException("A carga inicial não contém cursos.");
        Validate(courses);

        // A transação evita deixar apenas parte da carga gravada se a importação falhar.
        await using var transaction = await db.Database.BeginTransactionAsync(cancellationToken);
        foreach (var course in courses)
        {
            var record = new CourseRecord
            {
                Id = course.Id,
                Slug = course.Slug,
                Name = course.Name,
                ShortName = course.ShortName,
                NormalizedName = CourseQuery.Normalize($"{course.Name} {course.ShortName}"),
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
                OnlineInformationJson = JsonSerializer.Serialize(course.OnlineInformation, JsonOptions),
                Faqs = course.Faqs.Select((faq, index) => new FaqRecord
                {
                    Position = index,
                    Question = faq.Question,
                    Answer = faq.Answer
                }).ToList()
            };
            // A posição vem da ordem do JSON. As navegações permitem ao EF ligar módulos e aulas.
            record.Modules = course.Modules.Select((module, index) => new CourseModuleRecord
            {
                Position = index,
                Title = module.Title,
                Description = module.Description,
                Topics = module.Topics.Select((topic, position) => new TopicRecord { Position = position, Text = topic }).ToList(),
                Lessons = module.Lessons.Select((lesson, position) => ToLesson(lesson, record, position)).ToList()
            }).ToList();
            // Introdução pertence ao curso diretamente e permanece com ModuleId nulo.
            record.Lessons.AddRange(course.Introduction.Select((lesson, index) => ToLesson(lesson, record, index)));
            db.Courses.Add(record);
        }
        // Add acompanha os objetos em memória; SaveChanges envia as inclusões ao banco.
        await db.SaveChangesAsync(cancellationToken);
        await transaction.CommitAsync(cancellationToken);
    }

    // Mantém a chave pública da aula e a referência do curso; o EF resolve as chaves relacionais.
    private static LessonRecord ToLesson(LessonDto lesson, CourseRecord course, int position) => new()
    {
        Course = course,
        Key = lesson.Id,
        Position = position,
        Title = lesson.Title,
        Description = lesson.Description,
        ContentJson = JsonSerializer.Serialize(lesson.Content, JsonOptions)
    };

    // Falhas da fonte inicial impedem a carga. Não há correção silenciosa de IDs ou textos.
    // Este método verifica a estrutura usada na importação; as FKs e índices completam a integridade.
    private static void Validate(CourseDto[] courses)
    {
        if (courses.Length < 6 || courses.Select(c => c.Id).Distinct().Count() != courses.Length ||
            courses.Select(c => c.Slug).Distinct().Count() != courses.Length)
            throw new InvalidDataException("A carga deve conter seis ou mais cursos com IDs e slugs únicos.");
        foreach (var course in courses)
        {
            // Reutiliza as opções aceitas pela API e verifica as chaves de todas as aulas do curso.
            var query = new CourseQuery(null, course.Category, course.Modality, course.Status, null);
            var lessons = course.Introduction.Concat(course.Modules.SelectMany(module => module.Lessons)).ToArray();
            if (query.Validate().Count > 0 || course.Id <= 0 || course.Hours <= 0 ||
                string.IsNullOrWhiteSpace(course.Name) || string.IsNullOrWhiteSpace(course.ShortName) ||
                string.IsNullOrWhiteSpace(course.Slug) || string.IsNullOrWhiteSpace(course.Description) ||
                string.IsNullOrWhiteSpace(course.Audience) || !course.Image.StartsWith("assets/", StringComparison.Ordinal) ||
                course.Image.Contains("..", StringComparison.Ordinal) || course.Modules.Length == 0 ||
                lessons.Select(lesson => lesson.Id).Distinct().Count() != lessons.Length ||
                lessons.Any(lesson => string.IsNullOrWhiteSpace(lesson.Id) || string.IsNullOrWhiteSpace(lesson.Title)))
                throw new InvalidDataException($"Dados inválidos na carga do curso {course.Id}.");
        }
    }
}

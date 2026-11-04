using SesiIndustria.Api.Contracts;

namespace SesiIndustria.Api.Data;

/// <summary>
/// Formato completo da fonte de carga inicial. O conteúdo é persistido, mas não acompanha
/// o índice público do curso; as requisições de aula o consultam separadamente.
/// </summary>
public sealed record CatalogSeedCourse : CourseSummaryDto
{
    public required string About { get; init; }
    public string Certification { get; init; } = "SESI";
    public CatalogSeedModule[] Modules { get; init; } = [];
    public LessonDto[] Introduction { get; init; } = [];
    public FaqDto[] Faqs { get; init; } = [];
    public ContentBlockDto[] OnlineInformation { get; init; } = [];
}

public sealed record CatalogSeedModule(string Title, string Description, string[] Topics, LessonDto[] Lessons);

namespace SesiIndustria.Api.Contracts;

// DTO (Data Transfer Object) define os dados trocados entre a API e o frontend.
// Estes records são serializados em JSON pelo ASP.NET Core; não representam tabelas.
// A separação evita expor chaves estrangeiras e propriedades de navegação do EF.

/// <summary>Resumo de um curso usado na listagem e nos destaques da página inicial.</summary>
public record CourseSummaryDto
{
    public int Id { get; init; }
    /// <summary>Identificador legível usado em /api/courses/{slug} e nas rotas Angular.</summary>
    public required string Slug { get; init; }
    public required string Name { get; init; }
    public required string ShortName { get; init; }
    public required string Description { get; init; }
    public required string Category { get; init; }
    public required string Modality { get; init; }
    public int Hours { get; init; }
    public required string Audience { get; init; }
    public required string Status { get; init; }
    public required string Image { get; init; }
    public required string ImageAlt { get; init; }
    // Data de calendário, sem horário/fuso; null indica que não há data informada.
    public DateOnly? AvailableAt { get; init; }
    public bool Featured { get; init; }
}

/// <summary>
/// Detalhes completos do curso, reutilizando o resumo e acrescentando conteúdo para detalhes e AVA.
/// Coleções vazias representam conteúdo não fornecido na referência.
/// </summary>
public sealed record CourseDto : CourseSummaryDto
{
    public required string About { get; init; }
    public string Certification { get; init; } = "SESI";
    public CourseModuleDto[] Modules { get; init; } = [];
    // Itens de introdução ficam fora dos módulos na navegação da AVA.
    public LessonDto[] Introduction { get; init; } = [];
    public FaqDto[] Faqs { get; init; } = [];
    public ContentBlockDto[] OnlineInformation { get; init; } = [];
}

/// <summary>Módulo com tópicos e aulas já ordenados pelo repositório.</summary>
public sealed record CourseModuleDto(string Title, string Description, string[] Topics, LessonDto[] Lessons);

/// <summary>Aula identificada por uma chave estável dentro do curso; inclui descrição e blocos de texto.</summary>
public sealed record LessonDto(string Id, string Title, string Description)
{
    public ContentBlockDto[] Content { get; init; } = [];
}
/// <summary>Pergunta e resposta fornecidas pelo conteúdo de referência.</summary>
public sealed record FaqDto(string Question, string Answer);

/// <summary>
/// Conteúdo estruturado: paragraph, heading, list ou note, conforme o contrato Angular.
/// Text e Items são textos; o contrato não envia HTML para inserção na página.
/// </summary>
public sealed record ContentBlockDto(string Kind, string Text)
{
    public string[] Items { get; init; } = [];
}
/// <summary>Resposta da listagem: resultados da consulta e sua contagem, sem paginação nesta versão.</summary>
public sealed record CoursePageDto(CourseSummaryDto[] Items, int Total);

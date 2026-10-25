namespace SesiIndustria.Api.Data;

// Entidades persistidas pelo Entity Framework Core. CatalogDbContext define tabelas e restrições;
// CourseRepository transforma estes registros nos DTOs enviados ao frontend.

/// <summary>Registro principal do catálogo, com relações para módulos, aulas e perguntas.</summary>
public sealed class CourseRecord
{
    public int Id { get; set; }
    public string Slug { get; set; } = "";
    public string Name { get; set; } = "";
    public string ShortName { get; set; } = "";
    // Combina nome completo e curto em forma normalizada; serve à busca, sem alterar a exibição.
    public string NormalizedName { get; set; } = "";
    public string Description { get; set; } = "";
    public string About { get; set; } = "";
    public string Category { get; set; } = "";
    public string Modality { get; set; } = "";
    public int Hours { get; set; }
    public string Audience { get; set; } = "";
    public string Status { get; set; } = "";
    public string Image { get; set; } = "";
    public string ImageAlt { get; set; } = "";
    public DateOnly? AvailableAt { get; set; }
    public bool Featured { get; set; }
    public string Certification { get; set; } = "SESI";
    // Blocos de apresentação são serializados em JSON; a relação curso/módulo/aula é relacional.
    public string OnlineInformationJson { get; set; } = "[]";
    // Propriedades de navegação permitem ao EF carregar os registros relacionados com Include.
    public List<CourseModuleRecord> Modules { get; set; } = [];
    public List<LessonRecord> Lessons { get; set; } = [];
    public List<FaqRecord> Faqs { get; set; } = [];
}

/// <summary>Módulo de um curso; Position define sua ordem, começando em zero na carga inicial.</summary>
public sealed class CourseModuleRecord
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    // CourseId é a chave estrangeira; Course é a referência ao objeto carregado pelo EF.
    public CourseRecord Course { get; set; } = null!;
    public int Position { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<TopicRecord> Topics { get; set; } = [];
    public List<LessonRecord> Lessons { get; set; } = [];
}

/// <summary>Aula vinculada a um curso e, opcionalmente, a um dos módulos desse mesmo curso.</summary>
public sealed class LessonRecord
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public CourseRecord Course { get; set; } = null!;
    // A introdução pertence diretamente ao curso; as demais aulas têm módulo.
    public int? ModuleId { get; set; }
    public CourseModuleRecord? Module { get; set; }
    // Key é enviada como LessonDto.Id. Mantê-la estável preserva o progresso local da AVA.
    public string Key { get; set; } = "";
    public int Position { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    // Texto estruturado conforme ContentBlockDto; não contém marcação HTML para execução.
    public string ContentJson { get; set; } = "[]";
}

/// <summary>Tópico apresentado nos detalhes de um módulo, ordenado por Position.</summary>
public sealed class TopicRecord
{
    public int Id { get; set; }
    public int ModuleId { get; set; }
    public CourseModuleRecord Module { get; set; } = null!;
    public int Position { get; set; }
    public string Text { get; set; } = "";
}

/// <summary>Pergunta frequente de um curso, ordenada por Position.</summary>
public sealed class FaqRecord
{
    public int Id { get; set; }
    public int CourseId { get; set; }
    public CourseRecord Course { get; set; } = null!;
    public int Position { get; set; }
    public string Question { get; set; } = "";
    public string Answer { get; set; } = "";
}

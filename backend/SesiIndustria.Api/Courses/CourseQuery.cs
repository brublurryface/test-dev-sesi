using System.Globalization;
using System.Text;
using SesiIndustria.Api.Contracts;

namespace SesiIndustria.Api.Courses;

/// <summary>
/// Reúne os filtros recebidos pelo endpoint de listagem e suas regras de validação.
/// Um valor nulo ou vazio deixa o filtro de texto correspondente sem seleção.
/// Featured é opcional: null traz todos; true traz destaques; false traz os demais.
/// </summary>
public sealed record CourseQuery(string? Search, string? Category, string? Modality, string? Status, bool? Featured,
    int Page = 1, int PageSize = CourseQuery.DefaultPageSize)
{
    // Regra do contrato HTTP, também usada pelo limite do campo no frontend.
    public const int MaxSearchLength = 120;
    public const int DefaultPageSize = 20;
    public const int MaxPageSize = 100;
    /// <summary>
    /// Retorna erros por nome de parâmetro, no formato usado pelo HTTP 400 de validação.
    /// A API valida novamente mesmo quando a interface já restringe os campos.
    /// </summary>
    public Dictionary<string, string[]> Validate(CourseFilterOptionsDto options)
    {
        var errors = new Dictionary<string, string[]>();
        if (Search?.Length > MaxSearchLength) errors["search"] = [$"A busca deve ter no máximo {MaxSearchLength} caracteres."];
        if (Page < 1) errors["page"] = ["A página deve ser maior ou igual a 1."];
        if (PageSize < 1 || PageSize > MaxPageSize)
            errors["pageSize"] = [$"O tamanho da página deve estar entre 1 e {MaxPageSize}."];
        // Skip recebe int: verifica o produto com long antes de calcular o deslocamento.
        if (Page > 0 && PageSize > 0 && ((long)Page - 1) * PageSize > int.MaxValue)
            errors["page"] = ["A página solicitada excede o limite de navegação."];
        Check("category", Category, options.Categories);
        Check("modality", Modality, options.Modalities);
        Check("status", Status, options.Statuses);
        return errors;

        // Filtro preenchido deve coincidir com uma opção; busca livre tem apenas limite de tamanho.
        void Check(string key, string? value, string[] allowed)
        {
            if (!string.IsNullOrWhiteSpace(value) && !allowed.Contains(value))
                errors[key] = ["Escolha uma das opções válidas do catálogo."];
        }
    }

    /// <summary>
    /// Produz a representação de busca sem acentos, caixa ou espaços externos.
    /// O seeder usa a mesma regra ao gravar NormalizedName, mantendo as duas pontas compatíveis.
    /// </summary>
    public static string Normalize(string value)
    {
        var text = new StringBuilder();
        // FormD separa letra e acento; os sinais de acentuação são removidos da comparação.
        foreach (var character in value.Trim().Normalize(NormalizationForm.FormD))
            if (CharUnicodeInfo.GetUnicodeCategory(character) != UnicodeCategory.NonSpacingMark)
                text.Append(char.ToLowerInvariant(character));
        // O nome original continua preservado para exibição; somente a chave de busca é normalizada.
        return text.ToString().Normalize(NormalizationForm.FormC);
    }
}

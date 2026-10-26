using System.Text.Json;

namespace SesiIndustria.Api.Moodle;

/// <summary>Configuração privada do servidor. Token nunca integra respostas da API.</summary>
public sealed class MoodleOptions
{
    public bool Enabled { get; set; }
    public string BaseUrl { get; set; } = "";
    public string Token { get; set; } = "";
    public int CourseId { get; set; }
    public string CourseSlug { get; set; } = "matematica-para-a-precisao";

    public bool IsValid() => CourseId > 0 && !string.IsNullOrWhiteSpace(Token) &&
        !string.IsNullOrWhiteSpace(CourseSlug) && Uri.TryCreate(BaseUrl, UriKind.Absolute, out var url) &&
        url.Scheme == Uri.UriSchemeHttps && string.IsNullOrEmpty(url.UserInfo) &&
        string.IsNullOrEmpty(url.Query) && string.IsNullOrEmpty(url.Fragment) && url.AbsolutePath == "/";
}

/// <summary>
/// Usa somente as três funções autorizadas no Moodle. O token segue no corpo POST,
/// nunca na URL; redirecionamentos são desabilitados no registro do HttpClient.
/// </summary>
public sealed class MoodleClient(HttpClient http)
{
    public async Task<JsonElement> CallAsync(MoodleOptions options, string function,
        Dictionary<string, string> parameters, CancellationToken cancellationToken)
    {
        var fields = new Dictionary<string, string>(parameters)
        {
            ["wstoken"] = options.Token,
            ["wsfunction"] = function,
            ["moodlewsrestformat"] = "json"
        };
        using var body = new FormUrlEncodedContent(fields);
        using var request = new HttpRequestMessage(HttpMethod.Post, new Uri(new Uri(options.BaseUrl),
            "webservice/rest/server.php")) { Content = body };
        using var response = await http.SendAsync(request, HttpCompletionOption.ResponseHeadersRead, cancellationToken);
        response.EnsureSuccessStatusCode();
        // Impede que um retorno inesperadamente grande seja processado como catálogo.
        await response.Content.LoadIntoBufferAsync(2 * 1024 * 1024, cancellationToken);
        using var document = JsonDocument.Parse(await response.Content.ReadAsStringAsync(cancellationToken));
        var root = document.RootElement;
        // Moodle pode responder HTTP 200 com erro. Arrays de módulos não são objetos de erro.
        if (root.ValueKind == JsonValueKind.Object &&
            (root.TryGetProperty("exception", out _) || root.TryGetProperty("errorcode", out _)))
            throw new InvalidDataException("O Moodle recusou a consulta.");
        return root.Clone();
    }
}

using System.Net;
using System.Text.RegularExpressions;
using SesiIndustria.Api.Contracts;

namespace SesiIndustria.Api.Moodle;

/// <summary>
/// Reduz o HTML do editor Moodle a blocos de texto. Não transfere HTML, links,
/// imagens, arquivos ou scripts ao Angular. O frontend mantém a interpolação escapada.
/// </summary>
public static class MoodleText
{
    private static string Replace(string text, string pattern, string replacement) =>
        Regex.Replace(text, pattern, replacement, RegexOptions.IgnoreCase | RegexOptions.Singleline,
            TimeSpan.FromSeconds(1));

    public static ContentBlockDto[] Blocks(string html)
    {
        if (html.Length > 200_000) throw new InvalidDataException("Conteúdo excede o limite de importação.");
        var text = Replace(html, @"<(script|style)\b[^>]*>.*?</\1\s*>", "");
        text = Replace(text, @"</(?:p|div|h[1-6]|li)\s*>|<br\s*/?>", "\n");
        text = WebUtility.HtmlDecode(Replace(text, "<[^>]*>", ""));
        return text.Split('\n', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries)
            .Select(line => new ContentBlockDto("paragraph", line)).ToArray();
    }

    public static string Plain(string html) => string.Join(" ", Blocks(html).Select(block => block.Text));
}

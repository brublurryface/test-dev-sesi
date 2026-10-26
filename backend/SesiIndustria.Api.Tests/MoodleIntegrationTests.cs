using System.Net;
using System.Text.Json;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using Microsoft.Extensions.Options;
using SesiIndustria.Api.Contracts;
using SesiIndustria.Api.Data;
using SesiIndustria.Api.Moodle;

namespace SesiIndustria.Api.Tests;

/// <summary>Integração com SQLite real e Moodle simulado; não usa credenciais externas.</summary>
public sealed class MoodleIntegrationTests
{
    [Fact]
    public async Task SuccessfulImportPreservesLocalKeysAndOtherCoursesAndReadsAllThreeFunctions()
    {
        await using var fixture = await ImportFixture.CreateAsync();
        var keys = await fixture.Db.Lessons.OrderBy(l => l.Id).Select(l => l.Key).ToArrayAsync();
        await fixture.RunAsync();
        Assert.Equal("synchronized", fixture.Status.State);
        Assert.NotNull(fixture.Status.LastSuccessAt);
        Assert.Equal(keys, await fixture.Db.Lessons.OrderBy(l => l.Id).Select(l => l.Key).ToArrayAsync());
        Assert.Equal(7, await fixture.Db.Courses.CountAsync());
        Assert.Equal("Conteúdo vindo do Moodle.", (await fixture.Db.Lessons.SingleAsync(l => l.CourseId == 1 && l.Key == "m1-a1")).Description);
        Assert.StartsWith("Exemplo fictício:", (await fixture.Db.Lessons.SingleAsync(l => l.CourseId == 2 && l.Key == "m1-a1")).Description);
        var guide = await fixture.Db.Lessons.SingleAsync(l => l.CourseId == 1 && l.Key == "boas-praticas");
        var blocks = JsonSerializer.Deserialize<ContentBlockDto[]>(guide.ContentJson, new JsonSerializerOptions(JsonSerializerDefaults.Web))!;
        Assert.Equal(8, blocks.Length);
        Assert.Contains(blocks, block => block.Kind == "heading" && block.Text == "Recomendações");
        Assert.Equal(3, fixture.Handler.Functions.Count);
        Assert.All(fixture.Handler.Urls, url => Assert.DoesNotContain("test-token", url));
        Assert.All(fixture.Handler.Bodies, body => Assert.Contains("wstoken=test-token", body));
    }

    [Theory]
    [InlineData("missing-page")]
    [InlineData("wrong-course")]
    [InlineData("moodle-error")]
    [InlineData("http-error")]
    [InlineData("timeout")]
    public async Task FailedImportPreservesDatabaseAndReportsUnavailable(string failure)
    {
        await using var fixture = await ImportFixture.CreateAsync(failure);
        var before = await fixture.Db.Lessons.OrderBy(l => l.Id).Select(l => l.ContentJson + l.Description).ToArrayAsync();
        await fixture.RunAsync();
        Assert.Equal("unavailable", fixture.Status.State);
        Assert.Null(fixture.Status.LastSuccessAt);
        Assert.Equal(before, await fixture.Db.Lessons.OrderBy(l => l.Id).Select(l => l.ContentJson + l.Description).ToArrayAsync());
    }

    [Fact]
    public async Task DisabledOrInvalidConfigurationMakesNoExternalRequests()
    {
        await using var fixture = await ImportFixture.CreateAsync();
        fixture.Options.Enabled = false;
        await fixture.RunAsync();
        Assert.Equal("disabled", fixture.Status.State);
        fixture.Options.Enabled = true;
        fixture.Options.BaseUrl = "http://moodle.example/";
        await fixture.RunAsync();
        Assert.Equal("invalid_configuration", fixture.Status.State);
        Assert.Empty(fixture.Handler.Functions);
    }

    [Fact]
    public void HtmlIsReducedToPlainBlocksWithoutScriptsOrExecutableMarkup()
    {
        var blocks = MoodleText.Blocks("<p>Texto &amp; exemplo</p><script>alert('x')</script><p><img src=x onerror=alert(1)>Final</p>");
        Assert.Equal(["Texto & exemplo", "Final"], blocks.Select(b => b.Text).ToArray());
    }

    private sealed class ImportFixture : IAsyncDisposable
    {
        private readonly string directory = Path.Combine(Path.GetTempPath(), $"sesi-moodle-{Guid.NewGuid():N}");
        public CatalogDbContext Db { get; private set; } = null!;
        public MoodleHandler Handler { get; private set; } = null!;
        private HttpClient http = null!;
        public MoodleOptions Options { get; } = new()
        {
            Enabled = true, BaseUrl = "https://moodle.example/", CourseId = 3, Token = "test-token"
        };
        public MoodleImportStatus Status { get; } = new();

        public static async Task<ImportFixture> CreateAsync(string? failure = null)
        {
            var fixture = new ImportFixture();
            Directory.CreateDirectory(fixture.directory);
            fixture.Db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
                .UseSqlite($"Data Source={Path.Combine(fixture.directory, "catalog.db")};Foreign Keys=True").Options);
            await fixture.Db.Database.MigrateAsync();
            var seed = Path.GetFullPath("../../../../SesiIndustria.Api/Data/Seed/courses.json", AppContext.BaseDirectory);
            await CatalogSeeder.SeedAsync(fixture.Db, seed);
            var course = JsonSerializer.Deserialize<CourseDto[]>(await File.ReadAllTextAsync(seed),
                new JsonSerializerOptions(JsonSerializerDefaults.Web))!.Single(c => c.Id == 1);
            fixture.Handler = new MoodleHandler(course, failure);
            fixture.http = new HttpClient(fixture.Handler);
            return fixture;
        }

        public Task RunAsync() => new MoodleImporter(new MoodleClient(http), Db,
            Microsoft.Extensions.Options.Options.Create(Options), Status, NullLogger<MoodleImporter>.Instance).ImportAsync();

        public async ValueTask DisposeAsync()
        {
            http.Dispose();
            await Db.DisposeAsync();
            SqliteConnection.ClearAllPools();
            Directory.Delete(directory, true);
        }
    }

    private sealed class MoodleHandler(CourseDto course, string? failure) : HttpMessageHandler
    {
        public List<string> Functions { get; } = [];
        public List<string> Urls { get; } = [];
        public List<string> Bodies { get; } = [];

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var body = await request.Content!.ReadAsStringAsync(cancellationToken);
            var fields = body.Split('&').Select(pair => pair.Split('=', 2))
                .ToDictionary(pair => WebUtility.UrlDecode(pair[0]), pair => WebUtility.UrlDecode(pair[1]));
            var function = fields["wsfunction"];
            Functions.Add(function);
            Urls.Add(request.RequestUri!.ToString());
            Bodies.Add(body);
            if (failure == "timeout") throw new TaskCanceledException();
            if (failure == "http-error") return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
            object response = function switch
            {
                "core_course_get_courses_by_field" => new { courses = new[] { new { id = 3, idnumber = failure == "wrong-course" ? "outro" : course.Slug, fullname = course.Name } } },
                "core_course_get_contents" when failure == "moodle-error" => new { exception = "moodle_exception", errorcode = "access_exception" },
                "core_course_get_contents" => Sections(),
                "mod_page_get_pages_by_courses" => new { pages = Pages() },
                _ => throw new InvalidOperationException("Função inesperada.")
            };
            return new HttpResponseMessage(HttpStatusCode.OK) { Content = new StringContent(JsonSerializer.Serialize(response)) };
        }

        private object[] Sections() => Enumerable.Range(0, 11).Select(index => (object)new
        {
            section = index,
            name = index == 0 ? "General" : course.Modules[index - 1].Title,
            // Introdução deliberadamente fora de ordem: a importação deve usar os nomes e manter IDs locais.
            modules = index == 0 ? new[] { 102, 101, 103 }.Select(id => new { id, modname = "page" }).ToArray()
                : new[] { new { id = index, modname = "page" } }
        }).ToArray();

        private object[] Pages()
        {
            var pages = new List<object>();
            for (var i = 0; i < 10; i++)
            {
                if (failure == "missing-page" && i == 9) continue;
                pages.Add(new { course = 3, coursemodule = i + 1, name = "Aula 1", intro = "<p>Conteúdo vindo do Moodle.</p>", content = "<p>Conteúdo vindo do Moodle.</p>" });
            }
            for (var i = 0; i < 3; i++)
            {
                var lesson = course.Introduction[i];
                var text = lesson.Content.Length > 0 ? string.Join("", lesson.Content.Select(b => $"<p>{WebUtility.HtmlEncode(b.Text)}</p>"))
                    : $"<p>{lesson.Description}</p>";
                pages.Add(new { course = 3, coursemodule = 101 + i, name = lesson.Title, intro = lesson.Description, content = text });
            }
            return pages.ToArray();
        }
    }
}

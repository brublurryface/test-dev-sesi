using System.Collections.Concurrent;
using System.Data.Common;
using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using SesiIndustria.Api.Contracts;
using SesiIndustria.Api.Data;

namespace SesiIndustria.Api.Tests;

public sealed class LessonContentTests
{
    [Fact]
    public async Task IndexDoesNotReadLessonContentAndDoesNotIssueOneQueryPerLesson()
    {
        var recorder = new QueryRecorder();
        using var app = new CatalogApplication();
        using var configured = app.WithWebHostBuilder(builder => builder.ConfigureServices(services =>
            services.AddDbContext<CatalogDbContext>(options => options.AddInterceptors(recorder))));
        using var client = configured.CreateClient();
        await client.GetAsync("/api/health");
        recorder.Queries.Clear();
        var baseline = await client.GetFromJsonAsync<CourseDto>("/api/courses/matematica-para-a-precisao");
        var baselineQueries = recorder.Queries.Count;
        Assert.True(baselineQueries > 0);
        Assert.DoesNotContain(recorder.Queries, sql => sql.Contains("ContentJson", StringComparison.Ordinal));

        using (var scope = configured.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var module = await db.Modules.SingleAsync(m => m.CourseId == 1 && m.Position == 0);
            db.Lessons.AddRange(Enumerable.Range(1, 500).Select(position => new LessonRecord
            {
                CourseId = 1, ModuleId = module.Id, Key = $"indice-teste-{position}",
                Position = position, Title = $"Aula de teste {position}", Description = "Resumo para o índice",
                ContentJson = """[{"kind":"paragraph","text":"CONTEUDO_FORA_DO_INDICE","items":[]}]"""
            }));
            await db.SaveChangesAsync();
        }
        recorder.Queries.Clear();
        var response = await client.GetStringAsync("/api/courses/matematica-para-a-precisao");
        Assert.Equal(baselineQueries, recorder.Queries.Count);
        Assert.DoesNotContain(recorder.Queries, sql => sql.Contains("ContentJson", StringComparison.Ordinal));
        Assert.DoesNotContain("CONTEUDO_FORA_DO_INDICE", response);
        Assert.DoesNotContain("\"content\"", response);
        var index = await client.GetFromJsonAsync<CourseDto>("/api/courses/matematica-para-a-precisao");
        Assert.Equal(501, index!.Modules[0].Lessons.Length);
        Assert.Equal("m1-a1", index.Modules[0].Lessons[0].Id);
        Assert.Equal(baseline!.Introduction.Select(l => l.Id), index.Introduction.Select(l => l.Id));

        recorder.Queries.Clear();
        var selected = await client.GetFromJsonAsync<LessonDto>("/api/courses/matematica-para-a-precisao/lessons/indice-teste-500");
        Assert.Equal("CONTEUDO_FORA_DO_INDICE", Assert.Single(selected!.Content).Text);
        Assert.Single(recorder.Queries);
        Assert.Contains("ContentJson", recorder.Queries.Single());
    }

    [Fact]
    public async Task LessonKeysAreScopedByCourseAndReferenceContentIsPreserved()
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        var math = await client.GetFromJsonAsync<LessonDto>("/api/courses/matematica-para-a-precisao/lessons/boas-praticas");
        var geometry = await client.GetFromJsonAsync<LessonDto>("/api/courses/geometria-e-modelagem/lessons/boas-praticas");
        Assert.Equal("boas-praticas", math!.Id);
        Assert.Equal(math.Id, geometry!.Id);
        Assert.Equal(8, math.Content.Length);
        Assert.Contains(math.Content, block => block.Kind == "heading" && block.Text == "Recomendações");
        Assert.Empty(geometry.Content);
        Assert.StartsWith("Exemplo fictício:", geometry.Description);
    }

    [Fact]
    public async Task ChangingContentDoesNotChangeTheIndexOrAnotherLesson()
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        var before = await client.GetStringAsync("/api/courses/matematica-para-a-precisao");
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var lesson = await db.Lessons.SingleAsync(l => l.CourseId == 1 && l.Key == "m1-a1");
            lesson.ContentJson = """[{"kind":"paragraph","text":"Texto alterado somente nesta aula.","items":[]}]""";
            await db.SaveChangesAsync();
        }
        Assert.Equal(before, await client.GetStringAsync("/api/courses/matematica-para-a-precisao"));
        var updated = await client.GetFromJsonAsync<LessonDto>("/api/courses/matematica-para-a-precisao/lessons/m1-a1");
        Assert.Equal("Texto alterado somente nesta aula.", Assert.Single(updated!.Content).Text);
        Assert.Equal(8, (await client.GetFromJsonAsync<LessonDto>("/api/courses/matematica-para-a-precisao/lessons/boas-praticas"))!.Content.Length);
    }

    [Theory]
    [InlineData("m1-a1")]
    [InlineData("introducao")]
    public async Task LongDescriptionsArePreviewedInTheIndexAndCompleteInTheSelectedLesson(string lessonId)
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        await client.GetAsync("/api/health");
        using (var scope = app.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
            var lesson = await db.Lessons.SingleAsync(l => l.CourseId == 1 && l.Key == lessonId);
            lesson.Description = new string('x', 50000);
            await db.SaveChangesAsync();
        }
        var index = (await client.GetFromJsonAsync<CourseDto>("/api/courses/matematica-para-a-precisao"))!;
        var summary = index.Introduction.Concat(index.Modules.SelectMany(m => m.Lessons)).Single(l => l.Id == lessonId);
        Assert.Equal(new string('x', 240) + "…", summary.Description);
        var selected = await client.GetFromJsonAsync<LessonDto>($"/api/courses/matematica-para-a-precisao/lessons/{lessonId}");
        Assert.Equal(new string('x', 50000), selected!.Description);
    }

    [Theory]
    [InlineData("matematica-para-a-precisao", "nao-existe")]
    [InlineData("nao-existe", "boas-praticas")]
    [InlineData("geometria-e-modelagem", "indice-teste-500")]
    public async Task MissingCourseOrLessonReturns404(string slug, string lessonId)
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        using var response = await client.GetAsync($"/api/courses/{slug}/lessons/{lessonId}");
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private sealed class QueryRecorder : DbCommandInterceptor
    {
        public ConcurrentQueue<string> Queries { get; } = new();
        public override ValueTask<InterceptionResult<DbDataReader>> ReaderExecutingAsync(DbCommand command,
            CommandEventData eventData, InterceptionResult<DbDataReader> result,
            CancellationToken cancellationToken = default)
        {
            Queries.Enqueue(command.CommandText);
            return ValueTask.FromResult(result);
        }
    }
}

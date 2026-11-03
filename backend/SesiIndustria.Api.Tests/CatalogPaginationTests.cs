using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using SesiIndustria.Api.Contracts;
using SesiIndustria.Api.Data;

namespace SesiIndustria.Api.Tests;

public sealed class CatalogPaginationTests
{
    [Fact]
    public async Task DefaultPagesHaveTwentyItemsAndIndependentTotalsWithoutRepeatedOrMissingCourses()
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        await AddTestCoursesAsync(app, client);
        var first = (await client.GetFromJsonAsync<CoursePageDto>("/api/courses"))!;
        Assert.Equal(47, first.Total);
        Assert.Equal(20, first.PageSize);
        Assert.Equal(1, first.Page);
        Assert.Equal(3, first.TotalPages);
        Assert.Equal(Enumerable.Range(1, 20), first.Items.Select(course => course.Id));

        var ids = new List<int>(first.Items.Select(course => course.Id));
        for (var number = 2; number <= first.TotalPages; number++)
        {
            var page = (await client.GetFromJsonAsync<CoursePageDto>($"/api/courses?page={number}"))!;
            Assert.Equal(47, page.Total);
            Assert.Equal(number == 3 ? 7 : 20, page.Items.Length);
            Assert.Equal(first.Filters.Categories, page.Filters.Categories);
            ids.AddRange(page.Items.Select(course => course.Id));
        }
        Assert.Equal(Enumerable.Range(1, 47), ids);
    }

    [Fact]
    public async Task FiltersAreCountedBeforePaginationAndOptionsIncludeCoursesOnOtherPages()
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        await AddTestCoursesAsync(app, client);
        var category = Uri.EscapeDataString("Teste de paginação");
        var page = (await client.GetFromJsonAsync<CoursePageDto>($"/api/courses?search=CURSO%20DE%20TESTE&category={category}&modality=Online&status=Dispon%C3%ADvel&page=3&pageSize=15"))!;
        Assert.Equal(40, page.Total);
        Assert.Equal(3, page.TotalPages);
        Assert.Equal(15, page.PageSize);
        Assert.Equal(Enumerable.Range(38, 10), page.Items.Select(course => course.Id));
        Assert.Contains("Matemática", page.Filters.Categories);
        Assert.Contains("Presencial", page.Filters.Modalities);
        Assert.Contains("Encerrado", page.Filters.Statuses);
    }

    [Fact]
    public async Task PageBeyondTheEndIsEmptyButKeepsTotalAndAllowsReturningToTheFirstPage()
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        var page = (await client.GetFromJsonAsync<CoursePageDto>("/api/courses?page=99"))!;
        Assert.Empty(page.Items);
        Assert.Equal(7, page.Total);
        Assert.Equal(99, page.Page);
        Assert.Equal(1, page.TotalPages);
        Assert.Equal(7, (await client.GetFromJsonAsync<CoursePageDto>("/api/courses?page=1"))!.Items.Length);

        var empty = (await client.GetFromJsonAsync<CoursePageDto>("/api/courses?search=sem-correspondencia&page=2"))!;
        Assert.Empty(empty.Items);
        Assert.Equal(0, empty.Total);
        Assert.Equal(0, empty.TotalPages);
        Assert.Equal(page.Filters.Categories, empty.Filters.Categories);
    }

    [Theory]
    [InlineData("page=0")]
    [InlineData("page=-1")]
    [InlineData("pageSize=0")]
    [InlineData("pageSize=-1")]
    [InlineData("pageSize=101")]
    [InlineData("pageSize=2147483647")]
    [InlineData("page=2147483647&pageSize=100")]
    [InlineData("page=abc")]
    [InlineData("pageSize=abc")]
    [InlineData("featured=abc")]
    public async Task InvalidPaginationReturns400InsteadOfUnlimitedResultsOrArithmeticFailure(string query)
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        using var response = await client.GetAsync($"/api/courses?{query}");
        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        Assert.Equal("application/problem+json", response.Content.Headers.ContentType?.MediaType);
    }

    private static async Task AddTestCoursesAsync(CatalogApplication app, HttpClient client)
    {
        await client.GetAsync("/api/health");
        using var scope = app.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
        // Os dados adicionais existem somente no SQLite temporário desta instância de teste.
        db.Courses.AddRange(Enumerable.Range(8, 40).Select(id => new CourseRecord
        {
            Id = id,
            Slug = $"curso-de-teste-{id}",
            Name = $"Curso de teste {id}",
            ShortName = $"Curso de teste {id}",
            NormalizedName = $"curso de teste {id}",
            Category = "Teste de paginação",
            Modality = "Online",
            Status = "Disponível",
            Hours = 1
        }));
        await db.SaveChangesAsync();
    }
}

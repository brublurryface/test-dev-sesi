using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using SesiIndustria.Api.Contracts;
using SesiIndustria.Api.Data;

namespace SesiIndustria.Api.Tests;

/// <summary>Verifica que as opções acompanham os dados reais, sem listas fixas ou cache de valores antigos.</summary>
public sealed class CatalogFilterTests
{
    [Fact]
    public async Task OptionsMatchWholeCatalogEvenWhenTheCurrentCombinationHasNoResults()
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        var all = (await client.GetFromJsonAsync<CoursePageDto>("/api/courses"))!;
        Assert.Equal(all.Items.Select(c => c.Category).Distinct().Order(), all.Filters.Categories.Order());
        Assert.Equal(all.Items.Select(c => c.Modality).Distinct().Order(), all.Filters.Modalities.Order());
        Assert.Equal(all.Items.Select(c => c.Status).Distinct().Order(), all.Filters.Statuses.Order());

        var empty = (await client.GetFromJsonAsync<CoursePageDto>("/api/courses?search=termo-sem-resultado"))!;
        Assert.Equal(0, empty.Total);
        Assert.Empty(empty.Items);
        Assert.Equal(all.Filters.Categories, empty.Filters.Categories);
        Assert.Equal(all.Filters.Modalities, empty.Filters.Modalities);
        Assert.Equal(all.Filters.Statuses, empty.Filters.Statuses);
    }

    [Fact]
    public async Task NewDatabaseValuesAppearAndCanBeCombinedWithoutChangingApplicationCode()
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        // Inicializa a API; os próximos pedidos precisam refletir mudanças no mesmo banco.
        await client.GetAsync("/api/courses");
        await using var connection = new SqliteConnection($"Data Source={app.DatabasePath};Foreign Keys=True");
        await connection.OpenAsync();
        await using (var command = connection.CreateCommand())
        {
            command.CommandText = "UPDATE Courses SET Category = $category, Modality = $modality, Status = $status WHERE Id = 1";
            command.Parameters.AddWithValue("$category", "Tecnologia & Inovação");
            command.Parameters.AddWithValue("$modality", "Híbrido");
            command.Parameters.AddWithValue("$status", "Lista de espera");
            Assert.Equal(1, await command.ExecuteNonQueryAsync());
        }
        var query = "search=MATEMATICA&category=Tecnologia%20%26%20Inova%C3%A7%C3%A3o&modality=H%C3%ADbrido&status=Lista%20de%20espera";
        var selected = (await client.GetFromJsonAsync<CoursePageDto>($"/api/courses?{query}"))!;
        Assert.Equal("matematica-para-a-precisao", Assert.Single(selected.Items).Slug);
        Assert.Contains("Tecnologia & Inovação", selected.Filters.Categories);
        Assert.Contains("Híbrido", selected.Filters.Modalities);
        Assert.Contains("Lista de espera", selected.Filters.Statuses);

        var empty = (await client.GetFromJsonAsync<CoursePageDto>($"/api/courses?{query.Replace("modality=H%C3%ADbrido", "modality=Presencial")}"))!;
        Assert.Empty(empty.Items);
        Assert.Contains("Híbrido", empty.Filters.Modalities);
        Assert.Equal(7, (await client.GetFromJsonAsync<CoursePageDto>("/api/courses"))!.Total);

        await using var remove = connection.CreateCommand();
        remove.CommandText = "UPDATE Courses SET Category = 'Matemática' WHERE Id = 1";
        await remove.ExecuteNonQueryAsync();
        var updated = (await client.GetFromJsonAsync<CoursePageDto>("/api/courses"))!;
        Assert.DoesNotContain("Tecnologia & Inovação", updated.Filters.Categories);
        Assert.Equal(HttpStatusCode.BadRequest, (await client.GetAsync($"/api/courses?{query}")).StatusCode);
    }

    [Fact]
    public async Task EmptyDatabaseReturnsEmptyOptionsRatherThanTheOriginalFixedLists()
    {
        using var app = new CatalogApplication();
        using var client = app.CreateClient();
        await client.GetAsync("/api/courses");
        await using var connection = new SqliteConnection($"Data Source={app.DatabasePath};Foreign Keys=True");
        await connection.OpenAsync();
        await using var command = connection.CreateCommand();
        command.CommandText = "DELETE FROM Courses";
        await command.ExecuteNonQueryAsync();
        var result = (await client.GetFromJsonAsync<CoursePageDto>("/api/courses"))!;
        Assert.Equal(0, result.Total);
        Assert.Empty(result.Items);
        Assert.Empty(result.Filters.Categories);
        Assert.Empty(result.Filters.Modalities);
        Assert.Empty(result.Filters.Statuses);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("category")]
    [InlineData("modality")]
    [InlineData("status")]
    public async Task SeedAcceptsNewLabelsButStillRejectsBlankRequiredFields(string? blankField)
    {
        var directory = Path.Combine(Path.GetTempPath(), $"sesi-filter-seed-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        try
        {
            var source = Path.GetFullPath("../../../../SesiIndustria.Api/Data/Seed/courses.json", AppContext.BaseDirectory);
            var data = JsonNode.Parse(await File.ReadAllTextAsync(source))!;
            data[0]!["category"] = "Tecnologia";
            data[0]!["modality"] = "Híbrido";
            data[0]!["status"] = "Lista de espera";
            if (blankField is not null) data[0]![blankField] = " ";
            var seed = Path.Combine(directory, "courses.json");
            await File.WriteAllTextAsync(seed, data.ToJsonString());
            await using var db = new CatalogDbContext(new DbContextOptionsBuilder<CatalogDbContext>()
                .UseSqlite($"Data Source={Path.Combine(directory, "catalog.db")};Foreign Keys=True").Options);
            await db.Database.MigrateAsync();
            if (blankField is not null)
            {
                await Assert.ThrowsAsync<InvalidDataException>(() => CatalogSeeder.SeedAsync(db, seed));
                Assert.Empty(await db.Courses.ToArrayAsync());
            }
            else
            {
                await CatalogSeeder.SeedAsync(db, seed);
                var course = await db.Courses.SingleAsync(c => c.Id == 1);
                Assert.Equal("Tecnologia", course.Category);
                Assert.Equal("Híbrido", course.Modality);
                Assert.Equal("Lista de espera", course.Status);
            }
        }
        finally
        {
            SqliteConnection.ClearAllPools();
            Directory.Delete(directory, true);
        }
    }
}

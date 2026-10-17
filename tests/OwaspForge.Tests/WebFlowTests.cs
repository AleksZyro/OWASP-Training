using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using OwaspForge.Core;
using OwaspForge.Web.Data;

namespace OwaspForge.Tests;

public sealed class WebFlowTests : IClassFixture<ForgeWebApplicationFactory>
{
    private readonly HttpClient client;

    public WebFlowTests(ForgeWebApplicationFactory factory)
    {
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Theory]
    [InlineData("/", "Sicherheitswissen")]
    [InlineData("/en", "Security knowledge")]
    public async Task Start_pages_expose_the_local_security_boundary(string path, string expectedContent)
    {
        var response = await client.GetAsync(path);
        var page = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expectedContent, page, StringComparison.Ordinal);
        Assert.Contains("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single(), StringComparison.Ordinal);
        Assert.Contains("object-src 'none'", response.Headers.GetValues("Content-Security-Policy").Single(), StringComparison.Ordinal);
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
        Assert.Equal("same-origin", response.Headers.GetValues("Cross-Origin-Opener-Policy").Single());
        Assert.Contains("frame-ancestors 'none'", response.Headers.GetValues("Content-Security-Policy").Single(), StringComparison.Ordinal);
    }

    [Fact]
    public async Task Challenge_completion_requires_an_antiforgery_token_and_marks_progress_server_side()
    {
        const string challengePath = "/challenge/sql-injection";
        var directPost = await client.PostAsync($"{challengePath}?handler=Check", new FormUrlEncodedContent([new KeyValuePair<string, string>("answer", "parameter")]));
        Assert.Equal(HttpStatusCode.BadRequest, directPost.StatusCode);

        var getResponse = await client.GetAsync($"{challengePath}?step=quiz");
        var page = await getResponse.Content.ReadAsStringAsync();
        var token = Regex.Match(page, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        Assert.False(string.IsNullOrWhiteSpace(token));
        var cookie = Assert.Single(getResponse.Headers.GetValues("Set-Cookie"), value => value.StartsWith("OwaspForge.Antiforgery=", StringComparison.Ordinal));
        Assert.Contains("httponly", cookie, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("samesite=strict", cookie, StringComparison.OrdinalIgnoreCase);

        var formValues = new List<KeyValuePair<string, string>> { new("__RequestVerificationToken", token) };
        formValues.AddRange(new ChallengeRegistry().Find("sql-injection")!.Questions.Select((question, index) => new KeyValuePair<string, string>($"answers[{index}]", question.CorrectOption)));
        var completeResponse = await client.PostAsync($"{challengePath}?handler=Check", new FormUrlEncodedContent(formValues));

        Assert.Equal(HttpStatusCode.Redirect, completeResponse.StatusCode);
        Assert.Equal("/Challenge/sql-injection?solved=true&step=solution", completeResponse.Headers.Location?.OriginalString);

        var home = await client.GetStringAsync("/");
        Assert.Contains("Abgeschlossen", home, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Opening_a_station_is_side_effect_free_and_a_protected_action_starts_it()
    {
        const string challengePath = "/Challenge/xss";
        var openResponse = await client.GetAsync(challengePath);
        Assert.Equal(HttpStatusCode.OK, openResponse.StatusCode);

        var untouchedHome = await client.GetStringAsync("/");
        Assert.Contains("Nicht begonnen", untouchedHome, StringComparison.Ordinal);

        var quizResponse = await client.GetAsync($"{challengePath}?step=quiz");
        var page = await quizResponse.Content.ReadAsStringAsync();
        var token = Regex.Match(page, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        var hintResponse = await client.PostAsync(
            $"{challengePath}?handler=Hint",
            new FormUrlEncodedContent([
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("level", "1"),
            ]));
        Assert.Equal(HttpStatusCode.Redirect, hintResponse.StatusCode);

        var inProgressHome = await client.GetStringAsync("/");
        Assert.Contains("In Bearbeitung", inProgressHome, StringComparison.Ordinal);

        var hintedPage = await client.GetStringAsync(hintResponse.Headers.Location!.OriginalString);
        token = Regex.Match(hintedPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        var resetResponse = await client.PostAsync(
            $"{challengePath}?handler=Reset",
            new FormUrlEncodedContent([new KeyValuePair<string, string>("__RequestVerificationToken", token)]));

        Assert.Equal(HttpStatusCode.Redirect, resetResponse.StatusCode);
        Assert.Equal("/Challenge/xss?reset=true", resetResponse.Headers.Location?.OriginalString);

        var resetPage = await client.GetStringAsync("/Challenge/xss?reset=true");
        Assert.Contains("Station zurückgesetzt", resetPage, StringComparison.Ordinal);

        var resetHome = await client.GetStringAsync("/");
        Assert.Contains("Nicht begonnen", resetHome, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/Challenge/idor")]
    [InlineData("/en/challenge/idor")]
    public async Task Viewing_any_localized_challenge_does_not_create_progress(string path)
    {
        using var isolatedFactory = new ForgeWebApplicationFactory();
        using var isolatedClient = isolatedFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await isolatedClient.GetAsync(path);
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        await using var scope = isolatedFactory.Services.CreateAsyncScope();
        var database = scope.ServiceProvider.GetRequiredService<ForgeDbContext>();
        Assert.Empty(await database.Progress.ToListAsync());
    }

    [Theory]
    [InlineData("/privacy?lang=de", "Lokaler Datenschutzhinweis")]
    [InlineData("/privacy?lang=en", "Local data notice")]
    public async Task Privacy_notice_explains_the_local_data_model(string path, string expectedContent)
    {
        var response = await client.GetAsync(path);
        var page = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expectedContent, page, StringComparison.Ordinal);
        Assert.DoesNotContain("href=\"/docs/PRIVACY.md\"", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Privacy_notice_names_the_project_author_and_operator_boundary()
    {
        var page = await client.GetStringAsync("/privacy?lang=de");

        Assert.Contains("Aleksandar Zyro", page, StringComparison.Ordinal);
        Assert.Contains("keine eigene juristische Person", page, StringComparison.Ordinal);
        Assert.Contains("Rechtsgrundlage", page, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/challenge/sql-injection?solved=true", "Warum diese Lösung funktioniert")]
    [InlineData("/en/challenge/sql-injection?solved=true", "Solution explanation")]
    public async Task Solution_is_not_unlocked_by_a_query_string(string path, string solutionText)
    {
        using var isolatedFactory = new ForgeWebApplicationFactory();
        using var isolatedClient = isolatedFactory.CreateClient();
        var page = await isolatedClient.GetStringAsync(path);

        Assert.DoesNotContain(solutionText, page, StringComparison.Ordinal);
        Assert.DoesNotContain("class=\"success\"", page, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("/Challenge/idor", "Deine aktuelle Station")]
    [InlineData("/en/challenge/idor", "Your current station")]
    public async Task Starting_a_station_requires_antiforgery_and_is_recommended_on_the_homepage(string path, string currentHeading)
    {
        using var isolatedFactory = new ForgeWebApplicationFactory();
        using var isolatedClient = isolatedFactory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        Assert.Equal(HttpStatusCode.BadRequest, (await isolatedClient.PostAsync($"{path}?handler=Start", new FormUrlEncodedContent([]))).StatusCode);
        var intro = await isolatedClient.GetStringAsync(path);
        var token = Regex.Match(intro, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        var start = await isolatedClient.PostAsync($"{path}?handler=Start",
            new FormUrlEncodedContent([new KeyValuePair<string, string>("__RequestVerificationToken", token)]));
        Assert.Equal(HttpStatusCode.Redirect, start.StatusCode);
        var home = await isolatedClient.GetStringAsync(path.StartsWith("/en", StringComparison.Ordinal) ? "/en" : "/");
        Assert.Contains(currentHeading, home, StringComparison.Ordinal);
        Assert.Contains("idor", home, StringComparison.Ordinal);
        await using var scope = isolatedFactory.Services.CreateAsyncScope();
        var record = Assert.Single(await scope.ServiceProvider.GetRequiredService<ForgeDbContext>().Progress.ToListAsync());
        Assert.Equal("idor", record.ChallengeId);
        Assert.False(record.IsCompleted);
    }

    [Fact]
    public async Task Existing_progress_survives_localization_and_drives_course_totals()
    {
        using var isolatedFactory = new ForgeWebApplicationFactory();
        using var isolatedClient = isolatedFactory.CreateClient();
        var completedAt = DateTimeOffset.Parse("2026-09-01T12:00:00+00:00", System.Globalization.CultureInfo.InvariantCulture);
        await using (var scope = isolatedFactory.Services.CreateAsyncScope())
        {
            var database = scope.ServiceProvider.GetRequiredService<ForgeDbContext>();
            database.Progress.Add(new ProgressRecord { ChallengeId = "sql-injection", IsCompleted = true, CompletedAt = completedAt, PenaltyPoints = 25, HintsUsed = 1, Attempts = 1 });
            await database.SaveChangesAsync();
        }
        foreach (var path in new[] { "/", "/en", "/Challenge/sql-injection", "/en/challenge/sql-injection" })
        {
            Assert.Equal(HttpStatusCode.OK, (await isolatedClient.GetAsync(path)).StatusCode);
        }
        Assert.Contains("1 von 3 abgeschlossen", await isolatedClient.GetStringAsync("/"), StringComparison.Ordinal);
        Assert.Contains("1 of 3 completed", await isolatedClient.GetStringAsync("/en"), StringComparison.Ordinal);
        await using var readScope = isolatedFactory.Services.CreateAsyncScope();
        var record = Assert.Single(await readScope.ServiceProvider.GetRequiredService<ForgeDbContext>().Progress.ToListAsync());
        Assert.Equal((true, completedAt, 25, 1, 1), (record.IsCompleted, record.CompletedAt, record.PenaltyPoints, record.HintsUsed, record.Attempts));
        var progressService = readScope.ServiceProvider.GetRequiredService<ProgressService>();
        Assert.False(await progressService.UseHintAsync("sql-injection", 2, CancellationToken.None));
        await progressService.RecordIncorrectAttemptAsync("sql-injection", 15, CancellationToken.None);
        await readScope.ServiceProvider.GetRequiredService<ForgeDbContext>().Entry(record).ReloadAsync();
        Assert.Equal((25, 1, 1), (record.PenaltyPoints, record.HintsUsed, record.Attempts));
    }

    [Theory]
    [InlineData("sql-injection", "5101")]
    [InlineData("xss", "5102")]
    [InlineData("idor", "5103")]
    [InlineData("authentication", "5104")]
    [InlineData("file-upload", "5105")]
    [InlineData("ssrf", "5106")]
    public async Task Every_challenge_links_only_to_its_fixed_local_demo(string challengeId, string port)
    {
        var page = await client.GetStringAsync($"/Challenge/{challengeId}");

        Assert.Contains($"http://127.0.0.1:{port}", page, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Migration_initializer_preserves_progress_from_a_legacy_ensurecreated_database()
    {
        var directory = Path.Combine(Path.GetTempPath(), "OwaspForge.Tests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        var databasePath = Path.Combine(directory, "legacy.db");
        var options = new DbContextOptionsBuilder<ForgeDbContext>().UseSqlite($"Data Source={databasePath};Pooling=False").Options;

        try
        {
            await using (var legacyDatabase = new ForgeDbContext(options))
            {
                await legacyDatabase.Database.EnsureCreatedAsync();
                legacyDatabase.Progress.Add(new ProgressRecord { ChallengeId = "xss", IsCompleted = true, CompletedAt = DateTimeOffset.UtcNow });
                await legacyDatabase.SaveChangesAsync();
            }

            await using (var migratedDatabase = new ForgeDbContext(options))
            {
                await ForgeDatabaseInitializer.InitializeAsync(migratedDatabase);
                Assert.Contains(ForgeDatabaseInitializer.InitialMigrationId, await migratedDatabase.Database.GetAppliedMigrationsAsync());
                var progress = await migratedDatabase.Progress.SingleAsync(record => record.ChallengeId == "xss");
                Assert.True(progress.IsCompleted);
            }
        }
        finally
        {
            Directory.Delete(directory, recursive: true);
        }
    }

    [Fact]
    public async Task Challenge_requires_the_intro_step_and_deducts_for_hints_and_wrong_answers()
    {
        const string challengePath = "/Challenge/xss";
        var intro = await client.GetStringAsync(challengePath);
        Assert.Contains("Historische Variante", intro, StringComparison.Ordinal);
        Assert.Contains("Weiter", intro, StringComparison.Ordinal);
        Assert.DoesNotContain("Warum diese Lösung funktioniert", intro, StringComparison.Ordinal);

        var quizResponse = await client.GetAsync($"{challengePath}?step=quiz");
        var quiz = await quizResponse.Content.ReadAsStringAsync();
        var token = Regex.Match(quiz, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        Assert.Contains("Auswerten", quiz, StringComparison.Ordinal);
        Assert.Contains("Welche Antwort passt jeweils?", quiz, StringComparison.Ordinal);

        var wrongResponse = await client.PostAsync($"{challengePath}?handler=Check", new FormUrlEncodedContent([
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
            new KeyValuePair<string, string>("answers[0]", "raw"),
            new KeyValuePair<string, string>("answers[1]", "raw"),
            new KeyValuePair<string, string>("answers[2]", "raw"),
            new KeyValuePair<string, string>("answers[3]", "raw"),
            new KeyValuePair<string, string>("answers[4]", "raw"),
            new KeyValuePair<string, string>("answers[5]", "raw"),
        ]));
        var wrongPage = await wrongResponse.Content.ReadAsStringAsync();
        Assert.Contains("90 Punkte", wrongPage, StringComparison.Ordinal);
        Assert.DoesNotContain("Warum diese Lösung funktioniert", wrongPage, StringComparison.Ordinal);

        var wrongToken = Regex.Match(wrongPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        var hintResponse = await client.PostAsync($"{challengePath}?handler=Hint", new FormUrlEncodedContent([
            new KeyValuePair<string, string>("__RequestVerificationToken", wrongToken),
            new KeyValuePair<string, string>("level", "1"),
        ]));
        Assert.Equal(HttpStatusCode.Redirect, hintResponse.StatusCode);
        Assert.Equal("/Challenge/xss?step=quiz&hint=1", hintResponse.Headers.Location?.OriginalString);
        var hintedPage = await client.GetStringAsync(hintResponse.Headers.Location!.OriginalString);
        Assert.Contains("Razor encodiert normale Textausgaben automatisch", hintedPage, StringComparison.Ordinal);
        Assert.DoesNotContain("Warum diese Lösung funktioniert", hintedPage, StringComparison.Ordinal);
    }
}

public sealed class ForgeWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string dataDirectory = Path.Combine(Path.GetTempPath(), "OwaspForge.Tests", Guid.NewGuid().ToString("N"));

    protected override void ConfigureWebHost(IWebHostBuilder builder) =>
        builder
            .UseEnvironment("Testing")
            .UseSetting("Forge:DataDirectory", dataDirectory);

}

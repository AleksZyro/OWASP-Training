using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
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

        var completeResponse = await client.PostAsync(
            $"{challengePath}?handler=Check",
            new FormUrlEncodedContent(
            [
                new KeyValuePair<string, string>("__RequestVerificationToken", token),
                new KeyValuePair<string, string>("answer", "parameter"),
            ]));

        Assert.Equal(HttpStatusCode.Redirect, completeResponse.StatusCode);
        Assert.Equal("/Challenge/sql-injection?solved=true&step=solution", completeResponse.Headers.Location?.OriginalString);

        var home = await client.GetStringAsync("/");
        Assert.Contains("Abgeschlossen", home, StringComparison.Ordinal);
    }

    [Fact]
    public async Task Opening_and_resetting_a_station_updates_the_visible_local_progress_state()
    {
        const string challengePath = "/Challenge/xss";
        var openResponse = await client.GetAsync(challengePath);
        Assert.Equal(HttpStatusCode.OK, openResponse.StatusCode);

        var inProgressHome = await client.GetStringAsync("/");
        Assert.Contains("In Bearbeitung", inProgressHome, StringComparison.Ordinal);

        var page = await openResponse.Content.ReadAsStringAsync();
        var token = Regex.Match(page, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        var resetResponse = await client.PostAsync(
            $"{challengePath}?handler=Reset",
            new FormUrlEncodedContent([new KeyValuePair<string, string>("__RequestVerificationToken", token)]));

        Assert.Equal(HttpStatusCode.Redirect, resetResponse.StatusCode);
        Assert.Equal("/Challenge/xss?reset=true", resetResponse.Headers.Location?.OriginalString);

        var resetPage = await client.GetStringAsync("/Challenge/xss?reset=true");
        Assert.Contains("Station zurückgesetzt", resetPage, StringComparison.Ordinal);

        var resetHome = await client.GetStringAsync("/");
        Assert.Contains("Bereit", resetHome, StringComparison.Ordinal);
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
        Assert.Contains("Weiter zur Prüfung", intro, StringComparison.Ordinal);
        Assert.DoesNotContain("Lösungserklärung", intro, StringComparison.Ordinal);

        var quizResponse = await client.GetAsync($"{challengePath}?step=quiz");
        var quiz = await quizResponse.Content.ReadAsStringAsync();
        var token = Regex.Match(quiz, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        Assert.Contains("Antwort prüfen", quiz, StringComparison.Ordinal);

        var wrongResponse = await client.PostAsync($"{challengePath}?handler=Check", new FormUrlEncodedContent([
            new KeyValuePair<string, string>("__RequestVerificationToken", token),
            new KeyValuePair<string, string>("answer", "raw"),
        ]));
        var wrongPage = await wrongResponse.Content.ReadAsStringAsync();
        Assert.Contains("15 Punkte wurden", wrongPage, StringComparison.Ordinal);
        Assert.DoesNotContain("Lösungserklärung", wrongPage, StringComparison.Ordinal);

        var wrongToken = Regex.Match(wrongPage, "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"([^\"]+)\"").Groups[1].Value;
        var hintResponse = await client.PostAsync($"{challengePath}?handler=Hint", new FormUrlEncodedContent([
            new KeyValuePair<string, string>("__RequestVerificationToken", wrongToken),
            new KeyValuePair<string, string>("level", "1"),
        ]));
        Assert.Equal(HttpStatusCode.Redirect, hintResponse.StatusCode);
        Assert.Equal("/Challenge/xss?step=quiz&hint=1", hintResponse.Headers.Location?.OriginalString);
        var hintedPage = await client.GetStringAsync(hintResponse.Headers.Location!.OriginalString);
        Assert.Contains("Hinweis 1", hintedPage, StringComparison.Ordinal);
        Assert.DoesNotContain("Lösungserklärung", hintedPage, StringComparison.Ordinal);
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

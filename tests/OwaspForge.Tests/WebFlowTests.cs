using System.Net;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;

namespace OwaspForge.Tests;

public sealed class WebFlowTests : IClassFixture<ForgeWebApplicationFactory>
{
    private readonly HttpClient client;

    public WebFlowTests(ForgeWebApplicationFactory factory)
    {
        client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
    }

    [Theory]
    [InlineData("/", "Defensives Wissen")]
    [InlineData("/en", "Defensive knowledge")]
    public async Task Start_pages_expose_the_local_security_boundary(string path, string expectedContent)
    {
        var response = await client.GetAsync(path);
        var page = await response.Content.ReadAsStringAsync();

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains(expectedContent, page, StringComparison.Ordinal);
        Assert.Contains("default-src 'self'", response.Headers.GetValues("Content-Security-Policy").Single(), StringComparison.Ordinal);
        Assert.Equal("DENY", response.Headers.GetValues("X-Frame-Options").Single());
        Assert.Equal("nosniff", response.Headers.GetValues("X-Content-Type-Options").Single());
    }

    [Fact]
    public async Task Challenge_completion_requires_an_antiforgery_token_and_marks_progress_server_side()
    {
        const string challengePath = "/challenge/sql-injection";
        var directPost = await client.PostAsync($"{challengePath}?handler=Check", new FormUrlEncodedContent([new KeyValuePair<string, string>("answer", "parameter")]));
        Assert.Equal(HttpStatusCode.BadRequest, directPost.StatusCode);

        var getResponse = await client.GetAsync(challengePath);
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
        Assert.Equal("/Challenge/sql-injection?solved=true", completeResponse.Headers.Location?.OriginalString);

        var home = await client.GetStringAsync("/");
        Assert.Contains("Gesichert", home, StringComparison.Ordinal);
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

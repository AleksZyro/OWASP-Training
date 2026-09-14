using System.Diagnostics;
using Microsoft.Playwright;
using OwaspForge.Core;

namespace OwaspForge.Tests;

public sealed class BrowserFlowTests
{
    [Fact]
    public async Task Learning_path_and_server_checked_solution_work_in_chromium()
    {
        using var playwright = await Playwright.CreateAsync();
        if (!File.Exists(playwright.Chromium.ExecutablePath))
        {
            return;
        }

        var port = 5091;
        var dataDirectory = Path.Combine(Path.GetTempPath(), "OwaspForge.BrowserTests", Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(dataDirectory);
        using var process = StartPlatform(port, dataDirectory);

        try
        {
            await WaitForPlatformAsync(port);
            await using var browser = await playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions { Headless = true });
            var page = await browser.NewPageAsync(new BrowserNewPageOptions { ViewportSize = new ViewportSize { Width = 375, Height = 800 } });
            page.SetDefaultTimeout(5_000);
            page.SetDefaultNavigationTimeout(5_000);

            await page.GotoAsync($"http://127.0.0.1:{port}/");
            Assert.Contains("Sicherheitswissen", await page.Locator("h1").InnerTextAsync(), StringComparison.Ordinal);
            Assert.Equal(12, await page.Locator(".path-station").CountAsync());

            await page.GotoAsync($"http://127.0.0.1:{port}/Challenge/sql-injection");
            await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Weiter zur Prüfung" }).ClickAsync();
            var questions = new ChallengeRegistry().Find("sql-injection")!.Questions;
            for (var questionIndex = 0; questionIndex < questions.Count; questionIndex++)
            {
                await page.Locator($".question-block:nth-of-type({questionIndex + 1}) input[value='{questions[questionIndex].CorrectOption}']").CheckAsync();
            }
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Antworten prüfen" }).ClickAsync();
            await page.WaitForURLAsync("**/Challenge/sql-injection?solved=true&step=solution");
            Assert.Contains("Station gesichert", await page.Locator("main").InnerTextAsync(), StringComparison.Ordinal);
        }
        finally
        {
            if (!process.HasExited)
            {
                process.Kill(entireProcessTree: true);
                await process.WaitForExitAsync();
            }

            DeleteDirectoryWithRetry(dataDirectory);
        }
    }

    private static Process StartPlatform(int port, string dataDirectory)
    {
        var webAssembly = Path.Combine(AppContext.BaseDirectory, "OwaspForge.Web.dll");
        var startInfo = new ProcessStartInfo("dotnet", $"\"{webAssembly}\"")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = AppContext.BaseDirectory,
        };
        startInfo.Environment["ASPNETCORE_URLS"] = $"http://127.0.0.1:{port}";
        startInfo.Environment["Forge__DataDirectory"] = dataDirectory;
        return Process.Start(startInfo) ?? throw new InvalidOperationException("The local test platform could not be started.");
    }

    private static async Task WaitForPlatformAsync(int port)
    {
        using var client = new HttpClient();
        for (var attempt = 0; attempt < 30; attempt++)
        {
            try
            {
                if ((await client.GetAsync($"http://127.0.0.1:{port}/")).IsSuccessStatusCode)
                {
                    return;
                }
            }
            catch (HttpRequestException)
            {
                // The local Kestrel host is still starting.
            }

            await Task.Delay(TimeSpan.FromMilliseconds(200));
        }

        throw new TimeoutException("The local test platform did not become reachable.");
    }

    private static void DeleteDirectoryWithRetry(string path)
    {
        for (var attempt = 1; attempt <= 3; attempt++)
        {
            try
            {
                Directory.Delete(path, recursive: true);
                return;
            }
            catch (IOException) when (attempt < 3)
            {
                Thread.Sleep(100 * attempt);
            }
        }

        Directory.Delete(path, recursive: true);
    }
}

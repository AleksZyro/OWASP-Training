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
            throw new InvalidOperationException("Install Chromium using the generated playwright.ps1 install chromium script before running browser tests.");
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

            foreach (var width in new[] { 375, 768, 1024, 1440 })
            {
                await page.SetViewportSizeAsync(width, 900);
                foreach (var prefix in new[] { "", "/en" })
                {
                    await page.GotoAsync($"http://127.0.0.1:{port}{prefix}/");
                    Assert.Equal(12, await page.Locator(".path-station").CountAsync());
                    Assert.Equal(4, await page.Locator(".course-completion progress").CountAsync());
                    Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= window.innerWidth"), $"Home overflow at {width}px ({prefix}).");
                    Assert.Equal("rgb(16, 19, 24)", await page.Locator("body").EvaluateAsync<string>("element => getComputedStyle(element).backgroundColor"));
                    await CaptureAsync(page, $"home-{(prefix.Length == 0 ? "de" : "en")}-{width}");
                    await page.GotoAsync($"http://127.0.0.1:{port}{prefix}/Challenge/sql-injection");
                    Assert.Equal(1, await page.Locator(".risky-example").CountAsync());
                    Assert.Equal(1, await page.Locator(".safe-example").CountAsync());
                    Assert.True(await page.EvaluateAsync<bool>("document.documentElement.scrollWidth <= window.innerWidth"), $"Lesson overflow at {width}px ({prefix}).");
                    await CaptureAsync(page, $"lesson-{(prefix.Length == 0 ? "de" : "en")}-{width}");
                }
            }

            await page.GotoAsync($"http://127.0.0.1:{port}/Challenge/sql-injection");
            await page.Keyboard.PressAsync("Tab");
            Assert.Equal("Zum Inhalt springen", await page.Locator(":focus").InnerTextAsync());
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Weiter", Exact = true }).ClickAsync();
            var questions = new ChallengeRegistry().Find("sql-injection")!.Questions;
            for (var questionIndex = 0; questionIndex < questions.Count; questionIndex++)
            {
                await page.Locator($".question-block:nth-of-type({questionIndex + 1}) input[value='{questions[questionIndex].CorrectOption}']").CheckAsync();
            }
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Auswerten" }).ClickAsync();
            await page.WaitForURLAsync("**/Challenge/sql-injection?solved=true&step=solution");
            Assert.Contains("Station abgeschlossen", await page.Locator("main").InnerTextAsync(), StringComparison.Ordinal);
            await page.GotoAsync($"http://127.0.0.1:{port}/en/challenge/sql-injection?step=solution");
            Assert.Contains("Station complete", await page.Locator("main").InnerTextAsync(), StringComparison.Ordinal);
            Assert.Equal(1, await page.Locator(".solution").CountAsync());
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Reset station", Exact = true }).ClickAsync();
            await page.WaitForURLAsync("**/en/challenge/sql-injection?reset=true");
            Assert.Equal(0, await page.Locator(".solution").CountAsync());
            await page.GotoAsync($"http://127.0.0.1:{port}/en");
            Assert.Contains("0 of 12 stations completed", await page.Locator(".course-progress").InnerTextAsync(), StringComparison.Ordinal);
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

    private static async Task CaptureAsync(IPage page, string name)
    {
        var directory = Environment.GetEnvironmentVariable("FORGE_UI_ARTIFACTS");
        if (string.IsNullOrWhiteSpace(directory)) return;
        Directory.CreateDirectory(directory);
        await page.ScreenshotAsync(new PageScreenshotOptions { Path = Path.Combine(directory, $"{name}.png"), FullPage = true });
    }

    private static Process StartPlatform(int port, string dataDirectory)
    {
        var webAssembly = Path.Combine(AppContext.BaseDirectory, "OwaspForge.Web.dll");
        var startInfo = new ProcessStartInfo("dotnet", $"\"{webAssembly}\"")
        {
            UseShellExecute = false,
            CreateNoWindow = true,
            WorkingDirectory = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..", "src", "OwaspForge.Web")),
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

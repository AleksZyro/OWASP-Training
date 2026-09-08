using System.Diagnostics;
using Microsoft.Playwright;

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
            Assert.Equal(6, await page.Locator(".path-station").CountAsync());

            await page.GotoAsync($"http://127.0.0.1:{port}/Challenge/sql-injection");
            await page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Weiter zur Prüfung" }).ClickAsync();
            await page.Locator("input[value='parameter']").CheckAsync();
            await page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Antwort prüfen" }).ClickAsync();
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

            Directory.Delete(dataDirectory, recursive: true);
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
}

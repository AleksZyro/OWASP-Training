using Microsoft.AspNetCore.Antiforgery;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OwaspForge.Core;
using OwaspForge.Web.Data;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.WebHost.UseUrls("http://127.0.0.1:5080");
}
var configuredDataDirectory = builder.Configuration["Forge:DataDirectory"];
var dataDirectory = string.IsNullOrWhiteSpace(configuredDataDirectory)
    ? Path.Combine(builder.Environment.ContentRootPath, "data")
    : Path.GetFullPath(configuredDataDirectory, builder.Environment.ContentRootPath);
Directory.CreateDirectory(dataDirectory);
builder.Services.AddDbContext<ForgeDbContext>(options => options.UseSqlite($"Data Source={Path.Combine(dataDirectory, "forge.db")}"));
builder.Services.AddSingleton<ChallengeRegistry>();
builder.Services.AddSingleton<EnglishChallengeRegistry>();
builder.Services.AddScoped<ChallengeValidator>();
builder.Services.AddScoped<ProgressService>();
builder.Services.AddAntiforgery(options =>
{
    // __Host- cookies require HTTPS. The lab intentionally serves only local HTTP.
    options.Cookie.Name = "OwaspForge.Antiforgery";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Strict;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
});
builder.Services.AddRazorPages(options => options.Conventions.ConfigureFilter(new AutoValidateAntiforgeryTokenAttribute()));

var app = builder.Build();
app.UseExceptionHandler("/Error");
app.Use(async (context, next) =>
{
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("Referrer-Policy", "no-referrer");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'; base-uri 'self'; form-action 'self'; frame-ancestors 'none'");
    await next();
});
app.UseStaticFiles(new StaticFileOptions { OnPrepareResponse = context => context.Context.Response.Headers.Append("Cache-Control", "no-store") });
app.UseRouting();
app.MapRazorPages();
using (var scope = app.Services.CreateScope())
{
    var database = scope.ServiceProvider.GetRequiredService<ForgeDbContext>();
    await database.Database.EnsureCreatedAsync();
}
app.Run();

public partial class Program;

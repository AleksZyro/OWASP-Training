using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OwaspForge.Core;
using OwaspForge.Web.Data;

namespace OwaspForge.Web.Pages;

public sealed class EnglishChallengeModel(EnglishChallengeRegistry registry, ChallengeValidator validator, ProgressService progressService) : PageModel
{
    private static readonly IReadOnlyDictionary<string, int> DemoPorts = new Dictionary<string, int>(StringComparer.Ordinal)
    {
        ["sql-injection"] = 5101,
        ["xss"] = 5102,
        ["idor"] = 5103,
        ["authentication"] = 5104,
        ["file-upload"] = 5105,
        ["ssrf"] = 5106,
    };

    public ChallengeDefinition? SelectedChallenge { get; private set; }
    public ChallengeDefinition? NextChallenge { get; private set; }
    public bool IsCompleted { get; private set; }
    public string? Feedback { get; private set; }
    public string? DemoUrl => SelectedChallenge is not null && DemoPorts.TryGetValue(SelectedChallenge.Id, out var port)
        ? $"http://127.0.0.1:{port}"
        : null;

    public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
    {
        SelectedChallenge = registry.Find(id);
        if (SelectedChallenge is null) return NotFound();
        if (Request.Query["reset"] != "true")
        {
            await progressService.MarkStartedAsync(SelectedChallenge, cancellationToken);
        }
        var completedIds = await progressService.CompletedIdsAsync(cancellationToken);
        IsCompleted = completedIds.Contains(id);
        NextChallenge = registry.All.FirstOrDefault(challenge => challenge.Id != id && !completedIds.Contains(challenge.Id));
        return Page();
    }

    public async Task<IActionResult> OnPostCheckAsync(string id, string? answer, CancellationToken cancellationToken)
    {
        SelectedChallenge = registry.Find(id);
        if (SelectedChallenge is null) return NotFound();
        await progressService.MarkStartedAsync(SelectedChallenge, cancellationToken);
        if (validator.IsSolved(id, answer))
        {
            await progressService.MarkCompletedAsync(SelectedChallenge, cancellationToken);
            return Redirect($"/en/challenge/{id}?solved=true");
        }

        Feedback = SelectedChallenge.Options.SingleOrDefault(option => option.Value == answer)?.Feedback
            ?? "Not quite. Use a hint and identify the rule enforced on the server.";
        return Page();
    }

    public async Task<IActionResult> OnPostResetAsync(string id, CancellationToken cancellationToken)
    {
        if (registry.Find(id) is null) return NotFound();
        await progressService.ResetAsync(id, cancellationToken);
        return Redirect($"/en/challenge/{id}?reset=true");
    }
}

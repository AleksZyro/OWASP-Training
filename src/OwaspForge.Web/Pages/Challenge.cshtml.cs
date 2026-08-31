using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OwaspForge.Core;
using OwaspForge.Web.Data;

namespace OwaspForge.Web.Pages;

public sealed class ChallengeModel(ChallengeRegistry registry, ChallengeValidator validator, ProgressService progressService) : PageModel
{
    public ChallengeDefinition? SelectedChallenge { get; private set; }
    public bool IsCompleted { get; private set; }
    public string? Feedback { get; private set; }
    public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
    {
        SelectedChallenge = registry.Find(id);
        if (SelectedChallenge is null) return NotFound();
        IsCompleted = (await progressService.CompletedIdsAsync(cancellationToken)).Contains(id);
        return Page();
    }
    public async Task<IActionResult> OnPostCheckAsync(string id, string? answer, CancellationToken cancellationToken)
    {
        SelectedChallenge = registry.Find(id);
        if (SelectedChallenge is null) return NotFound();
        if (validator.IsSolved(id, answer))
        {
            await progressService.MarkCompletedAsync(SelectedChallenge, cancellationToken);
            return RedirectToPage(new { id, solved = "true" });
        }
        Feedback = "Noch nicht ganz. Nutze einen Hinweis und prüfe, welche Regel auf dem Server durchgesetzt werden muss.";
        return Page();
    }
    public async Task<IActionResult> OnPostResetAsync(string id, CancellationToken cancellationToken)
    {
        if (registry.Find(id) is null) return NotFound();
        await progressService.ResetAsync(id, cancellationToken);
        return RedirectToPage(new { id });
    }
}

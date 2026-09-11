using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OwaspForge.Core;
using OwaspForge.Web.Data;

namespace OwaspForge.Web.Pages;

public sealed class ChallengeModel(ChallengeRegistry registry, ChallengeValidator validator, ProgressService progressService) : PageModel
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
    public bool ShowQuiz { get; private set; }
    public bool CanSeeSolution { get; private set; }
    public int PenaltyPoints { get; private set; }
    public int EarnedPoints => SelectedChallenge is null ? 0 : Math.Max(0, SelectedChallenge.Points - PenaltyPoints);
    public int HintsUsed { get; private set; }
    public int Attempts { get; private set; }
    public int QuestionCount => SelectedChallenge?.Questions.Count ?? 0;
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
        ShowQuiz = Request.Query["step"] == "quiz" || Request.Query["solved"] == "true";
        CanSeeSolution = IsCompleted;
        var record = await progressService.FindAsync(id, cancellationToken);
        PenaltyPoints = record?.PenaltyPoints ?? 0;
        HintsUsed = record?.HintsUsed ?? 0;
        Attempts = record?.Attempts ?? 0;
        NextChallenge = registry.All.FirstOrDefault(challenge => challenge.Id != id && !completedIds.Contains(challenge.Id));
        return Page();
    }
    public async Task<IActionResult> OnPostCheckAsync(string id, string[]? answers, CancellationToken cancellationToken)
    {
        SelectedChallenge = registry.Find(id);
        if (SelectedChallenge is null) return NotFound();
        var existingRecord = await progressService.FindAsync(id, cancellationToken);
        if (existingRecord?.IsCompleted == true)
        {
            return RedirectToPage(new { id, solved = "true", step = "solution" });
        }
        ShowQuiz = true;
        await progressService.MarkStartedAsync(SelectedChallenge, cancellationToken);
        var submittedAnswers = answers ?? [];
        if (validator.IsSolved(id, submittedAnswers))
        {
            await progressService.MarkCompletedAsync(SelectedChallenge, cancellationToken);
            return RedirectToPage(new { id, solved = "true", step = "solution" });
        }
        var incorrectCount = Math.Max(1, validator.CountIncorrect(id, submittedAnswers));
        await progressService.RecordIncorrectAttemptAsync(id, 15 * incorrectCount, cancellationToken);
        var currentRecord = await progressService.FindAsync(id, cancellationToken);
        PenaltyPoints = currentRecord?.PenaltyPoints ?? 0;
        HintsUsed = currentRecord?.HintsUsed ?? 0;
        Attempts = currentRecord?.Attempts ?? 0;
        var selectedOption = SelectedChallenge.Questions.FirstOrDefault()?.Options.SingleOrDefault(option => option.Value == submittedAnswers.FirstOrDefault());
        Feedback = selectedOption is { Feedback.Length: > 0 }
            ? $"{selectedOption.Feedback} {incorrectCount} Antwort(en) waren noch nicht korrekt; dafür wurden {15 * incorrectCount} Punkte abgezogen."
            : $"Noch nicht ganz. {incorrectCount} Antwort(en) waren noch nicht korrekt; dafür wurden {15 * incorrectCount} Punkte abgezogen. Nutze einen Hinweis und prüfe, welche Regel auf dem Server durchgesetzt werden muss.";
        return Page();
    }
    public async Task<IActionResult> OnPostHintAsync(string id, int level, CancellationToken cancellationToken)
    {
        SelectedChallenge = registry.Find(id);
        if (SelectedChallenge is null || level is < 1 or > 2) return NotFound();
        await progressService.UseHintAsync(id, level, cancellationToken);
        return RedirectToPage(new { id, step = "quiz", hint = level });
    }
    public async Task<IActionResult> OnPostResetAsync(string id, CancellationToken cancellationToken)
    {
        if (registry.Find(id) is null) return NotFound();
        await progressService.ResetAsync(id, cancellationToken);
        return RedirectToPage(new { id, reset = "true" });
    }
}

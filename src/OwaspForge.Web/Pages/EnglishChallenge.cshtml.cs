using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OwaspForge.Core;
using OwaspForge.Core.Challenges;
using OwaspForge.Web.Data;

namespace OwaspForge.Web.Pages;

public sealed class EnglishChallengeModel(EnglishChallengeRegistry registry, ChallengeValidator validator, ProgressService progressService, ILogger<EnglishChallengeModel> logger) : PageModel
{
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
    public string? DemoUrl => DemoCatalog.Find(SelectedChallenge?.DemoId)?.Url;

    public async Task<IActionResult> OnGetAsync(string id, CancellationToken cancellationToken)
    {
        SelectedChallenge = registry.Find(id);
        if (SelectedChallenge is null) return NotFound();
        var completedIds = await progressService.CompletedIdsAsync(cancellationToken);
        IsCompleted = completedIds.Contains(id);
        ShowQuiz = IsCompleted || Request.Query["step"] == "quiz" || Request.Query["step"] == "solution";
        CanSeeSolution = IsCompleted;
        var record = await progressService.FindAsync(id, cancellationToken);
        PenaltyPoints = record?.PenaltyPoints ?? 0;
        HintsUsed = record?.HintsUsed ?? 0;
        Attempts = record?.Attempts ?? 0;
        NextChallenge = registry.All.FirstOrDefault(challenge => challenge.Id != id && !completedIds.Contains(challenge.Id));
        return Page();
    }

    public async Task<IActionResult> OnPostStartAsync(string id, CancellationToken cancellationToken)
    {
        var challenge = registry.Find(id);
        if (challenge is null) return NotFound();
        await progressService.MarkStartedAsync(challenge, cancellationToken);
        return Redirect($"/en/challenge/{id}?step=quiz");
    }

    public async Task<IActionResult> OnPostCheckAsync(string id, string[]? answers, CancellationToken cancellationToken)
    {
        SelectedChallenge = registry.Find(id);
        if (SelectedChallenge is null)
        {
            logger.LogWarning("Rejected challenge check for an unknown station.");
            return NotFound();
        }
        var existingRecord = await progressService.FindAsync(id, cancellationToken);
        if (existingRecord?.IsCompleted == true)
        {
            return Redirect($"/en/challenge/{id}?solved=true&step=solution");
        }
        ShowQuiz = true;
        await progressService.MarkStartedAsync(SelectedChallenge, cancellationToken);
        var submittedAnswers = answers ?? [];
        if (validator.IsSolved(id, submittedAnswers))
        {
            await progressService.MarkCompletedAsync(SelectedChallenge, cancellationToken);
            logger.LogInformation("Challenge completed for station {ChallengeId}.", SelectedChallenge.Id);
            return Redirect($"/en/challenge/{id}?solved=true&step=solution");
        }

        var incorrectCount = Math.Max(1, validator.CountIncorrect(id, submittedAnswers));
        await progressService.RecordIncorrectAttemptAsync(id, 15 * incorrectCount, cancellationToken);
        logger.LogWarning("Challenge answer rejected for station {ChallengeId}; incorrect answers: {IncorrectCount}.", SelectedChallenge.Id, incorrectCount);
        var currentRecord = await progressService.FindAsync(id, cancellationToken);
        PenaltyPoints = currentRecord?.PenaltyPoints ?? 0;
        HintsUsed = currentRecord?.HintsUsed ?? 0;
        Attempts = currentRecord?.Attempts ?? 0;

        var selectedOption = SelectedChallenge.Questions.FirstOrDefault()?.Options.SingleOrDefault(option => option.Value == submittedAnswers.FirstOrDefault());
        Feedback = selectedOption is { Feedback.Length: > 0 }
            ? $"{selectedOption.Feedback} {incorrectCount} answer(s) were incorrect; {15 * incorrectCount} points were deducted."
            : $"Not quite. {incorrectCount} answer(s) were incorrect; {15 * incorrectCount} points were deducted. Use a hint and identify the rule enforced on the server.";
        return Page();
    }

    public async Task<IActionResult> OnPostHintAsync(string id, int level, CancellationToken cancellationToken)
    {
        SelectedChallenge = registry.Find(id);
        if (SelectedChallenge is null || level is < 1 or > 2)
        {
            logger.LogWarning("Rejected invalid challenge hint request.");
            return NotFound();
        }
        if (await progressService.UseHintAsync(id, level, cancellationToken))
        {
            logger.LogInformation("Challenge hint opened for station {ChallengeId}; level {HintLevel}.", SelectedChallenge.Id, level);
        }
        return Redirect($"/en/challenge/{id}?step=quiz&hint={level}");
    }

    public async Task<IActionResult> OnPostResetAsync(string id, CancellationToken cancellationToken)
    {
        if (registry.Find(id) is null)
        {
            logger.LogWarning("Rejected reset for an unknown station.");
            return NotFound();
        }
        await progressService.ResetAsync(id, cancellationToken);
        logger.LogInformation("Challenge progress reset for station {ChallengeId}.", id);
        return Redirect($"/en/challenge/{id}?reset=true");
    }
}

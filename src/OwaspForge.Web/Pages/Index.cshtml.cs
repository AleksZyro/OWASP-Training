using Microsoft.AspNetCore.Mvc.RazorPages;
using OwaspForge.Core;
using OwaspForge.Web.Data;

namespace OwaspForge.Web.Pages;

public sealed class IndexModel(ChallengeRegistry registry, ProgressService progressService) : PageModel
{
    public IReadOnlyList<ChallengeDefinition> Challenges { get; private set; } = [];
    public IReadOnlySet<string> CompletedIds { get; private set; } = new HashSet<string>();
    public int CompletedCount => CompletedIds.Count;
    public int MaximumPoints => Challenges.Sum(challenge => challenge.Points);
    public int TotalPoints => Challenges.Where(challenge => CompletedIds.Contains(challenge.Id)).Sum(challenge => challenge.Points);

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Challenges = registry.All;
        CompletedIds = await progressService.CompletedIdsAsync(cancellationToken);
    }
}

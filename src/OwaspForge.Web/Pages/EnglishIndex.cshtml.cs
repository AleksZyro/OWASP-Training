using Microsoft.AspNetCore.Mvc.RazorPages;
using OwaspForge.Core;
using OwaspForge.Web.Data;

namespace OwaspForge.Web.Pages;

public sealed class EnglishIndexModel(EnglishChallengeRegistry registry, ProgressService progressService) : PageModel
{
    public IReadOnlyList<ChallengeDefinition> Challenges { get; private set; } = [];
    public IReadOnlySet<string> CompletedIds { get; private set; } = new HashSet<string>();
    public IReadOnlySet<string> StartedIds { get; private set; } = new HashSet<string>();
    public IReadOnlyDictionary<string, int> Penalties { get; private set; } = new Dictionary<string, int>();
    public int CompletedCount => CompletedIds.Count;
    public int MaximumPoints => Challenges.Sum(item => item.Points);
    public int TotalPoints => Challenges.Where(item => CompletedIds.Contains(item.Id)).Sum(item => Math.Max(0, item.Points - (Penalties.TryGetValue(item.Id, out var penalty) ? penalty : 0)));
    public ChallengeDefinition? NextChallenge => Challenges.FirstOrDefault(challenge => !CompletedIds.Contains(challenge.Id));
    public IReadOnlyList<IGrouping<string, ChallengeDefinition>> Courses => Challenges.GroupBy(challenge => challenge.Course).ToArray();
    public async Task OnGetAsync(CancellationToken cancellationToken) { Challenges = registry.All; CompletedIds = await progressService.CompletedIdsAsync(cancellationToken); StartedIds = await progressService.StartedIdsAsync(cancellationToken); Penalties = await progressService.PenaltiesAsync(cancellationToken); }
}

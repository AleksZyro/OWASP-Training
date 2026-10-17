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
    public int CompletedCount => Challenges.Count(challenge => CompletedIds.Contains(challenge.Id));
    public int MaximumPoints => Challenges.Sum(item => item.Points);
    public int TotalPoints => Challenges.Where(item => CompletedIds.Contains(item.Id)).Sum(item => Math.Max(0, item.Points - (Penalties.TryGetValue(item.Id, out var penalty) ? penalty : 0)));
    public ChallengeDefinition? NextChallenge => Challenges.FirstOrDefault(challenge => StartedIds.Contains(challenge.Id) && !CompletedIds.Contains(challenge.Id))
        ?? Challenges.FirstOrDefault(challenge => !CompletedIds.Contains(challenge.Id));
    public int CourseCompleted(IEnumerable<ChallengeDefinition> course) => course.Count(challenge => CompletedIds.Contains(challenge.Id));
    public IReadOnlyList<IGrouping<string, ChallengeDefinition>> Courses => Challenges.GroupBy(challenge => challenge.Course).ToArray();
    public string CourseDescription(string course) => course switch
    {
        "Input and Output" => "How input reaches databases, browsers and file systems safely.",
        "Identity and Access" => "How identity, sessions and object ownership work together on the server.",
        "Secure Architecture" => "How boundaries, keys and configuration limit risk early.",
        _ => "How inventory, origin and logs make operational security reviewable."
    };
    public async Task OnGetAsync(CancellationToken cancellationToken) { Challenges = registry.All; CompletedIds = await progressService.CompletedIdsAsync(cancellationToken); StartedIds = await progressService.StartedIdsAsync(cancellationToken); Penalties = await progressService.PenaltiesAsync(cancellationToken); }
}

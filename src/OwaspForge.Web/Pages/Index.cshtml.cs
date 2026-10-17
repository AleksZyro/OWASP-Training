using Microsoft.AspNetCore.Mvc.RazorPages;
using OwaspForge.Core;
using OwaspForge.Web.Data;

namespace OwaspForge.Web.Pages;

public sealed class IndexModel(ChallengeRegistry registry, ProgressService progressService) : PageModel
{
    public IReadOnlyList<ChallengeDefinition> Challenges { get; private set; } = [];
    public IReadOnlySet<string> CompletedIds { get; private set; } = new HashSet<string>();
    public IReadOnlySet<string> StartedIds { get; private set; } = new HashSet<string>();
    public IReadOnlyDictionary<string, int> Penalties { get; private set; } = new Dictionary<string, int>();
    public int CompletedCount => Challenges.Count(challenge => CompletedIds.Contains(challenge.Id));
    public int MaximumPoints => Challenges.Sum(challenge => challenge.Points);
    public int TotalPoints => Challenges.Where(challenge => CompletedIds.Contains(challenge.Id)).Sum(challenge => Math.Max(0, challenge.Points - (Penalties.TryGetValue(challenge.Id, out var penalty) ? penalty : 0)));
    public ChallengeDefinition? NextChallenge => Challenges.FirstOrDefault(challenge => StartedIds.Contains(challenge.Id) && !CompletedIds.Contains(challenge.Id))
        ?? Challenges.FirstOrDefault(challenge => !CompletedIds.Contains(challenge.Id));
    public int CourseCompleted(IEnumerable<ChallengeDefinition> course) => course.Count(challenge => CompletedIds.Contains(challenge.Id));
    public IReadOnlyList<IGrouping<string, ChallengeDefinition>> Courses => Challenges.GroupBy(challenge => challenge.Course).ToArray();
    public string CourseDescription(string course) => course switch
    {
        "Eingabe und Ausgabe" => "Wie Eingaben Datenbank, Browser und Dateisystem sicher erreichen.",
        "Identität und Zugriffe" => "Wie Identität, Sitzungen und Objektbesitz auf dem Server zusammenarbeiten.",
        "Sichere Architektur" => "Wie sichere Grenzen, Schlüssel und Konfiguration Risiken früh begrenzen.",
        _ => "Wie Inventar, Herkunft und Protokolle Sicherheit im Betrieb nachvollziehbar machen."
    };

    public async Task OnGetAsync(CancellationToken cancellationToken)
    {
        Challenges = registry.All;
        CompletedIds = await progressService.CompletedIdsAsync(cancellationToken);
        StartedIds = await progressService.StartedIdsAsync(cancellationToken);
        Penalties = await progressService.PenaltiesAsync(cancellationToken);
    }
}

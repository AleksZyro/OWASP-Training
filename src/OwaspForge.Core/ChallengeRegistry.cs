using OwaspForge.Core.Challenges;

namespace OwaspForge.Core;

public sealed class ChallengeRegistry
{
    public IReadOnlyList<ChallengeDefinition> All => ChallengeCatalog.German;

    public ChallengeDefinition? Find(string id) => All.SingleOrDefault(challenge => StringComparer.Ordinal.Equals(challenge.Id, id));
}

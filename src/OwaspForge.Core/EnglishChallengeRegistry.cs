using OwaspForge.Core.Challenges;

namespace OwaspForge.Core;

public sealed class EnglishChallengeRegistry
{
    public IReadOnlyList<ChallengeDefinition> All => ChallengeCatalog.English;

    public ChallengeDefinition? Find(string id) => All.SingleOrDefault(challenge => StringComparer.Ordinal.Equals(challenge.Id, id));
}

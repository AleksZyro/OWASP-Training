namespace OwaspForge.Core;

public sealed class ChallengeValidator(ChallengeRegistry registry)
{
    public bool IsSolved(string challengeId, string? selectedOption) =>
        registry.Find(challengeId) is { } challenge && StringComparer.Ordinal.Equals(challenge.CorrectOption, selectedOption);
}

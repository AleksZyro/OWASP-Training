namespace OwaspForge.Core;

public sealed class ChallengeValidator(ChallengeRegistry registry)
{
    public bool IsSolved(string challengeId, string? selectedOption) =>
        registry.Find(challengeId) is { } challenge && StringComparer.Ordinal.Equals(challenge.CorrectOption, selectedOption);

    public bool IsSolved(string challengeId, IReadOnlyList<string?> answers) =>
        registry.Find(challengeId) is { } challenge
        && answers.Count == challenge.Questions.Count
        && answers.All(answer => StringComparer.Ordinal.Equals(challenge.CorrectOption, answer));

    public int CountIncorrect(string challengeId, IReadOnlyList<string?> answers) =>
        registry.Find(challengeId) is not { } challenge
            ? answers.Count
            : Enumerable.Range(0, challenge.Questions.Count).Count(index => index >= answers.Count || !StringComparer.Ordinal.Equals(challenge.CorrectOption, answers[index]));
}

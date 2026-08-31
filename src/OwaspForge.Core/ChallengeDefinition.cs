namespace OwaspForge.Core;

public sealed record ChallengeDefinition(
    string Id,
    string Title,
    string Category,
    string Difficulty,
    string Duration,
    int Points,
    string LearningGoal,
    string Explanation,
    string Task,
    string Impact,
    string ExpectedAfterFix,
    string OfficialUrl,
    IReadOnlyList<string> Hints,
    IReadOnlyList<ChallengeOption> Options,
    string CorrectOption);

public sealed record ChallengeOption(string Value, string Label);

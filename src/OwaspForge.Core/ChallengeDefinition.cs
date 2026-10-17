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
    string CorrectOption)
{
    public string CourseId { get; init; } = string.Empty;
    public int Order { get; init; }
    public string? DemoId { get; init; }
    public string Course { get; init; } = string.Empty;
    public IReadOnlyList<ChallengeQuestion> Questions { get; init; } = [];
    public string SolutionExplanation { get; init; } = string.Empty;
    public LearningExample Example { get; init; } = LearningExample.Empty;
    public ProtocolEvidence Protocol { get; init; } = ProtocolEvidence.Empty;
}

public sealed record LearningExample(string Context, string RiskySnippet, string SaferSnippet, string Takeaway)
{
    public static LearningExample Empty { get; } = new(string.Empty, string.Empty, string.Empty, string.Empty);
    public IReadOnlyList<string> KeyPoints { get; init; } = [];
}

public sealed record ChallengeQuestion(string Prompt, IReadOnlyList<ChallengeOption> Options, string CorrectOption);
public sealed record ChallengeOption(string Value, string Label, string Feedback);

/// <summary>Describes the expected local HTTP decision without performing a network request.</summary>
public sealed record ProtocolEvidence(string Method, string Status, string SecurityHeaders, string AuthenticationState, string AuthorizationResult)
{
    public static ProtocolEvidence Empty { get; } = new(string.Empty, string.Empty, string.Empty, string.Empty, string.Empty);
    public bool IsAvailable => !string.IsNullOrWhiteSpace(Method);
}

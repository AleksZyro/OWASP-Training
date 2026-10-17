namespace OwaspForge.Core.Challenges;

public sealed record ChallengeMetadata(
    string Id, string CourseId, int Order, string Category, string Difficulty,
    int DurationMinutes, int Points, string OfficialUrl, string? DemoId);

public sealed record AnswerRule(IReadOnlyList<string> OptionValues, string CorrectOption);
public sealed record OptionText(string Label, string Feedback);
public sealed record QuestionText(string Prompt, IReadOnlyDictionary<string, OptionText> Options);

public sealed record ChallengeText(
    string Title, string LearningGoal, string Explanation, string Task, string Impact,
    string ExpectedAfterFix, IReadOnlyList<string> Hints, string SolutionExplanation,
    LearningExample Example, ProtocolEvidence Protocol, IReadOnlyList<QuestionText> Questions);

public sealed record ChallengeModule(
    ChallengeMetadata Metadata, IReadOnlyList<AnswerRule> Rules,
    ChallengeText German, ChallengeText English);

using System.Text.Json;

namespace OwaspForge.Core.Challenges;

/// <summary>Explicitly registered, embedded content. No directory discovery or executable plugins.</summary>
public static class ChallengeCatalog
{
    public static IReadOnlyList<ChallengeModule> Modules { get; } = LoadModules();
    public static IReadOnlyList<ChallengeDefinition> German { get; } = Array.AsReadOnly(Modules.Select(module => Localize(module, false)).ToArray());
    public static IReadOnlyList<ChallengeDefinition> English { get; } = Array.AsReadOnly(Modules.Select(module => Localize(module, true)).ToArray());

    private static IReadOnlyList<ChallengeModule> LoadModules()
    {
        // Course order and stable IDs are deliberately explicit and reviewable.
        ChallengeModule[] modules = [
            Load("sql-injection"), Load("xss"), Load("file-upload"),
            Load("authentication"), Load("session-csrf"), Load("idor"),
            Load("ssrf"), Load("cryptography-secrets"), Load("security-misconfiguration"),
            Load("dependencies-sbom"), Load("supply-chain-integrity"), Load("logging-monitoring")
        ];
        Validate(modules);
        return Array.AsReadOnly(modules.OrderBy(module => module.Metadata.Order).ToArray());
    }

    private static ChallengeModule Load(string id)
    {
        using var resource = typeof(ChallengeCatalog).Assembly.GetManifestResourceStream($"{id}.json")
            ?? throw new InvalidOperationException($"Missing registered challenge resource: {id}.");
        var module = JsonSerializer.Deserialize<ChallengeModule>(resource)
            ?? throw new InvalidOperationException($"Empty challenge resource: {id}.");
        if (!StringComparer.Ordinal.Equals(module.Metadata.Id, id))
        {
            throw new InvalidOperationException($"Challenge resource ID does not match its registration: {id}.");
        }
        return module;
    }

    public static void Validate(IReadOnlyList<ChallengeModule> modules)
    {
        if (modules.Count == 0
            || modules.Select(module => module.Metadata.Id).Distinct(StringComparer.Ordinal).Count() != modules.Count
            || modules.Select(module => module.Metadata.Order).Distinct().Count() != modules.Count)
        {
            throw new InvalidOperationException("Challenge IDs and positions must be present and unique.");
        }

        foreach (var module in modules)
        {
            var metadata = module.Metadata;
            RequireText(metadata.Id, metadata.CourseId, metadata.Category, metadata.OfficialUrl);
            if (metadata.Order < 1 || metadata.DurationMinutes < 1 || metadata.Points < 0
                || metadata.Difficulty is not ("easy" or "medium")
                || !CourseCatalog.All.Any(course => course.Id == metadata.CourseId)
                || (metadata.DemoId is not null && DemoCatalog.Find(metadata.DemoId) is null)
                || module.Rules.Count == 0)
            {
                throw new InvalidOperationException($"Invalid challenge metadata: {metadata.Id}.");
            }
            ValidateText(module.German, module.Rules);
            ValidateText(module.English, module.Rules);
        }
    }

    private static void ValidateText(ChallengeText text, IReadOnlyList<AnswerRule> rules)
    {
        RequireText(text.Title, text.LearningGoal, text.Explanation, text.Task, text.Impact,
            text.ExpectedAfterFix, text.SolutionExplanation, text.Example.Context,
            text.Example.RiskySnippet, text.Example.SaferSnippet, text.Example.Takeaway);
        if (text.Hints.Count != 2 || text.Questions.Count != rules.Count || text.Example.KeyPoints.Count == 0)
        {
            throw new InvalidOperationException("Each translation needs matching questions, two hints and lesson key points.");
        }
        RequireText(text.Hints.Concat(text.Example.KeyPoints).ToArray());
        for (var index = 0; index < rules.Count; index++)
        {
            var rule = rules[index];
            var question = text.Questions[index];
            RequireText(question.Prompt, rule.CorrectOption);
            RequireText(rule.OptionValues.ToArray());
            if (rule.OptionValues.Count < 2
                || rule.OptionValues.Distinct(StringComparer.Ordinal).Count() != rule.OptionValues.Count
                || !rule.OptionValues.Contains(rule.CorrectOption, StringComparer.Ordinal)
                || !rule.OptionValues.ToHashSet(StringComparer.Ordinal).SetEquals(question.Options.Keys))
            {
                throw new InvalidOperationException("Translations must match the shared answer structure exactly.");
            }
            foreach (var (key, option) in question.Options)
            {
                RequireText(option.Label);
                if (key != rule.CorrectOption) RequireText(option.Feedback);
            }
        }
    }

    private static void RequireText(params string[] values)
    {
        if (values.Any(string.IsNullOrWhiteSpace))
        {
            throw new InvalidOperationException("Challenge content must not contain empty required text.");
        }
    }

    private static ChallengeDefinition Localize(ChallengeModule module, bool english)
    {
        var metadata = module.Metadata;
        var text = english ? module.English : module.German;
        var course = CourseCatalog.All.Single(course => course.Id == metadata.CourseId);
        var questions = module.Rules.Select((rule, index) => new ChallengeQuestion(
            text.Questions[index].Prompt,
            Array.AsReadOnly(rule.OptionValues.Select(value => new ChallengeOption(value,
                text.Questions[index].Options[value].Label, text.Questions[index].Options[value].Feedback)).ToArray()),
            rule.CorrectOption)).ToArray();
        var difficulty = metadata.Difficulty == "easy" ? (english ? "Easy" : "Einfach") : (english ? "Medium" : "Mittel");
        return new(metadata.Id, text.Title, metadata.Category, difficulty,
            $"{metadata.DurationMinutes} {(english ? "min" : "Min.")}", metadata.Points,
            text.LearningGoal, text.Explanation, text.Task, text.Impact, text.ExpectedAfterFix,
            metadata.OfficialUrl, text.Hints, questions[0].Options, questions[0].CorrectOption)
        {
            CourseId = metadata.CourseId,
            Order = metadata.Order,
            DemoId = metadata.DemoId,
            Course = english ? course.EnglishTitle : course.GermanTitle,
            Questions = Array.AsReadOnly(questions),
            SolutionExplanation = text.SolutionExplanation,
            Example = text.Example,
            Protocol = text.Protocol
        };
    }
}

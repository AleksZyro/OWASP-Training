using OwaspForge.Core;

namespace OwaspForge.Tests;

public sealed class ChallengeValidatorTests
{
    private readonly ChallengeRegistry registry = new();

    [Fact]
    public void Registry_contains_twelve_stations_in_four_courses()
    {
        Assert.Equal(12, registry.All.Count);
        Assert.Equal(4, registry.All.Select(challenge => challenge.Course).Distinct().Count());
        Assert.All(registry.All.GroupBy(challenge => challenge.Course), course => Assert.Equal(3, course.Count()));
    }

    [Fact]
    public void English_registry_keeps_the_same_twelve_station_question_structure()
    {
        var english = new EnglishChallengeRegistry();

        Assert.Equal(registry.All.Select(challenge => challenge.Id), english.All.Select(challenge => challenge.Id));
        Assert.All(english.All, challenge =>
        {
            Assert.Equal(6, challenge.Questions.Count);
            Assert.All(challenge.Questions, question => Assert.All(question.Options, option => Assert.DoesNotContain("Sichere", option.Label, StringComparison.Ordinal)));
        });
    }

    [Theory]
    [InlineData("sql-injection", "parameter")]
    [InlineData("xss", "encode")]
    [InlineData("idor", "owner")]
    [InlineData("authentication", "secure-auth")]
    [InlineData("file-upload", "upload-policy")]
    [InlineData("ssrf", "allowlist")]
    public void Validator_accepts_only_the_server_defined_safe_option(string id, string answer) => Assert.True(new ChallengeValidator(registry).IsSolved(id, answer));

    [Fact]
    public void Validator_rejects_unknown_or_insecure_options()
    {
        var validator = new ChallengeValidator(registry);
        Assert.False(validator.IsSolved("ssrf", "blocklist"));
        Assert.False(validator.IsSolved("unknown", "anything"));
    }

    [Fact]
    public void Each_station_requires_all_six_questions()
    {
        var validator = new ChallengeValidator(registry);
        var answers = registry.Find("sql-injection")!.Questions.Select(question => question.CorrectOption).Cast<string?>().ToArray();

        Assert.True(validator.IsSolved("sql-injection", answers));
        Assert.False(validator.IsSolved("sql-injection", answers[..5]));
        answers[1] = "concat";
        Assert.Equal(1, validator.CountIncorrect("sql-injection", answers));
    }

    [Fact]
    public void Questions_have_distinct_options_and_a_course_specific_solution()
    {
        Assert.All(registry.All, challenge =>
        {
            Assert.Equal(6, challenge.Questions.Count);
            Assert.All(challenge.Questions, question => Assert.Equal(3, question.Options.Count));
            Assert.False(string.IsNullOrWhiteSpace(challenge.SolutionExplanation));
            Assert.True(challenge.Questions.Select(question => string.Join('|', question.Options.Select(option => option.Label))).Distinct().Count() > 1);
        });
    }

    [Fact]
    public void Every_insecure_option_has_a_learning_explanation()
    {
        var insecureOptions = registry.All.SelectMany(challenge => challenge.Questions)
            .SelectMany(question => question.Options.Where(option => option.Value != question.CorrectOption));

        Assert.All(insecureOptions, option => Assert.False(string.IsNullOrWhiteSpace(option.Feedback)));
    }
}

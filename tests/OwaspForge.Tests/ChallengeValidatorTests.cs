using OwaspForge.Core;

namespace OwaspForge.Tests;

public sealed class ChallengeValidatorTests
{
    private readonly ChallengeRegistry registry = new();

    [Fact]
    public void Registry_contains_exactly_the_six_mvp_challenges() => Assert.Equal(6, registry.All.Count);

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
    public void Each_course_requires_all_three_questions()
    {
        var validator = new ChallengeValidator(registry);

        Assert.True(validator.IsSolved("sql-injection", ["parameter", "parameter", "parameter"]));
        Assert.False(validator.IsSolved("sql-injection", ["parameter", "parameter"]));
        Assert.Equal(1, validator.CountIncorrect("sql-injection", ["parameter", "concat", "parameter"]));
    }

    [Fact]
    public void Questions_have_distinct_options_and_a_course_specific_solution()
    {
        Assert.All(registry.All, challenge =>
        {
            Assert.Equal(3, challenge.Questions.Count);
            Assert.All(challenge.Questions, question => Assert.Equal(3, question.Options.Count));
            Assert.False(string.IsNullOrWhiteSpace(challenge.SolutionExplanation));
            Assert.True(challenge.Questions.Select(question => string.Join('|', question.Options.Select(option => option.Label))).Distinct().Count() > 1);
        });
    }

    [Fact]
    public void Every_insecure_option_has_a_learning_explanation()
    {
        var insecureOptions = registry.All
            .SelectMany(challenge => challenge.Options)
            .Where(option => option.Value != "parameter" && option.Value != "encode" && option.Value != "owner" && option.Value != "secure-auth" && option.Value != "upload-policy" && option.Value != "allowlist");

        Assert.All(insecureOptions, option => Assert.False(string.IsNullOrWhiteSpace(option.Feedback)));
    }
}

using OwaspForge.Core;
using OwaspForge.Core.Challenges;

namespace OwaspForge.Tests;

public sealed class ChallengeCatalogTests
{
    [Fact]
    public void Stable_progress_ids_and_course_order_are_preserved()
    {
        string[] expectedIds = [
            "sql-injection", "xss", "file-upload", "authentication", "session-csrf", "idor",
            "ssrf", "cryptography-secrets", "security-misconfiguration",
            "dependencies-sbom", "supply-chain-integrity", "logging-monitoring"
        ];
        Assert.Equal(expectedIds, new ChallengeRegistry().All.Select(challenge => challenge.Id));
        Assert.Equal(Enumerable.Range(1, 12), ChallengeCatalog.Modules.Select(module => module.Metadata.Order));
        Assert.Equal(CourseCatalog.All.Select(course => course.Id),
            ChallengeCatalog.Modules.Select(module => module.Metadata.CourseId).Distinct());
    }

    [Fact]
    public void Both_languages_share_metadata_and_every_answer_rule()
    {
        var german = new ChallengeRegistry().All;
        var english = new EnglishChallengeRegistry().All;
        Assert.Equal(german.Select(challenge => challenge.Id), english.Select(challenge => challenge.Id));
        foreach (var (de, en) in german.Zip(english))
        {
            Assert.Equal((de.Id, de.CourseId, de.Order, de.DemoId, de.Category, de.Points, de.OfficialUrl),
                (en.Id, en.CourseId, en.Order, en.DemoId, en.Category, en.Points, en.OfficialUrl));
            Assert.Equal(de.Questions.Count, en.Questions.Count);
            Assert.Equal(de.Questions.Select(question => question.CorrectOption), en.Questions.Select(question => question.CorrectOption));
            foreach (var (deQuestion, enQuestion) in de.Questions.Zip(en.Questions))
            {
                Assert.Equal(deQuestion.Options.Select(option => option.Value), enQuestion.Options.Select(option => option.Value));
            }
            Assert.True(new ChallengeValidator(new ChallengeRegistry()).IsSolved(en.Id, en.Questions.Select(question => (string?)question.CorrectOption).ToArray()));
        }
    }

    [Fact]
    public void Lessons_in_both_languages_are_complete_and_question_specific()
    {
        foreach (var challenge in ChallengeCatalog.German.Concat(ChallengeCatalog.English))
        {
            Assert.False(string.IsNullOrWhiteSpace(challenge.Title));
            Assert.All(challenge.Hints, hint => Assert.False(string.IsNullOrWhiteSpace(hint)));
            Assert.All(challenge.Questions, question =>
            {
                Assert.False(string.IsNullOrWhiteSpace(question.Prompt));
                Assert.All(question.Options, option => Assert.False(string.IsNullOrWhiteSpace(option.Label)));
                Assert.Single(question.Options, option => option.Value == question.CorrectOption);
            });
            Assert.True(challenge.Questions.Select(question => string.Join('|', question.Options.Select(option => option.Label))).Distinct().Count() > 1);
        }
    }

    [Fact]
    public void Demo_references_are_explicit_known_and_loopback_only()
    {
        Assert.Equal(6, ChallengeCatalog.Modules.Count(module => module.Metadata.DemoId is not null));
        Assert.Equal(6, DemoCatalog.All.Select(demo => demo.Port).Distinct().Count());
        foreach (var module in ChallengeCatalog.Modules.Where(module => module.Metadata.DemoId is not null))
        {
            var demo = DemoCatalog.Find(module.Metadata.DemoId);
            Assert.NotNull(demo);
            Assert.Equal(module.Metadata.Id, demo.Id);
            Assert.True(new Uri(demo.Url).IsLoopback);
        }
    }

    [Fact]
    public void Validation_rejects_duplicate_order_and_unknown_demo()
    {
        var modules = ChallengeCatalog.Modules.ToArray();
        modules[1] = modules[1] with { Metadata = modules[1].Metadata with { Order = modules[0].Metadata.Order } };
        Assert.Throws<InvalidOperationException>(() => ChallengeCatalog.Validate(modules));
        modules = ChallengeCatalog.Modules.ToArray();
        modules[0] = modules[0] with { Metadata = modules[0].Metadata with { DemoId = "unknown-demo" } };
        Assert.Throws<InvalidOperationException>(() => ChallengeCatalog.Validate(modules));
    }

    [Fact]
    public void Validation_rejects_missing_translation_text_or_answer_keys()
    {
        var modules = ChallengeCatalog.Modules.ToArray();
        modules[0] = modules[0] with { English = modules[0].English with { Title = "" } };
        Assert.Throws<InvalidOperationException>(() => ChallengeCatalog.Validate(modules));
        modules = ChallengeCatalog.Modules.ToArray();
        var questions = modules[0].English.Questions.ToArray();
        questions[0] = questions[0] with { Options = new Dictionary<string, OptionText>() };
        modules[0] = modules[0] with { English = modules[0].English with { Questions = questions } };
        Assert.Throws<InvalidOperationException>(() => ChallengeCatalog.Validate(modules));
    }
}

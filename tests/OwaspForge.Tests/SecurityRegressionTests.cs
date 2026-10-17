using OwaspForge.Core;

namespace OwaspForge.Tests;

public sealed class SecurityRegressionTests
{
    [Fact]
    public void Sql_input_cannot_change_the_parameterized_command_structure()
    {
        var command = DefensiveExamples.BuildProductSearch("' OR 1=1 --");

        Assert.Equal(DefensiveExamples.ProductSearchCommand, command.CommandText);
        Assert.Equal("@name", command.ParameterName);
        Assert.Equal("' OR 1=1 --", command.ParameterValue);
    }

    [Fact]
    public void Untrusted_markup_is_encoded_as_text()
    {
        var encoded = DefensiveExamples.EncodeHtmlText("<img src=x onerror=alert(1)>");

        Assert.Equal("&lt;img src=x onerror=alert(1)&gt;", encoded);
    }

    [Fact]
    public void Resource_owner_can_read_only_their_own_resource()
    {
        Assert.True(DefensiveExamples.CanReadResource("learner-a", "learner-a"));
        Assert.False(DefensiveExamples.CanReadResource("learner-a", "learner-b"));
        Assert.False(DefensiveExamples.CanReadResource(string.Empty, "learner-a"));
    }

    [Fact]
    public void Only_fixed_local_mock_keys_are_resolved()
    {
        Assert.True(DefensiveExamples.TryResolveLocalMock("catalogue", out var resource));
        Assert.Equal("catalogued-item", resource!.Payload);
        Assert.False(DefensiveExamples.TryResolveLocalMock("https://example.invalid", out _));
    }
}

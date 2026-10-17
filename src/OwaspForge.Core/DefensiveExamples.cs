using System.Net;

namespace OwaspForge.Core;

/// <summary>Small, side-effect-free secure coding examples used by the learning material and regression tests.</summary>
public static class DefensiveExamples
{
    public const string ProductSearchCommand = "SELECT Id, Name FROM Products WHERE Name = @name";

    public static ParameterizedCommand BuildProductSearch(string searchTerm) =>
        new(ProductSearchCommand, "@name", searchTerm ?? string.Empty);

    public static string EncodeHtmlText(string value) => WebUtility.HtmlEncode(value ?? string.Empty);

    public static bool CanReadResource(string currentUserId, string ownerUserId) =>
        !string.IsNullOrWhiteSpace(currentUserId)
        && StringComparer.Ordinal.Equals(currentUserId, ownerUserId);

    public static bool TryResolveLocalMock(string key, out LocalMockResource? resource)
    {
        resource = StringComparer.Ordinal.Equals(key, "catalogue")
            ? new LocalMockResource("catalogue", "catalogued-item")
            : null;
        return resource is not null;
    }
}

public sealed record ParameterizedCommand(string CommandText, string ParameterName, string ParameterValue);
public sealed record LocalMockResource(string Key, string Payload);

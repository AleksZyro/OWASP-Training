namespace OwaspForge.Core.Challenges;

public sealed record CourseDefinition(string Id, string GermanTitle, string EnglishTitle);

public static class CourseCatalog
{
    public static IReadOnlyList<CourseDefinition> All { get; } = Array.AsReadOnly<CourseDefinition>([
        new("input-output", "Eingabe und Ausgabe", "Input and Output"),
        new("identity-access", "Identität und Zugriffe", "Identity and Access"),
        new("secure-architecture", "Sichere Architektur", "Secure Architecture"),
        new("supply-chain", "Betrieb und Lieferkette", "Operations and Supply Chain")
    ]);
}

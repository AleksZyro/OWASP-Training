namespace OwaspForge.Core.Challenges;

public sealed record DemoDefinition(string Id, int Port)
{
    public string Url => $"http://127.0.0.1:{Port}";
}

public static class DemoCatalog
{
    public static IReadOnlyList<DemoDefinition> All { get; } = Array.AsReadOnly<DemoDefinition>([
        new("sql-injection", 5101), new("xss", 5102), new("idor", 5103),
        new("authentication", 5104), new("file-upload", 5105), new("ssrf", 5106)
    ]);

    public static DemoDefinition? Find(string? id) => All.SingleOrDefault(demo => StringComparer.Ordinal.Equals(demo.Id, id));
}

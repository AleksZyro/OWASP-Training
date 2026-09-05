namespace OwaspForge.Web.Data;

public sealed class ProgressRecord
{
    public int Id { get; set; }
    public required string ChallengeId { get; set; }
    public bool IsCompleted { get; set; }
    public DateTimeOffset? CompletedAt { get; set; }
}

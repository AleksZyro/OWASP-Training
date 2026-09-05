using Microsoft.EntityFrameworkCore;
using OwaspForge.Core;

namespace OwaspForge.Web.Data;

public sealed class ProgressService(ForgeDbContext database)
{
    public async Task<IReadOnlySet<string>> StartedIdsAsync(CancellationToken cancellationToken) =>
        (await database.Progress.Select(record => record.ChallengeId).ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);

    public async Task<IReadOnlySet<string>> CompletedIdsAsync(CancellationToken cancellationToken) =>
        (await database.Progress.Where(record => record.IsCompleted).Select(record => record.ChallengeId).ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);

    public async Task MarkStartedAsync(ChallengeDefinition challenge, CancellationToken cancellationToken)
    {
        var record = await database.Progress.SingleOrDefaultAsync(item => item.ChallengeId == challenge.Id, cancellationToken);
        if (record is not null)
        {
            return;
        }

        database.Progress.Add(new ProgressRecord { ChallengeId = challenge.Id, IsCompleted = false });
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task MarkCompletedAsync(ChallengeDefinition challenge, CancellationToken cancellationToken)
    {
        var record = await database.Progress.SingleOrDefaultAsync(item => item.ChallengeId == challenge.Id, cancellationToken);
        if (record is null)
        {
            database.Progress.Add(new ProgressRecord { ChallengeId = challenge.Id, IsCompleted = true, CompletedAt = DateTimeOffset.UtcNow });
        }
        else
        {
            record.IsCompleted = true;
            record.CompletedAt ??= DateTimeOffset.UtcNow;
        }

        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task ResetAsync(string challengeId, CancellationToken cancellationToken)
    {
        var record = await database.Progress.SingleOrDefaultAsync(item => item.ChallengeId == challengeId, cancellationToken);
        if (record is not null)
        {
            database.Progress.Remove(record);
            await database.SaveChangesAsync(cancellationToken);
        }
    }
}

using Microsoft.EntityFrameworkCore;
using OwaspForge.Core;

namespace OwaspForge.Web.Data;

public sealed class ProgressService(ForgeDbContext database)
{
    public async Task<IReadOnlySet<string>> StartedIdsAsync(CancellationToken cancellationToken) =>
        (await database.Progress.Select(record => record.ChallengeId).ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);

    public async Task<IReadOnlySet<string>> CompletedIdsAsync(CancellationToken cancellationToken) =>
        (await database.Progress.Where(record => record.IsCompleted).Select(record => record.ChallengeId).ToListAsync(cancellationToken)).ToHashSet(StringComparer.Ordinal);

    public async Task<IReadOnlyDictionary<string, int>> PenaltiesAsync(CancellationToken cancellationToken) =>
        await database.Progress.ToDictionaryAsync(record => record.ChallengeId, record => record.PenaltyPoints, StringComparer.Ordinal, cancellationToken);

    public Task<ProgressRecord?> FindAsync(string challengeId, CancellationToken cancellationToken) =>
        database.Progress.SingleOrDefaultAsync(record => record.ChallengeId == challengeId, cancellationToken);

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

    public async Task RecordIncorrectAttemptAsync(string challengeId, int penalty, CancellationToken cancellationToken)
    {
        var record = await database.Progress.SingleOrDefaultAsync(item => item.ChallengeId == challengeId, cancellationToken);
        if (record is null)
        {
            record = new ProgressRecord { ChallengeId = challengeId, IsCompleted = false };
            database.Progress.Add(record);
        }

        record.Attempts++;
        record.PenaltyPoints += Math.Max(0, penalty);
        await database.SaveChangesAsync(cancellationToken);
    }

    public async Task<bool> UseHintAsync(string challengeId, int level, CancellationToken cancellationToken)
    {
        var record = await database.Progress.SingleOrDefaultAsync(item => item.ChallengeId == challengeId, cancellationToken);
        if (record is null)
        {
            record = new ProgressRecord { ChallengeId = challengeId, IsCompleted = false };
            database.Progress.Add(record);
        }

        if (level != record.HintsUsed + 1 || level > 2)
        {
            return false;
        }

        record.HintsUsed++;
        record.PenaltyPoints += 10;
        await database.SaveChangesAsync(cancellationToken);
        return true;
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

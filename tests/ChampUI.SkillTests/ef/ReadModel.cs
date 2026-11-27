using Microsoft.EntityFrameworkCore;

namespace SkillEvaluation;

public sealed class Appointment
{
    public int Id { get; set; }
    public int TenantId { get; set; }
    public DateOnly Day { get; set; }
    public required string Service { get; set; }
}

public sealed class SalonContext(DbContextOptions<SalonContext> options) : DbContext(options)
{
    public DbSet<Appointment> Appointments => Set<Appointment>();
}

public sealed record VisitSummary(int Id, string Service);

public sealed class VisitReader(IDbContextFactory<SalonContext> factory)
{
    // The host supplies the tenant from its authorized server-side identity.
    public async Task<VisitSummary[]> ReadAsync(int authorizedTenant, DateOnly day, CancellationToken token)
    {
        await using var db = await factory.CreateDbContextAsync(token);
        return await db.Appointments.AsNoTracking()
            .Where(v => v.TenantId == authorizedTenant && v.Day == day)
            .OrderBy(v => v.Id)
            .Select(v => new VisitSummary(v.Id, v.Service))
            .ToArrayAsync(token);
    }
}

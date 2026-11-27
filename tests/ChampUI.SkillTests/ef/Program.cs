using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SkillEvaluation;

var services = new ServiceCollection();
services.AddDbContextFactory<SalonContext>(options => options.UseInMemoryDatabase("evaluation"));
services.AddScoped<VisitReader>();
await using var provider = services.BuildServiceProvider(validateScopes: true);
await using var scope = provider.CreateAsyncScope();
var factory = scope.ServiceProvider.GetRequiredService<IDbContextFactory<SalonContext>>();
var day = new DateOnly(2026, 11, 27);
await using (var db = await factory.CreateDbContextAsync())
{
    db.AddRange(
        new Appointment { Id = 4, Day = day, TenantId = 1, Service = "Color" },
        new Appointment { Id = 2, Day = day, TenantId = 1, Service = "Cut" },
        new Appointment { Id = 1, Day = day, TenantId = 2, Service = "Other tenant" },
        new Appointment { Id = 3, Day = day.AddDays(1), TenantId = 1, Service = "Other day" });
    await db.SaveChangesAsync();
}
var reader = scope.ServiceProvider.GetRequiredService<VisitReader>();
var visits = await reader.ReadAsync(1, day, CancellationToken.None);
if (!visits.SequenceEqual([new VisitSummary(2, "Cut"), new VisitSummary(4, "Color")]))
    throw new InvalidOperationException("Unexpected tenant, date, order or projection.");

try
{
    await reader.ReadAsync(1, day, new CancellationToken(true));
    throw new InvalidOperationException("The cancelled query completed.");
}
catch (OperationCanceledException)
{
    Console.WriteLine("EF Core fixture passed tenant/date isolation, sorted projection and cancellation.");
}

using System.Net.Http.Json;

namespace SkillDashboard.Services;

public sealed record ServiceRate(string Name, decimal Amount);

public sealed class ServiceRatesClient(HttpClient http)
{
    public async Task<ServiceRate[]> ReadAsync(string location, CancellationToken token) =>
        await http.GetFromJsonAsync<ServiceRate[]>($"skill-api/rates/{Uri.EscapeDataString(location)}", token)
            ?? throw new InvalidDataException("Expected an array of service rates.");
}

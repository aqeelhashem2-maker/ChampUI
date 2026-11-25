using Microsoft.Extensions.DependencyInjection;

namespace ChampUI.Services;

public static class ChampUiServiceCollectionExtensions
{
    /// <summary>Registers ChampUI services. Call once in Program.cs.</summary>
    public static IServiceCollection AddChampUI(this IServiceCollection services, Action<ChampUiOptions>? configure = null)
    {
        var options = new ChampUiOptions();
        configure?.Invoke(options);
        services.AddSingleton(options);
        services.AddScoped<UiThemeState>();
        services.AddScoped<ToastService>();
        return services;
    }
}

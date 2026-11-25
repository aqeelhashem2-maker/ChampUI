using ChampUI.Models;

namespace ChampUI.Services;

/// <summary>Holds the current light/dark mode for a user session (scoped service).</summary>
public sealed class UiThemeState
{
    public UiThemeState(ChampUiOptions options) => Mode = options.DefaultMode;

    public UiThemeMode Mode { get; private set; }
    public event Action? Changed;

    public void Set(UiThemeMode mode)
    {
        if (mode == Mode) return;
        Mode = mode;
        Changed?.Invoke();
    }

    public void Toggle() => Set(Mode == UiThemeMode.Dark ? UiThemeMode.Light : UiThemeMode.Dark);
}

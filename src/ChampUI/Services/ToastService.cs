using ChampUI.Models;

namespace ChampUI.Services;

/// <summary>Raises transient notifications rendered by <c>UiToastHost</c> (scoped service).</summary>
public sealed class ToastService
{
    private readonly List<ToastMessage> _toasts = new();
    private readonly object _gate = new();

    public event Action? Changed;

    public IReadOnlyList<ToastMessage> Toasts { get { lock (_gate) return _toasts.ToList(); } }

    public void Show(string text, ToastKind kind = ToastKind.Info, string? title = null, TimeSpan? duration = null)
    {
        var toast = new ToastMessage { Text = text, Kind = kind, Title = title, Duration = duration ?? TimeSpan.FromSeconds(4) };
        lock (_gate) _toasts.Add(toast);
        Changed?.Invoke();
        _ = ExpireAsync(toast);
    }

    public void Success(string text, string? title = null) => Show(text, ToastKind.Success, title);
    public void Info(string text, string? title = null) => Show(text, ToastKind.Info, title);
    public void Warning(string text, string? title = null) => Show(text, ToastKind.Warning, title);
    public void Error(string text, string? title = null) => Show(text, ToastKind.Danger, title);

    public void Dismiss(Guid id)
    {
        bool removed;
        lock (_gate) removed = _toasts.RemoveAll(t => t.Id == id) > 0;
        if (removed) Changed?.Invoke();
    }

    private async Task ExpireAsync(ToastMessage toast)
    {
        await Task.Delay(toast.Duration);
        Dismiss(toast.Id);
    }
}

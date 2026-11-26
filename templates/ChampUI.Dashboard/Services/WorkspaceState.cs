using System.Globalization;
using ChampUI.Models;
using ChampUI.Services;
using ChampUI.Starter.Models;

namespace ChampUI.Starter.Services;

public sealed record WorkspacePreferences(string Name, bool Arabic)
{
    private CultureInfo Culture => CultureInfo.GetCultureInfo(Arabic ? "ar-SA" : "en-US");

    public string Text(string english, string arabic) => Arabic ? arabic : english;
    public string Number(int value) => value.ToString("N0", Culture);
    public string Currency(decimal value) => value.ToString("C0", CultureInfo.GetCultureInfo("en-US"));
    public string Date(DateOnly date) => date.ToDateTime(TimeOnly.MinValue)
        .ToString("d MMM yyyy", CultureInfo.GetCultureInfo(Arabic ? "ar-EG" : "en-US"));
    public string Month(DateOnly date) => date.ToDateTime(TimeOnly.MinValue)
        .ToString("MMM", CultureInfo.GetCultureInfo(Arabic ? "ar-EG" : "en-US"));

    public string Status(CustomerStatus status) => status switch
    {
        CustomerStatus.Active => Text("Active", "نشط"),
        CustomerStatus.Onboarding => Text("Onboarding", "قيد الانضمام"),
        _ => Text("Inactive", "غير نشط")
    };

    public string Status(OrderStatus status) => status switch
    {
        OrderStatus.Completed => Text("Completed", "مكتمل"),
        OrderStatus.Processing => Text("Processing", "قيد المعالجة"),
        _ => Text("Pending", "بانتظار المعالجة")
    };
}

public sealed class WorkspaceState(UiThemeState theme, DashboardOptions identity)
{
    public WorkspacePreferences Current { get; private set; } = new(identity.CustomDisplayName ?? "Workspace", identity.IsArabic);

    public event Action? Changed;

    public void ToggleLanguage() => Set(Current with { Arabic = !Current.Arabic });

    public void Save(string name, bool arabic, UiThemeMode mode)
    {
        theme.Set(mode);
        Set(new WorkspacePreferences(name, arabic));
    }

    private void Set(WorkspacePreferences preferences)
    {
        if (Current == preferences)
            return;

        Current = preferences;
        Changed?.Invoke();
    }
}

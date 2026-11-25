namespace ChampUI.Models;

/// <summary>Semantic colour variants shared by buttons, badges, alerts, progress bars and more.</summary>
public enum UiVariant { Primary, Secondary, Ghost, Outline, Neutral, Success, Warning, Danger, Info }

public enum UiSize { Small, Medium, Large }

public enum UiTextDirection { Ltr, Rtl }

public enum UiThemeMode { Light, Dark }

public enum UiSortDirection { None, Ascending, Descending }

public enum UiTrend { Neutral, Up, Down }

public enum UiAlign { Start, Center, End }

/// <summary>Inline editing mode for <c>UiDataGrid</c>.</summary>
public enum UiDataGridEditMode { None, Inline }

/// <summary>Row virtualization mode for <c>UiDataGrid</c>.</summary>
public enum UiDataGridVirtualization { None, Enabled }

public static class UiEnumExtensions
{
    public static string Css(this UiVariant v) => v.ToString().ToLowerInvariant();
    public static string Css(this UiSize s) => s switch { UiSize.Small => "sm", UiSize.Large => "lg", _ => "md" };
    public static string Css(this UiAlign a) => a.ToString().ToLowerInvariant();
}

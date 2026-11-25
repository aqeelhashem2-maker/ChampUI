namespace ChampUI.Components.Display;

/// <summary>Built-in icon set (24x24 grid, stroke based). Applications may register extra icons.</summary>
public static class UiIcons
{
    private static readonly Dictionary<string, string> _paths = new(StringComparer.OrdinalIgnoreCase)
    {
        ["home"] = "<path d='M3 10.5 12 3l9 7.5'/><path d='M5 9.5V20h5v-6h4v6h5V9.5'/>",
        ["users"] = "<circle cx='9' cy='8' r='3.5'/><path d='M2.5 20c.6-3.6 3.2-5.5 6.5-5.5s5.9 1.9 6.5 5.5'/><circle cx='17.5' cy='9' r='2.5'/><path d='M17 14.3c2.6.3 4.1 2 4.5 4.7'/>",
        ["user"] = "<circle cx='12' cy='8' r='4'/><path d='M4.5 20.5c.8-4 3.6-6 7.5-6s6.7 2 7.5 6'/>",
        ["shield"] = "<path d='M12 3 4.5 6v5.5c0 4.6 3.1 8 7.5 9.5 4.4-1.5 7.5-4.9 7.5-9.5V6z'/>",
        ["shield-check"] = "<path d='M12 3 4.5 6v5.5c0 4.6 3.1 8 7.5 9.5 4.4-1.5 7.5-4.9 7.5-9.5V6z'/><path d='m8.8 12 2.4 2.4 4.2-4.6'/>",
        ["ball"] = "<circle cx='12' cy='12' r='9'/><path d='m12 8 3.8 2.8-1.4 4.4H9.6l-1.4-4.4z'/><path d='M12 8V3.2M15.8 10.8l4.4-1.4M14.4 15.2l2.7 3.7M9.6 15.2l-2.7 3.7M8.2 10.8 3.8 9.4'/>",
        ["chat"] = "<path d='M4 5h16v11H9.5L5 20v-4H4z'/><path d='M8 9.5h8M8 12.5h5'/>",
        ["card"] = "<rect x='3' y='5.5' width='18' height='13' rx='2.5'/><path d='M3 10h18M7 15h4'/>",
        ["trophy"] = "<path d='M8 4h8v5a4 4 0 0 1-8 0z'/><path d='M8 6H4.5v1.5A3.5 3.5 0 0 0 8 11M16 6h3.5v1.5A3.5 3.5 0 0 1 16 11M12 13v4M8.5 20h7M10 17h4'/>",
        ["chart"] = "<path d='M4 20V4M4 20h16'/><path d='m7.5 15 3.5-4 3 2.5 4.5-6'/>",
        ["bar-chart"] = "<path d='M5 20V11M12 20V4M19 20v-7'/>",
        ["bell"] = "<path d='M6 16.5V11a6 6 0 0 1 12 0v5.5l1.5 2h-15z'/><path d='M10 21h4'/>",
        ["settings"] = "<path d='M4 7h10M18 7h2M4 17h2M10 17h10'/><circle cx='16' cy='7' r='2'/><circle cx='8' cy='17' r='2'/>",
        ["search"] = "<circle cx='11' cy='11' r='6.5'/><path d='m16 16 4.5 4.5'/>",
        ["sun"] = "<circle cx='12' cy='12' r='4'/><path d='M12 2.5v2.5M12 19v2.5M2.5 12H5M19 12h2.5M5.3 5.3l1.8 1.8M16.9 16.9l1.8 1.8M5.3 18.7l1.8-1.8M16.9 7.1l1.8-1.8'/>",
        ["moon"] = "<path d='M20 14.5A8.5 8.5 0 0 1 9.5 4 8.5 8.5 0 1 0 20 14.5z'/>",
        ["globe"] = "<circle cx='12' cy='12' r='9'/><path d='M3 12h18M12 3c2.6 2.6 3.8 5.6 3.8 9s-1.2 6.4-3.8 9c-2.6-2.6-3.8-5.6-3.8-9S9.4 5.6 12 3z'/>",
        ["download"] = "<path d='M12 4v11M7.5 11 12 15.5 16.5 11M5 20h14'/>",
        ["plus"] = "<path d='M12 5v14M5 12h14'/>",
        ["minus"] = "<path d='M5 12h14'/>",
        ["x"] = "<path d='m6 6 12 12M18 6 6 18'/>",
        ["check"] = "<path d='m5 12.5 4.5 4.5L19 7.5'/>",
        ["chevron-down"] = "<path d='m6 9 6 6 6-6'/>",
        ["chevron-up"] = "<path d='m6 15 6-6 6 6'/>",
        ["chevron-left"] = "<path d='m15 6-6 6 6 6'/>",
        ["chevron-right"] = "<path d='m9 6 6 6-6 6'/>",
        ["arrow-up"] = "<path d='M12 19V5M6 11l6-6 6 6'/>",
        ["arrow-down"] = "<path d='M12 5v14M6 13l6 6 6-6'/>",
        ["arrow-up-down"] = "<path d='M8 4v16M4.5 7.5 8 4l3.5 3.5M16 20V4M12.5 16.5 16 20l3.5-3.5'/>",
        ["menu"] = "<path d='M4 7h16M4 12h16M4 17h16'/>",
        ["eye"] = "<path d='M2.5 12S6 5.5 12 5.5 21.5 12 21.5 12 18 18.5 12 18.5 2.5 12 2.5 12z'/><circle cx='12' cy='12' r='2.8'/>",
        ["activity"] = "<path d='M3 12h4l3-7 4 14 3-7h4'/>",
        ["star"] = "<path d='m12 3.5 2.6 5.4 5.9.8-4.3 4.1 1 5.9L12 16.9l-5.2 2.8 1-5.9-4.3-4.1 5.9-.8z'/>",
        ["flag"] = "<path d='M5 21V4M5 4.5h12l-2 4 2 4H5'/>",
        ["info"] = "<circle cx='12' cy='12' r='9'/><path d='M12 11v5M12 7.8h.01'/>",
        ["alert"] = "<path d='M12 3.5 2.5 20h19z'/><path d='M12 10v4.5M12 17.2h.01'/>",
        ["check-circle"] = "<circle cx='12' cy='12' r='9'/><path d='m8.2 12.3 2.6 2.6 5-5.4'/>",
        ["x-circle"] = "<circle cx='12' cy='12' r='9'/><path d='m9 9 6 6M15 9l-6 6'/>",
        ["calendar"] = "<rect x='3.5' y='5' width='17' height='15.5' rx='2.5'/><path d='M3.5 10h17M8 3v4M16 3v4'/>",
        ["dots"] = "<circle cx='5.5' cy='12' r='1.4'/><circle cx='12' cy='12' r='1.4'/><circle cx='18.5' cy='12' r='1.4'/>",
        ["filter"] = "<path d='M3.5 5h17l-6.5 7.5V19l-4 1.5v-8z'/>",
        ["inbox"] = "<path d='M3.5 13 6 5h12l2.5 8v6h-17z'/><path d='M3.5 13h5l1 2.5h5l1-2.5h5'/>",
        ["live"] = "<circle cx='12' cy='12' r='2.5'/><path d='M7.4 7.4a6.5 6.5 0 0 0 0 9.2M16.6 7.4a6.5 6.5 0 0 1 0 9.2M4.6 4.6a10.5 10.5 0 0 0 0 14.8M19.4 4.6a10.5 10.5 0 0 1 0 14.8'/>",
        ["trending-up"] = "<path d='m3.5 17 6-6 4 4 7-7.5'/><path d='M15 7.5h5.5V13'/>",
        ["crown"] = "<path d='m3.5 8 4.5 4 4-7 4 7 4.5-4-1.5 10.5h-14z'/>",
        ["map-pin"] = "<path d='M12 21s7-6.2 7-11.5A7 7 0 0 0 5 9.5C5 14.8 12 21 12 21z'/><circle cx='12' cy='9.5' r='2.5'/>",
        ["clock"] = "<circle cx='12' cy='12' r='9'/><path d='M12 7v5l3 2'/>",
        ["layers"] = "<path d='m12 3.5 9 4.8-9 4.8-9-4.8z'/><path d='m3 12.5 9 4.8 9-4.8M3 16.7l9 4.8 9-4.8'/>",
        ["building"] = "<rect x='5' y='3.5' width='14' height='17' rx='2'/><path d='M9 8h2M13 8h2M9 12h2M13 12h2M10 20.5v-4h4v4'/>",
        ["mail"] = "<rect x='3' y='5.5' width='18' height='13' rx='2.5'/><path d='m4 8 8 5.5L20 8'/>",
        ["sparkle"] = "<path d='M12 3.5 14 10l6.5 2L14 14l-2 6.5L10 14l-6.5-2L10 10z'/>",
        // Phase 2 additions
        ["columns"] = "<path d='M4 4h6v16H4zM14 4h6v16h-6z'/>",
        ["filter-filled"] = "<path d='M3.5 5h17l-6.5 7.5V19l-4 1.5v-8z' fill='currentColor' stroke='none'/>",
        ["file-text"] = "<path d='M6 2h9l5 5v15H4V2h2z'/><path d='M14 2v5h5'/><path d='M8 10h8M8 13h8M8 16h5'/>",
        ["file-spreadsheet"] = "<path d='M6 2h9l5 5v15H4V2h2z'/><path d='M14 2v5h5'/><path d='M4 10h16M12 10v12M4 15h16'/>",
        ["alert-circle"] = "<circle cx='12' cy='12' r='9'/><path d='M12 8v4.5M12 15.2h.01'/>",
        ["edit"] = "<path d='M4 20 15.5 8.5l4 4L8 20H4z'/><path d='m17.5 6.5 2 2'/>",
        ["arrow-left"] = "<path d='M19 12H5M11 6l-6 6 6 6'/>",
    };

    public static void Register(string name, string innerSvg) => _paths[name] = innerSvg;

    public static string? Get(string name) => _paths.TryGetValue(name, out var p) ? p : null;
}

using Microsoft.JSInterop;

namespace ChampUI.Demo.Services;

/// <summary>
/// Triggers a browser file download. This is the only JavaScript interop in the project: a browser
/// cannot save a file from the server without it.
/// </summary>
public sealed class DownloadService(IJSRuntime js)
{
    public async Task DownloadTextAsync(string fileName, string content, string mimeType = "text/csv;charset=utf-8")
    {
        await using var module = await js.InvokeAsync<IJSObjectReference>("import", "./js/download.js");
        await module.InvokeVoidAsync("downloadText", fileName, content, mimeType);
    }

    public static string ToCsv(IEnumerable<string> header, IEnumerable<IEnumerable<object?>> rows)
    {
        static string Esc(object? v)
        {
            var s = Convert.ToString(v, System.Globalization.CultureInfo.InvariantCulture) ?? "";
            // Neutralise spreadsheet formula injection as well as quoting.
            if (s.Length > 0 && "=+-@".Contains(s[0])) s = "'" + s;
            return "\"" + s.Replace("\"", "\"\"") + "\"";
        }
        var lines = new List<string> { string.Join(',', header.Select(Esc)) };
        lines.AddRange(rows.Select(r => string.Join(',', r.Select(Esc))));
        return "﻿" + string.Join("\r\n", lines); // BOM so Excel detects UTF-8 (Arabic text)
    }
}

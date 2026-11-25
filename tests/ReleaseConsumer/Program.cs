using System.Diagnostics;
using System.Reflection;
using ChampUI.Services;
using ReleaseConsumer.Components;

var assembly = typeof(UiThemeState).Assembly;
var assemblyVersion = assembly.GetName().Version?.ToString();
var fileVersion = FileVersionInfo.GetVersionInfo(assembly.Location).FileVersion;
var informationalVersion = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
if (assemblyVersion != "0.3.1.0" || fileVersion != "0.3.1.0" || informationalVersion != "0.3.1")
    throw new InvalidOperationException("The installed ChampUI assembly contains unexpected version metadata.");
Console.WriteLine($"ChampUI metadata: assembly {assemblyVersion}, file {fileVersion}, informational {informationalVersion}");

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
builder.Services.AddChampUI();

var app = builder.Build();
app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

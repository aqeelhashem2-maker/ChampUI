using System.ComponentModel.DataAnnotations;
using ChampUI.Services;
using ChampUI.Starter.Components;
using ChampUI.Starter.Models;
using ChampUI.Starter.Services;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddRazorComponents().AddInteractiveServerComponents();
var identity = builder.Configuration.GetSection(DashboardOptions.Section).Get<DashboardOptions>() ?? new DashboardOptions();
Validator.ValidateObject(identity, new ValidationContext(identity), validateAllProperties: true);
identity.ResolveLogo(builder.Environment.WebRootFileProvider);
builder.Services.AddSingleton(identity);
builder.Services.AddChampUI(options => options.DefaultMode = identity.DefaultTheme);
builder.Services.AddScoped<WorkspaceState>();
builder.Services.AddScoped<IDashboardData, SampleDashboardData>();

var app = builder.Build();
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseAntiforgery();
app.MapStaticAssets();
app.MapRazorComponents<App>().AddInteractiveServerRenderMode();
app.Run();

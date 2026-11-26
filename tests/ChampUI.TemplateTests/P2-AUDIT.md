# P2 baseline audit

Recorded against the starter source and the 0.1.1 acceptance evidence before making P2 changes.

| Requirement | Status | Baseline evidence |
| --- | --- | --- |
| Central identity and validated required fields | Implemented and verified | `DashboardOptions`, the `Dashboard` section in `appsettings.json`, and startup validation in `Program.cs`. The prior identity browser test verifies the display name and description. |
| Sidebar brand, subtitle and valid logo | Implemented and verified | `MainLayout.razor` renders the configured brand, language-specific subtitle, and logo. The prior identity test verified a published custom logo. |
| Missing or malformed logo, favicon and responsive branding | Partially implemented | Missing images require a fallback. The path validator aborts startup for malformed optional input. No favicon is wired and mobile identity was not explicitly verified. |
| Document title, initial heading and topbar | Partially implemented | `DashboardTitle.razor` combines the localized page with the display name. There is no separate browser title setting. The dashboard heading and initial workspace label use built-in copy. |
| Initial language, theme and RTL | Implemented and verified | `App.razor`, `UiThemeState`, and `WorkspaceState` use the configured defaults. The prior identity scenario covers static and interactive rendering. |
| Developer README | Partially implemented | Local installation, branding, sample data and Production instructions exist. The guide toggle and new P2 options are absent. |
| In-app Getting Started and route visibility control | Missing | No corresponding page or sidebar route. |
| Template installation, generated projects, and prior regressions | Implemented and verified | The 0.1.1 gate built two projects from the installed package and passed 21 Chromium cases. |
| New isolated package and checksum | Missing | The most recent local package is 0.1.1. |

## Files changed in P2

All paths are relative to the workspace.

- `templates/ChampUI.Dashboard/Components/App.razor`
- `templates/ChampUI.Dashboard/Components/Layout/MainLayout.razor`
- `templates/ChampUI.Dashboard/Components/Pages/Dashboard.razor`
- `templates/ChampUI.Dashboard/Components/Pages/GettingStarted.razor`
- `templates/ChampUI.Dashboard/Components/Shared/DashboardTitle.razor`
- `templates/ChampUI.Dashboard/Models/DashboardOptions.cs`
- `templates/ChampUI.Dashboard/Program.cs`
- `templates/ChampUI.Dashboard/README.md`
- `templates/ChampUI.Dashboard/Services/WorkspaceState.cs`
- `templates/ChampUI.Dashboard/appsettings.json`
- `templates/ChampUI.Dashboard/wwwroot/dashboard-icon.svg`
- `templates/ChampUI.Dashboard/wwwroot/dashboard.css`
- `templates/ChampUI.Templates.Packaging/ChampUI.Templates.Packaging.csproj`
- `tests/ChampUI.TemplateTests/P2-AUDIT.md`
- `tests/ChampUI.TemplateTests/README.md`
- `tests/ChampUI.TemplateTests/browser.cjs`
- `tests/ChampUI.TemplateTests/fixtures/FocusAudit.razor`
- `tests/ChampUI.TemplateTests/guide_examples.py`
- `tests/ChampUI.TemplateTests/identity.cjs`
- `tests/ChampUI.TemplateTests/p2-config.cjs`
- `tests/ChampUI.TemplateTests/verify.sh`

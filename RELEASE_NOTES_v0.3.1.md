# ChampUI v0.3.1 — NuGet Metadata & Release Maintenance

**Developer and publisher** Sadeem Systems

## Changes

- The NuGet package now includes official project and Git repository links, an exact repository commit and patch release notes.
- Package and informational versions are `0.3.1`. Assembly and file versions are `0.3.1.0`.
- The package still targets `net10.0` and references the ASP.NET Core shared framework. README, license, CSS and JavaScript ship with the package.
- There are no intentional breaking API changes or functional changes to components or the neutral Showcase.

[Project URL](https://github.com/aqeelhashem2-maker/ChampUI) · [Repository URL](https://github.com/aqeelhashem2-maker/ChampUI)

## Local validation

| Check | Result |
| --- | --- |
| Clean Release build | 0 warnings, 0 errors |
| Unit and integration | 264 passed, 0 failed, 0 skipped |
| Chromium | 47 passed, 0 failed, 5 optional skips |
| Independent NuGet consumer | Restored from a local feed, built and verified InteractiveServer, UiDataGrid, static assets, RTL, theme switching, CSV and XLSX |
| Neutral Showcase | All five captured views matched the v0.3.0 visual baseline |
| API and IL comparison | Public API shape and resolved IL matched the approved v0.3.0 library |

## Installation and coverage

This package targets .NET 10. The independent consumer verifies the InteractiveServer render mode. Once the package is published to NuGet.org, consumers can run `dotnet add package ChampUI --version 0.3.1`. The current candidate can be restored from a local NuGet feed.

The browser gate uses Playwright Chromium. The five optional skips cover checkbox structure, empty search and the select-all locator diagnostics, plus Firefox and WebKit. Virtual range-provider full-query export requires provider support. Current-page export is disabled in that mode, and Select All is limited to loaded identities, capped at 10,000.

## License

MIT. Copyright (c) 2026 Sadeem Systems. [License](LICENSE)

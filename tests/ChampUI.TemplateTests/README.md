# Dashboard starter acceptance tests

Requires the .NET 10 SDK, Node.js 18+, Python 3, Bash, and Playwright Chromium. Run from the repository root:

```sh
npm ci --prefix tests/ChampUI.TemplateTests
(cd tests/ChampUI.TemplateTests && npx playwright install chromium)
CHAMPUI_APPROVED_PACKAGE=/absolute/path/to/ChampUI.0.3.1.nupkg \
  tests/ChampUI.TemplateTests/verify.sh
```

Use the `ChampUI.0.3.1.nupkg` attachment from the [official GitHub release](https://github.com/aqeelhashem2-maker/ChampUI/releases/tag/v0.3.1). The gate verifies its SHA-256. Set `CHAMPUI_TEMPLATE_PACKAGE` to an existing local template package to test those exact bytes. When that variable is omitted, the gate packs the checked-out template source before testing.

On Linux, use `npx playwright install --with-deps chromium` to install browser dependencies. The gate starts two isolated applications from the packed template. Development restores the verified local ChampUI package. Published Production restores ChampUI from the official public registry. The SDK may also restore its own ASP.NET Core static assets package.

The gate uses fresh template and NuGet caches. It validates package provenance, generated names, signed public package contents, the browser scenarios, and the CSV and XLSX record identities. Logs, screenshots, downloads, and Playwright traces are written to `.local/champui-templates/verification` by default. Pass a directory as the first argument to choose a different location.

The focus audit fixture and custom logo are copied only into temporary generated projects. They are not included in the template. The gate also starts the published app with an alternate identity and Arabic/dark defaults, and checks the same values before and after hydration. See [Focus audit](FOCUS-AUDIT.md) for the selectors, retained indicators and separate library proposal.

For an isolated P2 package and evidence directory, run `tests/ChampUI.TemplateTests/verify.sh .local/champui-templates/v0.1.2/evidence` with a new output directory. In both generated projects, the gate compiles the Getting Started page, sidebar and REST client examples from the guide. The REST registration stays in a test fixture and makes no API requests. The browser gate also checks the default and customized favicon, missing and rejected logo paths, and direct and interactive attempts to open a disabled guide.

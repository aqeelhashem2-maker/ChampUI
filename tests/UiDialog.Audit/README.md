# Chromium dialog regression

Copy DialogAudit.razor into Components/Pages of an isolated Dashboard Starter 0.1.2 app. Set that generated app's ChampUI PackageReference to the candidate package, then build and start the Development app or publish and start the Production output. Install the Starter test Playwright dependency and run:

```sh
PLAYWRIGHT_MODULE=/absolute/path/to/node_modules/playwright \
  node tests/UiDialog.Audit/dialog.cjs http://127.0.0.1:5185 /absolute/path/to/evidence
```

Create the evidence directory first. The fixture controls are for testing keyboard focus and dismissal. The tests use real clicks, keys and Chromium's accessibility tree. Run them for both hosting environments. The fixture is excluded from the library package.

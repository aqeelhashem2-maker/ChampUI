# ChampUI Showcase

Developed by Sadeem Systems. The `demo/ChampUI.Demo` project hosts the neutral showcase and the browser fixtures on a development machine.

| Route | Components and behavior |
| --- | --- |
| `/` | Typography, icons, badges, tabs, alerts, toasts, dialogs and loading states |
| `/forms` | Text and numeric inputs, selector, date picker, checkbox, switch and generic field wrapper |
| `/data` | Local, paged and virtual DataGrid modes, filtering, sorting, selection, editing and export |
| `/charts` | Line, bar, area and donut charts |

The shell's theme and language buttons switch the theme and show the Arabic RTL shell. The project records and chart values are fictional. Fixture routes `/interactive-smoke`, `/virtualize-smoke`, `/virtual-data-test` and `/row-identity-test` are retained for behavioral tests.

Use `verify_fixes.sh` for full local regression. Evidence goes to the directory specified by `CHAMPUI_EVIDENCE_DIR`. Run `python3 tests/verify_ci_gates.py` after a successful local run to replay the recorded TRX through the release gate. The gate supports Bash 3.2 or newer with Python 3, curl, .NET 10 and the pinned Chromium installed. Visual baselines document this showcase and its environment. Capture again with the same Chromium build, system fonts, viewport and settings to compare future changes.

For a local capture, build and start the showcase, then set `CHAMPUI_PLAYWRIGHT_MODULE` to the Playwright 1.49.0 Node package. With Chromium installed, run:

```sh
node tests/ReleaseConsumer/verify-showcase-visual.cjs http://localhost:5000 ./audit-evidence/showcase
```

The script records the four light pages and the dark Arabic RTL landing page at 1440 × 1000 with a scale factor of 1.

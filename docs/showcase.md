# ChampUI Showcase

Developed by Sadeem Systems. The `demo/ChampUI.Demo` project hosts the neutral showcase and the browser fixtures on a development machine.

| Route | Components and behavior |
| --- | --- |
| `/` | Typography, icons, badges, tabs, alerts, toasts, dialogs and loading states |
| `/forms` | Text and numeric inputs, selector, date picker, checkbox, switch and generic field wrapper |
| `/data` | Local, paged and virtual DataGrid modes, filtering, sorting, selection, editing and export |
| `/charts` | Line, bar, area and donut charts |

The shell's theme and language buttons switch the theme and show the Arabic RTL shell. The project records and chart values are fictional. Fixture routes `/interactive-smoke`, `/virtualize-smoke`, `/virtual-data-test` and `/row-identity-test` are retained for behavioral tests.

Use `verify_fixes.sh` for full local regression. Evidence goes to the directory specified by `CHAMPUI_EVIDENCE_DIR`. Visual baselines document this showcase and its environment. Capture again with the same Chromium build, system fonts, viewport and settings to compare future changes.

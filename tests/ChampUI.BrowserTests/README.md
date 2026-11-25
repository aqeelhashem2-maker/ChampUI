# ChampUI Chromium verification

Run `./verify_fixes.sh` from the repository root. It performs a clean Release build, runs the complete unit and integration suite, starts the built demo on `http://localhost:5000`, and runs the complete browser project. It records logs and TRX results in `audit-evidence`. Set `CHAMPUI_EVIDENCE_DIR` to direct the output elsewhere.

.NET 10 and Playwright's pinned Chromium build are required. After building the browser test project, install Chromium with:

```sh
pwsh tests/ChampUI.BrowserTests/bin/Release/net10.0/playwright.ps1 install chromium
```

The mandatory Chromium scenarios cover query propagation, empty results and recovery, stable row identity in all three grid rendering modes, selection, virtual scrolling, and real CSV/XLSX downloads. The export tests also exercise a range-only provider and an explicitly held query refresh. The skipped Firefox and WebKit scenarios and the three historical diagnostic cases are outside the mandatory Chromium gate.

The test pages expose exact request search, filter, sort and range values under `/virtual-data-test` and row-identity cases under `/row-identity-test`. Playwright traces and screenshots are written when `CHAMPUI_EVIDENCE_DIR` is set.

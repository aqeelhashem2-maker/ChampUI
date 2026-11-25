#!/usr/bin/env bash
set -uo pipefail
cd "$(dirname "$0")" || exit 1

evidence_dir=${CHAMPUI_EVIDENCE_DIR:-"$PWD/audit-evidence"}
mkdir -p "$evidence_dir" || exit 1
evidence_dir=$(cd "$evidence_dir" && pwd) || exit 1
export CHAMPUI_EVIDENCE_DIR="$evidence_dir"
export DOTNET_CLI_TELEMETRY_OPTOUT=1 DOTNET_NOLOGO=1

build_args=()
if [[ -n ${CHAMPUI_NUGET_SOURCE:-} ]]; then
    build_args+=(--source "$CHAMPUI_NUGET_SOURCE")
fi
if [[ ${CHAMPUI_OFFLINE:-0} == 1 ]]; then
    build_args+=(-p:NuGetAudit=false)
fi

run_logged() {
    local log=$1
    shift
    "$@" 2>&1 | tee "$evidence_dir/$log"
}

run_logged clean-release.log dotnet clean ChampUI.slnx -c Release --nologo -m:1 -v:normal || exit 1
run_logged build-release.log dotnet build ChampUI.slnx -c Release --nologo -m:1 ${build_args[@]+"${build_args[@]}"} || exit 1

failed=0
if ! awk '/^[[:space:]]*[0-9]+ Warning\(s\)[[:space:]]*$/ { seen=1; warnings=$1 }
    END { if (!seen) print "Build warning summary is missing"; else print "Build warnings:", warnings;
          exit !seen || warnings != 0 }' "$evidence_dir/build-release.log"; then
    failed=1
fi

rm -f "$evidence_dir/unit.trx" "$evidence_dir/chromium.trx"
run_logged unit.log dotnet test tests/ChampUI.Tests -c Release --no-build --no-restore \
    --logger 'console;verbosity=detailed' --logger 'trx;LogFileName=unit.trx' --results-directory "$evidence_dir" || failed=1

if curl -fsS --max-time 2 http://localhost:5000/ >/dev/null 2>&1; then
    echo "Port 5000 is already serving a site. Chromium requires the demo from this build." | tee "$evidence_dir/chromium.log"
    failed=1
else
    (cd demo/ChampUI.Demo && exec env ASPNETCORE_ENVIRONMENT=Development \
        dotnet bin/Release/net10.0/ChampUI.Demo.dll --urls http://localhost:5000) > "$evidence_dir/demo-server.log" 2>&1 &
    demo_pid=$!
    trap 'kill "$demo_pid" 2>/dev/null; wait "$demo_pid" 2>/dev/null || true' EXIT
    for ((attempt=0; attempt<60; attempt++)); do
        if curl -fsS --max-time 2 http://localhost:5000/virtual-data-test >/dev/null 2>&1; then
            break
        fi
        if ! kill -0 "$demo_pid" 2>/dev/null; then break; fi
        sleep 1
    done

    if curl -fsS --max-time 2 http://localhost:5000/virtual-data-test >/dev/null 2>&1; then
        run_logged chromium.log dotnet test tests/ChampUI.BrowserTests -c Release --no-build --no-restore \
            --logger 'console;verbosity=detailed' --logger 'trx;LogFileName=chromium.trx' --results-directory "$evidence_dir" || failed=1
    else
        echo "Demo startup failed. See $evidence_dir/demo-server.log" | tee "$evidence_dir/chromium.log"
        failed=1
    fi
fi

if ! python3 - "$evidence_dir" <<'PY' | tee "$evidence_dir/test-summary.log"; then
from pathlib import Path
from collections import Counter
import sys
import xml.etree.ElementTree as ET

root = Path(sys.argv[1])
ns = {"v": "http://microsoft.com/schemas/VisualStudio/TeamTest/2010"}
optional_chromium = {
    "CheckboxDiagnosticTest",
    "EmptySearchDiagnostic",
    "SelectAllLocatorTest",
}
mandatory = {
    "unit": ("LateRangeResponsesCannotUpdateSelectionAcrossRangesOrQueries",
             "VirtualExportDropsObsoleteQueryAndDownloadsExactFreshIds"),
    "chromium": ("RendersEmptyStateWhenNoResults", "SearchChange_TriggersVirtualQueryRefresh",
                 "SortChange_TriggersVirtualQueryRefresh", "FilterChange_PropagatesOperatorAndValue",
                 "Identity_TypedKeysSurviveRefreshSortAndFilter", "E2E_LegacyCurrentPage",
                 "E2E_VirtualExport_ExactRecordsAcrossScrollRanges", "E2E_VirtualExport_UsesNewQueryWhileRangeRefreshIsPending",
                 "E2E_Export_UnsupportedScope_ShowsError", "B10_LoadingState", "B11_QueryGeneration",
                 "Showcase_supports_themes_RTL_and_data_sources_without_console_errors",
                 "StableSelectionTest.B12_01", "StableSelectionTest.B12_02", "StableSelectionTest.B12_03",
                 "StableSelectionTest.B12_04", "StableSelectionTest.B12_05"),
}
failed = False
for suite in ("unit", "chromium"):
    path = root / f"{suite}.trx"
    if not path.is_file():
        print(f"{suite}: blocked (no test results)")
        failed = True
        continue
    results = ET.parse(path).findall(".//v:UnitTestResult", ns)
    counts = Counter(result.attrib["outcome"] for result in results)
    skipped = [result.attrib["testName"] for result in results if result.attrib["outcome"] == "NotExecuted"]
    passed = [result.attrib["testName"] for result in results if result.attrib["outcome"] == "Passed"]
    blocked = len(results) - counts["Passed"] - counts["Failed"] - counts["NotExecuted"]
    print(f"{suite}: {counts['Passed']} passed, {counts['Failed']} failed, {len(skipped)} skipped, {blocked} blocked")
    for name in skipped:
        print(f"  skipped: {name}")
    unexpected_skips = skipped if suite == "unit" else [
        name for name in skipped if not (
            any(f".{cls}." in name for cls in optional_chromium) or name.endswith(("_Firefox", "_WebKit"))
        )
    ]
    missing = [name for name in mandatory[suite] if not any(name in test for test in passed)]
    if suite == "chromium" and sum("Identity_TypedKeysSurviveRefreshSortAndFilter" in name for name in passed) != 3:
        missing.append("all three rendering-mode identity cases")
    if unexpected_skips or missing:
        print(f"  unexpected skips: {unexpected_skips}; missing mandatory passes: {missing}")
    failed |= not results or bool(counts["Failed"] or blocked or unexpected_skips or missing)
sys.exit(failed)
PY
    failed=1
fi
exit "$failed"

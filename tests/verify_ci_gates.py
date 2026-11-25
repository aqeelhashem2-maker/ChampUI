"""Replay completed TRX results through the release gate with isolated tool stubs."""

import argparse
import os
from pathlib import Path
import shutil
import subprocess
from tempfile import TemporaryDirectory
from xml.etree import ElementTree as ET

REPO = Path(__file__).resolve().parents[1]

DOTNET = """#!/usr/bin/env python3
import os
from pathlib import Path
import shutil
import sys

command, *args = sys.argv[1:]
root = Path(os.environ["GATE_FIXTURE"])
if command == "build":
    print(os.environ["GATE_WARNINGS"] + " Warning(s)")
elif command == "test":
    suite = "chromium" if "BrowserTests" in args[0] else "unit"
    name = suite + ".trx"
    if (root / name).exists():
        shutil.copyfile(root / name, Path(args[args.index("--results-directory") + 1]) / name)
elif command.endswith("ChampUI.Demo.dll"):
    os.execl("/bin/sleep", "sleep", "30")
"""

CURL = """#!/bin/sh
if [ -f "$GATE_FIXTURE/startup-check" ]; then exit 0; fi
touch "$GATE_FIXTURE/startup-check"
exit 7
"""


def run_case(evidence, scenario, suite=None, test_name=None, outcome=None, diagnostic=None):
    with TemporaryDirectory(prefix="champui-ci-gate-") as temporary:
        root = Path(temporary)
        tools = root / "tools"
        tools.mkdir()
        (root / "demo/ChampUI.Demo").mkdir(parents=True)
        shutil.copyfile(REPO / "verify_fixes.sh", root / "verify_fixes.sh")
        for name, body in (("dotnet", DOTNET), ("curl", CURL)):
            command = tools / name
            command.write_text(body)
            command.chmod(0o755)
        for name in ("unit", "chromium"):
            if name == "chromium" and scenario == "missing Chromium results":
                continue
            fixture = evidence / f"{name}.trx"
            if name == suite:
                report = ET.parse(fixture)
                result = next(node for node in report.findall(".//{*}UnitTestResult")
                              if test_name in node.attrib["testName"])
                assert result.attrib["outcome"] == "Passed", test_name
                result.set("outcome", outcome)
                report.write(root / fixture.name, encoding="utf-8")
            else:
                shutil.copyfile(fixture, root / fixture.name)
        environment = dict(os.environ, PATH=f'{tools}{os.pathsep}{os.environ["PATH"]}',
                           GATE_FIXTURE=str(root), CHAMPUI_EVIDENCE_DIR=str(root / "results"),
                           GATE_WARNINGS="1" if scenario == "build warnings" else "0")
        environment.pop("CHAMPUI_OFFLINE", None)
        environment.pop("CHAMPUI_NUGET_SOURCE", None)
        process = subprocess.run(
            ["bash", str(root / "verify_fixes.sh")], env=environment,
            capture_output=True, text=True, timeout=60,
        )
        expected = 0 if diagnostic is None else 1
        assert process.returncode == expected, (scenario, process.returncode, process.stdout, process.stderr)
        if diagnostic:
            assert diagnostic in process.stdout, (scenario, process.stdout, process.stderr)
        print(f"{scenario}: exit {process.returncode}")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("evidence", nargs="?", type=Path, default=REPO / "audit-evidence")
    reports = parser.parse_args().evidence.resolve()
    run_case(reports, "unchanged TRX")
    run_case(reports, "build warnings", diagnostic="Build warnings: 1")
    run_case(reports, "failing unit test", "unit",
             "LateRangeResponsesCannotUpdateSelectionAcrossRangesOrQueries", "Failed", "1 failed")
    run_case(reports, "failing mandatory Chromium test", "chromium",
             "SearchChange_TriggersVirtualQueryRefresh", "Failed", "1 failed")
    run_case(reports, "skipped mandatory Chromium test", "chromium",
             "SearchChange_TriggersVirtualQueryRefresh", "NotExecuted", "unexpected skips:")
    run_case(reports, "missing Chromium results", diagnostic="chromium: blocked (no test results)")

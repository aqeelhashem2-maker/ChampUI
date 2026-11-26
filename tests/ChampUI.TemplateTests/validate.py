import csv
import json
from pathlib import Path
import sys
from xml.etree import ElementTree as ET
from zipfile import ZipFile


def verify_project(directory, packages, source):
    directory, packages = Path(directory), Path(packages)
    project = ET.parse(next(directory.glob("*.csproj")))
    assert project.findtext(".//TargetFramework") == "net10.0"
    assert project.findall(".//ProjectReference") == []
    assert [ref.attrib for ref in project.findall(".//PackageReference")] == [
        {"Include": "ChampUI", "Version": "0.3.3"}
    ]
    sources = [*directory.rglob("*.razor"), *directory.rglob("*.cs"), *directory.glob("*.csproj")]
    for file in sources:
        if not {"bin", "obj"}.intersection(file.relative_to(directory).parts):
            assert "ChampUI.Starter" not in file.read_text(), file
    assert not (directory / ".template.config").exists()
    assets = json.loads((directory / "obj/project.assets.json").read_text())
    for identifier, library in assets["libraries"].items():
        assert library["type"] == "package", identifier
        assert identifier == "ChampUI/0.3.3" or identifier.startswith(
            "Microsoft.AspNetCore.App.Internal.Assets/10.0."
        ), identifier
    assert "ChampUI/0.3.3" in assets["libraries"]
    metadata = json.loads((packages / "champui/0.3.3/.nupkg.metadata").read_text())
    assert metadata["source"] == source, metadata["source"]
    print(f"{directory.name}: exact ChampUI 0.3.3 package and source verified ({source})")


def verify_downloads(directory):
    directory = Path(directory)
    with (directory / "orders.csv").open(encoding="utf-8-sig", newline="") as stream:
        rows = list(csv.reader(stream))
    expected = [f"ORD-{number}" for number in range(1048, 1038, -1)]
    assert rows[0][0] == "Order"
    assert [row[0] for row in rows[1:]] == expected, rows
    ns = {"s": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}
    with ZipFile(directory / "orders.xlsx") as zip_file:
        worksheet = ET.fromstring(zip_file.read("xl/worksheets/sheet1.xml"))
        ids = []
        for row in worksheet.findall(".//s:row", ns):
            first = row.find("s:c", ns)
            ids.append("".join(text.text or "" for text in first.findall(".//s:t", ns)))
    assert ids == ["Order", *expected], ids
    print("CSV and XLSX contain all ten expected order IDs in the same order.")


if __name__ == "__main__":
    operation, *arguments = sys.argv[1:]
    {"project": verify_project, "downloads": verify_downloads}[operation](*arguments)

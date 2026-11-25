import csv
import sys
from pathlib import Path
from xml.etree import ElementTree as ET
from zipfile import ZipFile

package, evidence = map(Path, sys.argv[1:3])
with ZipFile(package) as archive:
    for name in ("champui.css", "js/champui.js"):
        assert archive.read(f"staticwebassets/{name}") == (evidence / Path(name).name).read_bytes()
print("Both served static assets match the NuGet package byte for byte.")

namespace = {"x": "http://schemas.openxmlformats.org/spreadsheetml/2006/main"}
for scope, expected in (("local", list(range(6, 11))), ("virtual", list(range(29, 19, -1)))):
    csv_path = evidence / f"consumer-{scope}.csv"
    assert csv_path.read_bytes().startswith(b"\xef\xbb\xbf")
    with csv_path.open(encoding="utf-8-sig", newline="") as stream:
        header, *records = csv.reader(stream)
    assert header == ["ID", "Name"]
    assert [int(record[0]) for record in records] == expected
    with ZipFile(evidence / f"consumer-{scope}.xlsx") as workbook:
        assert workbook.testzip() is None
        sheet = ET.fromstring(workbook.read("xl/worksheets/sheet1.xml"))
    rows = sheet.findall("x:sheetData/x:row", namespace)
    assert [int(row.findtext("x:c/x:v", namespaces=namespace)) for row in rows[1:]] == expected
    print(f"{scope} CSV and XLSX match {len(expected)} exact ordered IDs: {expected}")

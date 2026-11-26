"""Add a query diagnostic to an isolated Starter 0.1.2 Orders page."""
from pathlib import Path
import sys

page = Path(sys.argv[1])
source = page.read_text()
grid = '<UiDataGrid @key="P.Arabic" TItem="Order" Items="orders"'
assert source.count(grid) == 1 and source.count("@code {") == 1
source = source.replace(grid, grid.replace("<UiDataGrid", '<UiDataGrid @ref="_auditGrid"'))
start = source.rfind('<UiCard Flush="true">', 0, source.index('<UiDataGrid @ref="_auditGrid"'))
assert start >= 0
source = source[:start] + '''<button id="capture-query" @onclick="CaptureAuditQuery">Capture test query</button>
        <output id="effective-query" data-seq="@_auditSequence">@_auditQuery</output>
        ''' + source[start:]
page.write_text(source.replace("@code {", '''@code {
    private UiDataGrid<Order>? _auditGrid;
    private int _auditSequence;
    private string? _auditQuery;

    private void CaptureAuditQuery()
    {
        var field = typeof(UiDataGrid<Order>).GetField("_search",
            System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic);
        _auditQuery = (string?)field!.GetValue(_auditGrid);
        _auditSequence++;
    }
'''))

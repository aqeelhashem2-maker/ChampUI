# Chromium input regression

Use an isolated Dashboard Starter application restored against the candidate
ChampUI package. Add TextFieldAudit.razor to Components/Pages. Instrument only the isolated generated Orders page before building:

```sh
python3 tests/UiTextField.Audit/add-orders-probe.py /absolute/path/to/generated/Components/Pages/Orders.razor
```

Run against the Interactive Server app:

```sh
PLAYWRIGHT_MODULE=/absolute/path/to/node_modules/playwright \
  node tests/UiTextField.Audit/input.cjs http://127.0.0.1:5245
```

This suite sends keyboard events at four speeds, with and without delayed
server WebSocket messages. It also exercises caret position, Chromium IME,
the Enter callback, and external value changes. The probes belong in the
test app only.

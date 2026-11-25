# Contributing to ChampUI

Run `dotnet build ChampUI.slnx -c Release`. Install Chromium with `pwsh tests/ChampUI.BrowserTests/bin/Release/net10.0/playwright.ps1 install --with-deps chromium` on Linux, then run `./verify_fixes.sh` from the root. Use `install chromium` on other supported development platforms.

Keep changes focused and describe how you verified them. Use synthetic examples and remove secrets from logs and attachments. Submit code you have the right to contribute under the [MIT License](LICENSE). Preserve the notices for third-party material.

Use the [security policy](SECURITY.md) for potential vulnerabilities.

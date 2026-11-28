# Independent generated-artifact checks

These fixtures are the original user-supplied `Customers (1).razor` and
`SalonDashboard.zip`, copied byte-for-byte. `verify.sh` checks their SHA-256
before execution, installs the published Starter 0.1.3 in an isolated template
hive, restores ChampUI 0.3.3 from public NuGet.org in a fresh package cache,
publishes both consumers and runs the original salon service checks and two
Chromium suites. The salon source and business logic are not changed.

Prerequisites: .NET 10 SDK, Node 22, npm, Python 3, unzip, curl and Chromium
installed for the pinned Playwright dependency. From the repository root:

```sh
npm ci --prefix tests/ChampUI.TemplateTests
tests/ChampUI.TemplateTests/node_modules/.bin/playwright install chromium
bash tests/ChampUI.AiArtifacts/verify.sh
```

The script returns nonzero for any failed assertion and prints its isolated
evidence directory containing `verification.log` and server logs. Port 5191
and 5193 must be free. The generated example records are synthetic and are
discarded when their circuit ends. `BusinessChecks.cs` exercises independently
identified rules; it does not claim to reproduce Work's undocumented 11 checks.

The current suites do not establish payment receipt, screen-reader coverage,
or a direct ChatGPT skill-read trace. Expand tests when evidence is available;
do not treat absence of a failure as proof of untested behaviors.

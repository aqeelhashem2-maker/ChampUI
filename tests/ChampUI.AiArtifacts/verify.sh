#!/usr/bin/env bash
set -euo pipefail
root="$(cd "$(dirname "$0")/../.." && pwd)"
fixture="$root/tests/ChampUI.AiArtifacts/fixtures"
work="$(mktemp -d "${TMPDIR:-/tmp}/champui-ai-artifacts.XXXXXX")"
echo "Evidence: $work"
exec > >(tee "$work/verification.log") 2>&1
export DOTNET_CLI_HOME="$work/home" NUGET_PACKAGES="$work/packages" DOTNET_NOLOGO=1
export PLAYWRIGHT_MODULE="$root/tests/ChampUI.TemplateTests/node_modules/playwright"
server_pid=""
stop_server() { if [[ -n "$server_pid" ]]; then kill "$server_pid" 2>/dev/null || true; wait "$server_pid" 2>/dev/null || true; server_pid=""; fi; }
trap stop_server EXIT
python3 - "$fixture" <<'PY'
import hashlib, pathlib, sys
p = pathlib.Path(sys.argv[1])
for name, expected in [('Customers.razor', '16fb271a943f5a41f162f52fe91c8ca3aac9fc91f66aa64c3803318011060e40'), ('SalonDashboard.zip', 'b69511cd47d5df851a77cdd41087f1b50354006a9058fdde0b4c83adf784a707')]:
    assert hashlib.sha256((p / name).read_bytes()).hexdigest() == expected, name
    print(name, expected)
PY
dotnet new install SadeemSystems.ChampUI.Templates@0.1.3 --nuget-source https://api.nuget.org/v3/index.json --debug:custom-hive "$work/hive"
dotnet new champui-dashboard -n CustomerConsumer -o "$work/customer" --debug:custom-hive "$work/hive"
cp "$fixture/Customers.razor" "$work/customer/Components/Pages/Customers.razor"
unzip -q "$fixture/SalonDashboard.zip" -d "$work/salon"
(cd "$work/customer" && dotnet restore --source https://api.nuget.org/v3/index.json --no-cache && dotnet publish -c Release --no-restore -o "$work/customer-published" -warnaserror)
(cd "$work/salon/SalonDashboard" && dotnet restore --source https://api.nuget.org/v3/index.json --no-cache && dotnet publish -c Release --no-restore -o "$work/salon-published" -warnaserror)
python3 - "$work/customer/obj/project.assets.json" "$work/salon/SalonDashboard/obj/project.assets.json" <<'PY'
import json, sys
for path in sys.argv[1:]:
    data = json.load(open(path))
    assert 'ChampUI/0.3.3' in data['libraries'], path
    assert list(data['project']['restore']['sources']) == ['https://api.nuget.org/v3/index.json'], path
    print('Verified ChampUI 0.3.3 and sole public NuGet source:', path)
PY
dotnet run --project "$root/tests/ChampUI.AiArtifacts/BusinessChecks.csproj" -c Release -p:SalonSource="$work/salon/SalonDashboard"
run_browser() {
  project="$1"; dll="$2"; port="$3"; variable="$4"; testfile="$5"
  (cd "$project" && exec dotnet "$dll" --urls "http://127.0.0.1:$port") > "$work/$variable-server.log" 2>&1 &
  server_pid=$!
  for _ in $(seq 1 90); do
    if curl -fsS "http://127.0.0.1:$port/" -o /dev/null; then break; fi
    if ! kill -0 "$server_pid" 2>/dev/null; then cat "$work/$variable-server.log"; return 1; fi
    sleep 1
  done
  env "$variable=http://127.0.0.1:$port" node --test "$root/tests/ChampUI.AiArtifacts/$testfile"
  stop_server
}
run_browser "$work/customer-published" CustomerConsumer.dll 5191 CUSTOMER_TEST_URL customer-browser.cjs
run_browser "$work/salon-published" SalonDashboard.dll 5193 SALON_TEST_URL salon-browser.cjs

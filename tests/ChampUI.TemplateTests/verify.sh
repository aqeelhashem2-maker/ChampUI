#!/usr/bin/env bash
set -euo pipefail

test_dir="$(cd "$(dirname "$0")" && pwd)"
root="$(cd "$test_dir/../.." && pwd)"
evidence="${1:-$root/.local/champui-templates/verification}"
mkdir -p "$evidence"
evidence="$(cd "$evidence" && pwd)"
work="$(mktemp -d)"
server_pid=""
public_source="https://api.nuget.org/v3/index.json"

stop_server() {
    if [[ -n "$server_pid" ]]; then
        kill "$server_pid" 2>/dev/null || true
        wait "$server_pid" 2>/dev/null || true
        server_pid=""
    fi
}

cleanup() {
    stop_server
    rm -rf "$work"
}

trap cleanup EXIT
trap 'exit 130' INT
trap 'exit 143' TERM

# Tests and subsequent generate/restore use fresh caches and never read a user's NuGet.Config.
export DOTNET_CLI_HOME="$work/dotnet-home"
export DOTNET_SKIP_FIRST_TIME_EXPERIENCE=1
export DOTNET_CLI_TELEMETRY_OPTOUT=1
export DOTNET_GENERATE_ASPNET_CERTIFICATE=false
export DOTNET_ADD_GLOBAL_TOOLS_TO_PATH=false
export DOTNET_CLI_WORKLOAD_UPDATE_NOTIFY_DISABLE=true
export NUGET_HTTP_CACHE_PATH="$work/nuget-http"
export Logging__LogLevel__Microsoft_AspNetCore_DataProtection=Error
approved="${CHAMPUI_APPROVED_PACKAGE:-$root/release/v0.3.1/assets/ChampUI.0.3.1.nupkg}"
mkdir "$work/feed" "$work/generated"
python3 - "$approved" <<'PY'
from hashlib import sha256
from pathlib import Path
import sys
payload = Path(sys.argv[1]).read_bytes()
assert len(payload) == 110625 and sha256(payload).hexdigest() == (
    "fc652da514741ce2d0bb6c6f681d2d359c91e06cba6d06a6251a59196ccaab5e"
), "Unexpected approved package bytes"
PY
cp "$approved" "$work/feed/ChampUI.0.3.1.nupkg"

cat > "$work/public.config" <<XML
<configuration>
  <packageSources><clear /><add key="nuget.org" value="$public_source" /></packageSources>
  <fallbackPackageFolders><clear /></fallbackPackageFolders>
</configuration>
XML
cat > "$work/local.config" <<XML
<configuration>
  <packageSources>
    <clear />
    <add key="approved" value="$work/feed" />
    <add key="nuget.org" value="$public_source" />
  </packageSources>
  <packageSourceMapping>
    <clear />
    <packageSource key="approved"><package pattern="ChampUI" /></packageSource>
    <packageSource key="nuget.org"><package pattern="Microsoft.AspNetCore.App.Internal.Assets" /></packageSource>
  </packageSourceMapping>
  <auditSources><clear /><add key="nuget.org" value="$public_source" /></auditSources>
  <fallbackPackageFolders><clear /></fallbackPackageFolders>
</configuration>
XML

if [[ -n "${CHAMPUI_TEMPLATE_PACKAGE:-}" ]]; then
    template="$(cd "$(dirname "$CHAMPUI_TEMPLATE_PACKAGE")" && pwd)/$(basename "$CHAMPUI_TEMPLATE_PACKAGE")"
    test -f "$template"
    shasum -a 256 "$template" | tee "$evidence/template-checksum.log"
else
    dotnet pack "$root/templates/ChampUI.Templates.Packaging" -c Release -o "$(dirname "$evidence")" \
        -p:RestoreConfigFile="$work/public.config" --disable-build-servers -warnaserror | tee "$evidence/pack.log"
    template="$(dirname "$evidence")/SadeemSystems.ChampUI.Templates.0.1.2.nupkg"
fi
python3 - "$template" <<'PY'
from pathlib import PurePosixPath
import sys
from zipfile import ZipFile
with ZipFile(sys.argv[1]) as package:
    names = package.namelist()
    assert "content/ChampUI.Dashboard/.template.config/template.json" in names
    assert "content/ChampUI.Dashboard/ChampUI.Starter.csproj" in names
    for name in names:
        assert not name.endswith((".dll", ".pdb"))
        assert not {"bin", "obj", ".git", "TestResults"}.intersection(PurePosixPath(name).parts)
PY

cd "$work/generated"
dotnet new install "$template" --debug:custom-hive "$work/hive" | tee "$evidence/install.log"
dotnet new list champui-dashboard --debug:custom-hive "$work/hive" | tee "$evidence/discover.log"
dotnet new champui-dashboard -n MyDashboard --no-update-check --debug:custom-hive "$work/hive" | tee "$evidence/generate.log"
dotnet new champui-dashboard -n Northwind.Ops-Portal --no-update-check --debug:custom-hive "$work/hive" | tee -a "$evidence/generate.log"

for project in MyDashboard Northwind.Ops-Portal; do
    cp "$test_dir/fixtures/FocusAudit.razor" "$project/Components/Pages/"
    python3 "$test_dir/guide_examples.py" "$project"
done
mkdir -p Northwind.Ops-Portal/wwwroot/images
cp "$test_dir/fixtures/logo.svg" Northwind.Ops-Portal/wwwroot/images/audit-logo.svg

dotnet restore MyDashboard --configfile "$work/local.config" --packages "$work/local-packages" --no-http-cache -warnaserror \
    | tee "$evidence/restore-local.log"
python3 "$test_dir/validate.py" project MyDashboard "$work/local-packages" "$work/feed" | tee -a "$evidence/restore-local.log"
cmp "$approved" "$work/local-packages/champui/0.3.1/champui.0.3.1.nupkg"

dotnet restore Northwind.Ops-Portal --configfile "$work/public.config" --packages "$work/public-packages" --no-http-cache -warnaserror \
    | tee "$evidence/restore-public.log"
python3 "$test_dir/validate.py" project Northwind.Ops-Portal "$work/public-packages" "$public_source" | tee -a "$evidence/restore-public.log"
public_package="$work/public-packages/champui/0.3.1/champui.0.3.1.nupkg"
dotnet nuget verify "$public_package" --all | tee "$evidence/public-signature.log"
python3 - "$approved" "$public_package" <<'PY' | tee "$evidence/public-content.log"
from hashlib import sha256
from pathlib import Path
import sys
from zipfile import ZipFile
original, distributed = map(Path, sys.argv[1:])
with ZipFile(original) as approved, ZipFile(distributed) as public:
    assert set(public.namelist()) == {*approved.namelist(), ".signature.p7s"}
    assert all(approved.read(name) == public.read(name) for name in approved.namelist())
    print(f"{len(approved.namelist())} substantive entries match the approved package.")
print(f"Public SHA-256 {sha256(distributed.read_bytes()).hexdigest()}")
PY

dotnet build MyDashboard -c Release --no-restore --disable-build-servers -warnaserror | tee "$evidence/build.log"
dotnet publish Northwind.Ops-Portal -c Release --no-restore --disable-build-servers -warnaserror -o "$work/publish" | tee "$evidence/publish.log"

run_browser() {
    local mode="$1" output url status=0
    shift
    output="$evidence/${CHAMPUI_EVIDENCE_NAME:-$mode}"
    mkdir -p "$output"
    "$@" --urls http://127.0.0.1:0 > "$output/server.log" 2>&1 &
    server_pid=$!
    url="$(python3 - "$output/server.log" "$server_pid" "$mode" <<'PY'
from pathlib import Path
import os
import re
import sys
import time
log, pid, environment = sys.argv[1:]
for _ in range(300):
    content = Path(log).read_text()
    match = re.search(r"Now listening on: (http://127\.0\.0\.1:\d+)", content)
    if match and f"Hosting environment: {environment}" in content:
        print(match[1])
        break
    try:
        os.kill(int(pid), 0)
    except ProcessLookupError:
        sys.exit(content)
    time.sleep(0.1)
else:
    sys.exit(content or "Server did not start")
PY
)" || { stop_server; return 1; }
    node "$test_dir/${CHAMPUI_BROWSER_FILE:-browser.cjs}" "$url" "$output" 2>&1 | tee "$output/browser.log" || status=$?
    if [[ "$status" == 0 && -z "${CHAMPUI_BROWSER_FILE:-}" ]]; then
        python3 "$test_dir/validate.py" downloads "$output" | tee "$output/downloads.log" || status=$?
    fi
    stop_server
    return "$status"
}

dev_status=0
prod_status=0
run_browser Development env -u ASPNETCORE_ENVIRONMENT -u DOTNET_ENVIRONMENT \
    dotnet run --project MyDashboard --no-build -c Release || dev_status=$?
(
    cd "$work/publish"
    run_browser Production env ASPNETCORE_ENVIRONMENT=Production DOTNET_ENVIRONMENT=Production dotnet Northwind.Ops-Portal.dll
) || prod_status=$?
identity_status=0
(
    cd "$work/publish"
    CHAMPUI_EVIDENCE_NAME=Identity CHAMPUI_BROWSER_FILE=identity.cjs run_browser Production env \
        ASPNETCORE_ENVIRONMENT=Production DOTNET_ENVIRONMENT=Production \
        Dashboard__DisplayName="Atlas Operations" Dashboard__SidebarBrand="Atlas" \
        Dashboard__BrowserTitle="Atlas Portal" \
        Dashboard__Description="Atlas workspace overview" Dashboard__LogoPath=/images/audit-logo.svg \
        Dashboard__DefaultLanguage=ar Dashboard__DefaultTheme=Dark dotnet Northwind.Ops-Portal.dll
) || identity_status=$?
missing_status=0
(
    cd "$work/publish"
    CHAMPUI_EVIDENCE_NAME=MissingLogo CHAMPUI_BROWSER_FILE=p2-config.cjs CHAMPUI_P2_CASE=missing \
        run_browser Production env ASPNETCORE_ENVIRONMENT=Production DOTNET_ENVIRONMENT=Production \
        Dashboard__LogoPath=/images/missing-logo.svg Dashboard__ShowDeveloperGuide=false dotnet Northwind.Ops-Portal.dll
) || missing_status=$?
malformed_status=0
CHAMPUI_EVIDENCE_NAME=MalformedLogo CHAMPUI_BROWSER_FILE=p2-config.cjs CHAMPUI_P2_CASE=malformed \
    run_browser Development env -u ASPNETCORE_ENVIRONMENT -u DOTNET_ENVIRONMENT \
    Dashboard__LogoPath=https://invalid.example/brand.svg \
    dotnet run --project MyDashboard --no-build -c Release || malformed_status=$?
printf 'Development exit %s\nProduction exit %s\nCustomized Production identity exit %s\nMissing logo and disabled guide exit %s\nMalformed logo exit %s\n' \
    "$dev_status" "$prod_status" "$identity_status" "$missing_status" "$malformed_status" | tee "$evidence/result.log"
[[ "$dev_status" == 0 && "$prod_status" == 0 && "$identity_status" == 0 && "$missing_status" == 0 && "$malformed_status" == 0 ]]

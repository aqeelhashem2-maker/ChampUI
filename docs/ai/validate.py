"""Compile the Markdown examples as published-package consumers in a new directory."""
import json
import os
import re
import shutil
import subprocess
import tempfile
import time
from urllib.request import urlopen
from pathlib import Path

docs = Path(__file__).resolve().parent
repo = docs.parents[1]
work = Path(tempfile.mkdtemp(prefix="champui-docs-"))
source = "https://api.nuget.org/v3/index.json"
env = {
    **os.environ,
    "DOTNET_CLI_HOME": str(work / "home"),
    "NUGET_PACKAGES": str(work / "packages"),
    "NUGET_HTTP_CACHE_PATH": str(work / "http-cache"),
    "DOTNET_NOLOGO": "1",
    "DOTNET_CLI_TELEMETRY_OPTOUT": "1",
}
print(f"Documentation evidence: {work}", flush=True)
config = work / "NuGet.Config"
config.write_text(f'<configuration><packageSources><clear/><add key="nuget.org" value="{source}"/></packageSources><fallbackPackageFolders><clear/></fallbackPackageFolders></configuration>')

def dotnet(*args, cwd=work):
    subprocess.run(["dotnet", *args], env=env, cwd=cwd, check=True)

def build(project):
    dotnet("restore", "--configfile", str(config), "-warnaserror", cwd=project)
    assets = json.loads((project / "obj/project.assets.json").read_text())
    assert list(assets["project"]["restore"]["sources"]) == [source]
    assert "ChampUI/0.3.3" in assets["libraries"]
    dotnet("build", "-c", "Release", "--no-restore", "-warnaserror", cwd=project)

components = sorted((repo / "src/ChampUI/Components").glob("*/*.razor"))
references = docs / "component-reference"
assert {p.stem for p in components} == {p.stem for p in references.glob("*.md")}
parameters = 0
for path in components:
    reference = (references / (path.stem + ".md")).read_text()
    # Include captured HTML attributes and the public parent cascade.
    signature = r"\[(?:Parameter|CascadingParameter)[^\]]*\]\s*public ([^\n]+?) (\w+) \{ get; set; \}"
    actual = [(name, typ) for typ, name in re.findall(signature, path.read_text())]
    documented = re.findall(r"^\| `(\w+)` \| `([^`]+)`", reference, re.M)
    assert actual == documented, path
    parameters += len(actual)

dotnet("new", "blazor", "-n", "DocsConsumer", "--interactivity", "Server", "--all-interactive", "--no-restore")
project = work / "DocsConsumer"
dotnet("add", "package", "ChampUI", "--version", "0.3.3", "--source", source, cwd=project)
setup = (docs / "application-setup.md").read_text()
setup_files = re.findall(r"^## ([\w/]+\.(?:cs|razor))\n.*?^```(?:csharp|razor)\n(.*?)^```", setup, re.M | re.S)
assert {name for name, _ in setup_files} == {
    "Program.cs", "Components/App.razor", "Components/_Imports.razor", "Components/Layout/MainLayout.razor"
}
for filename, code in setup_files:
    target = project / filename
    if target.name == "_Imports.razor":
        code = target.read_text() + "\n" + code
    target.write_text(code)
examples = project / "Components/DocumentationExamples"
examples.mkdir()
shutil.copyfile(repo / "demo/ChampUI.Demo/Data/ProjectData.cs", examples / "ProjectData.cs")
count = 0
for page in sorted(docs.rglob("*.md")):
    if page.name == "application-setup.md":
        continue
    for code in re.findall(r"^```razor\n(.*?)^```", page.read_text(), re.M | re.S):
        count += 1
        name = "".join(s.capitalize() for s in page.stem.split("-"))
        route = f"/documentation-example/{count}"
        (examples / f"Example{name}{count}.razor").write_text(
            f'@page "{route}"\n' + code + f'\n<span hidden>example-{count}-rendered</span>\n'
        )
assert count >= len(components) + 5
build(project)
# Render each route to exercise parameter assignment, parent cascades and dependency injection.
log = work / "render.log"
with log.open("w") as output:
    host = subprocess.Popen(
        ["dotnet", "run", "-c", "Release", "--no-build", "--no-launch-profile", "--urls", "http://127.0.0.1:0"],
        cwd=project, env=env, stdout=output, stderr=subprocess.STDOUT,
    )
    try:
        for _ in range(300):
            listener = re.search(r"Now listening on: (http://127\.0\.0\.1:\d+)", log.read_text())
            if listener or host.poll() is not None:
                break
            time.sleep(0.2)
        assert listener, log
        for index in range(1, count + 1):
            with urlopen(f"{listener[1]}/documentation-example/{index}", timeout=30) as response:
                assert f"example-{index}-rendered" in response.read().decode(), index
    finally:
        host.terminate()
        host.wait(timeout=30)
dotnet("new", "install", "SadeemSystems.ChampUI.Templates@0.1.3", "--nuget-source", source)
dotnet("new", "champui-dashboard", "-n", "DocsDashboard")
build(work / "DocsDashboard")
print(f"Passed: {len(components)} component contracts, {parameters} parameters, {count} Razor examples and both consumer builds.")

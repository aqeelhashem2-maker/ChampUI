"""Build and exercise the five forward-test outputs in a clean public Starter."""
import json
import os
from pathlib import Path
import re
import shutil
import subprocess
import tempfile
import time

from check_api import check

here = Path(__file__).resolve().parent
scenarios = {
    "customers": ("Customer directory", "دليل العملاء"),
    "employees": ("Employee form", "نموذج الموظف"),
    "salon": ("Salon appointments", "مواعيد الصالون"),
    "finance": ("Revenue report", "تقرير الإيرادات"),
    "rates": ("Service rates", "أسعار الخدمات"),
}
source = "https://api.nuget.org/v3/index.json"
work = Path(tempfile.mkdtemp(prefix="champui-skill-evaluation-"))
project = work / "SkillDashboard"
env = dict(os.environ, DOTNET_CLI_HOME=str(work / "home"), NUGET_PACKAGES=str(work / "packages"),
           NUGET_HTTP_CACHE_PATH=str(work / "http-cache"), DOTNET_NOLOGO="1")
print(f"Evidence directory: {work}", flush=True)

def run(*cmd, cwd=work):
    subprocess.run(cmd, cwd=cwd, env=env, check=True)

def insert(file, anchor, fragment):
    text = file.read_text()
    if text.count(anchor) != 1:
        raise ValueError(f"Expected one {anchor!r} in {file.name}")
    file.write_text(text.replace(anchor, fragment + "\n" + anchor))

for invalid in ['<UiScheduler />', '<UiButton InventedAction="Save" />', '<UiDialog @bind-Open="open" />']:
    try:
        check(invalid)
    except ValueError:
        continue
    raise AssertionError(f"Unexpectedly accepted: {invalid}")

config = work / "NuGet.Config"
config.write_text(f'<configuration><packageSources><clear/><add key="nuget.org" value="{source}"/></packageSources></configuration>')
skill = here.parents[1] / "skills/champui/SKILL.md"
bootstrap, = re.findall(r"^```sh\n(.*?)^```", skill.read_text(), re.M | re.S)
# Run the installed-skill instructions exactly as written before adding the scenario outputs.
run("bash", "-eu", "-o", "pipefail", "-c", bootstrap)
layout = project / "Components/Layout/MainLayout.razor"
pages = project / "Components/Pages"
count = 0
for name in scenarios:
    fixture = here / name
    page, = fixture.glob("*.razor")
    count += check(page.read_text())
    shutil.copyfile(page, pages / page.name)
    # Navigation entries use the same sidebar slot as the production guide.
    english, arabic = scenarios[name]
    link = f"""                    <UiSidebarItem Href="/skill-{name}" Text='@Preferences.Text("{english}", "{arabic}")' />"""
    count += check(link)
    insert(layout, "                    @if (Identity.ShowDeveloperGuide)", link)
    for cs in fixture.glob("*.cs"):
        shutil.copyfile(cs, project / "Services" / cs.name)
    for filename, anchor in [("registration.txt", "var app = builder.Build();"), ("endpoints.txt", "app.Run();")]:
        fragment = fixture / filename
        if fragment.exists():
            insert(project / "Program.cs", anchor, fragment.read_text())

print(f"{count} ChampUI tags matched the catalog; three invalid API controls rejected.", flush=True)
run("dotnet", "restore", "--configfile", str(config), "-warnaserror", cwd=project)
assets = json.loads((project / "obj/project.assets.json").read_text())
assert list(assets["project"]["restore"]["sources"]) == [source]
assert "ChampUI/0.3.3" in assets["libraries"]
run("dotnet", "build", "-c", "Release", "--no-restore", "-warnaserror", cwd=project)
log = work / "server.log"
with log.open("w") as output:
    host = subprocess.Popen(["dotnet", "run", "--no-build", "--no-launch-profile", "-c", "Release",
                             "--urls", "http://127.0.0.1:0"], cwd=project, env=dict(env, ASPNETCORE_ENVIRONMENT="Development"),
                            stdout=output, stderr=subprocess.STDOUT)
    try:
        for _ in range(300):
            listener = re.search(r"Now listening on: (http://127\.0\.0\.1:\d+)", log.read_text())
            if listener or host.poll() is not None:
                break
            time.sleep(0.2)
        assert listener, log
        run("node", str(here / "browser.cjs"), listener[1], *scenarios)
    finally:
        host.terminate()
        host.wait(timeout=30)

ef = work / "EfFixture"
shutil.copytree(here / "ef", ef, ignore=shutil.ignore_patterns("bin", "obj"))
run("dotnet", "restore", "--configfile", str(config), "-warnaserror", cwd=ef)
run("dotnet", "build", "-c", "Release", "--no-restore", "-warnaserror", cwd=ef)
run("dotnet", "run", "-c", "Release", "--no-build", cwd=ef)

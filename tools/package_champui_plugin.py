"""Validate the marketplace and build a deterministic, standalone ChampUI plugin ZIP."""
import argparse
import json
from pathlib import Path
from zipfile import ZipFile, ZipInfo

ROOT = Path(__file__).resolve().parents[1]
PLUGIN = ROOT / "plugins/champui"


def files(directory):
    return {p.relative_to(directory).as_posix(): p.read_bytes()
            for p in directory.rglob("*") if p.is_file()}


def require(condition, message):
    if not condition:
        raise ValueError(message)


def build(output):
    source = files(ROOT / "skills/champui")
    expected = {"skills/champui/" + name: content for name, content in source.items()}
    expected["LICENSE"] = (ROOT / "LICENSE").read_bytes()
    require("SKILL.md" in source, "Missing canonical skill")
    portable = json.loads((PLUGIN / "plugin.json").read_bytes())
    codex = json.loads((PLUGIN / ".codex-plugin/plugin.json").read_bytes())
    for field in ("name", "version", "description", "author", "license", "repository"):
        require(portable[field] == codex[field], f"Manifest mismatch: {field}")
    require(codex["interface"] == portable["extensions"]["com.openai"]["interface"],
            "Interface metadata mismatch")
    require(codex["skills"] == "./skills/", "Unexpected skill path")
    market = json.loads((ROOT / ".agents/plugins/marketplace.json").read_bytes())
    entry, = market["plugins"]
    require(market["name"] == "sadeem-champui" and entry["name"] == portable["name"] == "champui"
            and entry["source"] == {"source": "local", "path": "./plugins/champui"},
            "Marketplace entry does not resolve the packaged skill")
    for name in ("plugin.json", ".codex-plugin/plugin.json"):
        expected[name] = (PLUGIN / name).read_bytes()
    require(files(PLUGIN) == expected, "Plugin content differs from canonical skill or license")
    with ZipFile(output, "w") as archive:
        for name, data in sorted(expected.items()):
            info = ZipInfo(name)
            info.create_system = 3
            info.external_attr = 0o100644 << 16
            archive.writestr(info, data)
    with ZipFile(output) as archive:
        require({name: archive.read(name) for name in archive.namelist()} == expected,
                "Archive content mismatch")


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("output", type=Path)
    build(parser.parse_args().output)

"""Audit the quoted attributes in the skill's authored Razor fixtures against the Core catalog."""
from pathlib import Path
import re

docs = Path(__file__).resolve().parents[2] / "docs/ai/component-reference"
# Fixtures use quoted attribute values. Quoting permits spaces, lambda arrows and
# comparisons inside an attribute. Unhandled syntax must fail instead of being skipped.
attribute = re.compile(r"""\s+([@\w:-]+)\s*=\s*(?:"[^"]*"|'[^']*')""")

def check(markup):
    count = 0
    for tag in re.finditer(r"<(Ui[A-Z]\w*|ChartText)\b", markup):
        name = tag[1]
        ref = docs / f"{name}.md"
        if not ref.is_file():
            raise ValueError(f"Unknown ChampUI component: {name}")
        contract = ref.read_text()
        allowed = set(re.findall(r"^\| `(\w+)` \|", contract, re.M))
        generic = re.search(r"Generic parameters: (\w+)\.", contract)[1]
        pos = tag.end()
        while not re.match(r"\s*/?>", markup[pos:]):
            match = attribute.match(markup, pos)
            if not match:
                raise ValueError(f"Unsupported fixture attribute syntax on {name}: {markup[pos:pos+50]}")
            key = match[1]
            if key.startswith("@bind-"):
                value = key[6:]
                valid = value in allowed and value + "Changed" in allowed
            else:
                valid = key in allowed or (generic != "none" and key == generic)
                if "captures HTML attributes" in contract:
                    valid |= key.startswith(("aria-", "data-"))
            if not valid:
                raise ValueError(f"Unknown parameter: {name}.{key}")
            pos = match.end()
        count += 1
    return count

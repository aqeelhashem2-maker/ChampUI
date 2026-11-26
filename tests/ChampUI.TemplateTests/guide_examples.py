"""Compile the guide's examples in a generated project without contacting an API."""
from pathlib import Path
import re
import sys
from textwrap import dedent

project = Path(sys.argv[1])
guide = (project / "Components/Pages/GettingStarted.razor").read_text()


def example(name):
    snippet = re.search(rf'private const string {name} = """\n(.*?)\n\s*""";', guide, re.S)
    assert snippet, name
    return dedent(snippet.group(1)) + "\n"


pages = project / "Components/Pages"
(pages / "Inventory.razor").write_text(example("PageExample"))
(project / "Services/ApiDashboardData.cs").write_text(example("ApiService"))
registration = "\n".join("        " + line for line in example("ApiRegistration").splitlines())
namespace = re.search(r"namespace ([\w.]+);", example("ApiService")).group(1)
(project / "Services/ApiRegistrationExample.cs").write_text(
    f"""namespace {namespace};

internal static class ApiRegistrationExample
{{
    internal static void Register(WebApplicationBuilder builder)
    {{
{registration}
    }}
}}
"""
)
fixture = pages / "FocusAudit.razor"
fixture.write_text(fixture.read_text() + "\n" + example("NavigationExample"))

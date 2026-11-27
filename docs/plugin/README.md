# Install the ChampUI coding plugin

This review package distributes the canonical ChampUI skill for Core 0.3.3 and Dashboard Starter 0.1.3. It includes instructions, a service-integration reference and the MIT license. It uses no MCP server, hooks, credentials or remote service. Access to the component catalog, public NuGet feed and .NET 10 SDK is needed for the build workflow.

## Codex

In a checkout of this branch run:

```sh
codex plugin marketplace add "$PWD"
codex plugin add champui@sadeem-champui
codex plugin list --json
```

Check for installed and enabled, then start a fresh conversation. Ask Codex to use the ChampUI skill to create a bilingual customer page. Check the execution trace for a read of the installed SKILL.md, then build the generated application. The marketplace registration refers to the local checkout, so retain it. For project-scoped configuration on a trusted checkout, put the following in the project's .codex/config.toml, substituting its actual absolute path. For a personal installation use the CLI procedure above. Only that CLI procedure was exercised.

```toml
[marketplaces.sadeem-champui]
source_type = "local"
source = "/absolute/path/to/ChampUI"

[plugins."champui@sadeem-champui"]
enabled = true
```

The marketplace definition is in .agents/plugins/marketplace.json. The installable plugin root is plugins/champui. The plugin has both a portable manifest and a Codex compatibility manifest. [OpenAI plugin documentation](https://developers.openai.com/plugins/build/plugins)

## ChatGPT and ChatGPT Work

Create the upload file from the checkout:

```sh
python3 tools/package_champui_plugin.py champui.zip
```

The two manifests and skills directory must be at the paths inside that ZIP. In an eligible workspace with private plugin upload enabled, open the plugin management surface, upload the ZIP privately, enable the plugin and start a new conversation. Record whether the UI discovers it and invokes the skill. Availability depends on the account, plan, client and workspace administrator's plugin permissions. A missing upload control is an untested environment. Giving a chat the skill text or a Project document does not demonstrate installation. [Build plugins](https://learn.chatgpt.com/docs/build-plugins) · [Workspace controls](https://learn.chatgpt.com/docs/enterprise/apps-and-connectors)

## Claude Code

From an isolated project root, install the canonical skill, preserving its references:

```sh
mkdir -p .claude/skills
cp -R /absolute/path/to/ChampUI/skills/champui .claude/skills/
diff -qr /absolute/path/to/ChampUI/skills/champui .claude/skills/champui
claude
```

Use a fresh project without an existing champui directory. Enter `/champui Create a bilingual customer management page with a customer form`. Verify discovery, actual generation and a Release build separately. A personal skill can instead reside at ~/.claude/skills/champui. The project-local installation was exercised. Claude.ai has a separate product surface. [Claude Code skills](https://code.claude.com/docs/en/skills)

## Independent acceptance

Run each client in a separate disposable checkout. Provide the same public catalog, published template, .NET SDK and the following prompt. Keep transcripts private. Retain a sanitized outcome, model/client version, generated file inventory, Release build, and browser checks. Do not copy generated code between clients.

> Create a Blazor .NET 10 salon management dashboard with customers, employees, services, appointments and revenue KPIs. Use the official ChampUI library and its published Dashboard Starter. Support Arabic RTL and English, light/dark themes, and responsive layouts. Do not invent ChampUI components.

Check component names and parameters against docs/ai/component-reference, visit all five areas, exercise a real form, switch language and theme, inspect narrow-screen overflow and keyboard focus. Record manual fixes separately. CI replays curated examples and checks ZIP reproducibility. Live model outputs are independent of CI.

Before any future public submission, confirm publisher identity, rights, support and privacy information, obtain target-client screenshots and complete private installation and invocation in the target workspace. Submission requires a separate owner decision.

[Arabic integration audit](audit-ar.md)

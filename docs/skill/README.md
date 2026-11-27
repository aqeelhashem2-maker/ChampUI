# ChampUI coding skill

This repository distributes the [champui skill](../../skills/champui/SKILL.md) as a portable folder. Its API reference targets Core 0.3.3, Dashboard Starter 0.1.3, and .NET 10 Blazor Interactive Server. It includes a pinned catalog link for use outside the ChampUI checkout. Keep the references subfolder beside SKILL.md. The skill and library are from Sadeem Systems under the repository's [MIT license](../../LICENSE).

## Install in a project

From the consuming repository root, choose the command block for the coding environment. Set CHAMPUI_CHECKOUT to the absolute path of the ChampUI checkout containing this skill. The examples copy into an unused project-local name. Inspect an existing installation before updating it.

### Codex

```sh
mkdir -p .agents/skills
test ! -e .agents/skills/champui && cp -R "$CHAMPUI_CHECKOUT/skills/champui" .agents/skills/champui
```

Open Codex in the project, start a new session and select the skill or invoke `$champui`. For example, ask it to add a localized appointment page and run its interaction checks. Codex can also select installed skills by their descriptions. [Official skill discovery guide](https://learn.chatgpt.com/docs/build-skills)

### Claude Code

```sh
mkdir -p .claude/skills
test ! -e .claude/skills/champui && cp -R "$CHAMPUI_CHECKOUT/skills/champui" .claude/skills/champui
```

Open the project in Claude Code. Invoke `/champui` followed by a task, such as adding a customer grid with a details dialog. Check the slash menu for the installed name. Project trust and organization settings can affect discovery. [Claude Code skill locations and invocation](https://code.claude.com/docs/en/skills)

### ChatGPT project or conversation

Use the instructions as project context: attach SKILL.md and application-services.md, then attach the relevant catalog pages from docs/ai/ or supply the pinned links where browsing is available. Ask ChatGPT to follow the ChampUI workflow and to identify the source of each component contract. If the session cannot run .NET or Chromium, run the validation in the consuming repository and share its output for review. This is a manual file/instruction workflow. Publishing this folder to GitHub does not install it into ChatGPT. [Projects in ChatGPT](https://help.openai.com/en/articles/10169521-projects-in-chatgpt)

## Evaluate

The [bilingual prompts](scenarios.md) correspond to committed forward-test outputs under [ChampUI.SkillTests](../../tests/ChampUI.SkillTests). For a fresh model evaluation, provide the skill and one prompt in a clean Starter checkout before showing the reference output. Judge the application by the observable acceptance criteria. To replay the saved outputs, use .NET 10, Python 3, Node.js 18+ and the repository's pinned Playwright Chromium:

```sh
npm ci --prefix tests/ChampUI.TemplateTests
(cd tests/ChampUI.TemplateTests && npx playwright install chromium)
export PLAYWRIGHT_MODULE="$PWD/tests/ChampUI.TemplateTests/node_modules/playwright"
python3 tests/ChampUI.SkillTests/evaluate.py
python3 docs/ai/validate.py
```

On Linux, provision the browser's system dependencies as described in the root README. The evaluation script creates a new template hive, package and HTTP caches, and a generated project in a temporary directory. It adds the five pages, navigation and application-service fixtures, checks the Core API names, builds with warnings as errors, serves the application on loopback and executes the browser assertions. It also runs the EF Core in-memory service fixture. The printed evidence directory contains the generated project and server log. See the [Arabic evaluation report](audit-ar.md) for measured results and boundaries.

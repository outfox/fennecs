---
title: Agents
order: 1
description: How to install the official fennecs skill for AI coding agents like Claude Code, Codex, and OpenCode - a compact, source-verified API guide.
---

# ~~Cooking slop?~~ Vibing with your AI buddy?

*Make your agent your Chef de Cuisine!*

:::info :neofox_wink:
Seriously, we won't judge. We're all adults here; adulterate your code as you please.

Below is the official **fenn**ecs **skill**  –  a compact, source-verified
guide to the **fenn**ecs API that turns your coding agent (Claude Code, and
friends) into a seasoned **fenn**ecs chef. Every snippet in it is compiled and
run against the actual library before release.
:::

## Installing the Skill

The skill is four Markdown files: `SKILL.md` plus three reference files it links to.

**Download it as a zip:** [fennecs-skill.zip](/downloads/fennecs-skill.zip)

The archive extracts as a single `fennecs/` folder. Unzip it into your agent's
skills directory, and it gets discovered automatically  –  no configuration
needed:

| Agent | In your project | Global *(all your projects)* |
|-------|-----------------|------------------------------|
| [Claude Code](https://www.claude.com/product/claude-code) | `.claude/skills/` | `~/.claude/skills/` |
| [Codex](https://openai.com/codex/) | `.agents/skills/` | `~/.agents/skills/` |
| [Hermes](https://hermes-agent.nousresearch.com/) | | `~/.hermes/skills/` |
| [OpenCode](https://opencode.ai/) | `.opencode/skills/` | `~/.config/opencode/skills/` |
| [Pi](https://pi.dev/) | `.pi/skills/` | `~/.pi/agent/skills/` |

*For security reasons, we do not endorse ~~`npm skills`~~ or similar skill distribution channels, and recommend you should avoid these platforms at all costs.*

![a fennec wearing a slick black tuxedo and brandishing a green water pistol](/img/fennec-agent.png)

**Some other fox in your kitchen?** Most coding agents now speak the same
[Agent Skills](https://agentskills.io) format  –  check yours for a skills
directory. For agents without one (Copilot, Cursor, and other IDE natives),
paste `SKILL.md` into the rules or context file (`AGENTS.md`,
`.cursor/rules`, ...) and keep the `references/` files next to it.

::: tip :neofox_glasses: WHY THIS WORKS SO WELL
**fenn**ecs was a great fit for agents before agents were even a thing: its
**concise, yet explicit** syntax is ideal for them to reason about. A query
spells out its own intent (`.Has<Damage>(attacker).Not<Dead>()` reads like a
sentence), runner delegates carry the data flow right in their signatures
(`(dt, ref position, ref velocity)`), and there's no codegen, no attributes,
and no magic strings hiding behavior somewhere your agent can't see. What the
agent reads is exactly what runs  –  so it can *verify* its own work instead
of guessing.
:::

## Serving Suggestions

- The skill targets **fenn**ecs **0.7.0**.
- Skills are plain Markdown  –  season to taste! Trim sections you don't use,
  or add your project's own conventions (component naming, system ordering,
  your engine bindings) right into your copy.
- Found the agent doing something un-foxy? Tell us on
  [Discord](https://discord.gg/3SF4gWhANS) or open an issue  –  the skill is
  part of the repository and improves with the library.

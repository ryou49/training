# OrderHub training — agent bootstrap

This repository is an **AI coding training** workspace. Keep responses grounded in these paths.

## Map

| Path | Purpose |
| ---- | ------- |
| `training-repo/` | **Application code only** (ASP.NET Core OrderHub) |
| `AI-AGENTS/` | Modular agent rules + skills (source of truth) |
| `documents/` | Curriculum, PROCESS log, prompting guides |

## Mandatory project rules

Before implementing non-trivial work, follow the modular rules (in order):

1. `AI-AGENTS/rules/01-overview.md`
2. `AI-AGENTS/rules/02-architecture-layering.md`
3. `AI-AGENTS/rules/03-stack-and-commands.md`
4. `AI-AGENTS/rules/04-coding-conventions.md`
5. `AI-AGENTS/rules/05-safety-and-donts.md`

Human map: `AI-AGENTS/README.md`.

## Skills (preferred workflows)

| Skill | Use for |
| ----- | ------- |
| `/analyze-plan` | Explore `training-repo`, produce a plan only, stop for approval |
| `/fix-bug` | One bug: symptom → root cause gate → fix → review → regression test |
| `/code-reviewer` | Diff review against OrderHub layering conventions |
| `/test-runner` | `dotnet test` summary |

**Delivery loop:** `/analyze-plan` → (approve) → implement → `/code-reviewer` → `/test-runner`.

## Commands (app)

From `training-repo/`:

```powershell
dotnet build
dotnet test
dotnet run --project src/OrderHub.Web
```

## Hard defaults

- Controllers thin; business logic in Core services; only repositories use EF `DbContext`
- No drive-by refactors; no new NuGet packages without asking
- Do not commit unless the user asks
- Verify UI in the browser for user-facing changes; agents are not a substitute for that

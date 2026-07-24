# AI-AGENTS — OrderHub training agent kit

This folder is the **project agent configuration** for Grok (and compatible tools).  
Application code stays under `training-repo/` only.

## Layout

```text
AI-AGENTS/
├── README.md                 ← this file
├── rules/                    ← modular project memory (load by number)
│   ├── 01-overview.md
│   ├── 02-architecture-layering.md
│   ├── 03-stack-and-commands.md
│   ├── 04-coding-conventions.md
│   └── 05-safety-and-donts.md
└── skills/                   ← slash-command workflows
    ├── analyze-plan/         ← /analyze-plan
    ├── fix-bug/              ← /fix-bug
    ├── code-reviewer/        ← /code-reviewer
    └── test-runner/          ← /test-runner
```

Repo root also has:

- `AGENTS.md` — thin bootstrap Grok always loads
- `.grok/rules` and `.grok/skills` — discovery **junctions** pointing here (so slash skills work when working under `training-repo/`)

### Recreate junctions (after clone on Windows)

If `/fix-bug` etc. do not appear, from the **repo root** run:

```powershell
powershell -File AI-AGENTS/scripts/link-grok-discovery.ps1
```

Edit rules and skills **only** under `AI-AGENTS/`; do not duplicate content under `.grok/`.

## Delivery loop

```text
/analyze-plan  →  (you approve)  →  implement  →  /code-reviewer  →  /test-runner
                     ↑________________________________fix|
```

For Exercise 2 customer-complaint bugs, prefer `/fix-bug` (includes root-cause gate + regression test).

## Human verification

Agents do not replace:

1. Browser checks on the running site
2. Reading the diff yourself
3. `documents/PROCESS.md` notes for the training

## Curriculum

Training tasks and checklists live under `documents/` (especially `documents/activities/activity-guideline.md`).

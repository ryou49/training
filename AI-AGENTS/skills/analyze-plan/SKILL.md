---
name: analyze-plan
description: >
  Explore training-repo and produce an implementation plan only (no code edits).
  Use for features, refactors, or non-trivial fixes before coding.
  Use when the user runs /analyze-plan or asks to plan/analyze before implementing.
disable-model-invocation: true
---

# analyze-plan

Read-only analysis and planning for OrderHub work under `training-repo/`.

## Constraints

- **Do not** edit source, tests, config, or commit.
- **Do not** install packages or change the database schema.
- Follow `AI-AGENTS/rules/02-architecture-layering.md` and `04-coding-conventions.md`.
- Prefer existing patterns over new libraries or architectures.
- Application code lives only in `training-repo/`.

## Steps

1. **Restate the goal** in 2–4 bullets (feature, bug, or refactor). Note open questions.
2. **Explore the codebase** starting from anchors (do not random-walk the whole solution):
   - Controllers / Views / ViewModels: `training-repo/src/OrderHub.Web/`
   - Services / domain: `training-repo/src/OrderHub.Core/`
   - Repositories / EF: `training-repo/src/OrderHub.Infrastructure/`
   - Tests: `training-repo/tests/OrderHub.Tests/`
   - Pattern copies: `ProductsController`, `ProductService`, `ProductRepository`; orders via `OrderService` / `OrdersController`
3. **Describe current behavior** relevant to the goal (with file paths).
4. **Gap analysis** — what is missing or wrong vs the goal.
5. **Write the plan** using the output format below.
6. **STOP** — wait for human approval before any implementation.

## Required plan format

```markdown
## Goal
...

## Assumptions / open questions
- ...

## Current behavior (from code)
- ...

## Proposed approach
- Layer split: Controller / Service / Repository / ViewModel / View / Tests
- Why this matches OrderHub conventions

## Files to add or change
| Path | Action | Responsibility |
|------|--------|----------------|
| ...  | add/edit | ... |

## Data / queries
- How key data is loaded or computed
- N+1 or migration risk (yes/no)

## Validation & errors
- Defaults, invalid input → ModelState vs 500

## Test plan
| Proposed test | Behavior asserted |
|---------------|-------------------|
| ...           | ...               |

## Out of scope
- ...

## Implementation order
1. ...
2. ...

## Ready for coding?
STOP — wait for user approval. Do not implement in this skill.
```

## Quality bar

- Concrete file paths under `training-repo/`, not vague layer names only
- Explicit layer placement (no EF in controllers/services; thin controllers)
- At least one edge case (defaults, cancelled orders, inactive products, empty lists, etc.)
- Tests assert behavior
- No drive-by refactors unless the user asked for a refactor

## Handoff after approval

Implementation (main agent as coder) should:

1. Follow the approved plan only
2. Run `/code-reviewer` on the diff
3. Run `/test-runner`
4. Fix findings and re-test as needed
5. Ask the user to verify in the browser when UI is involved

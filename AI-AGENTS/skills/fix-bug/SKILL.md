---
name: fix-bug
description: >
  Fix one bug with the standard training loop: confirm symptom, find root cause,
  wait for approval, minimal fix, review, regression test, then commit guidance.
  Use when the user runs /fix-bug or clearly asks to fix a described bug symptom.
disable-model-invocation: true
---

# fix-bug

Fix **one** bug at a time for OrderHub (`training-repo/`).

## Inputs

Symptom description from the user (page, amounts, stock numbers, steps). If details are vague, ask for concrete observations before deep code changes.

## Steps

1. **Confirm understanding**  
   Restate the symptom, likely pages/routes, and how you will verify after the fix. Ask the user if anything is wrong.

2. **Locate root cause**  
   Trace Controller → Service → Repository (and Views/ViewModels if display-only).  
   Explain the root cause with file paths.  
   **Wait for user confirmation** before editing code.

3. **Minimal fix**  
   Change only what is required. No drive-by refactors or unrelated cleanup.  
   Follow `AI-AGENTS/rules/02-architecture-layering.md` and `04-coding-conventions.md`.

4. **Review**  
   Follow the same checklist as `/code-reviewer` on the current diff (layering, ViewModels, validation, decimal, real tests).

5. **Regression test**  
   Add a test that would have failed before the fix and passes after.  
   Run tests the same way as `/test-runner` (`dotnet test` from `training-repo`). All green required.

6. **Human UI check**  
   Tell the user exactly how to re-verify in the browser (routes and expected numbers/state).

7. **Commit (only if user asks)**  
   Message format: **symptom → root cause → fix**. One bug = one commit. Do not commit unless asked.

## Constraints

- Application code only under `training-repo/`
- Do not add NuGet packages without asking
- Do not hand-edit old EF migrations to “patch” history
- Prefer service-layer unit tests with existing `TestSetup` helpers

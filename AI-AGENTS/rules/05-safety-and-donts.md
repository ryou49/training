# 05 — Safety and don'ts

## Ask first

- Adding or upgrading **NuGet** packages
- Changing **connection strings** or environment-specific config
- Dropping/recreating databases or running destructive SQL
- Large refactors outside the current task scope
- Editing training curriculum under `documents/` unless the user asked

## Do not

- Hand-edit EF **migration history** files under `Migrations/` to “fix” schema (create a new migration if schema must change — and only when required)
- Inject or use **`DbContext`** in Controllers or Core services
- Bind **domain entities** directly to Views
- Drive-by refactor unrelated code “while you’re here”
- Commit secrets, `*.pfx`, production credentials, or user-secrets
- Force-push or rewrite published history unless the user explicitly requests it
- Invent microservices, CQRS, or new frameworks for this small training app

## Dangerous paths

| Path | Why careful |
| ---- | ----------- |
| `training-repo/src/OrderHub.Infrastructure/Migrations/**` | Migration history |
| `training-repo/src/OrderHub.Web/appsettings*.json` | Connection strings |
| Anything outside `training-repo/` for app fixes | Keep agent kit / curriculum separate unless asked |

## Commits (when user asks)

- One logical change per commit (one bug fix, one feature, one refactor)
- Bug fix messages: **symptom → root cause → fix**
- Do not commit unless the user asks

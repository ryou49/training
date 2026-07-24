# 01 — Overview

## What this is

**OrderHub** is a small internal order management system used for **AI agent coding training**.

Staff can:

- List / create / cancel orders
- Browse products (including stock) and customers
- View a customer’s orders

## Scale and non-goals

- Single app, single SQL Server database (`OrderHubTraining` by default)
- Not multi-tenant, not high-scale microservices
- Prefer simple, conventional ASP.NET Core MVC patterns over over-engineering

## Repo map

| Path | Role |
| ---- | ---- |
| `training-repo/` | **Application code only** (.sln, src, tests) |
| `AI-AGENTS/` | Agent rules and skills |
| `documents/` | Training curriculum, PROCESS log, references |

When implementing features or fixing bugs, **change code under `training-repo/` only** unless the user explicitly asks to update agent docs or training documents.

# 03 — Stack and commands

## Stack

- **.NET 8** / ASP.NET Core MVC (Razor Views) + Bootstrap 5 (local static files)
- **EF Core 8** + **SQL Server** for the web app
- **xUnit** for tests (EF Core **InMemory** — tests do not need SQL Server)

## Database (web app)

- Provider: SQL Server via `UseSqlServer` in `OrderHub.Web/Program.cs`
- Default DB name: `OrderHubTraining`
- Connection string: `training-repo/src/OrderHub.Web/appsettings.Development.json` (`ConnectionStrings:Default`)
- On startup: `Database.Migrate()` then seed (20 customers, 50 products, 200 orders)

## Commands

Run from **`training-repo/`** (or pass full project paths):

```powershell
dotnet build
dotnet test
dotnet run --project src/OrderHub.Web
```

Typical HTTP URL (profile `http`): `http://localhost:5150`

Useful pages:

| Page | Route |
| ---- | ----- |
| Orders | `/Orders` |
| Create order | `/Orders/Create` |
| Order details | `/Orders/Details/{id}` |
| Products | `/Products` |
| Customers | `/Customers` |

## Tests

```powershell
dotnet test
```

No SQL Server required for tests. Prefer adding service-layer unit tests next to existing files under `tests/OrderHub.Tests/`.

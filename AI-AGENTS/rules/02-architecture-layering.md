# 02 — Architecture and layering

## Projects (under `training-repo/`)

```text
src/
├── OrderHub.Web/            # MVC: Controllers, ViewModels, Views (wiring + display)
├── OrderHub.Core/           # Domain, service interfaces, business logic
└── OrderHub.Infrastructure/ # EF Core DbContext, repositories, migrations, seed
tests/
└── OrderHub.Tests/          # xUnit (EF InMemory — no SQL Server required)
```

## Layer duties

| Layer | Does | Does not |
| ----- | ---- | -------- |
| **Web** | HTTP, model binding, `ModelState`, map to/from ViewModels, call services, TempData messages | Business rules, EF queries, discount math |
| **Core services** | Validation, discounts, stock rules, status transitions; return `ServiceResult<T>` | Touch `DbContext` directly; render HTML |
| **Infrastructure** | EF Core, repositories, migrations, seeder | UI decisions; HTTP concerns |
| **Tests** | Service-level behavior with InMemory DB | Depend on real SQL Server |

## Hard rules

1. **Controllers stay thin** — orchestrate only; logic lives in Core services.
2. **Only repositories** (Infrastructure) use `DbContext`. Controllers and services must not inject or use `DbContext`.
3. **Services** depend on repository **interfaces** from Core (`ICustomerRepository`, etc.).
4. **Views bind ViewModels**, never domain entities directly. Mapping is hand-written in the controller (or small helper), not AutoMapper unless already introduced.
5. Follow existing naming and folder layout when adding features.

## Reference implementations

Copy style from:

- Controller: `training-repo/src/OrderHub.Web/Controllers/ProductsController.cs`
- Service: `training-repo/src/OrderHub.Core/Services/ProductService.cs`
- Repository: `training-repo/src/OrderHub.Infrastructure/Repositories/ProductRepository.cs`

For order rules (create/cancel/pricing): `OrderService` + `OrdersController`.

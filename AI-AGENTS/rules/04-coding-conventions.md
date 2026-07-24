# 04 — Coding conventions

## Results and errors

- Services return **`ServiceResult` / `ServiceResult<T>`** for expected failures (validation, not found, business rules). Do not throw for normal business failures.
- Controllers map failed `ServiceResult` to user-visible errors (ModelState / TempData), not unhandled 500s.
- User input: **DataAnnotations + ModelState**. Invalid input must never become a 500 error page.

## Money and discounts

- Use **`decimal`** for money — never `float` / `double`.
- Membership tiers: Standard (none), Silver (5% off), Gold (10% off). Keep discount logic centralized in order pricing code; do not reimplement ad hoc in views or multiple places.

## UI patterns

- Success/error flashes: `TempData["Success"]` / `TempData["Error"]` (shared layout alerts).
- POST actions use anti-forgery tokens.
- Prefer GET forms for filter/query pages (query string parameters), matching existing list pages.

## New feature checklist

When adding a page or capability, touch the layers that already own those concerns:

1. Domain / DTOs if needed (Core)
2. Repository interface + implementation (Core interface, Infrastructure impl)
3. Service interface + implementation (Core)
4. Controller action (Web)
5. ViewModel + View (Web)
6. Nav link in `_Layout.cshtml` if user-facing
7. Unit tests (Tests)

## Tests

- Assert **behavior** (real assertions), not tautologies.
- Regression tests for bugs should fail before the fix and pass after.
- Prefer service-level tests with `TestSetup` helpers already in the test project.

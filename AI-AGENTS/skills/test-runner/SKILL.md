---
name: test-runner
description: >
  Run OrderHub unit tests with dotnet test and report a short summary.
  Use when verifying a fix or feature, or when the user runs /test-runner.
disable-model-invocation: false
---

# test-runner

Run the OrderHub test suite and report a concise result.

## Steps

1. Run tests from the app root:

   ```powershell
   dotnet test training-repo/OrderHub.sln
   ```

   If already inside `training-repo/`:

   ```powershell
   dotnet test
   ```

2. **On success**  
   Report only: total tests passed (and duration if available). No full log dump.

3. **On failure**  
   List:
   - Failed test names
   - Assertion / error messages
   - Brief likely cause (file or area if obvious)

   Do **not** paste the entire test output.

4. **Fixes**  
   Do not change production or test code unless the user asks you to fix failures. This skill’s default job is **report**.

## Notes

- Tests use EF InMemory; SQL Server is not required.
- Prefer `training-repo` as the working tree for `dotnet` commands.

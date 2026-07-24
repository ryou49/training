# Local Grok discovery (not the source of truth)

Skills and rules live under `../AI-AGENTS/`.

After clone on Windows, create junctions:

```powershell
powershell -File AI-AGENTS/scripts/link-grok-discovery.ps1
```

This makes `/analyze-plan`, `/fix-bug`, `/code-reviewer`, and `/test-runner` discoverable while editing app code under `training-repo/`.

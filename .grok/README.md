# 本機 Grok 探索目錄（不是真相來源）

**English below.**

`.grok/rules` 與 `.grok/skills` 應為指向 `AI-AGENTS/` 的 **junction（目錄連接）**。  
**請勿在此目錄直接維護規則或 skill 正文**——單一真相來源是：

```text
AI-AGENTS/rules/
AI-AGENTS/skills/
```

## 為何需要這個資料夾

Grok 會在 repo 的 `.grok/skills`、`.grok/rules` 尋找專案 skills／規則。  
本專案把內容放在 `AI-AGENTS/`（與 `training-repo/` 應用程式碼分離），再用 junction 讓 Grok 找得到。

## Clone 後（Windows）請重建 junction

```powershell
# 在 repo 根目錄執行
powershell -File AI-AGENTS/scripts/link-grok-discovery.ps1
```

成功後應可使用：

| 指令 | 用途 |
| ---- | ---- |
| `/analyze-plan` | 只出計畫 |
| `/fix-bug` | 修 bug 流程 |
| `/code-reviewer` | 分層 code review |
| `/test-runner` | 跑測試並摘要 |

更完整說明見 [AI-AGENTS/README.md](../AI-AGENTS/README.md) 與根目錄 [README.md](../README.md)。

---

# Local Grok discovery (not the source of truth)

`.grok/rules` and `.grok/skills` should be **junctions** into `AI-AGENTS/`.  
**Do not maintain rule or skill bodies here** — the single source of truth is:

```text
AI-AGENTS/rules/
AI-AGENTS/skills/
```

## Why this folder exists

Grok discovers project skills/rules under `.grok/skills` and `.grok/rules`.  
This repo keeps content in `AI-AGENTS/` (separate from `training-repo/` app code) and uses junctions so Grok can still load them.

## After clone on Windows, recreate junctions

```powershell
# From the repository root
powershell -File AI-AGENTS/scripts/link-grok-discovery.ps1
```

You should then get:

| Command | Purpose |
| ------- | ------- |
| `/analyze-plan` | Plan only |
| `/fix-bug` | Bug-fix workflow |
| `/code-reviewer` | Layering review |
| `/test-runner` | Run tests, short summary |

See [AI-AGENTS/README.md](../AI-AGENTS/README.md) and root [README.md](../README.md) for the full Grok vs Claude/Codex story.

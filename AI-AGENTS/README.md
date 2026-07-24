# AI-AGENTS — OrderHub 培訓用 Agent 設定

**English follows each section.**

本目錄是專案的 **Grok（及相容工具）agent 設定**，與應用程式碼分離。  
應用程式只放在 `training-repo/`。

This folder is the **project agent configuration** for Grok (and compatible tools).  
Application code stays under `training-repo/` only.

---

## 為何用 Grok，而不是課綱的 Claude / Codex

上游培訓預設 **Claude Code**（`CLAUDE.md`、`.claude/`）或 **Codex**（`AGENTS.md`、`.codex/`、`.agents/skills`）。  
**本 fork 以 Grok Build 為主**，並用更模組化、可版控的結構承載同等（或更強）的訓練目標。

| 項目 | 課綱主流 | 本目錄（Grok） |
| ---- | -------- | -------------- |
| 工具 | Claude Code / Codex CLI | **Grok Build** |
| 專案記憶 | 單一 `CLAUDE.md` 或 `AGENTS.md`（常塞在 app 旁） | 根目錄薄 `AGENTS.md` + **本目錄 `rules/01–05`** |
| Skills | `.claude/skills` 或 `.agents/skills` | **`skills/`** → 經 junction 給 `.grok/skills` 發現 |
| 與程式碼 | 設定常與 `.sln` 混在 `training-repo/` | **`training-repo/` code-only**；設定只在這裡 |
| 工作流 | 偏 fix-bug | **plan → 實作 → review → test**（含 `/analyze-plan`） |

### Why Grok instead of curriculum Claude / Codex

Upstream training defaults to **Claude Code** or **Codex**.  
**This fork uses Grok Build**, with a more modular, version-controlled layout for the same (or stronger) goals.

| Item | Curriculum default | This folder (Grok) |
| ---- | ------------------ | ------------------ |
| Tool | Claude Code / Codex CLI | **Grok Build** |
| Project memory | Single `CLAUDE.md` / `AGENTS.md` (often next to the app) | Root thin `AGENTS.md` + **`rules/01–05` here** |
| Skills | `.claude/skills` or `.agents/skills` | **`skills/`** → discovered via `.grok/skills` junctions |
| Vs app code | Config often mixed into `training-repo/` | **`training-repo/` code-only**; agent config only here |
| Workflow | Mostly fix-bug oriented | **plan → implement → review → test** (includes `/analyze-plan`) |

---

## 比課綱預設更進階之處

1. **模組化規則** — 不是一份巨石 CLAUDE.md；可分段維護、降低每次 context 噪音。  
2. **App 與 agent 設定分離** — review 程式 diff 不會混進大量 agent 設定。  
3. **`/analyze-plan`** — 課綱未強制的「先計畫、人工核准再動手」。  
4. **完整交付迴圈** — 寫進 skills／根 `AGENTS.md`，可重複執行。  
5. **細切 Bug / Feature commit** — Unit Test → Core → Infra → Web 等，歷史可審。  
6. **單一真相來源** — 只編輯本目錄；`.grok` 用 junction，避免重複 commit。

### Where this is more advanced than the default guides

1. **Modular rules** — not one monolithic CLAUDE.md; lower context noise.  
2. **App vs agent separation** — app diffs stay clean.  
3. **`/analyze-plan`** — plan-first with human approval (beyond minimal syllabus).  
4. **Full delivery loop** — encoded in skills and root `AGENTS.md`.  
5. **Fine-grained Bug / Feature commits** — test → layers → UI; reviewable history.  
6. **Single source of truth** — edit only here; `.grok` junctions avoid duplicate commits.

課綱權限檔與 hooks（擋危險 SQL、log 編輯）本目錄**尚未**接成 Grok `.grok/hooks`；腳本參考見 `documents/activities/scripts/`。  
Permissions and hooks from the syllabus are **not** wired as Grok project hooks yet; scripts live under `documents/activities/scripts/`.

---

## 目錄配置 / Layout

```text
AI-AGENTS/
├── README.md                 ← 本檔 / this file
├── rules/                    ← 模組化專案記憶（依編號）/ modular project memory
│   ├── 01-overview.md
│   ├── 02-architecture-layering.md
│   ├── 03-stack-and-commands.md
│   ├── 04-coding-conventions.md
│   └── 05-safety-and-donts.md
├── skills/                   ← 斜線指令流程 / slash-command workflows
│   ├── analyze-plan/         ← /analyze-plan
│   ├── fix-bug/              ← /fix-bug
│   ├── code-reviewer/        ← /code-reviewer
│   └── test-runner/          ← /test-runner
└── scripts/
    └── link-grok-discovery.ps1
```

Repo 根目錄另有 / Repo root also has:

- `AGENTS.md` — Grok 每次 session 會載入的薄引導 / thin bootstrap Grok always loads  
- `.grok/rules`、`.grok/skills` — 指向本目錄的 **junction**，方便技能發現 / discovery **junctions** into this folder  

### 重建 junction（Windows clone 後）/ Recreate junctions (after clone on Windows)

若 `/fix-bug` 等指令沒出現，在 **repo 根目錄** 執行 / If slash skills do not appear, from the **repo root** run:

```powershell
powershell -File AI-AGENTS/scripts/link-grok-discovery.ps1
```

**只編輯 `AI-AGENTS/` 下的 rules 與 skills**，不要在 `.grok/` 複製一份內容。  
Edit rules and skills **only** under `AI-AGENTS/`; do not duplicate content under `.grok/`.

---

## 交付迴圈 / Delivery loop

```text
/analyze-plan  →  (你核准 / you approve)  →  實作 / implement  →  /code-reviewer  →  /test-runner
                        ↑______________________________________________|
```

| 指令 / Command | 用途（中） | Purpose (EN) |
| -------------- | ---------- | ------------ |
| `/analyze-plan` | 只出計畫、不改檔 | Plan only, no edits |
| `/fix-bug` | 修 bug 標準流程 | Bug fix loop with root-cause gate |
| `/code-reviewer` | 依 OrderHub 分層審 diff | Layering checklist on the diff |
| `/test-runner` | 精簡 `dotnet test` 報告 | Concise test summary |

練習 2 客訴 bug 優先用 `/fix-bug`（含根因確認與回歸測試）。  
For Exercise 2 customer-complaint bugs, prefer `/fix-bug` (root-cause gate + regression test).

---

## 人工驗證 / Human verification

Agent **不能取代** / Agents do not replace:

1. 在跑起來的網站上用瀏覽器確認 / Browser checks on the running site  
2. 自己讀 diff / Reading the diff yourself  
3. 培訓心得寫入 `documents/PROCESS.md` / Notes in `documents/PROCESS.md`  

---

## 課綱位置 / Curriculum

練習任務與檢查清單在 `documents/`（尤其 `documents/activities/activity-guideline.md`）。  
更完整的「Grok vs Claude/Codex」說明見 **repo 根目錄 [README.md](../README.md)**。  

Training tasks and checklists live under `documents/` (especially `documents/activities/activity-guideline.md`).  
Broader Grok vs Claude/Codex notes: root **[README.md](../README.md)**.

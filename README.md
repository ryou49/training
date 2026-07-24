# OrderHub

練習說明入口請看 **[documents/README.md](documents/README.md)**  
心得紀錄：**[documents/PROCESS.md](documents/PROCESS.md)**  
應用程式碼：`training-repo/`（保持 code-only）

## 本 fork 的 Agent 路線：Grok（非 Claude / Codex 主流路徑）

上游培訓指南預設 **Claude Code**（`CLAUDE.md`、`.claude/`）或 **Codex**（`AGENTS.md`、`.codex/`、`.agents/skills`）。  
**本 fork 改以 [Grok](https://x.ai) / Grok Build CLI 為主**，並把 agent 設定做成可版控、可細切、比課綱預設更進階的結構。

### 為什麼用 Grok 而不是課綱的 Claude / Codex

| | 課綱主流（Claude / Codex） | 本 fork（Grok） |
| --- | --- | --- |
| 工具 | Claude Code 或 Codex CLI | **Grok Build**（skills、plan mode、subagents） |
| 專案記憶 | 單一 `CLAUDE.md` 或 `AGENTS.md`（常放在 `training-repo/`） | 根目錄薄 **`AGENTS.md`** + **`AI-AGENTS/rules/01–05`** 模組化 |
| Skills | `.claude/skills` 或 `.agents/skills` | **`AI-AGENTS/skills/`** → `.grok/skills` junction 供 Grok 發現 |
| 與程式碼混放 | 設定常塞在 `training-repo/` 與 .sln 同層 | **`training-repo/` 只放 app**；agent 產物獨立在 `AI-AGENTS/` |
| 工作流 | 課綱以 fix-bug 為主 | **plan → code → review → test** 完整迴圈（含 `/analyze-plan`） |

課綱文件仍可參考：

- Claude：[documents/references/agent-configuration.md](documents/references/agent-configuration.md)
- Codex：[documents/references/agent-configuration-codex.md](documents/references/agent-configuration-codex.md)

**對 Grok 請以本 README + [AI-AGENTS/README.md](AI-AGENTS/README.md) 為準。**

### 本方案比課綱預設更進階的地方

1. **模組化專案記憶（不是一份巨石 CLAUDE.md）**  
   慣例拆成 `01-overview` … `05-safety`，可依任務加載、降低 token 噪音，也方便只改一層規則。

2. **Code-only 應用目錄**  
   `training-repo/` 不混 `.claude` / skills；agent 設定集中 `AI-AGENTS/`，review app diff 更乾淨。

3. **比課綱多一層「先計畫」skill**  
   除 `/fix-bug`、`/code-reviewer`、`/test-runner` 外，有 **`/analyze-plan`**（唯讀探索 + 實作計畫 + 等人 approve），對齊練習 3 與大型功能。

4. **交付迴圈寫進設定**  
   ```text
   /analyze-plan → (人工核准) → 實作 → /code-reviewer → /test-runner → UI 驗證 → 細切 Feature/Bug commit
   ```
   不是「一句話叫 agent 改完」。

5. **細切 commit 紀律（Bug / Feature 前綴）**  
   例：Unit Test 先 → Core → Infrastructure → Web；或症狀／根因分 commit。歷史可審、可回溯，優於「一次 commit 塞滿整包設定」。

6. **Grok 原生發現機制**  
   根 `AGENTS.md` 自動載入；`.grok/rules` / `.grok/skills` 以 **junction** 指到 `AI-AGENTS`（單一真相來源，避免 Git 把 junction 內容重複 commit）。

7. **實作成果超出課綱最小集合**  
   除練習 1–3 外，延伸 **商品管理**（新增、列表改 SKU／名稱／庫存／狀態、狀態篩選），並用 PROCESS 記錄改善前後。

### 本機啟用 Grok agent 設定

從 **repo 根目錄**（有 `AGENTS.md` 與 `AI-AGENTS/` 的那層）開 Grok：

```powershell
# 若 slash skills 沒出現，重建 discovery junctions（Windows）
powershell -File AI-AGENTS/scripts/link-grok-discovery.ps1
```

| 指令 | 用途 |
| ---- | ---- |
| `/analyze-plan` | 只出計畫、不改檔 |
| `/fix-bug` | 修 bug 標準流程 |
| `/code-reviewer` | 依 OrderHub 分層審 diff |
| `/test-runner` | `dotnet test` 精簡報告 |

### 與 Claude / Codex 對照：我們刻意沒照抄的部分

| 課綱項目 | 本 fork |
| -------- | ------- |
| `training-repo/CLAUDE.md` | 用根 `AGENTS.md` + `AI-AGENTS/rules` 取代 |
| `.claude/settings.json` 權限 | **尚未**建 Grok 專案權限檔（可後補） |
| Pre/Post hooks（擋 SQL、log 編輯） | 腳本在 `documents/activities/scripts/`，**尚未**接成 `.grok/hooks` |
| Claude subagent 檔 | 改以 **skills**（效果同等訓練目標，sandbox 子代理可選） |

權限與 hooks 是下一步可加強的硬安全層；**專案記憶與 skills 已優於課綱單檔／單工具預設。**

### 目錄一覽（agent 相關）

```text
training/                          ← git root，在此開 Grok
├── AGENTS.md                      ← Grok 自動載入的薄引導
├── AI-AGENTS/
│   ├── rules/01–05                ← 模組化專案慣例
│   ├── skills/                    ← analyze-plan, fix-bug, code-reviewer, test-runner
│   └── scripts/link-grok-discovery.ps1
├── .grok/                         ← rules/skills junctions（gitignore 本體內容）
├── documents/                     ← 課綱、PROCESS、Claude/Codex 參考
└── training-repo/                 ← OrderHub 應用（無 agent 設定雜訊）
```

---

## 練習規則

請 fork 專案到自己的帳號進行練習。

## Fork 流程

1. 點右上角 **Fork** 建立自己帳號下的複本。

2. Clone 你 fork 出來的專案並進入目錄（把 `你的帳號` 換成你的 GitHub 帳號）：

   ```powershell
   git clone https://github.com/你的帳號/traning.git
   cd traning
   ```

3. 在你的 fork 上進行練習並 commit：

   ```powershell
   git add .
   git commit -m "你的 commit 訊息"
   ```

4. 推上你的 fork：

   ```powershell
   git push
   ```

## 同步原專案最新內容

當原專案 `main` 有更新時，用以下步驟把最新內容拉進你的 fork。

1. 加上原專案為 `upstream` 遠端（只需設定一次，`git remote -v` 可確認）：

   ```powershell
   git remote add upstream https://github.com/sox6769/traning.git
   ```

2. 抓取原專案最新內容並合併到本地 `main`：

   ```powershell
   git switch main
   git fetch upstream
   git merge upstream/main
   ```

   ⚠️ 若有衝突，Git 會列出衝突檔案，解完後 `git add .` 再 `git commit` 完成合併。

3. 把同步後的 `main` 推回你的 fork：

   ```powershell
   git push
   ```

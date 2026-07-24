# PROCESS.md — 我的練習心得

> 一個原則：**寫「具體發生的事」，不寫感想文。**
> 貼上當時真實的 prompt、真實的數字、真實的錯誤訊息——三個月後的你（和你的同事）才用得上。

#### 使用的 agent 與模型：

- **工具**：Grok Build（CLI / TUI）
- **模型**：Grok（session 預設模型）
- **專案設定**：`AGENTS.md` + `AI-AGENTS/rules/01–05` + skills（`analyze-plan` / `fix-bug` / `code-reviewer` / `test-runner`）
- **執行環境**：Windows、VS2022 Debug / `dotnet run`；資料庫 SQL Server 預設實例 `localhost`，資料庫名 `OrderHubTraining`（種子：客戶 20、商品 50、訂單 200）
- **基準測試**（Phase 0 `/test-runner`）：`dotnet test training-repo/OrderHub.sln` → **Passed 28 / Failed 0 / Total 28**（約 2s）

---

## 通用四問

### 1. 我的任務拆解

（開工前你把任務拆成哪幾步？實際做的時候順序有變嗎？為什麼變？）

**原計畫（練習 1）：**

1. 確認網站與 SQL Server 可跑  
2. 依工具設定 agent（對應 Grok 的 AGENTS / skills）  
3. 讓 agent 說明分層與建單流程並人工核對  
4. 寫入 PROCESS.md 並 commit  

**實際順序有調整：**

1. 先讀 `documents/` 與 `activity-guideline` 弄清「這是 AI 培訓不是產品交付」  
2. 確認 DB：不是 LocalDB，而是 `Server=localhost;Database=OrderHubTraining`（本機 `MSSQLSERVER` 已在跑）  
3. **先規劃再實作** agent 目錄：決定放 `training/AI-AGENTS/`（`training-repo/` 保持 code-only）、規則拆成多檔、skills 含 `analyze-plan`  
4. 實作 → 拆成 **7 個 commit** 推上 fork（`ryou49/training`）  
5. 最後才補 PROCESS 與 `/test-runner` 基準  

**為什麼變：** 環境與「agent 設定要怎麼 commit、Grok 如何發現 skills」若不先定案，後面修 bug 會反覆改路徑；所以把 Exercise 1 的設定做完整，再進練習 2。

### 2. AI 幫上大忙的地方

（哪件事 agent 做得又快又好？**貼上當時的提問原文**，說明為什麼這樣問有效。）

**有效提問範例 1（釐清資料庫，避免自己猜）：**

> 幫我確認現在 Debug 跑起來是連哪個資料庫？我覺得資料庫好像已經在跑了。

Agent 對照 `appsettings*.json`、`Program.cs` 的 `UseSqlServer`，並用 `sqlcmd` 查到 `OrderHubTraining` 已 ONLINE 且列數為 20/50/200。  
**為什麼有效：** 問的是「連線事實 + 如何驗證」，不是「SQL Server 是什麼」；agent 能讀設定又查本機服務。

**有效提問範例 2（先計畫再動手，對齊培訓要求）：**

> 我們應該建立多個 AGENTS.md 而不是單一巨大檔，放到 `training/AI-AGENTS/`，`training-repo` 只放 code。先 plan 再實作。

後來又補：

> 再加一個 skill：先分析 training-repo 做 planning，然後 coder / reviewer / tester 迴圈。

產出：`rules/01–05` + 四個 skills 的完整目錄與內容，並用 junction 解決 Grok 只掃 `.grok/skills` 的限制。  
**為什麼有效：** 先約束**目錄邊界**與**多檔策略**，再要實作，減少一次生成塞進 `training-repo/` 的走鐘。

### 3. AI 誤導我的地方，與我如何發現

（agent 說錯／改錯／過度自信的時刻。你靠什麼抓到——對照程式碼？頁面實測？跑測試？）

**1）文件／agent 對「折扣只算一次、集中在 CalculateTotal」的簡化**

- 培訓文件與 agent 設定常見說法：會員折扣在**訂單總額折一次**，且「折扣集中在 `OrderService.CalculateTotal`」。  
- **對照程式碼後發現不精確：**
  - `CreateOrderAsync` 在 **Gold** 時已對 `UnitPriceSnapshot` 先乘折扣（約 L75–79）。  
  - `CalculateTotal` 又對 **整筆 subtotal** 依客戶等級再乘 `(1 - GetDiscountRate)`（約 L134–138）。  
  - 因此「只在 CalculateTotal 算一次」**不符合現況**；Gold 路徑可能與文件描述的「總額折一次」不一致（後續練習 2 客訴 2 很可能相關）。  
- **如何發現：** 讀 `OrderService.cs`，不要只信 `AGENTS.md` / README 的折扣一句話。

**2）Git 與 Windows junction**

- 若直接 `git add .grok/`，Git 會**跟著 junction 走進** `AI-AGENTS`，把 rules/skills **重複加入** `.grok/` 路徑。  
- **如何發現：** `git add -n .grok/` 預覽出現完整 duplicate 檔案列表。  
- **處理：** 只 commit `AI-AGENTS/` 為單一真相；`.gitignore` 忽略 `.grok/rules/`、`.grok/skills/`；用 `link-grok-discovery.ps1` 本機重建 junction。

### 4. 我會帶回日常工作的一招

（一個具體、可複製的做法，不要寫「要多驗證」這種口號——寫出**操作步驟**。）

**招式：Agent 設定「根薄 + 模組規則 + 可 commit 的 skills」，code 目錄不塞設定**

1. Repo 根放薄的 `AGENTS.md`（地圖、硬性規則路徑、skill 列表）。  
2. 慣例拆到 `AI-AGENTS/rules/01-….md`（概述／分層／指令／慣例／安全），避免單一 400 行檔。  
3. 重複流程做成 skill：`analyze-plan`（只規劃）、`fix-bug`、`code-reviewer`、`test-runner`。  
4. 應用程式只在 `training-repo/`；agent 產物不混進 .sln 樹。  
5. 每個邏輯塊**分開 commit**（bootstrap → rules → 各 skill → 工具腳本），方便 review 與回溯。  
6. 動手前先對「文件一句話」開程式核對（例如折扣到底算在哪）。

---

## 自我驗證（做到哪個階段答哪題）

### 第一階段 — Agentic Coding

練習 1

1. 我能不看筆記說出三個專案（Web/Core/Infrastructure）各自的職責  
   - **是。**  
   - **Web**：Controller / ViewModel / View，薄轉接與畫面。  
   - **Core**：Domain、service 介面與商業邏輯（建單驗證、庫存扣減、狀態、金額計算）。  
   - **Infrastructure**：EF `DbContext`、Repository、Migration、Seeder。  

2. 我核對過 agent 描述的建單流程，且**至少找出一處不精確或過度簡化的說法**  
   - **是。**  
   - **實際建單流程（精簡）：**  
     `OrdersController.Create (POST)` → ModelState → `OrderService.CreateOrderAsync` → 驗證客戶/明細/庫存 → 扣庫存 → 寫入 `UnitPriceSnapshot` → `IOrderRepository.Add/Save` → 導向 Details。  
   - **不精確處：** 「折扣只在 `CalculateTotal`、訂單總額折一次」——實際上 Gold 在建單時已寫入折後單價，列表/明細的 `CalculateTotal` 又可能再套等級折扣（見上節「AI 誤導」）。  

3. 我知道商業邏輯應該放在哪一層、新增頁面要動哪些地方  
   - **是。**  
   - 商業邏輯 → **Core service**；EF → **Infrastructure repository**；HTTP/畫面 → **Web**。  
   - 新頁面典型動到：Repository 介面+實作 → Service 介面+實作 → Controller → ViewModel/View →（可選）`_Layout` 導覽 → `OrderHub.Tests` 測試。  
   - 已用 skill：`/analyze-plan`（規劃）、`/code-reviewer`、`/test-runner`；修 bug 用 `/fix-bug`。

練習 2

1. 三個 bug 我都先在頁面上重現過，才開始找程式  
2. 我給 agent 的資訊包含具體觀察（頁碼／金額數字／庫存數字），而不是只貼客訴原文  
3. 每個修復都回到頁面驗證過症狀消失  
4. 每個 bug 都補了一個回歸測試，`dotnet test` 全綠  
5. 三個獨立 commit，message 說明症狀與根因  
6. （思考題）為什麼原本的測試沒抓到這三個 bug？

（尚未開始——下一步：客訴 1 訂單列表分頁。）

練習 3

1. `/Products/LowStock` 不帶參數 → 門檻 10 的結果；帶 `?threshold=3` → 結果隨之改變  
2. `?threshold=0`、`?threshold=-1` → 頁面顯示驗證錯誤，不是 500  
3. 售出數量欄位排除了 Cancelled 訂單（可用一筆已取消的訂單驗證）  
4. 停售（已停售 badge）商品不出現在列表  
5. 程式分層與命名跟既有的 Products 功能一致（請 agent 自我 review 一次，並自己確認）  
6. 至少 3 個新測試，`dotnet test` 全綠  

（尚未開始。）

練習 4

1. 重構後 `dotnet test` 全綠  
2. 我能說出這次重構「改善了什麼、沒有改變什麼」  
3. 我有在 code review 的角度看過 diff（不是 agent 說好就好）  

（尚未開始。）

---

## 附錄：值得留下的對話片段

### 片段 A — 確認 DB（具體、可查證）

- **我問：** Debug 到底連哪個資料庫？看起來 DB 已經在跑。  
- **Agent 答（摘要）：** `UseSqlServer` + `Server=localhost;Database=OrderHubTraining`；本機 `MSSQLSERVER` Running；查詢確認庫存在且 20/50/200 筆種子。  
- **重點：** 要求「設定檔 + 本機查證」，不要只聽預設 LocalDB 教學。

### 片段 B — Agent 目錄與 skills 規劃

- **我問：** 多檔 AGENTS、放 `AI-AGENTS/`、code-only `training-repo`；再加 analyze-plan + coder/reviewer/tester 迴圈。先 plan。  
- **Agent 答（摘要）：** 根 `AGENTS.md` 薄引導；`rules/01–05`；skills 四個；`.grok` junction 對 `AI-AGENTS` 做 discovery；勿把 junction 內容重複 commit。  
- **實作結果：** 7 個結構化 commit 已 push 至 `origin/main`。

### 片段 C — Phase 0 `/test-runner`

- **指令：** `dotnet test training-repo/OrderHub.sln`  
- **結果：** **28 通過，0 失敗**（基準線，練習 2 開始前）。

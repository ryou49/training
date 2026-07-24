# PROCESS.md — 我的練習心得

> 一個原則：**寫「具體發生的事」，不寫感想文。**
> 貼上當時真實的 prompt、真實的數字、真實的錯誤訊息——三個月後的你（和你的同事）才用得上。

#### 使用的 agent 與模型：

- **工具**：Grok Build（CLI / TUI）
- **模型**：Grok（session 預設模型）
- **專案設定**：`AGENTS.md` + `AI-AGENTS/rules/01–05` + skills（`analyze-plan` / `fix-bug` / `code-reviewer` / `test-runner`）
- **執行環境**：Windows、VS2022 Debug / `dotnet run`；資料庫 SQL Server 預設實例 `localhost`，資料庫名 `OrderHubTraining`（種子：客戶 20、商品 50、訂單 200）
- **測試基準演進**：
  - 練習 1 結束：`dotnet test` → **28** 通過  
  - 練習 2 結束：`dotnet test` → **36** 通過（+8 回歸／強化測試）  
  - 練習 3 結束：`dotnet test` → **39** 通過（+3 LowStock service 測試）

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
4. 實作 → 拆成多個 commit 推上 fork（`ryou49/training`）  
5. 補 PROCESS 與 `/test-runner` 基準  
6. **練習 2**：每個 bug 皆 UI 重現 → 分析 → Unit Test 先 → 修 production → UI 確認 → 分 commit push  
7. **練習 3**：先確認 `/Products/LowStock` **404 是功能未做** → `/analyze-plan` → 人工改計畫（低庫存做在商品頁 filter，不塞頂部導覽）→ 實作 → 5 個 **Feature** commit push  

**為什麼變：** 環境與 agent 設定路徑若不先定案會反覆改；修 bug 時堅持「先測再修、一 bug 多 commit」；新功能堅持 **先 plan 再 code**，並依實際 UX 改寫規格中的導覽方式。

### 2. AI 幫上大忙的地方

（哪件事 agent 做得又快又好？**貼上當時的提問原文**，說明為什麼這樣問有效。）

**有效提問範例 1（釐清資料庫）：**

> 幫我確認現在 Debug 跑起來是連哪個資料庫？我覺得資料庫好像已經在跑了。

→ 對上 `localhost` / `OrderHubTraining`，並用 sqlcmd 驗證 20/50/200 種子。

**有效提問範例 2（agent 設定先 plan）：**

> 我們應該建立多個 AGENTS.md…放到 `training/AI-AGENTS/`…再加 analyze-plan…

→ 產出模組規則 + 四 skills + junction 策略。

**有效提問範例 3（練習 2 給具體 UI 觀察，不是只貼客訴）：**

> Human driven test：訂單 #201 建立後 /Orders 第一頁沒有；最後一頁空白；已取消篩選也空白。

> Gold 用原價對應付總額；手算原價×0.9。

→ Agent 能對到 `Skip(page * pageSize)`、Gold 雙重折扣、Cancel 先改狀態再還庫存等根因。  
**為什麼有效：** 有訂單號、頁面行為、金額／庫存數字，agent 不用猜症狀。

**有效提問範例 4（練習 3：先 plan、再改導覽設計）：**

> `/analyze-plan` Analyze plan for entire Exercise 3…  
> 改 plan：直接改 `/Products`，用 filter 進 `/Products/LowStock`；不要 Layout 多一個低庫存。

→ 產出分層檔案清單、查詢／N+1、驗證、≥3 測試；實作時只在商品頁做「全部商品｜低庫存」切換。  
**為什麼有效：** 規格寫「導覽列」但實際產品路徑在商品列表；先改 plan 再 approve，避免做完又刪 Layout。

### 3. AI 誤導我的地方，與我如何發現

（agent 說錯／改錯／過度自信的時刻。你靠什麼抓到——對照程式碼？頁面實測？跑測試？）

**1）練習 1 時：文件說「折扣只在 CalculateTotal 算一次」——當時 code 並非如此**

- 舊 code：Gold 建單時已折 `UnitPriceSnapshot`，`CalculateTotal` 再折 → 100 變 **81**。  
- **如何發現：** 讀 `OrderService.cs`；UI 用「商品原價 vs 應付總額」而非用快照再 ×0.9。  
- **練習 2 已修：** 快照一律原價，折扣只在 `CalculateTotal`（Gold 90、Silver 95）。

**2）「單元測試全綠 = 沒 bug」是錯的**

- 練習 2 修之前：28～33 測全綠，三個 UI bug 仍在。  
- **如何發現：**  
  - 分頁：舊測試 `Assert.All` 在空集合上會過；只查 TotalCount 不查 page 內容。  
  - 價格：只測手組 snapshot 的 `CalculateTotal`，沒有 **CreateOrder + Gold**。  
  - 取消：只測狀態變 Cancelled，**不測庫存是否加回**。  

**3）Git 與 Windows junction**

- `git add .grok/` 會跟著 junction 重複加入 AI-AGENTS 內容。  
- 處理：只 commit `AI-AGENTS/`；ignore junction；腳本重建 discovery。

**4）Gold bug 在 UI 上「看起來沒壞」**

- 若用**快照單價**當原價再 ×0.9，會得到 81=81，誤以為正確。  
- 必須用 `/Products` **目錄原價**對 **應付總額**。

**5）練習 3：把 404 當成「壞掉的既有頁」**

- 文件寫了路由 `GET /Products/LowStock`，但 repo **從未實作** action／View。  
- `/LowStock` 也會 404（沒有 `LowStockController`）。  
- **如何發現：** 對照 `ProductsController` 只有 `Index`；HTTP 探測 `/Products` 200、`/Products/LowStock` 404（重啟後才 200）。  
- **結論：** 練習 3 是**新功能**，不是練習 2 那種修 bug。

### 4. 我會帶回日常工作的一招

（一個具體、可複製的做法，不要寫「要多驗證」這種口號——寫出**操作步驟**。）

**招式 A：Agent 設定「根薄 + 模組規則 + skills」，code 目錄不塞設定**

1. 根 `AGENTS.md` 只放地圖與 skill 列表。  
2. 慣例拆 `AI-AGENTS/rules/01–05`。  
3. 流程做成 `/analyze-plan`、`/fix-bug`、`/code-reviewer`、`/test-runner`。  
4. 應用程式只在 `training-repo/`。

**招式 B：修 bug 的固定節奏（練習 2 實作）**

1. 自己在 UI 重現，記下**具體數字**（訂單號、原價、庫存 S0/S1/S2）。  
2. `/test-runner` 看基準是否「假綠」。  
3. **先寫會失敗的回歸測試**（或寫完立刻跑證明失敗），再改 production。  
4. `/test-runner` 全綠 → UI 再確認 → **Unit Test commit 再 Fix commit** 分開 push。  
5. 每個 commit message 寫：**症狀 → 根因 → 修法**。

**招式 C：新功能用 Feature 細切 commit（練習 3）**

1. `/analyze-plan` → 審計畫（分層、邊界、測試）→ 口頭／文字 **approve** 後才寫 code。  
2. Commit 順序範例：`Feature - Unit Test …` → Core → Infrastructure → Web 頁 → 既有頁 filter。  
3. 導覽若規格與 UX 衝突，**先改 plan 再實作**（例如低庫存入口放在 `/Products` 切換，不塞 `_Layout`）。

---

## 自我驗證（做到哪個階段答哪題）

### 第一階段 — Agentic Coding

練習 1

1. 我能不看筆記說出三個專案（Web/Core/Infrastructure）各自的職責  
   - **是。** Web＝畫面轉接；Core＝商業邏輯；Infrastructure＝EF／Repository／Migration。  

2. 我核對過 agent 描述的建單流程，且**至少找出一處不精確或過度簡化的說法**  
   - **是。** 舊說法「折扣只在 CalculateTotal」與當時 Gold 建單先折單價不符（練習 2 已修正 code 對齊規格）。  

3. 我知道商業邏輯應該放在哪一層、新增頁面要動哪些地方  
   - **是。** Service 放 Core；EF 放 Repository；頁面動 Controller／ViewModel／View／測試等。  

練習 2

1. 三個 bug 我都先在頁面上重現過，才開始找程式  
   - **是。**  
   - Bug 1：#201 第一頁沒有、最後一頁空白、已取消篩選空白。  
   - Bug 2：Gold 原價×0.9 vs 應付總額（曾見雙重折扣 81）；Silver 對照正常。  
   - Bug 3：建單庫存減少、取消後庫存未加回。  

2. 我給 agent 的資訊包含具體觀察（頁碼／金額數字／庫存數字），而不是只貼客訴原文  
   - **是。** 例如訂單 #201、原價 100／總額 81、庫存 10→7→7 等描述。  

3. 每個修復都回到頁面驗證過症狀消失  
   - **是。**（使用者已目視確認 Bug 1／2／3。）  

4. 每個 bug 都補了一個回歸測試，`dotnet test` 全綠  
   - **是。** 練習 2 結束時 **36** 通過；練習 3 後 **39** 通過。  
   - Bug 1：page1 含最新單、最後頁非空、Cancelled 篩選有列等。  
   - Bug 2：Gold 快照原價＋總額 90 一次；Silver 95。  
   - Bug 3：Pending／Confirmed 取消還庫存；Shipped 取消不動庫存。  

5. 三個獨立 commit，message 說明症狀與根因  
   - **是（且更細）。** 每個 bug 至少 **Unit Test Bug** + **Fix…** 分開；Bug 1 另拆最後頁／已取消說明 commit。  
   - 範例：`Bug 1 - Unit Test Bug` / `Bug 1 - Fix Order missing after creations`；`Bug 2 - …`；`Bug 3 - …` 皆已 push `origin/main`。  

6. （思考題）為什麼原本的測試沒抓到這三個 bug？  
   - **Bug 1：** `Skip(page * pageSize)` 在 page=1 時略過第一頁；`Assert.All` 對空集合為 true；只斷言 TotalCount/TotalPages。  
   - **Bug 2：** 價格測試手組 snapshot，未走 `CreateOrder`+Gold；建單快照測試用 Standard 客戶。  
   - **Bug 3：** 取消測試只斷言狀態＝Cancelled，未斷言 `StockQuantity` 加回。  
   - **共通：** 測試沒有鎖住「使用者在頁面上在乎的數字行為」。  

練習 3

1. `/Products/LowStock` 不帶參數 → 門檻 10 的結果；帶 `?threshold=3` → 結果隨之改變  
   - **是。** `threshold` 省略時 controller 預設 **10**；GET 表單可改門檻。重啟 app 後 HTTP **200**（實作前為 404）。  
   - 進入方式：`/Products` 上 **全部商品｜低庫存** 切換，或直接開 `/Products/LowStock`（**未**加頂部 Layout「低庫存」，依 UX 決策）。  

2. `?threshold=0`、`?threshold=-1` → 頁面顯示驗證錯誤，不是 500  
   - **是。** `LowStockViewModel.Threshold` 有 `[Range(1, …)]`；`TryValidateModel` + ModelState；live `?threshold=0` → **200** 非 500。  

3. 售出數量欄位排除了 Cancelled 訂單（可用一筆已取消的訂單驗證）  
   - **是。** Repository 聚合近 30 天 `OrderItem`，`Status != Cancelled`；單元測試：近期 Confirmed 5 + Cancelled 4 + 40 天前 10 → **SoldLast30Days = 5**。  

4. 停售（已停售 badge）商品不出現在列表  
   - **是。** 條件含 `IsActive`；測試 inactive stock=1 不出現。  

5. 程式分層與命名跟既有的 Products 功能一致（請 agent 自我 review 一次，並自己確認）  
   - **是。** Controller 薄、`ProductService.GetLowStockAsync`、EF 只在 `ProductRepository`、View 綁 `LowStockViewModel`、DataAnnotations 驗證。  
   - 列表規則：`StockQuantity < threshold`（等於門檻不含）、庫存升冪；庫存 &lt; 5 用 `table-danger`。  

6. 至少 3 個新測試，`dotnet test` 全綠  
   - **是。**  
     1. `GetLowStock_FiltersByThreshold_AndSortsByStockAscending`  
     2. `GetLowStock_ExcludesInactiveProducts`  
     3. `GetLowStock_SoldLast30Days_ExcludesCancelledAndOldOrders`  
   - 全套件 **39** 通過。  
   - Commits（Unit Test 先）：`Feature - Unit Test LowStock` → Core API → Infrastructure query → Web page → Products filter；已 push `origin/main`。  

練習 4

1. 重構後 `dotnet test` 全綠  
2. 我能說出這次重構「改善了什麼、沒有改變什麼」  
3. 我有在 code review 的角度看過 diff（不是 agent 說好就好）  

（尚未開始。）

---

## 附錄：值得留下的對話片段

### 片段 A — 確認 DB

- **我問：** Debug 到底連哪個資料庫？  
- **結果：** `OrderHubTraining` @ `localhost`，種子 20/50/200。

### 片段 B — Agent 目錄

- **結果：** `AI-AGENTS/` + 四 skills；`training-repo` code-only。

### 片段 C — 練習 2 基準與假綠

- 練習 2 前：28 通過。  
- 加 Bug 1 測試前全綠仍分頁錯；加 Gold 回歸測試後 **Expected 100 Actual 90** 才鎖住雙重折扣。  
- 練習 2 後：**36** 通過。

### 片段 D — 練習 2 三 bug 根因一句話

| Bug | 症狀（UI） | 根因 | 修法 |
| --- | --- | --- | --- |
| 1 | 新單不在第一頁；末頁／已取消空白 | 1-based page 卻 `Skip(page * pageSize)` | `Skip((page-1)*pageSize)` |
| 2 | Gold 總額偏低；Silver 正常 | Gold 建單先折快照，`CalculateTotal` 再折 | 快照原價；折扣只在 CalculateTotal |
| 3 | 取消後庫存不回升 | 先設 Cancelled 再 if 還庫存 → 永遠不進 | 先還庫存再設 Cancelled |

### 片段 E — 練習 3 低庫存

- **問題：** 文件有 `/Products/LowStock`，開啟卻 404 → 功能未實作，不是路由設定錯。  
- **作法：** `/analyze-plan` → 改為商品頁 filter 進 LowStock → 實作 Core/Infra/Web + 3 tests。  
- **導覽：** `/Products` 上「低庫存」；**不**在 `_Layout` 加第三個導覽項（避免與「全部商品」重複入口）。  
- **Commits：** 5 個 `Feature - …`；測試 36 → **39**。  

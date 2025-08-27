# Handover Unity

## 外部連結

- [Unity](https://unity.com/)
- [Unity Hub](https://unity.com/download)

## 目錄

1. [簡介](#簡介)
2. [腳本常見指令](#腳本常見指令)
3. [CLI 常見指令](#cli-常見指令)

## 簡介

Unity是一款跨平台的遊戲引擎，可用於開發2D和3D遊戲，支援多種個人電腦、行動裝置、遊戲主機、網頁平台、擴增實境和虛擬實境。

### 安裝步驟

1.  **安裝 Unity Hub**
    - 下載連結：<https://unity.com/download>

2.  **安裝對應的 Unity 版本**
    - 專案所需版本請使用 `2021.3.1f1` 或參考 `ProjectSettings/ProjectVersion.txt`。
    - 在 Unity Hub 中新增對應版本，並安裝必要的模組 WebGL Support。

## 腳本常見指令

### WebSocket

- **建立與連線**

  ```csharp
  websocket = new WebSocket("wss://ar.imrc.be/op4/");
  await websocket.Connect();
  ```

  - `new WebSocket(url)`：建立連線物件
  - `await Connect()`：非同步發起連線

- **事件綁定**

  ```csharp
  websocket.OnOpen  += () => { Debug.Log("Connection open!"); };
  websocket.OnClose += (e) => { Debug.Log("Connection closed!"); };
  websocket.OnError += (e) => { Debug.Log("Error: " + e); };
  websocket.OnMessage += (bytes) => {
      ws_msg = System.Text.Encoding.UTF8.GetString(bytes);
      new_ws_msg_flag = true;
  };
  ```

  - `OnOpen`：連上時回傳
  - `OnClose`：關閉時回傳
  - `OnError`：錯誤時回傳
  - `OnMessage`：**收到伺服器訊息**時回傳，這裡把位元組轉字串並設 `new_ws_msg_flag = true` 交給 `Update()` 處理

- **訊息佇列（非 WebGL）**

  ```csharp
  #if !UNITY_WEBGL || UNITY_EDITOR
  websocket.DispatchMessageQueue();
  #endif
  ```

  在 Editor/非 WebGL 環境需要手動將 WebSocket 事件排入 Unity 主執行緒處理。

- **斷線自動重連**

  ```csharp
  if (websocket.State == WebSocketState.Closed) WSStart();
  ```

  每幀檢查狀態，斷線就重建連線。

### 物件移動

- 當 **MC_X = 0** 時，對應到物件的初始 Unity X 座標 (以沿 X 軸移動為例)
- 在 `Start()` 初始化時紀錄
- `targetPosition`：目標位置
- `hasTarget`：是否已設定新目標（避免 Update 每幀亂跑）

```csharp
private float baseX;
private Vector3 targetPosition;
private bool hasTarget = false;
```

- 輸入機台傳來的 **座標字串 (`coord`)**
- 嘗試轉成浮點數
- 使用 `scaleFactor` 將 **mm 轉換成 m**，再加上 `baseX`
- 更新 `targetPosition`
- 設定 `hasTarget = true`，代表有新位置要移動過去

```csharp
public void setCoordinates(string coord)
{
    if (float.TryParse(coord, out float mcx))
    {
        float xCoord = mcx * scaleFactor + baseX;
        targetPosition = new Vector3(xCoord, transform.localPosition.y, transform.localPosition.z);
        hasTarget = true;
    }
}
```

- 每幀檢查是否有新目標 (`hasTarget`)
- 使用 **`Vector3.Lerp`** 讓位置以插值方式平滑移動到 `targetPosition`
- `Time.deltaTime * 5f` 控制移動速度（數字越大移動越快）

```csharp
void Update()
{
    if (hasTarget)
    {
        transform.localPosition = Vector3.Lerp(
            transform.localPosition,
            targetPosition,
            Time.deltaTime * 5f
        );
    }
}
```

## CLI 常見指令

- **以命令列啟動 Unity**

  ```bash
  unity -projectPath <專案路徑>
  ```

- **以命令列建置專案**

  ```bash
  unity -quit -batchmode -nographics \
  -projectPath /path/to/UnityProject \
  -buildTarget WebGL \
  -logFile ./unity_build.log \
  -buildPlayer "Assets/Scenes/MainScene.unity;./Build"
  ```

- **批次模式 (Batch Mode) 建置**

  ```bash
  unity -batchmode -nographics -quit -projectPath <專案路徑> -executeMethod BuildScript.PerformBuild
  ```
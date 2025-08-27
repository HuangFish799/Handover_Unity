import websocket
import time
import json
import pandas as pd
from datetime import datetime

csv_path = "YourPath\\YourCSVFile.csv"
ws_url = "ws://localhost:port"  # 或你 Unity 使用的 IP

df = pd.read_csv(csv_path).fillna("").astype(str)

def on_open(ws):
    print("WebSocket connected. Sending data...")

    for i, row in df.iterrows():
        msg = {
            "JSON": row.to_dict(),
            "WebSocketInfo": {
                "WebSocketServerSendingTime": datetime.utcnow().isoformat() + "Z",
                "WSLatencyTestFlag": False,
                "WSLatencyTestID": i
            },
            "DateTime": datetime.utcnow().isoformat() + "Z"
        }
        ws.send(json.dumps(msg))
        print(f"Sent row {i + 1}")
        time.sleep(0.01)  # 控制送資料間隔

    print("Finished sending. Closing.")
    ws.close()

if __name__ == "__main__":
    ws = websocket.WebSocketApp(ws_url, on_open=on_open)
    ws.run_forever()
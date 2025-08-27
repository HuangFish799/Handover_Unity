### 1. 安裝套件

```bash
pip install confluent-kafka websocket-client
```

### 2. 開啟 WebSocket Server

```bash
python Example_Kafka2WebSocket.py --topic my_topic --bootstrap YourIP --ws-url ws://localhost:8765 --print --pack-json
```
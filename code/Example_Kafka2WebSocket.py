import argparse
import socket
import time
import json
from datetime import datetime
from typing import Optional

from confluent_kafka import Consumer
import websocket

def connect_ws(ws_url: str, timeout: float = 5.0) -> websocket.WebSocket:
    """Connect to WebSocket server with timeout; return live WebSocket object."""
    ws = websocket.create_connection(ws_url, timeout=timeout)
    return ws

def safe_send(ws: websocket.WebSocket, data: str) -> bool:
    """Try to send data; return False if closed/error."""
    try:
        ws.send(data)
        return True
    except Exception:
        return False

def main():
    ap = argparse.ArgumentParser(description="Consume Kafka messages and forward to WebSocket")
    ap.add_argument("--bootstrap", default="YourIP", help="Kafka bootstrap servers")
    ap.add_argument("--topic", required=True, help="Kafka topic to consume")
    ap.add_argument("--group", default="g-home", help="Kafka consumer group id")
    ap.add_argument("--from-beginning", action="store_true", help="Start from earliest offset")
    ap.add_argument("--ws-url", default="ws://localhost:YourPort", help="WebSocket server URL (e.g., ws://localhost:8765)")
    ap.add_argument("--ws-timeout", type=float, default=5.0, help="WebSocket connect/send timeout (seconds)")
    ap.add_argument("--print", dest="do_print", action="store_true", help="Also print messages to stdout")
    ap.add_argument("--pack-json", action="store_true", help="Wrap outgoing message as JSON with metadata")
    args = ap.parse_args()

    conf = {
        "bootstrap.servers": args.bootstrap,
        "group.id": args.group,
        "enable.auto.commit": True,
        "auto.offset.reset": "earliest" if args.from_beginning else "latest",
        "client.id": socket.gethostname(),
    }
    c = Consumer(conf)
    c.subscribe([args.topic])

    print(f"Kafka -> WebSocket bridge")
    print(f"  Kafka: topic='{args.topic}', group='{args.group}', from_beginning={args.from_beginning}, bootstrap='{args.bootstrap}'")
    print(f"  WebSocket: {args.ws_url}")
    print("Ctrl+C to stop")

    ws: Optional[websocket.WebSocket] = None
    backoff_s = 1.0

    def ensure_ws_connected() -> Optional[websocket.WebSocket]:
        nonlocal ws, backoff_s
        if ws is not None:
            return ws
        while True:
            try:
                print(f"[WS] Connecting to {args.ws_url} ...")
                ws = connect_ws(args.ws_url, timeout=args.ws_timeout)
                ws.settimeout(args.ws_timeout)
                print("[WS] Connected")
                backoff_s = 1.0
                return ws
            except KeyboardInterrupt:
                raise
            except Exception as e:
                print(f"[WS] Connect failed: {e} (retry in {backoff_s:.1f}s)")
                time.sleep(backoff_s)
                backoff_s = min(backoff_s * 2, 30.0)

    try:
        while True:
            msg = c.poll(1.0)
            if msg is None:
                continue
            if msg.error():
                print("[Kafka error]", msg.error())
                continue

            _, ts = msg.timestamp()
            tstr = datetime.fromtimestamp(ts / 1000).strftime("%Y-%m-%d %H:%M:%S") if ts and ts > 0 else "-"
            key = msg.key().decode() if msg.key() else None
            val = msg.value().decode(errors="replace") if msg.value() else ""

            if args.do_print:
                print(f"[{tstr}] {args.topic}[{msg.partition()}]@{msg.offset()} key={key} value={val}")

            if args.pack_json:
                payload = json.dumps({
                    "topic": args.topic,
                    "partition": msg.partition(),
                    "offset": msg.offset(),
                    "timestamp": tstr,
                    "key": key,
                    "value": val,
                }, ensure_ascii=False)
            else:
                payload = val

            ws = ensure_ws_connected()
            if ws is None:
                continue
            ok = safe_send(ws, payload)
            if not ok:
                try:
                    ws.close()
                except Exception:
                    pass
                ws = None
                # Retry once
                ws = ensure_ws_connected()
                if ws:
                    ok = safe_send(ws, payload)
                    if not ok:
                        try:
                            ws.close()
                        except Exception:
                            pass
                        ws = None
                        print("[WS] Send failed after reconnect; message dropped.")

    except KeyboardInterrupt:
        print("\nStopping...")
    finally:
        try:
            if ws is not None:
                ws.close()
        except Exception:
            pass
        c.close()

if __name__ == "__main__":
    main()
import sys
import json
import threading

# Define message type enums (match C# values)
class ServerToClientMessageType:
    SPLIT = 0
    PEEL = 1
    DUMP = 2
    BANANAS = 3

class ClientToServerMessageType:
    DUMP = 0
    PEEL = 1

def parse_server_message(message):
    msg_type = message["Type"]
    payload = message.get("Payload")

    if msg_type == ServerToClientMessageType.SPLIT or msg_type == ServerToClientMessageType.DUMP:
        payload = list(payload)
    elif msg_type == ServerToClientMessageType.PEEL:
        payload = payload[0] if isinstance(payload, str) else payload
    elif msg_type == ServerToClientMessageType.BANANAS:
        payload = None

    return msg_type, payload

def listen_to_server():
    while True:
        line = sys.stdin.readline()
        if not line:
            break
        try:
            message = json.loads(line)
            msg_type, payload = parse_server_message(message)
            print(f"[Server] Type: {msg_type}, Payload: {payload}")
        except Exception as e:
            print(f"[Error] Failed to parse message: {e}")

def send_to_server(msg_type, payload):
    message = {
        "Type": msg_type,
        "Payload": payload
    }
    json.dump(message, sys.stdout)
    sys.stdout.write("\n")
    sys.stdout.flush()

def handle_user_input():
    while True:
        try:
            command = input("Enter command (dump <char> | peel): ").strip().lower()
            if command.startswith("dump "):
                letter = command.split(" ")[1][0]
                send_to_server(ClientToServerMessageType.DUMP, letter)
            elif command == "peel":
                # Example board: [['A', 'B'], ['C', 'D']]
                board = [['A', 'B'], ['C', 'D']]
                send_to_server(ClientToServerMessageType.PEEL, board)
            else:
                print("Unknown command.")
        except EOFError:
            break

if __name__ == "__main__":
    threading.Thread(target=listen_to_server, daemon=True).start()
    handle_user_input()

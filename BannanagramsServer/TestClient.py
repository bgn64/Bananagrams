import sys
import json
import threading
import msvcrt
import win32pipe
import win32file

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

def listen_to_server(pipe_in):
    while True:
        try:
            result, data = win32file.ReadFile(pipe_in, 4096)
            if not data:
                break
            lines = data.decode().splitlines()
            for line in lines:
                if not line.strip():
                    continue
                message = json.loads(line)
                msg_type, payload = parse_server_message(message)
                print(f"[Server] Type: {msg_type}, Payload: {payload}")
        except Exception as e:
            print(f"[Error] {e}")
            break

def send_to_server(pipe_out, msg_type, payload):
    message = {
        "Type": msg_type,
        "Payload": payload
    }
    data = (json.dumps(message) + "\n").encode()
    win32file.WriteFile(pipe_out, data)

def handle_user_input(pipe_out):
    while True:
        try:
            command = input("Enter command (dump <char> | peel): ").strip().lower()
            if command.startswith("dump "):
                letter = command.split(" ")[1][0]
                send_to_server(pipe_out, ClientToServerMessageType.DUMP, letter)
            elif command == "peel":
                board = [['A', 'B'], ['C', 'D']]
                send_to_server(pipe_out, ClientToServerMessageType.PEEL, board)
            else:
                print("Unknown command.")
        except EOFError:
            break

if __name__ == "__main__":
    if len(sys.argv) != 3:
        print("Usage: python client.py <pipe_name_out> <pipe_name_in>")
        sys.exit(1)

    pipe_name_out = r"\\.\pipe\\" + sys.argv[1]
    pipe_name_in = r"\\.\pipe\\" + sys.argv[2]

    pipe_in = win32file.CreateFile(
        pipe_name_out,
        win32file.GENERIC_READ,
        0,
        None,
        win32file.OPEN_EXISTING,
        0,
        None
    )

    pipe_out = win32file.CreateFile(
        pipe_name_in,
        win32file.GENERIC_WRITE,
        0,
        None,
        win32file.OPEN_EXISTING,
        0,
        None
    )

    threading.Thread(target=listen_to_server, args=(pipe_in,), daemon=True).start()
    handle_user_input(pipe_out)

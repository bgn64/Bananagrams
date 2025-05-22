import sys
import json
import threading
import win32file

class ServerToClientMessageType:
    SPLIT = 0
    PEEL = 1
    DUMP = 2
    BANANAS = 3

class ClientToServerMessageType:
    DUMP = 0
    PEEL = 1

class ClientMessageEndpoint:
    def __init__(self, pipe_in, pipe_out):
        self.pipe_in = pipe_in
        self.pipe_out = pipe_out

    def send_dump(self, letter):
        self._send_message({"Type": ClientToServerMessageType.DUMP, "Payload": letter})

    def send_peel(self, board):
        self._send_message({"Type": ClientToServerMessageType.PEEL, "Payload": board})

    def _send_message(self, message):
        data = (json.dumps(message) + "\n").encode()
        win32file.WriteFile(self.pipe_out, data)

    def listen(self):
        while True:
            try:
                _, data = win32file.ReadFile(self.pipe_in, 4096)
                if not data:
                    break
                for line in data.decode().splitlines():
                    if not line.strip():
                        continue
                    message = json.loads(line)
                    self._handle_message(message)
            except Exception as e:
                print(f"[Error] {e}")
                break

    def _handle_message(self, message):
        msg_type = message["Type"]
        payload = message.get("Payload")

        if msg_type == ServerToClientMessageType.SPLIT:
            payload = list(payload)
            self.on_split(payload)
        elif msg_type == ServerToClientMessageType.PEEL:
            self.on_peel(payload)
        elif msg_type == ServerToClientMessageType.DUMP:
            payload = list(payload)
            self.on_dump(payload)
        elif msg_type == ServerToClientMessageType.BANANAS:
            self.on_bananas(payload)
        else:
            print(f"Unknown server message type {msg_type}.")

    def on_split(self, payload): raise NotImplementedError
    def on_peel(self, payload): raise NotImplementedError
    def on_dump(self, payload): raise NotImplementedError
    def on_bananas(self, payload): raise NotImplementedError

class MyClientMessageEndpoint(ClientMessageEndpoint):
    def on_split(self, payload):
        print(f"Received SPLIT: {', '.join(payload)}")

    def on_peel(self, payload):
        print(f"Received PEEL: {payload}")

    def on_dump(self, payload):
        print(f"Received DUMP: {', '.join(payload)}")

    def on_bananas(self, payload):
        print("Received BANNANAS")

def create_client_endpoint(pipe_name_out, pipe_name_in):
    pipe_in = win32file.CreateFile(
        r"\\.\pipe\\" + pipe_name_out,
        win32file.GENERIC_READ,
        0, None, win32file.OPEN_EXISTING, 0, None
    )
    pipe_out = win32file.CreateFile(
        r"\\.\pipe\\" + pipe_name_in,
        win32file.GENERIC_WRITE,
        0, None, win32file.OPEN_EXISTING, 0, None
    )
    return MyClientMessageEndpoint(pipe_in, pipe_out)

def handle_user_input(endpoint):
    print("Enter commands (e.g., DUMP A or PEEL):")
    while True:
        try:
            command = input().strip()
            if not command:
                continue
            parts = command.split(' ', 1)
            cmd = parts[0].upper()
            if cmd == "DUMP":
                if len(parts) < 2 or len(parts[1]) != 1:
                    print("Usage: DUMP <letter>")
                    continue
                endpoint.send_dump(parts[1][0])
            elif cmd == "PEEL":
                endpoint.send_peel([])  # Empty board
            elif cmd == "EXIT":
                print("Exiting client...")
                break
            else:
                print("Unknown command. Try DUMP <letter>, PEEL, or EXIT.")
        except Exception as e:
            print(f"Error sending message: {e}")

def main():
    if len(sys.argv) != 3:
        print("Usage: python client.py <pipe_name_out> <pipe_name_in>")
        sys.exit(1)

    pipe_name_out = sys.argv[1]
    pipe_name_in = sys.argv[2]

    endpoint = create_client_endpoint(pipe_name_out, pipe_name_in)
    threading.Thread(target=endpoint.listen, daemon=True).start()
    handle_user_input(endpoint)

if __name__ == "__main__":
    main()

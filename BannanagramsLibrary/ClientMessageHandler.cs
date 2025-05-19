using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BannanagramsLibrary
{
    public class ClientMessageHandler
    {
        private readonly PipeMessenger messenger;

        public ClientMessageHandler(PipeMessenger messenger)
        {
            this.messenger = messenger;
        }

        public Task SendDumpAsync(char letter) =>
        messenger.SendAsync(new ClientToServerMessage { Type = ClientToServerMessageType.DUMP, Payload = letter });

        public Task SendPeelAsync(char[][] board) =>
        messenger.SendAsync(new ClientToServerMessage { Type = ClientToServerMessageType.PEEL, Payload = board });

        public async Task HandleServerMessagesAsync()
        {
            while (true)
            {
                var message = await messenger.ReceiveAsync<ServerToClientMessage>();
                if (message == null) break;

                switch (message.Type)
                {
                    case ServerToClientMessageType.SPLIT:
                        Console.WriteLine($"CLIENT: Received SPLIT: {string.Join(", ", JsonSerializer.Deserialize<List<char>>(message.Payload!.ToString()!))}");
                        break;
                    case ServerToClientMessageType.PEEL:
                        Console.WriteLine($"CLIENT: Received PEEL: {message.Payload}");
                        break;
                    case ServerToClientMessageType.DUMP:
                        Console.WriteLine($"CLIENT: Received DUMP: {string.Join(", ", JsonSerializer.Deserialize<List<char>>(message.Payload!.ToString()!))}");
                        break;
                    case ServerToClientMessageType.BANNANAS:
                        Console.WriteLine("CLIENT: Received BANNANAS");
                        break;
                }
            }
        }
    }

}

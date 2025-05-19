using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace BannanagramsLibrary
{
    public class ServerMessageHandler
    {
        private readonly PipeMessenger _messenger;

        public ServerMessageHandler(PipeMessenger messenger)
        {
            _messenger = messenger;
        }

        public Task SendSplitAsync(List<char> letters) =>
        _messenger.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.SPLIT, Payload = letters });

        public Task SendPeelAsync(char letter) =>
        _messenger.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.PEEL, Payload = letter });

        public Task SendDumpAsync(List<char> letters) =>
        _messenger.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.DUMP, Payload = letters });

        public Task SendBannanasAsync() =>
        _messenger.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.BANNANAS });

        public async Task HandleClientMessagesAsync()
        {
            while (true)
            {
                var message = await _messenger.ReceiveAsync<ClientToServerMessage>();
                if (message == null) break;

                switch (message.Type)
                {
                    case ClientToServerMessageType.DUMP:
                        Console.WriteLine($"SERVER: Received DUMP message with letter: {message.Payload}");
                        break;
                    case ClientToServerMessageType.PEEL:
                        Console.WriteLine("SERVER: Received PEEL message with board:");
                        var board = JsonSerializer.Deserialize<char[][]>(message.Payload!.ToString()!);
                        foreach (var row in board!)
                        {
                            Console.WriteLine(string.Join(" ", row));
                        }
                        break;
                }
            }
        }
    }
}

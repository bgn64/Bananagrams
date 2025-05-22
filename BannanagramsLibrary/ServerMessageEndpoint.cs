using System.Text.Json;

namespace BannanagramsLibrary
{
    public abstract class ServerMessageEndpoint
    {
        private readonly PipeMessageChannel channel;

        protected ServerMessageEndpoint(PipeMessageChannel channel)
        {
            this.channel = channel;
        }

        // Outgoing messages (Server → Client)
        public Task SendSplitAsync(List<char> letters) =>
 channel.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.SPLIT, Payload = letters });

        public Task SendPeelAsync(char letter) =>
        channel.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.PEEL, Payload = letter });

        public Task SendDumpAsync(List<char> letters) =>
        channel.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.DUMP, Payload = letters });

        public Task SendBananasAsync() =>
        channel.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.BANANAS });

        // Incoming messages (Client → Server)
        public async Task ListenAsync()
        {
            while (true)
            {
                var message = await channel.ReceiveAsync<ClientToServerMessage>();
                if (message == null) break;

                CastClientPayload(message);

                switch (message.Type)
                {
                    case ClientToServerMessageType.PEEL:
                        await OnPeelAsync(message);
                        break;
                    case ClientToServerMessageType.DUMP:
                        await OnDumpAsync(message);
                        break;
                    default:
                        Console.WriteLine($"Unknown client message type {message.Type}.");
                        break;
                }
            }
        }

        private static void CastClientPayload(ClientToServerMessage message)
        {
            var payloadJson = JsonSerializer.Serialize(message.Payload);
            switch (message.Type)
            {
                case ClientToServerMessageType.PEEL:
                    message.Payload = JsonSerializer.Deserialize<char[][]>(payloadJson);
                    break;
                case ClientToServerMessageType.DUMP:
                    message.Payload = JsonSerializer.Deserialize<char>(payloadJson);
                    break;
            }
        }

        protected abstract Task OnPeelAsync(ClientToServerMessage message);
        protected abstract Task OnDumpAsync(ClientToServerMessage message);
    }
}

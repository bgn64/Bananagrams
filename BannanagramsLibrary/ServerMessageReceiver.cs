using System.Text.Json;

namespace BannanagramsLibrary
{
    public abstract class ServerMessageReceiver
    {
        private readonly PipeReceiver receiver;

        protected ServerMessageReceiver(PipeReceiver receiver)
        {
            this.receiver = receiver;
        }

        public async Task ListenAsync()
        {
            while (true)
            {
                ServerToClientMessage? message = await receiver.ReceiveAsync<ServerToClientMessage>();
                if (message == null) break;

                CastServerPayload(message);

                switch (message.Type)
                {
                    case ServerToClientMessageType.SPLIT:
                        await OnSplitAsync(message);
                        break;
                    case ServerToClientMessageType.PEEL:
                        await OnPeelAsync(message);
                        break;
                    case ServerToClientMessageType.DUMP:
                        await OnDumpAsync(message);
                        break;
                    case ServerToClientMessageType.BANANAS:
                        await OnBananasAsync(message);
                        return;
                    default:
                        Console.WriteLine($"Unknown message type {message.Type}.");
                        break;
                }
            }
        }

        private void CastServerPayload(ServerToClientMessage message)
        {
            var payloadJson = JsonSerializer.Serialize(message.Payload);

            switch (message.Type)
            {
                case ServerToClientMessageType.SPLIT:
                case ServerToClientMessageType.DUMP:
                    message.Payload = JsonSerializer.Deserialize<List<char>>(payloadJson);
                    break;
                case ServerToClientMessageType.PEEL:
                    message.Payload = JsonSerializer.Deserialize<char>(payloadJson);
                    break;
            }
        }

        protected abstract Task OnSplitAsync(ServerToClientMessage message);
        protected abstract Task OnPeelAsync(ServerToClientMessage message);
        protected abstract Task OnDumpAsync(ServerToClientMessage message);
        protected abstract Task OnBananasAsync(ServerToClientMessage message);
    }
}

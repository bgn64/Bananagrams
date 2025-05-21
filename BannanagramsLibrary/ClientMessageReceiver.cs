using System.Text.Json;

namespace BannanagramsLibrary
{
    public abstract class ClientMessageReceiver
    {
        private readonly PipeReceiver receiver;

        protected ClientMessageReceiver(PipeReceiver receiver)
        {
            this.receiver = receiver;
        }

        public async Task ListenAsync()
        {
            while (true)
            {
                ClientToServerMessage? message = await receiver.ReceiveAsync<ClientToServerMessage>();
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
                        Console.WriteLine($"Unknown message type {message.Type}.");
                        break;
                }
            }
        }

        public static void CastClientPayload(ClientToServerMessage message)
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

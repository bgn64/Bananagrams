namespace BannanagramsLibrary
{
    public class ClientMessageSender
    {
        private readonly PipeSender sender;

        public ClientMessageSender(PipeSender sender)
        {
            this.sender = sender;
        }

        public Task SendDumpAsync(char letter) =>
        sender.SendAsync(new ClientToServerMessage { Type = ClientToServerMessageType.DUMP, Payload = letter });

        public Task SendPeelAsync(char[][] board) =>
        sender.SendAsync(new ClientToServerMessage { Type = ClientToServerMessageType.PEEL, Payload = board });
    }
}

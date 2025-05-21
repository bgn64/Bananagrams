namespace BannanagramsLibrary
{
    public class ServerMessageSender
    {
        private readonly PipeSender sender;

        public ServerMessageSender(PipeSender sender)
        {
            this.sender = sender;
        }

        public Task SendSplitAsync(List<char> letters) => sender.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.SPLIT, Payload = letters });

        public Task SendPeelAsync(char letter) => sender.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.PEEL, Payload = letter });

        public Task SendDumpAsync(List<char> letters) => sender.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.DUMP, Payload = letters });

        public Task SendBannanasAsync() => sender.SendAsync(new ServerToClientMessage { Type = ServerToClientMessageType.BANANAS });
    }
}

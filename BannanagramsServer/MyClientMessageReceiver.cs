using BannanagramsLibrary;
using System.Threading.Channels;

namespace BannanagramsServer
{
    internal class MyClientMessageReceiver : ClientMessageReceiver
    {
        private Channel<QueuedClientMessage> messageQueue;
        private string clientId;

        public MyClientMessageReceiver(Channel<QueuedClientMessage> messageQueue, PipeReceiver receiver, string clientId) : base(receiver)
        {
            this.messageQueue = messageQueue;
            this.clientId = clientId;
        }

        protected override async Task OnPeelAsync(ClientToServerMessage message)
        {
            await messageQueue.Writer.WriteAsync(new QueuedClientMessage
            {
                Message = message,
                ClientId = clientId
            });
        }

        protected override async Task OnDumpAsync(ClientToServerMessage message)
        {
            await messageQueue.Writer.WriteAsync(new QueuedClientMessage
            {
                Message = message,
                ClientId = clientId
            });
        }
    }
}

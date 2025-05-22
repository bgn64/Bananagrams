using Library;
using System.Threading.Channels;

namespace Server
{
    internal class MyServerMessageEndpoint : ServerMessageEndpoint
    {
        private Channel<QueuedClientMessage> messageQueue;
        private string clientId;

        public MyServerMessageEndpoint(Channel<QueuedClientMessage> messageQueue, PipeMessageChannel channel, string clientId) : base(channel)
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

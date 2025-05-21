using BannanagramsLibrary;

namespace BannanagramsClient
{
    internal class MyServerMessageReceiver : ServerMessageReceiver
    {
        public MyServerMessageReceiver(PipeReceiver receiver) : base(receiver)
        {
        }

        protected override async Task OnBananasAsync(ServerToClientMessage message)
        {
            Console.WriteLine("Received BANNANAS");
        }

        protected override async Task OnDumpAsync(ServerToClientMessage message)
        {
            Console.WriteLine($"Received DUMP: {string.Join(", ", message.Payload!.ToString())}");
        }

        protected override async Task OnPeelAsync(ServerToClientMessage message)
        {
            Console.WriteLine($"Received PEEL: {message.Payload}");
        }

        protected override async Task OnSplitAsync(ServerToClientMessage message)
        {
            Console.WriteLine($"Received SPLIT: {string.Join(", ", message.Payload!.ToString())}");
        }
    }
}

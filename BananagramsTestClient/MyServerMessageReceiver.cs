using BannanagramsLibrary;

namespace BannanagramsTestClient
{
    internal class MyClientMessageEndpoint : ClientMessageEndpoint
    {
        public MyClientMessageEndpoint(PipeMessageChannel channel) : base(channel)
        {
        }

        protected override Task OnBananasAsync(ServerToClientMessage message)
        {
            Console.WriteLine("Received BANNANAS");

            return Task.CompletedTask;
        }

        protected override Task OnDumpAsync(ServerToClientMessage message)
        {
            Console.WriteLine($"Received DUMP: {string.Join(", ", (List<char>)message.Payload!)}");

            return Task.CompletedTask;
        }

        protected override Task OnPeelAsync(ServerToClientMessage message)
        {
            Console.WriteLine($"Received PEEL: {message.Payload}");

            return Task.CompletedTask;
        }

        protected override Task OnSplitAsync(ServerToClientMessage message)
        {
            Console.WriteLine($"Received SPLIT: {string.Join(", ", (List<char>)message.Payload!)}");

            return Task.CompletedTask;
        }
    }
}

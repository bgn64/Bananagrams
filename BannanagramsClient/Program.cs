using BannanagramsLibrary;
using System.IO.Pipes;

class Program
{
    static async Task Main(string[] args)
    {
        using var pipeIn = new AnonymousPipeClientStream(PipeDirection.In, args[0]);
        using var pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, args[1]);

        var messenger = new PipeMessenger(pipeIn, pipeOut);
        var handler = new ClientMessageHandler(messenger);

        await handler.SendDumpAsync('Z');
        await handler.SendPeelAsync([['A', 'B'], ['C', 'D']]);

        await handler.HandleServerMessagesAsync();
    }
}
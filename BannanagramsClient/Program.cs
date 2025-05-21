using BannanagramsClient;
using BannanagramsLibrary;
using System.IO.Pipes;

class Program
{
    static async Task Main(string[] args)
    {
        using AnonymousPipeClientStream pipeIn = new AnonymousPipeClientStream(PipeDirection.In, args[0]);
        using AnonymousPipeClientStream pipeOut = new AnonymousPipeClientStream(PipeDirection.Out, args[1]);

        PipeSender pipeSender = new PipeSender(pipeOut);
        PipeReceiver pipeReceiver = new PipeReceiver(pipeIn);

        ClientMessageSender sender = new ClientMessageSender(pipeSender);
        ServerMessageReceiver receiver = new MyServerMessageReceiver(pipeReceiver);

        await receiver.ListenAsync();
    }
}
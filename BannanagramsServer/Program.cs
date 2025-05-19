using System.IO.Pipes;
using System.Diagnostics;
using BannanagramsLibrary;

class PipeServer
{
    static async Task Main()
    {
        var pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
        var pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);

        var psi = new ProcessStartInfo
        {
            FileName = "BannanagramsClient.exe",
            Arguments = $"{pipeOut.GetClientHandleAsString()} {pipeIn.GetClientHandleAsString()}",
            UseShellExecute = false
        };

        Process client = Process.Start(psi)!;
        pipeOut.DisposeLocalCopyOfClientHandle();
        pipeIn.DisposeLocalCopyOfClientHandle();

        var messenger = new PipeMessenger(pipeIn, pipeOut);
        var handler = new ServerMessageHandler(messenger);

        await handler.SendSplitAsync(new List<char> { 'A', 'B', 'C' });
        await handler.SendPeelAsync('D');
        await handler.SendDumpAsync(new List<char> { 'E', 'F', 'G' });
        await handler.SendBannanasAsync();

        await handler.HandleClientMessagesAsync();
    }
}
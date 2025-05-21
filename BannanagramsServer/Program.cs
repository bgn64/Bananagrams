using System.IO.Pipes;
using System.Diagnostics;
using BannanagramsLibrary;
using System.Threading.Channels;
using BannanagramsServer;

class Program
{
    private static readonly Channel<QueuedClientMessage> messageQueue = Channel.CreateUnbounded<QueuedClientMessage>();

    static async Task Main()
    {
        List<string> exeFiles = Directory.GetFiles(Directory.GetCurrentDirectory(), "*.exe").Where(f => Path.GetFileName(f) != "BannanagramsServer.exe").ToList();

        Console.WriteLine("Found the following client executables:");
        exeFiles.ForEach(Console.WriteLine);

        Console.Write("Are these correct? (yes/no): ");
        if (Console.ReadLine()?.Trim().ToLower() != "yes")
            return;

        Dictionary<char, int> distribution = new Dictionary<char, int>
        {
            ['A'] = 13,
            ['B'] = 3,
            ['C'] = 3,
            ['D'] = 6,
            ['E'] = 18,
            ['F'] = 3,
            ['G'] = 4,
            ['H'] = 3,
            ['I'] = 12,
            ['J'] = 2,
            ['K'] = 2,
            ['L'] = 5,
            ['M'] = 3,
            ['N'] = 8,
            ['O'] = 11,
            ['P'] = 3,
            ['Q'] = 2,
            ['R'] = 9,
            ['S'] = 6,
            ['T'] = 9,
            ['U'] = 6,
            ['V'] = 3,
            ['W'] = 3,
            ['X'] = 2,
            ['Y'] = 3,
            ['Z'] = 2
        };


        List<char> letterBag = distribution.SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value)).ToList();
        Random rng = new Random();
        letterBag = letterBag.OrderBy(_ => rng.Next()).ToList();

        List<(ServerMessageSender Sender, ClientMessageReceiver Receiver, string Id)> clients = new List<(ServerMessageSender Sender, ClientMessageReceiver Receiver, string Id)>();

        foreach (string exe in exeFiles)
        {
            AnonymousPipeServerStream pipeOut = new AnonymousPipeServerStream(PipeDirection.Out, HandleInheritability.Inheritable);
            AnonymousPipeServerStream pipeIn = new AnonymousPipeServerStream(PipeDirection.In, HandleInheritability.Inheritable);

            ProcessStartInfo psi = new ProcessStartInfo
            {
                FileName = exe,
                Arguments = $"{pipeOut.GetClientHandleAsString()} {pipeIn.GetClientHandleAsString()}",
                UseShellExecute = false
            };

            Process process = Process.Start(psi)!;
            pipeOut.DisposeLocalCopyOfClientHandle();
            pipeIn.DisposeLocalCopyOfClientHandle();

            PipeSender pipeSender = new PipeSender(pipeOut);
            PipeReceiver pipeReceiver = new PipeReceiver(pipeIn);
            ServerMessageSender sender = new ServerMessageSender(pipeSender);
            ClientMessageReceiver receiver = new MyClientMessageReceiver(messageQueue, pipeReceiver, Path.GetFileName(exe));
            
            clients.Add((sender, receiver, Path.GetFileName(exe)));

            List<char> letters = letterBag.Take(21).ToList();
            letterBag.RemoveRange(0, 21);
            await sender.SendSplitAsync(letters);

            _ = Task.Run(receiver.ListenAsync);
        }

        while (await messageQueue.Reader.WaitToReadAsync())
        {
            QueuedClientMessage queued = await messageQueue.Reader.ReadAsync();
            ClientToServerMessage message = queued.Message;

            switch (message.Type)
            {
                case ClientToServerMessageType.PEEL:
                    // TODO: Validate board
                    if (letterBag.Count >= clients.Count)
                    {
                        List<char> letters = letterBag.Take(clients.Count).ToList();
                        letterBag.RemoveRange(0, clients.Count);

                        for (int i = 0; i < clients.Count; i++)
                        {
                            (ServerMessageSender sender, _, _) = clients[i];
                            await sender.SendPeelAsync(letters[i]);
                        }
                    }
                    else
                    {
                        foreach ((ServerMessageSender sender, _, _) in clients)
                            await sender.SendBannanasAsync();
                        Console.WriteLine($"Game over! {queued.ClientId} wins.");
                        return;
                    }
                    break;

                case ClientToServerMessageType.DUMP:
                    // TODO: Verify that the user has that letter
                    char dumpedLetter = (char)message.Payload!;
                    letterBag.Add(dumpedLetter);
                    letterBag = letterBag.OrderBy(_ => rng.Next()).ToList();
                    List<char> newLetters = letterBag.Take(3).ToList();
                    letterBag.RemoveRange(0, 3);
                    await clients.Where(x => x.Id == queued.ClientId).Single().Sender.SendDumpAsync(newLetters);
                    break;
            }
        }
    }
}
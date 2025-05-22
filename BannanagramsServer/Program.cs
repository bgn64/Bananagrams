using System.IO.Pipes;
using System.Diagnostics;
using BannanagramsLibrary;
using System.Threading.Channels;
using BannanagramsServer;

class Program
{
    private static readonly Dictionary<char, int> distribution = new Dictionary<char, int>
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

    private static readonly Channel<QueuedClientMessage> messageQueue = Channel.CreateUnbounded<QueuedClientMessage>();

    static async Task Main()
    {
        List<string> clientFiles = Directory.GetFiles(Directory.GetCurrentDirectory())
         .Where(f => (f.EndsWith(".exe") && Path.GetFileName(f) != "BannanagramsServer.exe") || f.EndsWith(".py"))
         .ToList();

        if (clientFiles.Count == 0)
        {
            Console.WriteLine("No client executables or scripts found.");
            return;
        }

        Console.WriteLine("Found the following client executables/scripts:");
        for (int i = 0; i < clientFiles.Count; i++)
        {
            Console.WriteLine($"{i + 1}: {Path.GetFileName(clientFiles[i])}");
        }

        Console.Write("Enter the numbers of the clients you want to launch (comma-separated): ");
        string? input = Console.ReadLine();
        if (string.IsNullOrWhiteSpace(input))
            return;

        var selectedIndices = input.Split(',')
         .Select(s => int.TryParse(s.Trim(), out int index) ? index - 1 : -1)
         .Where(i => i >= 0 && i < clientFiles.Count)
         .ToList();

        if (selectedIndices.Count == 0)
        {
            Console.WriteLine("No valid selections made.");
            return;
        }

        var selectedClientFiles = selectedIndices.Select(i => clientFiles[i]).ToList();

        List<char> letterBag = distribution.SelectMany(kvp => Enumerable.Repeat(kvp.Key, kvp.Value)).ToList();
        Random rng = new Random();
        letterBag = letterBag.OrderBy(_ => rng.Next()).ToList();

        List<(ServerMessageEndpoint Endpoint, string Id)> clients = new();

        foreach (string clientFile in selectedClientFiles)
        {
            var endpoint = await CreateServerEndpointAsync(clientFile, messageQueue);
            string clientId = Path.GetFileName(clientFile);

            clients.Add((endpoint, clientId));

            List<char> letters = letterBag.Take(21).ToList();
            letterBag.RemoveRange(0, 21);
            await endpoint.SendSplitAsync(letters);

            _ = Task.Run(endpoint.ListenAsync);
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
                            (ServerMessageEndpoint endpoint, _) = clients[i];
                            await endpoint.SendPeelAsync(letters[i]);
                        }
                    }
                    else
                    {
                        foreach ((ServerMessageEndpoint endpoint, _) in clients)
                            await endpoint.SendBananasAsync();
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
                    await clients.Where(x => x.Id == queued.ClientId).Single().Endpoint.SendDumpAsync(newLetters);
                    break;
            }
        }
    }

    private static async Task<ServerMessageEndpoint> CreateServerEndpointAsync(
        string clientFile,
        Channel<QueuedClientMessage> messageQueue)
    {
        string pipeNameIn = $"pipe_in_{Guid.NewGuid()}";
        string pipeNameOut = $"pipe_out_{Guid.NewGuid()}";

        var pipeOut = new NamedPipeServerStream(pipeNameOut, PipeDirection.Out, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);
        var pipeIn = new NamedPipeServerStream(pipeNameIn, PipeDirection.In, 1, PipeTransmissionMode.Byte, PipeOptions.Asynchronous);

        string fileName = Path.GetFileName(clientFile);
        bool isPython = fileName.EndsWith(".py");

        var psi = new ProcessStartInfo
        {
            FileName = isPython ? "python" : clientFile,
            Arguments = isPython ? $"\"{clientFile}\" {pipeNameOut} {pipeNameIn}" : $"{pipeNameOut} {pipeNameIn}",
            UseShellExecute = false
        };

        Process.Start(psi);

        await pipeOut.WaitForConnectionAsync();
        await pipeIn.WaitForConnectionAsync();

        var channel = new PipeMessageChannel(pipeIn, pipeOut);
        return new MyServerMessageEndpoint(messageQueue, channel, fileName);
    }
}
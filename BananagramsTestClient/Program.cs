using BannanagramsLibrary;
using BannanagramsTestClient;
using System.IO.Pipes;

class Program
{
    static async Task Main(string[] args)
    {
        string pipeNameOut = args[0];
        string pipeNameIn = args[1];

        var endpoint = await CreateClientEndpointAsync(pipeNameOut, pipeNameIn);

        Task listenTask = endpoint.ListenAsync();
        Task inputTask = HandleUserInputAsync(endpoint);

        await Task.WhenAny(listenTask, inputTask);
    }


    private static async Task<ClientMessageEndpoint> CreateClientEndpointAsync(string pipeNameOut, string pipeNameIn)
    {
        var pipeIn = new NamedPipeClientStream(".", pipeNameOut, PipeDirection.In);
        var pipeOut = new NamedPipeClientStream(".", pipeNameIn, PipeDirection.Out);

        await pipeIn.ConnectAsync();
        await pipeOut.ConnectAsync();

        var channel = new PipeMessageChannel(pipeIn, pipeOut);

        return new MyClientMessageEndpoint(channel);
    }


    private static async Task HandleUserInputAsync(ClientMessageEndpoint endpoint)
    {
        Console.WriteLine("Enter commands (e.g., DUMP A or PEEL):");

        while (true)
        {
            string? input = Console.ReadLine();
            if (string.IsNullOrWhiteSpace(input)) continue;

            string[] parts = input.Trim().Split(' ', 2, StringSplitOptions.RemoveEmptyEntries);
            string command = parts[0].ToUpperInvariant();

            try
            {
                switch (command)
                {
                    case "DUMP":
                        if (parts.Length < 2 || parts[1].Length != 1)
                        {
                            Console.WriteLine("Usage: DUMP <letter>");
                            break;
                        }
                        await endpoint.SendDumpAsync(parts[1][0]);
                        break;

                    case "PEEL":
                        // For simplicity, send an empty board
                        await endpoint.SendPeelAsync(new char[0][]);
                        break;

                    case "EXIT":
                        Console.WriteLine("Exiting client...");
                        return;

                    default:
                        Console.WriteLine("Unknown command. Try DUMP <letter>, PEEL, or EXIT.");
                        break;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending message: {ex.Message}");
            }
        }
    }
}
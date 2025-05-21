using BannanagramsLibrary;
using BannanagramsTestClient;
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

        // Run receiver and input loop concurrently
        Task receiverTask = receiver.ListenAsync();
        Task inputTask = HandleUserInputAsync(sender);

        await Task.WhenAny(receiverTask, inputTask);
    }

    private static async Task HandleUserInputAsync(ClientMessageSender sender)
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
                        await sender.SendDumpAsync(parts[1][0]);
                        break;

                    case "PEEL":
                        // For simplicity, send an empty board
                        await sender.SendPeelAsync(new char[0][]);
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
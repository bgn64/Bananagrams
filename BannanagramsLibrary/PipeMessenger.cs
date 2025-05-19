using System.Text.Json;

namespace BannanagramsLibrary
{
    public class PipeMessenger
    {
        private readonly StreamReader reader;
        private readonly StreamWriter writer;

        public PipeMessenger(Stream input, Stream output)
        {
            reader = new StreamReader(input);
            writer = new StreamWriter(output) { AutoFlush = true };
        }

        public async Task SendAsync<T>(T message)
        {
            string json = JsonSerializer.Serialize(message);
            await writer.WriteLineAsync(json);
        }

        public async Task<T?> ReceiveAsync<T>()
        {
            string? line = await reader.ReadLineAsync();
            return line != null ? JsonSerializer.Deserialize<T>(line) : default;
        }
    }

}

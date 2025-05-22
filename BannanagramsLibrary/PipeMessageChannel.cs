using System.Text.Json;

namespace BannanagramsLibrary
{
    public class PipeMessageChannel
    {
        private readonly StreamWriter writer;
        private readonly StreamReader reader;

        public PipeMessageChannel(Stream readStream, Stream writeStream)
        {
            reader = new StreamReader(readStream);
            writer = new StreamWriter(writeStream) { AutoFlush = true };
        }

        public async Task SendAsync<T>(T message)
        {
            string json = JsonSerializer.Serialize(message);
            await writer.WriteLineAsync(json);
        }

        public async Task<T?> ReceiveAsync<T>()
        {
            string? line = await reader.ReadLineAsync();
            if (line == null) return default;
            return JsonSerializer.Deserialize<T>(line);
        }
    }
}

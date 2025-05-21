using System.Text.Json;

namespace BannanagramsLibrary
{
    public class PipeSender
    {
        private readonly StreamWriter writer;

        public PipeSender(Stream output)
        {
            writer = new StreamWriter(output) { AutoFlush = true };
        }

        public async Task SendAsync<T>(T message)
        {
            string json = JsonSerializer.Serialize(message);
            await writer.WriteLineAsync(json);
        }
    }
}

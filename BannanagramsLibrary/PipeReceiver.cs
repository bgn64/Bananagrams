using System.Text.Json;

namespace BannanagramsLibrary
{
    public class PipeReceiver
    {
        private readonly StreamReader reader;

        public PipeReceiver(Stream input)
        {
            reader = new StreamReader(input);
        }

        public async Task<T?> ReceiveAsync<T>()
        {
            string? line = await reader.ReadLineAsync();
            return JsonSerializer.Deserialize<T>(line!);
        }
    }
}

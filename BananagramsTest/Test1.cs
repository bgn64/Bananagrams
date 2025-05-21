using System.Text.Json;
using BannanagramsLibrary;

namespace BannanagramsTests
{
    [TestClass]
    public class ClientMessageReceiverTests
    {
        private static readonly JsonSerializerOptions options = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };

        [TestMethod]
        public void CastClientPayload_DumpMessage_CorrectlyCastsChar()
        {
            // Arrange
            var original = new ClientToServerMessage
            {
                Type = ClientToServerMessageType.DUMP,
                Payload = "A" // JSON will deserialize this as string
            };

            // Act
            ClientMessageReceiver.CastClientPayload(original);

            // Assert
            Assert.IsInstanceOfType(original.Payload, typeof(char));
            Assert.AreEqual('A', (char)original.Payload!);
        }

        [TestMethod]
        public void CastClientPayload_PeelMessage_CorrectlyCastsCharArrayArray()
        {
            // Arrange
            var board = new char[][] { new[] { 'A', 'B' }, new[] { 'C', 'D' } };
            var json = JsonSerializer.Serialize(board, options);
            var payload = JsonSerializer.Deserialize<object>(json, options);

            var original = new ClientToServerMessage
            {
                Type = ClientToServerMessageType.PEEL,
                Payload = payload
            };

            // Act
            ClientMessageReceiver.CastClientPayload(original);

            // Assert
            Assert.IsInstanceOfType(original.Payload, typeof(char[][]));
            var result = (char[][])original.Payload!;
            CollectionAssert.AreEqual(board[0], result[0]);
            CollectionAssert.AreEqual(board[1], result[1]);
        }
    }
}

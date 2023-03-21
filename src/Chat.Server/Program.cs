using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Server.Database;
using Chat.Server.Net;

namespace Chat.Server;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.Title = $"Chat Server - {Constants.Version}";

        var server = new ChatServer("127.0.0.1", 9000);
        PacketHandlers.RegisterPackets();
        DatabaseManager.Setup();
        server.Start();

        while (true)
        {
            var command = Console.ReadLine();

            switch (command)
            {
                case "/exit":
                    return;
            }
        }
    }
}
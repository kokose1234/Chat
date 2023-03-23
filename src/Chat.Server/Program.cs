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
            var command = Console.ReadLine() ?? "";
            var commands = command.Split(' ');

            switch (commands[0])
            {
                case "/exit":
                    return;
                case "/wipe":
                    DatabaseManager.Factory.Statement("truncate table messages");
                    DatabaseManager.Factory.Statement("truncate table channel_users");
                    DatabaseManager.Factory.Statement("truncate table channels");
                    DatabaseManager.Factory.Statement("truncate table accounts");
                    break;
            }
        }
    }
}
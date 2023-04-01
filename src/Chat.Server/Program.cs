using System.Net;
using Chat.Server.Database;
using Chat.Server.Net;

namespace Chat.Server;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.Title = $"Chat Server - {Constants.Version}";

        var server = new ChatServer(IPAddress.Any, 9000);
        PacketHandlers.RegisterPackets();
        DatabaseManager.Setup();
        server.Start();

        // var data = File.ReadAllBytes("./test.png");
        //
        // for (var i = 0; i < 10; i++)
        // {
        //     DatabaseManager.Factory.Query("messages").Insert(new
        //     {
        //         channel_id = 1,
        //         user_id = 1,
        //         date = 0,
        //         message = (string?)null,
        //         attachment = data,
        //         is_encrypted = 0
        //     });
        // }

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
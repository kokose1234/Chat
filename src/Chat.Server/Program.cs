using System.Net;
using Chat.Server.Database;
using Chat.Server.Net;
using SqlKata.Execution;

namespace Chat.Server;

internal static class Program
{
    private static void Main(string[] args)
    {
        Console.Title = $"Chat Server - {Constants.VERSION}";

        var server = new ChatServer(IPAddress.Any, 9000);
        PacketHandlers.RegisterPackets();
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
                    DatabaseManager.Factory.Statement("truncate table friends");
                    DatabaseManager.Factory.Statement("truncate table accounts");
                    break;
                case "/test":
                {
                    for (var i = 1; i <= 6; i++)
                    {
                        var data = File.ReadAllBytes($"./{i}.png");
                        DatabaseManager.Factory.Query("accounts").Where("id", i).Update(new
                        {
                            avatar = data,
                            avatar_update_date = DateTime.Now.Ticks
                        });
                    }

                    DatabaseManager.Factory.Query("accounts").Where("id", 7).Update(new
                    {
                        avatar = File.ReadAllBytes("./7.jpeg"),
                        avatar_update_date = DateTime.Now.Ticks
                    });
                    break;
                }
                case "/test2":
                {
                    DatabaseManager.Factory.Query("accounts").Where("id", 3).Update(new
                    {
                        avatar = File.ReadAllBytes("./tung.png"),
                        avatar_update_date = DateTime.Now.Ticks
                    });
                    break;
                }
            }
        }
    }
}
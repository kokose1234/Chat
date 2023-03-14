using System;
using System.Linq;
using System.Threading;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Chat.Client;
using Chat.Client.Net;
using Chat.Client.Tools;
using Chat.Client2.ViewModels;
using Chat.Client2.Views;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;

namespace Chat.Client2
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            Console.Title = $"Chat Client - {Constants.Version}";
            PacketHandlers.RegisterPackets();
            _ = ChatClient.Instance;

            AvaloniaXamlLoader.Load(this);

#if DEBUG
            new Thread(ProcessCommand).Start();
#endif
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }

#if DEBUG
        private static void ProcessCommand()
        {
            while (true)
            {
                var command = Console.ReadLine() ?? "";
                var arguments = command.Split(' ').Skip(1).ToArray();

                switch (command.Split(' ')[0])
                {
                    case "/register":
                    case "/r":
                    {
                        for (var i = 0; i < 100; i++)
                        {
                            using var packet = new OutPacket(ClientHeader.ClientAccountRegister);
                            var request = new ClientAccountRegister
                            {
                                UserName = arguments[0],
                                Password = arguments[1],
                                MacAddress = Util.GetMacAddress()
                            };

                            packet.Encode(request);
                            ChatClient.Instance.Send(packet);
                        }

                        break;
                    }
                }
            }
        }
#endif
    }
}
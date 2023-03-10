//  Copyright 2021 Jonguk Kim
//
//  Licensed under the Apache License, Version 2.0 (the "License");
//  you may not use this file except in compliance with the License.
//  You may obtain a copy of the License at
//
//      http://www.apache.org/licenses/LICENSE-2.0
//
//   Unless required by applicable law or agreed to in writing, software
//   distributed under the License is distributed on an "AS IS" BASIS,
//   WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//   See the License for the specific language governing permissions and
//   limitations under the License.

using System;
using System.Linq;
using System.Threading;
using System.Windows;
using Chat.Client.Net;
using Chat.Client.Tools;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;

namespace Chat.Client;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        Console.Title = $"ProtoChat Client - {Constants.Version}";
        PacketHandlers.RegisterPackets();
        _ = ChatClient.Instance;
        new Thread(ProcessCommand).Start();
    }

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
}
using System.Collections.Immutable;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientLogin)]
public class LoginHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        using var packet = new OutPacket(ServerHeader.ServerLogin);
        var request = inPacket.Decode<ClientLogin>();
        var account = (await DatabaseManager.Factory.Query("accounts").Where("username", request.UserName).GetAsync()).ToImmutableArray();

        if (account.Length == 0)
        {
            packet.Encode(new ServerLogin {Result = ServerLogin.LoginResult.FailedWrongInfo});
            session.Send(packet);
            return;
        }

        if (account[0].password != request.Password)
        {
            packet.Encode(new ServerLogin {Result = ServerLogin.LoginResult.FailedWrongInfo});
            session.Send(packet);
            return;
        }

        packet.Encode(new ServerLogin {Result = ServerLogin.LoginResult.Success});
        session.Send(packet);
    }
}
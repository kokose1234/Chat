using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using Chat.Common.Packet.Data.Server;
using Chat.Server.Database;
using SqlKata.Execution;

namespace Chat.Server.Net.Handlers;

[PacketHandler(ClientHeader.ClientRequestImage)]
public class RequestImageHandler : AbstractHandler
{
    internal override async Task Handle(ChatSession session, InPacket inPacket)
    {
        var request = inPacket.Decode<ClientRequestImage>();
        var response = new ServerResponseImage();

        switch (request.type)
        {
            case ClientRequestImage.Type.Profile:
                var users = await DatabaseManager.Factory.Query("accounts").WhereIn("id", request.Ids).GetAsync();

                foreach (var user in users)
                {
                    response.Datas.Add(new ServerResponseImage.Data
                    {
                        Id = user.id,
                        Image = user.avatar,
                        Update = user.avatar_update_date
                    });
                }

                response.type = ServerResponseImage.Type.Profile;
                break;
            case ClientRequestImage.Type.ChannelProfile:
                response.type = ServerResponseImage.Type.ChannelProfile;
                break;
            case ClientRequestImage.Type.Message:
                response.type = ServerResponseImage.Type.Message;
                break;
        }

        using var packet = new OutPacket(ServerHeader.ServerResponseImage);
        packet.Encode(response);
        session.Send(packet);
    }
}
using System;
using System.IO;
using System.Threading.Tasks;
using Chat.Client.Database;
using Chat.Client.Database.Entities;
using Chat.Client.Database.Repositories;
using Chat.Common.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Server;
using LiteDB;

namespace Chat.Client.Net.Handlers;

[PacketHandler(ServerHeader.ServerResponseImage)]
public class ImageHandler : AbstractHandler
{
    internal override Task Handle(ChatClient session, InPacket inPacket)
    {
        var response = inPacket.Decode<ServerResponseImage>();
        var imageRepo = DatabaseManager.GetRepository<ImageRepository>();

        switch (response.type)
        {
            case ServerResponseImage.Type.Profile:
            {
                var userRepo = DatabaseManager.GetRepository<UserRepository>();

                foreach (var data in response.Datas)
                {
                    var user = userRepo.GetUser(data.Id);
                    if (user == null)
                    {
                        user = new UserEntity {Id = ObjectId.NewObjectId(), UserId = data.Id, LastAvatarUpdate = data.Update};
                        userRepo.Add(user);
                    }

                    using var stream = new MemoryStream(data.Image);
                    user.LastAvatarUpdate = data.Update;
                    imageRepo.UploadProfileImage(data.Id, stream);
                }

                break;
            }
            case ServerResponseImage.Type.ChannelProfile:
                break;
            case ServerResponseImage.Type.Message:
                break;
        }

        return Task.CompletedTask;
    }
}
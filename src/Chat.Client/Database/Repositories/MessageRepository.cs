using System.Collections.Generic;
using Chat.Client.Database.Entities;

namespace Chat.Client.Database.Repositories;

public class MessageRepository : RepositoryBase<MessageEntity>
{
    public MessageRepository(string id) : base(id, "message") { }

    public MessageEntity? GetMessage(uint id)
    {
        var result = Collection.Query()
                               .Where(x => x.MessageId == id)
                               .FirstOrDefault();
        return result;
    }

    public IEnumerable<MessageEntity> GetMessages(uint channelId)
    {
        var result = Collection.Query()
                               .Where(x => x.ChannelId == channelId)
                               .OrderBy(x => x.MessageId)
                               .ToEnumerable();
        return result;
    }

    public IEnumerable<MessageEntity> GetAllMessages()
    {
        var result = Collection.Query()
                               .OrderBy(x => x.MessageId)
                               .ToEnumerable();
        return result;
    }

    public void AddMessage(MessageEntity message)
    {
        Collection.Insert(message);
        Collection.EnsureIndex(x => x.MessageId);
    }
}
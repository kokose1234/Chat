using System.Collections.Generic;
using Chat.Client.Database.Entities;

namespace Chat.Client.Database.Repositories;

public class ChannelRepository : RepositoryBase<ChannelEntity>
{
    public ChannelRepository(string id) : base(id, "channel") { }

    public IEnumerable<ChannelEntity>? GetAllChannels()
    {
        var result = Collection.Query()
                               .OrderBy(x => x.ChannelId)
                               .ToEnumerable();
        return result;
    }

    public ChannelEntity? GetChannel(uint id)
    {
        var result = Collection.Query()
                               .Where(x => x.ChannelId == id)
                               .FirstOrDefault();

        return result;
    }

    public void AddChannel(ChannelEntity channel)
    {
        Collection.Insert(channel);
        Collection.EnsureIndex(x => x.ChannelId);
    }

    public void UpdateChannel(ChannelEntity channel)
    {
        Collection.Update(channel);
        Collection.EnsureIndex(x => x.ChannelId);
    }
}
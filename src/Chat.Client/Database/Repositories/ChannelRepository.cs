using Chat.Client.Database.Entities;

namespace Chat.Client.Database.Repositories;

public class ChannelRepository : RepositoryBase<ChannelEntity>
{
    public ChannelRepository() : base("channel") { }

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
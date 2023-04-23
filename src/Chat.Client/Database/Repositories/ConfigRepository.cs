using Chat.Client.Database.Entities;
using LiteDB;

namespace Chat.Client.Database.Repositories;

public class ConfigRepository : RepositoryBase<ConfigEntity>
{
    public ConfigRepository(string id) : base(id, "config")
    {
        var config = Collection.FindOne(Query.All());
        if (config == null)
        {
            config = new()
            {
                Id = ObjectId.NewObjectId(),
                LastMessage = 0
            };
            Collection.Insert(config);
        }
    }

    public ConfigEntity GetConfig()
    {
        var config = Collection.FindOne(Query.All());
        return config;
    }

    public void UpdateConfig(ConfigEntity config)
    {
        Collection.Update(config);
    }
}
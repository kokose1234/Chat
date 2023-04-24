using Chat.Client.Database.Entities;

namespace Chat.Client.Database.Repositories;

public class UserRepository : RepositoryBase<UserEntity>
{
    public UserRepository(string id) : base(id, "user") { }

    public UserEntity? GetUser(uint id)
    {
        var result = Collection.Query()
                               .Where(x => x.UserId == id)
                               .FirstOrDefault();
        return result;
    }

    public void Add(UserEntity user)
    {
        Collection.Insert(user);
        Collection.EnsureIndex(x => x.UserId);
    }

    public void Update(UserEntity user)
    {
        Collection.Update(user);
        Collection.EnsureIndex(x => x.UserId);
    }
}
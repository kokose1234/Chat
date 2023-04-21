using LiteDB;

namespace Chat.Client.Database.Entities;

public abstract class EntityBase
{
    public ObjectId Id { get; set; }
}
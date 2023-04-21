using System;
using System.IO;
using Chat.Client.Database.Entities;
using LiteDB;
using Nito.AsyncEx;

namespace Chat.Client.Database.Repositories;

public abstract class RepositoryBase<T> : IDisposable, IRepository where T : EntityBase
{
    public AsyncReaderWriterLock Mutex { get; } = new();

    protected LiteDatabase Database { get; }

    protected ILiteCollection<T> Collection { get; }

    protected RepositoryBase(string name)
    {
        Database = new($"Filename=./Database/{name}.db;Password=baba1234;Upgrade=true");
        Collection = Database.GetCollection<T>();
    }

    public void Dispose()
    {
        Database.Dispose();
    }
}
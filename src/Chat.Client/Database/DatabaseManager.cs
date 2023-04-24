using System;
using System.Collections.Generic;
using System.IO;
using Chat.Client.Database.Repositories;

namespace Chat.Client.Database;

public static class DatabaseManager
{
    private static bool IsInitialized { get; set; }

    private static readonly Dictionary<Type, IRepository> Repositories = new();

    public static void Setup(string id)
    {
        if (!Directory.Exists($"./Database/{id}")) Directory.CreateDirectory($"./Database/{id}");

        Repositories.Add(typeof(ConfigRepository), new ConfigRepository(id));
        Repositories.Add(typeof(ChannelRepository), new ChannelRepository(id));
        Repositories.Add(typeof(UserRepository), new UserRepository(id));

        Repositories.Add(typeof(ImageRepository), new ImageRepository());

        IsInitialized = true;
    }

    public static T GetRepository<T>() where T : IRepository
    {
        if (!IsInitialized)
            throw new InvalidOperationException("DatabaseManager is not initialized.");

        if (!Repositories.ContainsKey(typeof(T)))
            throw new InvalidOperationException($"Repository {typeof(T).Name} is not registered.");

        return (T) Repositories[typeof(T)];
    }
}
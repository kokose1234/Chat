using System;
using System.Collections.Generic;
using System.IO;
using Chat.Client.Database.Repositories;

namespace Chat.Client.Database;

public static class DatabaseManager
{
    private static bool IsInitialized { get; set; }

    private static readonly Dictionary<Type, IRepository> Repositories = new();

    public static void Setup()
    {
        if (!Directory.Exists("./Database")) Directory.CreateDirectory("./Database");

        Repositories.Add(typeof(ConfigRepository), new ConfigRepository());
        Repositories.Add(typeof(ChannelRepository), new ChannelRepository());


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
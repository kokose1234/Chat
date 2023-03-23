using System.Data;
using MySql.Data.MySqlClient;
using Newtonsoft.Json;
using Nito.AsyncEx;
using SqlKata.Compilers;
using SqlKata.Execution;

namespace Chat.Server.Database;

internal static class DatabaseManager
{
    private static readonly DatabaseOption Options =
        JsonConvert.DeserializeObject<DatabaseOption>(File.ReadAllText("./Configs/Database.json"));

    private static QueryFactory s_factory = null!;

    internal static QueryFactory Factory
    {
        get
        {
            if ((s_factory.Connection.State == ConnectionState.Broken) |
                (s_factory.Connection.State == ConnectionState.Closed))
                Setup();

            return s_factory;
        }
    }

    public static AsyncReaderWriterLock Mutex { get; } = new();
    
    internal static void Setup()
    {
        var connection =
            new MySqlConnection(
                $"Server={Options.Host};Port={Options.Port};Database=chat;User={Options.Username};Password={Options.Password}");
        var compiler = new MySqlCompiler();
        s_factory?.Connection.Close();
        s_factory = new QueryFactory(connection, compiler);
    }
}
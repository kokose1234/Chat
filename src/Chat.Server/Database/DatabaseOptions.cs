using Newtonsoft.Json;

namespace Chat.Server.Database;

internal record struct DatabaseOption
{
    [JsonProperty("host")]
    internal string Host { get; init; }

    [JsonProperty("username")]
    internal string Username { get; init; }

    [JsonProperty("password")]
    internal string Password { get; init; }

    [JsonProperty("port")]
    internal short Port { get; init; }
}
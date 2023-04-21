namespace Chat.Server.Tools;

public static class Util
{
    private static readonly Random Random = new();

    internal static byte[] GetRandomBytes(int length)
    {
        var bytes = new byte[length];
        Random.NextBytes(bytes);
        return bytes;
    }
}
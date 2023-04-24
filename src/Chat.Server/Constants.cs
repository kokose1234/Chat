namespace Chat.Server;

internal static class Constants
{
    internal const string VERSION = "1.0.0-a";
    internal static readonly byte[] DefaultProfileImage;

    static Constants()
    {
        DefaultProfileImage = File.ReadAllBytes("./default-profile.png");
    }
}
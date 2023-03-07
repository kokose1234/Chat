namespace Chat.Common.Net.Packet.Header
{
    public enum ServerHeader : uint
    {
        NullPacket = 0,
        ServerPing = 3105391283,
        ServerAccountRegister = 3515379851,
        ServerHandshake = 23746173,
        ServerLogin = 1005611576,
    }
}
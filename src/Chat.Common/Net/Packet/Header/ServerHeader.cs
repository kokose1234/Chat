namespace Chat.Common.Net.Packet.Header
{
    public enum ServerHeader : uint
    {
        NullPacket = 0,
        ServerAccountRegister = 3515379851,
        ServerChannelSync = 2238466440,
        ServerHandshake = 23746173,
        ServerLogin = 1005611576,
        ServerMessage = 1244765267,
        ServerMessageSync = 4059829408,
        ServerPing = 3105391283,
        ServerUserList = 1889783576,
    }
}
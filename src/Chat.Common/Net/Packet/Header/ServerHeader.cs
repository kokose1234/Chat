namespace Chat.Common.Net.Packet.Header
{
    public enum ServerHeader : uint
    {
        NullPacket = 0,
        ServerAccountRegister = 3515379851,
        ServerAddFriend = 2207791083,
        ServerChannelSync = 2238466440,
        ServerCreateChannel = 1881070083,
        ServerHandshake = 23746173,
        ServerLogin = 1005611576,
        ServerMessage = 1244765267,
        ServerMessageSync = 4059829408,
        ServerPing = 3105391283,
        ServerRemoveFriend = 914356615,
        ServerSyncMusic = 3845237243,
        ServerSyncVideo = 1420054941,
        ServerUserList = 1889783576,
        ServerUserSearchResult = 4053635411,
    }
}
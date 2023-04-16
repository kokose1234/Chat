namespace Chat.Common.Net.Packet.Header
{
    public enum ClientHeader : uint
    {
        NullPacket = 0,
        ClientAccountRegister = 4183544625,
        ClientAddFriend = 1876657810,
        ClientEditProfile = 3262430192,
        ClientLogin = 1642911930,
        ClientMessage = 1726737958,
        ClientMessageSync = 3009919302,
        ClientPong = 2767742975,
        ClientRemoveFriend = 2136221031,
        ClientStartChat = 2394788432,
        ClientSyncMusic = 158472322,
        ClientSyncVideo = 3102204132,
        ClientUserSearch = 628670698,
    }
}
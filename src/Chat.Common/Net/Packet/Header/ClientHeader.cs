namespace Chat.Common.Net.Packet.Header
{
    public enum ClientHeader : uint
    {
        NullPacket = 0,
        ClientLogin = 1642911930,
        ClientPong = 2767742975,
        ClientAccountRegister = 4183544625,
        ClientMessageSync = 3009919302,
        ClientEditProfile = 3262430192,
    }
}
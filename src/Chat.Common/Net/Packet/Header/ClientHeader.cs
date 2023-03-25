namespace Chat.Common.Net.Packet.Header
{
    public enum ClientHeader : uint
    {
        NullPacket = 0,
        ClientAccountRegister = 4183544625,
        ClientEditProfile = 3262430192,
        ClientLogin = 1642911930,
        ClientMessage = 1726737958,
        ClientMessageSync = 3009919302,
        ClientPong = 2767742975,
    }
}
using System.Linq;
using System.Net.NetworkInformation;

namespace Chat.Client.Tools;

internal static class Util
{
    internal static string GetMacAddress()
    {
        return
        (
            from nic in NetworkInterface.GetAllNetworkInterfaces()
            where nic.OperationalStatus == OperationalStatus.Up
            select nic.GetPhysicalAddress().ToString()
        ).FirstOrDefault() ?? string.Empty;
    }

    internal static byte[] Encrypt(byte[] data, byte[] key)
    {
        for (var i = 0; i < data.Length; i++)
        {
            data[i] ^= key[i % key.Length];
        }

        return data;
    }

    internal static byte[] Decrypt(byte[] encryptedData, byte[] key)
    {
        for (var i = 0; i < encryptedData.Length; i++)
        {
            encryptedData[i] ^= key[i % key.Length];
        }

        return encryptedData;
    }
}
using System.IO;
using System.IO.Compression;
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

    public static byte[] Compress(byte[] buffer)
    {
        using var ms = new MemoryStream();
        using (var ds = new DeflateStream(ms, CompressionLevel.SmallestSize))
        {
            ds.Write(buffer, 0, buffer.Length);
        }

        var compressedByte = ms.ToArray();

        return compressedByte;
    }

    public static byte[] Decompress(byte[] buffer)
    {
        using var resultStream = new MemoryStream();
        using (var ms = new MemoryStream(buffer))
        {
            using (var ds = new DeflateStream(ms, CompressionMode.Decompress))
            {
                ds.CopyTo(resultStream);
                ds.Close();
            }
        }

        var decompressedByte = resultStream.ToArray();
        return decompressedByte;
    }
}
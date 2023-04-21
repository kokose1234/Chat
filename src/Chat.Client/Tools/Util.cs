using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Security.Cryptography;

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
        using var aesAlg = Aes.Create();
        aesAlg.Key = key;
        aesAlg.GenerateIV();

        var encryptor = aesAlg.CreateEncryptor(aesAlg.Key, aesAlg.IV);

        using var msEncrypt = new System.IO.MemoryStream();
        msEncrypt.Write(aesAlg.IV, 0, aesAlg.IV.Length);
        using (var csEncrypt = new CryptoStream(msEncrypt, encryptor, CryptoStreamMode.Write))
        {
            csEncrypt.Write(data, 0, data.Length);
            csEncrypt.FlushFinalBlock();
        }

        return msEncrypt.ToArray();
    }

    internal static byte[] Decrypt(byte[] encryptedData, byte[] key)
    {
        using var aesAlg = Aes.Create();
        aesAlg.Key = key;

        var iv = new byte[16];
        Array.Copy(encryptedData, 0, iv, 0, 16);
        aesAlg.IV = iv;

        var decryptor = aesAlg.CreateDecryptor(aesAlg.Key, aesAlg.IV);

        using var msDecrypt = new System.IO.MemoryStream(encryptedData);
        using var csDecrypt = new CryptoStream(msDecrypt, decryptor, CryptoStreamMode.Read);
        var decryptedData = new byte[encryptedData.Length - 16];
        csDecrypt.Read(decryptedData, 0, decryptedData.Length);
        return decryptedData;
    }
}
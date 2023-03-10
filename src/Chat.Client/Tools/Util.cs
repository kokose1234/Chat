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
}
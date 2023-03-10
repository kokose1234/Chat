using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Chat.Common.Net;

namespace Chat.Client.Net;

internal static class PacketHandlers
{
    private static readonly Dictionary<uint, AbstractHandler?> Packets = new();

    internal static void RegisterPackets()
    {
        var handlers = new Dictionary<uint, AbstractHandler>();
        foreach (var type in Assembly.GetExecutingAssembly().GetTypes())
        {
            if (Attribute.IsDefined(type, typeof(PacketHandler)))
                handlers.Add(((PacketHandler)type.GetCustomAttributes(typeof(PacketHandler), true).FirstOrDefault()!)?.Header ?? 0,
                    (AbstractHandler)Activator.CreateInstance(type)!);
        }

        foreach (var kvp in handlers.Where(kvp => !Packets.ContainsKey(kvp.Key)))
        {
            Packets.Add(kvp.Key, kvp.Value);
        }
    }

    internal static bool GetHandler(uint header, out AbstractHandler? handler) => Packets.TryGetValue(header, out handler);
}
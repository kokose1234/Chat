using System;

namespace Chat.Client.Database.Entities;

public class UserEntity : EntityBase
{
    public uint UserId { get; set; }
    public ulong LastAvatarUpdate { get; set; }
}
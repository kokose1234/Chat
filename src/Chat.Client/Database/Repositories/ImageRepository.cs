﻿using System;
using System.IO;
using System.Linq;
using LiteDB;
using Nito.AsyncEx;

namespace Chat.Client.Database.Repositories;

public class ImageRepository : IRepository, IDisposable
{
    private readonly LiteDatabase _database;
    private readonly ILiteStorage<string> _storage;

    public ImageRepository(string id)
    {
        _database = new($"Filename=./Database/{id}/file.db;Password=baba1234;Upgrade=true");
        _storage = _database.GetStorage<string>();
    }

    public void UploadProfileImage(uint userId, Stream stream) => UploadImage($"$/profile/{userId}", "profile", stream);

    public void UploadChannelImage(uint channelId, Stream stream) => UploadImage($"$/channel/{channelId}", "channel-icon", stream);

    public void UploadImage(uint messageId, Stream stream) => UploadImage($"$/message/{messageId}", "message-image", stream);

    public void UploadThumbnailImage(uint messageId, Stream stream) => UploadImage($"$/thumbnail/{messageId}", "thumbnail", stream);

    public void GetProfileImage(uint userId, Stream stream) => GetImage($"$/profile/{userId}", stream);

    public void GetChannelImage(uint channelId, Stream stream) => GetImage($"$/channel/{channelId}", stream);

    public void GetImage(uint messageId, Stream stream) => GetImage($"$/message/{messageId}", stream);

    public void GetThumbnailImage(uint messageId, Stream stream) => GetImage($"$/thumbnail/{messageId}", stream);

    public void UploadImage(string id, string name, Stream stream) => _storage.Upload(id, name, stream);

    public void GetImage(string id, Stream stream) => _storage.Download(id, stream);

    public void Dispose()
    {
        _database.Commit();
        _database.Dispose();
    }
}
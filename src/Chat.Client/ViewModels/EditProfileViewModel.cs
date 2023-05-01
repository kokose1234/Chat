using System;
using System.IO;
using System.Reactive;
using System.Windows.Forms;
using Avalonia.Media.Imaging;
using Chat.Client.Database;
using Chat.Client.Database.Repositories;
using Chat.Client.Net;
using Chat.Common.Data;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Chat.Client.ViewModels;

public class EditProfileViewModel : DialogViewModelBase, IDisposable
{
    [Reactive]
    public string Name { get; set; }

    [Reactive]
    public string Description { get; set; }

    public Bitmap Avatar
    {
        get => _avatar;
        set => this.RaiseAndSetIfChanged(ref _avatar, value);
    }

    public ReactiveCommand<Unit, Unit> EditPictureCommand { get; }

    private readonly uint _userId;

    private MemoryStream _avatarStream;
    private byte[] _avatarData;
    private Bitmap _avatar;
    private bool _changedAvatar;

    public EditProfileViewModel() { }

    public EditProfileViewModel(UserInfo info)
    {
        _userId = info.Id;
        Name = info.Name;
        Description = info.Message;
        EditPictureCommand = ReactiveCommand.Create(EditPicture);

        var repo = DatabaseManager.GetRepository<ImageRepository>();
        using var tempStream = new MemoryStream();
        repo.GetProfileImage(_userId, tempStream);

        var data = tempStream.ToArray();
        _avatarStream = new MemoryStream(data);
        Avatar = new Bitmap(_avatarStream);
    }

    private void EditPicture()
    {
        using var ofd = new OpenFileDialog
        {
            Filter = "이미지 파일 (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg",
            Multiselect = false,
            CheckFileExists = true,
            CheckPathExists = true,
            Title = "파일 선택"
        };

        if (ofd.ShowDialog() != DialogResult.OK) return;
        if (new FileInfo(ofd.FileName).Length > 5000000) return;

        var repo = DatabaseManager.GetRepository<ImageRepository>();
        _avatarData = File.ReadAllBytes(ofd.FileName);
        _avatarStream = new MemoryStream(_avatarData);
        Avatar = new Bitmap(_avatarStream);
        _changedAvatar = true;
    }

    private void SendProfileData()
    {
        using var packet = new OutPacket(ClientHeader.ClientEditProfile);
        var data = new ClientEditProfile
        {
            Name = Name,
            Message = Description,
            Picture = _changedAvatar ? _avatarData : null
        };

        packet.Encode(data);
        ChatClient.Instance.Send(packet);
    }

    private void SaveProfileImage()
    {
        var repo = DatabaseManager.GetRepository<ImageRepository>();
        using var stream = new MemoryStream(_avatarData);
        repo.UploadProfileImage(_userId, stream);
    }

    public void Dispose()
    {
        _avatarStream.Dispose();
        Avatar.Dispose();

        if (_changedAvatar) SaveProfileImage();
        SendProfileData();
    }
}
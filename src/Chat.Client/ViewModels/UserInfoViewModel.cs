using System;
using System.IO;
using System.Reactive;
using Avalonia.Media.Imaging;
using Chat.Client.Data;
using Chat.Client.Database;
using Chat.Client.Database.Repositories;
using Chat.Client.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Chat.Client.ViewModels;

public sealed class UserInfoViewModel : DialogViewModelBase, IDisposable
{
    public UserSearchResult UserInfo { get; }

    public Bitmap Avatar { get; }

    [Reactive]
    public bool IsFriend { get; set; }

    public ReactiveCommand<Unit, Unit> AddFriendCommand { get; }

    public ReactiveCommand<Unit, Unit> RemoveFriendCommand { get; }

    public ReactiveCommand<Unit, Unit> StartChatCommand { get; }

    private readonly MainWindowViewModel _mainWindowViewModel;
    private readonly MemoryStream _avatarStream;


    public UserInfoViewModel()
    {
        UserInfo = new()
        {
            Id = 1,
            Username = "@bigjaeseok",
            Nickname = "대재석",
            Message = "대 재 석",
        };
    }

    public UserInfoViewModel(UserSearchResult userInfo, bool isFriend, MainWindowViewModel mainWindowViewModel)
    {
        IsFriend = isFriend;
        UserInfo = userInfo;
        _mainWindowViewModel = mainWindowViewModel;
        AddFriendCommand = ReactiveCommand.Create(AddFriend);
        RemoveFriendCommand = ReactiveCommand.Create(RemoveFriend);
        StartChatCommand = ReactiveCommand.Create(StartChat);

        var repo = DatabaseManager.GetRepository<ImageRepository>();
        using var mutex = repo.Mutex.ReaderLock();
        using var tempStream = new MemoryStream();
        repo.GetProfileImage(userInfo.Id, tempStream);

        var data = tempStream.ToArray();
        _avatarStream = new MemoryStream(data);
        Avatar = new Bitmap(_avatarStream);
    }

    private void AddFriend()
    {
        using var packet = new OutPacket(ClientHeader.ClientAddFriend);
        var data = new ClientAddFriend {Id = UserInfo.Id};

        packet.Encode(data);
        ChatClient.Instance.Send(packet);
        IsFriend = true;
    }

    private void RemoveFriend()
    {
        using var packet = new OutPacket(ClientHeader.ClientRemoveFriend);
        var data = new ClientRemoveFriend {Id = UserInfo.Id};

        packet.Encode(data);
        ChatClient.Instance.Send(packet);
        IsFriend = false;
    }

    private void StartChat()
    {
        using var packet = new OutPacket(ClientHeader.ClientStartChat);
        var data = new ClientStartChat
        {
            UserIds = new[] {UserInfo.Id},
            IsSecret = true,
            Name = null
        };

        packet.Encode(data);
        ChatClient.Instance.Send(packet);
        _mainWindowViewModel.SearchTerm = string.Empty;
    }

    public void Dispose()
    {
        _avatarStream.Dispose();
        Avatar.Dispose();
        AddFriendCommand.Dispose();
        RemoveFriendCommand.Dispose();
        StartChatCommand.Dispose();
    }
}
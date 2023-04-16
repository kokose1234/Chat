using System.IO;
using System.Reactive;
using Avalonia.Media.Imaging;
using Chat.Client.Data;
using Chat.Client.Net;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Chat.Client.ViewModels;

//TODO: https://dev.to/ingvarx/dialogs-in-avaloniaui-3pl0
public sealed class UserInfoViewModel : DialogViewModelBase
{
    public UserSearchResult UserInfo { get; }

    public Bitmap Avatar { get; }

    [Reactive]
    public bool IsFriend { get; set; }

    public ReactiveCommand<Unit, Unit> AddFriendCommand { get; }

    public ReactiveCommand<Unit, Unit> RemoveFriendCommand { get; }

    public ReactiveCommand<Unit, Unit> StartChatCommand { get; }


    private MainWindowViewModel _mainWindowViewModel;


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

        if (File.Exists(userInfo.Avatar)) Avatar = new Bitmap(userInfo.Avatar);
        AddFriendCommand = ReactiveCommand.Create(AddFriend);
        RemoveFriendCommand = ReactiveCommand.Create(RemoveFriend);
        StartChatCommand = ReactiveCommand.Create(StartChat);
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
        if (!IsFriend) AddFriend();

        using var packet = new OutPacket(ClientHeader.ClientStartChat);
        var data = new ClientStartChat {Id = UserInfo.Id};

        packet.Encode(data);
        ChatClient.Instance.Send(packet);
    }
}
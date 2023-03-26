using System;
using System.Collections.Generic;
using System.Linq;
using System.Reactive;
using Chat.Client.Data;
using Chat.Client.Models;
using Chat.Client.Net;
using Chat.Client.Tools;
using Chat.Common.Data;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Chat.Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        #region Login

        [Reactive]
        public string Username { get; set; } = string.Empty;

        [Reactive]
        public string Password { get; set; } = string.Empty;

        [Reactive]
        public LoginMessage LoginMessage { get; set; } = new();

        [Reactive]
        public bool IsLogined { get; set; }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }
        public ReactiveCommand<Unit, Unit> RegisterCommand { get; }

        #endregion

        #region Messages

        public IObservableCollection<Channel> Channels { get; } = new ObservableCollectionExtended<Channel>();
        public IObservableCollection<Message> Messages { get; } = new ObservableCollectionExtended<Message>();
        public IObservableCollection<Message> CurrentMessages { get; } = new ObservableCollectionExtended<Message>();

        [Reactive]
        public Channel? SelectedChannel { get; set; }

        #endregion

        public List<UserInfo> Users { get; } = new();

        [Reactive]
        public string SearchTerm { get; set; } = string.Empty;

        public MainWindowViewModel()
        {
            ChatClient.Instance.ViewModel = this;
            LoginCommand = ReactiveCommand.Create(Login);
            RegisterCommand = ReactiveCommand.Create(Register);

            this.WhenAnyValue(x => x.SelectedChannel)
                .Subscribe(channel =>
                {
                    if (channel != null)
                    {
                        CurrentMessages.Clear();
                        CurrentMessages.AddRange(Messages.Where(m => m.ChannelId == channel.Id));
                    }
                });
        }

        private void Login()
        {
            using var packet = new OutPacket(ClientHeader.ClientLogin);
            var request = new ClientLogin
            {
                UserName = Username,
                Password = Password,
                MacAddress = Util.GetMacAddress()
            };

            packet.Encode(request);
            ChatClient.Instance.Send(packet);
        }

        private void Register()
        {
            using var packet = new OutPacket(ClientHeader.ClientAccountRegister);
            var request = new ClientLogin
            {
                UserName = Username,
                Password = Password,
                MacAddress = Util.GetMacAddress()
            };

            packet.Encode(request);
            ChatClient.Instance.Send(packet);
        }

        public void AddMessage(Message message)
        {
            if (Channels.All(x => x.Id != message.ChannelId))
            {
                Channels.Add(new Channel
                {
                    Id = message.ChannelId,
                    Name = Users.FirstOrDefault(x => x.Id == message.Sender)?.Name ?? "테스트",
                    Description = message.Text
                });
            }

            Messages.Add(message);
        }
    }
}
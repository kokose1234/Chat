using System.Reactive;
using Chat.Client.Net;
using Chat.Client.Tools;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;

namespace Chat.Client.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        [Reactive]
        public string Username { get; set; } = string.Empty;

        [Reactive]
        public string Password { get; set; } = string.Empty;

        [Reactive]
        public string ErrorMessage { get; set; } = string.Empty;

        [Reactive]
        public bool IsLogined { get; set; }

        public ReactiveCommand<Unit, Unit> LoginCommand { get; }

        public MainWindowViewModel()
        {
            ChatClient.Instance.ViewModel = this;
            LoginCommand = ReactiveCommand.Create(Login);
        }

        private void Login()
        {
            using var packet = new OutPacket(ClientHeader.ClientLogin);
            var request = new ClientLogin()
            {
                UserName = Username,
                Password = Password,
                MacAddress = Util.GetMacAddress()
            };

            packet.Encode(request);
            ChatClient.Instance.Send(packet);
        }
    }
}
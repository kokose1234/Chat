using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using Avalonia.Controls;
using Chat.Client.Models;
using Chat.Client.Net;
using Chat.Client.Tools;
using Chat.Common.Data;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using DynamicData;
using DynamicData.Binding;
using NAudio.Wave;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using Message = Chat.Common.Data.Message;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;

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
        public IObservableCollection<ChatMessage> Messages { get; } = new ObservableCollectionExtended<ChatMessage>();
        public IObservableCollection<ChatMessage> CurrentMessages { get; } = new ObservableCollectionExtended<ChatMessage>();

        [Reactive]
        public Channel? SelectedChannel { get; set; }

        [Reactive]
        public string ChatMessage { get; set; } = string.Empty;

        [Reactive]
        public bool IsPlayingMusic { get; set; }

        [Reactive]
        public bool IsMusicPaused { get; set; }

        public Thread? MusicThread { get; set; }


        public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }
        public ReactiveCommand<Unit, Unit> AttachCommand { get; }

        #endregion

        public List<UserInfo> Users { get; } = new();

        [Reactive]
        public string SearchTerm { get; set; } = string.Empty;

        private Window _window;

        public MainWindowViewModel() { }

        public MainWindowViewModel(Window window)
        {
            _window = window;
            ChatClient.Instance.ViewModel = this;
            LoginCommand = ReactiveCommand.Create(Login);
            RegisterCommand = ReactiveCommand.Create(Register);
            SendMessageCommand = ReactiveCommand.Create(SendMessage);
            AttachCommand = ReactiveCommand.Create(Attach);

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
            var msg = new ChatMessage
            {
                // Id = message.Id,
                ChannelId = message.ChannelId,
                SenderId = message.Sender,
                Message = message.Text,
                Time = message.Date ?? DateTime.Now
            };
            Channels.First(x => x.Id == message.ChannelId).Description = message.Text;
            msg.SenderName = Users.First(x => x.Id == message.Sender).Name;

            Messages.Add(msg);

            if (SelectedChannel != null && SelectedChannel.Id == message.ChannelId)
            {
                CurrentMessages.Add(msg);
            }
        }

        public void PlayMusic(byte[] data)
        {
            try
            {
                IsPlayingMusic = true;
                IsMusicPaused = false;

                using var ms = new MemoryStream(data);
                using var rdr = new Mp3FileReader(ms);
                using var wavStream = WaveFormatConversionStream.CreatePcmStream(rdr);
                using var baStream = new BlockAlignReductionStream(wavStream);
                using var waveOut = new WaveOut(WaveCallbackInfo.FunctionCallback());

                waveOut.Volume = 0.5f;
                waveOut.Init(baStream);
                waveOut.Play();

                while (waveOut.PlaybackState == PlaybackState.Playing)
                {
                    Thread.Sleep(10);
                }
            }
            catch (ThreadInterruptedException)
            {
                // TODO: Log
            }
            finally
            {
                IsPlayingMusic = false;
                IsMusicPaused = false;
            }
        }

        private void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(ChatMessage))
            {
                using var packet = new OutPacket(ClientHeader.ClientMessage);
                var request = new ClientMessage
                {
                    Message = ChatMessage,
                    IsEncrypted = false
                };
                packet.Encode(request);
                ChatClient.Instance.Send(packet);
            }

            ChatMessage = string.Empty;
        }

        private void Attach()
        {
            using var ofd = new OpenFileDialog
            {
                Filter = "MP3 파일 (*.mp3)|*.mp3",
                Multiselect = false
            };
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var file = File.ReadAllBytes(ofd.FileName);
            var data = new byte[file.Length + 1];
            using var packet = new OutPacket(ClientHeader.ClientMessage);

            data[0] = 1; //TODO: Enum
            Buffer.BlockCopy(file, 0, data, 1, file.Length);
            var request = new ClientMessage
            {
                Message = Path.GetFileName(ofd.FileName),
                Attachment = data,
                IsEncrypted = false
            };
            packet.Encode(request);
            ChatClient.Instance.Send(packet);
        }
    }
}
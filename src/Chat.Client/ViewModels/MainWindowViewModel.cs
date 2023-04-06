using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Threading;
using Avalonia.Controls;
using Chat.Client.Data;
using Chat.Client.Data.Types;
using Chat.Client.Models;
using Chat.Client.Net;
using Chat.Client.Tools;
using Chat.Common.Data;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using DynamicData;
using DynamicData.Binding;
using LibVLCSharp.Shared;
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


        public ReactiveCommand<Unit, Unit> SendMessageCommand { get; }
        public ReactiveCommand<Unit, Unit> AttachCommand { get; }

        #endregion

        #region Music

        [Reactive]
        public bool IsPlayingMusic { get; set; }

        [Reactive]
        public bool IsMusicPaused { get; set; }

        [Reactive]
        public int MusicLength { get; private set; }

        private int _musicPosition;

        public int MusicPosition
        {
            get => _player?.Position ?? 0;
            set
            {
                if (_player == null) return;

                if (value < 0)
                {
                    var time = Math.Abs(value);

                    _player.Position = time;
                    this.RaiseAndSetIfChanged(ref _musicPosition, time);
                }
                else
                {
                    if (_player.Position == value)
                    {
                        this.RaiseAndSetIfChanged(ref _musicPosition, value);
                        return;
                    }

                    _player.Position = value;
                    WarpMusic(value);
                    this.RaiseAndSetIfChanged(ref _musicPosition, value);
                }
            }
        }

        private int _musicVolume;

        public byte MusicVolume
        {
            get => _player != null ? (byte) (_player.Volume * 100) : (byte) 75;
            set
            {
                if (_player != null)
                {
                    _player.Volume = value / 100f;
                    this.RaiseAndSetIfChanged(ref _musicVolume, value);
                }
            }
        }

        public ReactiveCommand<Unit, Unit> ResumeCommand { get; }
        public ReactiveCommand<Unit, Unit> PauseCommand { get; }

        private Timer _musicTimer;

        #endregion

        #region Video

        public MediaPlayer MediaPlayer { get; }

        private readonly LibVLC _libVlc = new();

        #endregion

        public List<UserInfo> Users { get; } = new();

        [Reactive]
        public string SearchTerm { get; set; } = string.Empty;

        private Window _window;
        private SoundPlayer? _player;

        public MainWindowViewModel() { }

        public MainWindowViewModel(Window window)
        {
            _window = window;
            ChatClient.Instance.ViewModel = this;
            LoginCommand = ReactiveCommand.Create(Login);
            RegisterCommand = ReactiveCommand.Create(Register);
            SendMessageCommand = ReactiveCommand.Create(SendMessage);
            AttachCommand = ReactiveCommand.Create(Attach);
            ResumeCommand = ReactiveCommand.Create(ResumeMusic);
            PauseCommand = ReactiveCommand.Create(PauseMusic);

            MediaPlayer = new MediaPlayer(_libVlc);

            _musicTimer = new Timer(state =>
            {
                if (_player is {Disposed: false})
                {
                    MusicPosition = _player.Position;
                }
            }, null, 0, 100);

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
            if (_player is {Disposed: false})
            {
                _player.Stop();
                _player.Dispose();
                _player = null;
            }

            try
            {
                _player = new SoundPlayer(data, this);
                _player.Play();
                IsPlayingMusic = true;
                IsMusicPaused = false;
                MusicLength = _player.Length;
            }
            catch (Exception)
            {
                _player?.Dispose();
            }
        }

        private void SendMessage()
        {
            if (!string.IsNullOrWhiteSpace(ChatMessage) && SelectedChannel != null)
            {
                using var packet = new OutPacket(ClientHeader.ClientMessage);
                var request = new ClientMessage
                {
                    Channel = SelectedChannel.Id,
                    Message = ChatMessage,
                };
                packet.Encode(request);
                ChatClient.Instance.Send(packet);
            }

            ChatMessage = string.Empty;
            Play();
        }

        private void Attach()
        {
            if (SelectedChannel == null) return;

            using var ofd = new OpenFileDialog
            {
                Filter = "MP3 파일 (*.mp3)|*.mp3",
                Multiselect = false
            };
            if (ofd.ShowDialog() != System.Windows.Forms.DialogResult.OK) return;

            var file = File.ReadAllBytes(ofd.FileName);
            var data = new byte[file.Length + 1];
            using var packet = new OutPacket(ClientHeader.ClientMessage);

            data[0] = (byte) AttachmentType.Music;
            Buffer.BlockCopy(file, 0, data, 1, file.Length);

            var request = new ClientMessage
            {
                Channel = SelectedChannel.Id,
                Message = Path.GetFileName(ofd.FileName),
                Attachment = data,
            };
            packet.Encode(request);
            ChatClient.Instance.Send(packet);
        }

        public void ResumeMusic()
        {
            _player?.Play();
            IsMusicPaused = false;
        }

        public void PauseMusic()
        {
            _player?.Pause();
            IsMusicPaused = true;
        }

        private void WarpMusic(int position)
        {
            if (SelectedChannel != null)
            {
                using var packet = new OutPacket(ClientHeader.ClientSyncMusic);
                var request = new ClientSyncMusic {Channel = SelectedChannel.Id, Data = position};

                packet.Encode(request);
                ChatClient.Instance.Send(packet);
            }
        }

        public void SendResumeMusicPacket()
        {
            if (SelectedChannel == null) return;

            using var packet = new OutPacket(ClientHeader.ClientSyncMusic);
            var request = new ClientSyncMusic {Channel = SelectedChannel.Id, Data = int.MinValue + 1};

            packet.Encode(request);
            ChatClient.Instance.Send(packet);
        }

        public void SendPauseMusicPacket()
        {
            if (SelectedChannel == null) return;

            using var packet = new OutPacket(ClientHeader.ClientSyncMusic);
            var request = new ClientSyncMusic {Channel = SelectedChannel.Id, Data = int.MinValue + 2};

            packet.Encode(request);
            ChatClient.Instance.Send(packet);
        }

        public void Play()
        {
            using var media = new Media(_libVlc, "./test.mp4", FromType.FromPath);
            MediaPlayer.Play(media);
        }
    }
}
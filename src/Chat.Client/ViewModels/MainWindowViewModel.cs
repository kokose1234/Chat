using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Text;
using System.Windows.Forms;
using Avalonia.Controls;
using Avalonia.Threading;
using Chat.Client.Data;
using Chat.Client.Data.Types;
using Chat.Client.Models;
using Chat.Client.Net;
using Chat.Client.Tools;
using Chat.Client.Views;
using Chat.Common.Data;
using Chat.Common.Net.Packet;
using Chat.Common.Net.Packet.Header;
using Chat.Common.Packet.Data.Client;
using DynamicData;
using DynamicData.Binding;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ChatMessage = Chat.Client.Models.ChatMessage;
using Message = Chat.Common.Data.Message;
using OpenFileDialog = System.Windows.Forms.OpenFileDialog;
using Timer = System.Threading.Timer;

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
        public IObservableCollection<UserSearchResult> SearchUsers { get; } = new ObservableCollectionExtended<UserSearchResult>();
        public IObservableCollection<ChatMessage> Messages { get; } = new ObservableCollectionExtended<ChatMessage>();
        public IObservableCollection<MessageViewModel> CurrentMessages { get; } = new ObservableCollectionExtended<MessageViewModel>();

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

        #region SearchUser

        [Reactive]
        public UserSearchResult? SelectedUser { get; set; }

        public ReactiveCommand<Unit, Unit> OpenUserInfoCommand { get; }

        #endregion

        public VideoPlayerViewModel? VideoPlayer { get; private set; }

        public List<UserInfo> Users { get; } = new();
        public List<UserInfo> Friends { get; } = new();
        public uint UserId { get; set; }

        public Action OnMessageAdded { get; set; }

        [Reactive]
        public string SearchTerm { get; set; } = string.Empty;

        private readonly Window _window;
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
            OpenUserInfoCommand = ReactiveCommand.Create(OpenUserInfo);

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
                        CurrentMessages.AddRange(Messages.Where(m => m.ChannelId == channel.Id).Select(x => new MessageViewModel(x.SenderName, x.Message, x.Time.ToString("yyyy-MM-dd tt h:mm"), x.SenderId == UserId, x.Attachment != null ? x.Id : null)));
                        OnMessageAdded?.Invoke();
                    }
                });

            this.WhenAnyValue(x => x.SearchTerm)
                .Select(x => x.Trim())
                .DistinctUntilChanged()
                .Where(x => !string.IsNullOrWhiteSpace(x))
                .ForEachAsync(SendUserSearch);
        }

        private void Login()
        {
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)) return;

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
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)) return;

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
            var text = Encoding.UTF8.GetString(message.Text);
            var msg = new ChatMessage
            {
                Id = message.Id,
                ChannelId = message.ChannelId,
                SenderId = message.Sender,
                Message = text,
                Time = message.Date ?? DateTime.Now,
                SenderName = Users.First(x => x.Id == message.Sender).Name
            };

            Channels.First(x => x.Id == message.ChannelId).Description = text;
            Messages.Add(msg);

            if (SelectedChannel != null && SelectedChannel.Id == message.ChannelId)
            {
                CurrentMessages.Add(new MessageViewModel(msg.SenderName, msg.Message, msg.Time.ToString("yyyy-MM-dd tt h:mm"), msg.SenderId == UserId));
                OnMessageAdded?.Invoke();
            }
        }

        public void AddImageMessage(Message message)
        {
            var msg = new ChatMessage
            {
                Id = message.Id,
                ChannelId = message.ChannelId,
                SenderId = message.Sender,
                Attachment = message.Attachment,
                Time = message.Date ?? DateTime.Now,
                SenderName = Users.First(x => x.Id == message.Sender).Name
            };

            Messages.Add(msg);

            if (SelectedChannel != null && SelectedChannel.Id == message.ChannelId)
            {
                CurrentMessages.Add(new MessageViewModel(msg.SenderName, msg.Message, msg.Time.ToString("yyyy-MM-dd tt h:mm"), msg.SenderId == UserId, msg.Id));
                OnMessageAdded?.Invoke();
            }
        }

        public void AddImageMessage(ChatMessage msg)
        {
            Messages.Add(msg);

            if (SelectedChannel != null && SelectedChannel.Id == msg.ChannelId)
            {
                CurrentMessages.Add(new MessageViewModel(msg.SenderName, msg.Message, msg.Time.ToString("yyyy-MM-dd tt h:mm"), msg.SenderId == UserId, msg.Id));
                OnMessageAdded?.Invoke();
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

        public void PlayVideo(string filename, byte[] data)
        {
            if (SelectedChannel == null) return;

            if (!File.Exists(Path.Combine("./Downloads", filename)))
            {
                File.WriteAllBytes(Path.Combine("./Downloads", filename), data);
            }

            Dispatcher.UIThread.Post(() =>
            {
                var window = new VideoPlayer();
                var player = new VideoPlayerViewModel(SelectedChannel.Id);
                window.DataContext = player;
                VideoPlayer = player;
                player.Play(Path.Combine("./Downloads", filename));
                window.Show();
            });
        }

        private void SendMessage()
        {
            if (SelectedChannel == null) return;
            if (SelectedChannel.IsSecret && SelectedChannel.Key.Length == 0)
            {
                var alert = new MessageViewModel("단대단 암호화 키가 없습니다.");
                CurrentMessages.Add(alert);
                OnMessageAdded?.Invoke();
                ChatMessage = string.Empty;
                return;
            }

            if (!string.IsNullOrWhiteSpace(ChatMessage) && SelectedChannel != null)
            {
                using var packet = new OutPacket(ClientHeader.ClientMessage);
                var request = new ClientMessage
                {
                    Channel = SelectedChannel.Id,
                    Message = Util.Encrypt(Encoding.UTF8.GetBytes(ChatMessage), SelectedChannel.Key)
                };
                packet.Encode(request);
                ChatClient.Instance.Send(packet);
            }

            ChatMessage = string.Empty;
        }

        private void Attach()
        {
            if (SelectedChannel == null) return;
            if (SelectedChannel.IsSecret && SelectedChannel.Key.Length == 0)
            {
                var alert = new MessageViewModel("단대단 암호화 키가 없습니다.");
                CurrentMessages.Add(alert);
                OnMessageAdded?.Invoke();
                return;
            }

            using var ofd = new OpenFileDialog
            {
                Filter = "음악 파일 (*.mp3)|*.mp3|영상 파일 (*.mp4)|*.mp4|이미지 파일 (*.png, *.jpg, *.jpeg)|*.png;*.jpg;*.jpeg",
                Multiselect = false,
                CheckFileExists = true,
                CheckPathExists = true,
                Title = "파일 선택"
            };

            if (ofd.ShowDialog() != DialogResult.OK) return;

            if (new FileInfo(ofd.FileName).Length > 30000000)
            {
                var alert = new MessageViewModel("최대 30MB의 파일만 전송할 수 있습니다.");
                CurrentMessages.Add(alert);
                OnMessageAdded?.Invoke();
                return;
            }

            var file = File.ReadAllBytes(ofd.FileName);
            var data = new byte[file.Length + 1];
            using var packet = new OutPacket(ClientHeader.ClientMessage);

            data[0] = Path.GetExtension(ofd.FileName) switch
            {
                ".png" => (byte) AttachmentType.Image,
                ".jpg" => (byte) AttachmentType.Image,
                ".jpeg" => (byte) AttachmentType.Image,
                ".mp3" => (byte) AttachmentType.Music,
                ".mp4" => (byte) AttachmentType.Video,
                _ => (byte) AttachmentType.Etc
            };

            Buffer.BlockCopy(file, 0, data, 1, file.Length);
            data = Util.Compress(data);

            if (SelectedChannel.IsSecret)
            {
                var encryptedData = Util.Encrypt(data, SelectedChannel.Key);
                var encryptedMessage = Util.Encrypt(Encoding.UTF8.GetBytes(Path.GetFileName(ofd.FileName)), SelectedChannel.Key);
                var request = new ClientMessage
                {
                    Channel = SelectedChannel.Id,
                    Message = encryptedMessage,
                    Attachment = encryptedData,
                };
                packet.Encode(request);
            }
            else
            {
                var request = new ClientMessage
                {
                    Channel = SelectedChannel.Id,
                    Message = Encoding.UTF8.GetBytes(Path.GetFileName(ofd.FileName)),
                    Attachment = data,
                };
                packet.Encode(request);
            }

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

        private void OpenUserInfo()
        {
            if (SelectedUser == null) return;

            Dispatcher.UIThread.Post(() =>
            {
                var match = Users.FirstOrDefault(x => x.Id == SelectedUser.Id, null);
                var window = new AddFriendWindow();
                var player = new UserInfoViewModel(SelectedUser, Friends.Any(x => x.Id == match.Id), this);
                window.DataContext = player;
                window.ShowDialog(_window);
            });
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

        private void SendUserSearch(string term)
        {
            using var packet = new OutPacket(ClientHeader.ClientUserSearch);
            var request = new ClientUserSearch {SearchTerm = term};
            packet.Encode(request);
            ChatClient.Instance.Send(packet);
        }
    }
}
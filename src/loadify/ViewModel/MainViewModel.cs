using System.Collections.ObjectModel;
using System.Runtime.InteropServices.ComTypes;
using Caliburn.Micro;
using loadify.Event;
using loadify.Spotify;
using loadify.View;
using MahApps.Metro.Controls.Dialogs;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class MainViewModel : ViewModelBase, IHandle<DataRefreshRequestEvent>, 
                                                IHandle<DownloadContractPausedEvent>,
                                                IHandle<AddPlaylistRequestEvent>, 
                                                IHandle<ErrorOcurredEvent>,
                                                IHandle<AddTrackRequestEvent>,
                                                IHandle<DownloadPossibleEvent>
    {
        private LoadifySession _Session;

        private MenuViewModel _Menu;
        public MenuViewModel Menu
        {
            get { return _Menu; }
            set
            {
                if (_Menu == value) return;
                _Menu = value;
                NotifyOfPropertyChange(() => Menu);
            }
        }

        private StatusViewModel _Status;
        public StatusViewModel Status
        {
            get { return _Status; }
            set
            {
                if (_Status == value) return;
                _Status = value;
                NotifyOfPropertyChange(() => Status);
            }
        }

        private PlaylistsViewModel _Playlists;
        public PlaylistsViewModel Playlists
        {
            get { return _Playlists; }
            set
            {
                if (_Playlists == value) return;
                _Playlists = value;
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        private SettingsViewModel _Settings;
        public SettingsViewModel Settings
        {
            get { return _Settings; }
            set
            {
                if (_Settings == value) return;
                _Settings = value;
                NotifyOfPropertyChange(() => Settings);
            }
        }

        private UserViewModel _LoggedInUser;
        public UserViewModel LoggedInUser
        {
            get { return _LoggedInUser; }
            set
            {
                if (_LoggedInUser == value) return;
                _LoggedInUser = value;
                NotifyOfPropertyChange(() => LoggedInUser);
            }
        }

        private bool _CanStartDownload;
        public bool CanStartDownload
        {
            get { return _CanStartDownload; }
            set
            {
                if(_CanStartDownload == value) return;
                _CanStartDownload = value;
                NotifyOfPropertyChange(() => CanStartDownload);
                NotifyOfPropertyChange(() => CanCancelDownload);
            }
        }

        public bool CanCancelDownload
        {
            get { return !CanStartDownload; }
        }

        public MainViewModel(LoadifySession session, UserViewModel loggedInUser, IEventAggregator eventAggregator, IWindowManager windowManager):
            base(eventAggregator, windowManager)
        {
            _Session = session;
            _LoggedInUser = loggedInUser;
            _Menu = new MenuViewModel(_EventAggregator, _WindowManager);
            _Status = new StatusViewModel(loggedInUser, _EventAggregator);
            _Playlists = new PlaylistsViewModel(_EventAggregator);
            _Settings = new SettingsViewModel(_EventAggregator);

            _EventAggregator.PublishOnUIThread(new DataRefreshAuthorizedEvent(_Session));
        }

        public void StartDownload()
        {
            _EventAggregator.PublishOnUIThread(new DownloadContractRequestEvent(_Session));
        }

        public void CancelDownload()
        {
            
        }

        public void Handle(DataRefreshRequestEvent message)
        {
            // accept all requests by default (debugging purposes)
            _EventAggregator.PublishOnUIThread(new DataRefreshAuthorizedEvent(_Session));
        }

        public async void Handle(DownloadContractPausedEvent message)
        {
            var view = GetView() as MainView;
            await view.ShowMessageAsync("Download Error", message.Reason 
                                        + "\nPlease resolve this error before continuing downloading");
            _EventAggregator.PublishOnUIThread(new DownloadContractResumedEvent(_Session));
        }

        public async void Handle(AddPlaylistRequestEvent message)
        {
            var view = GetView() as MainView;
            var response = await view.ShowInputAsync(message.Title, message.Content);

            _EventAggregator.PublishOnUIThread(new AddPlaylistReplyEvent(response, _Session));
        }

        public async void Handle(ErrorOcurredEvent message)
        {
            var view = GetView() as MainView;
            await view.ShowMessageAsync(message.Title, message.Content);
        }

        public async void Handle(AddTrackRequestEvent message)
        {
            var view = GetView() as MainView;
            var response = await view.ShowInputAsync(message.Title, message.Content);

            _EventAggregator.PublishOnUIThread(new AddTrackReplyEvent(response, message.Playlist, _Session));
        }

        public void Handle(DownloadPossibleEvent message)
        {
            CanStartDownload = message.Possible;
        }
    }
}

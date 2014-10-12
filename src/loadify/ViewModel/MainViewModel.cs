using System.Collections.ObjectModel;
using System.Runtime.InteropServices.ComTypes;
using Caliburn.Micro;
using loadify.Event;
using loadify.View;
using MahApps.Metro.Controls.Dialogs;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class MainViewModel : ViewModelBase, IHandle<DataRefreshRequest>, 
                                                IHandle<InvalidSettingEvent>,
                                                IHandle<DownloadPausedEvent>
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

        public MainViewModel(LoadifySession session, UserViewModel loggedInUser, IEventAggregator eventAggregator):
            base(eventAggregator)
        {
            _Session = session;
            _LoggedInUser = loggedInUser;
            _Menu = new MenuViewModel(_EventAggregator);
            _Status = new StatusViewModel(loggedInUser, _EventAggregator);
            _Playlists = new PlaylistsViewModel(_EventAggregator);
            _Settings = new SettingsViewModel(_EventAggregator);

            _EventAggregator.PublishOnUIThread(new DataRefreshDisposal(_Session));
        }

        public void StartDownload()
        {
            _EventAggregator.PublishOnUIThread(new DownloadRequestEvent(_Session));
        }

        public void Handle(DataRefreshRequest message)
        {
            // accept all requests by default (debugging purposes)
            _EventAggregator.PublishOnUIThread(new DataRefreshDisposal(_Session));
        }

        public void Handle(InvalidSettingEvent message)
        {
            var view = GetView() as MainView;
            view.ShowMessageAsync("Settings Error", message.ErrorDescription);
        }

        public async void Handle(DownloadPausedEvent message)
        {
            var view = GetView() as MainView;
            await view.ShowMessageAsync("Download Error", message.Reason 
                                        + "\nPlease resolve this error before continuing downloading");
            _EventAggregator.PublishOnUIThread(new DownloadResumedEvent(_Session));
        }
    }
}

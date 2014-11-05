using System;
using Caliburn.Micro;
using loadify.Configuration;
using loadify.Event;
using loadify.Spotify;
using loadify.View;
using MahApps.Metro.Controls.Dialogs;

namespace loadify.ViewModel
{
    public class MainViewModel : ViewModelBase, IHandle<DataRefreshRequestEvent>, 
                                                IHandle<DownloadContractPausedEvent>,
                                                IHandle<AddPlaylistRequestEvent>, 
                                                IHandle<NotificationEvent>,
                                                IHandle<AddTrackRequestEvent>,
                                                IHandle<SelectedTracksChangedEvent>,
                                                IHandle<DownloadContractCompletedEvent>,
                                                IHandle<DownloadContractResumedEvent>,
                                                IHandle<DisplayProgressEvent>,
                                                IHandle<HideProgressEvent>
    {
        private readonly LoadifySession _Session;

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

        private bool _CanStartDownload = false;
        public bool CanStartDownload
        {
            get { return _CanStartDownload; }
            set
            {
                if(_CanStartDownload == value) return;
                _CanStartDownload = value;
                NotifyOfPropertyChange(() => CanStartDownload);
            }
        }

        private bool _CanCancelDownload = false;
        public bool CanCancelDownload
        {
            get { return _CanCancelDownload; }
            set
            {
                if (_CanCancelDownload == value) return;
                _CanCancelDownload = value;
                NotifyOfPropertyChange(() => CanCancelDownload);
            }
        }

        private bool _ProgressHideRequested = false;
        private ProgressDialogController _ProgressDialogController;

        public MainViewModel(LoadifySession session, UserViewModel loggedInUser,
                             IEventAggregator eventAggregator,
                             IWindowManager windowManager,
                             ISettingsManager settingsManager):
            base(eventAggregator, windowManager, settingsManager)
        {
            _Session = session;
            LoggedInUser = loggedInUser;
            Menu = new MenuViewModel(_EventAggregator, _WindowManager);
            Status = new StatusViewModel(loggedInUser, new DownloaderViewModel(_EventAggregator, _SettingsManager),  _EventAggregator);
            Playlists = new PlaylistsViewModel(_EventAggregator, settingsManager);
            Settings = new SettingsViewModel(_EventAggregator, _SettingsManager);

            _EventAggregator.PublishOnUIThread(new DataRefreshAuthorizedEvent(_Session));
        }

        public void StartDownload()
        {
            _EventAggregator.PublishOnUIThread(new DownloadContractRequestEvent(_Session));
            CanCancelDownload = true;
            CanStartDownload = false;
        }

        public void CancelDownload()
        {
            _EventAggregator.PublishOnUIThread(new DownloadContractCancelledEvent());
        }

        public void Handle(DataRefreshRequestEvent message)
        {
            _EventAggregator.PublishOnUIThread(new DataRefreshAuthorizedEvent(_Session));
        }

        public async void Handle(DownloadContractPausedEvent message)
        {
            var view = GetView() as MainView;
            var dialogResult = await view.ShowMessageAsync("Download Paused", 
                                        message.Reason
                                        + "\nPlease resolve this issue before continuing downloading.",
                                        MessageDialogStyle.AffirmativeAndNegative);
            
            if(dialogResult == MessageDialogResult.Affirmative) // pressed "OK"
                _EventAggregator.PublishOnUIThread(new DownloadContractResumedEvent(_Session));
            else
                _EventAggregator.PublishOnUIThread(new DownloadContractCompletedEvent());
        }

        public async void Handle(AddPlaylistRequestEvent message)
        {
            var view = GetView() as MainView;
            var response = await view.ShowInputAsync("Add Playlist", "Please insert the link to the Spotify playlist you want to add.");

            _EventAggregator.PublishOnUIThread(new AddPlaylistReplyEvent(response, _Session));
        }

        public async void Handle(NotificationEvent message)
        {
            var view = GetView() as MainView;
            await view.ShowMessageAsync(message.Title, message.Content);
        }

        public async void Handle(AddTrackRequestEvent message)
        {
            var view = GetView() as MainView;
            var response = await view.ShowInputAsync(String.Format("Add Track to Playlist {0}", message.Playlist.Name), "Please insert the link to the Spotify track you want to add.");

            _EventAggregator.PublishOnUIThread(new AddTrackReplyEvent(response, message.Playlist, _Session));
        }

        public void Handle(SelectedTracksChangedEvent message)
        {
            CanStartDownload = message.SelectedTracks.Count != 0;
        }

        public void Handle(DownloadContractCompletedEvent message)
        {
            CanCancelDownload = false;
            CanStartDownload = true;
        }

        public void Handle(DownloadContractResumedEvent message)
        {
            CanCancelDownload = true;
            CanStartDownload = false;
        }

        public async void Handle(DisplayProgressEvent message)
        {
            var view = GetView() as MainView;
            _ProgressDialogController = await view.ShowProgressAsync(message.Title, message.Content);
            if (_ProgressHideRequested)
            {
                await _ProgressDialogController.CloseAsync();
                _ProgressHideRequested = false;
            }
        }

        public void Handle(HideProgressEvent message)
        {
            _ProgressHideRequested = true;

            if (_ProgressDialogController == null) return;
            if (!_ProgressDialogController.IsOpen) return;
            
            _ProgressDialogController.CloseAsync();
            _ProgressHideRequested = false;
        }
    }
}

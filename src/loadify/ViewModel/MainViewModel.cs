using System;
using System.Diagnostics;
using System.Windows.Forms.VisualStyles;
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
                                                IHandle<HideProgressEvent>,
                                                IHandle<UnselectExistingTracksRequestEvent>,
                                                IHandle<RemovePlaylistRequestEvent>
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
        }

        public void StartDownload()
        {
            _Logger.Debug("Download contract has been requested, Buttons locked");
            _EventAggregator.PublishOnUIThread(new DownloadContractRequestEvent(_Session));
            CanCancelDownload = true;
            CanStartDownload = false;
        }

        public void CancelDownload()
        {
            _Logger.Debug("Download contract has been requested to cancel");
            _EventAggregator.PublishOnUIThread(new DownloadContractCancelledEvent());
        }

        protected override void OnViewLoaded(object view)
        {
            _Logger.Debug("Main window was loaded. Broadcasting the data update event...");
            _EventAggregator.PublishOnUIThread(new DataRefreshAuthorizedEvent(_Session));
        }

        public void Handle(DataRefreshRequestEvent message)
        {
            _Logger.Debug("Data update has been requested. Authorizing the request...");
            _EventAggregator.PublishOnUIThread(new DataRefreshAuthorizedEvent(_Session));
        }

        public async void Handle(DownloadContractPausedEvent message)
        {
            _Logger.Info(String.Format("Download contract was paused. Reason: {0}", message.Reason));
            if (_SettingsManager.BehaviorSetting.SkipOnDownloadFailures)
            {
                _Logger.Debug("SkipOnDownloadFailures settings has been enabled, skipping...");
                _EventAggregator.PublishOnUIThread(new DownloadContractResumedEvent(_Session, message.DownloadIndex + 1));
                return;
            }

            var view = GetView() as MainView;
            var dialogResult = await view.ShowMessageAsync("Download Paused", 
                                                            message.Reason
                                                            + "\nPlease resolve this issue before continuing downloading.",
                                                            MessageDialogStyle.AffirmativeAndNegativeAndSingleAuxiliary, 
                                                            new MetroDialogSettings()
                                                            {
                                                                AffirmativeButtonText = "retry",
                                                                NegativeButtonText = "skip",
                                                                FirstAuxiliaryButtonText = "cancel download"
                                                            });

            switch (dialogResult) // pressed "retry"
            {
                case MessageDialogResult.Affirmative:
                {
                    _Logger.Debug("User wants to retry downloading the current track and claims to have resolved all issues. Broadcasting the download contract resume event...");
                    _EventAggregator.PublishOnUIThread(new DownloadContractResumedEvent(_Session, message.DownloadIndex));
                    break;
                }
                case MessageDialogResult.Negative: // pressed "skip"
                {
                    _Logger.Debug("User wants to skip the current track and claims to have resolved all issues. Broadcasting the download contract resume event with the next track...");
                    _EventAggregator.PublishOnUIThread(new DownloadContractResumedEvent(_Session, message.DownloadIndex + 1));
                    break;
                }
                case MessageDialogResult.FirstAuxiliary: // pressed "cancel download"
                {
                    _Logger.Debug("User cancelled the download contract after it was paused. Broadcasting the download contract complete event...");
                    _EventAggregator.PublishOnUIThread(new DownloadContractCompletedEvent());
                    break;
                }
            }
        }

        public async void Handle(AddPlaylistRequestEvent message)
        {
            _Logger.Debug("User requested to manually add a playlist");
            var view = GetView() as MainView;
            var response = await view.ShowInputAsync("Add Playlist", "Please insert the link to the Spotify playlist you want to add.");
            if (!String.IsNullOrEmpty(response))
            {
                _Logger.Debug(String.Format("Following text was entered by the user: {0}", response));
                var dialogResult =  await view.ShowMessageAsync("Add Playlist",
                                                                "Do you want to permanently add this playlist to your account?",
                                                                MessageDialogStyle.AffirmativeAndNegative,
                                                                new MetroDialogSettings()
                                                                {
                                                                    AffirmativeButtonText = "yes",
                                                                    NegativeButtonText = "no"
                                                                });
                _Logger.Debug(dialogResult == MessageDialogResult.Affirmative
                            ? "The playlist will be permanently added to the logged-in Spotify account"
                            : "The playlist won't be permanently added to the logged-in Spotify account");

                _EventAggregator.PublishOnUIThread(new AddPlaylistReplyEvent(response, _Session, dialogResult == MessageDialogResult.Affirmative));
            }
        }

        public async void Handle(NotificationEvent message)
        {
            var view = GetView() as MainView;
            await view.ShowMessageAsync(message.Title, message.Content);
        }

        public async void Handle(AddTrackRequestEvent message)
        {
            _Logger.Debug(String.Format("User requested to manually add a track to playlist {0}", message.Playlist.Name));
            var view = GetView() as MainView;
            var response = await view.ShowInputAsync(String.Format("Add Track to Playlist {0}", message.Playlist.Name), "Please insert the link to the Spotify track you want to add.");

            if (!String.IsNullOrEmpty(response))
            {
                _Logger.Debug(String.Format("Following text was entered by the user: {0}", response));
                _EventAggregator.PublishOnUIThread(new AddTrackReplyEvent(response, message.Playlist, _Session));
            }
            else
            {
                _Logger.Debug(String.Format("User cancelled the dialog to manually add a track to playlist {0}", message.Playlist.Name));
            }
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

        public async void Handle(UnselectExistingTracksRequestEvent message)
        {
            if (message.ExistingTracks.Count == 0) return;

            _Logger.Debug(String.Format("{0} tracks were detected as existing, awaiting user instructions...", message.ExistingTracks.Count));
            var view = GetView() as MainView;
            var dialogResult = await view.ShowMessageAsync("Detected existing Tracks", 
                                                            String.Format("Loadify detected that you already have {0} of the selected tracks in your download directory.\n" +
                                                            "Do you want to remove them from your download contract?",
                                                            message.ExistingTracks.Count), MessageDialogStyle.AffirmativeAndNegative, new MetroDialogSettings() { AffirmativeButtonText = "yes", NegativeButtonText = "no" });
            
            _Logger.Debug("User requested to remove existing tracks");
            _EventAggregator.PublishOnUIThread(new UnselectExistingTracksReplyEvent(dialogResult == MessageDialogResult.Affirmative));
        }

        public async void Handle(RemovePlaylistRequestEvent message)
        {
            _Logger.Debug(String.Format("User requested to manually remove playlist {0}", message.Playlist.Name));
            var view = GetView() as MainView;
            var dialogResult = await view.ShowMessageAsync("Remove Playlist",
                                                            "Do you want to permanently remove this playlist from your account?",
                                                            MessageDialogStyle.AffirmativeAndNegative,
                                                            new MetroDialogSettings()
                                                            {
                                                                AffirmativeButtonText = "yes",
                                                                NegativeButtonText = "no"
                                                            });
            _Logger.Debug(dialogResult == MessageDialogResult.Affirmative
                        ? String.Format("Playlist {0} will be removed permanently from the logged-in Spotify account",
                            message.Playlist.Name)
                        : String.Format("Playlist {0} won't be removed permanently from the logged-in Spotify account",
                            message.Playlist.Name));
            _EventAggregator.PublishOnUIThread(new RemovePlaylistReplyEvent(_Session, message.Playlist, dialogResult == MessageDialogResult.Affirmative));
        }
    }
}

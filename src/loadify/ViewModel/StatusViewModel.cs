using System;
using Caliburn.Micro;

namespace loadify.ViewModel
{
    public class StatusViewModel : ViewModelBase
    {
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

        private DownloaderViewModel _Downloader;
        public DownloaderViewModel Downloader
        {
            get { return _Downloader; }
            set
            {
                if (_Downloader == value) return;
                _Downloader = value;
                NotifyOfPropertyChange(() => Downloader);
            }
        }

        public string ConnectedUserInfo
        {
            get { return String.Format(Localization.Status.ConnectedUserInfo, LoggedInUser.Name); }
        }

        public StatusViewModel(UserViewModel loggedInUser, DownloaderViewModel downloaderViewModel, IEventAggregator eventAggregator):
            base(eventAggregator)
        {
            LoggedInUser = loggedInUser;
            Downloader = downloaderViewModel;
        }
    }
}

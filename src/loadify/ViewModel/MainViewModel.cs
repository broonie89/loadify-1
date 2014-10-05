using System.Collections.ObjectModel;
using Caliburn.Micro;
using loadify.Spotify;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class MainViewModel : PropertyChangedBase
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

        public MainViewModel(LoadifySession session):
            this()
        {
            _Session = session;
            _Playlists = new PlaylistsViewModel(_Session.GetPlaylists());
        }

        public MainViewModel()
        {
            _Menu = new MenuViewModel();
            _Status = new StatusViewModel();
            _Playlists = new PlaylistsViewModel(new PlaylistCollection());
        }
    }
}

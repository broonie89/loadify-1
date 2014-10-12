using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Event;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class DownloaderViewModel : ViewModelBase, IHandle<DownloadEvent>
    {
        private PlaylistViewModel _CurrentPlaylist;
        public PlaylistViewModel CurrentPlaylist
        {
            get { return _CurrentPlaylist; }
            set
            {
                if (_CurrentPlaylist == value) return;
                _CurrentPlaylist = value;
                NotifyOfPropertyChange(() => CurrentPlaylist);
            }
        }

        private ObservableCollection<PlaylistViewModel> _DownloadedPlaylists;
        public ObservableCollection<PlaylistViewModel> DownloadedPlaylists
        {
            get { return _DownloadedPlaylists; }
            set
            {
                if (_DownloadedPlaylists == value) return;
                _DownloadedPlaylists = value;
                NotifyOfPropertyChange(() => DownloadedPlaylists);
            }
        }

        private ObservableCollection<PlaylistViewModel> _RemainingPlaylists;
        public ObservableCollection<PlaylistViewModel> RemainingPlaylists
        {
            get { return _RemainingPlaylists; }
            set
            {
                if (_RemainingPlaylists == value) return;
                _RemainingPlaylists = value;
                NotifyOfPropertyChange(() => RemainingPlaylists);
                NotifyOfPropertyChange(() => Progress);
                NotifyOfPropertyChange(() => Active);
            }
        }

        public int Progress
        {
            get { return (RemainingPlaylists.Count != 0) ? RemainingPlaylists.Count / 100 : 0; }
        }

        public bool Active
        {
            get { return _RemainingPlaylists.Count != 0; }
        }


        public DownloaderViewModel(IEventAggregator eventAggregator):
            base(eventAggregator)
        {
            _DownloadedPlaylists = new ObservableCollection<PlaylistViewModel>();
            _RemainingPlaylists = new ObservableCollection<PlaylistViewModel>();
            _CurrentPlaylist = new PlaylistViewModel(eventAggregator);
        }

        public void Handle(DownloadEvent message)
        {
            RemainingPlaylists = new ObservableCollection<PlaylistViewModel>(message.SelectedPlaylists);

            // download init stuff
        }
    }
}

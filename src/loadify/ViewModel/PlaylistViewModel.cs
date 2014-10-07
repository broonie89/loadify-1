using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Model;

namespace loadify.ViewModel
{
    public class PlaylistViewModel : ViewModelBase
    {
        private PlaylistModel _Playlist;
        public PlaylistModel Playlist
        {
            get { return _Playlist; }
            set
            {
                if (_Playlist == value) return;
                _Playlist = value;
                NotifyOfPropertyChange(() => Playlist);
            }
        }

        public ObservableCollection<TrackViewModel> Tracks
        {
            get { return new ObservableCollection<TrackViewModel>(Playlist.Tracks.Select(track => new TrackViewModel(track))); }
            set
            {
                var tracksCollection = new ObservableCollection<TrackViewModel>(Playlist.Tracks.Select(track => new TrackViewModel(track)));
                if (tracksCollection == value) return;
                Playlist.Tracks = new List<TrackModel>(value.Select(trackViewModel => trackViewModel.Track));
                NotifyOfPropertyChange(() => Tracks);
            }
        }

        public string Name
        {
            get { return Playlist.Name; }
            set
            {
                if (Playlist.Name == value) return;
                Playlist.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public string Description
        {
            get { return Playlist.Description; }
            set
            {
                if (Playlist.Description == value) return;
                Playlist.Description = value;
                NotifyOfPropertyChange(() => Description);
            }
        }

        public string Creator
        {
            get { return Playlist.Creator; }
            set
            {
                if (Playlist.Creator == value) return;
                Playlist.Creator = value;
                NotifyOfPropertyChange(() => Creator);
            }
        }

        public PlaylistViewModel(PlaylistModel playlist)
        {
            _Playlist = playlist;
        }

        public PlaylistViewModel():
            this(new PlaylistModel())
        { }
    }
}

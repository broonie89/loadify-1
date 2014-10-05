using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Spotify;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class PlaylistsViewModel : ViewModelBase
    {
        private ObservableCollection<Playlist> _Playlists = new ObservableCollection<Playlist>();
        public ObservableCollection<Playlist> Playlists
        {
            get { return _Playlists; }
            set
            {
                if (_Playlists == value) return;
                _Playlists = value;
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        public PlaylistsViewModel(IEnumerable<Playlist> playlistCollection)
        {
            _Playlists = new ObservableCollection<Playlist>(playlistCollection);
        }
    }
}

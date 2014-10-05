using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Model;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class PlaylistsViewModel : ViewModelBase
    {
        private ObservableCollection<PlaylistModel> _Playlists = new ObservableCollection<PlaylistModel>();
        public ObservableCollection<PlaylistModel> Playlists
        {
            get { return _Playlists; }
            set
            {
                if (_Playlists == value) return;
                _Playlists = value;
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        public PlaylistsViewModel(IEnumerable<PlaylistModel> playlistCollection)
        {
            _Playlists = new ObservableCollection<PlaylistModel>(playlistCollection);
        }

        public PlaylistsViewModel():
            this(new ObservableCollection<PlaylistModel>())
        { }
    }
}

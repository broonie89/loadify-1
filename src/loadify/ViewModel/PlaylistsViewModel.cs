using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class PlaylistsViewModel : ViewModelBase, IHandle<DataRefreshDisposal>
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

        public PlaylistsViewModel(IEnumerable<PlaylistModel> playlistCollection, IEventAggregator eventAggregator):
            base(eventAggregator)
        {
            _Playlists = new ObservableCollection<PlaylistModel>(playlistCollection);
        }

        public PlaylistsViewModel(IEventAggregator eventAggregator) :
            this(new ObservableCollection<PlaylistModel>(), eventAggregator)
        { }

        public void Handle(DataRefreshDisposal message)
        {
            Playlists = new ObservableCollection<PlaylistModel>(message.Session.GetPlaylists());
            _EventAggregator.PublishOnUIThread(new PlaylistsUpdatedEvent(_Playlists.ToList()));
        }
    }
}

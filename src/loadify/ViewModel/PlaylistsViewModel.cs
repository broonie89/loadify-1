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
    public class PlaylistsViewModel : ViewModelBase, IHandle<DataRefreshDisposal>, IHandle<DownloadRequestEvent>
    {
        private ObservableCollection<PlaylistViewModel> _Playlists = new ObservableCollection<PlaylistViewModel>();
        public ObservableCollection<PlaylistViewModel> Playlists
        {
            get { return _Playlists; }
            set
            {
                if (_Playlists == value) return;
                _Playlists = value;
                NotifyOfPropertyChange(() => Playlists);
            }
        }

        public PlaylistsViewModel(IEnumerable<PlaylistViewModel> playlistCollection, IEventAggregator eventAggregator) :
            base(eventAggregator)
        {
            _Playlists = new ObservableCollection<PlaylistViewModel>(playlistCollection);
        }

        public PlaylistsViewModel(IEventAggregator eventAggregator) :
            this(new ObservableCollection<PlaylistViewModel>(), eventAggregator)
        { }

        public async void Handle(DataRefreshDisposal message)
        {
            var playlists = await message.Session.GetPlaylists();
            Playlists = new ObservableCollection<PlaylistViewModel>(playlists.Select(playlist => new PlaylistViewModel(playlist, _EventAggregator)));
        }

        public void Handle(DownloadRequestEvent message)
        {
            _EventAggregator.PublishOnUIThread(new DownloadEvent(message.Session, _Playlists.Where(playlist => playlist.AllTracksSelected)));
        }
    }
}

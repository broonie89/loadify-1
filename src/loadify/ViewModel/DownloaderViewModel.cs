using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class DownloaderViewModel : ViewModelBase, IHandle<DownloadEvent>, IHandle<DownloadResumedEvent>
    {
        private TrackViewModel _CurrentTrack;
        public TrackViewModel CurrentTrack
        {
            get { return _CurrentTrack; }
            set
            {
                if (_CurrentTrack == value) return;
                _CurrentTrack = value;
                NotifyOfPropertyChange(() => CurrentTrack);
            }
        }

        private ObservableCollection<TrackViewModel> _DownloadedTracks;
        public ObservableCollection<TrackViewModel> DownloadedTracks
        {
            get { return _DownloadedTracks; }
            set
            {
                if (_DownloadedTracks == value) return;
                _DownloadedTracks = value;
                NotifyOfPropertyChange(() => DownloadedTracks);
            }
        }

        private ObservableCollection<TrackViewModel> _RemainingTracks;
        public ObservableCollection<TrackViewModel> RemainingTracks
        {
            get { return _RemainingTracks; }
            set
            {
                if (_RemainingTracks == value) return;
                _RemainingTracks = value;
                NotifyOfPropertyChange(() => RemainingTracks);
                NotifyOfPropertyChange(() => Active);
            }
        }

        public double Progress
        {
            get
            {
                var totalTracksCount = RemainingTracks.Count + DownloadedTracks.Count();
                return (totalTracksCount != 0) ? (double)(100 / (totalTracksCount / (double) DownloadedTracks.Count)) : 0;
            }
        }

        public bool Active
        {
            get { return RemainingTracks.Count != 0; }
        }


        public DownloaderViewModel(IEventAggregator eventAggregator):
            base(eventAggregator)
        {
            _DownloadedTracks = new ObservableCollection<TrackViewModel>();
            _RemainingTracks = new ObservableCollection<TrackViewModel>();
            _CurrentTrack = new TrackViewModel(_EventAggregator);
        }

        public async void StartDownload(LoadifySession session)
        {
            foreach (var track in new ObservableCollection<TrackViewModel>(RemainingTracks))
            {
                CurrentTrack = track;

                try
                {
                    var rawTrack = await session.DownloadTrack(track.Track.UnmanagedTrack);
                    DownloadedTracks.Add(CurrentTrack);
                    RemainingTracks.Remove(CurrentTrack);
                    NotifyOfPropertyChange(() => Progress);
                }
                catch (Exception)
                {
                    _EventAggregator.PublishOnUIThread(new DownloadPausedEvent(
                                                            String.Format("{0} could not be downloaded because the logged-in Spotify account is in use",
                                                            CurrentTrack.ToString())));
                }
            }
        }

        public void Handle(DownloadEvent message)
        {
            DownloadedTracks = new ObservableCollection<TrackViewModel>();
            RemainingTracks = new ObservableCollection<TrackViewModel>(message.SelectedTracks);
            StartDownload(message.Session);
        }

        public void Handle(DownloadResumedEvent message)
        {
            StartDownload(message.Session);
        }
    }
}

using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using loadify.Audio;
using loadify.Configuration;
using loadify.Event;
using loadify.Properties;
using loadify.Spotify;
using System.Threading;

namespace loadify.ViewModel
{
    public class DownloaderViewModel : ViewModelBase, IHandle<DownloadContractStartedEvent>, 
                                                      IHandle<DownloadContractResumedEvent>,
                                                      IHandle<DownloadContractCompletedEvent>,
                                                      IHandle<DownloadContractCancelledEvent>
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

        public int CurrentTrackIndex
        {
            get { return DownloadedTracks.Count + 1; }
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
                NotifyOfPropertyChange(() => TotalTracks);
                NotifyOfPropertyChange(() => CurrentTrackIndex);
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
                NotifyOfPropertyChange(() => TotalTracks);
                NotifyOfPropertyChange(() => CurrentTrackIndex);
                NotifyOfPropertyChange(() => Active);
            }
        }

        public ObservableCollection<TrackViewModel> TotalTracks
        {
            get { return new ObservableCollection<TrackViewModel>(DownloadedTracks.Concat(RemainingTracks)); }
        }

        public double TotalProgress
        {
            get
            {
                var totalTracksCount = RemainingTracks.Count + DownloadedTracks.Count();
                return (totalTracksCount != 0) ? (100 / (totalTracksCount / (double) DownloadedTracks.Count)) : 0;
            }
        }

        private double _TrackProgress = 0;
        public double TrackProgress
        {
            get { return _TrackProgress; }
            set
            {
                if (_TrackProgress == value) return;
                _TrackProgress = value;
                NotifyOfPropertyChange(() => TrackProgress);
            }
        }

        public bool Active
        {
            get { return RemainingTracks.Count != 0; }
        }

        private CancellationTokenSource _CancellationToken = new CancellationTokenSource();

        public DownloaderViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager):
            base(eventAggregator, settingsManager)
        {
            _DownloadedTracks = new ObservableCollection<TrackViewModel>();
            _RemainingTracks = new ObservableCollection<TrackViewModel>();
            _CurrentTrack = new TrackViewModel(_EventAggregator);
        }

        public async void StartDownload(LoadifySession session, int startIndex = 0)
        {
            _CancellationToken = new CancellationTokenSource();

            try
            {
                if (!Directory.Exists(_SettingsManager.DirectorySetting.DownloadDirectory))
                    Directory.CreateDirectory(_SettingsManager.DirectorySetting.DownloadDirectory);
            }
            catch (UnauthorizedAccessException)
            {
                _EventAggregator.PublishOnUIThread(new DownloadContractPausedEvent(
                                                       String.Format("{0} could not be downloaded because the application is not " +
                                                                     "authorized to create the download directory",
                                                       CurrentTrack.ToString()),
                                                       RemainingTracks.IndexOf(CurrentTrack)));
                return;
            }
            catch (IOException)
            {
                _EventAggregator.PublishOnUIThread(new DownloadContractPausedEvent(
                                                       String.Format("{0} could not be downloaded because the path to " +
                                                                     "the download directory is not valid",
                                                       CurrentTrack.ToString()),
                                                       RemainingTracks.IndexOf(CurrentTrack)));
                return;
            }

            foreach(var track in new ObservableCollection<TrackViewModel>(RemainingTracks.Skip(startIndex)))
            {
                CurrentTrack = track;

                var playlistDownloadDirectory = String.Format("{0}/{1}", 
                                                        _SettingsManager.DirectorySetting.DownloadDirectory,
                                                        CurrentTrack.Track.Playlist.Name);
                if(!Directory.Exists(playlistDownloadDirectory))
                    Directory.CreateDirectory(playlistDownloadDirectory);

                var result = await session.DownloadTrack(CurrentTrack.Track, 
                                        new TrackDownloadService(
                                        playlistDownloadDirectory,
                                        CurrentTrack.Name,
                                        _SettingsManager.BehaviorSetting.AudioProcessor,
                                        _SettingsManager.BehaviorSetting.AudioConverter,
                                        _SettingsManager.BehaviorSetting.AudioFileDescriptor,
                                        new Mp3MetaData() 
                                        { 
                                            Title = CurrentTrack.Name,
                                            Artists = String.Join(", ", CurrentTrack.Artists.Select(artist => artist.Name)),
                                            Album = CurrentTrack.Album.Name,
                                            Year = CurrentTrack.Album.ReleaseYear,
                                            Cover = CurrentTrack.Album.Cover
                                        },
                                        progress =>
                                        {
                                            TrackProgress = progress;
                                        }),
                                        _CancellationToken.Token);

                if (result == TrackDownloadService.CancellationReason.UserInteraction)
                {
                    _EventAggregator.PublishOnUIThread(new NotificationEvent("Download cancelled", String.Format("The download contract was cancelled. \n" +
                                                                                                    "Tracks downloaded: {0}\n" +
                                                                                                    "Tracks remaining: {1}\n",
                                                                                                    DownloadedTracks.Count, RemainingTracks.Count)));
                    break;
                }
  
                if (result == TrackDownloadService.CancellationReason.None)
                {
                    DownloadedTracks.Add(CurrentTrack);
                    RemainingTracks.Remove(CurrentTrack);
                    NotifyOfPropertyChange(() => TotalProgress);
                    NotifyOfPropertyChange(() => Active);
                    NotifyOfPropertyChange(() => DownloadedTracks);
                    NotifyOfPropertyChange(() => RemainingTracks);
                    NotifyOfPropertyChange(() => CurrentTrackIndex);
                    _EventAggregator.PublishOnUIThread(new TrackDownloadComplete(CurrentTrack));        
                }
                else
                {
                    _EventAggregator.PublishOnUIThread(new DownloadContractPausedEvent(
                                                        String.Format("{0} could not be downloaded because the account being used triggered an action in another client.",
                                                        CurrentTrack.ToString()),
                                                        RemainingTracks.IndexOf(CurrentTrack)));
                    return;
                }
            }

            _EventAggregator.PublishOnUIThread(new DownloadContractCompletedEvent());
            TrackProgress = 0;
        }

        public void Handle(DownloadContractStartedEvent message)
        {
            DownloadedTracks = new ObservableCollection<TrackViewModel>();
            RemainingTracks = new ObservableCollection<TrackViewModel>(message.SelectedTracks);
            StartDownload(message.Session);
        }

        public void Handle(DownloadContractResumedEvent message)
        {
            StartDownload(message.Session, message.DownloadIndex);
        }

        public void Handle(DownloadContractCompletedEvent message)
        {
            DownloadedTracks = new ObservableCollection<TrackViewModel>();
            RemainingTracks = new ObservableCollection<TrackViewModel>();
            _TrackProgress = 0;
        }

        public void Handle(DownloadContractCancelledEvent message)
        {
            _CancellationToken.Cancel();
        }
    }
}


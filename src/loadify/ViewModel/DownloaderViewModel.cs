using System;
using System.Collections.ObjectModel;
using System.Drawing;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using loadify.Audio;
using loadify.Configuration;
using loadify.Event;
using loadify.Spotify;
using System.Threading;
using SpotifySharp;

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
            _CurrentTrack = new TrackViewModel(_EventAggregator, _SettingsManager);
        }

        public async void StartDownload(LoadifySession session, int startIndex = 0)
        {
            _CancellationToken = new CancellationTokenSource();

            var remainingTracksToDownload = RemainingTracks.Count > startIndex
                                            ? new ObservableCollection<TrackViewModel>(RemainingTracks.Skip(startIndex))
                                            : new ObservableCollection<TrackViewModel>();
            foreach (var track in remainingTracksToDownload)
            {
                CurrentTrack = track;

                try
                {
                    var trackDownloadService = new TrackDownloadService(CurrentTrack.Track,
                        _SettingsManager.BehaviorSetting.AudioProcessor,
                        _SettingsManager.BehaviorSetting.DownloadPathConfigurator)
                    {
                        Cleanup = _SettingsManager.BehaviorSetting.CleanupAfterConversion,
                        OutputDirectory = _SettingsManager.DirectorySetting.DownloadDirectory,
                        AudioConverter = _SettingsManager.BehaviorSetting.AudioConverter,
                        AudioFileDescriptor = _SettingsManager.BehaviorSetting.AudioFileDescriptor,
                        Mp3MetaData = new Mp3MetaData()
                        {
                            Title = CurrentTrack.Name,
                            Artists = CurrentTrack.Artists.Select(artist => artist.Name),
                            Album = CurrentTrack.Album.Name,
                            Year = CurrentTrack.Album.ReleaseYear,
                            Cover = CurrentTrack.Album.Cover
                        },
                        DownloadProgressUpdated = progress =>
                        {
                            TrackProgress = progress;
                        }
                    };

                    _Logger.Debug(String.Format("Configured Track download service: OutputDirectory {0}, Cleanup? {1}, Track: {2}",
                                                trackDownloadService.OutputDirectory,
                                                trackDownloadService.Cleanup ? "Yes" : "No",
                                                CurrentTrack.ToString()));
                    _Logger.Info(String.Format("Downloading {0}...", CurrentTrack.ToString()));
                    await session.DownloadTrack(trackDownloadService, _CancellationToken.Token);
                    _Logger.Debug(String.Format("Track downloaded with result: {0}", trackDownloadService.Cancellation.ToString()));

                    if (trackDownloadService.Cancellation == TrackDownloadService.CancellationReason.UserInteraction)
                    {
                        _Logger.Info("Download contract was cancelled");
                        _EventAggregator.PublishOnUIThread(new NotificationEvent("Download cancelled",
                                                                                String.Format("The download contract was cancelled. \n" +
                                                                                              "Tracks downloaded: {0}\n" +
                                                                                              "Tracks remaining: {1}\n",
                                                                                    DownloadedTracks.Count, RemainingTracks.Count)));
                        break;
                    }

                    if (trackDownloadService.Cancellation == TrackDownloadService.CancellationReason.None)
                    {
                        DownloadedTracks.Add(CurrentTrack);
                        RemainingTracks.Remove(CurrentTrack);
                        NotifyOfPropertyChange(() => TotalProgress);
                        NotifyOfPropertyChange(() => Active);
                        NotifyOfPropertyChange(() => DownloadedTracks);
                        NotifyOfPropertyChange(() => RemainingTracks);
                        NotifyOfPropertyChange(() => CurrentTrackIndex);
                        _EventAggregator.PublishOnUIThread(new TrackDownloadComplete(CurrentTrack));
                        _Logger.Info(String.Format("{0} was successfully downloaded to directory {1}", CurrentTrack.ToString(), trackDownloadService.OutputDirectory));
                    }
                    else
                    {
                        _EventAggregator.PublishOnUIThread(new DownloadContractPausedEvent(
                                                            String.Format("{0} could not be downloaded because the account being used triggered an action in another client.",
                                                            CurrentTrack.ToString()),
                                                            RemainingTracks.IndexOf(CurrentTrack)));
                        _Logger.Info("Download was paused because the account being used triggered an action in another client");
                        return;
                    }
                }
                catch (ConfigurationException exception)
                {
                    _Logger.Error("A configuration error occured", exception);
                    _EventAggregator.PublishOnUIThread(new DownloadContractPausedEvent(exception.ToString(), RemainingTracks.IndexOf(CurrentTrack)));
                    return;
                }
                catch (SpotifyException exception)
                {
                    _Logger.Error("A Spotify error occured", exception);
                    _EventAggregator.PublishOnUIThread(new DownloadContractPausedEvent(String.Format("{0} could not be download because a Spotify error occured.", 
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
            _Logger.Info(String.Format("Download contract was started for {0} tracks", message.SelectedTracks.Count()));
            DownloadedTracks = new ObservableCollection<TrackViewModel>();
            RemainingTracks = new ObservableCollection<TrackViewModel>(message.SelectedTracks);
            StartDownload(message.Session);
        }

        public void Handle(DownloadContractResumedEvent message)
        {
            _Logger.Info(String.Format("Download contract has been resumed and will be continued with track {0} of {1}", message.DownloadIndex, TotalTracks.Count));
            StartDownload(message.Session, message.DownloadIndex);
        }

        public void Handle(DownloadContractCompletedEvent message)
        {
            _Logger.Info(String.Format("Download contract has been completed, tracks downloaded: {0}", DownloadedTracks.Count));
            DownloadedTracks = new ObservableCollection<TrackViewModel>();
            RemainingTracks = new ObservableCollection<TrackViewModel>();
            _TrackProgress = 0;
        }

        public void Handle(DownloadContractCancelledEvent message)
        {
            _Logger.Debug("Attempting to cancel the current download contract...");
            _CancellationToken.Cancel();
        }
    }
}


using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using Caliburn.Micro;
using loadify.Configuration;
using loadify.Event;
using loadify.Model;

namespace loadify.ViewModel
{
    public class PlaylistViewModel : ViewModelBase, IHandle<TrackSelectedChangedEvent>,
                                                    IHandle<UnselectExistingTracksReplyEvent>
    {
        private PlaylistModel _Playlist;
        public PlaylistModel Playlist
        {
            get { return _Playlist; }
            set
            {
                if (_Playlist == value) return;
                _Playlist = value;

                if (_SettingsManager != null)
                {
                    _Logger.Debug(String.Format("Checking local existance of tracks in playlist {0}", Playlist.Name));
                    foreach (var track in Playlist.Tracks)
                    {
                        var path = _SettingsManager.BehaviorSetting.DownloadPathConfigurator.Configure(
                                            _SettingsManager.DirectorySetting.DownloadDirectory,
                                            (_SettingsManager.BehaviorSetting.AudioConverter != null
                                                ? _SettingsManager.BehaviorSetting.AudioConverter.TargetFileExtension
                                                : _SettingsManager.BehaviorSetting.AudioProcessor.TargetFileExtension),
                                            track);

                        _Logger.Debug(String.Format("Checking if track {0} exists locally ({1})...", track.Name, path));
                        track.ExistsLocally = File.Exists(path);
                        _Logger.Debug(track.ExistsLocally ? String.Format("Track {0} does exist", track.Name) : String.Format("Track {0} does not exist locally", track.Name));
                    }

                    _Logger.Info(String.Format("{0}/{1} tracks in playlist {2} were detected as existing on the local filesystem",
                                                Playlist.Tracks.Count(track => track.ExistsLocally),
                                                Playlist.Tracks.Count,
                                                Playlist.Name));
                }

                NotifyOfPropertyChange(() => Playlist);
            }
        }

        private ObservableCollection<TrackViewModel> _Tracks;
        public ObservableCollection<TrackViewModel> Tracks
        {
            get { return _Tracks; }
            set
            {
                if (_Tracks == value) return;
                _Tracks = value;
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

        public bool? AllTracksSelected
        {
            get
            {
                if ( Tracks.All(track => (bool)track.Selected) && Tracks.Count != 0 )
                    return true;
                if ( Tracks.Any(track => (bool)track.Selected) && Tracks.Count != 0 )
                    return null;

                return false;
            }
            set
            {
                if (value == null)
                    value = false;

                foreach (var track in Tracks)
                    track.Selected = (bool) value;

                if(AllTracksSelected == true && _SettingsManager.BehaviorSetting.NotifyLocalTrackDetections)
                    _EventAggregator.PublishOnUIThread(new UnselectExistingTracksRequestEvent(
                                                            new ObservableCollection<TrackViewModel>(
                                                                Tracks.Where(track => track.ExistsLocally))));

                NotifyOfPropertyChange(() => AllTracksSelected);
            }
        }

        private bool _Expanded;
        public bool Expanded
        {
            get { return _Expanded; }
            set
            {
                if (_Expanded == value) return;
                _Expanded = value;
                NotifyOfPropertyChange(() => Expanded);
            }
        }

        public ObservableCollection<TrackViewModel> SelectedTracks
        {
            get { return new ObservableCollection<TrackViewModel>(Tracks.Where(track => (bool) track.Selected)); }
        }

        public PlaylistViewModel(PlaylistModel playlist, IEventAggregator eventAggregator, ISettingsManager settingsManager):
            base(eventAggregator, settingsManager)
        {
            Playlist = playlist;
            Tracks = new ObservableCollection<TrackViewModel>(playlist.Tracks.Select(track => new TrackViewModel(track, eventAggregator)));
        }

        public PlaylistViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager):
            this(new PlaylistModel(), eventAggregator, settingsManager)
        { }

        public PlaylistViewModel(PlaylistViewModel playlistViewModel)
        {
            _EventAggregator = playlistViewModel._EventAggregator;
            _SettingsManager = playlistViewModel._SettingsManager;
            Tracks = new ObservableCollection<TrackViewModel>(playlistViewModel.Tracks);
            Playlist = new PlaylistModel(playlistViewModel.Playlist);
            AllTracksSelected = playlistViewModel.AllTracksSelected;
            Expanded = playlistViewModel.Expanded;
        }

        public void Handle(TrackSelectedChangedEvent message)
        {
            if (!Tracks.Contains(message.Track)) return;

            NotifyOfPropertyChange(() => SelectedTracks);
            NotifyOfPropertyChange(() => AllTracksSelected);

            _EventAggregator.PublishOnUIThread(new SelectedTracksChangedEvent(SelectedTracks));
        }

        public void Handle(UnselectExistingTracksReplyEvent message)
        {
            if (message.Unselect)
            {
                foreach (var existingTrack in Tracks.Where(track => track.ExistsLocally))
                    existingTrack.Selected = false;

                _Logger.Info(String.Format("{0} tracks were automatically unselected since they already exist locally", Tracks.Count(track => track.ExistsLocally)));
            }
        }
    }
}

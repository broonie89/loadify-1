using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Windows.Input;
using Caliburn.Micro;
using loadify.Configuration;
using loadify.Event;
using loadify.Model;

namespace loadify.ViewModel
{
    public class TrackViewModel : ViewModelBase
    {
        private TrackModel _Track;
        public TrackModel Track
        {
            get { return _Track; }
            set
            {
                if (_Track == value) return;
                _Track = value;
                NotifyOfPropertyChange(() => Track);
            }
        }

        public string Name
        {
            get { return Track.Name; }
            set
            {
                if (Track.Name == value) return;
                Track.Name = value;
                NotifyOfPropertyChange(() => Name);
            }
        }

        public TimeSpan Duration
        {
            get { return Track.Duration; }
            set
            {
                if (Track.Duration == value) return;
                Track.Duration = value;
                NotifyOfPropertyChange(() => Duration);
            }
        }

        public AlbumModel Album
        {
            get { return Track.Album; }
            set
            {
                if (Track.Album == value) return;
                _Track.Album = value;
                NotifyOfPropertyChange(() => Album);
            }
        }

        public List<ArtistModel> Artists
        {
            get { return Track.Artists; }
            set
            {
                if (Track.Artists == value) return;
                Track.Artists = value;
                NotifyOfPropertyChange(() => Track.Artists);
            }
        }

        public bool ExistsLocally
        {
            get { return Track.ExistsLocally; }
            set
            {
                if (Track.ExistsLocally == value) return;
                _Track.ExistsLocally = value;
                NotifyOfPropertyChange(() => ExistsLocally);
            }
        }

        private bool _Selected;
        public bool Selected
        {
            get { return _Selected; }
            set
            {
                if (_Selected == value) return;
                _Selected = value;
                NotifyOfPropertyChange(() => Selected);
                _EventAggregator.PublishOnUIThread(new TrackSelectedChangedEvent(this, _Selected));
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

        public TrackViewModel(TrackModel track, IEventAggregator eventAggregator, ISettingsManager settingsManager):
            base(eventAggregator, settingsManager)
        {
            _Track = track;
        }

        public TrackViewModel(IEventAggregator eventAggregator, ISettingsManager settingsManager) :
            this(new TrackModel(), eventAggregator, settingsManager)
        { }

        public void OnMouseDown(MouseButtonEventArgs eventArgs)
        {
            if(eventArgs.ClickCount >= 2)
                OpenContainingDirectory();
        }

        /// <summary>
        /// Opens the directory that contains the track in the windows explorer
        /// </summary>
        public void OpenContainingDirectory()
        {
            try
            {
                var trackAudioFilePath = _SettingsManager.BehaviorSetting.DownloadPathConfigurator.Configure(
                                                        _SettingsManager.DirectorySetting.DownloadDirectory,
                                                        _SettingsManager.BehaviorSetting.AudioConverter.TargetFileExtension,
                                                        Track);

                // if the audio file for that track exists, open the explorer and select (or highlight) the track using the
                // /select command line switch. Note that the path should be wrapped by quotes (") since the explorer will treat the path
                // as list of arguments otherwise if it contains whitespaces
                if (File.Exists(trackAudioFilePath))
                    Process.Start("explorer.exe", String.Format("/select, \"{0}\"", trackAudioFilePath));
            }
            catch (InvalidOperationException exception)
            {
                _EventAggregator.PublishOnUIThread(new NotificationEvent("Error", "The folder cannot be opened because there was an unhandled error"));
                _Logger.Error("The folder cannot be opened because there was an unhandled error", exception);
            }
        }

        public override string ToString()
        {
            return string.Format("{0} - {1}", String.Join(", ", Artists.Select(artist => artist.Name)), Name);
        }
    }
}

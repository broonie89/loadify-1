﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Caliburn.Micro;
using loadify.Event;
using loadify.Model;
using loadify.Properties;
using loadify.Spotify;
using SpotifySharp;

namespace loadify.ViewModel
{
    public class DownloaderViewModel : ViewModelBase, IHandle<DownloadEvent>, 
                                                      IHandle<DownloadResumedEvent>,
                                                      IHandle<DownloadProgressUpdatedEvent>
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


        public DownloaderViewModel(IEventAggregator eventAggregator):
            base(eventAggregator)
        {
            _DownloadedTracks = new ObservableCollection<TrackViewModel>();
            _RemainingTracks = new ObservableCollection<TrackViewModel>();
            _CurrentTrack = new TrackViewModel(_EventAggregator);
        }

        public async void StartDownload(LoadifySession session, int startIndex = 0)
        {
            try
            {
                if (!Directory.Exists(Settings.Default.DownloadDirectory))
                    Directory.CreateDirectory(Settings.Default.DownloadDirectory);
            }
            catch (UnauthorizedAccessException)
            {
                _EventAggregator.PublishOnUIThread(new DownloadPausedEvent(
                                                       String.Format("{0} could not be downloaded because the application is not " +
                                                                     "authorized to create the download directory",
                                                       CurrentTrack.ToString()),
                                                       RemainingTracks.IndexOf(CurrentTrack)));
                return;
            }
            catch (IOException)
            {
                _EventAggregator.PublishOnUIThread(new DownloadPausedEvent(
                                                       String.Format("{0} could not be downloaded because the path to " +
                                                                     "the download directory is not valid",
                                                       CurrentTrack.ToString()),
                                                       RemainingTracks.IndexOf(CurrentTrack)));
                return;
            }


            foreach(var track in new ObservableCollection<TrackViewModel>(RemainingTracks.Skip(startIndex)))
            {
                CurrentTrack = track;

                try
                {
                    await session.DownloadTrack(CurrentTrack.Track,
                                                new WaveAudioProcessor(Properties.Settings.Default.DownloadDirectory, CurrentTrack.Name),
                                                new WaveToMp3Converter(Properties.Settings.Default.DownloadDirectory, CurrentTrack.Name));
                    DownloadedTracks.Add(CurrentTrack);
                    RemainingTracks.Remove(CurrentTrack);
                    NotifyOfPropertyChange(() => TotalProgress);
                    NotifyOfPropertyChange(() => Active);
                    NotifyOfPropertyChange(() => DownloadedTracks);
                    NotifyOfPropertyChange(() => RemainingTracks);
                    NotifyOfPropertyChange(() => CurrentTrackIndex);
                }
                catch (PlayTokenLostException)
                {
                    _EventAggregator.PublishOnUIThread(new DownloadPausedEvent(
                                                            String.Format("{0} could not be downloaded because the logged-in" +
                                                                          " Spotify account is in use",
                                                            CurrentTrack.ToString()),
                                                            RemainingTracks.IndexOf(CurrentTrack)));
                    break;
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
            StartDownload(message.Session, message.DownloadIndex);
        }

        public void Handle(DownloadProgressUpdatedEvent message)
        {
            TrackProgress = message.Progress;
        }
    }
}
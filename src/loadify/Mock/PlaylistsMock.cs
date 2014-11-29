using System.Collections.Generic;
using System.Collections.ObjectModel;
using Caliburn.Micro;
using loadify.Configuration;
using loadify.Model;
using loadify.ViewModel;

namespace loadify.Mock
{
    /// <summary>
    /// Mock for designing the PlaylistsView. Necessarily implemented as d:DataContext for design time since at the time of
    /// programming, bindings are not available
    /// </summary>
    public class PlaylistsMock
    {
        public PlaylistsMock()
        {
            SelectedTracks = new List<TrackViewModel>
            {
                new TrackViewModel(new TrackModel() {Name = "Test"}, new EventAggregator(), new NETSettingsManager())
            };

            EstimatedDownloadTime = "48:54:22";
            Playlists = new List<PlaylistViewModel>
            {
                new PlaylistViewModel(new EventAggregator(), new NETSettingsManager())
                {
                    Creator = "Mostey",
                    Description = "Test",
                    Name = "Rock",
                    Tracks = new ObservableCollection<TrackViewModel>
                    {
                        new TrackViewModel(new EventAggregator(), new NETSettingsManager()) {Name = "Rock this Shit"},
                        new TrackViewModel(new EventAggregator(), new NETSettingsManager()) {Name = "Could it be"}
                    }
                },
                new PlaylistViewModel(new EventAggregator(), new NETSettingsManager()) {Creator = "Mostey", Description = "Test2", Name = "Hardcore"}
            };
        }

        public List<PlaylistViewModel> Playlists { get; set; }
        public List<TrackViewModel> SelectedTracks { get; set; }
        public string EstimatedDownloadTime { get; set; }
    }
}

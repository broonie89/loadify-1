using System;
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
            PlaylistsViewModel = new PlaylistsViewModel(
                new ObservableCollection<PlaylistViewModel>
                {
                    new PlaylistViewModel(new EventAggregator(), new NETSettingsManager())
                    {
                        Creator = "Mostey",
                        Description = "Test",
                        Name = "Rock",
                        Tracks = new ObservableCollection<TrackViewModel>
                        {
                            new TrackViewModel(new EventAggregator(), new NETSettingsManager()) {Name = "Rock this Shit", Selected = true, Duration = new TimeSpan(0,0,4,26)},
                            new TrackViewModel(new EventAggregator(), new NETSettingsManager()) {Name = "Could it be"}
                        }
                    },
                    new PlaylistViewModel(new EventAggregator(), new NETSettingsManager()) {Creator = "Mostey", Description = "Test2", Name = "Hardcore"}
                }, new EventAggregator(), new NETSettingsManager());
        }

        public PlaylistsViewModel PlaylistsViewModel { get; set; }
    }
}

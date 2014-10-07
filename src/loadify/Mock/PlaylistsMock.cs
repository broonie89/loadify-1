using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Caliburn.Micro;
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
            Playlists = new List<PlaylistViewModel>
            {
                new PlaylistViewModel(new EventAggregator())
                {
                    Creator = "Mostey",
                    Description = "Test",
                    Name = "Rock",
                    Tracks = new ObservableCollection<TrackViewModel>
                    {
                        new TrackViewModel(new EventAggregator()) {Name = "Rock this Shit"},
                        new TrackViewModel(new EventAggregator()) {Name = "Could it be"}
                    }
                },
                new PlaylistViewModel(new EventAggregator()) {Creator = "Mostey", Description = "Test2", Name = "Hardcore"}
            };
        }

        public List<PlaylistViewModel> Playlists { get; set; }
    }
}

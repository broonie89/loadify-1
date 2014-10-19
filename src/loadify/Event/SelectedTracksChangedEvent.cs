using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.ViewModel;

namespace loadify.Event
{
    public class SelectedTracksChangedEvent
    {
        public ObservableCollection<TrackViewModel> SelectedTracks { get; set; }

        public SelectedTracksChangedEvent(ObservableCollection<TrackViewModel> selectedTracks)
        {
            SelectedTracks = selectedTracks;
        }
    }
}

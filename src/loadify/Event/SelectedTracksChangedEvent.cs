using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using loadify.ViewModel;

namespace loadify.Event
{
    public class SelectedTracksChangedEvent
    {
        public List<TrackViewModel> SelectedTracks { get; set; }

        public SelectedTracksChangedEvent(List<TrackViewModel> selectedTracks)
        {
            SelectedTracks = selectedTracks;
        }
    }
}

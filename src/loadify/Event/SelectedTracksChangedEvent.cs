using System.Collections.ObjectModel;
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

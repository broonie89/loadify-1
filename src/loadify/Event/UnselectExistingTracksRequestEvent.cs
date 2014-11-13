using System.Collections.ObjectModel;
using loadify.ViewModel;

namespace loadify.Event
{
    public class UnselectExistingTracksRequestEvent
    {
        public ObservableCollection<TrackViewModel> ExistingTracks { get; set; }

        public UnselectExistingTracksRequestEvent(ObservableCollection<TrackViewModel> existingTracks)
        {
            ExistingTracks = existingTracks;
        }
    }
}

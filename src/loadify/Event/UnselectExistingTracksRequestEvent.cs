using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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

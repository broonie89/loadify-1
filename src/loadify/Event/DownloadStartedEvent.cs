using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Spotify;
using loadify.ViewModel;

namespace loadify.Event
{
    public class DownloadStartedEvent : SessionEvent
    {
        public IEnumerable<TrackViewModel> SelectedTracks { get; set; }

        public DownloadStartedEvent(LoadifySession session, IEnumerable<TrackViewModel> selectedTracks) :
            base(session)
        {
            SelectedTracks = selectedTracks; 
        }
    }
}

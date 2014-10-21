using System.Collections.Generic;
using loadify.Spotify;
using loadify.ViewModel;

namespace loadify.Event
{
    public class DownloadContractStartedEvent : SessionEvent
    {
        public IEnumerable<TrackViewModel> SelectedTracks { get; set; }

        public DownloadContractStartedEvent(LoadifySession session, IEnumerable<TrackViewModel> selectedTracks) :
            base(session)
        {
            SelectedTracks = selectedTracks; 
        }
    }
}

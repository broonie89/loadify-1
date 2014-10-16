using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Spotify;
using loadify.ViewModel;

namespace loadify.Event
{
    public class DownloadEvent
    {
        public LoadifySession Session { get; set; }
        public IEnumerable<TrackViewModel> SelectedTracks { get; set; }

        public DownloadEvent(LoadifySession session, IEnumerable<TrackViewModel> selectedTracks)
        {
            Session = session;
            SelectedTracks = selectedTracks; 
        }
    }
}

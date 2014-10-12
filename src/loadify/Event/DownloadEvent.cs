using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.ViewModel;

namespace loadify.Event
{
    public class DownloadEvent
    {
        public LoadifySession Session { get; set; }
        public IEnumerable<PlaylistViewModel> SelectedPlaylists { get; set; }

        public DownloadEvent(LoadifySession session, IEnumerable<PlaylistViewModel> selectedPlaylists)
        {
            Session = session;
            SelectedPlaylists = selectedPlaylists; 
        }
    }
}

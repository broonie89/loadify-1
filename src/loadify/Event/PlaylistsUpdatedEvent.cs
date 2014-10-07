using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Model;
using loadify.ViewModel;

namespace loadify.Event
{
    public class PlaylistsUpdatedEvent
    {
        public List<PlaylistViewModel> Playlists { get; set; }

        public PlaylistsUpdatedEvent(List<PlaylistViewModel> playlists)
        {
            Playlists = playlists;
        }
    }
}

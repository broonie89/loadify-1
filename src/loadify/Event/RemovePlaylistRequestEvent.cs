using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Model;
using loadify.ViewModel;

namespace loadify.Event
{
    public class RemovePlaylistRequestEvent
    {
        public PlaylistViewModel Playlist { get; set; }

        public RemovePlaylistRequestEvent(PlaylistViewModel playlist)
        {
            Playlist = playlist;
        }
    }
}

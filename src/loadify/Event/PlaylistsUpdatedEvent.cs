using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Model;

namespace loadify.Event
{
    public class PlaylistsUpdatedEvent
    {
        public List<PlaylistModel> Playlists { get; set; }

        public PlaylistsUpdatedEvent(List<PlaylistModel> playlists)
        {
            Playlists = playlists;
        }
    }
}

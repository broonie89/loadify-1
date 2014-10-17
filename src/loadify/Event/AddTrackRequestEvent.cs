using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.ViewModel;

namespace loadify.Event
{
    public class AddTrackRequestEvent : UserInputRequestEvent
    {
        public PlaylistViewModel Playlist { get; set; }

        public AddTrackRequestEvent(string title, string content, PlaylistViewModel playlist) :
            base(title, content)
        {
            Playlist = playlist;
        }
    }
}

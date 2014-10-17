using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Spotify;

namespace loadify.Event
{
    public class AddPlaylistReplyEvent : SessionEvent
    {
        public string Url { get; set; }

        public AddPlaylistReplyEvent(string url, LoadifySession session):
            base(session)
        {
            Url = url;
        }
    }
}

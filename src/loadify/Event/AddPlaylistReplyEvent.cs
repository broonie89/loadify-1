using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Spotify;

namespace loadify.Event
{
    public class AddPlaylistReplyEvent
    {
        public string Url { get; set; }
        public LoadifySession Session { get; set; }

        public AddPlaylistReplyEvent(string url, LoadifySession session)
        {
            Url = url;
            Session = session;
        }
    }
}

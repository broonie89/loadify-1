using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Spotify;

namespace loadify.Event
{
    public class SessionEvent
    {
        public LoadifySession Session { get; set; }

        public SessionEvent(LoadifySession session)
        {
            Session = session;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Spotify;

namespace loadify.Event
{
    public class DownloadResumedEvent
    {
        public LoadifySession Session { get; set; }

        public DownloadResumedEvent(LoadifySession session)
        {
            Session = session;
        }
    }
}

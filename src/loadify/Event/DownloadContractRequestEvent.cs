using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Spotify;

namespace loadify.Event
{
    public class DownloadContractRequestEvent : SessionEvent
    {
        public DownloadContractRequestEvent(LoadifySession session)
            : base(session)
        { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Event
{
    public class DownloadPausedEvent
    {
        public string Reason { get; set; }

        public DownloadPausedEvent(string reason = null)
        {
            Reason = reason;
        }
    }
}

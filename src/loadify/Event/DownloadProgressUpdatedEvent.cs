using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Event
{
    public class DownloadProgressUpdatedEvent
    {
        public double Progress { get; set; }

        public DownloadProgressUpdatedEvent(double progress)
        {
            Progress = progress;
        }
    }
}

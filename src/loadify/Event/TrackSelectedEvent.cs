using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.ViewModel;

namespace loadify.Event
{
    public class TrackSelectedEvent
    {
        public TrackViewModel Track { get; set; }

        public TrackSelectedEvent(TrackViewModel track)
        {
            Track = track;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.ViewModel;

namespace loadify.Event
{
    public class TrackSelectedChangedEvent
    {
        public bool? IsSelected { get; set; }
        public TrackViewModel Track { get; set; }

        public TrackSelectedChangedEvent(TrackViewModel track, bool? isSelected)
        {
            Track = track;
            IsSelected = isSelected;
        }
    }
}

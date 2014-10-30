using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Model;
using loadify.ViewModel;

namespace loadify.Event
{
    public class TrackDownloadComplete
    {
        public TrackViewModel Track { get; set; }

        public TrackDownloadComplete(TrackViewModel track)
        {
            Track = track;
        }
    }
}

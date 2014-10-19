using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Event
{
    public class DownloadPossibleEvent
    {
        public bool Possible { get; set; }

        public DownloadPossibleEvent(bool possible)
        {
            Possible = possible;
        }
    }
}

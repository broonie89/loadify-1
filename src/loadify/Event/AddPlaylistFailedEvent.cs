using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Event
{
    public class AddPlaylistFailedEvent
    {
        public string Url { get; set; }

        public AddPlaylistFailedEvent(string url)
        {
            Url = url;
        }
    }
}

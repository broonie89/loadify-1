using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Event
{
    public class UnselectExistingTracksReplyEvent
    {
        public bool Unselect { get; set; }

        public UnselectExistingTracksReplyEvent(bool unselect)
        {
            Unselect = unselect;
        }
    }
}

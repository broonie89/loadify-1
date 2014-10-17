using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Event
{
    public class AddPlaylistRequestEvent : UserInputRequestEvent
    {
        public AddPlaylistRequestEvent(string title, string content):
            base(title, content)
        { }
    }
}

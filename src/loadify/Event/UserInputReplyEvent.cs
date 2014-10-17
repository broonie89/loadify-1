using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Event
{
    public class UserInputReplyEvent
    {
        public string Content { get; set; }

        public UserInputReplyEvent(string content)
        {
            Content = content;
        }
    }
}

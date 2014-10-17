using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Event
{
    public class ErrorOcurredEvent
    {
        public string Title { get; set; }
        public string Content { get; set; }

        public ErrorOcurredEvent(string title, string content)
        {
            Title = title;
            Content = content;
        }
    }
}

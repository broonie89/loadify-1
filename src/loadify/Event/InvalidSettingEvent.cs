using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Event
{
    public class InvalidSettingEvent
    {
        public string ErrorDescription { get; set; }

        public InvalidSettingEvent(string errorDescription)
        {
            ErrorDescription = errorDescription;
        }
    }
}

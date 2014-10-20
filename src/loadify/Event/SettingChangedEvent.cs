using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Configuration;

namespace loadify.Event
{
    public class SettingChangedEvent<T>
    {
        public T Setting { get; set; }

        public SettingChangedEvent(T setting)
        {
            Setting = setting;
        }
    }
}

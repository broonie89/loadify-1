using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Configuration;

namespace loadify.Event
{
    public class DirectorySettingChangedEvent
    {
        public IDirectorySetting DirectorySetting { get; set; }

        public DirectorySettingChangedEvent(IDirectorySetting directorySetting)
        {
            DirectorySetting = directorySetting;
        }
    }
}

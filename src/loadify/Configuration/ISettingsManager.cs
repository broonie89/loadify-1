using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Configuration
{
    public interface ISettingsManager
    {
        IDirectorySetting DirectorySetting { get; set; }
        IConnectionSetting ConnectionSetting { get; set; }
        IBehaviorSetting BehaviorSetting { get; set; }
    }
}

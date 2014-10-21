using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Configuration
{
    public class NETSettingsManager : ISettingsManager
    {
        public IDirectorySetting DirectorySetting { get; set; }
        public IConnectionSetting ConnectionSetting { get; set; }
        public IBehaviorSetting BehaviorSetting { get; set; }
        public ICredentialsSetting CredentialsSetting { get; set; }

        public NETSettingsManager()
        {
            DirectorySetting = new NETDirectorySetting();
            ConnectionSetting = new NETConnectionSetting();
            BehaviorSetting = new NETBehaviorSetting();
            CredentialsSetting = new NETCredentialsSetting();
        }
    }
}

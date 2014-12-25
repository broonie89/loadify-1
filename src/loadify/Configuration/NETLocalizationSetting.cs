using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Localization;
using Settings = loadify.Properties.Settings;

namespace loadify.Configuration
{
    public class NETLocalizationSetting : ILocalizationSetting
    {
        public ILocalizationManager LocalizationManager { get; set; }

        public Language UILanguage
        {
            get { return new Language(Settings.Default.UILanguage); }
            set
            {
                if (value == null) return;
                Settings.Default.UILanguage = value.Code;
                Settings.Default.Save();
                
                if(LocalizationManager != null)
                    LocalizationManager.SetLanguage(value);
            }
        }

        public NETLocalizationSetting()
        {
            LocalizationManager = new ResxLocalizationManager();
            LocalizationManager.SetLanguage(UILanguage);
        }
    }
}

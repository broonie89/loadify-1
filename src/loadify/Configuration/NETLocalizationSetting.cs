using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Localization;

namespace loadify.Configuration
{
    public class NETLocalizationSetting : ILocalizationSetting
    {
        public ILocalizationManager LocalizationManager { get; set; }

        public NETLocalizationSetting()
        {
            LocalizationManager = new ResxLocalizationManager();
        }
    }
}

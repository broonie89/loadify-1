using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Localization;

namespace loadify.Configuration
{
    public interface ILocalizationSetting
    {
        ILocalizationManager LocalizationManager { get; set; }
        Language UILanguage { get; set; }
    }
}

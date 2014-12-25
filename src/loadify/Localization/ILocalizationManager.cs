using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Localization
{
    public interface ILocalizationManager
    {
        Language GetCurrentLanguage();
        void SetLanguage(Language language);
        IEnumerable<Language> GetSupportedLanguages();
    }
}

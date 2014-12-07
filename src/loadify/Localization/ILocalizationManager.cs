using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Localization
{
    public interface ILocalizationManager
    {
        void SetLanguage(Language language);
        IEnumerable<Language> GetSupportedLanguages();
    }
}

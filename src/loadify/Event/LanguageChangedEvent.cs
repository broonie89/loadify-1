using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Localization;

namespace loadify.Event
{
    /// <summary>
    /// Event that notifies subscribing ViewModels about an ongoing language change
    /// Mainly used as additional "Hack component" since the LocalizationManager being used is now capable of 
    /// updating all bindings automatically
    /// </summary>
    public class LanguageChangedEvent
    {
        /// <summary>
        /// The language used before the change was performed
        /// </summary>
        public Language OldLanguage { get; set; }

        /// <summary>
        /// The new (and current) language being used
        /// </summary>
        public Language NewLanguage { get; set; }

        public LanguageChangedEvent(Language oldLanguage, Language newLanguage)
        {
            OldLanguage = oldLanguage;
            NewLanguage = newLanguage;
        }
    }
}

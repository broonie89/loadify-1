using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Localization
{
    public class Language
    {
        /// <summary> CultureInfo of the language </summary>
        public CultureInfo Culture { get; set; }

        /// <summary> Retrieves the full name of the language, translated into the current language set </summary>
        public string Name
        {
            get { return Culture.NativeName; }
        }

        public string Code
        {
            get { return Culture.Name; }
        }

        public static Language English
        {
            get { return new Language("en"); }
        }

        public static Language German
        {
            get { return new Language("de"); }
        }

        public Language(CultureInfo culture)
        {
            Culture = culture;
        }

        public Language(string name)
        {
            try
            {
                Culture = new CultureInfo(name);
            }
            catch (CultureNotFoundException)
            {
                Culture = English.Culture;
            }
        }

        public override string ToString()
        {
            return Culture.ToString();
        }

        public override bool Equals(object obj)
        {
            if (obj == null || !(obj is Language))
                return false;

            return ((Language)obj).Name == this.Name;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }
}

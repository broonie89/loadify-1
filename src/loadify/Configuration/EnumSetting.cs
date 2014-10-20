using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Configuration
{
    public class EnumSetting<T>
    {
        public string RawValue { get; set; }

        public T ConvertedValue
        {
            get { return (T) Enum.Parse(typeof (T), RawValue); }
            set { RawValue = value.ToString(); }
        }

        public EnumSetting(string rawValue)
        {
            RawValue = rawValue;
        }

        public EnumSetting(T convertedValue)
        {
            ConvertedValue = convertedValue;
        }
    }
}

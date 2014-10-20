using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using loadify.Audio;
using loadify.Properties;

namespace loadify.Configuration
{
    public class NETBehaviorSetting : IBehaviorSetting
    {
        public EnumSetting<WriteConflictAction> WriteConflictAction
        {
            get { return new EnumSetting<WriteConflictAction>(Settings.Default.WriteConflictAction); }
            set
            {
                Settings.Default.WriteConflictAction = value.RawValue;
                Settings.Default.Save();
            }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using loadify.Audio;

namespace loadify.Configuration
{
    public interface IBehaviorSetting
    {
        EnumSetting<WriteConflictAction> WriteConflictAction { get; set; }  
    }
}

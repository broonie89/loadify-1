using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Audio
{
    public enum WriteConflictAction
    {
        Skip,
        Overwrite,
        Rename,
        Notify
    }
}

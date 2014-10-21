using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Configuration
{
    public interface ICredentialsSetting
    {
        string Username { get; set; }
        string Password { get; set; }
    }
}

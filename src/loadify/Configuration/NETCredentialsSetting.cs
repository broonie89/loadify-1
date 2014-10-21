using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Properties;

namespace loadify.Configuration
{
    public class NETCredentialsSetting : ICredentialsSetting
    {
        public string Username
        {
            get { return Settings.Default.Username; }
            set
            {
                Settings.Default.Username = value;
                Settings.Default.Save();
            }
        }

        public string Password
        {
            get { return Settings.Default.Password; }
            set
            {
                Settings.Default.Password = value;
                Settings.Default.Save();
            }
        }
    }
}

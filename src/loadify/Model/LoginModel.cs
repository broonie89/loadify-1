using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifySharp;

namespace loadify.Model
{
    public class LoginModel
    {
        public string Username { get; set; }
        public LoadifySession Session { get; set; }

        public LoginModel(LoadifySession session)
        {
            Session = session;
        }
    }
}

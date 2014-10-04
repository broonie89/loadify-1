using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifySharp;

namespace loadify.Events
{
    public class LoginFailedEvent
    {
        public SpotifyError Error { get; set; }

        public LoginFailedEvent(SpotifyError error)
        {
            Error = error;
        }
    }
}

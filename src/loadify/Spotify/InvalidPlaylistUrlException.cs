using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Spotify
{
    public class InvalidPlaylistUrlException : Exception
    {
        public InvalidPlaylistUrlException(string playlistUrl):
            base("The given url is not pointing to a valid Spotify playlistUrl: " + playlistUrl)
        { }
    }
}

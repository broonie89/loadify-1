using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Spotify
{
    public class InvalidSpotifyUrlException : Exception
    {
        public InvalidSpotifyUrlException(string url):
            base("The given url is not pointing to a valid Spotify resource: " + url)
        { }
    }
}

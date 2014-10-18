using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Spotify
{
    public class AudioFileMetaData
    {
        public string TrackName { get; set; }
        public string ArtistName { get; set; }

        public AudioFileMetaData(string trackName, string artistName)
        {
            TrackName = trackName;
            ArtistName = artistName;
        }
    }
}

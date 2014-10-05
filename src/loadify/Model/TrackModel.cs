using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifySharp;

namespace loadify.Model
{
    public class TrackModel
    {
        private Track _UnmanagedTrack { get; set; }

        public string Name { get; set; }
        public int Duration { get; set; }
        public List<ArtistModel> Artists { get; set; } 

        public TrackModel(Track unmanagedTrack)
        {
            _UnmanagedTrack = unmanagedTrack;      
            Artists = new List<ArtistModel>();
        }
    }
}

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
        public Track UnmanagedTrack { get; set; }
        public string Name { get; set; }
        public TimeSpan Duration { get; set; }
        public List<ArtistModel> Artists { get; set; }
        public int Rating { get; set; }
        public AlbumModel Album { get; set; }

        public TrackModel(Track unmanagedTrack):
            this()
        {
            UnmanagedTrack = unmanagedTrack;
        }

        public TrackModel()
        {
            Artists = new List<ArtistModel>();
            Album = new AlbumModel();
        }
    }
}

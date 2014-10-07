using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Model
{
    public class TrackModel
    {
        public string Name { get; set; }
        public int Duration { get; set; }
        public List<ArtistModel> Artists { get; set; }
        public int Rating { get; set; }
        public AlbumModel Album { get; set; }

        public TrackModel()
        {
            Artists = new List<ArtistModel>();
            Album = new AlbumModel();
        }
    }
}

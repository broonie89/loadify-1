using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifySharp;

namespace loadify.Model
{
    public class AlbumModel
    {
        public string Name { get; set; }
        public int ReleaseYear { get; set; }
        public AlbumType AlbumType { get; set; }

        public AlbumModel()
        { }
    }
}

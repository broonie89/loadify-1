using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifySharp;

namespace loadify.Model
{
    public class ArtistModel
    {
        public string Name { get; set; }
        public byte[] Portrait { get; set; }

        public ArtistModel()
        { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Spotify
{
    public class AudioFileMetaData
    {
        public string Title { get; set; }
        public string Artists { get; set; }
        public string Album { get; set; }
        public int Year { get; set; }
        public byte[] Cover { get; set; }

        public AudioFileMetaData()
        { }
    }
}

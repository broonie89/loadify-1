using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Spotify
{
    public abstract class AudioFileDescriptor
    {
        public AudioFileMetaData Data { get; set; }

        public AudioFileDescriptor(AudioFileMetaData audioFileMetaData)
        {
            Data = audioFileMetaData;
        }

        public abstract void Write(string inputFilePath);
        public abstract void Read(string inputFilePath);
    }
}

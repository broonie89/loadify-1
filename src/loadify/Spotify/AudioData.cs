using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Spotify
{
    public struct AudioData
    {
        public byte[] Data;
        public int SampleRate;
        public int BitRate;
        public int Channels;

        public AudioData(byte[] data, int sampleRate, int bitRate, int channels)
        {
            Data = data;
            SampleRate = sampleRate;
            BitRate = bitRate;
            Channels = channels;
        }
    }
}

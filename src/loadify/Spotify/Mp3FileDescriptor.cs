using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Spotify
{
    public class Mp3FileDescriptor : AudioFileDescriptor
    {
        public Mp3FileDescriptor(AudioFileMetaData audioFileMetaData):
            base(audioFileMetaData)
        { }

        public override void Write(string inputFilePath)
        {
            throw new NotImplementedException();
        }

        public override void Read(string inputFilePath)
        {
            throw new NotImplementedException();
        }
    }
}

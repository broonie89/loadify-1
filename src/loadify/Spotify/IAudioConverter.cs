using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Spotify
{
    public interface IAudioConverter
    {
        string Convert(string inputFilePath, string outputDirectory, string outputFileName);
    }
}

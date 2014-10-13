using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace loadify.Spotify
{
    public abstract class AudioProcessor
    {
        public string OutputDirectory { get; set; }
        public string OutputFileName { get; set; }

        public AudioProcessor(string outputDirectory, string outputFileName)
        {
            OutputDirectory = outputDirectory;
            OutputFileName = outputFileName;
        }

        public abstract string Process(AudioData audioData);
    }
}

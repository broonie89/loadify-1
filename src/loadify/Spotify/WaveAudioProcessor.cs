using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Wave;

namespace loadify.Spotify
{
    public class WaveAudioProcessor : AudioProcessor
    {
        public WaveAudioProcessor(string outputDirectory, string outputFileName) :
            base(outputDirectory, outputFileName)
        { }

        public override string Process(AudioData audioData)
        {
            if (!Directory.Exists(OutputDirectory))
                Directory.CreateDirectory(OutputDirectory);

            var outputFilePath = String.Format("{0}/{1}.wav", OutputDirectory, OutputFileName);
            using (var wavWriter = new WaveFileWriter(outputFilePath, new WaveFormat(audioData.SampleRate, audioData.BitRate, audioData.Channels)))
            {
                wavWriter.Write(audioData.Data, 0, audioData.Data.Length);
            }

            return outputFilePath;
        }
    }
}

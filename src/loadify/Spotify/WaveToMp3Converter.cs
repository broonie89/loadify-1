using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NAudio.Lame;
using NAudio.Wave;

namespace loadify.Spotify
{
    public class WaveToMp3Converter : IAudioConverter
    {
        public WaveToMp3Converter()
        { }

        public string Convert(string inputFilePath, string outputDirectory, string outputFileName)
        {
            if (!Directory.Exists(outputDirectory))
                Directory.CreateDirectory(outputDirectory);

            var outputFilePath = String.Format("{0}/{1}.mp3", outputDirectory, outputFileName);

            using (var wavReader = new WaveFileReader(inputFilePath))
            using (var mp3Writer = new LameMP3FileWriter(outputFilePath, wavReader.WaveFormat, 128))
                wavReader.CopyTo(mp3Writer);

            return outputFilePath;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading.Tasks;
using SpotifySharp;

namespace loadify.Spotify
{
    public class TrackDownloader
    {
        public AudioProcessor AudioProcessor { get; set; }
        public IAudioConverter AudioConverter { get; set; }

        public TrackDownloader(AudioProcessor audioProcessor)
            : this(audioProcessor, null)
        { }

        public TrackDownloader(AudioProcessor audioProcessor, IAudioConverter audioConverter)
        {
            AudioProcessor = audioProcessor;
            AudioConverter = audioConverter;
        }

        public void Download(Track track, AudioData audioData)
        {
            var processedAudioFile = AudioProcessor.Process(audioData);
            if (AudioConverter != null)
                AudioConverter.Convert(processedAudioFile, AudioProcessor.OutputDirectory, AudioProcessor.OutputFileName);
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using loadify.Audio;
using loadify.Model;
using SpotifySharp;

namespace loadify.Spotify
{
    public class TrackDownloadService
    {
        private class Statistic
        {
            public TimeSpan TargetDuration { get; set; }
            public uint Processings { get; set; }
            public double AverageFrameSize { get; set; }

            public Statistic(TimeSpan targetDuration)
            {
                TargetDuration = targetDuration;
            }

            public Statistic()
                : this(new TimeSpan())
            { }
        }


        public enum CancellationReason
        {
            None,
            PlayTokenLost,
            Unknown,
            ConnectionLost
        };

        public bool Active { get; set; }
        public AudioMetaData AudioMetaData { get; set; }
        public AudioProcessor AudioProcessor { get; set; }
        public AudioConverter AudioConverter { get; set; }
        public AudioFileDescriptor AudioFileDescriptor { get; set; }

        private Statistic _Statistic = new Statistic();
        private readonly Action<CancellationReason> _DownloadCompletedCallback = cancellationReason => { };

        public double Progress
        {
            get
            {
                var trackDuration = _Statistic.TargetDuration.TotalMilliseconds;
                return (trackDuration != 0)
                        ? 100 / _Statistic.TargetDuration.TotalMilliseconds * (46.4 * _Statistic.Processings)
                        : 100;
            }
        }

        public TrackDownloadService(AudioProcessor audioProcessor, AudioConverter audioConverter, AudioFileDescriptor audioFileDescriptor,
                                    Action<CancellationReason> callback)
        {
            AudioProcessor = audioProcessor;
            AudioConverter = audioConverter;
            AudioFileDescriptor = audioFileDescriptor;
            _DownloadCompletedCallback = callback;
            AudioMetaData = new AudioMetaData();
        }

        public void Start(TrackModel track)
        {
            _Statistic = new Statistic(track.Duration);
            Active = true;
        }

        public void Stop()
        {
            Active = false;
            AudioProcessor.Release();
        }

        public void ProcessInput(AudioFormat format, IntPtr frames, int num_frames)
        {
            AudioMetaData.SampleRate = format.sample_rate;
            AudioMetaData.Channels = format.channels;

            var size = num_frames * format.channels * 2;
            var buffer = new byte[size];
            Marshal.Copy(frames, buffer, 0, size);
            AudioProcessor.Process(buffer);

            _Statistic.Processings++;
            _Statistic.AverageFrameSize = (_Statistic.AverageFrameSize + size) / 2;
        }

        public void Finish()
        {
            Stop();

            var outputFilePath = AudioProcessor.OutputFilePath;
            if (AudioConverter != null)
                outputFilePath = AudioConverter.Convert(AudioProcessor.OutputFilePath);

            if (AudioFileDescriptor != null)
                AudioFileDescriptor.Write(outputFilePath);

            _DownloadCompletedCallback(CancellationReason.None);
        }

        public void Cancel(CancellationReason reason)
        {
            Stop();
            _DownloadCompletedCallback(reason);
        }
    }
}

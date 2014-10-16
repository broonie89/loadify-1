using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Spotify;

namespace loadify.Event
{
    public class CapturingFinishedEvent
    {
        public AudioData AudioData { get; set; }

        public CapturingFinishedEvent(AudioData audioData)
        {
            AudioData = audioData;
        }
    }
}

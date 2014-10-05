using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifySharp;

namespace loadify.Model
{
    public class PlaylistModel
    {
        private readonly Playlist _UnmanagedPlaylist;

        public string Name { get; set; }
        public string Description { get; set; }
        public List<TrackModel> Tracks { get; set; }
        public List<string> Subscribers { get; set; }
        public string Creator { get; set; }
        public byte[] Image { get; set; }

        public PlaylistModel(Playlist unmanagedPlaylist)
        {
            Tracks = new List<TrackModel>();
            _UnmanagedPlaylist = unmanagedPlaylist;
        }
    }
}

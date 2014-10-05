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

        public PlaylistModel(Playlist unmanagedPlaylist)
        {
            Tracks = new List<TrackModel>();
            _UnmanagedPlaylist = unmanagedPlaylist;

            if (_UnmanagedPlaylist != null)
            {
                Name = _UnmanagedPlaylist.Name();
                Subscribers = _UnmanagedPlaylist.Subscribers().ToList();
                Creator = _UnmanagedPlaylist.Owner().DisplayName();
                Description = _UnmanagedPlaylist.GetDescription();

                for(int i = 0; i < _UnmanagedPlaylist.NumTracks(); i++)
                    Tracks.Add(new TrackModel(_UnmanagedPlaylist.Track(i)));
            }
        }
    }
}

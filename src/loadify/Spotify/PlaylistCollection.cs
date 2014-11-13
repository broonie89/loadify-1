using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Model;
using SpotifySharp;

namespace loadify.Spotify
{
    public class PlaylistCollection
    {
        public PlaylistContainer UnmanagedPlaylistContainer { get; set; }

        public PlaylistCollection(PlaylistContainer playlistContainer)
        {
            UnmanagedPlaylistContainer = playlistContainer;
        }

        public async Task Add(Playlist playlist)
        {
            if (UnmanagedPlaylistContainer == null) return;
            await SpotifyObject.WaitForInitialization(UnmanagedPlaylistContainer.IsLoaded);
            UnmanagedPlaylistContainer.AddPlaylist(Link.CreateFromPlaylist(playlist));
        }

        public async Task Remove(Playlist playlist)
        {
            if (UnmanagedPlaylistContainer == null) return;
            await SpotifyObject.WaitForInitialization(UnmanagedPlaylistContainer.IsLoaded);

            for (int i = 0; i < UnmanagedPlaylistContainer.NumPlaylists(); i++)
            {
                var unmanagedPlaylist = UnmanagedPlaylistContainer.Playlist(i);
                await SpotifyObject.WaitForInitialization(unmanagedPlaylist.IsLoaded);

                if (unmanagedPlaylist.Name() == playlist.Name())
                {
                    UnmanagedPlaylistContainer.RemovePlaylist(i);
                    break;
                }
            }
        }

        public async Task<IEnumerable<Playlist>> GetPlaylists()
        {
            var playlists = new List<Playlist>();
            if (UnmanagedPlaylistContainer == null) return playlists;
            await SpotifyObject.WaitForInitialization(UnmanagedPlaylistContainer.IsLoaded);

            for (var i = 0; i < UnmanagedPlaylistContainer.NumPlaylists(); i++)
            {
                var unmanagedPlaylist = UnmanagedPlaylistContainer.Playlist(i);
                if (unmanagedPlaylist == null) continue;
                await SpotifyObject.WaitForInitialization(unmanagedPlaylist.IsLoaded);
                playlists.Add(unmanagedPlaylist);
            }

            return playlists;
        }
    }
}

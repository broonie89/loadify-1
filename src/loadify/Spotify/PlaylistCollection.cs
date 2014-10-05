using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SpotifySharp;

namespace loadify.Spotify
{
    public class PlaylistCollection : IEnumerable<Playlist>
    {
        public List<Playlist> Items { get; set; }

        public Playlist this[int index]
        {
            get { return Items[index]; }
            set { Items.Insert(index, value); }
        }

        public IEnumerator<Playlist> GetEnumerator()
        {
            return Items.GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public PlaylistCollection()
        {
            Items = new List<Playlist>();
        }

        public void Add(Playlist playlist)
        {
            Items.Add(playlist);
        }

        public static PlaylistCollection FromPlaylistContainer(PlaylistContainer container)
        {
            var collection = new PlaylistCollection();
            if (container == null) return collection;

            for (int i = 0; i < container.NumPlaylists(); i++)
                collection.Add(container.Playlist(i));

            container.Release();
            return collection;
        }
    }
}

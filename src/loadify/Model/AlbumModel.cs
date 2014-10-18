using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using loadify.Spotify;
using SpotifySharp;

namespace loadify.Model
{
    public class AlbumModel
    {
        private readonly Album _UnmanagedAlbum;

        public string Name { get; set; }
        public int ReleaseYear { get; set; }
        public AlbumType AlbumType { get; set; }
        public byte[] Cover { get; set; }

        public AlbumModel(Album unmanagedAlbum)
        {
            _UnmanagedAlbum = unmanagedAlbum;
        }

        public AlbumModel()
        { }

        public static async Task<AlbumModel> FromLibrary(Album unmanagedAlbum, LoadifySession session)
        {
            var albumModel = new AlbumModel(unmanagedAlbum);
            if (unmanagedAlbum == null) return albumModel;
            await SpotifyObject.WaitForInitialization(unmanagedAlbum.IsLoaded);

            var coverImage = session.GetImage(unmanagedAlbum.Cover(ImageSize.Large));
            await SpotifyObject.WaitForInitialization(coverImage.IsLoaded);

            albumModel.Name = unmanagedAlbum.Name();
            albumModel.ReleaseYear = unmanagedAlbum.Year();
            albumModel.AlbumType = unmanagedAlbum.Type();
            albumModel.Cover = coverImage.Data();
            
            return albumModel;
        }
    }
}

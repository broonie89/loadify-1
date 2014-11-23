using System;
using System.Threading.Tasks;
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

            albumModel.Name = unmanagedAlbum.Name();
            albumModel.ReleaseYear = unmanagedAlbum.Year();
            albumModel.AlbumType = unmanagedAlbum.Type();

            try
            {
                // retrieve the cover image of the album...
                var coverImage = session.GetImage(unmanagedAlbum.Cover(ImageSize.Large));
                await SpotifyObject.WaitForInitialization(coverImage.IsLoaded);
                albumModel.Cover = coverImage.Data();
            }
            catch (AccessViolationException)
            {
                // nasty work-around - swallow if the cover image could not be retrieved
                // since the ImageId class does not expose a property or function for checking if the buffer/handle is null/0
            }
            
            return albumModel;
        }
    }
}

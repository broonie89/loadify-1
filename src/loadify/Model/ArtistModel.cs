using System.Threading.Tasks;
using loadify.Spotify;
using SpotifySharp;

namespace loadify.Model
{
    public class ArtistModel
    {
        private readonly Artist _UnmanagedArtist;

        public string Name { get; set; }
        public byte[] Portrait { get; set; }

        public ArtistModel(Artist unmanagedArtist)
        {
            _UnmanagedArtist = unmanagedArtist;
        }

        public static async Task<ArtistModel> FromLibrary(Artist unmanagedArtist, LoadifySession session)
        {
            var artistModel = new ArtistModel(unmanagedArtist);
            if (unmanagedArtist == null) return artistModel;
            await SpotifyObject.WaitForInitialization(unmanagedArtist.IsLoaded);

            artistModel.Name = unmanagedArtist.Name();
            return artistModel;
        }
    }
}

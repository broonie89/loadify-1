using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using loadify.Spotify;
using SpotifySharp;

namespace loadify.Model
{
    public class TrackModel
    {
        public Track UnmanagedTrack { get; set; }

        public string Name { get; set; }
        public PlaylistModel Playlist { get; set; }
        public TimeSpan Duration { get; set; }
        public List<ArtistModel> Artists { get; set; }
        public int Rating { get; set; }
        public AlbumModel Album { get; set; }
        public bool ExistsLocally { get; set; }

        public TrackModel(Track unmanagedTrack):
            this()
        {
            UnmanagedTrack = unmanagedTrack;
        }

        public TrackModel()
        {
            Artists = new List<ArtistModel>();
            Album = new AlbumModel();
            Playlist = new PlaylistModel();
        }

        public static async Task<TrackModel> FromLibrary(Track unmanagedTrack, LoadifySession session)
        {
            var trackModel = new TrackModel(unmanagedTrack);
            if (unmanagedTrack == null) return trackModel;
            await SpotifyObject.WaitForInitialization(unmanagedTrack.IsLoaded);

            trackModel.Name = unmanagedTrack.Name();
            trackModel.Duration = TimeSpan.FromMilliseconds(unmanagedTrack.Duration());
            trackModel.Rating = unmanagedTrack.Popularity();
            trackModel.Album = await AlbumModel.FromLibrary(unmanagedTrack.Album(), session);

            for (var j = 0; j < unmanagedTrack.NumArtists(); j++)
            {
                var unmanagedArtist = unmanagedTrack.Artist(j);
                if (unmanagedArtist == null) continue;

                trackModel.Artists.Add(await ArtistModel.FromLibrary(unmanagedArtist, session));
            }

            return trackModel;
        }
    }
}

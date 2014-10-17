﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using loadify.Spotify;
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

        public static async Task<PlaylistModel> FromLibrary(Playlist unmanagedPlaylist, LoadifySession session)
        {
            var playlistModel = new PlaylistModel(unmanagedPlaylist);

            if (unmanagedPlaylist == null) return playlistModel;
            await SpotifyObject.WaitForInitialization(unmanagedPlaylist.IsLoaded);

            playlistModel.Name = unmanagedPlaylist.Name();
            playlistModel.Subscribers = unmanagedPlaylist.Subscribers().ToList();
            playlistModel.Creator = unmanagedPlaylist.Owner().DisplayName();
            playlistModel.Description = unmanagedPlaylist.GetDescription();

            var playlistImageId = unmanagedPlaylist.GetImage();
            if (playlistImageId != null)
                playlistModel.Image = session.GetImage(playlistImageId).Data();

            for (var i = 0; i < unmanagedPlaylist.NumTracks(); i++)
            {
                var unmanagedTrack = unmanagedPlaylist.Track(i);
                if (unmanagedTrack == null) continue;
                var managedTrack = await TrackModel.FromLibrary(unmanagedTrack, session);
                managedTrack.Album = await AlbumModel.FromLibrary(unmanagedTrack.Album(), session);
               
                for (var j = 0; j < unmanagedTrack.NumArtists(); j++)
                {
                    var unmanagedArtist = unmanagedTrack.Artist(j);
                    if (unmanagedArtist == null) continue;

                    managedTrack.Artists.Add(await ArtistModel.FromLibrary(unmanagedArtist, session));
                }

                playlistModel.Tracks.Add(managedTrack);
            }

            return playlistModel;
        }

        public PlaylistModel(Playlist unmanagedPlaylist)
            : this()
        {
            _UnmanagedPlaylist = unmanagedPlaylist;
        }

        public PlaylistModel()
        {
            Tracks = new List<TrackModel>();
        }

        public PlaylistModel(PlaylistModel playlist)
        {
            Name = playlist.Name;
            Description = playlist.Description;
            Tracks = new List<TrackModel>(playlist.Tracks);
            Subscribers = new List<string>(playlist.Subscribers);
            Creator = playlist.Creator;
            Image = playlist.Image;
        }
    }
}

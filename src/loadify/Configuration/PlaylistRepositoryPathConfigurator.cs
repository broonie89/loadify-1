using System;
using System.IO;
using System.Linq;
using loadify.Model;
using loadify.Spotify;

namespace loadify.Configuration
{
    public class PlaylistRepositoryPathConfigurator : IDownloadPathConfigurator
    {
        public string Configure(string basePath, string targetFileExtension, TrackModel track)
        {
            basePath += (basePath.Last() != '\\') ? "\\" : "";
            var completePath = basePath + track.Name.ValidateFileName();

            if (track.Playlist != null)
            {
                if (track.Playlist.Name.Length != 0)
                {
                    var playlistRepositoryDirectory = basePath + track.Playlist.Name.ValidateFileName() + "\\";
                    if (!Directory.Exists(playlistRepositoryDirectory))
                        Directory.CreateDirectory(playlistRepositoryDirectory);

                    completePath = playlistRepositoryDirectory + track.Name.ValidateFileName() + "." + targetFileExtension;
                }
            }

            return completePath;
        }
    }
}

// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotifySharp;

namespace SpShellSharp
{
    class Browser
    {
        SpotifySession iSession;
        IMetadataWaiter iMetadataWaiter;
        IConsoleReader iConsoleReader;
        Track iTrackBrowse;
        Playlist iPlaylistBrowse;
        BrowsingPlaylistListener iPlaylistListener;
        bool iListeningForPlaylist;

        class BrowsingPlaylistListener : PlaylistListener
        {
            Browser iBrowser;

            public BrowsingPlaylistListener(Browser aBrowser)
            {
                iBrowser = aBrowser;
            }

            public override void TracksAdded(Playlist pl, Track[] tracks, int position, object userdata)
            {
                Console.WriteLine("\t{0} tracks added", tracks.Length);
            }
            public override void TracksRemoved(Playlist pl, int[] tracks, object userdata)
            {
                Console.WriteLine("\t{0} tracks removed", tracks.Length);
            }
            public override void TracksMoved(Playlist pl, int[] tracks, int new_position, object userdata)
            {
                Console.WriteLine("\t{0} tracks moved", tracks.Length);
            }
            public override void PlaylistRenamed(Playlist pl, object userdata)
            {
                Console.WriteLine("List name: {0}", pl.Name());
            }
            public override void PlaylistStateChanged(Playlist pl, object userdata)
            {
                iBrowser.PlaylistBrowseTry();
            }
        }
        public Browser(SpotifySession aSession, IMetadataWaiter aMetadataWaiter, IConsoleReader aConsoleReader)
        {
            iSession = aSession;
            iMetadataWaiter = aMetadataWaiter;
            iConsoleReader = aConsoleReader;
            iPlaylistListener = new BrowsingPlaylistListener(this);
        }
        public int CmdBrowse(string[] args)
        {
            if (args.Length != 2)
            {
                Console.Error.WriteLine("Usage: browse <spotify-url>");
                return -1;
            }
            var link = Link.CreateFromString(args[1]);
            if (link == null)
            {
                Console.Error.WriteLine("Not a spotify link");
                return -1;
            }
            switch (link.Type())
            {
                default:
                    Console.Error.WriteLine("Can not handle link");
                    link.Release();
                    return -1;
                case LinkType.Album:
                    AlbumBrowse.Create(iSession, link.AsAlbum(), BrowseAlbumCallback, null);
                    break;
                case LinkType.Artist:
                    ArtistBrowse.Create(iSession, link.AsArtist(), ArtistBrowseType.Full, BrowseArtistCallback, null);
                    break;
                case LinkType.Localtrack:
                case LinkType.Track:
                    iTrackBrowse = link.AsTrack();
                    iMetadataWaiter.AddMetadataUpdatedCallback(TrackBrowseTry);
                    iTrackBrowse.AddRef();
                    TrackBrowseTry();
                    break;
                case LinkType.Playlist:
                    BrowsePlaylist(Playlist.Create(iSession, link));
                    break;
            }
            link.Release();
            return 0;
        }

        void StartingListeningForPlaylistChanges()
        {
            if (!iListeningForPlaylist)
            {
                iMetadataWaiter.AddMetadataUpdatedCallback(PlaylistBrowseTry);
                iListeningForPlaylist = true;
            }
        }

        void StopListeningForPlaylistChanges()
        {
            if (iListeningForPlaylist)
            {
                iMetadataWaiter.RemoveMetadataUpdatedCallback(PlaylistBrowseTry);
                iListeningForPlaylist = false;
            }
        }

        public void BrowsePlaylist(Playlist aPlaylist)
        {
            iPlaylistBrowse = aPlaylist;
            aPlaylist.AddCallbacks(iPlaylistListener, null);
            PlaylistBrowseTry();
        }

        void PlaylistBrowseTry()
        {
            StartingListeningForPlaylistChanges();
            if (!iPlaylistBrowse.IsLoaded())
            {
                Console.WriteLine("\tPlaylist not loaded");
                return;
            }

            int tracks = iPlaylistBrowse.NumTracks();
            for (int i = 0; i != tracks; ++i)
            {
                Track t = iPlaylistBrowse.Track(i);
                if (!t.IsLoaded())
                {
                    return;
                }
            }

            Console.WriteLine("\tPlaylist and metadata loaded");

            for (int i = 0; i != tracks; ++i)
            {
                Track t = iPlaylistBrowse.Track(i);
                Console.Write(" {0,5}: ", i + 1);
                PrintTrack(t);
            }

            iPlaylistBrowse.RemoveCallbacks(iPlaylistListener, null);
            StopListeningForPlaylistChanges();
            iPlaylistBrowse.Release();
            iPlaylistBrowse = null;
            iConsoleReader.RequestInput("> ");
        }


        void TrackBrowseTry()
        {
            try
            {
                iTrackBrowse.Error();
                PrintTrack(iTrackBrowse);
            }
            catch (SpotifyException e)
            {
                switch (e.Error)
                {
                    case SpotifyError.IsLoading:
                        return;
                    default:
                        Console.WriteLine("Unable to resolve track: {0}", e.Message);
                        break;
                }
            }
            iMetadataWaiter.RemoveMetadataUpdatedCallback(TrackBrowseTry);
            iConsoleReader.RequestInput("> ");
            iTrackBrowse.Release();
            iTrackBrowse = null;
        }

        void PrintTrack(Track aTrack)
        {
            Printing.PrintTrack(iSession, aTrack);
        }


        void BrowseAlbumCallback(AlbumBrowse aResult, object aUserdata)
        {
            try
            {
                aResult.Error();
                PrintAlbumBrowse(aResult);
            }
            catch (SpotifyException e)
            {
                Console.Error.WriteLine("Failed to browse album: {0}", e.Message);
            }
            aResult.Dispose();
            iConsoleReader.RequestInput("> ");
        }

        void PrintAlbumBrowse(AlbumBrowse aAlbumBrowse)
        {
            Printing.PrintAlbumBrowse(iSession, aAlbumBrowse);
        }


        void BrowseArtistCallback(ArtistBrowse aResult, object aUserdata)
        {
            try
            {
                aResult.Error();
                PrintArtistBrowse(aResult);
            }
            catch (SpotifyException e)
            {
                Console.Error.WriteLine("Failed to browse artist: {0}", e.Message);
            }
            aResult.Dispose();
            iConsoleReader.RequestInput("> ");
        }

        void PrintArtistBrowse(ArtistBrowse aArtistBrowse)
        {
            Printing.PrintArtistBrowse(iSession, aArtistBrowse);
        }
    }
}

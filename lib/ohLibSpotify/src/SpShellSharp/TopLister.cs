// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotifySharp;

namespace SpShellSharp
{
    class TopLister
    {
        SpotifySession iSession;
        IConsoleReader iConsoleReader;

        public TopLister(SpotifySession aSession, IConsoleReader aConsoleReader)
        {
            iSession = aSession;
            iConsoleReader = aConsoleReader;
        }

        void TopListUsage()
        {
            Console.Error.WriteLine("Usage: toplist (tracks | albums | artists) (global | region <countrycode> | user)\n");
        }

        public int CmdTopList(string[] args)
        {
            TopListType type;
            TopListRegion region;
            if (args.Length < 3)
            {
                TopListUsage();
                return -1;
            }
            switch (args[1])
            {
                case "artists": type = TopListType.Artists; break;
                case "albums": type = TopListType.Albums; break;
                case "tracks": type = TopListType.Tracks; break;
                default:
                    TopListUsage();
                    return -1;
            }
            switch (args[2])
            {
                case "global": region = TopListRegion.Everywhere; break;
                case "user": region = TopListRegion.User; break;
                case "region":
                    if (args.Length != 4 || args[3].Length != 2)
                    {
                        TopListUsage();
                        return -1;
                    }
                    region = Spotify.TopListRegion(args[3]);
                    break;
                default:
                    TopListUsage();
                    return -1;
            }
            TopListBrowse.Create(iSession, type, region, null, GotTopList, null);
            return 0;
        }

        void GotTopList(TopListBrowse aResult, object aUserdata)
        {
            for (int i = 0; i != aResult.NumArtists(); ++i)
            {
                PrintArtist(i + 1, aResult.Artist(i));
            }
            for (int i = 0; i != aResult.NumAlbums(); ++i)
            {
                PrintAlbum(i + 1, aResult.Album(i));
            }
            for (int i = 0; i != aResult.NumTracks(); ++i)
            {
                Console.Write("{0,3}: ", i + 1);
                Printing.PrintTrack(iSession, aResult.Track(i));
            }
            aResult.Dispose();
            iConsoleReader.RequestInput("> ");
        }

        void PrintArtist(int aIndex, Artist aArtist)
        {
            Console.WriteLine("  Artist {0,3}: \"{1}\"", aIndex, aArtist.Name());
            Link portraitLink = Link.CreateFromArtistPortrait(aArtist, ImageSize.Normal);
            if (portraitLink != null)
            {
                Console.WriteLine("    Portrait: {0}", portraitLink.AsString());
                portraitLink.Release();
            }
        }

        void PrintAlbum(int aIndex, Album aAlbum)
        {
            Console.WriteLine("  Album {0,3}: \"{1}\" by \"{2}\"", aIndex, aAlbum.Name(), aAlbum.Artist().Name());
        }
    }
}

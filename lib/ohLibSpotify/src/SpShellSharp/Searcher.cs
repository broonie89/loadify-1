// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotifySharp;

namespace SpShellSharp
{
    class Searcher
    {
        SpotifySession iSession;
        IConsoleReader iConsoleReader;

        public Searcher(SpotifySession aSession, IConsoleReader aConsoleReader)
        {
            iSession = aSession;
            iConsoleReader = aConsoleReader;
        }

        void SearchUsage()
        {
            Console.Error.WriteLine("Usage: search <query>");
        }

        public int CmdSearch(string[] aArgs)
        {
            if (aArgs.Length < 2)
            {
                SearchUsage();
                return -1;
            }
            string query = String.Join(" ", aArgs.Skip(1));
            Search.Create(iSession, query, 0, 100, 0, 100, 0, 100, 0, 100, SearchType.Standard, OnSearchComplete, null);
            return 0;
        }

        public int CmdWhatsNew(string[] aArgs)
        {
            Search.Create(iSession, "tag:new", 0, 0, 0, 250, 0, 0, 0, 0, SearchType.Standard, OnSearchComplete, null);
            return 0;
        }

        void OnSearchComplete(Search aResult, object aUserdata)
        {
            try
            {
                aResult.Error();
                PrintSearch(aResult);
            }
            catch (SpotifyException e)
            {
                Console.Error.WriteLine("Failed to search: {0}", e.Message);
            }
            aResult.Dispose();
            iConsoleReader.RequestInput("> ");
        }

        void PrintSearch(Search aSearch)
        {
            Console.WriteLine("Query          : {0}", aSearch.Query());
            Console.WriteLine("Did you mean   : {0}", aSearch.DidYouMean());
            Console.WriteLine("Tracks in total: {0}", aSearch.TotalTracks());
            Console.WriteLine();
            for (int i=0; i!=aSearch.NumTracks(); ++i)
                Printing.PrintTrack(iSession, aSearch.Track(i));
            for (int i=0; i!=aSearch.NumAlbums(); ++i)
                PrintAlbum(iSession, aSearch.Album(i));
            for (int i=0; i!=aSearch.NumArtists(); ++i)
                PrintArtist(iSession, aSearch.Artist(i));
            for (int i=0; i!=aSearch.NumPlaylists(); ++i)
                Console.WriteLine("  Playlist \"{0}\"", aSearch.PlaylistName(i));
        }

        void PrintArtist(SpotifySession aSession, Artist aArtist)
        {
            Console.WriteLine("  Artist \"{0}\"", aArtist.Name());
        }

        void PrintAlbum(SpotifySession aSession, Album aAlbum)
        {
            Console.WriteLine("  Album \"{0}\" ({1})", aAlbum.Name(), aAlbum.Year());
        }
    }
}

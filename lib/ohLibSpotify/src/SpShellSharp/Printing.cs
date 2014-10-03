// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotifySharp;

namespace SpShellSharp
{
    static class Printing
    {
        static string Truncate(string s, int length)
        {
            return s.Length <= length ? s : (s.Substring(0, length) + "...");
        }
        public static void PrintTrack(SpotifySession aSession, Track aTrack)
        {
            int duration = aTrack.Duration();
            Console.Write(" {0} ", Track.IsStarred(aSession, aTrack) ? "*" : " ");
            Console.Write("Track {0} [{1}:{2:D02}] has {3} artist(s), {4}% popularity",
                aTrack.Name(),
                duration / 60000,
                (duration / 1000) % 60,
                aTrack.NumArtists(),
                aTrack.Popularity());
            if (aTrack.Disc() != 0)
            {
                Console.Write(", {0} on disc {1}",
                    aTrack.Index(),
                    aTrack.Disc());
            }
            for (int i = 0; i < aTrack.NumArtists(); ++i)
            {
                var artist = aTrack.Artist(i);
                Console.Write("\tArtist {0}: {1}", i + 1, artist.Name());
            }
            var link = Link.CreateFromTrack(aTrack, 0);
            Console.WriteLine("\t\t{0}", link.AsString());
            link.Release();
        }
        public static void PrintAlbumBrowse(SpotifySession aSession, AlbumBrowse aResult)
        {
            Console.WriteLine("Album browse of \"{0}\" ({1})", aResult.Album().Name(), aResult.Album().Year());
            for (int i = 0; i != aResult.NumCopyrights(); ++i)
            {
                Console.WriteLine("  Copyright: {0}", aResult.Copyright(i));
            }
            Console.WriteLine("  Tracks: {0}", aResult.NumTracks());
            Console.WriteLine("  Review: {0}", Truncate(aResult.Review(), 60));
            Console.WriteLine();
            for (int i = 0; i != aResult.NumTracks(); ++i)
            {
                PrintTrack(aSession, aResult.Track(i));
            }
            Console.WriteLine();
        }
        public static void PrintArtistBrowse(SpotifySession aSession, ArtistBrowse aArtistBrowse)
        {
            Console.WriteLine("Artist browse of \"{0}\"", aArtistBrowse.Artist().Name());
            for (int i = 0; i != aArtistBrowse.NumSimilarArtists(); ++i)
            {
                Console.WriteLine("  Similar artist: {0}", aArtistBrowse.SimilarArtist(i).Name());
            }
            Console.WriteLine("  Portraits: {0}", aArtistBrowse.NumPortraits());
            Console.WriteLine("  Tracks: {0}", aArtistBrowse.NumTracks());
            Console.WriteLine("  Biography: {0}", Truncate(aArtistBrowse.Biography(),60));
            Console.WriteLine();
            for (int i = 0; i != aArtistBrowse.NumTracks(); ++i)
            {
                PrintTrack(aSession, aArtistBrowse.Track(i));
            }
            Console.WriteLine();
        }

    }
}

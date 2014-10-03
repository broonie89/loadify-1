// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotifySharp;

namespace SpShellSharp
{
    class StarManager
    {
        SpotifySession iSession;
        IConsoleReader iConsoleReader;
        readonly Browser iBrowser;

        public StarManager(SpotifySession aSession, IConsoleReader aConsoleReader, Browser aBrowser)
        {
            iSession = aSession;
            iConsoleReader = aConsoleReader;
            iBrowser = aBrowser;
        }

        void StarUsage(string aPrefix)
        {
            Console.Error.WriteLine("Usage: {0}star <track-uri>", aPrefix);
        }

        int DoStar(string[] aArgs, bool aSet)
        {
            if (aArgs.Length != 2)
            {
                StarUsage(aSet ? "" : "un");
                return -1;
            }
            var link = Link.CreateFromString(aArgs[1]);
            if (link == null)
            {
                Console.Error.WriteLine("Not a spotify link");
                return -1;
            }
            if (link.Type() != LinkType.Track)
            {
                Console.Error.WriteLine("Not a track link");
                link.Release();
                return -1;
            }
            var track = link.AsTrack();
            try
            {
                Track.SetStarred(iSession, new[]{track}, aSet);
            }
            catch (SpotifyException)
            {
                // Pass
            }
            link.Release();
            return -1;
        }

        public int CmdStar(string[] aArgs) { return DoStar(aArgs, true); }
        public int CmdUnstar(string[] aArgs) { return DoStar(aArgs, false); }
        public int CmdStarred(string[] aArgs)
        {
            Playlist starred;
            if (aArgs.Length > 1)
            {
                starred = iSession.StarredForUserCreate(aArgs[1]);
            }
            else
            {
                starred = iSession.StarredCreate();
            }
            if (starred != null)
            {
                iBrowser.BrowsePlaylist(starred);
                return 0;
            }
            Console.WriteLine("Starred not loaded");
            return -1;
        }
    }
}

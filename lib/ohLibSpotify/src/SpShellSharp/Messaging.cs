// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotifySharp;

namespace SpShellSharp
{
    class Messaging
    {
        SpotifySession iSession;
        IConsoleReader iConsoleReader;
        readonly Browser iBrowser;

        public Messaging(SpotifySession aSession, IConsoleReader aConsoleReader, Browser aBrowser)
        {
            iSession = aSession;
            iConsoleReader = aConsoleReader;
            iBrowser = aBrowser;
        }

        void PostUsage()
        {
            Console.Error.WriteLine("Usage: post <recipient> <message> [<track-uri> ...]");
        }

        Track CreateTrackFromLink(string aLink)
        {
            Link link = Link.CreateFromString(aLink);
            Track track = link.AsTrack();
            track.AddRef();
            link.Release();
            return track;
        }

        public int CmdPost(string[] args)
        {
            if (args.Length < 3)
            {
                PostUsage();
                return -1;
            }
            Track[] tracks;
            if (args.Length == 3)
            {
                // No arguments: rickroll recipient
                tracks = new[]{
                    CreateTrackFromLink("spotify:track:6JEK0CvvjDjjMUBFoXShNZ"),
                };
            }
            else
            {
                tracks = args.Skip(3).Select(CreateTrackFromLink).ToArray();
            }
            Console.WriteLine("Trying to post {0} tracks to {1} with message \"{2}\"", tracks.Length, args[1], args[2]);
            var inboxResult = Inbox.PostTracks(iSession, args[1], tracks, args[2], InboxPostCompleted, null);
            foreach (var t in tracks)
                t.Release();
            if (inboxResult == null)
            {
                Console.Error.WriteLine("inbox post failed");
                return -1;
            }
            return 0;
        }

        void InboxPostCompleted(Inbox aResult, object aUserdata)
        {
            try
            {
                aResult.Error();
                Console.Error.WriteLine("Inbox post result: Ok");
            }
            catch (SpotifyException e)
            {
                Console.Error.WriteLine("Inbox post result: {0}", e.Message);
            }
            iConsoleReader.RequestInput("> ");
        }

        public int CmdInbox(string[] args)
        {
            var inbox = iSession.InboxCreate();
            if (inbox == null)
            {
                Console.WriteLine("Inbox not loaded");
                return -1;
            }
            iBrowser.BrowsePlaylist(inbox);
            return 0;
        }
    }
}

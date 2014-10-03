// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using SpotifySharp;

namespace SpShellSharp
{
    class PlaylistManager
    {
        SpotifySession iSession;
        IConsoleReader iConsoleReader;
        readonly Browser iBrowser;
        bool iSubscriptionsUpdated;
        Callbacks iCallbacks;

        public PlaylistManager(SpotifySession aSession, IConsoleReader aConsoleReader, Browser aBrowser)
        {
            iSession = aSession;
            iConsoleReader = aConsoleReader;
            iBrowser = aBrowser;
            iCallbacks = new Callbacks(this);
        }
        public int CmdUpdateSubscriptions(string[] aArgs)
        {
            var pc = iSession.Playlistcontainer();
            for (int i = 0; i != pc.NumPlaylists(); ++i)
            {
                switch (pc.PlaylistType(i))
                {
                    case PlaylistType.Playlist:
                        var playlist = pc.Playlist(i);
                        Playlist.UpdateSubscribers(iSession, playlist);
                        break;
                    default:
                        break;
                }
            }
            iSubscriptionsUpdated = true;
            return 1;
        }

        public int CmdPlaylists(string[] aArgs)
        {
            var pc = iSession.Playlistcontainer();
            Console.WriteLine("{0} entries in the container", pc.NumPlaylists());
            int level = 0;
            Action indent = () => { for (int j=0; j!=level; ++j) Console.Write("\t"); };
            for (int i = 0; i != pc.NumPlaylists(); ++i)
            {
                switch (pc.PlaylistType(i))
                {
                    case PlaylistType.Playlist:
                        Console.Write("{0}. ", i);
                        indent();
                        var pl = pc.Playlist(i);
                        Console.Write(pl.Name());
                        if (iSubscriptionsUpdated)
                            Console.Write(" ({0} subscribers)", pl.NumSubscribers());
                        int unseen = pc.GetUnseenTracks(pl, null);
                        if (unseen != 0)
                        {
                            Console.Write(" ({0} new)", unseen);
                        }
                        Console.WriteLine();
                        break;
                    case PlaylistType.StartFolder:
                        Console.Write("{0}. ", i);
                        indent();
                        Console.WriteLine("Folder: {0} with id {1}", pc.PlaylistFolderName(i), pc.PlaylistFolderId(i));
                        level++;
                        break;
                    case PlaylistType.EndFolder:
                        level--;
                        Console.Write("{0}. ", i);
                        indent();
                        Console.WriteLine("End folder with id {0}", pc.PlaylistFolderId(i));
                        break;
                    case PlaylistType.Placeholder:
                        Console.Write("{0}. Placeholder", i);
                        break;

                }
            }
            return 1;
        }
        public int CmdPlaylist(string[] aArgs)
        {
            var pc = iSession.Playlistcontainer();

            if (aArgs.Length <= 1)
            {
                Console.WriteLine("playlist <playlist-index>");
                return -1;
            }

            int index;
            if (!int.TryParse(aArgs[1], out index) || index<0 || index>=pc.NumPlaylists())
            {
                Console.WriteLine("Invalid index");
                return -1;
            }

            var playlist = pc.Playlist(index);

            int unseen = pc.GetUnseenTracks(playlist, null);

            Console.WriteLine(
                "Playlist {0} by {1}{2}{3}, {4} new tracks",
                playlist.Name(),
                playlist.Owner().DisplayName(),
                playlist.IsCollaborative() ? " (collaborative)" : "",
                playlist.HasPendingChanges() ? " with pending changes" : "",
                unseen
                );
            if (aArgs.Length == 3)
            {
                if (aArgs[2] == "new")
                {
                    if (unseen < 0)
                        return 1;
                    Track[] tracks = new Track[unseen];
                    pc.GetUnseenTracks(playlist, tracks);
                    for (int i = 0; i != unseen; ++i)
                    {
                        PrintTrack2(tracks[i], i);
                    }
                    return 1;
                }
                else if (aArgs[2] == "clear-unseen")
                {
                    pc.ClearUnseenTracks(playlist);
                }
            }
            for (int i = 0; i < playlist.NumTracks(); ++i)
            {
                Track track = playlist.Track(i);
                PrintTrack2(track, i);
            }
            return 1;
        }
        public void PrintTrack2(Track aTrack, int aIndex)
        {
            Console.WriteLine("{0,3}. {1} {2}{3} {4}",
                aIndex,
                Track.IsStarred(iSession, aTrack) ? "*" : " ",
                Track.IsLocal(iSession, aTrack) ? "local" : "     ",
                Track.IsAutolinked(iSession, aTrack) ? "autolinked" : "          ",
                aTrack.Name());
        }
        public int CmdSetAutolink(string[] aArgs)
        {
            var pc = iSession.Playlistcontainer();
            if (aArgs.Length < 3)
            {
                Console.WriteLine("Usage: set_autolink <playlist-index> <0/1>");
                return -1;
            }
            int index;
            if (!int.TryParse(aArgs[1], out index) ||
                index < 0 ||
                index >= pc.NumPlaylists())
            {
                Console.WriteLine("invalid index");
                return -1;
            }

            int autolinkValue;
            if (!int.TryParse(aArgs[2], out autolinkValue) ||
                autolinkValue < 0 ||
                autolinkValue > 1)
            {
                Console.WriteLine("invalid value, specify 0 or 1");
                return -1;
            }

            var playlist = pc.Playlist(index);
            playlist.SetAutolinkTracks(autolinkValue != 0);
            Console.WriteLine("Set autolinking to {0} on playlist {1}", autolinkValue != 0 ? "true": "false", playlist.Name());
            return 1;
        }

        public int CmdAddFolder(string[] aArgs)
        {
            var pc = iSession.Playlistcontainer();
            if (aArgs.Length < 2)
            {
                Console.WriteLine("Usage: add_folder <playlist-index> <name>");
                return -1;
            }

            int index;
            if (!int.TryParse(aArgs[1], out index) ||
                index < 0 ||
                index > pc.NumPlaylists())
            {
                Console.WriteLine("invalid index");
                return -1;
            }

            string name = aArgs[2];

            pc.AddFolder(index, name);

            return 1;
        }

        class UpdateWork
        {
            public int Index { get; set; }
            public Track[] Tracks { get; set; }
        }

        bool ApplyChanges(Playlist aPlaylist, UpdateWork aWork)
        {
            if (!aPlaylist.IsLoaded())
                return false;

            Link l = Link.CreateFromPlaylist(aPlaylist);
            if (l == null)
                return false;
            l.Release();

            Console.Error.Write("Playlist loaded, applying changes ... ");

            try
            {
                aPlaylist.AddTracks(aWork.Tracks, aWork.Index, iSession);
                Console.Error.WriteLine("OK");
            }
            catch (SpotifyException e)
            {
                switch (e.Error)
                {
                    case SpotifyError.InvalidIndata:
                        Console.Error.WriteLine("Invalid position");
                        break;
                    case SpotifyError.PermissionDenied:
                        Console.Error.WriteLine("Access denied");
                        break;
                    default:
                        Console.Error.WriteLine("Other error (should not happen)");
                        break;
                }
            }
            return true;
        }

        class Callbacks : PlaylistListener
        {
            PlaylistManager iPlaylistManager;

            public Callbacks(PlaylistManager aPlaylistManager)
            {
                iPlaylistManager = aPlaylistManager;
            }

            public override void PlaylistStateChanged(Playlist aPlaylist, object aUserdata)
            {
                var updateWork = (UpdateWork)aUserdata;
                if (!iPlaylistManager.ApplyChanges(aPlaylist, updateWork))
                    return;
                aPlaylist.RemoveCallbacks(iPlaylistManager.iCallbacks, aUserdata);
                aPlaylist.Release();
            }
        }

        IEnumerable<Track> CreateTracksFromLinks(IEnumerable<string> aLinks)
        {
            foreach (string linkString in aLinks)
            {
                Link link = Link.CreateFromString(linkString);
                if (link == null)
                {
                    Console.WriteLine("{0} is not a spotify link, skipping", linkString);
                    continue;
                }
                Track track = link.AsTrack();
                if (track == null)
                {
                    Console.WriteLine("{0} is not a track link, skipping", linkString);
                    link.Release();
                    continue;
                }
                track.AddRef();
                link.Release();
                yield return track;
            }
        }

        public int CmdAddTrack(string[] aArgs)
        {
            List<Action> cleanup = new List<Action>();
            try
            {
                if (aArgs.Length < 4)
                {
                    Console.WriteLine("Usage: add <playlist-uri> <position> <track-uri> [<track-uri>...]");
                    return 1;
                }

                Link plink = Link.CreateFromString(aArgs[1]);
                if (plink == null)
                {
                    Console.Error.WriteLine("{0} is not a spotify link", aArgs[1]);
                    return -1;
                }

                cleanup.Add(plink.Release);

                if (plink.Type() != LinkType.Playlist)
                {
                    Console.Error.WriteLine("{0} is not a playlist link", aArgs[1]);
                    return -1;
                }

                Playlist playlist = Playlist.Create(iSession, plink);

                cleanup.Add(() => { if (playlist != null) { playlist.Release(); } });

                int position;
                if (!int.TryParse(aArgs[2], out position) ||
                    position < 0 ||
                    position > playlist.NumTracks())
                {
                    Console.Error.WriteLine("Position out of range");
                }

                var tracks = CreateTracksFromLinks(aArgs.Skip(3)).ToArray();

                var work = new UpdateWork { Index = position, Tracks = tracks };

                if (ApplyChanges(playlist, work))
                {
                    return 1;
                }
                Console.Error.WriteLine("Playlist not yet loaded, waiting...");
                playlist.AddCallbacks(iCallbacks, work);

                playlist = null; // Callback now owns the playlist. Don't release it in 'finally'.
                return 0;
            }
            finally
            {
                cleanup.Reverse();
                foreach (var action in cleanup)
                    action();
            }
        }

        static readonly Dictionary<PlaylistOfflineStatus, string> OfflineStatus = new Dictionary<PlaylistOfflineStatus, string> {
            { PlaylistOfflineStatus.No, "None" },
            { PlaylistOfflineStatus.Yes, "Synchronized" },
            { PlaylistOfflineStatus.Downloading, "Downloading" },
            { PlaylistOfflineStatus.Waiting, "Waiting" }
        };

        public int CmdPlaylistOffline(string[] aArgs)
        {
            Link plink = null;
            Playlist playlist = null;
            try
            {
                if (aArgs.Length == 2 && aArgs[1] == "status")
                {
                    Console.WriteLine("Offline status");
                    Console.WriteLine("  {0} tracks to sync", iSession.OfflineTracksToSync());
                    Console.WriteLine("  {0} offline playlists in total", iSession.OfflineNumPlaylists());
                    return 1;
                }

                if (aArgs.Length < 2 || aArgs.Length > 3)
                {
                    Console.WriteLine("Usage: offline status | <playlist-uri> [<on|off>]");
                    return 1;
                }

                plink = Link.CreateFromString(aArgs[1]);
                if (plink == null)
                {
                    Console.Error.WriteLine("{0} is not a spotify link", aArgs[1]);
                    return -1;
                }

                if (plink.Type() != LinkType.Playlist)
                {
                    Console.Error.WriteLine("{0} is not a playlist link", aArgs[1]);
                    return -1;
                }

                playlist = Playlist.Create(iSession, plink);

                if (aArgs.Length == 3)
                {
                    bool on;
                    if (aArgs[2].ToLowerInvariant() == "on")
                    {
                        on = true;
                    }
                    else if (aArgs[2].ToLowerInvariant() == "off")
                    {
                        on = false;
                    }
                    else
                    {
                        Console.Error.WriteLine("Invalid mode: {0}", aArgs[2]);
                        return -1;
                    }

                    Playlist.SetOfflineMode(iSession, playlist, on);
                }
                else
                {
                    var s = Playlist.GetOfflineStatus(iSession, playlist);
                    Console.WriteLine("Offline status for {0} ({1})", aArgs[1], playlist.Name());
                    Console.WriteLine("  Status: {0}", OfflineStatus[s]);
                    if (s == PlaylistOfflineStatus.Downloading)
                    {
                        Console.WriteLine("    {0}% complete", Playlist.GetOfflineDownloadCompleted(iSession, playlist));
                    }
                }
                return 1;
            }
            finally
            {
                if (playlist != null) playlist.Release();
                if (plink != null) plink.Release();
            }
        }
    }
}

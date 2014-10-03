// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SpotifySharp
{
    public sealed partial class Playlist
    {
        internal static readonly UserDataTable<PlaylistListener> ListenerTable = new UserDataTable<PlaylistListener>();
        public void AddCallbacks(PlaylistListener listener, object userdata)
        {
            IntPtr nativeUserdata = ListenerTable.PutListener(this._handle, listener, userdata);
            var callbacks = PlaylistDelegates.CallbacksPtr;
            NativeMethods.sp_playlist_add_callbacks(this._handle, callbacks, nativeUserdata);
        }
        public void RemoveCallbacks(PlaylistListener listener, object userdata)
        {
            IntPtr nativeUserdata;
            if (!ListenerTable.TryGetNativeUserdata(this._handle, listener, userdata, out nativeUserdata))
            {
                throw new ArgumentException("Playlist.RemoveCallbacks: No callback registered for userdata");
            }
            var callbacks = PlaylistDelegates.CallbacksPtr;
            NativeMethods.sp_playlist_remove_callbacks(this._handle, callbacks, nativeUserdata);
            ListenerTable.RemoveListener(this._handle, listener, userdata);
        }
        public string[] Subscribers()
        {
            IntPtr subscribers = NativeMethods.sp_playlist_subscribers(this._handle);
            string[] retval = SpotifyMarshalling.SubscribersToStrings(subscribers);
            var error = NativeMethods.sp_playlist_subscribers_free(subscribers);
            SpotifyMarshalling.CheckError(error);
            return retval;
        }
        public ImageId GetImage()
        {
            using (var buffer = new NativeByteArray(20))
            {
                if (NativeMethods.sp_playlist_get_image(_handle, buffer.IntPtr))
                {
                    return new ImageId(buffer.Value());
                }
                return null;
            }
        }
    }

    public abstract class PlaylistListener
    {
        public virtual void TracksAdded(Playlist pl, Track[] @tracks, int @position, object userdata) { }
        public virtual void TracksRemoved(Playlist pl, int[] @tracks, object userdata) { }
        public virtual void TracksMoved(Playlist pl, int[] @tracks, int @new_position, object userdata) { }
        public virtual void PlaylistRenamed(Playlist pl, object userdata) { }
        public virtual void PlaylistStateChanged(Playlist pl, object userdata) { }
        public virtual void PlaylistUpdateInProgress(Playlist pl, bool @done, object userdata) { }
        public virtual void PlaylistMetadataUpdated(Playlist pl, object userdata) { }
        public virtual void TrackCreatedChanged(Playlist pl, int @position, User @user, int @when, object userdata) { }
        public virtual void TrackSeenChanged(Playlist pl, int @position, bool @seen, object userdata) { }
        public virtual void DescriptionChanged(Playlist pl, string @desc, object userdata) { }
        public virtual void ImageChanged(Playlist pl, ImageId @image, object userdata) { }
        public virtual void TrackMessageChanged(Playlist pl, int @position, string @message, object userdata) { }
        public virtual void SubscribersChanged(Playlist pl, object userdata) { }
    }


    static class PlaylistDelegates
    {
        static readonly sp_playlist_callbacks Callbacks = CreateCallbacks();
        public static IntPtr CallbacksPtr;

        internal static void AllocNativeCallbacks()
        {
            int bytes = Marshal.SizeOf(typeof(sp_playlist_callbacks));
            IntPtr structPtr = Marshal.AllocHGlobal(bytes);
            Marshal.StructureToPtr(Callbacks, structPtr, false);
            CallbacksPtr = structPtr;
        }

        public static void FreeNativeCallbacks()
        {
            Marshal.FreeHGlobal(CallbacksPtr);
            CallbacksPtr = IntPtr.Zero;
        }

        static sp_playlist_callbacks CreateCallbacks()
        {
            return new sp_playlist_callbacks
            {
                tracks_added = tracks_added,
                tracks_removed = tracks_removed,
                tracks_moved = tracks_moved,
                playlist_renamed = playlist_renamed,
                playlist_state_changed = playlist_state_changed,
                playlist_update_in_progress = playlist_update_in_progress,
                playlist_metadata_updated = playlist_metadata_updated,
                track_created_changed = track_created_changed,
                track_seen_changed = track_seen_changed,
                description_changed = description_changed,
                image_changed = image_changed,
                track_message_changed = track_message_changed,
                subscribers_changed = subscribers_changed
            };
        }
        struct PlaylistAndListener
        {
            public Playlist Playlist;
            public PlaylistListener Listener;
            public object Userdata;
        }
        static PlaylistAndListener GetListener(IntPtr nativePlaylist, IntPtr userdata)
        {
            PlaylistAndListener retVal = new PlaylistAndListener();
            retVal.Playlist = new Playlist(nativePlaylist);
            if (!Playlist.ListenerTable.TryGetListenerFromNativeUserdata(userdata, out retVal.Listener, out retVal.Userdata))
            {
                Debug.Fail("Received callback from native code, but no callbacks are registed.");
            }
            return retVal;
        }
        static void tracks_added(IntPtr @pl, IntPtr @tracks, int @num_tracks, int @position, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.TracksAdded(context.Playlist, SpotifyMarshalling.NativeHandleArrayToArray(tracks, num_tracks).Select(x=>new Track(x)).ToArray(), position, context.Userdata);
        }
        static void tracks_removed(IntPtr @pl, IntPtr @tracks, int @num_tracks, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.TracksRemoved(context.Playlist, SpotifyMarshalling.NativeIntArrayToArray(tracks, num_tracks), context.Userdata);
        }
        static void tracks_moved(IntPtr @pl, IntPtr @tracks, int @num_tracks, int @new_position, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.TracksMoved(context.Playlist, SpotifyMarshalling.NativeIntArrayToArray(tracks, num_tracks), new_position, context.Userdata);
        }
        static void playlist_renamed(IntPtr @pl, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.PlaylistRenamed(context.Playlist, context.Userdata);
        }
        static void playlist_state_changed(IntPtr @pl, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.PlaylistStateChanged(context.Playlist, context.Userdata);
        }
        static void playlist_update_in_progress(IntPtr @pl, [MarshalAs(UnmanagedType.I1)]bool @done, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.PlaylistUpdateInProgress(context.Playlist, done, context.Userdata);
        }
        static void playlist_metadata_updated(IntPtr @pl, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.PlaylistMetadataUpdated(context.Playlist, context.Userdata);
        }
        static void track_created_changed(IntPtr @pl, int @position, IntPtr @user, int @when, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.TrackCreatedChanged(context.Playlist, position, new User(user), when, context.Userdata);
        }
        static void track_seen_changed(IntPtr @pl, int @position, [MarshalAs(UnmanagedType.I1)]bool @seen, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.TrackSeenChanged(context.Playlist, position, seen, context.Userdata);
        }
        static void description_changed(IntPtr @pl, IntPtr @desc, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.DescriptionChanged(context.Playlist, SpotifyMarshalling.Utf8ToString(desc), context.Userdata);
        }
        static void image_changed(IntPtr @pl, IntPtr @image, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.ImageChanged(context.Playlist, new ImageId(image), context.Userdata);
        }
        static void track_message_changed(IntPtr @pl, int @position, IntPtr @message, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.TrackMessageChanged(context.Playlist, position, SpotifyMarshalling.Utf8ToString(message), context.Userdata);
        }
        static void subscribers_changed(IntPtr @pl, IntPtr @userdata)
        {
            var context = GetListener(pl, userdata);
            context.Listener.SubscribersChanged(context.Playlist, context.Userdata);
        }

    }
}
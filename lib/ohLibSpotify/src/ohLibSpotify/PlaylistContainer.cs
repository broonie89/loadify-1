// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SpotifySharp
{
    public sealed partial class PlaylistContainer
    {
        internal static readonly UserDataTable<PlaylistContainerListener> ListenerTable = new UserDataTable<PlaylistContainerListener>();
        public void AddCallbacks(PlaylistContainerListener listener, object userdata)
        {
            IntPtr nativeUserdata = ListenerTable.PutListener(this._handle, listener, userdata);
            var callbacks = PlaylistContainerDelegates.CallbacksPtr;
            Console.WriteLine("sp_playlistcontainer_add_callbacks({0}, {1}, {2})", this._handle, callbacks, nativeUserdata);
            NativeMethods.sp_playlistcontainer_add_callbacks(this._handle, callbacks, nativeUserdata);
        }
        public void RemoveCallbacks(PlaylistContainerListener listener, object userdata)
        {
            IntPtr nativeUserdata;
            if (!ListenerTable.TryGetNativeUserdata(this._handle, listener, userdata, out nativeUserdata))
            {
                throw new ArgumentException("Playlist.RemoveCallbacks: No callback registered for userdata");
            }
            var callbacks = PlaylistContainerDelegates.CallbacksPtr;
            NativeMethods.sp_playlistcontainer_remove_callbacks(this._handle, callbacks, nativeUserdata);
            ListenerTable.RemoveListener(this._handle, listener, userdata);
        }
    }

    public abstract class PlaylistContainerListener
    {
        public virtual void PlaylistAdded(PlaylistContainer pc, Playlist playlist, int @position, object userdata) { }
        public virtual void PlaylistRemoved(PlaylistContainer pc, Playlist playlist, int @position, object userdata) { }
        public virtual void PlaylistMoved(PlaylistContainer pc, Playlist playlist, int @position, int @new_position, object userdata) { }
        public virtual void ContainerLoaded(PlaylistContainer pc, object userdata) { }
    }




    static class PlaylistContainerDelegates
    {
        static readonly sp_playlistcontainer_callbacks Callbacks = CreateCallbacks();
        public static IntPtr CallbacksPtr;

        internal static void AllocNativeCallbacks()
        {
            int bytes = Marshal.SizeOf(typeof(sp_playlistcontainer_callbacks));
            IntPtr structPtr = Marshal.AllocHGlobal(bytes);
            Marshal.StructureToPtr(Callbacks, structPtr, false);
            CallbacksPtr = structPtr;
        }

        public static void FreeNativeCallbacks()
        {
            Marshal.FreeHGlobal(CallbacksPtr);
            CallbacksPtr = IntPtr.Zero;
        }

        static sp_playlistcontainer_callbacks CreateCallbacks()
        {
            return new sp_playlistcontainer_callbacks
            {
                playlist_added = playlist_added,
                playlist_removed = playlist_removed,
                playlist_moved = playlist_moved,
                container_loaded = container_loaded
            };
        }
        struct ContainerAndListener
        {
            public PlaylistContainer Container;
            public PlaylistContainerListener Listener;
            public object Userdata;
        }
        static ContainerAndListener GetListener(IntPtr nativeContainer, IntPtr userdata)
        {
            ContainerAndListener retVal = new ContainerAndListener();
            retVal.Container = new PlaylistContainer(nativeContainer);
            if (!PlaylistContainer.ListenerTable.TryGetListenerFromNativeUserdata(userdata, out retVal.Listener, out retVal.Userdata))
            {
                Debug.Fail("Received callback from native code, but no callbacks are registered.");
            }
            return retVal;
        }

        static void playlist_added(IntPtr @pc, IntPtr @playlist, int @position, IntPtr @userdata)
        {
            var context = GetListener(pc, userdata);
            context.Listener.PlaylistAdded(context.Container, new Playlist(playlist), position, context.Userdata);
        }
        static void playlist_removed(IntPtr @pc, IntPtr @playlist, int @position, IntPtr @userdata)
        {
            var context = GetListener(pc, userdata);
            context.Listener.PlaylistRemoved(context.Container, new Playlist(playlist), position, context.Userdata);
        }
        static void playlist_moved(IntPtr @pc, IntPtr @playlist, int @position, int @new_position, IntPtr @userdata)
        {
            var context = GetListener(pc, userdata);
            context.Listener.PlaylistMoved(context.Container, new Playlist(playlist), position, new_position, context.Userdata);
        }
        static void container_loaded(IntPtr @pc, IntPtr @userdata)
        {
            var context = GetListener(pc, userdata);
            context.Listener.ContainerLoaded(context.Container, context.Userdata);
        }
    }





}
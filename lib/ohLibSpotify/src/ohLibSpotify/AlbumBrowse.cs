// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;

namespace SpotifySharp
{
    public delegate void AlbumBrowseComplete(AlbumBrowse @result, object userdata);
    public sealed partial class AlbumBrowse : IDisposable
    {
        internal static readonly ManagedWrapperTable<AlbumBrowse> BrowseTable = new ManagedWrapperTable<AlbumBrowse>(x=>new AlbumBrowse(x));
        internal static readonly ManagedListenerTable<AlbumBrowseComplete> ListenerTable = new ManagedListenerTable<AlbumBrowseComplete>();

        IntPtr ListenerToken { get; set; }

        static void AlbumBrowseComplete(IntPtr result, IntPtr nativeUserdata)
        {
            var browse = BrowseTable.GetUniqueObject(result);
            AlbumBrowseComplete listener;
            object managedUserdata;
            if (ListenerTable.TryGetListener(nativeUserdata, out listener, out managedUserdata))
            {
                listener(browse, managedUserdata);
            }
        }

        static readonly albumbrowse_complete_cb AlbumBrowseCompleteDelegate = AlbumBrowseComplete;

        public static AlbumBrowse Create(SpotifySession session, Album album, AlbumBrowseComplete callback, object userdata)
        {
            IntPtr listenerToken = ListenerTable.PutUniqueObject(callback, userdata);
            IntPtr ptr = NativeMethods.sp_albumbrowse_create(session._handle, album._handle, AlbumBrowseCompleteDelegate, listenerToken);
            AlbumBrowse browse = BrowseTable.GetUniqueObject(ptr);
            browse.ListenerToken = listenerToken;
            return browse;
        }

        public void Dispose()
        {
            if (_handle == IntPtr.Zero) return;
            var error = NativeMethods.sp_albumbrowse_release(_handle);
            BrowseTable.ReleaseObject(_handle);
            ListenerTable.ReleaseObject(ListenerToken);
            _handle = IntPtr.Zero;
            SpotifyMarshalling.CheckError(error);
        }
    }
}
// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;

namespace SpotifySharp
{
    public delegate void TopListBrowseComplete(TopListBrowse @result, object userdata);
    public sealed partial class TopListBrowse : IDisposable
    {
        internal static readonly ManagedWrapperTable<TopListBrowse> BrowseTable = new ManagedWrapperTable<TopListBrowse>(x=>new TopListBrowse(x));
        internal static readonly ManagedListenerTable<TopListBrowseComplete> ListenerTable = new ManagedListenerTable<TopListBrowseComplete>();

        IntPtr ListenerToken { get; set; }

        static void TopListBrowseComplete(IntPtr result, IntPtr nativeUserdata)
        {
            var browse = BrowseTable.GetUniqueObject(result);
            TopListBrowseComplete listener;
            object managedUserdata;
            if (ListenerTable.TryGetListener(nativeUserdata, out listener, out managedUserdata))
            {
                listener(browse, managedUserdata);
            }
        }

        static readonly toplistbrowse_complete_cb TopListBrowseCompleteDelegate = TopListBrowseComplete;

        public static TopListBrowse Create(SpotifySession session, TopListType type, TopListRegion region, string username, TopListBrowseComplete callback, object userdata)
        {
            using (var utf8_username = SpotifyMarshalling.StringToUtf8(username))
            {
                IntPtr listenerToken = ListenerTable.PutUniqueObject(callback, userdata);
                IntPtr ptr = NativeMethods.sp_toplistbrowse_create(session._handle, type, region, utf8_username.IntPtr, TopListBrowseCompleteDelegate, listenerToken);
                TopListBrowse browse = BrowseTable.GetUniqueObject(ptr);
                browse.ListenerToken = listenerToken;
                return browse;
            }
        }

        public void Dispose()
        {
            if (_handle == IntPtr.Zero) return;
            var error = NativeMethods.sp_toplistbrowse_release(_handle);
            BrowseTable.ReleaseObject(_handle);
            ListenerTable.ReleaseObject(ListenerToken);
            _handle = IntPtr.Zero;
            SpotifyMarshalling.CheckError(error);
        }
    }
}
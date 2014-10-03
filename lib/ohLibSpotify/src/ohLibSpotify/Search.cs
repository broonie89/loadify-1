// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;

namespace SpotifySharp
{
    public delegate void SearchComplete(Search @result, object @userdata);
    public sealed partial class Search : IDisposable
    {
        internal static readonly ManagedWrapperTable<Search> SearchTable = new ManagedWrapperTable<Search>(x=>new Search(x));
        internal static readonly ManagedListenerTable<SearchComplete> ListenerTable = new ManagedListenerTable<SearchComplete>();

        IntPtr ListenerToken { get; set; }

        static void SearchComplete(IntPtr result, IntPtr nativeUserdata)
        {
            var browse = SearchTable.GetUniqueObject(result);
            SearchComplete listener;
            object managedUserdata;
            if (ListenerTable.TryGetListener(nativeUserdata, out listener, out managedUserdata))
            {
                listener(browse, managedUserdata);
            }
        }

        static readonly search_complete_cb SearchCompleteDelegate = SearchComplete;

        public static Search Create(
            SpotifySession session,
            string query,
            int trackOffset,
            int trackCount,
            int albumOffset,
            int albumCount,
            int artistOffset,
            int artistCount,
            int playlistOffset,
            int playlistCount,
            SearchType searchType,
            SearchComplete callback,
            object userdata)
        {
            using (var utf8_query = SpotifyMarshalling.StringToUtf8(query))
            {
                IntPtr listenerToken = ListenerTable.PutUniqueObject(callback, userdata);
                IntPtr ptr = NativeMethods.sp_search_create(
                    session._handle,
                    utf8_query.IntPtr,
                    trackOffset,
                    trackCount,
                    albumOffset,
                    albumCount,
                    artistOffset,
                    artistCount,
                    playlistOffset,
                    playlistCount,
                    searchType,
                    SearchCompleteDelegate,
                    listenerToken);
                Search search = SearchTable.GetUniqueObject(ptr);
                search.ListenerToken = listenerToken;
                return search;
            }
        }

        public void Dispose()
        {
            if (_handle == IntPtr.Zero) return;
            var error = NativeMethods.sp_search_release(_handle);
            SearchTable.ReleaseObject(_handle);
            ListenerTable.ReleaseObject(ListenerToken);
            _handle = IntPtr.Zero;
            SpotifyMarshalling.CheckError(error);
        }
    }
}
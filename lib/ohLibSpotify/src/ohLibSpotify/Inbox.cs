// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Linq;

namespace SpotifySharp
{
    public delegate void InboxPostComplete(Inbox @result, object @userdata);
    public sealed partial class Inbox : IDisposable
    {
        internal static readonly ManagedWrapperTable<Inbox> InboxTable = new ManagedWrapperTable<Inbox>(x=>new Inbox(x));
        internal static readonly ManagedListenerTable<InboxPostComplete> ListenerTable = new ManagedListenerTable<InboxPostComplete>();

        IntPtr ListenerToken { get; set; }

        static void InboxPostComplete(IntPtr result, IntPtr nativeUserdata)
        {
            var browse = InboxTable.GetUniqueObject(result);
            InboxPostComplete listener;
            object managedUserdata;
            if (ListenerTable.TryGetListener(nativeUserdata, out listener, out managedUserdata))
            {
                listener(browse, managedUserdata);
            }
        }

        static readonly inboxpost_complete_cb InboxPostCompleteDelegate = InboxPostComplete;

        public static Inbox PostTracks(
            SpotifySession session,
            string username,
            Track[] tracks,
            string message,
            InboxPostComplete callback,
            object userdata)
        {
            using (var utf8_username = SpotifyMarshalling.StringToUtf8(username))
            using (var utf8_message = SpotifyMarshalling.StringToUtf8(message))
            using (var track_array = SpotifyMarshalling.ArrayToNativeArray(tracks.Select(x=>x._handle).ToArray()))
            {
                IntPtr listenerToken = ListenerTable.PutUniqueObject(callback, userdata);
                IntPtr ptr = NativeMethods.sp_inbox_post_tracks(
                    session._handle,
                    utf8_username.IntPtr,
                    track_array.IntPtr,
                    track_array.Length,
                    utf8_message.IntPtr,
                    InboxPostCompleteDelegate,
                    listenerToken);
                Inbox search = InboxTable.GetUniqueObject(ptr);
                search.ListenerToken = listenerToken;
                return search;
            }
        }

        public void Dispose()
        {
            if (_handle == IntPtr.Zero) return;
            var error = NativeMethods.sp_search_release(_handle);
            InboxTable.ReleaseObject(_handle);
            ListenerTable.ReleaseObject(ListenerToken);
            _handle = IntPtr.Zero;
            SpotifyMarshalling.CheckError(error);
        }
    }
}
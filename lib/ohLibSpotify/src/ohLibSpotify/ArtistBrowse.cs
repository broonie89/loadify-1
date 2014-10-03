// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;

namespace SpotifySharp
{
    public delegate void ArtistBrowseComplete(ArtistBrowse @result, object @userdata);

    public sealed partial class ArtistBrowse : IDisposable
    {
        internal static readonly ManagedWrapperTable<ArtistBrowse> BrowseTable = new ManagedWrapperTable<ArtistBrowse>(x=>new ArtistBrowse(x));
        internal static readonly ManagedListenerTable<ArtistBrowseComplete> ListenerTable = new ManagedListenerTable<ArtistBrowseComplete>();

        IntPtr ListenerToken { get; set; }

        static void ArtistBrowseComplete(IntPtr result, IntPtr nativeUserdata)
        {
            var browse = BrowseTable.GetUniqueObject(result);
            ArtistBrowseComplete listener;
            object managedUserdata;
            if (ListenerTable.TryGetListener(nativeUserdata, out listener, out managedUserdata))
            {
                listener(browse, managedUserdata);
            }
        }

        static readonly artistbrowse_complete_cb ArtistBrowseCompleteDelegate = ArtistBrowseComplete;

        public static ArtistBrowse Create(SpotifySession session, Artist artist, ArtistBrowseType type, ArtistBrowseComplete callback, object userdata)
        {
            IntPtr listenerToken = ListenerTable.PutUniqueObject(callback, userdata);
            IntPtr ptr = NativeMethods.sp_artistbrowse_create(session._handle, artist._handle, type, ArtistBrowseCompleteDelegate, listenerToken);
            ArtistBrowse browse = BrowseTable.GetUniqueObject(ptr);
            browse.ListenerToken = listenerToken;
            return browse;
        }

        public void Dispose()
        {
            if (_handle == IntPtr.Zero) return;
            var error = NativeMethods.sp_artistbrowse_release(_handle);
            BrowseTable.ReleaseObject(_handle);
            ListenerTable.ReleaseObject(ListenerToken);
            _handle = IntPtr.Zero;
            SpotifyMarshalling.CheckError(error);
        }

        public ImageId Portrait(int index)
        {
            return new ImageId(NativeMethods.sp_artistbrowse_portrait(this._handle, index));
        }
    }
}
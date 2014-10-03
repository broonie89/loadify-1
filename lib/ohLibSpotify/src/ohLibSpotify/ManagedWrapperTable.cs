// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;

namespace SpotifySharp
{
    /// <summary>
    /// Tracks managed wrapper objects corresponding to native spotify
    /// objects for types that require a 1-to-1 correspondence, currently
    /// SpotifySession, AlbumBrowse, ArtistBrowse, Inbox, Search and
    /// TopListBrowse.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ManagedWrapperTable<T>
    {
        readonly Func<IntPtr, T> _constructor;
        object _monitor = new object();
        readonly Dictionary<IntPtr, T> _table = new Dictionary<IntPtr, T>();

        public ManagedWrapperTable(Func<IntPtr,T> constructor)
        {
            _constructor = constructor;
        }

        public T GetUniqueObject(IntPtr ptr)
        {
            lock (_monitor)
            {
                T retval;
                if (_table.TryGetValue(ptr, out retval))
                {
                    return retval;
                }
                retval = _constructor(ptr);
                _table[ptr] = retval;
                return retval;
            }
        }

        public void ReleaseObject(IntPtr ptr)
        {
            lock (_monitor)
            {
                _table.Remove(ptr);
            }
        }
    }
}

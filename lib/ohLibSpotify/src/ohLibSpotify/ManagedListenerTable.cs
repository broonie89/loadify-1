// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;

namespace SpotifySharp
{
    /// <summary>
    /// Tracks userdata and listeners for types which have exactly one listener
    /// per object, currently SpotifySession, AlbumBrowse, ArtistBrowse, Inbox,
    /// Search and TopListBrowse.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    internal class ManagedListenerTable<T>
    {
        object _monitor = new object();
        int _counter = 100;
        struct Entry
        {
            public T Listener;
            public object Userdata;
        }
        readonly Dictionary<IntPtr, Entry> _table = new Dictionary<IntPtr, Entry>();

        public IntPtr PutUniqueObject(T obj, object userdata)
        {
            lock (_monitor)
            {
                _counter += 1;
                _table[(IntPtr)_counter] = new Entry { Listener = obj, Userdata = userdata };
                return (IntPtr)_counter;
            }
        }

        public bool TryGetListener(IntPtr ptr, out T listener, out object userdata)
        {
            lock (_monitor)
            {
                Entry entry;
                if (_table.TryGetValue(ptr, out entry))
                {
                    listener = entry.Listener;
                    userdata = entry.Userdata;
                    return true;
                }
                listener = default(T);
                userdata = null;
                return false;
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
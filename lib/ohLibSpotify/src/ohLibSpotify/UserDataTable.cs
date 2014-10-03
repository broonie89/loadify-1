// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;

namespace SpotifySharp
{
    /// <summary>
    /// Tracks userdata and listeners for spotify types that allow multiple listeners
    /// per object, i.e. Image, Playlist and PlaylistContainer.
    /// </summary>
    /// <typeparam name="T">Managed listener type</typeparam>
    internal class UserDataTable<T>
    {
        object _monitor = new object();
        class Entry
        {
            public IntPtr NativeUserdata;
            public object ManagedUserdata;
            public T Listener;
        }
        readonly Dictionary<Tuple<IntPtr, T, object>, Entry> _managedTable = new Dictionary<Tuple<IntPtr, T, object>, Entry>();
        readonly Dictionary<IntPtr, Entry> _nativeTable = new Dictionary<IntPtr, Entry>();
        int _counter = 100; // Starting point is arbitrary, but should help distinguish real tokens from mistakes when debugging.
        string Fmt(object o) { return o == null ? "null" : String.Format("#{0}", o.GetHashCode()); }
        public IntPtr PutListener(IntPtr owner, T listener, object userdata)
        {
            lock (_monitor)
            {
                _counter += 1;
                var token = (IntPtr) _counter;
                var managedKey = Tuple.Create(owner, listener, userdata);
                if (_managedTable.ContainsKey(managedKey))
                {
                    throw new ArgumentException("This userdata is already registered.", "userdata");
                }
                var entry = new Entry
                            {
                                NativeUserdata = token,
                                ManagedUserdata = userdata,
                                Listener = listener
                            };

                _managedTable[managedKey] = entry;
                _nativeTable[token] = entry;

                return token;
            }
        }
        public void RemoveListener(IntPtr owner, T listener, object userdata)
        {
            lock (_monitor)
            {
                var managedKey = Tuple.Create(owner, listener, userdata);
                Entry entry;
                if (!_managedTable.TryGetValue(managedKey, out entry))
                {
                    throw new KeyNotFoundException("RemoveListener: Key not found");
                }
                _managedTable.Remove(managedKey);
                _nativeTable.Remove(entry.NativeUserdata);
            }
        }
        public bool TryGetNativeUserdata(IntPtr owner, T listener, object managedUserdata, out IntPtr nativeUserdata)
        {
            lock (_monitor)
            {
                var managedKey = Tuple.Create(owner, listener, managedUserdata);
                Entry entry;
                if (!_managedTable.TryGetValue(managedKey, out entry))
                {
                    nativeUserdata = IntPtr.Zero;
                    return false;
                }
                nativeUserdata = entry.NativeUserdata;
                return true;
            }
        }
        public bool TryGetListenerFromNativeUserdata(IntPtr nativeUserdata, out T listener, out object managedUserdata)
        {
            lock (_monitor)
            {
                Entry entry;
                if (!_nativeTable.TryGetValue(nativeUserdata, out entry))
                {
                    listener = default(T);
                    managedUserdata = null;
                    return false;
                }
                listener = entry.Listener;
                managedUserdata = entry.ManagedUserdata;
                return true;
            }
        }
    }
}
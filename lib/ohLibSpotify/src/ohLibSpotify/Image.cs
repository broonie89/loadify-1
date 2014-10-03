// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;

namespace SpotifySharp
{
    public sealed partial class Image
    {
        internal static readonly UserDataTable<ImageLoaded> ListenerTable = new UserDataTable<ImageLoaded>();
        public void AddLoadCallbacks(ImageLoaded listener, object userdata)
        {
            IntPtr nativeUserdata = ListenerTable.PutListener(this._handle, listener, userdata);
            NativeMethods.sp_image_add_load_callback(this._handle, ImageDelegates.Callback, nativeUserdata);
        }
        public void RemoveLoadCallback(ImageLoaded listener, object userdata)
        {
            IntPtr nativeUserdata;
            if (!ListenerTable.TryGetNativeUserdata(this._handle, listener, userdata, out nativeUserdata))
            {
                throw new ArgumentException("Image.RemoveCallbacks: No callback registered for userdata");
            }
            NativeMethods.sp_image_remove_load_callback(this._handle, ImageDelegates.Callback, nativeUserdata);
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

        public byte[] Data()
        {
            UIntPtr size = UIntPtr.Zero;
            IntPtr ptr = NativeMethods.sp_image_data(this._handle, ref size);
            byte[] data = new byte[(int)size];
            Marshal.Copy(ptr, data, 0, (int)size);
            return data;
        }

        public ImageId ImageId()
        {
            return new ImageId(NativeMethods.sp_image_image_id(this._handle));
        }

        public static Image Create(SpotifySession session, ImageId image_id)
        {
            using (var id = image_id.Lock())
            {
                return new Image(NativeMethods.sp_image_create(session._handle, id.Ptr));
            }
        }
    }

    static class ImageDelegates
    {
        public static readonly image_loaded_cb Callback = image_loaded;

        struct ImageAndListener
        {
            public Image Image;
            public ImageLoaded Listener;
            public object Userdata;
        }
        static ImageAndListener GetListener(IntPtr nativeImage, IntPtr userdata)
        {
            ImageAndListener retVal = new ImageAndListener();
            retVal.Image = new Image(nativeImage);
            if (!Image.ListenerTable.TryGetListenerFromNativeUserdata(userdata, out retVal.Listener, out retVal.Userdata))
            {
                Debug.Fail("Received callback from native code, but no callbacks are registed.");
            }
            return retVal;
        }
        static void image_loaded(IntPtr @image, IntPtr @userdata)
        {
            var context = GetListener(image, userdata);
            context.Listener(context.Image, context.Userdata);
        }
    }

    public delegate void ImageLoaded(Image image, object userdata);

    public class ImageId
    {
        IntPtr _ptr;
        byte[] _buffer;
        internal ImageId(IntPtr ptr)
        {
            _ptr = ptr;
        }
        internal ImageId(byte[] buffer)
        {
            _buffer = buffer;
        }
        internal LockedImageId Lock()
        {
            if (_buffer != null)
            {
                return new LockedImageId(_buffer);
            }
            return new LockedImageId(_ptr);
        }
    }

    internal class LockedImageId : IDisposable
    {
        internal IntPtr Ptr { get; set; }
        bool _owned;
        public LockedImageId(IntPtr ptr)
        {
            _owned = false;
            Ptr = ptr;
        }
        public LockedImageId(byte[] buffer)
        {
            _owned = true;
            Ptr = Marshal.AllocHGlobal(buffer.Length);
            Marshal.Copy(buffer, 0, Ptr, buffer.Length);
        }
        public void Dispose()
        {
            if (_owned)
            {
                Marshal.FreeHGlobal(Ptr);
                _owned = false;
                Ptr = IntPtr.Zero;
            }
        }
    }
}
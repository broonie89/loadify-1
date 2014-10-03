// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace SpotifySharp
{
    internal static class SpotifyMarshalling
    {
        // This represents the sp_subscribers struct in libspotify.
        //
        // It looks roughly like this:
        // struct
        // {
        //     int count;
        //     char *subscribers[1];
        // }
        //
        // In actual fact, the array might have variable length, specified
        // by count.
        struct SpotifySubscribers
        {
            // Disable warnings about unassigned fields. We don't even instantiate
            // this class. It only exists for marshalling calculations.
#pragma warning disable 649
            // The size of the array.
            public int Count;
            // The first (index 0) item in the array.
            public IntPtr FirstSubscriber;
#pragma warning restore 649
        }

        public static string[] SubscribersToStrings(IntPtr aSubscribers)
        {
            // This is pretty painful.
            // Assumptions
            //     * C int is 32-bit. (This assumption is pervasive in P/Invoke code and
            //       not portable, e.g. to ILP64 systems, but it holds for every current
            //       system we might care about: Windows/Linux/Mac x86/x64, iOs/Android/
            //       Linux arm.)
            //     * Structs may not have padding before the first element. (Guaranteed by C.)
            //     * Arrays may not have padding before the first element. (Guaranteed by C?)
            //     * An array of pointers has the same alignment requirement as a single pointer. (?)
            // First of all, find the offset of the first item of the array inside the structure.
            var structOffset = (int)Marshal.OffsetOf(typeof(SpotifySubscribers), "FirstSubscriber");

            // Construct a pointer to the first item of the array.
            var arrayPtr = aSubscribers + structOffset;

            // Extract Count. I'm not 100% it's safe to use Marshal.PtrToStructure here, so
            // I'm using Marshal.Copy to extract the count into an array.
            int[] countArray = new int[1];
            Marshal.Copy(aSubscribers, countArray, 0, 1);
            int count = countArray[0];

            // Copy the array content into a managed array.
            IntPtr[] utf8Strings = new IntPtr[count];
            Marshal.Copy(arrayPtr, utf8Strings, 0, count);

            // Finally convert the strings to managed strings.
            return utf8Strings.Select(Utf8ToString).ToArray();
        }

        public static Utf8String StringToUtf8(string aString)
        {
            return new Utf8String(aString);
        }
        public static Utf8String AllocBuffer(int aBufferSize)
        {
            return new Utf8String(aBufferSize);
        }
        public static string Utf8ToString(IntPtr aUtf8)
        {
            if (aUtf8 == IntPtr.Zero)
                return null;
            int len = 0;
            while (Marshal.ReadByte(aUtf8, len) != 0)
                len++;
            if (len == 0)
                return "";
            byte[] array = new byte[len];
            Marshal.Copy(aUtf8, array, 0, len);
            return Encoding.UTF8.GetString(array);
        }

        public static NativeHandleArray ArrayToNativeArray<T>(IEnumerable<T> aObjects, Func<T,IntPtr> aMapFunction)
        {
            if (aObjects == null)
            {
                return new NativeHandleArray();
            }
            return ArrayToNativeArray(aObjects.Select(aMapFunction).ToArray());
        }

        public static NativeHandleArray ArrayToNativeArray(IntPtr[] aHandles)
        {
            var nativeArray = new NativeHandleArray(aHandles.Length);
            Marshal.Copy(aHandles, 0, nativeArray.IntPtr, aHandles.Length);
            return nativeArray;
        }

        public static NativeIntArray ArrayToNativeArray(int[] aArray)
        {
            var nativeArray = new NativeIntArray(aArray.Length);
            Marshal.Copy(aArray, 0, nativeArray.IntPtr, aArray.Length);
            return nativeArray;
        }

        public static IntPtr[] NativeHandleArrayToArray(IntPtr aHandles, int aLength)
        {
            IntPtr[] array = new IntPtr[aLength];
            Marshal.Copy(aHandles, array, 0, aLength);
            return array;
        }

        public static int[] NativeIntArrayToArray(IntPtr aInts, int aLength)
        {
            int[] array = new int[aLength];
            Marshal.Copy(aInts, array, 0, aLength);
            return array;
        }

        public static void CheckError(SpotifyError aError)
        {
            if (aError == SpotifyError.Ok) return;
            string message = Utf8ToString(NativeMethods.sp_error_message(aError));
            throw new SpotifyException(aError, message);
        }


        static Dictionary<IntPtr, object> iSpotifyObjects = new Dictionary<IntPtr, object>();
        static Dictionary<IntPtr, object> iSpotifyCallbacks = new Dictionary<IntPtr, object>();
        static Dictionary<IntPtr, SpotifySession> iSpotifySessions = new Dictionary<IntPtr, SpotifySession>();
        static Dictionary<Tuple<IntPtr, IntPtr>, object> iCallbackObjects = new Dictionary<Tuple<IntPtr, IntPtr>, object>();
        static object iGlobalLock = new object();

        //public static object GetSpotifyObject(IntPtr)

        public static SpotifySession GetManagedSession(IntPtr aSessionPtr)
        {
            lock (iGlobalLock)
            {
                SpotifySession session;
                if (iSpotifySessions.TryGetValue(aSessionPtr, out session))
                {
                    return session;
                }
                session = new SpotifySession(aSessionPtr);
                iSpotifySessions[aSessionPtr] = session;
                return session;
            }
        }

        public static void ReleaseManagedSession(IntPtr aSessionPtr)
        {
            lock (iGlobalLock)
            {
                iSpotifySessions.Remove(aSessionPtr);
            }
        }


        public static object GetCallbackObject(IntPtr aSpotifyObject, IntPtr aUserData)
        {
            lock (iGlobalLock)
            {
                object obj;
                if (!iCallbackObjects.TryGetValue(Tuple.Create(aSpotifyObject, aUserData), out obj))
                {
                    Console.WriteLine("No such spotify object: {0}", aSpotifyObject);
                    return null;
                    //throw new Exception("Spotify callback occurred after callbacks were unregistered.");
                }
                return obj;
            }
        }

        public static void RegisterCallbackObject(IntPtr aSpotifyObject, IntPtr aUserData, object aCallbackObject)
        {
            // Note: There might appear to be a race, in that CreateCallbackObject can
            // only be invoked after 
            lock (iGlobalLock)
            {
                Console.WriteLine("Registered spotify object: {0}", aSpotifyObject);
                var key = Tuple.Create(aSpotifyObject, aUserData);
                if (iCallbackObjects.ContainsKey(key))
                {
                    throw new Exception("Spotify callback occurred after callbacks were unregistered.");
                }
                iCallbackObjects.Add(key, aCallbackObject);
            }
        }

        public static void UnregisterCallbackObject(IntPtr aSpotifyObject, IntPtr aUserData)
        {
            lock (iGlobalLock)
            {
                var key = Tuple.Create(aSpotifyObject, aUserData);
                iCallbackObjects.Remove(key);
            }
        }
    }
}
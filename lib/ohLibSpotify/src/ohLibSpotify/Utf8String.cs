// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace SpotifySharp
{
    internal abstract class NativeArray<T> : IDisposable
    {
        IntPtr iPtr;
        public IntPtr IntPtr { get { return iPtr; } }
        public int Length { get { return iArrayLength; } }
        public int AllocatedBytes { get { return iAllocatedBytes; } }
        int iArrayLength;
        int iAllocatedBytes;

        //protected abstract void Copy(T[] aSource, IntPtr aTarget, int aLength);
        protected abstract void Copy(IntPtr aSource, T[] aTarget, int aLength);

        public NativeArray()
        {
        }
        public NativeArray(int aLength)
        {
            int elementSize = Marshal.SizeOf(typeof(T));
            iArrayLength = aLength;
            iAllocatedBytes = elementSize * iArrayLength;
            iPtr = Marshal.AllocHGlobal(iAllocatedBytes);
        }
        public T[] Value()
        {
            if (iPtr == IntPtr.Zero)
                throw new InvalidOperationException("Cannot take Value() of unallocated NativeArray");
            T[] array = new T[iArrayLength];
            Copy(iPtr, array, iArrayLength);
            return array;
        }

        public void CopyTo<T2>(T2[] aTarget, Func<T, T2> aMapFunction)
        {
            if (iPtr == IntPtr.Zero && aTarget == null) return; // No-op if both are null.
            if (iPtr == IntPtr.Zero)
                throw new InvalidOperationException("Cannot use CopyTo with unallocated NativeArray");
            if (aTarget == null)
                throw new InvalidOperationException("Cannot use CopyTo with null target array");
            T[] source = Value();
            for (int i = 0; i != Length; ++i)
            {
                aTarget[i] = aMapFunction(source[i]);
            }
        }

        public void Dispose()
        {
            if (iPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(iPtr);
                iPtr = IntPtr.Zero;
            }
        }
    }

    internal class NativeHandleArray : NativeArray<IntPtr>
    {
        public NativeHandleArray() { }
        public NativeHandleArray(int aLength) : base(aLength) { }

        protected override void Copy(IntPtr aSource, IntPtr[] aTarget, int aLength)
        {
            Marshal.Copy(aSource, aTarget, 0, aLength);
        }
    }

    internal class NativeByteArray : NativeArray<byte>
    {
        public NativeByteArray() { }
        public NativeByteArray(int aLength) : base(aLength) { }

        protected override void Copy(IntPtr aSource, byte[] aTarget, int aLength)
        {
            Marshal.Copy(aSource, aTarget, 0, aLength);
        }
    }

    internal class NativeIntArray : NativeArray<int>
    {
        public NativeIntArray() { }
        public NativeIntArray(int aLength) : base(aLength) { }

        protected override void Copy(IntPtr aSource, int[] aTarget, int aLength)
        {
            Marshal.Copy(aSource, aTarget, 0, aLength);
        }
    }

    internal class Utf8String : IDisposable
    {
        IntPtr iPtr;
        public IntPtr IntPtr { get { return iPtr; } }
        public int BufferLength { get { return iBufferSize; } }
        int iBufferSize;
        public Utf8String(int aBufferSize)
        {
            if (aBufferSize <= 0)
            {
                throw new ArgumentOutOfRangeException("aBufferSize", "Argument must be positive.");
            }
            iPtr = Marshal.AllocHGlobal(aBufferSize);
            iBufferSize = aBufferSize;
        }
        public Utf8String(string aValue)
        {
            if (aValue == null)
            {
                iPtr = IntPtr.Zero;
            }
            else
            {
                byte[] bytes = Encoding.UTF8.GetBytes(aValue);
                iPtr = Marshal.AllocHGlobal(bytes.Length + 1);
                Marshal.Copy(bytes, 0, iPtr, bytes.Length);
                Marshal.WriteByte(iPtr, bytes.Length, 0);
                iBufferSize = bytes.Length + 1;
            }
        }
        public void ReallocIfSmaller(int aMinLength)
        {
            if (iPtr == IntPtr.Zero)
            {
                throw new ObjectDisposedException("Utf8String");
            }
            if (iBufferSize <= aMinLength)
            {
                iPtr = Marshal.ReAllocHGlobal(iPtr, (IntPtr)aMinLength);
                iBufferSize = aMinLength;
            }
        }
        public string Value
        {
            get
            {
                if (iPtr == IntPtr.Zero)
                {
                    throw new ObjectDisposedException("Utf8String");
                }
                return SpotifyMarshalling.Utf8ToString(iPtr);
            }
        }
        public void Dispose()
        {
            if (iPtr != IntPtr.Zero)
            {
                Marshal.FreeHGlobal(iPtr);
                iPtr = IntPtr.Zero;
            }
        }

        public string GetString(int aStringLengthBuffer)
        {
            if (aStringLengthBuffer < 0)
            {
                return null;
            }
            return Value; // TODO: Include \0 characters.
        }
    }
}
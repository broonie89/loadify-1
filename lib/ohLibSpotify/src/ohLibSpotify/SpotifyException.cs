// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;

namespace SpotifySharp
{

    // Maintenance note:
    //     Public methods of public classes should not use prefixes
    //     for argument names. These may appear in client code using
    //     named arguments.

    [Serializable]
    public class SpotifyException : Exception
    {
        public SpotifyError Error { get; private set; }
        public SpotifyException(SpotifyError error)
        {
            Error = error;
        }
        public SpotifyException(SpotifyError error, string message) : base(message)
        {
            Error = error;
        }
        protected SpotifyException(
            System.Runtime.Serialization.SerializationInfo info,
            System.Runtime.Serialization.StreamingContext context)
            : base(info, context) { }
    }
}
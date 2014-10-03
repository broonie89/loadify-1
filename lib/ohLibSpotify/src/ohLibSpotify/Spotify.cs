// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpotifySharp
{
    public static class Spotify
    {
        public static string BuildId()
        {
            return SpotifyMarshalling.Utf8ToString(NativeMethods.sp_build_id());
        }
        public static string ErrorMessage(SpotifyError error)
        {
            return SpotifyMarshalling.Utf8ToString(NativeMethods.sp_error_message(error));
        }
        public static TopListRegion TopListRegion(string country)
        {
            if (country == null) throw new ArgumentNullException("country");
            if (country.Length != 2) throw new ArgumentException("String must be length 2", "country");
            country = country.ToUpperInvariant();
            return (TopListRegion)(((country[0]&0xff)<<8) + (country[1]&0xff));
        }
        public static string CountryString(int country)
        {
            return "" + (char)(country >> 8) + (char)(country & 0xff);
        }
        internal const string NativeLibrary = "libspotify";
    }
}

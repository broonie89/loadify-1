// Copyright 2013 Openhome.
// License: 2-clause BSD. See LICENSE.txt for details.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace SpotifySharp
{
    public partial class Artist
    {
        public ImageId Portrait(ImageSize size)
        {
            return new ImageId(NativeMethods.sp_artist_portrait(this._handle, size));
        }
    }
}

# Usage

ohLibSpotify is a wrapper around libspotify. In general, everything the
libspotify documentation says remains true for ohLibSpotify, unless specified
otherwise.

https://developer.spotify.com/technologies/libspotify/docs/12.1.45/

## Differences from libspotify

So far as possible, ohLibSpotify matches the libspotify API as closely as
possible and is used in the same way. However, there are some differences:

### Naming

In general, naming styles have been changed to match the Framework Design
Guidelines. This should make them more familiar to .NET developers. For example:

 * sp_error becomes SpotifyError
 * sp_session becomes SpotifySession
 * SPOTIFY_ERROR_BAD_API_VERSION becomes BadApiVersion
 * sp_session_create becomes SpotifySession.Create

### Error handling

Where the native function returns sp_error, the equivalent managed method
returns void and throws a SpotifyException if the native function returns
anything except for SP_ERROR_OK. The SpotifyException.Error property contains
the error code. (This may be revisited in future versions, it's not clear that
this is always helpful.)

Don't let exceptions escape from inside callbacks. Doing so will result in
undefined behaviour: it might seem to work sometimes, on some platforms, but in
general it might leak memory or crash. (In future versions we may automatically
catch such exceptions and log a warning or exit the process.)

### Reference counting

The following ohLibSpotify objects remain reference counted. The same rules
apply to these as are stated in libspotify's documentation. If they are
returned to you from a method with "Create" in its name, you are responsible
for calling "Release" when you are finished with them. Otherwise you can keep
them alive by calling "AddRef" to increase their reference count and later
calling "Release" to decrease it. In general you should add a reference to an
object if you want to continue referring to it after returning control to
ohLibSpotify, or if you are going to Release/Dispose its parent object. Failure
to call "Release" when required will result in memory leaks.

 * Album
 * Artist
 * Image
 * Playlist
 * PlaylistContainer

The following ohLibSpotify objects are IDisposable instead of being reference
counted. You must dispose of them when you are finished with them:

 * SpotifySession
 * AlbumBrowse
 * ArtistBrowse
 * Inbox
 * Search
 * TopListBrowse

### Threads

Follow the instructions on Threading in the libspotify docs.

### Asynchronous loading

Follow the instructions on Objects in the libspotify docs. 

### Disk cache

Follow the instructions on Disk Cache Management in the libspotify docs.

### Images

The same constraints apply to ImageId instances as apply to the "const byte \*"
values described under Images in the libspotify docs. They are only safe to use
so long as the object that returned them is still alive. If you want a more
long-lived reference to an image, consider a link, using Link.CreateFromImage.

### Audio

Unlike binary data in other parts of the API, ohLibSpotify does not marshal the
audio data that libspotify passes to the MusicDelivery callback. Your code will
receive it as an IntPtr pointing to the data. This is to provide more
flexibility - if your code needs to pass this pointer back to a native
function, it can do so without copying the data into and out of a managed
buffer. We may revisit this decision as we gain more experience of how the
library is used.

Otherwise, the instructions given under Audio in the libspotify docs apply.

### Strings

ohLibSpotify converts managed strings into UTF-8 and vice-versa as needed. Where native libspotify methods have separate arguments for string data and string lengths, and may require pre-allocation of buffers, in ohLibSpotify all such methods simply receive a System.String as a parameter or return a System.String.


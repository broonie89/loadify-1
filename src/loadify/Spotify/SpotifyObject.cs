using System;
using System.Threading.Tasks;

namespace loadify.Spotify
{
    public static class SpotifyObject
    {
        public static Task<bool> WaitForInitialization(Func<bool> func)
        {
            return Task.Factory.StartNew(() =>
            {
                while (true)
                {
                    if (func())
                        return true;
                };
            });
        }
    }
}

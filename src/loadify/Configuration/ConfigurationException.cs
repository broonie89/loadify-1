using System;

namespace loadify.Configuration
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string msg) :
            base(msg)
        { }

        public ConfigurationException(string msg, Exception inner) :
            base(msg, inner)
        { }
    }
}

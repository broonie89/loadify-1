using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using log4net;

namespace loadify.Logging
{
    public class Log4NetLogger : ILogger
    {
        private readonly ILog _Log;

        public Log4NetLogger(Type loggingType)
        {
            _Log = log4net.LogManager.GetLogger(loggingType);
        }

        public void Debug(string message)
        {
            _Log.Debug(message);
        }

        public void Info(string message)
        {
            _Log.Info(message);
        }

        public void Warning(string message)
        {
            _Log.Warn(message);
        }

        public void Error(string message, Exception exception)
        {
            _Log.Error(message, exception);
        }

        public void Error(string message)
        {
            _Log.Error(message);
        }
    }
}

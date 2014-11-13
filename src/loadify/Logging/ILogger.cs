using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace loadify.Logging
{
    public interface ILogger
    {
        void Debug(string message);
        void Info(string message);
        void Warning(string message);
        void Error(string message, Exception exception);
        void Error(string message);
        void Fatal(string message);
        void Fatal(string message, Exception exception);
    }
}

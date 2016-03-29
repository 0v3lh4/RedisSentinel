using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisSentinel.Exceptions
{
    public class SentinelsTimeoutException : Exception
    {
        public SentinelsTimeoutException(string message, Exception ex)
            : base(message, ex) { }

        public SentinelsTimeoutException(string message)
            : base(message) { }
    }
}

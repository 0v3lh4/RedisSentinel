using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisSentinel.Exceptions
{
    public class SentinelConnectionTimeoutException : Exception
    {
        public SentinelConnectionTimeoutException(string message, Exception ex)
            : base(message, ex) { }

        public SentinelConnectionTimeoutException(string message)
            : base(message) { }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisSentinel.Exceptions
{
    public class SentinelException : Exception
    {
        public SentinelException(string message) : base(message) { }
        public SentinelException(string message, Exception ex) : base(message, ex) { }
    }
}

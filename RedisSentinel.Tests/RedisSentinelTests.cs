using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisSentinel.Exceptions;

namespace RedisSentinel.Tests
{
    [TestClass]
    public class RedisSentinelTests
    {
        [TestMethod]
        [ExpectedException(typeof(SentinelConnectionTimeoutException))]
        public void Should_Connection_Fail()
        {
            var sentinel = new RedisSentinel("192.168.25.100", 26379);
        }

        [TestMethod]
        [ExpectedException(typeof(SentinelsTimeoutException))]
        public void Should_Connection_Fail_All_Sentinels()
        {
            var sentinel = new RedisSentinel(new[] { "192.168.25.100:26379", "192.168.25.10" });
        }
    }
}

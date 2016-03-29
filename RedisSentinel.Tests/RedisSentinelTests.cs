using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisSentinel.Exceptions;
using System.Text;

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
            sentinel.GetMasters();
        }

        [TestMethod]
        [ExpectedException(typeof(SentinelConnectionTimeoutException), "All sentinels are down")]
        public void Should_Connection_Fail_All_Sentinels()
        {
            var sentinel = new RedisSentinel(new[] { "192.168.25.100:26379", "192.168.25.10" });
            sentinel.GetMasters();
        }

        [TestMethod]
        public void Should_Connection_Ok()
        {
            var sentinel = new RedisSentinel("172.16.15.199", 26379);
            sentinel.GetMasters();
        }

        [TestMethod]
        public void Should_Connection_All_Sentinels_Ok()
        {
            var sentinel = new RedisSentinel(new[] { "172.16.15.199:26379", "172.16.15.102" });
            sentinel.GetMasters();
        }

        [TestMethod]
        public void Should_Connection_One_Sentinel_Fail()
        {
            var sentinel = new RedisSentinel(new[] { "172.16.15.19:26379", "172.16.15.102" });
            sentinel.GetMasters();
        }

        [TestMethod]
        public void Should_Return_Command_Sentinel_Masters_Response()
        {
            var sentinel = new RedisSentinel(new[] { "172.16.15.199", "172.16.15.102" });
            var obj = RespReader.Factory.Return(sentinel.SendCommand(Commands.MASTER()));
        }
    }
}

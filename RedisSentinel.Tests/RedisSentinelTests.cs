using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisSentinel.Exceptions;
using System.Text;
using System.Diagnostics;

namespace RedisSentinel.Tests
{
    [TestClass]
    public class RedisSentinelTests
    {
        private const string INVALID_HOST1 = "192.168.25.255";
        private const string INVALID_HOST2 = "192.168.25.255";
        private const string HOST1 = "192.168.25.191";
        private const string HOST2 = "192.168.25.191";
        private const int PORT = 26379;

        [TestMethod]
        [ExpectedException(typeof(SentinelConnectionTimeoutException))]
        public void Should_Connection_Fail()
        {
            var sentinel = new RedisSentinel(INVALID_HOST1, 26379);
            sentinel.GetMasters();
        }

        [TestMethod]
        [ExpectedException(typeof(SentinelConnectionTimeoutException), "All sentinels are down")]
        public void Should_Connection_Fail_All_Sentinels()
        {
            var sentinel = new RedisSentinel(new[] { INVALID_HOST1, INVALID_HOST2 });
            sentinel.GetMasters();
        }

        [TestMethod]
        public void Should_Connection_Ok()
        {
            var sentinel = new RedisSentinel(HOST1, PORT);
            sentinel.GetMasters();
        }

        [TestMethod]
        public void Should_Connection_All_Sentinels_Ok()
        {
            var sentinel = new RedisSentinel(new[] { HOST1, HOST2 });
            sentinel.GetMasters();
        }

        [TestMethod]
        public void Should_Connection_One_Sentinel_Fail()
        {
            var sentinel = new RedisSentinel(new[] { INVALID_HOST1, HOST1 });
            sentinel.GetMasters();
        }

        [TestMethod]
        public void Should_Return_Command_Sentinel_Masters_Response()
        {
            var sentinel = new RedisSentinel(new[] { HOST1, HOST2 });
            var masters = sentinel.GetMasters();

            Assert.AreEqual(1, masters.Count);
            Assert.AreEqual("mymaster", masters[0].Name);
        }

        [TestMethod]
        [ExpectedException(typeof(SentinelException))]
        public void Should_Return_Command_Sentinel_Slaves_Response_With_Invalid_MasterName()
        {
            var sentinel = new RedisSentinel(new[] { HOST1, HOST2 });
            var slaves = sentinel.GetSlaves("nomastername");
        }

        [TestMethod]
        public void Should_Return_Command_Sentinel_Slaves_Response_MasterName()
        {
            var sentinel = new RedisSentinel(new[] { HOST1, HOST2 });

            Stopwatch sw = new Stopwatch();

            sw.Start();

            for (int i = 0; i < 1000; i++)
            {
                var slaves = sentinel.GetSlaves("mymaster");

                Assert.AreEqual(2, slaves.Count);
                Assert.AreEqual(SentinelObjectBase.FlagsType.slave, slaves[0].Flags);
            }

            sw.Stop();

            Console.Write(sw.Elapsed.TotalSeconds);
        }
    }
}

using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using RedisSentinel.Exceptions;
using System.Text;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace RedisSentinel.Tests
{
    [TestClass]
    public class RedisSentinelTests
    {
        private const string INVALID_HOST1 = "192.168.25.255";
        private const string INVALID_HOST2 = "192.168.25.255";
        private const string HOST1 = "192.168.25.191";
        private const string HOST2 = "192.168.25.82";
        private const int PORT = 26379;

        [TestMethod]
        [ExpectedException(typeof(SentinelConnectionTimeoutException))]
        public void Should_Connection_Fail()
        {
            using (var sentinel = new RedisSentinel(INVALID_HOST1, 26379))
            {
                sentinel.GetMasters();
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SentinelConnectionTimeoutException), "All sentinels are down")]
        public void Should_Connection_Fail_All_Sentinels()
        {
            using (var sentinel = new RedisSentinel(new[] { INVALID_HOST1, INVALID_HOST2 }))
            {
                sentinel.GetMasters();
            }
        }

        [TestMethod]
        public void Should_Connection_Ok()
        {
            using (var sentinel = new RedisSentinel(HOST1, PORT))
            {
                sentinel.GetMasters();
            }
        }

        [TestMethod]
        public void Should_Connection_All_Sentinels_Ok()
        {
            using (var sentinel = new RedisSentinel(new[] { HOST1, HOST2 }))
            {
                sentinel.GetMasters();
            }
        }

        [TestMethod]
        public void Should_Connection_One_Sentinel_Fail()
        {
            using (var sentinel = new RedisSentinel(new[] { INVALID_HOST1, HOST1 }))
            {
                sentinel.GetMasters();
            }
        }

        [TestMethod]
        public void Should_Return_Sentinel_Masters_Response()
        {
            using (var sentinel = new RedisSentinel(new[] { HOST1, HOST2 }))
            {
                var masters = sentinel.GetMasters();

                Assert.AreEqual(1, masters.Count);
                Assert.AreEqual("mymaster", masters[0].Name);
            }
        }

        [TestMethod]
        [ExpectedException(typeof(SentinelException))]
        public void Should_Return_Sentinel_Slaves_Response_With_Invalid_MasterName()
        {
            using (var sentinel = new RedisSentinel(new[] { HOST1, HOST2 }))
            {
                var slaves = sentinel.GetSlaves("nomastername");
            }
        }

        [TestMethod]
        public void Should_Return_Sentinel_Slaves_Response_MasterName()
        {
            using (var sentinel = new RedisSentinel(new[] { HOST1, HOST2 }))
            {
                var slaves = sentinel.GetSlaves("mymaster");

                Assert.AreEqual(1, slaves.Count);
                Assert.AreEqual(SentinelObjectBase.FlagsType.slave, slaves[0].Flags);
            }
        }

        [TestMethod]
        public void Should_Return_Sentinel_Master_Host_By_Name()
        {
            using (var sentinel = new RedisSentinel(new[] { HOST1, HOST2 }))
            {
                var masterHost = sentinel.GetMasterHostByName("mymaster");
                Assert.AreEqual("192.168.25.191:6379", masterHost);
            }
        }

        [TestMethod]
        public void Should_Return_Sentinel_Master_And_Slaves_Hosts()
        {
            using (var sentinel = new RedisSentinel(new[] { HOST1, HOST2 }))
            {
                var hosts = sentinel.GetMasterAndSlavesHosts("mymaster");
                Assert.AreEqual("192.168.25.191:6379", hosts.Item1.First());
                Assert.AreEqual("192.168.25.82:6379", hosts.Item2.First());
            }
        }
    }
}

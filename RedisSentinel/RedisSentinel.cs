using RedisSentinel.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace RedisSentinel
{
    public class RedisSentinel : IDisposable
    {
        const string DEFAULT_MASTER_NAME = "mymaster";
        const int DEFAULT_PORT = 26379;
        Socket _socket;

        internal EndPoint[] SentinelsEndpoint { get; private set; }

        private RedisSentinel(EndPoint endpoint, string masterName = null)
        {
            SentinelInitialize(endpoint, masterName);
        }

        public RedisSentinel(string host, int port, string masterName = null)
        {
            SentinelInitialize(CreateEndPoint(host, port), masterName);
        }

        private EndPoint CreateEndPoint(string host, int port)
        {
            return new DnsEndPoint(host, port);
        }

        public RedisSentinel(IEnumerable<string> hosts, string masterName = null)
        {
            SetSentinelsEndpoint(hosts);

            for (int i = 0; i < SentinelsEndpoint.Count(); i++)
            {
                var endpoint = SentinelsEndpoint[0];

                try
                {
                    SentinelInitialize(endpoint, masterName);
                    break;
                }
                catch (SentinelConnectionTimeoutException)
                {
                    continue;
                }
            }

            if (_socket == null)
                throw new SentinelsTimeoutException("All setinels are down");
        }

        private void SentinelInitialize(EndPoint endpoint, string masterName = null)
        {
            Connect(endpoint);
        }

        private void SetSentinelsEndpoint(IEnumerable<string> hosts)
        {
            if (hosts != null && hosts.Count() > 0)
            {
                SentinelsEndpoint = hosts.Select(x => ConvertHostToEndpoint(x)).ToArray();
            }
            else
            {
                throw new ArgumentException("sentinels must have at least one entry");
            }
        }

        private EndPoint ConvertHostToEndpoint(string host)
        {
            var parts = host.Split(':');

            if (parts.Count() == 1)
                return CreateEndPoint(parts[0], DEFAULT_PORT);

            return CreateEndPoint(parts[0], Convert.ToInt32(parts[1]));
        }

        //public byte[] SendCommand()
        //{

        //}

        private void Connect(EndPoint sentinelEndPoint)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var asyncResult = _socket.BeginConnect(sentinelEndPoint, null, null);
            var success = asyncResult.AsyncWaitHandle.WaitOne(10000, true);

            if(!success || !_socket.Connected)
            {
                Dispose();
                asyncResult = null;
                throw new SentinelConnectionTimeoutException("Sentinel connection fail.");
            }
        }

        public void Dispose()
        {
            if (_socket != null && _socket.Connected)
                _socket.Close();

            _socket = null;
        }
    }
}

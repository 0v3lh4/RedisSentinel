using RedisSentinel.Exceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace RedisSentinel
{
    public class RedisSentinel : IDisposable
    {
        const string DEFAULT_MASTER_NAME = "mymaster";
        const int DEFAULT_PORT = 26379;
        const int DEFAULT_BUFFER_SIZE = 1024;
        Socket _socket;

        internal EndPoint[] SentinelsEndpoint { get; private set; }

        internal string MasterName { get; set; }

        /// <summary>
        /// Default value 1000 milliseconds
        /// </summary>
        public int SentinelConnectTimeoutMs { get; set; }

        public RedisSentinel(IEnumerable<string> hosts, string masterName = null)
        {
            SetSentinelsEndpoint(hosts);
            SentinelInitialize(masterName);
        }

        public RedisSentinel(string host, int port, string masterName = null)
        {
            SetSentinelsEndpoint(CreateEndPoint(host, port));
            SentinelInitialize(masterName);
        }

        public List<Tuple<string, int>> GetMasters()
        {

        }

        public byte[] SendCommand(byte[] command)
        {
            Start();

            var receivedData = new byte[DEFAULT_BUFFER_SIZE];
            int bytesRead = 0;
            int bytesReadAux = 0;
            byte[] bytes = null;

            if (_socket.Connected)
            {
                _socket.Send(command, command.Length, SocketFlags.None);

                do
                {
                    bytesRead = _socket.Receive(receivedData, receivedData.Length, SocketFlags.None);

                    if (bytes == null)
                    {
                        bytes = new byte[_socket.Available + bytesRead];
                    }

                    Buffer.BlockCopy(receivedData, 0, bytes, bytesReadAux, bytesRead);

                    bytesReadAux = bytesRead;
                }
                while (_socket.Available > bytesRead);
            }

            Dispose();

            return bytes;
        }

        private EndPoint CreateEndPoint(string host, int port)
        {
            return new DnsEndPoint(host, port);
        }

        private void SentinelInitialize(string masterName = null)
        {
            SentinelConnectTimeoutMs = 1000;
            MasterName = masterName;
        }

        private void SetSentinelsEndpoint(EndPoint endpoint)
        {
            SentinelsEndpoint = new EndPoint[] { endpoint };
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

        private void Start()
        {
            for (int i = 0; i < SentinelsEndpoint.Count(); i++)
            {
                var endpoint = SentinelsEndpoint[i];

                try
                {
                    Connect(endpoint);
                    break;
                }
                catch (SentinelConnectionTimeoutException)
                {
                    continue;
                }
            }

            if (_socket == null)
                throw new SentinelConnectionTimeoutException("All setinels are down");
        }

        private void Connect(EndPoint sentinelEndPoint)
        {
            _socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var asyncResult = _socket.BeginConnect(sentinelEndPoint, null, null);
            var success = asyncResult.AsyncWaitHandle.WaitOne(SentinelConnectTimeoutMs, true);

            if (!success || !_socket.Connected)
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

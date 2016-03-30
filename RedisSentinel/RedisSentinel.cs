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

        internal string MasterName { get; private set; }

        internal int SockectSendAttemps { get; private set; }

        /// <summary>
        /// Default value 1000 milliseconds
        /// </summary>
        public int SentinelConnectTimeoutMs { get; set; }

        /// <summary>
        /// Default value 3
        /// </summary>
        public int SentinelSendAttemps { get; set; }

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

        public List<SentinelMaster> GetMasters()
        {
            List<SentinelMaster> masters = new List<SentinelMaster>();
            var responseCommand = RespReader.Factory.Return(SendCommand(Commands.MASTER()));

            validResponse(responseCommand);

            foreach (var obj in (object[])responseCommand)
            {
                var fields = (object[])obj;
                var master = new SentinelMaster();

                for (int i = 0; i < fields.Count(); i += 2)
                {
                    var key = fields[i].ToString();
                    var value = fields[i + 1];

                    SetFieldMaster(key, value, master);
                }

                masters.Add(master);
            }

            return masters;
        }

        public List<SentinelSlave> GetSlaves(string masterName = null)
        {
            List<SentinelSlave> masters = new List<SentinelSlave>();
            var responseCommand = RespReader.Factory.Return(SendCommand(Commands.SLAVE(masterName ?? DEFAULT_MASTER_NAME)));

            validResponse(responseCommand);

            foreach (var obj in (object[])responseCommand)
            {
                var fields = (object[])obj;
                var slave = new SentinelSlave();

                for (int i = 0; i < fields.Count(); i += 2)
                {
                    var key = fields[i].ToString();
                    var value = fields[i + 1];

                    SetFieldSlave(key, value, slave);
                }

                masters.Add(slave);
            }

            return masters;
        }

        private void validResponse(object responseCommand)
        {
            if(responseCommand is string && responseCommand.ToString().Contains("ERR"))
            {
                throw new SentinelException(responseCommand.ToString().Replace("ERR", ""));
            }
        }

        private void SetFieldSlave(string key, object value, SentinelSlave slaveObj)
        {
            SetFieldSentinelObjectBase(key, value, slaveObj);

            switch (key)
            {
                case SentinelSlave.SENTINEL_KEYS_MASTER_LINK_DOWN_TIME:
                    slaveObj.MasterLinkDowntime = Convert.ToInt32(value);
                    break;
                case SentinelSlave.SENTINEL_KEYS_MASTER_LINK_STATUS:
                    slaveObj.MasterLinkStatus = (SentinelSlave.MasterLinkStatusType)Enum.Parse(
                        typeof(SentinelSlave.MasterLinkStatusType),
                        value.ToString());
                    break;
                case SentinelSlave.SENTINEL_KEYS_MASTER_HOST:
                    slaveObj.MasterHost = value.ToString();
                    break;
                case SentinelSlave.SENTINEL_KEYS_MASTER_PORT:
                    slaveObj.MasterPort = Convert.ToInt32(value);
                    break;
                case SentinelSlave.SENTINEL_KEYS_SLAVE_PRIORITY:
                    slaveObj.SlavePriority = Convert.ToInt32(value);
                    break;
                case SentinelSlave.SENTINEL_KEYS_SLAVE_REPL_OFFSET:
                    slaveObj.SlaveReplOffset = Convert.ToInt32(value);
                    break;
           }
        }


        private void SetFieldMaster(string key, object value, SentinelMaster masterObj)
        {
            SetFieldSentinelObjectBase(key, value, masterObj);

            switch (key)
            {
                case SentinelMaster.SENTINEL_KEYS_CONFIG_EPOCH:
                    masterObj.ConfigEpoch = Convert.ToInt32(value);
                    break;
                case SentinelMaster.SENTINEL_KEYS_NUM_SLAVES:
                    masterObj.SlavesCount = Convert.ToInt32(value);
                    break;
                case SentinelMaster.SENTINEL_KEYS_NUM_OTHER_SENTINELS:
                    masterObj.SentinelsCount = Convert.ToInt32(value);
                    break;
                case SentinelMaster.SENTINEL_KEYS_QUORUM:
                    masterObj.SentinelQuorum = Convert.ToInt32(value);
                    break;
                case SentinelMaster.SENTINEL_KEYS_FAILOVER_TIMEOUT:
                    masterObj.FailoverTimeout = Convert.ToInt32(value);
                    break;
                case SentinelMaster.SENTINEL_KEYS_PARALLEL_SYNCS:
                    masterObj.ParallelSyncs = Convert.ToInt32(value);
                    break;
            }
        }

        private void SetFieldSentinelObjectBase(string key, object value, SentinelObjectBase obj)
        {
            switch (key)
            {
                case SentinelObjectBase.SENTINEL_KEYS_NAME:
                    obj.Name = value.ToString();
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_IP:
                    obj.Host = value.ToString();
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_PORT:
                    obj.Port = Convert.ToInt32(value);
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_DOWN_AFTER_MILLISECONDS:
                    obj.DownAfterMilliseconds = Convert.ToInt32(value);
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_FLAGS:
                    obj.Flags = (SentinelObjectBase.FlagsType)Enum.Parse(
                        typeof(SentinelObjectBase.FlagsType),
                        value.ToString());
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_INFO_REFRESH:
                    obj.InfoRefresh = Convert.ToInt32(value);
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_LAST_OK_PING_REPLY:
                    obj.LastOkPingReply = Convert.ToInt32(value);
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_LAST_PING_REPLY:
                    obj.LastPingReply = Convert.ToInt32(value);
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_LAST_PING_SENT:
                    obj.LastPingSent = Convert.ToInt32(value);
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_PENDING_COMMANDS:
                    obj.PendingCommands = Convert.ToInt32(value);
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_ROLE_REPORTED:
                    obj.RoleReported = (SentinelObjectBase.RoleReportedType)Enum.Parse(
                        typeof(SentinelObjectBase.RoleReportedType),
                        value.ToString());
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_ROLE_REPORTED_TIME:
                    obj.RoleReportedTime = Convert.ToInt32(value);
                    break;
                case SentinelObjectBase.SENTINEL_KEYS_RUNID:
                    obj.RunId = value.ToString();
                    break;
            }
        }

        internal byte[] SendCommand(byte[] command)
        {
            Start();

            var receivedData = new byte[DEFAULT_BUFFER_SIZE];
            int bytesRead = 0;
            int bytesReadAux = 0;
            byte[] bytes = null;

            if (_socket.Connected)
            {
                try
                {
                    _socket.Send(command, command.Length, SocketFlags.None);

                    while (_socket.Available > 0)
                    {
                        bytesRead = _socket.Receive(receivedData, receivedData.Length, SocketFlags.None);

                        if (bytes == null)
                        {
                            bytes = new byte[_socket.Available + bytesRead];
                        }

                        Buffer.BlockCopy(receivedData, 0, bytes, bytesReadAux, bytesRead);

                        bytesReadAux += bytesRead;
                    }
                }
                catch (SocketException ex)
                {
                    if (SockectSendAttemps < SentinelSendAttemps)
                    {
                        Dispose();

                        SockectSendAttemps++;
                        return SendCommand(command);
                    } else
                    {
                        throw new SentinelException(ex.Message, ex);
                    }
                }
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
            SentinelSendAttemps = 3;
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

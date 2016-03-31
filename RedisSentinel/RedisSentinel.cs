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

        internal Socket Socket { get; set; }
        internal NetworkStream Stream { get; set; }
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
            SendCommand(Commands.Master());
            var responseCommand = RespReader.Factory.Object(Stream);

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
            SendCommand(Commands.Slave(masterName ?? MasterName));
            var responseCommand = RespReader.Factory.Object(Stream);

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

        public Tuple<IEnumerable<string>, IEnumerable<string>> GetMasterAndSlavesHosts(string masterName)
        {
            SendCommand(Commands.Slave(masterName ?? MasterName));
            var responseCommand = (object[])RespReader.Factory.Object(Stream);
            HashSet<string> master = new HashSet<string>();
            HashSet<string> slaves = new HashSet<string>();

            foreach (var obj in responseCommand)
            {
                var fields = (object[])obj;
                string slaveHost = null;
                string slavePort = null;
                string masterHost = null;
                string masterPort = null;

                for (int i = 0; i < fields.Count(); i += 2)
                {
                    var key = fields[i].ToString();
                    var value = fields[i + 1];

                    switch (key)
                    {
                        case SentinelObjectBase.SENTINEL_KEYS_IP:
                            slaveHost = value.ToString();
                            break;
                        case SentinelObjectBase.SENTINEL_KEYS_PORT:
                            slavePort = value.ToString();
                            break;
                        case SentinelSlave.SENTINEL_KEYS_MASTER_HOST:
                            masterHost = value.ToString();
                            break;
                        case SentinelSlave.SENTINEL_KEYS_MASTER_PORT:
                            masterPort = value.ToString();
                            break;
                    }

                    if (slavePort != null && slaveHost != null
                        && masterPort != null && masterHost != null)
                        break;
                }

                master.Add(string.Format("{0}:{1}", masterHost, masterPort));
                slaves.Add(string.Format("{0}:{1}", slaveHost, slavePort));
            }

            return new Tuple<IEnumerable<string>, IEnumerable<string>>(master, slaves);
        }

        public string GetMasterHostByName(string masterName = null)
        {
            SendCommand(Commands.MasterAddrByName(masterName ?? MasterName));
            var responseCommand = (object[])RespReader.Factory.Object(Stream);

            validResponse(responseCommand);

            return string.Format("{0}:{1}", responseCommand[0], responseCommand[1]);
        }

        private void validResponse(object responseCommand)
        {
            if (responseCommand is string && responseCommand.ToString().Contains("ERR"))
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

        internal void SendCommand(byte[] command)
        {
            Start();

            try
            {
                Socket.Send(command, command.Length, SocketFlags.None);
                Stream = new NetworkStream(Socket);
            }
            catch (SocketException ex)
            {
                if (SockectSendAttemps < SentinelSendAttemps)
                {
                    Dispose();

                    SockectSendAttemps++;
                    SendCommand(command);
                }
                else
                {
                    throw new SentinelException(ex.Message, ex);
                }
            }
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
            if (IsSocketConnected())
                return;

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

            if (Socket == null)
                throw new SentinelConnectionTimeoutException("All setinels are down");
        }

        private bool IsSocketConnected()
        {
            return Socket != null && Socket.Connected;
        }

        private void Connect(EndPoint sentinelEndPoint)
        {
            if (Socket == null)
                Socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);

            var asyncResult = Socket.BeginConnect(sentinelEndPoint, null, null);
            var success = asyncResult.AsyncWaitHandle.WaitOne(SentinelConnectTimeoutMs, true);

            if (!success || !Socket.Connected)
            {
                Dispose();
                asyncResult = null;
                throw new SentinelConnectionTimeoutException("Sentinel connection fail.");
            }
        }

        public void Dispose()
        {
            if (Stream != null)
            {
                Stream.Close();
                Stream.Dispose();
            }

            if (IsSocketConnected())
            {
                Socket.Shutdown(SocketShutdown.Both);
                Socket.Close();
                Socket.Dispose();
            }

            Socket = null;
            Stream = null;
        }

        public void Close()
        {
            Dispose();
        }
    }
}

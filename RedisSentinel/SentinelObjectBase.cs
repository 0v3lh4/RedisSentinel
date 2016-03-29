using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisSentinel
{
    public class SentinelObjectBase
    {
        public enum FlagsType
        {
            Master,
            Slave
        }
        public enum RoleReportedType
        {
            Master,
            Slave
        }

        public string Name { get; set; }
        public string Host { get; set; }
        public string Port { get; set; }
        public string RunId { get; set; }
        public FlagsType Flags { get; set; }
        public int PendingCommands { get; set; }
        public int LastPingSent { get; set; }
        public int LastOkPingReply { get; set; }
        public int LastPingReply { get; set; }
        public int DownAfterMilliseconds { get; set; }
        public int InfoRefresh { get; set; }
        public RoleReportedType RoleReported { get; set; }
        public int RoleReportedTime { get; set; }
    }
}

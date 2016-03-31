using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisSentinel
{
    public class SentinelSlave : SentinelObjectBase
    {
        public const string SENTINEL_KEYS_MASTER_LINK_DOWN_TIME = "master-link-down_time";
        public const string SENTINEL_KEYS_MASTER_LINK_STATUS = "master-link-status";
        public const string SENTINEL_KEYS_MASTER_HOST = "master-host";
        public const string SENTINEL_KEYS_MASTER_PORT = "master-port";
        public const string SENTINEL_KEYS_SLAVE_PRIORITY = "slave-priority";
        public const string SENTINEL_KEYS_SLAVE_REPL_OFFSET = "slave-repl-offset";

        public enum MasterLinkStatusType
        {
            ok,
            err
        }

        public long MasterLinkDowntime { get; set; }
        public MasterLinkStatusType MasterLinkStatus { get; set; }
        public string MasterHost { get; set; }
        public long MasterPort { get; set; }
        public long SlavePriority { get; set; }
        public long SlaveReplOffset { get; set; }
    }
}

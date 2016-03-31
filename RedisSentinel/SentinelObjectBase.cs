using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisSentinel
{
    public class SentinelObjectBase
    {
        [Flags]
        public enum FlagsType
        {
            master = 1 << 0,
            slave = 1 << 1,
            s_down = 1 << 2,
            o_down = 1 << 3,
            sentinel = 1 << 4,
            disconnected = 1 << 5,
            master_down = 1 << 6,
            failover_in_progress = 1 << 7,
            promoted = 1 << 8,
            reconf_sent = 1 << 9,
            reconf_inprog = 1 << 10,
            reconf_done = 1 << 11
        }

        public enum RoleReportedType
        {
            master,
            slave
        }

        public const string SENTINEL_KEYS_NAME = "name";
        public const string SENTINEL_KEYS_IP = "ip";
        public const string SENTINEL_KEYS_PORT = "port";
        public const string SENTINEL_KEYS_RUNID = "runid";
        public const string SENTINEL_KEYS_FLAGS = "flags";
        public const string SENTINEL_KEYS_PENDING_COMMANDS = "pending-commands";
        public const string SENTINEL_KEYS_LAST_PING_SENT = "last-ping-sent";
        public const string SENTINEL_KEYS_LAST_OK_PING_REPLY = "last-ok-ping-reply";
        public const string SENTINEL_KEYS_LAST_PING_REPLY = "last-ping-reply";
        public const string SENTINEL_KEYS_DOWN_AFTER_MILLISECONDS = "down-after-milliseconds";
        public const string SENTINEL_KEYS_INFO_REFRESH = "info-refresh";
        public const string SENTINEL_KEYS_ROLE_REPORTED = "role-reported";
        public const string SENTINEL_KEYS_ROLE_REPORTED_TIME = "role-reported-time";

        public string Name { get; set; }
        public string Host { get; set; }
        public long Port { get; set; }
        public string RunId { get; set; }
        public FlagsType Flags { get; set; }
        public long PendingCommands { get; set; }
        public long LastPingSent { get; set; }
        public long LastOkPingReply { get; set; }
        public long LastPingReply { get; set; }
        public long DownAfterMilliseconds { get; set; }
        public long InfoRefresh { get; set; }
        public RoleReportedType RoleReported { get; set; }
        public long RoleReportedTime { get; set; }
    }
}

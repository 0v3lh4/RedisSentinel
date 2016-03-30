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
            master,
            slave
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
        public int Port { get; set; }
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RedisSentinel
{
    public class SentinelMaster : SentinelObjectBase
    {
        public const string SENTINEL_KEYS_CONFIG_EPOCH = "config-epoch";
        public const string SENTINEL_KEYS_NUM_SLAVES = "num-slaves";
        public const string SENTINEL_KEYS_NUM_OTHER_SENTINELS = "num-other-sentinels";
        public const string SENTINEL_KEYS_QUORUM = "quorum";
        public const string SENTINEL_KEYS_FAILOVER_TIMEOUT = "failover-timeout";
        public const string SENTINEL_KEYS_PARALLEL_SYNCS = "parallel-syncs";

        public long ConfigEpoch { get; set; }
        public long SlavesCount { get; set; }
        public long SentinelsCount { get; set; }
        public long SentinelQuorum { get; set; }
        public long FailoverTimeout { get; set; }
        public long ParallelSyncs { get; set; }
    }
}

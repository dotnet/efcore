using System;

namespace StackExchange.Redis
{
    /// <summary>
    /// Represents the information known about long-running commands
    /// </summary>    
    public sealed class CommandTrace
    {
        internal static readonly ResultProcessor<CommandTrace[]> Processor = new CommandTraceProcessor();

        internal CommandTrace(long uniqueId, long time, long duration, RedisValue[] arguments)
        {
            this.UniqueId = uniqueId;
            this.Time = RedisBase.UnixEpoch.AddSeconds(time);
            // duration = The amount of time needed for its execution, in microseconds.
            // A tick is equal to 100 nanoseconds, or one ten-millionth of a second. 
            // So 1 microsecond = 10 ticks
            this.Duration = TimeSpan.FromTicks(duration * 10);
            this.Arguments = arguments;
        }

        /// <summary>
        /// The array composing the arguments of the command.
        /// </summary>
        public RedisValue[] Arguments { get; private set; }

        /// <summary>
        /// The amount of time needed for its execution
        /// </summary>
        public TimeSpan Duration { get; private set; }

        /// <summary>
        /// The time at which the logged command was processed.
        /// </summary>
        public DateTime Time { get; private set; }

        /// <summary>
        /// A unique progressive identifier for every slow log entry.
        /// </summary>
        /// <remarks>The entry's unique ID can be used in order to avoid processing slow log entries multiple times (for instance you may have a script sending you an email alert for every new slow log entry). The ID is never reset in the course of the Redis server execution, only a server restart will reset it.</remarks>
        public long UniqueId { get; private set; }

        /// <summary>
        /// Deduces a link to the redis documentation about the specified command
        /// </summary>
        public string GetHelpUrl()
        {
            if (Arguments == null || Arguments.Length == 0) return null;

            const string BaseUrl = "http://redis.io/commands/";

            string encoded0 = Uri.EscapeUriString(((string)Arguments[0]).ToLowerInvariant());

            if (Arguments.Length > 1)
            {

                switch (encoded0)
                {
                    case "script":
                    case "client":
                    case "cluster":
                    case "config":
                    case "debug":
                    case "pubsub":
                        string encoded1 = Uri.EscapeUriString(((string)Arguments[1]).ToLowerInvariant());
                        return BaseUrl + encoded0 + "-" + encoded1;
                }
            }
            return BaseUrl + encoded0;
        }

        private class CommandTraceProcessor : ResultProcessor<CommandTrace[]>
        {
            protected override bool SetResultCore(PhysicalConnection connection, Message message, RawResult result)
            {
                switch(result.Type)
                {
                    case ResultType.MultiBulk:
                        var parts = result.GetItems();
                        CommandTrace[] arr = new CommandTrace[parts.Length];
                        for (int i = 0; i < parts.Length; i++)
                        {
                            
                            var subParts = parts[i].GetItems();
                            long uniqueid, time, duration;
                            if (!subParts[0].TryGetInt64(out uniqueid) || !subParts[1].TryGetInt64(out time) || !subParts[2].TryGetInt64(out duration))
                                return false;
                            arr[i] = new CommandTrace(uniqueid, time, duration, subParts[3].GetItemsAsValues());
                        }
                        SetResult(message, arr);
                        return true;
                }
                return false;
            }
        }
    }
}

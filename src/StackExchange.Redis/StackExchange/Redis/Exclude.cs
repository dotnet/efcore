using System;

namespace StackExchange.Redis
{
    /// <summary>
    /// When performing a range query, by default the start / stop limits are inclusive;
    /// however, both can also be specified separately as exclusive
    /// </summary>
    [Flags]
    public enum Exclude
    {
        /// <summary>
        /// Both start and stop are inclusive
        /// </summary>
        None = 0,
        /// <summary>
        /// Start is exclusive, stop is inclusive
        /// </summary>
        Start = 1,
        /// <summary>
        /// Start is inclusive, stop is exclusive
        /// </summary>
        Stop = 2,
        /// <summary>
        /// Both start and stop are exclusive
        /// </summary>
        Both = Start | Stop
    }
}

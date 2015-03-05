using System;

namespace StackExchange.Redis
{
    /// <summary>
    /// Which settings to export
    /// </summary>
    [Flags]
    public enum ExportOptions
    {
        /// <summary>
        /// No options
        /// </summary>
        None = 0,
        /// <summary>
        /// The output of INFO
        /// </summary>
        Info = 1,
        /// <summary>
        /// The output of CONFIG GET *
        /// </summary>
        Config = 2,
        /// <summary>
        /// The output of CLIENT LIST
        /// </summary>
        Client = 4,
        /// <summary>
        /// The output of CLUSTER NODES
        /// </summary>
        Cluster = 8,
        /// <summary>
        /// Everything available
        /// </summary>
        All = -1
    }
}

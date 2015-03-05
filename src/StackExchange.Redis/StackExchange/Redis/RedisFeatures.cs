using System;
using System.Text;

namespace StackExchange.Redis
{
    /// <summary>
    /// Provides basic information about the features available on a particular version of Redis
    /// </summary>
    public struct RedisFeatures
    {
        internal static readonly Version v2_0_0 = new Version(2, 0, 0),
                                         v2_1_0 = new Version(2, 1, 0),
                                         v2_1_1 = new Version(2, 1, 1),
                                         v2_1_2 = new Version(2, 1, 2),
                                         v2_1_3 = new Version(2, 1, 3),
                                         v2_1_8 = new Version(2, 1, 8),
                                         v2_2_0 = new Version(2, 2, 0),
                                         v2_4_0 = new Version(2, 4, 0),
                                         v2_5_7 = new Version(2, 5, 7),
                                         v2_5_10 = new Version(2, 5, 10),
                                         v2_5_14 = new Version(2, 5, 14),
                                         v2_6_0 = new Version(2, 6, 0),
                                         v2_6_5 = new Version(2, 6, 5),
                                         v2_6_9 = new Version(2, 6, 9),
                                         v2_6_12 = new Version(2, 6, 12),
                                         v2_8_0 = new Version(2, 8, 0),
                                         v2_8_12 = new Version(2, 8, 12),
                                         v2_9_5 = new Version(2, 9, 5);

        private readonly Version version;
        /// <summary>
        /// Create a new RedisFeatures instance for the given version
        /// </summary>
        public RedisFeatures(Version version)
        {
            if (version == null) throw new ArgumentNullException("version");
            this.version = version;
        }

        /// <summary>
        /// Does BITOP / BITCOUNT exist?
        /// </summary>
        public bool BitwiseOperations { get { return Version >= v2_5_10; } }

        /// <summary>
        /// Is CLIENT SETNAME available?
        /// </summary>
        public bool ClientName { get { return Version >= v2_6_9; } }

        /// <summary>
        /// Does EXEC support EXECABORT if there are errors?
        /// </summary>
        public bool ExecAbort { get { return Version >= v2_6_5 && Version != v2_9_5; } }

        /// <summary>
        /// Can EXPIRE be used to set expiration on a key that is already volatile (i.e. has an expiration)?
        /// </summary>
        public bool ExpireOverwrite { get { return Version >= v2_1_3; } }

        /// <summary>
        /// Does HDEL support varadic usage?
        /// </summary>
        public bool HashVaradicDelete { get { return Version >= v2_4_0; } }

        /// <summary>
        /// Does INCRBYFLOAT / HINCRBYFLOAT exist?
        /// </summary>
        public bool IncrementFloat { get { return Version >= v2_5_7; } }

        /// <summary>
        /// Does INFO support sections?
        /// </summary>
        public bool InfoSections { get { return Version >= v2_8_0; } }

        /// <summary>
        /// Is LINSERT available?
        /// </summary>
        public bool ListInsert { get { return Version >= v2_1_1; } }

        /// <summary>
        /// Indicates whether PEXPIRE and PTTL are supported
        /// </summary>
        public bool MillisecondExpiry { get { return Version >= v2_6_0; } }

        /// <summary>
        /// Does SRANDMEMBER support "count"?
        /// </summary>
        public bool MultipleRandom { get { return Version >= v2_5_14; } }

        /// <summary>
        /// Is the PERSIST operation supported?
        /// </summary>
        public bool Persist { get { return Version >= v2_1_2; } }

        /// <summary>
        /// Is RPUSHX and LPUSHX available?
        /// </summary>
        public bool PushIfNotExists { get { return Version >= v2_1_1; } }

        /// <summary>
        /// Are cursor-based scans available?
        /// </summary>
        public bool Scan { get { return Version >= v2_8_0; } }

        /// <summary>
        /// Does EVAL / EVALSHA / etc exist?
        /// </summary>
        public bool Scripting { get { return Version >= v2_5_7; } }

        /// <summary>
        /// Does SET have the EX|PX|NX|XX extensions?
        /// </summary>
        public bool SetConditional { get { return Version >= v2_6_12; } }

        /// <summary>
        /// Does SADD support varadic usage?
        /// </summary>
        public bool SetVaradicAddRemove { get { return Version >= v2_4_0; } }

        /// <summary>
        /// Is STRLEN available?
        /// </summary>
        public bool StringLength { get { return Version >= v2_1_2; } }

        /// <summary>
        /// Is SETRANGE available?
        /// </summary>
        public bool StringSetRange { get { return Version >= v2_1_8; } }

        /// <summary>
        /// Does TIME exist?
        /// </summary>
        public bool Time { get { return Version >= v2_6_0; } }

        /// <summary>
        /// Are Lua changes to the calling database transparent to the calling client?
        /// </summary>
        public bool ScriptingDatabaseSafe { get { return Version >= v2_8_12; } }

        /// <summary>
        /// The Redis version of the server
        /// </summary>
        public Version Version { get { return version ?? v2_0_0; } }
        /// <summary>
        /// Create a string representation of the available features
        /// </summary>
        public override string ToString()
        {
            var sb = new StringBuilder().Append("Features in ").Append(Version).AppendLine()
                .Append("ExpireOverwrite: ").Append(ExpireOverwrite).AppendLine()
                .Append("Persist: ").Append(Persist).AppendLine();

            return sb.ToString();
        }
        // 2.9.5 (cluster beta 1) has a bug in EXECABORT re MOVED and ASK; it only affects cluster, but
        // frankly if you aren't playing with cluster, why are you using 2.9.5 in the first place?
    }
}

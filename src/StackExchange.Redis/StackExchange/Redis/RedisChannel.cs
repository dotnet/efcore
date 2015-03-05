using System;
using System.Text;

namespace StackExchange.Redis
{
    /// <summary>
    /// Represents a pub/sub channel name
    /// </summary>
    public struct RedisChannel : IEquatable<RedisChannel>
    {

        internal static readonly RedisChannel[] EmptyArray = new RedisChannel[0];

        private readonly byte[] value;

        /// <summary>
        /// Create a new redis channel from a buffer, explicitly controlling the pattern mode
        /// </summary>
        public RedisChannel(byte[] value, PatternMode mode) : this(value, DeterminePatternBased(value, mode))
        {   
        }

        /// <summary>
        /// Create a new redis channel from a string, explicitly controlling the pattern mode
        /// </summary>
        public RedisChannel(string value, PatternMode mode) : this(value == null ? null : Encoding.UTF8.GetBytes(value), mode)
        {
        }
        
        private RedisChannel(byte[] value, bool isPatternBased)
        {
            this.value = value;
            this.IsPatternBased = isPatternBased;
        }
        private static bool DeterminePatternBased(byte[] value, PatternMode mode)
        {
            switch (mode)
            {
                case PatternMode.Auto:
                    return value != null && Array.IndexOf(value, (byte)'*') >= 0;
                case PatternMode.Literal: return false;
                case PatternMode.Pattern: return true;
                default:
                    throw new ArgumentOutOfRangeException("mode");
            }
        }

        /// <summary>
        /// Indicates whether the channel-name is either null or a zero-length value
        /// </summary>
        public bool IsNullOrEmpty
        {
            get
            {
                return value == null || value.Length == 0;
            }
        }

        internal bool IsNull
        {
            get { return value == null; }
        }

        internal byte[] Value { get { return value; } }

        /// <summary>
        /// Indicate whether two channel names are not equal
        /// </summary>
        public static bool operator !=(RedisChannel x, RedisChannel y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two channel names are not equal
        /// </summary>
        public static bool operator !=(string x, RedisChannel y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two channel names are not equal
        /// </summary>
        public static bool operator !=(byte[] x, RedisChannel y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two channel names are not equal
        /// </summary>
        public static bool operator !=(RedisChannel x, string y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two channel names are not equal
        /// </summary>
        public static bool operator !=(RedisChannel x, byte[] y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two channel names are equal
        /// </summary>
        public static bool operator ==(RedisChannel x, RedisChannel y)
        {
            return x.IsPatternBased == y.IsPatternBased && RedisValue.Equals(x.value, y.value);
        }

        /// <summary>
        /// Indicate whether two channel names are equal
        /// </summary>
        public static bool operator ==(string x, RedisChannel y)
        {
            return RedisValue.Equals(x == null ? null : Encoding.UTF8.GetBytes(x), y.value);
        }

        /// <summary>
        /// Indicate whether two channel names are equal
        /// </summary>
        public static bool operator ==(byte[] x, RedisChannel y)
        {
            return RedisValue.Equals(x, y.value);
        }

        /// <summary>
        /// Indicate whether two channel names are equal
        /// </summary>
        public static bool operator ==(RedisChannel x, string y)
        {
            return RedisValue.Equals(x.value, y == null ? null : Encoding.UTF8.GetBytes(y));
        }

        /// <summary>
        /// Indicate whether two channel names are equal
        /// </summary>
        public static bool operator ==(RedisChannel x, byte[] y)
        {
            return RedisValue.Equals(x.value, y);
        }

        /// <summary>
        /// See Object.Equals
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is RedisChannel)
            {
                return RedisValue.Equals(this.value, ((RedisChannel)obj).value);
            }
            if (obj is string)
            {
                return RedisValue.Equals(this.value, Encoding.UTF8.GetBytes((string)obj));
            }
            if (obj is byte[])
            {
                return RedisValue.Equals(this.value, (byte[])obj);
            }
            return false;
        }

        /// <summary>
        /// Indicate whether two channel names are equal
        /// </summary>
        public bool Equals(RedisChannel other)
        {
            return this.IsPatternBased == other.IsPatternBased &&
                RedisValue.Equals(this.value, other.value);
        }

        /// <summary>
        /// See Object.GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            return RedisValue.GetHashCode(this.value) + (IsPatternBased ? 1 : 0);
        }

        /// <summary>
        /// Obtains a string representation of the channel name
        /// </summary>
        public override string ToString()
        {
            return ((string)this) ?? "(null)";
        }

        internal static bool AssertStarts(byte[] value, byte[] expected)
        {
            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != value[i]) return false;
            }
            return true;
        }

        internal void AssertNotNull()
        {
            if (IsNull) throw new ArgumentException("A null key is not valid in this context");
        }

        internal RedisChannel Clone()
        {
            byte[] clone = value == null ? null : (byte[])value.Clone();
            return clone;
        }

        internal readonly bool IsPatternBased;

        /// <summary>
        /// The matching pattern for this channel
        /// </summary>
        public enum PatternMode
        {
            /// <summary>
            /// Will be treated as a pattern if it includes *
            /// </summary>
            Auto = 0,
            /// <summary>
            /// Never a pattern
            /// </summary>
            Literal = 1,
            /// <summary>
            /// Always a pattern
            /// </summary>
            Pattern = 2
        }
        /// <summary>
        /// Create a channel name from a String
        /// </summary>
        public static implicit operator RedisChannel(string key)
        {
            if (key == null) return default(RedisChannel);
            return new RedisChannel(Encoding.UTF8.GetBytes(key), PatternMode.Auto);
        }
        /// <summary>
        /// Create a channel name from a Byte[]
        /// </summary>
        public static implicit operator RedisChannel(byte[] key)
        {
            if (key == null) return default(RedisChannel);
            return new RedisChannel(key, PatternMode.Auto);
        }
        /// <summary>
        /// Obtain the channel name as a Byte[]
        /// </summary>
        public static implicit operator byte[] (RedisChannel key)
        {
            return key.value;
        }
        /// <summary>
        /// Obtain the channel name as a String
        /// </summary>
        public static implicit operator string (RedisChannel key)
        {
            var arr = key.value;
            if (arr == null) return null;
            try
            {
                return Encoding.UTF8.GetString(arr);
            }
            catch
            {
                return BitConverter.ToString(arr);
            }
        }
    }
}

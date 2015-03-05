using System;
using System.Text;

namespace StackExchange.Redis
{
    /// <summary>
    /// Represents a key that can be stored in redis
    /// </summary>
    public struct RedisKey : IEquatable<RedisKey>
    {
        internal static readonly RedisKey[] EmptyArray = new RedisKey[0];
        private readonly byte[] keyPrefix;
        private readonly object keyValue; // always either a string or a byte[]
        internal RedisKey(byte[] keyPrefix, object keyValue)
        {
            this.keyPrefix = (keyPrefix != null && keyPrefix.Length == 0) ? null : keyPrefix;
            this.keyValue = keyValue;
        }
        
        internal RedisKey AsPrefix()
        {
            return new RedisKey((byte[])this, null);
        }
        internal bool IsNull
        {
            get { return keyPrefix == null && keyValue == null; }
        }

        internal bool IsEmpty
        {
            get
            {
                if (keyPrefix != null) return false;
                if (keyValue == null) return true;
                if (keyValue is string) return ((string)keyValue).Length == 0;
                return ((byte[])keyValue).Length == 0;
            }
        }

        internal byte[] KeyPrefix { get { return keyPrefix; } }
        internal object KeyValue { get { return keyValue; } }

        /// <summary>
        /// Indicate whether two keys are not equal
        /// </summary>
        public static bool operator !=(RedisKey x, RedisKey y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two keys are not equal
        /// </summary>
        public static bool operator !=(string x, RedisKey y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two keys are not equal
        /// </summary>
        public static bool operator !=(byte[] x, RedisKey y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two keys are not equal
        /// </summary>
        public static bool operator !=(RedisKey x, string y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two keys are not equal
        /// </summary>
        public static bool operator !=(RedisKey x, byte[] y)
        {
            return !(x == y);
        }

        /// <summary>
        /// Indicate whether two keys are equal
        /// </summary>
        public static bool operator ==(RedisKey x, RedisKey y)
        {
            return CompositeEquals(x.keyPrefix, x.keyValue, y.keyPrefix, y.keyValue);
        }

        /// <summary>
        /// Indicate whether two keys are equal
        /// </summary>
        public static bool operator ==(string x, RedisKey y)
        {
            return CompositeEquals(null, x, y.keyPrefix, y.keyValue);
        }

        /// <summary>
        /// Indicate whether two keys are equal
        /// </summary>
        public static bool operator ==(byte[] x, RedisKey y)
        {
            return CompositeEquals(null, x, y.keyPrefix, y.keyValue);
        }

        /// <summary>
        /// Indicate whether two keys are equal
        /// </summary>
        public static bool operator ==(RedisKey x, string y)
        {
            return CompositeEquals(x.keyPrefix, x.keyValue, null, y);
        }

        /// <summary>
        /// Indicate whether two keys are equal
        /// </summary>
        public static bool operator ==(RedisKey x, byte[] y)
        {
            return CompositeEquals(x.keyPrefix, x.keyValue, null, y);
        }

        /// <summary>
        /// See Object.Equals
        /// </summary>
        public override bool Equals(object obj)
        {
            if (obj is RedisKey)
            {
                var other = (RedisKey)obj;
                return CompositeEquals(this.keyPrefix, this.keyValue, other.keyPrefix, other.keyValue);
            }
            if (obj is string || obj is byte[])
            {
                return CompositeEquals(this.keyPrefix, this.keyValue, null, obj);
            }
            return false;
        }

        /// <summary>
        /// Indicate whether two keys are equal
        /// </summary>
        public bool Equals(RedisKey other)
        {
            return CompositeEquals(this.keyPrefix, this.keyValue, other.keyPrefix, other.keyValue);
        }

        private static bool CompositeEquals(byte[] keyPrefix0, object keyValue0, byte[] keyPrefix1, object keyValue1)
        {
            if(RedisValue.Equals(keyPrefix0, keyPrefix1))
            {
                if (keyValue0 == keyValue1) return true; // ref equal
                if (keyValue0 == null || keyValue1 == null) return false; // null vs non-null

                if (keyValue0 is string && keyValue1 is string) return ((string)keyValue0) == ((string)keyValue1);
                if (keyValue0 is byte[] && keyValue1 is byte[]) return RedisValue.Equals((byte[])keyValue0, (byte[])keyValue1);
            }

            return RedisValue.Equals(ConcatenateBytes(keyPrefix0, keyValue0, null), ConcatenateBytes(keyPrefix1, keyValue1, null));
        }

        /// <summary>
        /// See Object.GetHashCode
        /// </summary>
        public override int GetHashCode()
        {
            int chk0 = (keyPrefix == null) ? 0 : RedisValue.GetHashCode(this.keyPrefix),
                chk1 = keyValue is string ? keyValue.GetHashCode() : RedisValue.GetHashCode((byte[])keyValue);

            return unchecked((17 * chk0) + chk1);
        }

        /// <summary>
        /// Obtains a string representation of the key
        /// </summary>
        public override string ToString()
        {
            return ((string)this) ?? "(null)";
        }

        internal RedisValue AsRedisValue()
        {
            return (byte[])this;
        }

        internal void AssertNotNull()
        {
            if (IsNull) throw new ArgumentException("A null key is not valid in this context");
        }

        /// <summary>
        /// Create a key from a String
        /// </summary>
        public static implicit operator RedisKey(string key)
        {
            if (key == null) return default(RedisKey);
            return new RedisKey(null, key);
        }
        /// <summary>
        /// Create a key from a Byte[]
        /// </summary>
        public static implicit operator RedisKey(byte[] key)
        {
            if (key == null) return default(RedisKey);
            return new RedisKey(null, key);
        }
        /// <summary>
        /// Obtain the key as a Byte[]
        /// </summary>
        public static implicit operator byte[](RedisKey key)
        {
            return ConcatenateBytes(key.keyPrefix, key.keyValue, null);
        }
        /// <summary>
        /// Obtain the key as a String
        /// </summary>
        public static implicit operator string(RedisKey key)
        {
            byte[] arr;
            if(key.keyPrefix == null)
            {
                if (key.keyValue == null) return null;

                if (key.keyValue is string) return (string)key.keyValue;

                arr = (byte[])key.keyValue;
            }
            else
            {
                arr = (byte[])key;
            }
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

        /// <summary>
        /// Concatenate two keys
        /// </summary>
        [Obsolete]
        public static RedisKey operator +(RedisKey x, RedisKey y)
        {
            return new RedisKey(ConcatenateBytes(x.keyPrefix, x.keyValue, y.keyPrefix), y.keyValue);
        }

        internal static RedisKey WithPrefix(byte[] prefix, RedisKey value)
        {
            if(prefix == null || prefix.Length == 0) return value;
            if (value.keyPrefix == null) return new RedisKey(prefix, value.keyValue);
            if (value.keyValue == null) return new RedisKey(prefix, value.keyPrefix);

            // two prefixes; darn
            byte[] copy = new byte[prefix.Length + value.keyPrefix.Length];
            Buffer.BlockCopy(prefix, 0, copy, 0, prefix.Length);
            Buffer.BlockCopy(value.keyPrefix, 0, copy, prefix.Length, value.keyPrefix.Length);
            return new RedisKey(copy, value.keyValue);
        }

        internal static byte[] ConcatenateBytes(byte[] a, object b, byte[] c)
        {
            if ((a == null || a.Length == 0) && (c == null || c.Length == 0))
            {
                if (b == null) return null;
                if (b is string) return Encoding.UTF8.GetBytes((string)b);
                return (byte[])b;
            }

            int aLen = a == null ? 0 : a.Length,
                bLen = b == null ? 0 : (b is string
                ? Encoding.UTF8.GetByteCount((string)b)
                : ((byte[])b).Length),
                cLen = c == null ? 0 : c.Length;

            byte[] result = new byte[aLen + bLen + cLen];
            if (aLen != 0) Buffer.BlockCopy(a, 0, result, 0, aLen);
            if (bLen != 0)
            {
                if (b is string)
                {
                    string s = (string)b;
                    Encoding.UTF8.GetBytes(s, 0, s.Length, result, aLen);
                }
                else
                {
                    Buffer.BlockCopy((byte[])b, 0, result, aLen, bLen);
                }
            }
            if (cLen != 0) Buffer.BlockCopy(c, 0, result, aLen + bLen, cLen);
            return result;
        }
    }
}

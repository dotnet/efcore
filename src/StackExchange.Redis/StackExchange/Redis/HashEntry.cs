using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StackExchange.Redis
{
    /// <summary>
    /// Describes a hash-field (a name/value pair)
    /// </summary>
    public struct HashEntry : IEquatable<HashEntry>
    {
        internal readonly RedisValue name, value;

        /// <summary>
        /// Initializes a HashEntry value
        /// </summary>
        public HashEntry(RedisValue name, RedisValue value)
        {
            this.name = name;
            this.value = value;
        }
        /// <summary>
        /// The name of the hash field
        /// </summary>
        public RedisValue Name { get { return name; } }
        /// <summary>
        /// The value of the hash field
        /// </summary>
        public RedisValue Value{ get { return value; } }

        /// <summary>
        /// The name of the hash field
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Obsolete("Please use Name", false)]
        public RedisValue Key { get { return name; } }

        /// <summary>
        /// Converts to a key/value pair
        /// </summary>
        public static implicit operator KeyValuePair<RedisValue, RedisValue>(HashEntry value)
        {
            return new KeyValuePair<RedisValue, RedisValue>(value.name, value.value);
        }
        /// <summary>
        /// Converts from a key/value pair
        /// </summary>
        public static implicit operator HashEntry(KeyValuePair<RedisValue, RedisValue> value)
        {
            return new HashEntry(value.Key, value.Value);
        }

        /// <summary>
        /// See Object.ToString()
        /// </summary>
        public override string ToString()
        {
            return name + ": " + value;
        }
        /// <summary>
        /// See Object.GetHashCode()
        /// </summary>
        public override int GetHashCode()
        {
            return name.GetHashCode() ^ value.GetHashCode();
        }
        /// <summary>
        /// Compares two values for equality
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is HashEntry && Equals((HashEntry)obj);
        }

        /// <summary>
        /// Compares two values for equality
        /// </summary>
        public bool Equals(HashEntry value)
        {
            return this.name == value.name && this.value == value.value;
        }
        /// <summary>
        /// Compares two values for equality
        /// </summary>
        public static bool operator ==(HashEntry x, HashEntry y)
        {
            return x.name == y.name && x.value == y.value;
        }
        /// <summary>
        /// Compares two values for non-equality
        /// </summary>
        public static bool operator !=(HashEntry x, HashEntry y)
        {
            return x.name != y.name || x.value != y.value;
        }
    }
}

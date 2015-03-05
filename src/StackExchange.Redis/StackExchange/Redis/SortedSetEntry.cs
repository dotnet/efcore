using System;
using System.Collections.Generic;
using System.ComponentModel;

namespace StackExchange.Redis
{
    /// <summary>
    /// Describes a sorted-set element with the corresponding value
    /// </summary>
    public struct SortedSetEntry : IEquatable<SortedSetEntry>, IComparable, IComparable<SortedSetEntry>
    {
        internal readonly RedisValue element;
        internal readonly double score;

        /// <summary>
        /// Initializes a SortedSetEntry value
        /// </summary>
        public SortedSetEntry(RedisValue element, double score)
        {
            this.element = element;
            this.score = score;
        }
        /// <summary>
        /// The unique element stored in the sorted set
        /// </summary>
        public RedisValue Element { get { return element; } }
        /// <summary>
        /// The score against the element
        /// </summary>
        public double Score { get { return score; } }

        /// <summary>
        /// The score against the element
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Obsolete("Please use Score", false)]
        public double Value { get { return score; } }

        /// <summary>
        /// The unique element stored in the sorted set
        /// </summary>
        [Browsable(false), EditorBrowsable(EditorBrowsableState.Never), Obsolete("Please use Element", false)]
        public RedisValue Key { get { return element; } }

        /// <summary>
        /// Converts to a key/value pair
        /// </summary>
        public static implicit operator KeyValuePair<RedisValue,double>(SortedSetEntry value)
        {
            return new KeyValuePair<RedisValue, double>(value.element, value.score);
        }
        /// <summary>
        /// Converts from a key/value pair
        /// </summary>
        public static implicit operator SortedSetEntry(KeyValuePair<RedisValue, double> value)
        {
            return new SortedSetEntry(value.Key, value.Value);
        }

        /// <summary>
        /// See Object.ToString()
        /// </summary>
        public override string ToString()
        {
            return element + ": " + score;
        }
        /// <summary>
        /// See Object.GetHashCode()
        /// </summary>
        public override int GetHashCode()
        {
            return element.GetHashCode() ^ score.GetHashCode();
        }
        /// <summary>
        /// Compares two values for equality
        /// </summary>
        public override bool Equals(object obj)
        {
            return obj is SortedSetEntry && Equals((SortedSetEntry)obj);
        }

        /// <summary>
        /// Compares two values for equality
        /// </summary>
        public bool Equals(SortedSetEntry value)
        {
            return this.score == value.score && this.element == value.element;
        }

        /// <summary>
        /// Compares two values by score
        /// </summary>
        public int CompareTo(SortedSetEntry value)
        {
            return this.score.CompareTo(value.score);
        }

        /// <summary>
        /// Compares two values by score
        /// </summary>
        public int CompareTo(object value)
        {
            return value is SortedSetEntry ? CompareTo((SortedSetEntry)value) : -1;
        }

        /// <summary>
        /// Compares two values for equality
        /// </summary>
        public static bool operator ==(SortedSetEntry x, SortedSetEntry y)
        {
            return x.score == y.score && x.element == y.element;
        }
        /// <summary>
        /// Compares two values for non-equality
        /// </summary>
        public static bool operator !=(SortedSetEntry x, SortedSetEntry y)
        {
            return x.score != y.score || x.element != y.element;
        }

    }
}

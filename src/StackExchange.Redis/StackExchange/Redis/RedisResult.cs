using System;

namespace StackExchange.Redis
{
    /// <summary>
    /// Represents a general-purpose result from redis, that may be cast into various anticipated types
    /// </summary>
    public abstract class RedisResult
    {

        // internally, this is very similar to RawResult, except it is designed to be usable
        // outside of the IO-processing pipeline: the buffers are standalone, etc

        internal static RedisResult TryCreate(PhysicalConnection connection, RawResult result)
        {
            try
            {
                switch (result.Type)
                {
                    case ResultType.Integer:
                    case ResultType.SimpleString:
                    case ResultType.BulkString:
                        return new SingleRedisResult(result.AsRedisValue());
                    case ResultType.MultiBulk:
                        var items = result.GetItems();
                        var arr = new RedisResult[items.Length];
                        for (int i = 0; i < arr.Length; i++)
                        {
                            var next = TryCreate(connection, items[i]);
                            if (next == null) return null; // means we didn't understand
                            arr[i] = next;
                        }
                        return new ArrayRedisResult(arr);
                    case ResultType.Error:
                        return new ErrorRedisResult(result.GetString());
                    default:
                        return null;
                }
            } catch(Exception ex)
            {
                if(connection != null) connection.OnInternalError(ex);
                return null; // will be logged as a protocol fail by the processor
            }
        }

        /// <summary>
        /// Indicates whether this result was a null result
        /// </summary>
        public abstract bool IsNull { get; }

        /// <summary>
        /// Interprets the result as a String
        /// </summary>
        public static explicit operator string (RedisResult result) { return result.AsString(); }
        /// <summary>
        /// Interprets the result as a Byte[]
        /// </summary>
        public static explicit operator byte[] (RedisResult result) { return result.AsByteArray(); }
        /// <summary>
        /// Interprets the result as a Double
        /// </summary>
        public static explicit operator double (RedisResult result) { return result.AsDouble(); }
        /// <summary>
        /// Interprets the result as an Int64
        /// </summary>
        public static explicit operator long (RedisResult result) { return result.AsInt64(); }
        /// <summary>
        /// Interprets the result as an Int32
        /// </summary>
        public static explicit operator int (RedisResult result) { return result.AsInt32(); }
        /// <summary>
        /// Interprets the result as a Boolean
        /// </summary>
        public static explicit operator bool (RedisResult result) { return result.AsBoolean(); }
        /// <summary>
        /// Interprets the result as a RedisValue
        /// </summary>
        public static explicit operator RedisValue (RedisResult result) { return result.AsRedisValue(); }
        /// <summary>
        /// Interprets the result as a RedisKey
        /// </summary>
        public static explicit operator RedisKey (RedisResult result) { return result.AsRedisKey(); }
        /// <summary>
        /// Interprets the result as a Nullable Double
        /// </summary>
        public static explicit operator double? (RedisResult result) { return result.AsNullableDouble(); }
        /// <summary>
        /// Interprets the result as a Nullable Int64
        /// </summary>
        public static explicit operator long? (RedisResult result) { return result.AsNullableInt64(); }
        /// <summary>
        /// Interprets the result as a Nullable Int32
        /// </summary>
        public static explicit operator int? (RedisResult result) { return result.AsNullableInt32(); }
        /// <summary>
        /// Interprets the result as a Nullable Boolean
        /// </summary>
        public static explicit operator bool? (RedisResult result) { return result.AsNullableBoolean(); }
        /// <summary>
        /// Interprets the result as an array of String
        /// </summary>
        public static explicit operator string[] (RedisResult result) { return result.AsStringArray(); }
        /// <summary>
        /// Interprets the result as an array of Byte[]
        /// </summary>
        public static explicit operator byte[][] (RedisResult result) { return result.AsByteArrayArray(); }
        /// <summary>
        /// Interprets the result as an array of Double
        /// </summary>
        public static explicit operator double[] (RedisResult result) { return result.AsDoubleArray(); }
        /// <summary>
        /// Interprets the result as an array of Int64
        /// </summary>
        public static explicit operator long[] (RedisResult result) { return result.AsInt64Array(); }
        /// <summary>
        /// Interprets the result as an array of Int32
        /// </summary>
        public static explicit operator int[] (RedisResult result) { return result.AsInt32Array(); }
        /// <summary>
        /// Interprets the result as an array of Boolean
        /// </summary>
        public static explicit operator bool[] (RedisResult result) { return result.AsBooleanArray(); }
        /// <summary>
        /// Interprets the result as an array of RedisValue
        /// </summary>
        public static explicit operator RedisValue[] (RedisResult result) { return result.AsRedisValueArray(); }
        /// <summary>
        /// Interprets the result as an array of RedisKey
        /// </summary>
        public static explicit operator RedisKey[] (RedisResult result) { return result.AsRedisKeyArray(); }
        /// <summary>
        /// Interprets the result as an array of RedisResult
        /// </summary>
        public static explicit operator RedisResult[] (RedisResult result) { return result.AsRedisResultArray(); }

        internal abstract bool AsBoolean();

        internal abstract bool[] AsBooleanArray();

        internal abstract byte[] AsByteArray();

        internal abstract byte[][] AsByteArrayArray();

        internal abstract double AsDouble();

        internal abstract double[] AsDoubleArray();

        internal abstract int AsInt32();

        internal abstract int[] AsInt32Array();

        internal abstract long AsInt64();

        internal abstract long[] AsInt64Array();

        internal abstract bool? AsNullableBoolean();

        internal abstract double? AsNullableDouble();

        internal abstract int? AsNullableInt32();

        internal abstract long? AsNullableInt64();

        internal abstract RedisKey AsRedisKey();

        internal abstract RedisKey[] AsRedisKeyArray();

        internal abstract RedisResult[] AsRedisResultArray();

        internal abstract RedisValue AsRedisValue();

        internal abstract RedisValue[] AsRedisValueArray();
        internal abstract string AsString();
        internal abstract string[] AsStringArray();
        private sealed class ArrayRedisResult : RedisResult
        {
            public override bool IsNull
            {
                get { return value == null; }
            }
            private readonly RedisResult[] value;
            public ArrayRedisResult(RedisResult[] value)
            {
                if (value == null) throw new ArgumentNullException("value");
                this.value = value;
            }
            public override string ToString()
            {
                return value.Length + " element(s)";
            }
            internal override bool AsBoolean()
            {
                if (value.Length == 1) return value[0].AsBoolean();
                throw new InvalidCastException();
            }

            internal override bool[] AsBooleanArray() { return Array.ConvertAll(value, x => x.AsBoolean()); }

            internal override byte[] AsByteArray()
            {
                if (value.Length == 1) return value[0].AsByteArray();
                throw new InvalidCastException();
            }
            internal override byte[][] AsByteArrayArray() { return Array.ConvertAll(value, x => x.AsByteArray()); }

            internal override double AsDouble()
            {
                if (value.Length == 1) return value[0].AsDouble();
                throw new InvalidCastException();
            }

            internal override double[] AsDoubleArray() { return Array.ConvertAll(value, x => x.AsDouble()); }

            internal override int AsInt32()
            {
                if (value.Length == 1) return value[0].AsInt32();
                throw new InvalidCastException();
            }

            internal override int[] AsInt32Array() { return Array.ConvertAll(value, x => x.AsInt32()); }

            internal override long AsInt64()
            {
                if (value.Length == 1) return value[0].AsInt64();
                throw new InvalidCastException();
            }

            internal override long[] AsInt64Array() { return Array.ConvertAll(value, x => x.AsInt64()); }

            internal override bool? AsNullableBoolean()
            {
                if (value.Length == 1) return value[0].AsNullableBoolean();
                throw new InvalidCastException();
            }

            internal override double? AsNullableDouble()
            {
                if (value.Length == 1) return value[0].AsNullableDouble();
                throw new InvalidCastException();
            }

            internal override int? AsNullableInt32()
            {
                if (value.Length == 1) return value[0].AsNullableInt32();
                throw new InvalidCastException();
            }

            internal override long? AsNullableInt64()
            {
                if (value.Length == 1) return value[0].AsNullableInt64();
                throw new InvalidCastException();
            }

            internal override RedisKey AsRedisKey()
            {
                if (value.Length == 1) return value[0].AsRedisKey();
                throw new InvalidCastException();
            }

            internal override RedisKey[] AsRedisKeyArray() { return Array.ConvertAll(value, x => x.AsRedisKey()); }

            internal override RedisResult[] AsRedisResultArray() { return value; }

            internal override RedisValue AsRedisValue()
            {
                if (value.Length == 1) return value[0].AsRedisValue();
                throw new InvalidCastException();
            }

            internal override RedisValue[] AsRedisValueArray() { return Array.ConvertAll(value, x => x.AsRedisValue()); }

            internal override string AsString()
            {
                if (value.Length == 1) return value[0].AsString();
                throw new InvalidCastException();
            }
            internal override string[] AsStringArray() { return Array.ConvertAll(value, x => x.AsString()); }
        }

        private sealed class ErrorRedisResult : RedisResult
        {
            private readonly string value;
            public ErrorRedisResult(string value)
            {
                if (value == null) throw new ArgumentNullException("value");
                this.value = value;
            }
            public override bool IsNull
            {
                get { return value == null; }
            }
            public override string ToString() { return value; }
            internal override bool AsBoolean() { throw new RedisServerException(value); }

            internal override bool[] AsBooleanArray() { throw new RedisServerException(value); }

            internal override byte[] AsByteArray() { throw new RedisServerException(value); }

            internal override byte[][] AsByteArrayArray() { throw new RedisServerException(value); }

            internal override double AsDouble() { throw new RedisServerException(value); }

            internal override double[] AsDoubleArray() { throw new RedisServerException(value); }

            internal override int AsInt32() { throw new RedisServerException(value); }

            internal override int[] AsInt32Array() { throw new RedisServerException(value); }

            internal override long AsInt64() { throw new RedisServerException(value); }

            internal override long[] AsInt64Array() { throw new RedisServerException(value); }

            internal override bool? AsNullableBoolean() { throw new RedisServerException(value); }

            internal override double? AsNullableDouble() { throw new RedisServerException(value); }

            internal override int? AsNullableInt32() { throw new RedisServerException(value); }

            internal override long? AsNullableInt64() { throw new RedisServerException(value); }

            internal override RedisKey AsRedisKey() { throw new RedisServerException(value); }

            internal override RedisKey[] AsRedisKeyArray() { throw new RedisServerException(value); }

            internal override RedisResult[] AsRedisResultArray() { throw new RedisServerException(value); }

            internal override RedisValue AsRedisValue() { throw new RedisServerException(value); }

            internal override RedisValue[] AsRedisValueArray() { throw new RedisServerException(value); }

            internal override string AsString() { throw new RedisServerException(value); }
            internal override string[] AsStringArray() { throw new RedisServerException(value); }
        }

        private sealed class SingleRedisResult : RedisResult
        {
            private readonly RedisValue value;
            public SingleRedisResult(RedisValue value)
            {
                this.value = value;
            }

            public override bool IsNull
            {
                get { return value.IsNull; }
            }

            public override string ToString() { return value.ToString(); }
            internal override bool AsBoolean() { return (bool)value; }

            internal override bool[] AsBooleanArray() { return new[] { AsBoolean() }; }

            internal override byte[] AsByteArray() { return (byte[])value; }
            internal override byte[][] AsByteArrayArray() { return new[] { AsByteArray() }; }

            internal override double AsDouble() { return (double)value; }

            internal override double[] AsDoubleArray() { return new[] { AsDouble() }; }

            internal override int AsInt32() { return (int)value; }

            internal override int[] AsInt32Array() { return new[] { AsInt32() }; }

            internal override long AsInt64() { return (long)value; }

            internal override long[] AsInt64Array() { return new[] { AsInt64() }; }

            internal override bool? AsNullableBoolean() { return (bool?)value; }

            internal override double? AsNullableDouble() { return (double?)value; }

            internal override int? AsNullableInt32() { return (int?)value; }

            internal override long? AsNullableInt64() { return (long?)value; }

            internal override RedisKey AsRedisKey() { return (byte[])value; }

            internal override RedisKey[] AsRedisKeyArray() { return new[] { AsRedisKey() }; }

            internal override RedisResult[] AsRedisResultArray() { throw new InvalidCastException(); }

            internal override RedisValue AsRedisValue() { return value; }

            internal override RedisValue[] AsRedisValueArray() { return new[] { AsRedisValue() }; }

            internal override string AsString() { return (string)value; }
            internal override string[] AsStringArray() { return new[] { AsString() }; }
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;

namespace StackExchange.Redis
{


    internal struct RawResult
    {
        public static readonly RawResult EmptyArray = new RawResult(new RawResult[0]);
        public static readonly RawResult Nil = new RawResult();
        private static readonly byte[] emptyBlob = new byte[0];
        private readonly int offset, count;
        private readonly ResultType resultType;
        private Array arr;
        public RawResult(ResultType resultType, byte[] buffer, int offset, int count)
        {
            switch (resultType)
            {
                case ResultType.SimpleString:
                case ResultType.Error:
                case ResultType.Integer:
                case ResultType.BulkString:
                    break;
                default:
                    throw new ArgumentOutOfRangeException("resultType");
            }
            this.resultType = resultType;
            this.arr = buffer;
            this.offset = offset;
            this.count = count;
        }

        public RawResult(RawResult[] arr)
        {
            if (arr == null) throw new ArgumentNullException("arr");
            this.resultType = ResultType.MultiBulk;
            this.offset = 0;
            this.count = arr.Length;
            this.arr = arr;
        }

        public bool HasValue { get { return resultType != ResultType.None; } }

        public bool IsError { get { return resultType == ResultType.Error; } }

        public ResultType Type { get { return resultType; } }
        internal bool IsNull { get { return arr == null; } }

        public override string ToString()
        {
            if (arr == null)
            {
                return "(null)";
            }
            switch (resultType)
            {
                case ResultType.SimpleString:
                case ResultType.Integer:
                case ResultType.Error:
                    return string.Format("{0}: {1}", resultType, GetString());
                case ResultType.BulkString:
                    return string.Format("{0}: {1} bytes", resultType, count);
                case ResultType.MultiBulk:
                    return string.Format("{0}: {1} items", resultType, count);
                default:
                    return "(unknown)";
            }
        }
        internal RedisChannel AsRedisChannel(byte[] channelPrefix, RedisChannel.PatternMode mode)
        {
            switch (resultType)
            {
                case ResultType.SimpleString:
                case ResultType.BulkString:
                    if (channelPrefix == null)
                    {
                        return new RedisChannel(GetBlob(), mode);
                    }
                    if (AssertStarts(channelPrefix))
                    {
                        var src = (byte[])arr;

                        byte[] copy = new byte[count - channelPrefix.Length];
                        Buffer.BlockCopy(src, offset + channelPrefix.Length, copy, 0, copy.Length);
                        return new RedisChannel(copy, mode);
                    }
                    return default(RedisChannel);
                default:
                    throw new InvalidCastException("Cannot convert to RedisChannel: " + resultType);
            }
        }

        internal RedisKey AsRedisKey()
        {
            switch (resultType)
            {
                case ResultType.SimpleString:
                case ResultType.BulkString:
                    return (RedisKey)GetBlob();
                default:
                    throw new InvalidCastException("Cannot convert to RedisKey: " + resultType);
            }
        }
        internal RedisValue AsRedisValue()
        {
            switch (resultType)
            {
                case ResultType.Integer:
                    long i64;
                    if (TryGetInt64(out i64)) return (RedisValue)i64;
                    break;
                case ResultType.SimpleString:
                case ResultType.BulkString:
                    return (RedisValue)GetBlob();
            }
            throw new InvalidCastException("Cannot convert to RedisValue: " + resultType);
        }

        internal unsafe bool IsEqual(byte[] expected)
        {
            if (expected == null) throw new ArgumentNullException("expected");
            if (expected.Length != count) return false;
            var actual = arr as byte[];
            if (actual == null) return false;

            int octets = count / 8, spare = count % 8;
            fixed (byte* actual8 = &actual[offset])
            fixed (byte* expected8 = expected)
            {
                long* actual64 = (long*)actual8;
                long* expected64 = (long*)expected8;

                for (int i = 0; i < octets; i++)
                {
                    if (actual64[i] != expected64[i]) return false;
                }
                int index = count - spare;
                while (spare-- != 0)
                {
                    if (actual8[index] != expected8[index]) return false;
                }
            }
            return true;
        }

        internal bool AssertStarts(byte[] expected)
        {
            if (expected == null) throw new ArgumentNullException("expected");
            if (expected.Length > count) return false;
            var actual = arr as byte[];
            if (actual == null) return false;

            for (int i = 0; i < expected.Length; i++)
            {
                if (expected[i] != actual[offset + i]) return false;
            }
            return true;
        }
        internal byte[] GetBlob()
        {
            var src = (byte[])arr;
            if (src == null) return null;

            if (count == 0) return emptyBlob;

            byte[] copy = new byte[count];
            Buffer.BlockCopy(src, offset, copy, 0, count);
            return copy;
        }

        internal bool GetBoolean()
        {
            if (this.count != 1) throw new InvalidCastException();
            byte[] actual = arr as byte[];
            if (actual == null) throw new InvalidCastException();
            switch (actual[offset])
            {
                case (byte)'1': return true;
                case (byte)'0': return false;
                default: throw new InvalidCastException();
            }
        }

        internal RawResult[] GetItems()
        {
            return (RawResult[])arr;
        }

        internal RedisKey[] GetItemsAsKeys()
        {
            RawResult[] items = GetItems();
            if (items == null)
            {
                return null;
            }
            else if (items.Length == 0)
            {
                return RedisKey.EmptyArray;
            }
            else
            {
                var arr = new RedisKey[items.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = items[i].AsRedisKey();
                }
                return arr;
            }
        }

        internal RedisValue[] GetItemsAsValues()
        {
            RawResult[] items = GetItems();
            if (items == null)
            {
                return null;
            }
            else if (items.Length == 0)
            {
                return RedisValue.EmptyArray;
            }
            else
            {
                var arr = new RedisValue[items.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    arr[i] = items[i].AsRedisValue();
                }
                return arr;
            }
        }

        // returns an array of RawResults
        internal RawResult[] GetArrayOfRawResults()
        {
            if (arr == null)
            {
                return null;
            }
            else if (arr.Length == 0)
            {
                return new RawResult[0];
            }
            else
            {
                var rawResultArray = new RawResult[arr.Length];
                for (int i = 0; i < arr.Length; i++)
                {
                    var rawResult = (RawResult)arr.GetValue(i);
                    rawResultArray.SetValue(rawResult, i);
                }
                return rawResultArray;
            }
        }

        internal string GetString()
        {
            if (arr == null) return null;
            var blob = (byte[])arr;
            if (blob.Length == 0) return "";
            return Encoding.UTF8.GetString(blob, offset, count);
        }

        internal bool TryGetDouble(out double val)
        {
            if (arr == null)
            {
                val = 0;
                return false;
            }
            long i64;
            if (TryGetInt64(out i64))
            {
                val = i64;
                return true;
            }
            return Format.TryParseDouble(GetString(), out val);
        }

        internal bool TryGetInt64(out long value)
        {
            if (arr == null)
            {
                value = 0;
                return false;
            }
            return RedisValue.TryParseInt64(arr as byte[], offset, count, out value);
        }
    }
}


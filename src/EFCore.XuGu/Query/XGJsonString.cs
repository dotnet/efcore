// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;

// ReSharper disable once CheckNamespace
namespace Microsoft.EntityFrameworkCore
{
    /// <summary>
    /// This class can be used to represent a string that contains valid JSON data. When used in database queries
    /// against server implementations that support the `JSON` store type, a `CAST(xgJsonString as json)` clause will
    /// be generated where necessary.
    /// To mark a string as containing JSON data, just cast the string to `XGJsonString`.
    /// </summary>
    public sealed class XGJsonString : IConvertible
    {
        private readonly string _json;

        private XGJsonString(string json)
            => _json = json;

        public static implicit operator string(XGJsonString jsonStringObject)
            => jsonStringObject._json;

        public static implicit operator XGJsonString(string stringObject)
            => new XGJsonString(stringObject);

        public static bool operator ==(XGJsonString left, XGJsonString right)
            => left?.Equals(right) ?? ReferenceEquals(right, null);

        public static bool operator !=(XGJsonString left, XGJsonString right)
            => !(left == right);

        public static bool operator ==(XGJsonString left, string right)
            => left?.Equals(right) ?? ReferenceEquals(right, null);

        public static bool operator !=(XGJsonString left, string right)
            => !(left == right);

        private bool Equals(XGJsonString other)
            => _json == other._json;

        private bool Equals(string other)
            => _json == other;

        public override bool Equals(object obj)
            => ReferenceEquals(this, obj) ||
               obj is XGJsonString other && Equals(other) ||
               obj is string otherString && Equals(otherString);

        public override int GetHashCode()
            => HashCode.Combine(_json);

        public TypeCode GetTypeCode()
            => TypeCode.Object;

        public bool ToBoolean(IFormatProvider provider)
            => Convert.ToBoolean(_json, provider);

        public byte ToByte(IFormatProvider provider)
            => Convert.ToByte(_json, provider);

        public char ToChar(IFormatProvider provider)
            => Convert.ToChar(_json, provider);

        public DateTime ToDateTime(IFormatProvider provider)
            => Convert.ToDateTime(_json, provider);

        public decimal ToDecimal(IFormatProvider provider)
            => Convert.ToDecimal(_json, provider);

        public double ToDouble(IFormatProvider provider)
            => Convert.ToDouble(_json, provider);

        public short ToInt16(IFormatProvider provider)
            => Convert.ToInt16(_json, provider);

        public int ToInt32(IFormatProvider provider)
            => Convert.ToInt32(_json, provider);

        public long ToInt64(IFormatProvider provider)
            => Convert.ToInt64(_json, provider);

        public sbyte ToSByte(IFormatProvider provider)
            => Convert.ToSByte(_json, provider);

        public float ToSingle(IFormatProvider provider)
            => Convert.ToSingle(_json, provider);

        public string ToString(IFormatProvider provider)
            => Convert.ToString(_json, provider);

        public object ToType(Type conversionType, IFormatProvider provider)
            => Convert.ChangeType(_json, conversionType, provider);

        public ushort ToUInt16(IFormatProvider provider)
            => Convert.ToUInt16(_json, provider);

        public uint ToUInt32(IFormatProvider provider)
            => Convert.ToUInt32(_json, provider);

        public ulong ToUInt64(IFormatProvider provider)
            => Convert.ToUInt64(_json, provider);
    }
}

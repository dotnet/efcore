// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Globalization;
using System.Text;
using Microsoft.Data.Sqlite.Properties;
using static SQLitePCL.raw;

namespace Microsoft.Data.Sqlite
{
    internal abstract class SqliteValueReader
    {
        public abstract int FieldCount { get; }

        protected abstract int GetSqliteType(int ordinal);

        public virtual bool IsDBNull(int ordinal)
            => GetSqliteType(ordinal) == SQLITE_NULL;

        public virtual bool GetBoolean(int ordinal)
            => GetInt64(ordinal) != 0;

        public virtual byte GetByte(int ordinal)
            => checked((byte)GetInt64(ordinal));

        public virtual char GetChar(int ordinal)
        {
            var sqliteType = GetSqliteType(ordinal);
            if (sqliteType == SQLITE_TEXT)
            {
                var val = GetString(ordinal);
                if (val.Length == 1)
                {
                    return val[0];
                }
            }

            return checked((char)GetInt64(ordinal));
        }

        public virtual DateTime GetDateTime(int ordinal)
        {
            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case SQLITE_FLOAT:
                case SQLITE_INTEGER:
                    return FromJulianDate(GetDouble(ordinal));

                default:
                    return DateTime.Parse(GetString(ordinal), CultureInfo.InvariantCulture);
            }
        }

        public virtual DateTimeOffset GetDateTimeOffset(int ordinal)
        {
            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case SQLITE_FLOAT:
                case SQLITE_INTEGER:
                    return new DateTimeOffset(FromJulianDate(GetDouble(ordinal)));

                default:
                    return DateTimeOffset.Parse(GetString(ordinal), CultureInfo.InvariantCulture);
            }
        }

#if NET6_0_OR_GREATER
        public virtual DateOnly GetDateOnly(int ordinal)
        {
            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case SQLITE_FLOAT:
                case SQLITE_INTEGER:
                    return DateOnly.FromDateTime(FromJulianDate(GetDouble(ordinal)));

                default:
                    return DateOnly.Parse(GetString(ordinal), CultureInfo.InvariantCulture);
            }
        }

        public virtual TimeOnly GetTimeOnly(int ordinal)
            => TimeOnly.Parse(GetString(ordinal), CultureInfo.InvariantCulture);
#endif

        public virtual decimal GetDecimal(int ordinal)
            => decimal.Parse(GetString(ordinal), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);

        public virtual double GetDouble(int ordinal)
            => IsDBNull(ordinal)
                ? throw new InvalidOperationException(GetOnNullErrorMsg(ordinal))
                : GetDoubleCore(ordinal);

        protected abstract double GetDoubleCore(int ordinal);

        public virtual float GetFloat(int ordinal)
            => (float)GetDouble(ordinal);

        public virtual Guid GetGuid(int ordinal)
        {
            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case SQLITE_BLOB:
                    var bytes = GetBlob(ordinal)!;
                    return bytes.Length == 16
                        ? new Guid(bytes)
                        : new Guid(Encoding.UTF8.GetString(bytes, 0, bytes.Length));

                default:
                    return new Guid(GetString(ordinal));
            }
        }

        public virtual TimeSpan GetTimeSpan(int ordinal)
        {
            var sqliteType = GetSqliteType(ordinal);
            switch (sqliteType)
            {
                case SQLITE_FLOAT:
                case SQLITE_INTEGER:
                    return TimeSpan.FromDays(GetDouble(ordinal));
                default:
                    return TimeSpan.Parse(GetString(ordinal));
            }
        }

        public virtual short GetInt16(int ordinal)
            => checked((short)GetInt64(ordinal));

        public virtual int GetInt32(int ordinal)
            => checked((int)GetInt64(ordinal));

        public virtual long GetInt64(int ordinal)
            => IsDBNull(ordinal)
                ? throw new InvalidOperationException(GetOnNullErrorMsg(ordinal))
                : GetInt64Core(ordinal);

        protected abstract long GetInt64Core(int ordinal);

        public virtual string GetString(int ordinal)
            => IsDBNull(ordinal)
                ? throw new InvalidOperationException(GetOnNullErrorMsg(ordinal))
                : GetStringCore(ordinal);

        protected abstract string GetStringCore(int ordinal);

        public virtual T? GetFieldValue<T>(int ordinal)
        {
            // First do checks for value types of T; the JIT recognizes these and elides the rest of the code.
            if (typeof(T) == typeof(bool))
            {
                return (T)(object)GetBoolean(ordinal);
            }

            if (typeof(T) == typeof(byte))
            {
                return (T)(object)GetByte(ordinal);
            }

            if (typeof(T) == typeof(char))
            {
                return (T)(object)GetChar(ordinal);
            }

            if (typeof(T) == typeof(DateTime))
            {
                return (T)(object)GetDateTime(ordinal);
            }

            if (typeof(T) == typeof(DateTimeOffset))
            {
                return (T)(object)GetDateTimeOffset(ordinal);
            }

#if NET6_0_OR_GREATER
            if (typeof(T) == typeof(DateOnly))
            {
                return (T)(object)GetDateOnly(ordinal);
            }

            if (typeof(T) == typeof(TimeOnly))
            {
                return (T)(object)GetTimeOnly(ordinal);
            }
#endif

            if (typeof(T) == typeof(decimal))
            {
                return (T)(object)GetDecimal(ordinal);
            }

            if (typeof(T) == typeof(double))
            {
                return (T)(object)GetDouble(ordinal);
            }

            if (typeof(T) == typeof(float))
            {
                return (T)(object)GetFloat(ordinal);
            }

            if (typeof(T) == typeof(Guid))
            {
                return (T)(object)GetGuid(ordinal);
            }

            if (typeof(T) == typeof(int))
            {
                return (T)(object)GetInt32(ordinal);
            }

            if (typeof(T) == typeof(long))
            {
                return (T)(object)GetInt64(ordinal);
            }

            if (typeof(T) == typeof(sbyte))
            {
                return (T)(object)checked((sbyte)GetInt64(ordinal));
            }

            if (typeof(T) == typeof(short))
            {
                return (T)(object)GetInt16(ordinal);
            }

            if (typeof(T) == typeof(TimeSpan))
            {
                return (T)(object)GetTimeSpan(ordinal);
            }

            if (typeof(T) == typeof(uint))
            {
                return (T)(object)checked((uint)GetInt64(ordinal));
            }

            if (typeof(T) == typeof(ulong))
            {
                return (T)(object)((ulong)GetInt64(ordinal));
            }

            if (typeof(T) == typeof(ushort))
            {
                return (T)(object)checked((ushort)GetInt64(ordinal));
            }

            // None of the JIT-optimized value-type checks above succeeded.
            // Go into the "slow" path - from here the JIT actually emits code for the entire function.
            // Start with null and string as common cases, then handle nullable/enum types.

            if (IsDBNull(ordinal))
            {
                return default(T) is null ? GetNull<T>(ordinal) : throw new InvalidCastException();
            }

            if (typeof(T) == typeof(string))
            {
                return (T)(object)GetString(ordinal);
            }

            var type = typeof(T).UnwrapNullableType().UnwrapEnumType();
            if (type == typeof(bool))
            {
                return (T)(object)GetBoolean(ordinal);
            }

            if (type == typeof(byte))
            {
                return (T)(object)GetByte(ordinal);
            }

            if (type == typeof(byte[]))
            {
                return (T)(object)GetBlob(ordinal)!;
            }

            if (type == typeof(char))
            {
                return (T)(object)GetChar(ordinal);
            }

            if (type == typeof(DateTime))
            {
                return (T)(object)GetDateTime(ordinal);
            }

            if (type == typeof(DateTimeOffset))
            {
                return (T)(object)GetDateTimeOffset(ordinal);
            }

#if NET6_0_OR_GREATER
            if (type == typeof(DateOnly))
            {
                return (T)(object)GetDateOnly(ordinal);
            }

            if (type == typeof(TimeOnly))
            {
                return (T)(object)GetTimeOnly(ordinal);
            }
#endif

            if (type == typeof(decimal))
            {
                return (T)(object)GetDecimal(ordinal);
            }

            if (type == typeof(double))
            {
                return (T)(object)GetDouble(ordinal);
            }

            if (type == typeof(float))
            {
                return (T)(object)GetFloat(ordinal);
            }

            if (type == typeof(Guid))
            {
                return (T)(object)GetGuid(ordinal);
            }

            if (type == typeof(int))
            {
                return (T)(object)GetInt32(ordinal);
            }

            if (type == typeof(long))
            {
                return (T)(object)GetInt64(ordinal);
            }

            if (type == typeof(sbyte))
            {
                return (T)(object)checked((sbyte)GetInt64(ordinal));
            }

            if (type == typeof(short))
            {
                return (T)(object)GetInt16(ordinal);
            }

            if (type == typeof(TimeSpan))
            {
                return (T)(object)GetTimeSpan(ordinal);
            }

            if (type == typeof(uint))
            {
                return (T)(object)checked((uint)GetInt64(ordinal));
            }

            if (type == typeof(ulong))
            {
                return (T)(object)((ulong)GetInt64(ordinal));
            }

            if (type == typeof(ushort))
            {
                return (T)(object)checked((ushort)GetInt64(ordinal));
            }

            return (T)GetValue(ordinal)!;
        }

        public virtual object? GetValue(int ordinal)
            => GetSqliteType(ordinal) switch
            {
                SQLITE_INTEGER => GetInt64(ordinal),
                SQLITE_FLOAT => GetDouble(ordinal),
                SQLITE_TEXT => GetString(ordinal),
                SQLITE_NULL => GetNull<object>(ordinal),
                SQLITE_BLOB => GetBlob(ordinal),

                _ => throw new ArgumentOutOfRangeException("Unexpected column type: " + GetSqliteType(ordinal))
            };

        public virtual int GetValues(object?[] values)
        {
            int i;
            for (i = 0; i < FieldCount; i++)
            {
                values[i] = GetValue(i);
            }

            return i;
        }

        protected virtual byte[]? GetBlob(int ordinal)
            => IsDBNull(ordinal)
                ? GetNull<byte[]>(ordinal)
                : GetBlobCore(ordinal) ?? [];

        protected abstract byte[] GetBlobCore(int ordinal);

        protected virtual T? GetNull<T>(int ordinal)
            => typeof(T) == typeof(DBNull)
                ? (T)(object)DBNull.Value
                : default;

        protected virtual string GetOnNullErrorMsg(int ordinal)
            => Resources.CalledOnNullValue(ordinal);

        private static DateTime FromJulianDate(double julianDate)
        {
            // computeYMD
            var iJD = (long)(julianDate * 86400000.0 + 0.5);
            var Z = (int)((iJD + 43200000) / 86400000);
            var A = (int)((Z - 1867216.25) / 36524.25);
            A = Z + 1 + A - (A / 4);
            var B = A + 1524;
            var C = (int)((B - 122.1) / 365.25);
            var D = (36525 * (C & 32767)) / 100;
            var E = (int)((B - D) / 30.6001);
            var X1 = (int)(30.6001 * E);
            var day = B - D - X1;
            var month = E < 14 ? E - 1 : E - 13;
            var year = month > 2 ? C - 4716 : C - 4715;

            // computeHMS
            var s = (int)((iJD + 43200000) % 86400000);
            var fracSecond = s / 1000.0;
            s = (int)fracSecond;
            fracSecond -= s;
            var hour = s / 3600;
            s -= hour * 3600;
            var minute = s / 60;
            fracSecond += s - minute * 60;

            var second = (int)fracSecond;
            var millisecond = (int)Math.Round((fracSecond - second) * 1000.0);

            return new DateTime(year, month, day, hour, minute, second, millisecond);
        }
    }
}

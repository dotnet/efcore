// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using System.Text;
using SQLitePCL;
using Microsoft.Data.Sqlite.Properties;

namespace Microsoft.Data.Sqlite.Utilities
{
    /// <summary>
    /// Provides methods for converting values.
    /// </summary>
    internal static class ValueConversion
    {
        public static bool GetBoolean(Func<long> getInt64)
            => getInt64() != 0;

        public static byte GetByte(Func<long> getInt64)
            => (byte)getInt64();

        public static char GetChar(Func<long> getInt64)
            => (char)getInt64();

        public static DateTime GetDateTime(Func<int> getSqliteType, Func<double> getDouble, Func<string> getString)
        {
            var sqliteType = getSqliteType();
            switch (sqliteType)
            {
                case raw.SQLITE_FLOAT:
                case raw.SQLITE_INTEGER:
                    return FromJulianDate(getDouble());
                default:
                    return DateTime.Parse(getString(), CultureInfo.InvariantCulture);
            }
        }

        public static DateTimeOffset GetDateTimeOffset(Func<int> getSqliteType, Func<double> getDouble, Func<string> getString)
        {
            var sqliteType = getSqliteType();
            switch (sqliteType)
            {
                case raw.SQLITE_FLOAT:
                case raw.SQLITE_INTEGER:
                    return new DateTimeOffset(FromJulianDate(getDouble()));
                default:
                    return DateTimeOffset.Parse(getString(), CultureInfo.InvariantCulture);
            }
        }

        public static decimal GetDecimal(Func<string> getString)
            => decimal.Parse(getString(), NumberStyles.Number | NumberStyles.AllowExponent, CultureInfo.InvariantCulture);

        public static float GetFloat(Func<double> getDouble)
            => (float)getDouble();

        public static Guid GetGuid(Func<int> getSqliteType, Func<byte[]> getBlob, Func<string> getString)
        {
            var sqliteType = getSqliteType();
            switch (sqliteType)
            {
                case raw.SQLITE_BLOB:
                    var bytes = getBlob();
                    if (bytes.Length == 16)
                    {
                        return new Guid(bytes);
                    }
                    else
                    {
                        return new Guid(Encoding.UTF8.GetString(bytes, 0, bytes.Length));
                    }
                default:
                    return new Guid(getString());
            }
        }

        public static short GetInt16(Func<long> getInt64)
            => (short)getInt64();

        public static int GetInt32(Func<long> getInt64)
            => (int)getInt64();

        public static T GetValue<T>(
            Type type,
            Func<int> getSqliteType,
            Func<long> getInt64,
            Func<double> getDouble,
            Func<string> getString,
            Func<byte[]> getBlob,
            Func<T> getDefault)
        {
            if (type == typeof(bool))
            {
                return (T)(object)GetBoolean(getInt64);
            }
            if (type == typeof(byte))
            {
                return (T)(object)GetByte(getInt64);
            }
            if (type == typeof(byte[]))
            {
                return (T)(object)getBlob();
            }
            if (type == typeof(char))
            {
                return (T)(object)GetChar(getInt64);
            }
            if (type == typeof(DateTime))
            {
                return (T)(object)GetDateTime(getSqliteType, getDouble, getString);
            }
            if (type == typeof(DateTimeOffset))
            {
                return (T)(object)GetDateTimeOffset(getSqliteType, getDouble, getString);
            }
            if (type == typeof(decimal))
            {
                return (T)(object)GetDecimal(getString);
            }
            if (type == typeof(double))
            {
                return (T)(object)getDouble();
            }
            if (type == typeof(float))
            {
                return (T)(object)GetFloat(getDouble);
            }
            if (type == typeof(Guid))
            {
                return (T)(object)GetGuid(getSqliteType, getBlob, getString);
            }
            if (type == typeof(int))
            {
                return (T)(object)GetInt32(getInt64);
            }
            if (type == typeof(long))
            {
                return (T)(object)getInt64();
            }
            if (type == typeof(sbyte))
            {
                return (T)(object)((sbyte)getInt64());
            }
            if (type == typeof(short))
            {
                return (T)(object)GetInt16(getInt64);
            }
            if (type == typeof(string))
            {
                return (T)(object)getString();
            }
            if (type == typeof(TimeSpan))
            {
                return (T)(object)TimeSpan.Parse(getString());
            }
            if (type == typeof(uint))
            {
                return (T)(object)((uint)getInt64());
            }
            if (type == typeof(ulong))
            {
                return (T)(object)((ulong)getInt64());
            }
            if (type == typeof(ushort))
            {
                return (T)(object)((ushort)getInt64());
            }

            return getDefault();
        }

        /// <summary>
        /// Computes DateTime from julian date. This function is a port of the
        /// computeYMD and computeHMS functions from the original Sqlite core
        /// source code in 'date.c'.
        /// </summary>
        /// <param name="julianDate">Real value containing the julian date</param>
        /// <returns>The converted DateTime.</returns>
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

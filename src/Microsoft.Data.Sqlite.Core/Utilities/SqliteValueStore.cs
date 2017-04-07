// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Data.Sqlite.Properties;

namespace Microsoft.Data.Sqlite.Utilities
{
    /// <summary>
    /// Provides methods for converting values.
    /// </summary>
    internal abstract class SqliteValueStore
    {
        protected abstract void StoreInt64(long value);

        protected abstract void StoreDouble(double value);

        protected abstract void StoreString(string value);

        protected abstract void StoreBlob(byte[] value);

        protected abstract void StoreNull();

        public virtual void StoreValue(Type type, object val)
        {
            GetStoreValueAction(type, val).Invoke();
        }

        protected Action GetStoreValueAction(Type type, object val)
        {
            if (type == typeof(bool))
            {
                var value = (bool)val ? 1L : 0;
                return new Action(() => StoreInt64(value));
            }
            else if (type == typeof(byte))
            {
                var value = (long)(byte)val;
                return new Action(() => StoreInt64(value));
            }
            else if (type == typeof(byte[]))
            {
                var value = (byte[])val;
                return new Action(() => StoreBlob(value));
            }
            else if (type == typeof(char))
            {
                var value = (long)(char)val;
                return new Action(() => StoreInt64(value));
            }
            else if (type == typeof(DateTime))
            {
                var value = ((DateTime)val).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF");
                return new Action(() => StoreString(value));
            }
            else if (type == typeof(DateTimeOffset))
            {
                var value = ((DateTimeOffset)val).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz");
                return new Action(() => StoreString(value));
            }
            else if (type == typeof(DBNull))
            {
                return new Action(() => StoreNull());
            }
            else if (type == typeof(decimal))
            {
                var value = ((decimal)val).ToString("0.0###########################", CultureInfo.InvariantCulture); 
                return new Action(() => StoreString(value));
            }
            else if (type == typeof(double))
            {
                var value = (double)val;
                return new Action(() => StoreDouble(value));
            }
            else if (type == typeof(float))
            {
                var value = (double)(float)val;
                return new Action(() => StoreDouble(value));
            }
            else if (type == typeof(Guid))
            {
                var value = ((Guid)val).ToByteArray();
                return new Action(() => StoreBlob(value));
            }
            else if (type == typeof(int))
            {
                var value = (long)(int)val;
                return new Action(() => StoreInt64(value));
            }
            else if (type == typeof(long))
            {
                var value = (long)val;
                return new Action(() => StoreInt64(value));
            }
            else if (type == typeof(sbyte))
            {
                var value = (long)(sbyte)val;
                return new Action(() => StoreInt64(value));
            }
            else if (type == typeof(short))
            {
                var value = (long)(short)val;
                return new Action(() => StoreInt64(value));
            }
            else if (type == typeof(string))
            {
                var value = (string)val;
                return new Action(() => StoreString(value));
            }
            else if (type == typeof(TimeSpan))
            {
                var value = ((TimeSpan)val).ToString("c");
                return new Action(() => StoreString(value));
            }
            else if (type == typeof(uint))
            {
                var value = (long)(uint)val;
                return new Action(() => StoreInt64(value));
            }
            else if (type == typeof(ulong))
            {
                var value = (long)(ulong)val;
                return new Action(() => StoreInt64(value));
            }
            else if (type == typeof(ushort))
            {
                var value = (long)(ushort)val;
                return new Action(() => StoreInt64(value));
            }
            else
            {
                throw new InvalidOperationException(Resources.UnknownDataType(type));
            }
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Globalization;
using Microsoft.Data.Sqlite.Properties;

namespace Microsoft.Data.Sqlite
{
    // TODO: Make generic
    internal abstract class SqliteValueBinder
    {
        private readonly object _value;

        protected SqliteValueBinder(object value)
        {
            _value = value;
        }

        protected abstract void BindInt64(long value);

        protected virtual void BindDouble(double value)
        {
            if (double.IsNaN(value))
            {
                throw new InvalidOperationException(Resources.CannotStoreNaN);
            }

            BindDoubleCore(value);
        }

        protected abstract void BindDoubleCore(double value);

        protected abstract void BindText(string value);

        protected abstract void BindBlob(byte[] value);

        protected abstract void BindNull();

        public virtual void Bind()
        {
            if (_value == null)
            {
                BindNull();

                return;
            }

            var type = _value.GetType().UnwrapNullableType().UnwrapEnumType();
            if (type == typeof(bool))
            {
                var value = (bool)_value ? 1L : 0;
                BindInt64(value);
            }
            else if (type == typeof(byte))
            {
                var value = (long)(byte)_value;
                BindInt64(value);
            }
            else if (type == typeof(byte[]))
            {
                var value = (byte[])_value;
                BindBlob(value);
            }
            else if (type == typeof(char))
            {
                var value = (long)(char)_value;
                BindInt64(value);
            }
            else if (type == typeof(DateTime))
            {
                var value = ((DateTime)_value).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF");
                BindText(value);
            }
            else if (type == typeof(DateTimeOffset))
            {
                var value = ((DateTimeOffset)_value).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz");
                BindText(value);
            }
            else if (type == typeof(DBNull))
            {
                BindNull();
            }
            else if (type == typeof(decimal))
            {
                var value = ((decimal)_value).ToString("0.0###########################", CultureInfo.InvariantCulture);
                BindText(value);
            }
            else if (type == typeof(double))
            {
                var value = (double)_value;
                BindDouble(value);
            }
            else if (type == typeof(float))
            {
                var value = (double)(float)_value;
                BindDouble(value);
            }
            else if (type == typeof(Guid))
            {
                var value = ((Guid)_value).ToByteArray();
                BindBlob(value);
            }
            else if (type == typeof(int))
            {
                var value = (long)(int)_value;
                BindInt64(value);
            }
            else if (type == typeof(long))
            {
                var value = (long)_value;
                BindInt64(value);
            }
            else if (type == typeof(sbyte))
            {
                var value = (long)(sbyte)_value;
                BindInt64(value);
            }
            else if (type == typeof(short))
            {
                var value = (long)(short)_value;
                BindInt64(value);
            }
            else if (type == typeof(string))
            {
                var value = (string)_value;
                BindText(value);
            }
            else if (type == typeof(TimeSpan))
            {
                var value = ((TimeSpan)_value).ToString("c");
                BindText(value);
            }
            else if (type == typeof(uint))
            {
                var value = (long)(uint)_value;
                BindInt64(value);
            }
            else if (type == typeof(ulong))
            {
                var value = (long)(ulong)_value;
                BindInt64(value);
            }
            else if (type == typeof(ushort))
            {
                var value = (long)(ushort)_value;
                BindInt64(value);
            }
            else
            {
                throw new InvalidOperationException(Resources.UnknownDataType(type));
            }
        }
    }
}

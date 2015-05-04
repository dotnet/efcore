// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using Microsoft.Data.Entity.Relational;

namespace Microsoft.Data.Entity.Sqlite
{
    public class SqliteTypeMapper : RelationalTypeMapper
    {
        private static readonly RelationalTypeMapping _integer = new RelationalTypeMapping("INTEGER", DbType.Int64);
        private static readonly RelationalTypeMapping _real = new RelationalTypeMapping("REAL", DbType.Double);
        private static readonly RelationalTypeMapping _blob = new RelationalTypeMapping("BLOB", DbType.Binary);
        private static readonly RelationalTypeMapping _text = new RelationalTypeMapping("TEXT", DbType.String);

        public override RelationalTypeMapping GetTypeMapping(
            string specifiedType,
            string storageName,
            Type propertyType,
            bool isKey,
            bool isConcurrencyToken)
        {
            propertyType = propertyType.UnwrapNullableType().UnwrapEnumType();

            if (propertyType == typeof(bool)
                || propertyType == typeof(byte)
                || propertyType == typeof(char)
                || propertyType == typeof(int)
                || propertyType == typeof(long)
                || propertyType == typeof(sbyte)
                || propertyType == typeof(short)
                || propertyType == typeof(uint)
                || propertyType == typeof(ulong)
                || propertyType == typeof(ushort))
            {
                return _integer;
            }
            else if (propertyType == typeof(byte[])
                || propertyType == typeof(Guid))
            {
                return _blob;
            }
            else if (propertyType == typeof(DateTime)
                || propertyType == typeof(DateTimeOffset)
                || propertyType == typeof(decimal)
                || propertyType == typeof(TimeSpan)
                || propertyType == typeof(string))
            {
                return _text;
            }
            else if (propertyType == typeof(double)
                || propertyType == typeof(float))
            {
                return _real;
            }

            throw new NotSupportedException(Strings.UnsupportedType(storageName, propertyType.Name));
        }
    }
}

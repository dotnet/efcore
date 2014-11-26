// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Linq;
using Microsoft.Data.Entity.Relational;

namespace Microsoft.Data.Entity.SqlServer
{
    public class SqlServerTypeMapper : RelationalTypeMapper
    {
        // This dictionary is for invariant mappings from a sealed CLR type to a single
        // store type. If the CLR type is unsealed or if the mapping varies based on how the
        // type is used (e.g. in keys), then add custom mapping below.
        private readonly Tuple<Type, RelationalTypeMapping>[] _simpleMappings =
            {
                Tuple.Create(typeof(int), new RelationalTypeMapping("int", DbType.Int32)),
                Tuple.Create(typeof(DateTime), new RelationalTypeMapping("datetime2", DbType.DateTime2)),
                Tuple.Create(typeof(Guid), new RelationalTypeMapping("uniqueidentifier", DbType.Guid)),
                Tuple.Create(typeof(bool), new RelationalTypeMapping("bit", DbType.Boolean)),
                Tuple.Create(typeof(byte), new RelationalTypeMapping("tinyint", DbType.Byte)),
                Tuple.Create(typeof(double), new RelationalTypeMapping("float", DbType.Double)),
                Tuple.Create(typeof(DateTimeOffset), new RelationalTypeMapping("datetimeoffset", DbType.DateTimeOffset)),
                Tuple.Create(typeof(char), new RelationalTypeMapping("int", DbType.Int32)),
                Tuple.Create(typeof(sbyte), new RelationalTypeMapping("smallint", DbType.SByte)),
                Tuple.Create(typeof(ushort), new RelationalTypeMapping("int", DbType.UInt16)),
                Tuple.Create(typeof(uint), new RelationalTypeMapping("bigint", DbType.UInt32)),
                Tuple.Create(typeof(ulong), new RelationalTypeMapping("numeric(20, 0)", DbType.UInt64))
            };

        private readonly RelationalTypeMapping _nonKeyStringMapping
            = new RelationalTypeMapping("nvarchar(max)", DbType.String);

        private readonly RelationalTypeMapping _keyStringMapping
            = new RelationalSizedTypeMapping("nvarchar(450)", DbType.String, 900);

        private readonly RelationalTypeMapping _nonKeyByteArrayMapping
            = new RelationalTypeMapping("varbinary(max)", DbType.Binary);

        private readonly RelationalTypeMapping _keyByteArrayMapping
            = new RelationalSizedTypeMapping("varbinary(900)", DbType.Binary, 900);

        private readonly RelationalTypeMapping _rowVersionMapping
            = new RelationalSizedTypeMapping("rowversion", DbType.Binary, 8);

        public override RelationalTypeMapping GetTypeMapping(
            string specifiedType, string storageName, Type propertyType, bool isKey, bool isConcurrencyToken)
        {
            propertyType = propertyType.UnwrapNullableType();

            var mapping = _simpleMappings.FirstOrDefault(m => m.Item1 == propertyType);
            if (mapping != null)
            {
                return mapping.Item2;
            }

            if (propertyType == typeof(string))
            {
                if (isKey)
                {
                    return _keyStringMapping;
                }
                return _nonKeyStringMapping;
            }

            if (propertyType == typeof(byte[]))
            {
                if (isKey)
                {
                    return _keyByteArrayMapping;
                }

                if (isConcurrencyToken)
                {
                    return _rowVersionMapping;
                }

                return _nonKeyByteArrayMapping;
            }

            return base.GetTypeMapping(specifiedType, storageName, propertyType, isKey, isConcurrencyToken);
        }
    }
}

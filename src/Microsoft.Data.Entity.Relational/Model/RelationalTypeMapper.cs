// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;

namespace Microsoft.Data.Entity.Relational.Model
{
    // TODO: Some of this is SQL Server specific and should be moved into that class
    public class RelationalTypeMapper
    {
        // This dictionary is for invariant mappings from a sealed CLR type to a single
        // store type. If the CLR type is unsealed or if the mapping varies based on how the
        // type is used (e.g. in keys), then add custom mapping below.
        private readonly IDictionary<Type, RelationalTypeMapping> _simpleMappings = new Dictionary<Type, RelationalTypeMapping>()
            {
                { typeof(int), new RelationalTypeMapping("int", DbType.Int32) },
                { typeof(DateTime), new RelationalTypeMapping("datetime2", DbType.DateTime2) },
                { typeof(Guid), new RelationalTypeMapping("uniqueidentifier", DbType.Guid) },
                { typeof(bool), new RelationalTypeMapping("bit", DbType.Boolean) },
                { typeof(byte), new RelationalTypeMapping("tinyint", DbType.Byte) },
                { typeof(char), new RelationalTypeMapping("int", DbType.Int32) },
                { typeof(double), new RelationalTypeMapping("float", DbType.Double) },
                { typeof(short), new RelationalTypeMapping("smallint", DbType.Int16) },
                { typeof(long), new RelationalTypeMapping("bigint", DbType.Int64) },
                { typeof(sbyte), new RelationalTypeMapping("smallint", DbType.SByte) },
                { typeof(float), new RelationalTypeMapping("real", DbType.Single) },
                { typeof(ushort), new RelationalTypeMapping("int", DbType.UInt16) },
                { typeof(uint), new RelationalTypeMapping("bigint", DbType.UInt32) },
                { typeof(ulong), new RelationalTypeMapping("numeric(20, 0)", DbType.UInt64) },
                { typeof(DateTimeOffset), new RelationalTypeMapping("datetimeoffset", DbType.DateTimeOffset) },
            };

        private readonly RelationalTypeMapping _nonKeyStringMapping
            = new RelationalTypeMapping("nvarchar(max)", DbType.String);

        // TODO: It may be possible to increase 128 to 900, at least for SQL Server
        private readonly RelationalTypeMapping _keyStringMapping
            = new RelationalSizedTypeMapping("nvarchar(128)", DbType.String, 128);

        private readonly RelationalTypeMapping _nonKeyByteArrayMapping
            = new RelationalTypeMapping("varbinary(max)", DbType.Binary);

        // TODO: It may be possible to increase 128 to 900, at least for SQL Server
        private readonly RelationalTypeMapping _keyByteArrayMapping
            = new RelationalSizedTypeMapping("varbinary(128)", DbType.Binary, 128);

        private readonly RelationalTypeMapping _rowVersionMapping
            = new RelationalSizedTypeMapping("rowversion", DbType.Binary, 8);

        private readonly RelationalDecimalTypeMapping _decimalMapping
            = new RelationalDecimalTypeMapping(18, 2);

        // TODO: It would be nice to just pass IProperty into this method, but Migrations uses its own
        // store model for which there is no easy way to get an IProperty.
        public virtual RelationalTypeMapping GetTypeMapping(
            [CanBeNull] string specifiedType,
            [NotNull] string storageName,
            [NotNull] Type propertyType,
            bool isKey,
            bool isConcurrencyToken)
        {
            Check.NotNull(storageName, "storageName");
            Check.NotNull(propertyType, "propertyType");

            // TODO: if specifiedType is non-null then parse it to create a type mapping

            RelationalTypeMapping mapping;
            if (_simpleMappings.TryGetValue(propertyType, out mapping))
            {
                return mapping;
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

            if (propertyType == typeof(decimal))
            {
                // TODO: If scale/precision have been configured for the property, then create parameter appropriately
                return _decimalMapping;
            }

            //// TODO: Consider TimeSpan mapping

            throw new NotSupportedException(Strings.FormatUnsupportedType(storageName, propertyType.Name));
        }
    }
}

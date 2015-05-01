// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Data;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public class RelationalTypeMapper : IRelationalTypeMapper
    {
        // This table is for invariant mappings from a sealed CLR type to a single
        // store type. If the CLR type is unsealed or if the mapping varies based on how the
        // type is used (e.g. in keys), then add custom mapping below.
        private readonly Tuple<Type, RelationalTypeMapping>[] _simpleMappings =
            {
                Tuple.Create(typeof(int), new RelationalTypeMapping("integer", DbType.Int32)),
                Tuple.Create(typeof(DateTime), new RelationalTypeMapping("timestamp", DbType.DateTime)),
                Tuple.Create(typeof(bool), new RelationalTypeMapping("boolean", DbType.Boolean)),
                Tuple.Create(typeof(double), new RelationalTypeMapping("double precision", DbType.Double)),
                Tuple.Create(typeof(long), new RelationalTypeMapping("bigint", DbType.Int64)),
                Tuple.Create(typeof(DateTimeOffset), new RelationalTypeMapping("timestamp with time zone", DbType.DateTimeOffset)),
                Tuple.Create(typeof(short), new RelationalTypeMapping("smallint", DbType.Int16)),
                Tuple.Create(typeof(float), new RelationalTypeMapping("real", DbType.Single))
            };

        private readonly RelationalSizedTypeMapping _stringMapping
            = new RelationalSizedTypeMapping("varchar(4000)", DbType.AnsiString, 4000);

        private readonly RelationalDecimalTypeMapping _decimalMapping = new RelationalDecimalTypeMapping(18, 2);

        public virtual RelationalTypeMapping GetTypeMapping(IProperty property)
            => GetTypeMapping(
                property.Relational().ColumnType,
                property.Relational().Column,
                property.ClrType.UnwrapNullableType(),
                property.IsKey() || property.IsForeignKey(),
                property.IsConcurrencyToken);

        public virtual RelationalTypeMapping GetTypeMapping(ISequence sequence)
            => GetTypeMapping(
                /*specifiedType:*/ null,
                sequence.Name,
                sequence.Type,
                isKey: false,
                isConcurrencyToken: false);

        // TODO: It would be nice to just pass IProperty into this method, but Migrations uses its own
        // store model for which there is no easy way to get an IProperty.
        // Issue #769
        public virtual RelationalTypeMapping GetTypeMapping(
            string specifiedType,
            string storageName,
            Type propertyType,
            bool isKey,
            bool isConcurrencyToken)
        {
            Check.NotNull(storageName, nameof(storageName));
            Check.NotNull(propertyType, nameof(propertyType));

            // TODO: if specifiedType is non-null then parse it to create a type mapping
            // TODO: Consider allowing Code First to specify an actual type mapping instead of just the string
            // Issue #587
            // type since that would remove the need to parse the string.

            propertyType = propertyType.UnwrapNullableType();

            var mapping = _simpleMappings.FirstOrDefault(m => m.Item1 == propertyType);
            if (mapping != null)
            {
                return mapping.Item2;
            }

            if (propertyType.GetTypeInfo().IsEnum)
            {
                return GetTypeMapping(specifiedType, storageName, Enum.GetUnderlyingType(propertyType), isKey, isConcurrencyToken);
            }

            if (propertyType == typeof(decimal))
            {
                // TODO: If scale/precision have been configured for the property, then create parameter appropriately
                // Issue #587
                return _decimalMapping;
            }

            if (propertyType == typeof(string))
            {
                // TODO: Honor length if configured; fallthrough if unbounded
                return _stringMapping;
            }

            // TODO: Consider TimeSpan mapping
            // Issue #770

            throw new NotSupportedException(Strings.UnsupportedType(storageName, propertyType.Name));
        }
    }
}

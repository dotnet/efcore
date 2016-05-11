// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public abstract class RelationalTypeMapper : IRelationalTypeMapper
    {
        private readonly ConcurrentDictionary<string, RelationalTypeMapping> _explicitMappings
            = new ConcurrentDictionary<string, RelationalTypeMapping>();

        protected abstract IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings();

        protected abstract IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings();

        // Not using IRelationalAnnotationProvider here because type mappers are Singletons
        protected abstract string GetColumnType([NotNull] IProperty property);

        public virtual void ValidateTypeName(string storeType)
        {
        }

        public virtual RelationalTypeMapping FindMapping(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var storeType = GetColumnType(property);

            return (storeType != null ? FindMapping(storeType) : null)
                   ?? FindCustomMapping(property)
                   ?? FindMapping(property.ClrType);
        }

        public virtual RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            RelationalTypeMapping mapping;
            return GetClrTypeMappings().TryGetValue(clrType.UnwrapNullableType().UnwrapEnumType(), out mapping)
                ? mapping
                : null;
        }

        public virtual RelationalTypeMapping FindMapping(string storeType)
        {
            Check.NotNull(storeType, nameof(storeType));

            return _explicitMappings.GetOrAdd(storeType, CreateMappingFromStoreType);
        }

        protected virtual RelationalTypeMapping CreateMappingFromStoreType([NotNull] string storeType)
        {
            Check.NotNull(storeType, nameof(storeType));

            RelationalTypeMapping mapping;
            if (GetStoreTypeMappings().TryGetValue(storeType, out mapping)
                && mapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
            {
                return mapping;
            }

            var openParen = storeType.IndexOf("(", StringComparison.Ordinal);
            if (openParen > 0)
            {
                if (!GetStoreTypeMappings().TryGetValue(storeType.Substring(0, openParen), out mapping))
                {
                    return null;
                }

                var closeParen = storeType.IndexOf(")", openParen + 1, StringComparison.Ordinal);
                int size;
                if (closeParen > openParen
                    && int.TryParse(storeType.Substring(openParen + 1, closeParen - openParen - 1), out size)
                    && mapping.Size != size)
                {
                    return mapping.CreateCopy(storeType, size);
                }
            }

            return mapping?.CreateCopy(storeType, mapping.Size);
        }

        protected virtual RelationalTypeMapping FindCustomMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var clrType = property.ClrType.UnwrapNullableType();

            return clrType == typeof(string)
                ? GetStringMapping(property)
                : clrType == typeof(byte[])
                    ? GetByteArrayMapping(property)
                    : null;
        }

        public virtual IByteArrayRelationalTypeMapper ByteArrayMapper => null;

        public virtual IStringRelationalTypeMapper StringMapper => null;

        protected virtual RelationalTypeMapping GetStringMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            // TODO: Use unicode-ness defined in property metadata
            return StringMapper?.FindMapping(
                true,
                RequiresKeyMapping(property),
                property.GetMaxLength());
        }

        protected virtual RelationalTypeMapping GetByteArrayMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return ByteArrayMapper?.FindMapping(
                property.IsConcurrencyToken && property.ValueGenerated == ValueGenerated.OnAddOrUpdate,
                RequiresKeyMapping(property),
                property.GetMaxLength());
        }

        protected virtual bool RequiresKeyMapping([NotNull] IProperty property)
            => property.IsKey() || property.IsForeignKey();
    }
}

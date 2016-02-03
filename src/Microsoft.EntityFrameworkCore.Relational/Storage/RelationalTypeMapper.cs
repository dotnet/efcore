// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public abstract class RelationalTypeMapper : IRelationalTypeMapper
    {
        private readonly ConcurrentDictionary<int, RelationalTypeMapping> _boundedStringMappings
            = new ConcurrentDictionary<int, RelationalTypeMapping>();

        private readonly ConcurrentDictionary<int, RelationalTypeMapping> _boundedBinaryMappings
            = new ConcurrentDictionary<int, RelationalTypeMapping>();

        protected abstract IReadOnlyDictionary<Type, RelationalTypeMapping> GetSimpleMappings();

        protected abstract IReadOnlyDictionary<string, RelationalTypeMapping> GetSimpleNameMappings();

        // Not using IRelationalAnnotationProvider here because type mappers are Singletons
        protected abstract string GetColumnType([NotNull] IProperty property);

        public virtual RelationalTypeMapping FindMapping(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            RelationalTypeMapping mapping = null;

            var typeName = GetColumnType(property);
            if (typeName != null)
            {
                var paren = typeName.IndexOf("(", StringComparison.Ordinal);
                GetSimpleNameMappings().TryGetValue((paren >= 0 ? typeName.Substring(0, paren) : typeName).ToLowerInvariant(), out mapping);
            }

            return mapping
                   ?? FindCustomMapping(property)
                   ?? FindMapping(property.ClrType);
        }

        public virtual RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            RelationalTypeMapping mapping;

            return GetSimpleMappings().TryGetValue(clrType.UnwrapNullableType().UnwrapEnumType(), out mapping)
                ? mapping
                : null;
        }

        public virtual RelationalTypeMapping FindMapping(string typeName)
        {
            Check.NotNull(typeName, nameof(typeName));

            RelationalTypeMapping mapping;

            GetSimpleNameMappings().TryGetValue(typeName, out mapping);

            return mapping;
        }

        public virtual bool IsTypeMapped(Type clrType) => FindMapping(clrType) != null;

        protected virtual RelationalTypeMapping FindCustomMapping([NotNull] IProperty property) => null;

        protected virtual RelationalTypeMapping GetCustomMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var mapping = FindCustomMapping(property);

            if (mapping != null)
            {
                return mapping;
            }

            throw new NotSupportedException(RelationalStrings.UnsupportedType(property.ClrType.Name));
        }

        protected virtual bool RequiresKeyMapping([NotNull] IProperty property)
            => property.IsKey() || property.IsForeignKey();

        protected virtual RelationalTypeMapping GetStringMapping(
            [NotNull] IProperty property,
            int maxBoundedLength,
            [NotNull] Func<int, RelationalTypeMapping> boundedMapping,
            [NotNull] RelationalTypeMapping unboundedMapping,
            [NotNull] RelationalTypeMapping defaultMapping,
            [CanBeNull] RelationalTypeMapping keyMapping = null)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(defaultMapping, nameof(defaultMapping));

            var maxLength = property.GetMaxLength();

            return maxLength.HasValue
                ? maxLength <= maxBoundedLength
                    ? _boundedStringMappings.GetOrAdd(maxLength.Value, boundedMapping)
                    : unboundedMapping
                : ((keyMapping != null)
                   && RequiresKeyMapping(property)
                    ? keyMapping
                    : defaultMapping);
        }

        protected virtual RelationalTypeMapping GetByteArrayMapping(
            [NotNull] IProperty property,
            int maxBoundedLength,
            [NotNull] Func<int, RelationalTypeMapping> boundedMapping,
            [NotNull] RelationalTypeMapping unboundedMapping,
            [NotNull] RelationalTypeMapping defaultMapping,
            [CanBeNull] RelationalTypeMapping keyMapping = null,
            [CanBeNull] RelationalTypeMapping rowVersionMapping = null)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(defaultMapping, nameof(defaultMapping));

            if (property.IsConcurrencyToken
                && (property.ValueGenerated == ValueGenerated.OnAddOrUpdate)
                && (rowVersionMapping != null))
            {
                return rowVersionMapping;
            }

            var maxLength = property.GetMaxLength();

            return maxLength.HasValue
                ? maxLength <= maxBoundedLength
                    ? _boundedBinaryMappings.GetOrAdd(maxLength.Value, boundedMapping)
                    : unboundedMapping
                : ((keyMapping != null)
                   && RequiresKeyMapping(property)
                    ? keyMapping
                    : defaultMapping);
        }
    }
}

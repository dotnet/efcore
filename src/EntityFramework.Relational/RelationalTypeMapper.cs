// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Relational.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Relational
{
    public abstract class RelationalTypeMapper : IRelationalTypeMapper
    {
        private readonly ThreadSafeDictionaryCache<string, RelationalTypeMapping> _nameMappings
            = new ThreadSafeDictionaryCache<string, RelationalTypeMapping>();

        protected abstract IReadOnlyDictionary<Type, RelationalTypeMapping> SimpleMappings { get; }

        protected abstract IReadOnlyDictionary<string, RelationalTypeMapping> SimpleNameMappings { get; }

        public virtual RelationalTypeMapping MapPropertyType(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            RelationalTypeMapping mapping = null;

            var typeName = property.Relational().ColumnType;
            if (typeName != null)
            {
                mapping = GetOrAddNameMapping(typeName.ToLowerInvariant());
            }

            return mapping
                   ?? (SimpleMappings.TryGetValue(property.ClrType.UnwrapEnumType().UnwrapNullableType(), out mapping)
                       ? mapping
                       : MapCustom(property));
        }

        public virtual RelationalTypeMapping MapSequenceType(ISequence sequence)
        {
            Check.NotNull(sequence, nameof(sequence));

            RelationalTypeMapping mapping;
            if (SimpleMappings.TryGetValue(sequence.Type.UnwrapEnumType(), out mapping))
            {
                return mapping;
            }

            throw new NotSupportedException(Strings.UnsupportedType(sequence.Type.Name));
        }

        protected virtual RelationalTypeMapping TryMapFromName(
            [NotNull] string typeName,
            [NotNull] string typeNamePrefix,
            int? firstQualifier,
            int? secondQualifier)
        {
            Check.NotEmpty(typeName, nameof(typeName));
            Check.NotEmpty(typeNamePrefix, nameof(typeNamePrefix));

            // TODO: Log warning for unrecognized/supported store type.

            return null;
        }

        protected virtual RelationalTypeMapping MapCustom([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            throw new NotSupportedException(Strings.UnsupportedType(property.ClrType.Name));
        }

        protected virtual RelationalTypeMapping MapString(
            [NotNull] IProperty property,
            [NotNull] string sizedTypeName,
            [NotNull] RelationalTypeMapping defaultMapping,
            [CanBeNull] RelationalTypeMapping keyMapping = null)
        {
            Check.NotNull(property, nameof(property));
            Check.NotEmpty(sizedTypeName, nameof(sizedTypeName));
            Check.NotNull(defaultMapping, nameof(defaultMapping));

            var maxLength = property.GetMaxLength();

            return maxLength.HasValue
                ? GetOrAddNameMapping(sizedTypeName + "(" + maxLength + ")")
                : (keyMapping != null
                   && (property.IsKey() || property.IsForeignKey())
                    ? keyMapping
                    : defaultMapping);
        }

        protected virtual RelationalTypeMapping MapByteArray(
            [NotNull] IProperty property,
            [NotNull] string sizedTypeName,
            [NotNull] RelationalTypeMapping defaultMapping,
            [CanBeNull] RelationalTypeMapping keyMapping = null,
            [CanBeNull] RelationalTypeMapping rowVersionMapping = null)
        {
            Check.NotNull(property, nameof(property));
            Check.NotEmpty(sizedTypeName, nameof(sizedTypeName));
            Check.NotNull(defaultMapping, nameof(defaultMapping));

            if (property.IsConcurrencyToken
                && rowVersionMapping != null)
            {
                return rowVersionMapping;
            }

            var maxLength = property.GetMaxLength();

            return maxLength.HasValue
                ? GetOrAddNameMapping(sizedTypeName + "(" + maxLength + ")")
                : (keyMapping != null
                   && (property.IsKey() || property.IsForeignKey())
                    ? keyMapping
                    : defaultMapping);
        }

        protected virtual RelationalTypeMapping TryMapSized(
            [NotNull] string typeName,
            [NotNull] string typeNamePrefix,
            [NotNull] IReadOnlyList<string> toMatch,
            int? firstQualifier,
            DbType? storeType = null)
        {
            Check.NotEmpty(typeNamePrefix, nameof(typeNamePrefix));
            Check.NotNull(toMatch, nameof(toMatch));

            return firstQualifier != null
                   && toMatch.Contains(typeNamePrefix)
                ? new RelationalSizedTypeMapping(typeName, storeType, (int)firstQualifier)
                : null;
        }

        protected virtual RelationalTypeMapping TryMapScaled(
            [NotNull] string typeName,
            [NotNull] string typeNamePrefix,
            [NotNull] IReadOnlyList<string> toMatch,
            int? firstQualifier,
            int? secondQualifier,
            DbType? storeType = null)
        {
            Check.NotEmpty(typeNamePrefix, nameof(typeNamePrefix));
            Check.NotNull(toMatch, nameof(toMatch));

            return firstQualifier != null
                   && toMatch.Contains(typeNamePrefix)
                ? (secondQualifier == null
                    ? new RelationalScaledTypeMapping(typeName, storeType, (byte)firstQualifier)
                    : new RelationalScaledTypeMapping(typeName, storeType, (byte)firstQualifier, (byte)secondQualifier))
                : null;
        }

        private RelationalTypeMapping GetOrAddNameMapping(string typeName)
        {
            RelationalTypeMapping mapping;
            return SimpleNameMappings.TryGetValue(typeName, out mapping)
                ? mapping
                : _nameMappings.GetOrAdd(typeName, MapFromName);
        }

        private RelationalTypeMapping MapFromName(string typeName)
        {
            var pos1 = typeName.IndexOf("(", StringComparison.Ordinal) + 1;
            var pos2 = typeName.IndexOf(",", StringComparison.Ordinal);
            var pos3 = typeName.IndexOf(")", StringComparison.Ordinal);

            if (pos1 > 0
                && pos3 > pos1)
            {
                int? firstQualifier;
                int? secondQualifier = null;

                if (pos2 > 0)
                {
                    firstQualifier = TryParse(typeName, pos1, pos2);
                    secondQualifier = TryParse(typeName, pos2 + 1, pos3);
                }
                else
                {
                    firstQualifier = TryParse(typeName, pos1, pos3);
                }

                return TryMapFromName(
                    typeName,
                    typeName.Substring(0, pos1 - 1).Trim(),
                    firstQualifier,
                    secondQualifier);
            }

            return TryMapFromName(typeName, typeName, null, null);
        }

        private static int? TryParse(string stringValue, int start, int end)
        {
            int intValue;
            return int.TryParse(stringValue.Substring(start, end - start), out intValue)
                ? (int?)intValue
                : null;
        }
    }
}

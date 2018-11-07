// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FallbackRelationalTypeMappingSource : RelationalTypeMappingSource
    {
#pragma warning disable 618
        private readonly IRelationalTypeMapper _relationalTypeMapper;
#pragma warning restore 618

        [ThreadStatic]
        private static IProperty _property;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public FallbackRelationalTypeMappingSource(
            [NotNull] TypeMappingSourceDependencies dependencies,
            [NotNull] RelationalTypeMappingSourceDependencies relationalDependencies,
#pragma warning disable 618
            [NotNull] IRelationalTypeMapper typeMapper)
#pragma warning restore 618
            : base(dependencies, relationalDependencies)
        {
            _relationalTypeMapper = typeMapper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping FindMappingWithConversion(
            in RelationalTypeMappingInfo mappingInfo,
            IReadOnlyList<IProperty> principals)
        {
            _property = principals?[0];

            return base.FindMappingWithConversion(mappingInfo, principals);
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping FindMapping(in RelationalTypeMappingInfo mappingInfo)
        {
            var mapping = FilterByClrType(FindMappingForProperty(mappingInfo), mappingInfo)
                          ?? FilterByClrType(FindMappingForStoreTypeName(mappingInfo), mappingInfo)
                          ?? FilterByClrType(FindMappingForClrType(mappingInfo), mappingInfo);

            if (mapping != null
                && (mappingInfo.Precision != null
                    || mappingInfo.Scale != null))
            {
                var newStoreName = mapping.StoreType;
                var openParen = newStoreName.IndexOf("(", StringComparison.Ordinal);
                if (openParen > 0)
                {
                    newStoreName = mapping.StoreType.Substring(0, openParen);
                }

                newStoreName += mappingInfo.Precision != null
                                && mappingInfo.Scale != null
                    ? "(" + mappingInfo.Precision + "," + mappingInfo.Scale + ")"
                    : "(" + (mappingInfo.Precision ?? mappingInfo.Scale) + ")";

                mapping = mapping.Clone(newStoreName, mapping.Size);
            }

            return mapping ?? base.FindMapping(mappingInfo);
        }

        private RelationalTypeMapping FindMappingForProperty(in RelationalTypeMappingInfo mappingInfo)
            => _property != null
                ? _relationalTypeMapper.FindMapping(_property)
                : null;

        private RelationalTypeMapping FindMappingForClrType(in RelationalTypeMappingInfo mappingInfo)
        {
            if (mappingInfo.ClrType == null
                || (mappingInfo.StoreTypeName != null
                    && _relationalTypeMapper.FindMapping(mappingInfo.StoreTypeName) != null))
            {
                return null;
            }

            if (mappingInfo.ClrType == typeof(string)
                && _relationalTypeMapper.StringMapper != null)
            {
                return _relationalTypeMapper.StringMapper.FindMapping(
                    mappingInfo.IsUnicode != false,
                    mappingInfo.IsKeyOrIndex,
                    mappingInfo.Size);
            }

            return mappingInfo.ClrType == typeof(byte[])
                && _relationalTypeMapper.ByteArrayMapper != null
                ? _relationalTypeMapper.ByteArrayMapper.FindMapping(
                    mappingInfo.IsRowVersion == true,
                    mappingInfo.IsKeyOrIndex,
                    mappingInfo.Size)
                : _relationalTypeMapper.FindMapping(mappingInfo.ClrType);
        }

        private RelationalTypeMapping FindMappingForStoreTypeName(in RelationalTypeMappingInfo mappingInfo)
        {
            if (mappingInfo.StoreTypeName != null)
            {
                _relationalTypeMapper.ValidateTypeName(mappingInfo.StoreTypeName);

                return _relationalTypeMapper.FindMapping(mappingInfo.StoreTypeName);
            }

            return null;
        }

        private static RelationalTypeMapping FilterByClrType(RelationalTypeMapping mapping, in RelationalTypeMappingInfo mappingInfo)
            => mapping != null
               && (mappingInfo.ClrType == null
                   || mappingInfo.ClrType == mapping.ClrType)
                ? mapping
                : null;
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class FallbackRelationalCoreTypeMapper : RelationalCoreTypeMapperBase
    {
        private readonly IRelationalTypeMapper _relationalTypeMapper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public FallbackRelationalCoreTypeMapper(
            [NotNull] CoreTypeMapperDependencies dependencies,
            [NotNull] RelationalTypeMapperDependencies relationalDependencies,
            [NotNull] IRelationalTypeMapper typeMapper)
            : base(dependencies, relationalDependencies)
        {
            _relationalTypeMapper = typeMapper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected override RelationalTypeMapping FindMapping(RelationalTypeMappingInfo mappingInfo)
        {
            Check.NotNull(mappingInfo, nameof(mappingInfo));

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

            return mapping;
        }

        private RelationalTypeMapping FindMappingForProperty(RelationalTypeMappingInfo mappingInfo)
            => mappingInfo.Property != null
                ? _relationalTypeMapper.FindMapping(mappingInfo.Property)
                : null;

        private RelationalTypeMapping FindMappingForClrType(RelationalTypeMappingInfo mappingInfo)
        {
            if (mappingInfo.TargetClrType == null
                || (mappingInfo.StoreTypeName != null
                    && _relationalTypeMapper.FindMapping(mappingInfo.StoreTypeName) != null))
            {
                return null;
            }

            if (mappingInfo.TargetClrType == typeof(string)
                && _relationalTypeMapper.StringMapper != null)
            {
                return _relationalTypeMapper.StringMapper.FindMapping(
                    mappingInfo.IsUnicode != false,
                    mappingInfo.IsKeyOrIndex,
                    mappingInfo.Size);
            }

            if (mappingInfo.TargetClrType == typeof(byte[])
                && _relationalTypeMapper.ByteArrayMapper != null)
            {
                return _relationalTypeMapper.ByteArrayMapper.FindMapping(
                    mappingInfo.IsRowVersion == true,
                    mappingInfo.IsKeyOrIndex,
                    mappingInfo.Size);
            }

            return _relationalTypeMapper.FindMapping(mappingInfo.TargetClrType);
        }

        private RelationalTypeMapping FindMappingForStoreTypeName(RelationalTypeMappingInfo mappingInfo)
        {
            if (mappingInfo.StoreTypeName != null)
            {
                _relationalTypeMapper.ValidateTypeName(mappingInfo.StoreTypeName);

                return _relationalTypeMapper.FindMapping(mappingInfo.StoreTypeName);
            }

            return null;
        }

        private RelationalTypeMapping FilterByClrType(RelationalTypeMapping mapping, RelationalTypeMappingInfo mappingInfo)
            => mapping != null
               && (mappingInfo.TargetClrType == null
                   || mappingInfo.TargetClrType == mapping.ClrType)
                ? mapping
                : null;
    }
}

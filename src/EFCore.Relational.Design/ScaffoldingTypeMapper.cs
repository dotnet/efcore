// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class ScaffoldingTypeMapper
    {
        public ScaffoldingTypeMapper([NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            TypeMapper = typeMapper;
        }

        protected virtual IRelationalTypeMapper TypeMapper { get; }

        public virtual TypeScaffoldingInfo FindMapping(
            [NotNull] string storeType,
            bool keyOrIndex,
            bool rowVersion)
        {
            Check.NotEmpty(storeType, nameof(storeType));

            var mapping = TypeMapper.FindMapping(storeType);
            if (mapping == null)
            {
                return null;
            }

            if (mapping.ClrType == typeof(byte[])
                && TypeMapper.ByteArrayMapper != null)
            {
                var byteArrayMapping = TypeMapper.ByteArrayMapper.FindMapping(rowVersion, keyOrIndex, mapping.Size);

                if (byteArrayMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    return new TypeScaffoldingInfo(
                        mapping.ClrType,
                        inferred: true,
                        scaffoldUnicode: null,
                        scaffoldMaxLength: byteArrayMapping.HasNonDefaultSize ? byteArrayMapping.Size : null);
                }
            }
            else if (mapping.ClrType == typeof(string)
                     && TypeMapper.StringMapper != null)
            {
                var stringMapping = TypeMapper.StringMapper.FindMapping(mapping.IsUnicode, keyOrIndex, mapping.Size);

                if (stringMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    return new TypeScaffoldingInfo(
                        mapping.ClrType,
                        inferred: true,
                        scaffoldUnicode: stringMapping.HasNonDefaultUnicode ? (bool?)stringMapping.IsUnicode : null,
                        scaffoldMaxLength: stringMapping.HasNonDefaultSize ? stringMapping.Size : null);
                }
            }
            else
            {
                var defaultMapping = TypeMapper.GetMapping(mapping.ClrType);

                if (defaultMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    return new TypeScaffoldingInfo(
                        mapping.ClrType,
                        inferred: true,
                        scaffoldUnicode: null,
                        scaffoldMaxLength: null);
                }
            }

            return new TypeScaffoldingInfo(
                mapping.ClrType,
                inferred: false,
                scaffoldUnicode: null,
                scaffoldMaxLength: null);
        }
    }
}

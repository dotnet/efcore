// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    public class ScaffoldingTypeMapper : IScaffoldingTypeMapper
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
            // This is because certain providers can have no type specified as a default type e.g. SQLite
            Check.NotNull(storeType, nameof(storeType));

            var mapping = TypeMapper.FindMapping(storeType);
            if (mapping == null)
            {
                return null;
            }

            bool canInfer = false;
            bool? scaffoldUnicode = null;
            int? scaffoldMaxLengh = null;

            if (mapping.ClrType == typeof(byte[])
                && TypeMapper.ByteArrayMapper != null)
            {
                // Check for inference
                var byteArrayMapping = TypeMapper.ByteArrayMapper.FindMapping(rowVersion, keyOrIndex, mapping.Size);

                if (byteArrayMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    canInfer = true;

                    // Check for size
                    var sizedMapping = TypeMapper.ByteArrayMapper.FindMapping(rowVersion, keyOrIndex, size: null);
                    scaffoldMaxLengh = sizedMapping.Size != byteArrayMapping.Size ? byteArrayMapping.Size : null;
                }
            }
            else if (mapping.ClrType == typeof(string)
                     && TypeMapper.StringMapper != null)
            {
                // Check for inference
                var stringMapping = TypeMapper.StringMapper.FindMapping(mapping.IsUnicode, keyOrIndex, mapping.Size);

                if (stringMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    canInfer = true;

                    // Check for unicode
                    var unicodeMapping = TypeMapper.StringMapper.FindMapping(unicode: true, keyOrIndex: keyOrIndex, maxLength: mapping.Size);
                    scaffoldUnicode = unicodeMapping.IsUnicode != stringMapping.IsUnicode ? (bool?)stringMapping.IsUnicode : null;

                    // Check for size
                    var sizedMapping = TypeMapper.StringMapper.FindMapping(unicode: mapping.IsUnicode, keyOrIndex: keyOrIndex, maxLength: null);
                    scaffoldMaxLengh = sizedMapping.Size != stringMapping.Size ? stringMapping.Size : null;
                }
            }
            else
            {
                var defaultMapping = TypeMapper.GetMapping(mapping.ClrType);

                if (defaultMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    canInfer = true;
                }
            }

            return new TypeScaffoldingInfo(
                mapping.ClrType,
                inferred: canInfer,
                scaffoldUnicode: scaffoldUnicode,
                scaffoldMaxLength: scaffoldMaxLengh);
        }
    }
}

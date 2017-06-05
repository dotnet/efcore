// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding
{
    public class ScaffoldingTypeMapper : IScaffoldingTypeMapper
    {
        public ScaffoldingTypeMapper([NotNull] ScaffoldingTypeMapperDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        protected virtual ScaffoldingTypeMapperDependencies Dependencies { get; }

        public virtual TypeScaffoldingInfo FindMapping(
            [NotNull] string storeType,
            bool keyOrIndex,
            bool rowVersion)
        {
            // This is because certain providers can have no type specified as a default type e.g. SQLite
            Check.NotNull(storeType, nameof(storeType));

            var mapping = Dependencies.TypeMapper.FindMapping(storeType);
            if (mapping == null)
            {
                return null;
            }

            bool canInfer = false;
            bool? scaffoldUnicode = null;
            int? scaffoldMaxLengh = null;

            if (mapping.ClrType == typeof(byte[])
                && Dependencies.TypeMapper.ByteArrayMapper != null)
            {
                // Check for inference
                var byteArrayMapping = Dependencies.TypeMapper.ByteArrayMapper.FindMapping(rowVersion, keyOrIndex, mapping.Size);

                if (byteArrayMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    canInfer = true;

                    // Check for size
                    var sizedMapping = Dependencies.TypeMapper.ByteArrayMapper.FindMapping(rowVersion, keyOrIndex, size: null);
                    scaffoldMaxLengh = sizedMapping.Size != byteArrayMapping.Size ? byteArrayMapping.Size : null;
                }
            }
            else if (mapping.ClrType == typeof(string)
                     && Dependencies.TypeMapper.StringMapper != null)
            {
                // Check for inference
                var stringMapping = Dependencies.TypeMapper.StringMapper.FindMapping(mapping.IsUnicode, keyOrIndex, mapping.Size);

                if (stringMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    canInfer = true;

                    // Check for unicode
                    var unicodeMapping = Dependencies.TypeMapper.StringMapper.FindMapping(unicode: true, keyOrIndex: keyOrIndex, maxLength: mapping.Size);
                    scaffoldUnicode = unicodeMapping.IsUnicode != stringMapping.IsUnicode ? (bool?)stringMapping.IsUnicode : null;

                    // Check for size
                    var sizedMapping = Dependencies.TypeMapper.StringMapper.FindMapping(unicode: mapping.IsUnicode, keyOrIndex: keyOrIndex, maxLength: null);
                    scaffoldMaxLengh = sizedMapping.Size != stringMapping.Size ? stringMapping.Size : null;
                }
            }
            else
            {
                var defaultMapping = Dependencies.TypeMapper.GetMapping(mapping.ClrType);

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

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Scaffolding.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class ScaffoldingTypeMapper : IScaffoldingTypeMapper
    {
        private readonly IRelationalTypeMapper _typeMapper;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public ScaffoldingTypeMapper([NotNull] IRelationalTypeMapper typeMapper)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));

            _typeMapper = typeMapper;
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual TypeScaffoldingInfo FindMapping(
            string storeType,
            bool keyOrIndex,
            bool rowVersion)
        {
            // This is because certain providers can have no type specified as a default type e.g. SQLite
            Check.NotNull(storeType, nameof(storeType));

            var mapping = _typeMapper.FindMapping(storeType);
            if (mapping == null)
            {
                return null;
            }

            var canInfer = false;
            bool? scaffoldUnicode = null;
            int? scaffoldMaxLength = null;

            if (mapping.ClrType == typeof(byte[])
                && _typeMapper.ByteArrayMapper != null)
            {
                // Check for inference
                var byteArrayMapping = _typeMapper.ByteArrayMapper.FindMapping(rowVersion, keyOrIndex, mapping.Size);

                if (byteArrayMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    canInfer = true;

                    // Check for size
                    var sizedMapping = _typeMapper.ByteArrayMapper.FindMapping(rowVersion, keyOrIndex, size: null);
                    scaffoldMaxLength = sizedMapping.Size != byteArrayMapping.Size ? byteArrayMapping.Size : null;
                }
            }
            else if (mapping.ClrType == typeof(string)
                     && _typeMapper.StringMapper != null)
            {
                // Check for inference
                var stringMapping = _typeMapper.StringMapper.FindMapping(mapping.IsUnicode, keyOrIndex, mapping.Size);

                if (stringMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    canInfer = true;

                    // Check for unicode
                    var unicodeMapping = _typeMapper.StringMapper.FindMapping(unicode: true, keyOrIndex: keyOrIndex, maxLength: mapping.Size);
                    scaffoldUnicode = unicodeMapping.IsUnicode != stringMapping.IsUnicode ? (bool?)stringMapping.IsUnicode : null;

                    // Check for size
                    var sizedMapping = _typeMapper.StringMapper.FindMapping(mapping.IsUnicode, keyOrIndex, maxLength: null);
                    scaffoldMaxLength = sizedMapping.Size != stringMapping.Size ? stringMapping.Size : null;
                }
            }
            else
            {
                var defaultMapping = _typeMapper.GetMapping(mapping.ClrType);

                if (defaultMapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
                {
                    canInfer = true;
                }
            }

            return new TypeScaffoldingInfo(
                mapping.ClrType,
                canInfer,
                scaffoldUnicode,
                scaffoldMaxLength);
        }
    }
}

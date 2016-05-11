// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class StringRelationalTypeMapper : IStringRelationalTypeMapper
    {
        private readonly ConcurrentDictionary<int, RelationalTypeMapping> _boundedAnsiMappings
            = new ConcurrentDictionary<int, RelationalTypeMapping>();

        private readonly ConcurrentDictionary<int, RelationalTypeMapping> _boundedUnicodeMappings
            = new ConcurrentDictionary<int, RelationalTypeMapping>();

        public StringRelationalTypeMapper(
            int maxBoundedAnsiLength,
            [NotNull] RelationalTypeMapping defaultAnsiMapping,
            [CanBeNull] RelationalTypeMapping unboundedAnsiMapping,
            [CanBeNull] RelationalTypeMapping keyAnsiMapping, 
            [NotNull] Func<int, RelationalTypeMapping> createBoundedAnsiMapping, 
            int maxBoundedUnicodeLength,
            [NotNull] RelationalTypeMapping defaultUnicodeMapping,
            [CanBeNull] RelationalTypeMapping unboundedUnicodeMapping, 
            [CanBeNull] RelationalTypeMapping keyUnicodeMapping, 
            [NotNull] Func<int, RelationalTypeMapping> createBoundedUnicodeMapping)
        {
            MaxBoundedAnsiLength = maxBoundedAnsiLength;
            DefaultAnsiMapping = defaultAnsiMapping;
            UnboundedAnsiMapping = unboundedAnsiMapping;
            KeyAnsiMapping = keyAnsiMapping;
            CreateBoundedAnsiMapping = createBoundedAnsiMapping;

            MaxBoundedUnicodeLength = maxBoundedUnicodeLength;
            DefaultUnicodeMapping = defaultUnicodeMapping;
            UnboundedUnicodeMapping = unboundedUnicodeMapping;
            KeyUnicodeMapping = keyUnicodeMapping;
            CreateBoundedUnicodeMapping = createBoundedUnicodeMapping;
        }

        public virtual int MaxBoundedAnsiLength { get; }
        public virtual RelationalTypeMapping DefaultAnsiMapping { get; }
        public virtual RelationalTypeMapping UnboundedAnsiMapping { get; }
        public virtual RelationalTypeMapping KeyAnsiMapping { get; }
        public virtual Func<int, RelationalTypeMapping> CreateBoundedAnsiMapping { get; }

        public virtual int MaxBoundedUnicodeLength { get; }
        public virtual RelationalTypeMapping DefaultUnicodeMapping { get; }
        public virtual RelationalTypeMapping UnboundedUnicodeMapping { get; }
        public virtual RelationalTypeMapping KeyUnicodeMapping { get; }
        public virtual Func<int, RelationalTypeMapping> CreateBoundedUnicodeMapping { get; }

        public virtual RelationalTypeMapping FindMapping(bool unicode, bool keyOrIndex, int? maxLength)
            => unicode
                ? FindMapping(
                    keyOrIndex,
                    maxLength,
                    MaxBoundedUnicodeLength,
                    UnboundedUnicodeMapping,
                    DefaultUnicodeMapping,
                    KeyUnicodeMapping,
                    _boundedUnicodeMappings,
                    CreateBoundedUnicodeMapping)
                : FindMapping(
                    keyOrIndex,
                    maxLength,
                    MaxBoundedAnsiLength,
                    UnboundedAnsiMapping,
                    DefaultAnsiMapping,
                    KeyAnsiMapping,
                    _boundedAnsiMappings,
                    CreateBoundedAnsiMapping);

        private static RelationalTypeMapping FindMapping(
            bool isKeyOrIndex,
            int? maxLength,
            int maxBoundedLength,
            RelationalTypeMapping unboundedMapping,
            RelationalTypeMapping defaultMapping,
            RelationalTypeMapping keyMapping,
            ConcurrentDictionary<int, RelationalTypeMapping> boundedMappings,
            Func<int, RelationalTypeMapping> createBoundedMapping)
        {
            var mapping = isKeyOrIndex ? keyMapping : defaultMapping;

            if (maxLength.HasValue
                && maxLength != mapping.Size)
            {
                return maxLength <= maxBoundedLength
                    ? boundedMappings.GetOrAdd(maxLength.Value, createBoundedMapping)
                    : unboundedMapping;
            }

            return mapping;
        }
    }
}

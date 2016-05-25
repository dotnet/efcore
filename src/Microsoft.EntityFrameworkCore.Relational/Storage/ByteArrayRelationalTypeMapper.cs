// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    public class ByteArrayRelationalTypeMapper : IByteArrayRelationalTypeMapper
    {
        private readonly ConcurrentDictionary<int, RelationalTypeMapping> _boundedMappings
            = new ConcurrentDictionary<int, RelationalTypeMapping>();

        public ByteArrayRelationalTypeMapper(
            int maxBoundedLength, 
            [NotNull] RelationalTypeMapping defaultMapping, 
            [CanBeNull] RelationalTypeMapping unboundedMapping, 
            [CanBeNull] RelationalTypeMapping keyMapping, 
            [CanBeNull] RelationalTypeMapping rowVersionMapping, 
            [NotNull] Func<int, RelationalTypeMapping> createBoundedMapping)
        {
            MaxBoundedLength = maxBoundedLength;
            DefaultMapping = defaultMapping;
            UnboundedMapping = unboundedMapping;
            KeyMapping = keyMapping;
            RowVersionMapping = rowVersionMapping;
            CreateBoundedMapping = createBoundedMapping;
        }

        public virtual int MaxBoundedLength { get; }
        public virtual RelationalTypeMapping DefaultMapping { get; }
        public virtual RelationalTypeMapping UnboundedMapping { get; }
        public virtual RelationalTypeMapping KeyMapping { get; }
        public virtual RelationalTypeMapping RowVersionMapping { get; }
        public virtual Func<int, RelationalTypeMapping> CreateBoundedMapping { get; }

        public virtual RelationalTypeMapping FindMapping(bool rowVersion, bool keyOrIndex, int? size)
        {
            if (rowVersion
                && RowVersionMapping != null)
            {
                return RowVersionMapping;
            }

            var defaultMapping = keyOrIndex && KeyMapping != null ? KeyMapping : DefaultMapping;

            if (size.HasValue
                && size != defaultMapping.Size)
            {
                return size <= MaxBoundedLength
                    ? _boundedMappings.GetOrAdd(size.Value, CreateBoundedMapping)
                    : UnboundedMapping;
            }

            return defaultMapping;
        }
    }
}

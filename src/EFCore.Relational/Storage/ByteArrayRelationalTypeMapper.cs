// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Determines the type mapping to use for byte array properties.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    [Obsolete("Use RelationalTypeMappingSource.")]
    public class ByteArrayRelationalTypeMapper : IByteArrayRelationalTypeMapper
    {
        private readonly ConcurrentDictionary<int, RelationalTypeMapping> _boundedMappings
            = new ConcurrentDictionary<int, RelationalTypeMapping>();

        /// <summary>
        ///     Initialized a new instance of the <see cref="ByteArrayRelationalTypeMapper" /> class.
        /// </summary>
        /// <param name="maxBoundedLength"> Maximum length of data that can be stored in a byte array property. </param>
        /// <param name="defaultMapping"> Default mapping to be used. </param>
        /// <param name="unboundedMapping"> Mapping to be used for properties with no length specified. </param>
        /// <param name="keyMapping"> Mapping to be used for key properties. </param>
        /// <param name="rowVersionMapping"> Mapping to be used for properties being used as a row version. </param>
        /// <param name="createBoundedMapping"> Function to create a mapping for a property with a given length. </param>
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

        /// <summary>
        ///     Gets the maximum length of data that can be stored in a byte array property
        /// </summary>
        public virtual int MaxBoundedLength { get; }

        /// <summary>
        ///     Gets the default mapping to be used.
        /// </summary>
        public virtual RelationalTypeMapping DefaultMapping { get; }

        /// <summary>
        ///     Gets the mapping to be used for properties with no length specified
        /// </summary>
        public virtual RelationalTypeMapping UnboundedMapping { get; }

        /// <summary>
        ///     Gets the mapping to be used for key properties
        /// </summary>
        public virtual RelationalTypeMapping KeyMapping { get; }

        /// <summary>
        ///     Gets the mapping to be used for properties being used as a row version.
        /// </summary>
        public virtual RelationalTypeMapping RowVersionMapping { get; }

        /// <summary>
        ///     Gets a function to create a mapping for a property with a given length.
        /// </summary>
        public virtual Func<int, RelationalTypeMapping> CreateBoundedMapping { get; }

        /// <summary>
        ///     Gets the mapping for a property.
        /// </summary>
        /// <param name="rowVersion">
        ///     A value indicating whether the property is being used as a row version.
        /// </param>
        /// <param name="keyOrIndex">
        ///     A value indicating whether the property is being used as a key and/or index.
        /// </param>
        /// <param name="size">
        ///     The configured length of the property, or null if it is unbounded.
        /// </param>
        /// <returns> The mapping to be used for the property. </returns>
        public virtual RelationalTypeMapping FindMapping(bool rowVersion, bool keyOrIndex, int? size)
        {
            if (rowVersion
                && RowVersionMapping != null)
            {
                return RowVersionMapping;
            }

            var defaultMapping = keyOrIndex && KeyMapping != null ? KeyMapping : DefaultMapping;

            return size.HasValue
                && size != defaultMapping.Size
                ? size <= MaxBoundedLength
                    ? _boundedMappings.GetOrAdd(size.Value, CreateBoundedMapping)
                    : UnboundedMapping
                : defaultMapping;
        }
    }
}

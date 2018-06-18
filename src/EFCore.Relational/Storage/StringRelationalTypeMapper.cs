// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Maps string property types to their corresponding relational database types.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    [Obsolete("Use RelationalTypeMappingSource.")]
    public class StringRelationalTypeMapper : IStringRelationalTypeMapper
    {
        private readonly ConcurrentDictionary<int, RelationalTypeMapping> _boundedAnsiMappings
            = new ConcurrentDictionary<int, RelationalTypeMapping>();

        private readonly ConcurrentDictionary<int, RelationalTypeMapping> _boundedUnicodeMappings
            = new ConcurrentDictionary<int, RelationalTypeMapping>();

        /// <summary>
        ///     Initializes a new instance of the <see cref="StringRelationalTypeMapper" /> class.
        /// </summary>
        /// <param name="maxBoundedAnsiLength"> The maximum length of a bounded ANSI string. </param>
        /// <param name="defaultAnsiMapping"> The default mapping of an ANSI string. </param>
        /// <param name="unboundedAnsiMapping"> The mapping for an unbounded ANSI string. </param>
        /// <param name="keyAnsiMapping"> The mapping for an ANSI string that is part of a key. </param>
        /// <param name="createBoundedAnsiMapping"> The function to create a mapping for a bounded ANSI string. </param>
        /// <param name="maxBoundedUnicodeLength"> The maximum length of a bounded Unicode string. </param>
        /// <param name="defaultUnicodeMapping"> The default mapping of a Unicode string. </param>
        /// <param name="unboundedUnicodeMapping"> The mapping for an unbounded Unicode string. </param>
        /// <param name="keyUnicodeMapping"> The mapping for a Unicode string that is part of a key. </param>
        /// <param name="createBoundedUnicodeMapping"> The function to create a mapping for a bounded Unicode string. </param>
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

        /// <summary>
        ///     Gets the maximum length of a bounded ANSI string.
        /// </summary>
        public virtual int MaxBoundedAnsiLength { get; }

        /// <summary>
        ///     Gets the default mapping of an ANSI string.
        /// </summary>
        public virtual RelationalTypeMapping DefaultAnsiMapping { get; }

        /// <summary>
        ///     Gets the mapping for an unbounded ANSI string.
        /// </summary>
        public virtual RelationalTypeMapping UnboundedAnsiMapping { get; }

        /// <summary>
        ///     Gets the mapping for an ANSI string that is part of a key.
        /// </summary>
        public virtual RelationalTypeMapping KeyAnsiMapping { get; }

        /// <summary>
        ///     Gets the function to create a mapping for a bounded ANSI string.
        /// </summary>
        public virtual Func<int, RelationalTypeMapping> CreateBoundedAnsiMapping { get; }

        /// <summary>
        ///     Gets the maximum length of a bounded Unicode string.
        /// </summary>
        public virtual int MaxBoundedUnicodeLength { get; }

        /// <summary>
        ///     Gets the default mapping of a Unicode string.
        /// </summary>
        public virtual RelationalTypeMapping DefaultUnicodeMapping { get; }

        /// <summary>
        ///     Gets the mapping for an unbounded Unicode string.
        /// </summary>
        public virtual RelationalTypeMapping UnboundedUnicodeMapping { get; }

        /// <summary>
        ///     Gets the mapping for a Unicode string that is part of a key.
        /// </summary>
        public virtual RelationalTypeMapping KeyUnicodeMapping { get; }

        /// <summary>
        ///     Gets the function to create a mapping for a bounded Unicode string.
        /// </summary>
        public virtual Func<int, RelationalTypeMapping> CreateBoundedUnicodeMapping { get; }

        /// <summary>
        ///     Gets the relational database type for a string property.
        /// </summary>
        /// <param name="unicode">A value indicating whether the property should handle Unicode data or not.</param>
        /// <param name="keyOrIndex">A value indicating whether the property is part of a key or not.</param>
        /// <param name="maxLength">The maximum length of data the property is configured to store, or null if no maximum is configured.</param>
        /// <returns>
        ///     The type mapping to be used.
        /// </returns>
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

            return maxLength.HasValue
                && maxLength != mapping.Size
                ? maxLength <= maxBoundedLength
                    ? boundedMappings.GetOrAdd(maxLength.Value, createBoundedMapping)
                    : unboundedMapping
                : mapping;
        }
    }
}

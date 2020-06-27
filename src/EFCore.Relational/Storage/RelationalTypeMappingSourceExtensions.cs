// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Extension methods for the <see cref="IRelationalTypeMappingSource" /> class.
    /// </summary>
    public static class RelationalTypeMappingSourceExtensions
    {
        /// <summary>
        ///     Gets the relational database type for a given object, throwing if no mapping is found.
        /// </summary>
        /// <param name="typeMappingSource"> The type mapping source. </param>
        /// <param name="value"> The object to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        public static RelationalTypeMapping GetMappingForValue(
            [CanBeNull] this IRelationalTypeMappingSource typeMappingSource,
            [CanBeNull] object value)
            => value == null
                || value == DBNull.Value
                || typeMappingSource == null
                    ? RelationalTypeMapping.NullMapping
                    : typeMappingSource.GetMapping(value.GetType());

        /// <summary>
        ///     Gets the relational database type for a given property, throwing if no mapping is found.
        /// </summary>
        /// <param name="typeMappingSource"> The type mapping source. </param>
        /// <param name="property"> The property to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        public static RelationalTypeMapping GetMapping(
            [NotNull] this IRelationalTypeMappingSource typeMappingSource,
            [NotNull] IProperty property)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(property, nameof(property));

            var mapping = typeMappingSource.FindMapping(property);

            if (mapping != null)
            {
                return mapping;
            }

            throw new InvalidOperationException(
                RelationalStrings.UnsupportedPropertyType(
                    property.DeclaringEntityType.DisplayName(),
                    property.Name,
                    property.ClrType.ShortDisplayName()));
        }

        /// <summary>
        ///     Gets the relational database type for a given .NET type, throwing if no mapping is found.
        /// </summary>
        /// <param name="typeMappingSource"> The type mapping source. </param>
        /// <param name="clrType"> The type to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        public static RelationalTypeMapping GetMapping(
            [NotNull] this IRelationalTypeMappingSource typeMappingSource,
            [NotNull] Type clrType)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(clrType, nameof(clrType));

            var mapping = typeMappingSource.FindMapping(clrType);
            if (mapping != null)
            {
                return mapping;
            }

            throw new InvalidOperationException(RelationalStrings.UnsupportedType(clrType.ShortDisplayName()));
        }

        /// <summary>
        ///     <para>
        ///         Gets the mapping that represents the given database type, throwing if no mapping is found.
        ///     </para>
        ///     <para>
        ///         Note that sometimes the same store type can have different mappings; this method returns the default.
        ///     </para>
        /// </summary>
        /// <param name="typeMappingSource"> The type mapping source. </param>
        /// <param name="typeName"> The type to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        public static RelationalTypeMapping GetMapping(
            [NotNull] this IRelationalTypeMappingSource typeMappingSource,
            [NotNull] string typeName)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            // Note: Empty string is allowed for store type name because SQLite
            Check.NotNull(typeName, nameof(typeName));

            var mapping = typeMappingSource.FindMapping(typeName);
            if (mapping != null)
            {
                return mapping;
            }

            throw new InvalidOperationException(RelationalStrings.UnsupportedStoreType(typeName));
        }

        /// <summary>
        ///     <para>
        ///         Finds the type mapping for a given <see cref="Type" /> and additional facets, throwing if no mapping is found.
        ///     </para>
        ///     <para>
        ///         Note: Only call this method if there is no <see cref="IProperty" /> available, otherwise
        ///         call <see cref="GetMapping(IRelationalTypeMappingSource, IProperty)" />
        ///     </para>
        /// </summary>
        /// <param name="typeMappingSource"> The type mapping source. </param>
        /// <param name="type"> The CLR type. </param>
        /// <param name="storeTypeName"> The database type name. </param>
        /// <param name="keyOrIndex"> If <see langword="true" />, then a special mapping for a key or index may be returned. </param>
        /// <param name="unicode">
        ///     Specify <see langword="true" /> for Unicode mapping, <see langword="false" /> for Ansi mapping or <see langword="null" /> for the default.
        /// </param>
        /// <param name="size"> Specifies a size for the mapping, or <see langword="null" /> for default. </param>
        /// <param name="rowVersion"> Specifies a row-version, or <see langword="null" /> for default. </param>
        /// <param name="fixedLength"> Specifies a fixed length mapping, or <see langword="null" /> for default. </param>
        /// <param name="precision"> Specifies a precision for the mapping, or <see langword="null" /> for default. </param>
        /// <param name="scale"> Specifies a scale for the mapping, or <see langword="null" /> for default. </param>
        /// <returns> The type mapping, or <see langword="null" /> if none was found. </returns>
        public static RelationalTypeMapping GetMapping(
            [NotNull] this IRelationalTypeMappingSource typeMappingSource,
            [NotNull] Type type,
            [CanBeNull] string storeTypeName,
            bool keyOrIndex = false,
            bool? unicode = null,
            int? size = null,
            bool? rowVersion = null,
            bool? fixedLength = null,
            int? precision = null,
            int? scale = null)
        {
            Check.NotNull(typeMappingSource, nameof(typeMappingSource));
            Check.NotNull(type, nameof(type));

            var mapping = typeMappingSource.FindMapping(
                type, storeTypeName, keyOrIndex, unicode, size, rowVersion, fixedLength, precision, scale);
            if (mapping != null)
            {
                return mapping;
            }

            throw new InvalidOperationException(RelationalStrings.UnsupportedType(type));
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
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
            this IRelationalTypeMappingSource? typeMappingSource,
            object? value)
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
            this IRelationalTypeMappingSource typeMappingSource,
            IProperty property)
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
            this IRelationalTypeMappingSource typeMappingSource,
            Type clrType)
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
            this IRelationalTypeMappingSource typeMappingSource,
            string typeName)
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
    }
}

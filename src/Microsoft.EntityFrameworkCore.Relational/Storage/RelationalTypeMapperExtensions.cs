// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Extension methods for the <see cref="RelationalTypeMapping" /> class.
    /// </summary>
    public static class RelationalTypeMapperExtensions
    {
        /// <summary>
        ///     Gets the relational database type for a given object, throwing if no mapping is found.
        /// </summary>
        /// <param name="typeMapper"> The type mapper. </param>
        /// <param name="value"> The object to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        public static RelationalTypeMapping GetMappingForValue(
            [CanBeNull] this IRelationalTypeMapper typeMapper,
            [CanBeNull] object value)
            => (value == null)
               || (value == DBNull.Value)
               || (typeMapper == null)
                ? RelationalTypeMapping.NullMapping
                : typeMapper.GetMapping(value.GetType());

        /// <summary>
        ///     Gets the relational database type for a given property, throwing if no mapping is found.
        /// </summary>
        /// <param name="typeMapper"> The type mapper. </param>
        /// <param name="property"> The property to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        public static RelationalTypeMapping GetMapping(
            [NotNull] this IRelationalTypeMapper typeMapper,
            [NotNull] IProperty property)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(property, nameof(property));

            var mapping = typeMapper.FindMapping(property);
            if (mapping != null)
            {
                return mapping;
            }

            throw new InvalidOperationException(RelationalStrings.UnsupportedPropertyType(
                property.DeclaringEntityType.DisplayName(),
                property.Name,
                property.ClrType.ShortDisplayName()));
        }

        /// <summary>
        ///     Gets the relational database type for a given .NET type, throwing if no mapping is found.
        /// </summary>
        /// <param name="typeMapper"> The type mapper. </param>
        /// <param name="clrType"> The type to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        public static RelationalTypeMapping GetMapping(
            [NotNull] this IRelationalTypeMapper typeMapper,
            [NotNull] Type clrType)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(clrType, nameof(clrType));

            var mapping = typeMapper.FindMapping(clrType);
            if (mapping != null)
            {
                return mapping;
            }

            throw new InvalidOperationException(RelationalStrings.UnsupportedType(clrType.ShortDisplayName()));
        }

        /// <summary>
        ///     Gets the mapping that represents the given database type, throwing if no mapping is found.
        /// </summary>
        /// <param name="typeMapper"> The type mapper. </param>
        /// <param name="typeName"> The type to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        public static RelationalTypeMapping GetMapping(
            [NotNull] this IRelationalTypeMapper typeMapper,
            [NotNull] string typeName)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(typeName, nameof(typeName));

            var mapping = typeMapper.FindMapping(typeName);
            if (mapping != null)
            {
                return mapping;
            }

            throw new InvalidOperationException(RelationalStrings.UnsupportedType(typeName));
        }

        /// <summary>
        ///     Gets a value indicating whether the given .NET type is mapped.
        /// </summary>
        /// <param name="typeMapper"> The type mapper. </param>
        /// <param name="clrType"> The .NET type. </param>
        /// <returns> True if the type can be mapped; otherwise false. </returns>
        public static bool IsTypeMapped(
            [NotNull] this IRelationalTypeMapper typeMapper,
            [NotNull] Type clrType)
        {
            Check.NotNull(typeMapper, nameof(typeMapper));
            Check.NotNull(clrType, nameof(clrType));

            return typeMapper.FindMapping(clrType) != null;
        }
    }
}

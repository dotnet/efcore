// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         Maps .NET types to their corresponding relational database types.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    [Obsolete("Use RelationalTypeMappingSource.")]
    public abstract class RelationalTypeMapper : IRelationalTypeMapper
    {
        private readonly ConcurrentDictionary<string, RelationalTypeMapping> _explicitMappings
            = new ConcurrentDictionary<string, RelationalTypeMapping>();

        /// <summary>
        ///     Initializes a new instance of the this class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        protected RelationalTypeMapper([NotNull] RelationalTypeMapperDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));
        }

        /// <summary>
        ///     Gets the mappings from .NET types to database types.
        /// </summary>
        /// <returns> The type mappings. </returns>
        protected abstract IReadOnlyDictionary<Type, RelationalTypeMapping> GetClrTypeMappings();

        /// <summary>
        ///     Gets the mappings from database types to .NET types.
        /// </summary>
        /// <returns> The type mappings. </returns>
        protected abstract IReadOnlyDictionary<string, RelationalTypeMapping> GetStoreTypeMappings();

        /// <summary>
        ///     Gets column type for the given property.
        /// </summary>
        /// <param name="property"> The property. </param>
        /// <returns> The name of the database type. </returns>
        protected virtual string GetColumnType([NotNull] IProperty property)
            => (string)Check.NotNull(property, nameof(property))[RelationalAnnotationNames.ColumnType];

        /// <summary>
        ///     Ensures that the given type name is a valid type for the relational database.
        ///     An exception is thrown if it is not a valid type.
        /// </summary>
        /// <param name="storeType">The type to be validated.</param>
        public virtual void ValidateTypeName(string storeType)
        {
        }

        /// <summary>
        ///     Gets a value indicating whether the given .NET type is mapped.
        /// </summary>
        /// <param name="clrType"> The .NET type. </param>
        /// <returns> True if the type can be mapped; otherwise false. </returns>
        public virtual bool IsTypeMapped(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            return FindMapping(clrType) != null;
        }

        /// <summary>
        ///     Gets the relational database type for the given property.
        ///     Returns null if no mapping is found.
        /// </summary>
        /// <param name="property">The property to get the mapping for.</param>
        /// <returns>
        ///     The type mapping to be used.
        /// </returns>
        public virtual RelationalTypeMapping FindMapping(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var storeType = GetColumnType(property);

            if (storeType == null)
            {
                var principalProperty = property.FindPrincipal();
                if (principalProperty != null)
                {
                    storeType = GetColumnType(principalProperty);
                }
            }

            return (storeType != null ? FindMapping(storeType) : null)
                   ?? FindCustomMapping(property)
                   ?? FindMapping(property.ClrType);
        }

        /// <summary>
        ///     Gets the relational database type for a given .NET type.
        ///     Returns null if no mapping is found.
        /// </summary>
        /// <param name="clrType">The type to get the mapping for.</param>
        /// <returns>
        ///     The type mapping to be used.
        /// </returns>
        public virtual RelationalTypeMapping FindMapping(Type clrType)
        {
            Check.NotNull(clrType, nameof(clrType));

            return GetClrTypeMappings().TryGetValue(clrType.UnwrapNullableType().UnwrapEnumType(), out var mapping)
                ? mapping
                : null;
        }

        /// <summary>
        ///     Gets the mapping that represents the given database type.
        ///     Returns null if no mapping is found.
        /// </summary>
        /// <param name="storeType">The type to get the mapping for.</param>
        /// <returns>
        ///     The type mapping to be used.
        /// </returns>
        public virtual RelationalTypeMapping FindMapping(string storeType)
        {
            Check.NotNull(storeType, nameof(storeType));

            return _explicitMappings.GetOrAdd(storeType, CreateMappingFromStoreType);
        }

        /// <summary>
        ///     Creates the mapping for the given database type.
        /// </summary>
        /// <param name="storeType">The type to create the mapping for.</param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping CreateMappingFromStoreType([NotNull] string storeType)
        {
            Check.NotNull(storeType, nameof(storeType));

            if (GetStoreTypeMappings().TryGetValue(storeType, out var mapping)
                && mapping.StoreType.Equals(storeType, StringComparison.OrdinalIgnoreCase))
            {
                return mapping;
            }

            var openParen = storeType.IndexOf("(", StringComparison.Ordinal);
            if (openParen > 0)
            {
                if (!GetStoreTypeMappings().TryGetValue(storeType.Substring(0, openParen), out mapping))
                {
                    return null;
                }

                if (mapping.ClrType == typeof(string)
                    || mapping.ClrType == typeof(byte[]))
                {
                    var closeParen = storeType.IndexOf(")", openParen + 1, StringComparison.Ordinal);

                    if (closeParen > openParen
                        && int.TryParse(storeType.Substring(openParen + 1, closeParen - openParen - 1), out var size)
                        && mapping.Size != size)
                    {
                        return mapping.Clone(storeType, size);
                    }
                }
            }

            return mapping?.Clone(storeType, mapping.Size);
        }

        /// <summary>
        ///     Gets the relational database type for the given property, using a separate type mapper if needed.
        ///     This base implementation uses custom mappers for string and byte array properties.
        ///     Returns null if no mapping is found.
        /// </summary>
        /// <param name="property">The property to get the mapping for.</param>
        /// <returns>
        ///     The type mapping to be used.
        /// </returns>
        protected virtual RelationalTypeMapping FindCustomMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var clrType = property.ClrType.UnwrapNullableType();

            return clrType == typeof(string)
                ? GetStringMapping(property)
                : clrType == typeof(byte[])
                    ? GetByteArrayMapping(property)
                    : null;
        }

        /// <summary>
        ///     Gets the mapper to be used for byte array properties.
        /// </summary>
        public virtual IByteArrayRelationalTypeMapper ByteArrayMapper => null;

        /// <summary>
        ///     Gets the mapper to be used for string properties.
        /// </summary>
        public virtual IStringRelationalTypeMapper StringMapper => null;

        /// <summary>
        ///     Gets the relational database type for the given string property.
        /// </summary>
        /// <param name="property"> The property to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping GetStringMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            var principal = property.FindPrincipal();

            return StringMapper?.FindMapping(
                property.IsUnicode() ?? principal?.IsUnicode() ?? true,
                RequiresKeyMapping(property),
                property.GetMaxLength() ?? principal?.GetMaxLength());
        }

        /// <summary>
        ///     Gets the relational database type for the given byte array property.
        /// </summary>
        /// <param name="property"> The property to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        protected virtual RelationalTypeMapping GetByteArrayMapping([NotNull] IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return ByteArrayMapper?.FindMapping(
                property.IsConcurrencyToken && property.ValueGenerated == ValueGenerated.OnAddOrUpdate,
                RequiresKeyMapping(property),
                property.GetMaxLength() ?? property.FindPrincipal()?.GetMaxLength());
        }

        /// <summary>
        ///     Gets a value indicating whether the given property should use a database type that is suitable for key properties.
        /// </summary>
        /// <param name="property"> The property to get the mapping for. </param>
        /// <returns> True if the property is a key, otherwise false. </returns>
        protected virtual bool RequiresKeyMapping([NotNull] IProperty property)
            => property.IsKey() || property.IsForeignKey();
    }
}

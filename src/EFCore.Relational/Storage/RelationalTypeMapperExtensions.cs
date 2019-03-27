// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     Extension methods for the <see cref="IRelationalTypeMapper" /> class.
    /// </summary>
    [Obsolete("Use IRelationalTypeMappingSource instead.")]
    public static class RelationalTypeMapperExtensions
    {
        /// <summary>
        ///     Gets the relational database type for a given object, throwing if no mapping is found.
        /// </summary>
        /// <param name="typeMapper"> The type mapper. </param>
        /// <param name="value"> The object to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        [Obsolete("Use IRelationalTypeMappingSource instead.")]
        public static RelationalTypeMapping GetMappingForValue(
            [CanBeNull] this IRelationalTypeMapper typeMapper,
            [CanBeNull] object value)
            => typeMapper == null
                ? null
                : new FallbackRelationalTypeMappingSource(
                        new TypeMappingSourceDependencies(
                            new ValueConverterSelector(
                                new ValueConverterSelectorDependencies()),
                            Enumerable.Empty<ITypeMappingSourcePlugin>()),
                        new RelationalTypeMappingSourceDependencies(
                            Enumerable.Empty<IRelationalTypeMappingSourcePlugin>()),
                        typeMapper)
                    .GetMappingForValue(value);

        /// <summary>
        ///     Gets the relational database type for a given property, throwing if no mapping is found.
        /// </summary>
        /// <param name="typeMapper"> The type mapper. </param>
        /// <param name="property"> The property to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        [Obsolete("Use IRelationalTypeMappingSource instead.")]
        public static RelationalTypeMapping GetMapping(
            [NotNull] this IRelationalTypeMapper typeMapper,
            [NotNull] IProperty property)
            => new FallbackRelationalTypeMappingSource(
                    new TypeMappingSourceDependencies(
                        new ValueConverterSelector(
                            new ValueConverterSelectorDependencies()),
                        Enumerable.Empty<ITypeMappingSourcePlugin>()),
                    new RelationalTypeMappingSourceDependencies(
                        Enumerable.Empty<IRelationalTypeMappingSourcePlugin>()),
                    typeMapper)
                .GetMapping(property);

        /// <summary>
        ///     Gets the relational database type for a given .NET type, throwing if no mapping is found.
        /// </summary>
        /// <param name="typeMapper"> The type mapper. </param>
        /// <param name="clrType"> The type to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        [Obsolete("Use IRelationalTypeMappingSource instead.")]
        public static RelationalTypeMapping GetMapping(
            [NotNull] this IRelationalTypeMapper typeMapper,
            [NotNull] Type clrType)
            => new FallbackRelationalTypeMappingSource(
                    new TypeMappingSourceDependencies(
                        new ValueConverterSelector(
                            new ValueConverterSelectorDependencies()),
                        Enumerable.Empty<ITypeMappingSourcePlugin>()),
                    new RelationalTypeMappingSourceDependencies(
                        Enumerable.Empty<IRelationalTypeMappingSourcePlugin>()),
                    typeMapper)
                .GetMapping(clrType);

        /// <summary>
        ///     <para>
        ///         Gets the mapping that represents the given database type, throwing if no mapping is found.
        ///     </para>
        ///     <para>
        ///         Note that sometimes the same store type can have different mappings; this method returns the default.
        ///     </para>
        /// </summary>
        /// <param name="typeMapper"> The type mapper. </param>
        /// <param name="typeName"> The type to get the mapping for. </param>
        /// <returns> The type mapping to be used. </returns>
        [Obsolete("Use IRelationalTypeMappingSource instead.")]
        public static RelationalTypeMapping GetMapping(
            [NotNull] this IRelationalTypeMapper typeMapper,
            [NotNull] string typeName)
            => new FallbackRelationalTypeMappingSource(
                    new TypeMappingSourceDependencies(
                        new ValueConverterSelector(
                            new ValueConverterSelectorDependencies()),
                        Enumerable.Empty<ITypeMappingSourcePlugin>()),
                    new RelationalTypeMappingSourceDependencies(
                        Enumerable.Empty<IRelationalTypeMappingSourcePlugin>()),
                    typeMapper)
                .GetMapping(typeName);
    }
}

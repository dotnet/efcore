// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     <para>
    ///         Selects value generators to be used to generate values for properties of entities.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public class ValueGeneratorSelector : IValueGeneratorSelector
    {
        /// <summary>
        ///     The cache being used to store value generator instances.
        /// </summary>
        public virtual IValueGeneratorCache Cache => Dependencies.Cache;

        /// <summary>
        ///     Initializes a new instance of the <see cref="ValueGeneratorSelector" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public ValueGeneratorSelector([NotNull] ValueGeneratorSelectorDependencies dependencies)
        {
            Check.NotNull(dependencies, nameof(dependencies));

            Dependencies = dependencies;
        }

        /// <summary>
        ///     Parameter object containing dependencies for this service.
        /// </summary>
        protected virtual ValueGeneratorSelectorDependencies Dependencies { get; }

        /// <summary>
        ///     Selects the appropriate value generator for a given property.
        /// </summary>
        /// <param name="property"> The property to get the value generator for. </param>
        /// <param name="entityType">
        ///     The entity type that the value generator will be used for. When called on inherited properties on derived entity types,
        ///     this entity type may be different from the declared entity type on <paramref name="property" />
        /// </param>
        /// <returns> The value generator to be used. </returns>
        public virtual ValueGenerator Select(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            return Cache.GetOrAdd(property, entityType, (p, t) => CreateFromFactory(p, t) ?? Create(p, t));
        }

        private static ValueGenerator CreateFromFactory(IProperty property, IEntityType entityType)
        {
            var factory = property.GetValueGeneratorFactory();

            if (factory == null)
            {
                var mapping = property.FindMapping();
                factory = mapping?.ValueGeneratorFactory;

                if (factory == null)
                {
                    var converter = mapping?.Converter
                                    ?? property.GetValueConverter();

                    if (converter != null)
                    {
                        throw new NotSupportedException(
                            CoreStrings.ValueGenWithConversion(
                                property.DeclaringEntityType.DisplayName(),
                                property.Name,
                                converter.GetType().ShortDisplayName()));
                    }
                }
            }

            return factory?.Invoke(property, entityType);
        }

        /// <summary>
        ///     Creates a new value generator for the given property.
        /// </summary>
        /// <param name="property"> The property to get the value generator for. </param>
        /// <param name="entityType">
        ///     The entity type that the value generator will be used for. When called on inherited properties on derived entity types,
        ///     this entity type may be different from the declared entity type on <paramref name="property" />
        /// </param>
        /// <returns> The newly created value generator. </returns>
        public virtual ValueGenerator Create([NotNull] IProperty property, [NotNull] IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            var propertyType = property.ClrType.UnwrapNullableType().UnwrapEnumType();

            if (propertyType == typeof(Guid))
            {
                return new GuidValueGenerator();
            }

            if (propertyType == typeof(string))
            {
                return new StringValueGenerator(generateTemporaryValues: false);
            }

            if (propertyType == typeof(byte[]))
            {
                return new BinaryValueGenerator(generateTemporaryValues: false);
            }

            throw new NotSupportedException(
                CoreStrings.NoValueGenerator(property.Name, property.DeclaringEntityType.DisplayName(), propertyType.ShortDisplayName()));
        }
    }
}

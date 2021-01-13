// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.DependencyInjection;

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
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public class RelationalValueGeneratorSelector : ValueGeneratorSelector
    {
        private readonly TemporaryNumberValueGeneratorFactory _numberFactory
            = new TemporaryNumberValueGeneratorFactory();

        /// <summary>
        ///     Initializes a new instance of the <see cref="RelationalValueGeneratorSelector" /> class.
        /// </summary>
        /// <param name="dependencies"> Parameter object containing dependencies for this service. </param>
        public RelationalValueGeneratorSelector([NotNull] ValueGeneratorSelectorDependencies dependencies)
            : base(dependencies)
        {
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
        public override ValueGenerator Create(IProperty property, IEntityType entityType)
        {
            Check.NotNull(property, nameof(property));
            Check.NotNull(entityType, nameof(entityType));

            if (property.ValueGenerated != ValueGenerated.Never)
            {
                var propertyType = property.ClrType.UnwrapNullableType().UnwrapEnumType();

                if (propertyType.IsInteger()
                    || propertyType == typeof(decimal)
                    || propertyType == typeof(float)
                    || propertyType == typeof(double))
                {
                    return _numberFactory.Create(property);
                }

                if (propertyType == typeof(DateTime))
                {
                    return new TemporaryDateTimeValueGenerator();
                }

                if (propertyType == typeof(DateTimeOffset))
                {
                    return new TemporaryDateTimeOffsetValueGenerator();
                }

                if (property.GetDefaultValueSql() != null)
                {
                    if (propertyType == typeof(Guid))
                    {
                        return new TemporaryGuidValueGenerator();
                    }

                    if (propertyType == typeof(string))
                    {
                        return new TemporaryStringValueGenerator();
                    }

                    if (propertyType == typeof(byte[]))
                    {
                        return new TemporaryBinaryValueGenerator();
                    }
                }
            }

            return base.Create(property, entityType);
        }
    }
}

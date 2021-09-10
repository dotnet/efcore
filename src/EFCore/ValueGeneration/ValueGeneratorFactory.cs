// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     Base class for factories that create value generators.
    /// </summary>
    /// <remarks>
    ///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information.
    /// </remarks>
    public abstract class ValueGeneratorFactory
    {
        /// <summary>
        ///     Creates a new value generator.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information.
        /// </remarks>
        /// <param name="property"> The property to create the value generator for. </param>
        /// <returns> The newly created value generator. </returns>
        [Obsolete("Use the overload with most parameters")]
        public virtual ValueGenerator Create(IProperty property)
            => Create(property, property.DeclaringEntityType);

        /// <summary>
        ///     Creates a new value generator.
        /// </summary>
        /// <remarks>
        ///     See <see href="https://aka.ms/efcore-docs-value-generation">EF Core value generation</see> for more information.
        /// </remarks>
        /// <param name="property"> The property to create the value generator for. </param>
        /// <param name="entityType"> The entity type for which the value generator will be used. </param>
        /// <returns> The newly created value generator. </returns>
        public abstract ValueGenerator Create(IProperty property, IEntityType entityType);
    }
}

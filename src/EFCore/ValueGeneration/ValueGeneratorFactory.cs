// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     Base class for factories that create value generators.
    /// </summary>
    public abstract class ValueGeneratorFactory
    {
        /// <summary>
        ///     Creates a new value generator.
        /// </summary>
        /// <param name="property"> The property to create the value generator for. </param>
        /// <returns> The newly created value generator. </returns>
        [Obsolete("Use the overload with most parameters")]
        public virtual ValueGenerator Create(IProperty property)
            => Create(property, property.DeclaringEntityType);

        /// <summary>
        ///     Creates a new value generator.
        /// </summary>
        /// <param name="property"> The property to create the value generator for. </param>
        /// <param name="entityType"> The entity type for which the value generator will be used. </param>
        /// <returns> The newly created value generator. </returns>
        public abstract ValueGenerator Create(IProperty property, IEntityType entityType);
    }
}

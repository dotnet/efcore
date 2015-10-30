// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.ValueGeneration
{
    /// <summary>
    ///    Factory to create value generators of type <typeparam name="TValueGenerator" />.
    /// </summary>
    public class ValueGeneratorFactory<TValueGenerator> : ValueGeneratorFactory
        where TValueGenerator : ValueGenerator, new()
    {
        /// <summary>
        ///     Creates a new value generator.
        /// </summary>
        /// <param name="property"> The property to create the value generator for. </param>
        /// <returns> The newly created value generator. </returns>
        public override ValueGenerator Create(IProperty property)
        {
            Check.NotNull(property, nameof(property));

            return new TValueGenerator();
        }
    }
}

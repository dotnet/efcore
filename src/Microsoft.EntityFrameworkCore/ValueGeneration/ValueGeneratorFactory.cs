// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
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
        public abstract ValueGenerator Create([NotNull] IProperty property);
    }
}

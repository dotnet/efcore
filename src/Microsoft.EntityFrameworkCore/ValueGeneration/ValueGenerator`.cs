// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.ValueGeneration
{
    /// <summary>
    ///     Generates values for properties when an entity is added to a context.
    /// </summary>
    public abstract class ValueGenerator<TValue> : ValueGenerator
    {
        /// <summary>
        ///     Template method to be overridden by implementations to perform value generation.
        /// </summary>
        /// <returns> The generated value. </returns>
        public new abstract TValue Next();

        /// <summary>
        ///     Gets a value to be assigned to a property.
        /// </summary>
        /// <returns> The value to be assigned to a property. </returns>
        protected override object NextValue() => Next();
    }
}

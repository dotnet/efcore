// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents operations backed by compiled delegates that support getting the value
    ///     of a mapped EF property.
    /// </summary>
    public interface IClrPropertyGetter
    {
        /// <summary>
        ///     Gets the property value.
        /// </summary>
        /// <param name="entity"> The entity instance. </param>
        /// <returns> The property value. </returns>
        object GetClrValue([NotNull] object entity);

        /// <summary>
        ///     Checks whether or not the property is set to the CLR default for its type.
        /// </summary>
        /// <param name="entity"> The entity instance. </param>
        /// <returns> <see langword="true" /> if the property value is the CLR default; <see langword="false" /> it is any other value. </returns>
        bool HasDefaultValue([NotNull] object entity);
    }
}

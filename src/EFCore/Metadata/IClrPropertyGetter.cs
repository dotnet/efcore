// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

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
        object? GetClrValue(object entity);

        /// <summary>
        ///     Checks whether or not the property is set to the CLR default for its type.
        /// </summary>
        /// <param name="entity"> The entity instance. </param>
        /// <returns> <see langword="true" /> if the property value is the CLR default; <see langword="false" /> it is any other value. </returns>
        bool HasDefaultValue(object entity);
    }
}

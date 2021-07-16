// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents operations backed by compiled delegates that support setting the value
    ///     of a mapped EF property.
    /// </summary>
    public interface IClrPropertySetter
    {
        /// <summary>
        ///     Sets the value of the property.
        /// </summary>
        /// <param name="instance"> The entity instance. </param>
        /// <param name="value"> The value to set. </param>
        void SetClrValue(object instance, object? value);
    }
}

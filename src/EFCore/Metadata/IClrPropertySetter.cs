// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

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
        void SetClrValue([NotNull] object instance, [CanBeNull] object value);
    }
}

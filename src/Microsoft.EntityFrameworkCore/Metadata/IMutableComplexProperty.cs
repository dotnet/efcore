// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IMutableComplexProperty : IComplexProperty, IMutablePropertyBase
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexTypeUsage DeclaringType { get; }
    }
}

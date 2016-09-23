// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IComplexProperty : IProperty
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IComplexTypeUsage DeclaringType { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexPropertyDefinition Definition { get; }
    }
}

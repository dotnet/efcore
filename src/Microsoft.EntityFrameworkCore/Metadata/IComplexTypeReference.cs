// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     A referene to a usage of a complex type in an entity or complex type
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IComplexTypeReference : IAccessibleProperty
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexType ReferencedComplexType { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        bool IsRequired { get; }
    }
}

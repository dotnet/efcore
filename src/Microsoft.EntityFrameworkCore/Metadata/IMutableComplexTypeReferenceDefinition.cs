// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IMutableComplexTypeReferenceDefinition : IComplexTypeReferenceDefinition, IMutablePropertyBase
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexTypeDefinition ReferencedComplexTypeDefinition { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new bool IsRequired { get; set; }
    }
}

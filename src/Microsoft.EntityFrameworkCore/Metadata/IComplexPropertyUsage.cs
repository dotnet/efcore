// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Usage of a complex property in an entity type
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IComplexPropertyUsage : IStructuralProperty, IPropertyBase
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IComplexType DeclaringType { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexTypeUsage ComplexTypeUsage { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexPropertyDefinition DefiningProperty { get; }
    }
}

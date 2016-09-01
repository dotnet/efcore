// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     A referene to a usage of a complex type in an entity or complex type
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IComplexTypeUsage : IPropertyBase
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexTypeReference DefiningReference { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexPropertyUsage FindProperty([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IEnumerable<IComplexPropertyUsage> GetProperties();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexTypeUsage FindComplexTypeUsage([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IEnumerable<IComplexTypeUsage> GetComplexTypeUsages();
    }
}

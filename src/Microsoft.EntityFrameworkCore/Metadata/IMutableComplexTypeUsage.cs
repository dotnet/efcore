// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IMutableComplexTypeUsage : IComplexTypeUsage, IMutableAccessibleProperty
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexTypeReference DefiningReference { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new bool IsRequired { get; set; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexPropertyUsage FindProperty([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IEnumerable<IMutableComplexPropertyUsage> GetProperties();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexTypeUsage FindComplexTypeUsage([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IEnumerable<IMutableComplexTypeUsage> GetComplexTypeUsages();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IComplexTypeDefinition : ITypeBase
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexPropertyDefinition FindPropertyDefinition([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IEnumerable<IComplexPropertyDefinition> GetPropertyDefinitions();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexTypeReferenceDefinition FindComplexTypeReferenceDefinition([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IEnumerable<IComplexTypeReferenceDefinition> GetComplexTypeReferenceDefinitions();
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IMutableComplexTypeDefinition : IComplexTypeDefinition, IMutableTypeBase
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexPropertyDefinition FindPropertyDefinition([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IEnumerable<IMutableComplexPropertyDefinition> GetPropertyDefinitions();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableComplexPropertyDefinition AddPropertyDefinition([NotNull] string name, [CanBeNull] Type propertyType);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableComplexPropertyDefinition RemovePropertyDefinition([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexTypeReferenceDefinition FindComplexTypeReferenceDefinition([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IEnumerable<IMutableComplexTypeReferenceDefinition> GetComplexTypeReferenceDefinitions();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableComplexTypeReferenceDefinition AddComplexTypeReferenceDefinition([NotNull] string name, [NotNull] IMutableComplexTypeDefinition referencedType);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableComplexTypeReferenceDefinition RemoveComplexTypeReferenceDefinition([NotNull] string name);
    }
}

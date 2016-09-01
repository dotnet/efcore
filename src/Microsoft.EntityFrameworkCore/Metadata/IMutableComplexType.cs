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
    public interface IMutableComplexType : IComplexType, IMutableStructuralType
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexPropertyDefinition AddProperty([NotNull] string name, [NotNull] Type propertyType, bool shadow);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexPropertyDefinition FindProperty([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IEnumerable<IMutableComplexPropertyDefinition> GetProperties();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexPropertyDefinition RemoveProperty([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexTypeReference FindComplexTypeReference([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IEnumerable<IMutableComplexTypeReference> GetComplexTypeReferences();
    }
}

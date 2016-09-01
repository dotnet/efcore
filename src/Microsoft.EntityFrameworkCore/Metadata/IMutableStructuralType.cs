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
    public interface IMutableStructuralType : IStructuralType, IMutableAnnotatable
    {
        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableModel Model { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableStructuralProperty AddProperty([NotNull] string name, [NotNull] Type propertyType, bool shadow);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableStructuralProperty FindProperty([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IEnumerable<IMutableStructuralProperty> GetProperties();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableStructuralProperty RemoveProperty([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableComplexTypeReference AddComplexTypeReference([NotNull] string name, [NotNull] IMutableComplexType complexType, bool shadow);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexTypeReference FindComplexTypeReference([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IEnumerable<IMutableComplexTypeReference> GetComplexTypeReferences();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableComplexTypeReference RemoveComplexTypeReference([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        void PropertyMetadataChanged();
    }
}

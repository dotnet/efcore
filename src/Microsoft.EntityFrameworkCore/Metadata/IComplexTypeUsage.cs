// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IComplexTypeUsage : IPropertyBase, ITypeBase
    {
        /// <summary>
        ///     Gets the entity type that this property belongs to.
        /// </summary>
        IEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        bool IsRequired { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IComplexProperty FindProperty([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IEnumerable<IComplexProperty> GetProperties();

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

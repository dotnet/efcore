// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     TODO: ComplexType docs
    /// </summary>
    public interface IMutableComplexTypeUsage : IComplexTypeUsage, IMutablePropertyBase, IMutableTypeBase
    {
        /// <summary>
        ///     Gets the entity type that this property belongs to.
        /// </summary>
        new IMutableEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new bool IsRequired { get; set; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexTypeDefinition Definition { get; }

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexProperty FindProperty([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IEnumerable<IMutableComplexProperty> GetProperties();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableComplexProperty AddProperty([NotNull] IMutableComplexPropertyDefinition propertyDefinition);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableComplexProperty RemoveProperty([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IMutableComplexTypeUsage FindComplexTypeUsage([NotNull] string name);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        new IEnumerable<IMutableComplexTypeUsage> GetComplexTypeUsages();

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableComplexTypeUsage AddComplexTypeUsage([NotNull] IMutableComplexTypeReferenceDefinition referenceDefinition);

        /// <summary>
        ///     TODO: ComplexType docs
        /// </summary>
        IMutableComplexTypeUsage RemoveComplexTypeUsage([NotNull] string name);
    }
}

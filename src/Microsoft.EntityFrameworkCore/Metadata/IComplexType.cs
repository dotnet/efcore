// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a type without identity in an <see cref="IModel" />.
    /// </summary>
    public interface IComplexType : IStructuralType
    {
        /// <summary>
        ///     Gets the property with a given name. Returns null if no property with the given name is defined.
        /// </summary>
        /// <param name="name"> The name of the property. </param>
        /// <returns> The property, or null if none is found. </returns>
        new IComplexPropertyDefinition FindProperty([NotNull] string name);

        /// <summary>
        ///     Gets the properties defined on this type.
        /// </summary>
        /// <returns> The properties defined on this type. </returns>
        new IEnumerable<IComplexPropertyDefinition> GetProperties();
    }
}

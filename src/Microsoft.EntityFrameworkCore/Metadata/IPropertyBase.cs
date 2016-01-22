// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;

namespace Microsoft.Data.Entity.Metadata
{
    /// <summary>
    ///     Base type for navigation and scalar properties.
    /// </summary>
    public interface IPropertyBase : IAnnotatable
    {
        /// <summary>
        ///     Gets the name of the property.
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Gets the type that this property belongs to.
        /// </summary>
        IEntityType DeclaringEntityType { get; }
    }
}

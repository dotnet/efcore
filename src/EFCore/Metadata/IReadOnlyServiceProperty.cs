// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

#nullable enable

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     Represents a property on an entity type that represents an
    ///     injected service from the <see cref="DbContext" />.
    /// </summary>
    public interface IReadOnlyServiceProperty : IReadOnlyPropertyBase
    {
        /// <summary>
        ///     Gets the entity type that this property belongs to.
        /// </summary>
        IReadOnlyEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     The <see cref="ServiceParameterBinding" /> for this property.
        /// </summary>
        ServiceParameterBinding? ParameterBinding { get; }
    }
}

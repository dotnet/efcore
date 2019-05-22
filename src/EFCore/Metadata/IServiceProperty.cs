// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     A <see cref="IPropertyBase" /> in the Entity Framework model that represents an
    ///     injected service from the <see cref="DbContext" />.
    /// </summary>
    public interface IServiceProperty : IPropertyBase
    {
        /// <summary>
        ///     Gets the entity type that this property belongs to.
        /// </summary>
        IEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     The <see cref="ServiceParameterBinding" /> for this property.
        /// </summary>
        ServiceParameterBinding ParameterBinding { get; }
    }
}

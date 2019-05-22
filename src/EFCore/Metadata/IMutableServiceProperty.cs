// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         A <see cref="IPropertyBase" /> in the Entity Framework model that represents an
    ///         injected service from the <see cref="DbContext" />.
    ///     </para>
    ///     <para>
    ///         This interface is used during model creation and allows the metadata to be modified.
    ///         Once the model is built, <see cref="IServiceProperty" /> represents a read-only view of the same metadata.
    ///     </para>
    /// </summary>
    public interface IMutableServiceProperty : IServiceProperty, IMutablePropertyBase
    {
        /// <summary>
        ///     Gets the type that this property belongs to.
        /// </summary>
        new IMutableEntityType DeclaringEntityType { get; }

        /// <summary>
        ///     The <see cref="ServiceParameterBinding" /> for this property.
        /// </summary>
        new ServiceParameterBinding ParameterBinding { get; [param: CanBeNull] set; }
    }
}

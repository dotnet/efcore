// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Metadata;

namespace Microsoft.EntityFrameworkCore.Update
{
    /// <summary>
    ///     <para>
    ///         Factory for creating <see cref="IUpdateAdapter"/> instances.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IUpdateAdapterFactory
    {
        /// <summary>
        ///    Creates a tracker for the given model.
        /// </summary>
        /// <param name="model"> The model for which a tracker is needed, or null to use the current model. </param>
        /// <returns> The new tracker. </returns>
        IUpdateAdapter Create([CanBeNull] IModel model = null);
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata
{
    /// <summary>
    ///     <para>
    ///         Creates instances of <see cref="IMutableModel" /> that have no conventions. This is useful when
    ///         exhaustively configuring a model based on some existing metadata.
    ///     </para>
    ///     <para>
    ///         This is typically not used in application code since building a model by overriding
    ///         <see cref="DbContext.OnModelCreating(ModelBuilder)" /> or using <see cref="ModelBuilder" />
    ///         directly is much easier.
    ///     </para>
    /// </summary>
    public class SimpleModelFactory
    {
        /// <summary>
        ///     Creates an empty model with no conventions. All aspects of the model must be exhaustively configured.
        /// </summary>
        /// <returns> The newly created model. </returns>
        public virtual IMutableModel Create()
            => new Model();
    }
}

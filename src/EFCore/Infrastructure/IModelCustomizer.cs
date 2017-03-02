// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     <para>
    ///         Performs additional configuration of the model in addition to what is discovered by convention.
    ///     </para>
    ///     <para>
    ///         This interface is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IModelCustomizer
    {
        /// <summary>
        ///     <para>
        ///         Builds the model for a given context.
        ///     </para>
        ///     <para>
        ///         If any instance data from <paramref name="dbContext" /> is
        ///         used when building the model, then the implementation of <see cref="IModelCacheKeyFactory.Create(DbContext)" />
        ///         also needs to be updated to ensure the model is cached correctly.
        ///     </para>
        /// </summary>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model.
        /// </param>
        /// <param name="dbContext">
        ///     The context instance that the model is being created for.
        /// </param>
        void Customize([NotNull] ModelBuilder modelBuilder, [NotNull] DbContext dbContext);
    }
}

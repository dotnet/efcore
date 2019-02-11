// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

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
    ///     <para>
    ///         When replacing this service consider deriving the implementation from <see cref="ModelCustomizer" /> or
    ///         'RelationalModelCustomizer' to preserve the default behavior.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Singleton"/>. This means a single instance
    ///         is used by many <see cref="DbContext"/> instances. The implementation must be thread-safe.
    ///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped"/>.
    ///     </para>
    /// </summary>
    public interface IModelCustomizer
    {
        /// <summary>
        ///     <para>
        ///         Builds the model for a given context.
        ///     </para>
        ///     <para>
        ///         If any instance data from <paramref name="context" /> is
        ///         used when building the model, then the implementation of <see cref="IModelCacheKeyFactory.Create(DbContext)" />
        ///         also needs to be updated to ensure the model is cached correctly.
        ///     </para>
        /// </summary>
        /// <param name="modelBuilder">
        ///     The builder being used to construct the model.
        /// </param>
        /// <param name="context">
        ///     The context instance that the model is being created for.
        /// </param>
        void Customize([NotNull] ModelBuilder modelBuilder, [NotNull] DbContext context);
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Infrastructure;

/// <summary>
///     <para>
///         Performs additional configuration of the model in addition to what is discovered by convention.
///     </para>
///     <para>
///         This interface is typically used by database providers (and other extensions). It is generally
///         not used in application code.
///     </para>
/// </summary>
/// <remarks>
///     <para>
///         When replacing this service consider deriving the implementation from <see cref="ModelCustomizer" /> or
///         <see cref="T:Microsoft.EntityFrameworkCore.Infrastructure.RelationalModelCustomizer" /> to preserve the default behavior.
///     </para>
///     <para>
///         The service lifetime is <see cref="ServiceLifetime.Singleton" />. This means a single instance
///         is used by many <see cref="DbContext" /> instances. The implementation must be thread-safe.
///         This service cannot depend on services registered as <see cref="ServiceLifetime.Scoped" />.
///     </para>
///     <para>
///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
///         for more information and examples.
///     </para>
/// </remarks>
public interface IModelCustomizer
{
    /// <summary>
    ///     Builds the model for a given context.
    /// </summary>
    /// <remarks>
    ///     If any instance data from <paramref name="context" /> is
    ///     used when building the model, then the implementation of <see cref="IModelCacheKeyFactory.Create(DbContext, bool)" />
    ///     also needs to be updated to ensure the model is cached correctly.
    /// </remarks>
    /// <param name="modelBuilder">
    ///     The builder being used to construct the model.
    /// </param>
    /// <param name="context">
    ///     The context instance that the model is being created for.
    /// </param>
    void Customize(ModelBuilder modelBuilder, DbContext context);
}

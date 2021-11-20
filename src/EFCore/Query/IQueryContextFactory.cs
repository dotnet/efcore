// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     Factory for <see cref="QueryContext" /> instances.
    /// </summary>
    /// <remarks>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    ///     <para>
    ///         See <see href="https://aka.ms/efcore-docs-providers">Implementation of database providers and extensions</see>
    ///         and <see href="https://aka.ms/efcore-how-queries-work">How EF Core queries work</see> for more information and examples.
    ///     </para>
    /// </remarks>
    public interface IQueryContextFactory
    {
        /// <summary>
        ///     Creates a new QueryContext.
        /// </summary>
        /// <returns>
        ///     A QueryContext instance.
        /// </returns>
        QueryContext Create();
    }
}

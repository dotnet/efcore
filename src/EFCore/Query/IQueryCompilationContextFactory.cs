// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         Factory for <see cref="QueryCompilationContext" /> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped"/>. This means that each
    ///         <see cref="DbContext"/> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IQueryCompilationContextFactory
    {
        /// <summary>
        ///     Creates a new QueryCompilationContext.
        /// </summary>
        /// <param name="async"> true if the query will be executed asynchronously. </param>
        /// <returns>
        ///     A <see cref="QueryCompilationContext" /> instance.
        /// </returns>
        QueryCompilationContext Create(bool async);
    }
}

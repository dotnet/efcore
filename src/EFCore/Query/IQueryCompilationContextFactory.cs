// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Query
{
    /// <summary>
    ///     <para>
    ///         A factory for creating <see cref="QueryCompilationContext" /> instances.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public interface IQueryCompilationContextFactory
    {
        /// <summary>
        ///     Creates a new <see cref="QueryCompilationContext" />.
        /// </summary>
        /// <param name="async"> Specifies whether the query is async. </param>
        /// <returns> The created query compilation context. </returns>
        QueryCompilationContext Create(bool async);
    }
}

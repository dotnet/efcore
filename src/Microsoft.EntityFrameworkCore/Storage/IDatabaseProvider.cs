// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;

namespace Microsoft.EntityFrameworkCore.Storage
{
    /// <summary>
    ///     <para>
    ///         The primary point where a database provider can tell EF that it has been selected for the current context
    ///         and provide the services required for it to function.
    ///     </para>
    ///     <para>
    ///         This type is typically used by database providers (and other extensions). It is generally
    ///         not used in application code.
    ///     </para>
    /// </summary>
    public interface IDatabaseProvider
    {
        /// <summary>
        ///     Gets the base set of services required by EF for the database provider to function.
        /// </summary>
        /// <param name="serviceProvider"> The service provider to resolve services from. </param>
        /// <returns> The services for this database provider. </returns>
        IDatabaseProviderServices GetProviderServices([NotNull] IServiceProvider serviceProvider);

        /// <summary>
        ///     Gets a value indicating whether this database provider has been selected for a given context.
        /// </summary>
        /// <param name="options"> The options for the context. </param>
        /// <returns> True if the database provider has been selected, otherwise false. </returns>
        bool IsConfigured([NotNull] IDbContextOptions options);
    }
}

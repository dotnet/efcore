// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Utilities;
using Microsoft.Extensions.DependencyInjection;

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
    /// <typeparam name="TProviderServices">
    ///     The base set of services required by EF for the database provider to function.
    /// </typeparam>
    /// <typeparam name="TOptionsExtension">
    ///     The type of options that the database provider will add to <see cref="DbContextOptions.Extensions" />
    ///     to identify that is has been selected (and to store its database specific settings).
    /// </typeparam>
    public class DatabaseProvider<TProviderServices, TOptionsExtension> : IDatabaseProvider
        where TProviderServices : class, IDatabaseProviderServices
        where TOptionsExtension : class, IDbContextOptionsExtension
    {
        /// <summary>
        ///     Gets the base set of services required by EF for the database provider to function.
        /// </summary>
        /// <param name="serviceProvider"> The service provider to resolve services from. </param>
        /// <returns> The services for this database provider. </returns>
        public virtual IDatabaseProviderServices GetProviderServices(IServiceProvider serviceProvider)
            => Check.NotNull(serviceProvider, nameof(serviceProvider)).GetRequiredService<TProviderServices>();

        /// <summary>
        ///     Gets a value indicating whether this database provider has been selected for a given context.
        /// </summary>
        /// <param name="options"> The options for the context. </param>
        /// <returns> True if the database provider has been selected, otherwise false. </returns>
        public virtual bool IsConfigured(IDbContextOptions options)
            => Check.NotNull(options, nameof(options)).Extensions.OfType<TOptionsExtension>().Any();
    }
}

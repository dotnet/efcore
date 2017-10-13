// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Infrastructure
{
    /// <summary>
    ///     Extension methods for setting up Entity Framework related services in an <see cref="IServiceCollection" />.
    /// </summary>
    public static class EntityFrameworkServiceCollectionExtensions
    {
        /// <summary>
        ///     This method is no longer functional. Call a provider-specific method such as
        ///     AddEntityFrameworkSqlServer, AddEntityFrameworkSqlite, etc. instead.
        /// </summary>
        /// <param name="serviceCollection"> The service collection. </param>
        /// <returns> Always throws NotSupportedException. </returns>
        /// <exception cref="System.NotSupportedException"> Always throws NotSupportedException. </exception>
        [Obsolete(
            "AddEntityFramework is no longer functional. Use AddEntityFrameworkSqlServer, AddEntityFrameworkSqlite, etc. instead.",
            error: true)]
        public static IServiceCollection AddEntityFramework([NotNull] this IServiceCollection serviceCollection)
        {
            throw new NotSupportedException(
                "AddEntityFramework is no longer functional. Use AddEntityFrameworkSqlServer, AddEntityFrameworkSqlite, etc. instead.");
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     <para>
    ///         This is an internal API that supports the Entity Framework Core infrastructure and not subject to
    ///         the same compatibility standards as public APIs. It may be changed or removed without notice in
    ///         any release. You should only use it directly in your code with extreme caution and knowing that
    ///         doing so can result in application failures when updating to a new Entity Framework Core release.
    ///     </para>
    ///     <para>
    ///         The service lifetime is <see cref="ServiceLifetime.Scoped" />. This means that each
    ///         <see cref="DbContext" /> instance will use its own instance of this service.
    ///         The implementation may depend on other services registered with any lifetime.
    ///         The implementation does not need to be thread-safe.
    ///     </para>
    /// </summary>
    public abstract class NamedConnectionStringResolverBase : INamedConnectionStringResolver
    {
        private const string DefaultSection = "ConnectionStrings:";

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        protected abstract IServiceProvider ApplicationServiceProvider { get; }

        /// <summary>
        ///     This is an internal API that supports the Entity Framework Core infrastructure and not subject to
        ///     the same compatibility standards as public APIs. It may be changed or removed without notice in
        ///     any release. You should only use it directly in your code with extreme caution and knowing that
        ///     doing so can result in application failures when updating to a new Entity Framework Core release.
        /// </summary>
        public virtual string ResolveConnectionString(string connectionString)
        {
            var connectionName = TryGetConnectionName(connectionString);

            if (connectionName == null)
            {
                return connectionString;
            }

            var configuration = ApplicationServiceProvider
                ?.GetService<IConfiguration>();

            var resolved = configuration?[connectionName]
                ?? configuration?[DefaultSection + connectionName];

            if (resolved == null)
            {
                throw new InvalidOperationException(
                    RelationalStrings.NamedConnectionStringNotFound(connectionName));
            }

            return resolved;
        }

        private static string TryGetConnectionName(string connectionString)
        {
            var firstEquals = connectionString.IndexOf('=');
            if (firstEquals < 0)
            {
                return null;
            }

            if (connectionString.IndexOf('=', firstEquals + 1) >= 0)
            {
                return null;
            }

            return connectionString.Substring(0, firstEquals).Trim().Equals(
                "name", StringComparison.OrdinalIgnoreCase)
                ? connectionString.Substring(firstEquals + 1).Trim()
                : null;
        }
    }
}

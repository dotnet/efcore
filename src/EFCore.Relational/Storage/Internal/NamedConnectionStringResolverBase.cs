// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public abstract class NamedConnectionStringResolverBase : INamedConnectionStringResolver
    {
        private const string DefaultSection = "ConnectionStrings:";

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        protected abstract IServiceProvider ApplicationServiceProvider { get; }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
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

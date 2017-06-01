// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.EntityFrameworkCore.Storage.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class NamedConnectionStringResolver : INamedConnectionStringResolver
    {
        private const string DefaultSection = "ConnectionStrings:";

        private readonly IDbContextOptions _options;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public NamedConnectionStringResolver([NotNull] IDbContextOptions options)
        {
            _options = options;
        }

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

            var configuration = _options.FindExtension<CoreOptionsExtension>()
                ?.ApplicationServiceProvider
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

            if (connectionString.Substring(0, firstEquals).Trim().Equals(
                "name", StringComparison.OrdinalIgnoreCase))
            {
                return connectionString.Substring(firstEquals + 1).Trim();
            }

            return null;
        }
    }
}

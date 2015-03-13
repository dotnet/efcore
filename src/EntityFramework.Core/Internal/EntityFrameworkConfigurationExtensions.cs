// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using JetBrains.Annotations;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Data.Entity.Internal
{
    public static class EntityFrameworkConfigurationExtensions
    {
        public static string ResolveConnectionString([CanBeNull] this IConfiguration configuration, [NotNull] string connectionString)
        {
            var firstequals = connectionString.IndexOf('=');
            if ((firstequals > 0)
                && (connectionString.IndexOf('=', firstequals + 1) < 0)
                && (connectionString.Substring(0, firstequals).Trim().Equals(
                    "name", StringComparison.OrdinalIgnoreCase)))
            {
                var redirectionKey = connectionString.Substring(firstequals + 1).Trim();
                if (configuration == null
                    || string.IsNullOrEmpty(redirectionKey)
                    || !configuration.TryGet(redirectionKey, out connectionString))
                {
                    throw new InvalidOperationException(Strings.ConnectionStringNotFound(redirectionKey));
                }
            }

            return connectionString;
        }
    }
}

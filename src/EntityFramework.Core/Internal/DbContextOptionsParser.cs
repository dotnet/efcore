// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Framework.ConfigurationModel;
using Microsoft.Framework.DependencyInjection;

namespace Microsoft.Data.Entity.Internal
{
    public class DbContextOptionsParser
    {
        private const string EntityFrameworkKey = "EntityFramework";
        private const string ConnectionStringKey = "ConnectionString";

        public static DbContextOptions<TContext> DbContextOptionsFactory<TContext>(
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] IConfiguration configuration,
            [CanBeNull] Action<DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            var parser = serviceProvider.GetRequiredService<DbContextOptionsParser>();

            var options = new DbContextOptions<TContext>(
                parser.ReadRawOptions<TContext>(configuration),
                new Dictionary<Type, IDbContextOptionsExtension>());

            if (optionsAction != null)
            {
                var builder = new DbContextOptionsBuilder<TContext>(options);
                optionsAction(builder);
                options = builder.Options;
            }

            return options;
        }

        public virtual IReadOnlyDictionary<string, string> ReadRawOptions<TContext>(
            [CanBeNull] IConfiguration configuration)
            where TContext : DbContext
        {
            var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            if (configuration != null)
            {
                ReadRawOptions(options, configuration, string.Concat(
                    EntityFrameworkKey, Constants.KeyDelimiter, typeof(TContext).Name), string.Empty);

                ReadRawOptions(options, configuration, string.Concat(
                    EntityFrameworkKey, Constants.KeyDelimiter, typeof(TContext).FullName), string.Empty);
            }

            return options;
        }

        private static void ReadRawOptions(IDictionary<string, string> options, IConfiguration configuration, string contextKey, string keyPrefix)
        {
            foreach (var pair in configuration.GetSubKeys(contextKey))
            {
                string value;
                var key = pair.Key;
                if (!pair.Value.TryGet(null, out value))
                {
                    ReadRawOptions(options, configuration,
                        string.Concat(contextKey, Constants.KeyDelimiter, key),
                        string.Concat(keyPrefix, key, Constants.KeyDelimiter));
                    continue;
                }

                if (key.Equals(ConnectionStringKey, StringComparison.OrdinalIgnoreCase))
                {
                    // Check if the value is redirection to other key
                    var firstequals = value.IndexOf('=');
                    if ((firstequals > 0)
                        && (value.IndexOf('=', firstequals + 1) < 0)
                        && (value.Substring(0, firstequals).Trim().Equals(
                            "name", StringComparison.OrdinalIgnoreCase)))
                    {
                        var redirectionKey = value.Substring(firstequals + 1).Trim();
                        if (string.IsNullOrEmpty(redirectionKey)
                            || !configuration.TryGet(redirectionKey, out value))
                        {
                            throw new InvalidOperationException(Strings.ConnectionStringNotFound(redirectionKey));
                        }
                    }
                }

                options[string.Concat(keyPrefix, key)] = value;
            }
        }
    }
}

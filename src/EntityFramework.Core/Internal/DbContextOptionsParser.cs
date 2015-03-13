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

        public static DbContextOptions<TContext> DbContextOptionsFactory<TContext>(
            [NotNull] IServiceProvider serviceProvider,
            [CanBeNull] IConfiguration configuration,
            [CanBeNull] Action<DbContextOptionsBuilder> optionsAction)
            where TContext : DbContext
        {
            var parser = serviceProvider.GetRequiredService<DbContextOptionsParser>();

            var options = new DbContextOptions<TContext>(
                parser.ReadRawOptions<TContext>(configuration),
                new Dictionary<Type, IDbContextOptionsExtension>(),
                configuration);

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

                options[string.Concat(keyPrefix, key)] = value;
            }
        }
    }
}

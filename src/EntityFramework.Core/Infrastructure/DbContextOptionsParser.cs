// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Data.Entity.Infrastructure
{
    public class DbContextOptionsParser
    {
        private const string EntityFrameworkKey = "EntityFramework";
        private const string ConnectionStringKey = "ConnectionString";

        public virtual IReadOnlyDictionary<string, string> ReadRawOptions<TContext>(
            [NotNull] IConfiguration configuration,
            [NotNull] IReadOnlyDictionary<string, string> currentOptions)
            where TContext : DbContext
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(currentOptions, "currentOptions");

            return ReadRawOptions(configuration, typeof(TContext), currentOptions);
        }

        public virtual IReadOnlyDictionary<string, string> ReadRawOptions(
            [NotNull] IConfiguration configuration,
            [NotNull] Type contextType,
            [NotNull] IReadOnlyDictionary<string, string> currentOptions)
        {
            Check.NotNull(configuration, "configuration");
            Check.NotNull(contextType, "contextType");

            var options = currentOptions.ToDictionary(i => i.Key, i => i.Value, StringComparer.OrdinalIgnoreCase);

            ReadRawOptions(options, configuration, string.Concat(
                EntityFrameworkKey, Constants.KeyDelimiter, contextType.Name), string.Empty);

            ReadRawOptions(options, configuration, string.Concat(
                EntityFrameworkKey, Constants.KeyDelimiter, contextType.FullName), string.Empty);

            return options;
        }

        private static void ReadRawOptions(
            Dictionary<string, string> options,
            IConfiguration configuration,
            string contextKey,
            string keyPrefix)
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
                    var redirectionKey = string.Empty;
                    // Check if the value is redirection to other key
                    var firstequals = value.IndexOf('=');
                    if (firstequals < 0)
                    {
                        redirectionKey = value;
                    }
                    else if ((value.IndexOf('=', firstequals + 1) < 0)
                        && (value.Substring(0, firstequals).Trim().Equals(
                          "name", StringComparison.OrdinalIgnoreCase)))
                    {
                        redirectionKey = value.Substring(firstequals + 1).Trim();
                    }

                    if (!string.IsNullOrEmpty(redirectionKey) && !configuration.TryGet(redirectionKey, out value))
                    {
                        throw new InvalidOperationException(Strings.ConnectionStringNotFound(redirectionKey));
                    }
                }

                options[string.Concat(keyPrefix, key)] = value;
            }
        }
    }
}

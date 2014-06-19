// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Relational.Utilities;
using Microsoft.Framework.ConfigurationModel;

namespace Microsoft.Data.Entity.Relational
{
    public class ConnectionStringResolver
    {
        private readonly IConfiguration _configuration;

        public ConnectionStringResolver([CanBeNull] IEnumerable<IConfiguration> configurations)
        {
            _configuration = (configurations == null ? null : configurations.FirstOrDefault());
        }

        public virtual string Resolve([NotNull] string nameOrConnectionString)
        {
            Check.NotEmpty(nameOrConnectionString, "nameOrConnectionString");

            var name = TryGetConnectionName(nameOrConnectionString);
            if (name != null)
            {
                if (_configuration == null)
                {
                    throw new InvalidOperationException(Strings.FormatNoConfigForConnection(name));
                }

                if (!_configuration.TryGet(name, out nameOrConnectionString)
                    && !_configuration.TryGet("Data:" + name + ":ConnectionString", out nameOrConnectionString))
                {
                    throw new InvalidOperationException(Strings.FormatConnectionNotFound(name));
                }
            }

            return nameOrConnectionString;
        }

        // This is the same code as is used in EF6.
        private static string TryGetConnectionName(string nameOrConnectionString)
        {
            // No '=' at all means just treat the whole string as a name
            var firstEquals = nameOrConnectionString.IndexOf('=');
            if (firstEquals < 0)
            {
                return nameOrConnectionString;
            }

            // More than one equals means treat the whole thing as a connection string
            if (nameOrConnectionString.IndexOf('=', firstEquals + 1) >= 0)
            {
                return null;
            }

            // If the keyword before the single '=' is "name" then return the name value
            if (nameOrConnectionString.Substring(0, firstEquals).Trim().Equals(
                "name", StringComparison.OrdinalIgnoreCase))
            {
                return nameOrConnectionString.Substring(firstEquals + 1).Trim();
            }

            return null;
        }
    }
}

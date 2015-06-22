// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class DatabaseProviderSelector : IDatabaseProviderSelector
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContextOptions _contextOptions;
        private readonly IDatabaseProvider[] _sources;

        public DatabaseProviderSelector(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IDbContextOptions contextOptions,
            [CanBeNull] IEnumerable<IDatabaseProvider> sources)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));
            Check.NotNull(contextOptions, nameof(contextOptions));

            _serviceProvider = serviceProvider;
            _contextOptions = contextOptions;
            _sources = sources?.ToArray() ?? new IDatabaseProvider[0];
        }

        public virtual IDatabaseProviderServices SelectServices(ServiceProviderSource providerSource)
        {
            Check.IsDefined(providerSource, nameof(providerSource));

            var configured = _sources.Where(f => f.IsConfigured(_contextOptions)).ToArray();

            if (configured.Length == 1)
            {
                return configured[0].GetProviderServices(_serviceProvider);
            }

            if (configured.Length > 1)
            {
                throw new InvalidOperationException(Strings.MultipleProvidersConfigured(BuildDatabaseNamesString(configured)));
            }

            if (_sources.Length == 0)
            {
                if (providerSource == ServiceProviderSource.Implicit)
                {
                    throw new InvalidOperationException(Strings.NoProviderConfigured);
                }
                throw new InvalidOperationException(Strings.NoProviderServices);
            }

            if (_sources.Length > 1)
            {
                throw new InvalidOperationException(Strings.MultipleProvidersAvailable(BuildDatabaseNamesString(_sources)));
            }

            throw new InvalidOperationException(Strings.NoProviderConfigured);
        }

        private static string BuildDatabaseNamesString(IEnumerable<IDatabaseProvider> available)
            => available.Select(e => e.Name).Aggregate("", (n, c) => n + "'" + c + "' ");
    }
}

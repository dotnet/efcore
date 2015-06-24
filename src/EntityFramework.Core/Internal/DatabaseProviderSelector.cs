// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.Internal
{
    public class DatabaseProviderSelector : IDatabaseProviderSelector
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContextOptions _contextOptions;
        private readonly IDatabaseProvider[] _providers;

        public DatabaseProviderSelector(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IDbContextOptions contextOptions,
            [CanBeNull] IEnumerable<IDatabaseProvider> providers)
        {
            _serviceProvider = serviceProvider;
            _contextOptions = contextOptions;
            _providers = providers?.ToArray() ?? new IDatabaseProvider[0];
        }

        public virtual IDatabaseProviderServices SelectServices(ServiceProviderSource providerSource)
        {
            var configured = _providers.Where(f => f.IsConfigured(_contextOptions)).ToArray();

            if (configured.Length == 1)
            {
                return configured[0].GetProviderServices(_serviceProvider);
            }

            if (configured.Length > 1)
            {
                throw new InvalidOperationException(Strings.MultipleProvidersConfigured(BuildDatabaseNamesString(configured)));
            }

            if (_providers.Length == 0)
            {
                if (providerSource == ServiceProviderSource.Implicit)
                {
                    throw new InvalidOperationException(Strings.NoProviderConfigured);
                }
                throw new InvalidOperationException(Strings.NoProviderServices);
            }

            if (_providers.Length > 1)
            {
                throw new InvalidOperationException(Strings.MultipleProvidersAvailable(BuildDatabaseNamesString(_providers)));
            }

            throw new InvalidOperationException(Strings.NoProviderConfigured);
        }

        private string BuildDatabaseNamesString(IEnumerable<IDatabaseProvider> available)
            => available.Select(e => e.GetProviderServices(_serviceProvider).InvariantName).Aggregate("", (n, c) => n + "'" + c + "' ");
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;

namespace Microsoft.EntityFrameworkCore.Internal
{
    /// <summary>
    ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
    ///     directly from your code. This API may change or be removed in future releases.
    /// </summary>
    public class DatabaseProviderSelector : IDatabaseProviderSelector
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContextOptions _contextOptions;
        private readonly IDatabaseProvider[] _providers;

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public DatabaseProviderSelector(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IDbContextOptions contextOptions,
            [CanBeNull] IEnumerable<IDatabaseProvider> providers)
        {
            _serviceProvider = serviceProvider;
            _contextOptions = contextOptions;
            _providers = providers?.ToArray() ?? new IDatabaseProvider[0];
        }

        /// <summary>
        ///     This API supports the Entity Framework Core infrastructure and is not intended to be used
        ///     directly from your code. This API may change or be removed in future releases.
        /// </summary>
        public virtual IDatabaseProviderServices SelectServices()
        {
            var configured = _providers.Where(f => f.IsConfigured(_contextOptions)).ToArray();

            if (configured.Length == 1)
            {
                return configured[0].GetProviderServices(_serviceProvider);
            }

            if (configured.Length > 1)
            {
                throw new InvalidOperationException(CoreStrings.MultipleProvidersConfigured(BuildDatabaseNamesString(configured)));
            }

            if (_providers.Length > 1)
            {
                throw new InvalidOperationException(CoreStrings.MultipleProvidersAvailable(BuildDatabaseNamesString(_providers)));
            }

            throw new InvalidOperationException(CoreStrings.NoProviderConfigured);
        }

        private string BuildDatabaseNamesString(IEnumerable<IDatabaseProvider> available)
            => available.Select(e => e.GetProviderServices(_serviceProvider).InvariantName).Aggregate("", (n, c) => n + "'" + c + "' ");
    }
}

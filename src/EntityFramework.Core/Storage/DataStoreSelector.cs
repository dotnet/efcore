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
    public class DataStoreSelector : IDataStoreSelector
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly IDbContextOptions _contextOptions;
        private readonly IDataStoreSource[] _sources;

        public DataStoreSelector(
            [NotNull] IServiceProvider serviceProvider,
            [NotNull] IDbContextOptions contextOptions,
            [CanBeNull] IEnumerable<IDataStoreSource> sources)
        {
            Check.NotNull(serviceProvider, nameof(serviceProvider));
            Check.NotNull(contextOptions, nameof(contextOptions));

            _serviceProvider = serviceProvider;
            _contextOptions = contextOptions;
            _sources = sources == null ? new IDataStoreSource[0] : sources.ToArray();
        }

        public virtual IDataStoreServices SelectDataStore(ServiceProviderSource providerSource)
        {
            Check.IsDefined(providerSource, nameof(providerSource));

            var configured = _sources.Where(f => f.IsConfigured(_contextOptions)).ToArray();

            if (configured.Length == 1)
            {
                return configured[0].GetStoreServices(_serviceProvider);
            }

            if (configured.Length > 1)
            {
                throw new InvalidOperationException(Strings.MultipleDataStoresConfigured(BuildStoreNamesString(configured)));
            }

            if (_sources.Length == 0)
            {
                if (providerSource == ServiceProviderSource.Implicit)
                {
                    throw new InvalidOperationException(Strings.NoDataStoreConfigured);
                }
                throw new InvalidOperationException(Strings.NoDataStoreService);
            }

            if (_sources.Length > 1)
            {
                throw new InvalidOperationException(Strings.MultipleDataStoresAvailable(BuildStoreNamesString(_sources)));
            }

            throw new InvalidOperationException(Strings.NoDataStoreConfigured);
        }

        private static string BuildStoreNamesString(IEnumerable<IDataStoreSource> available)
            => available.Select(e => e.Name).Aggregate("", (n, c) => n + "'" + c + "' ");
    }
}

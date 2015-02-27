// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public class DataStoreSelector : IDataStoreSelector
    {
        private readonly IDataStoreSource[] _sources;

        public DataStoreSelector([CanBeNull] IEnumerable<IDataStoreSource> sources)
        {
            _sources = sources == null ? new IDataStoreSource[0] : sources.ToArray();
        }

        public virtual IDataStoreServices SelectDataStore(DbContextServices.ServiceProviderSource providerSource)
        {
            Check.IsDefined(providerSource, nameof(providerSource));

            var configured = _sources.Where(f => f.IsConfigured).ToArray();

            if (configured.Length == 1)
            {
                return configured[0].StoreServices;
            }

            if (configured.Length > 1)
            {
                throw new InvalidOperationException(Strings.MultipleDataStoresConfigured(BuildStoreNamesString(configured)));
            }

            if (_sources.Length == 0)
            {
                if (providerSource == DbContextServices.ServiceProviderSource.Implicit)
                {
                    throw new InvalidOperationException(Strings.NoDataStoreConfigured);
                }
                throw new InvalidOperationException(Strings.NoDataStoreService);
            }

            if (_sources.Length > 1)
            {
                throw new InvalidOperationException(Strings.MultipleDataStoresAvailable(BuildStoreNamesString(_sources)));
            }

            _sources[0].AutoConfigure();

            if (!_sources[0].IsAvailable)
            {
                throw new InvalidOperationException(Strings.NoDataStoreConfigured);
            }

            return _sources[0].StoreServices;
        }

        private static string BuildStoreNamesString(IEnumerable<IDataStoreSource> available) 
            => available.Select(e => e.Name).Aggregate("", (n, c) => n + "'" + c + "' ");
    }
}

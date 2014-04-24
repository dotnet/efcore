// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public class DataStoreSelector
    {
        private readonly DataStoreSource[] _factories;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DataStoreSelector()
        {
        }

        public DataStoreSelector([CanBeNull] IEnumerable<DataStoreSource> factories)
        {
            _factories = factories == null ? new DataStoreSource[0] : factories.ToArray();
        }

        public virtual DataStore SelectDataStore([NotNull] ContextConfiguration configuration)
        {
            var configured = _factories.Where(f => f.IsConfigured(configuration)).ToArray();

            if (configured.Length == 1)
            {
                return configured[0].GetDataStore(configuration);
            }

            if (configured.Length > 1)
            {
                throw new InvalidOperationException(Strings.FormatMultipleDataStoresConfigured(BuildStoreNamesString(configured)));
            }

            if (_factories.Length == 0)
            {
                if (configuration.ProviderSource == ContextConfiguration.ServiceProviderSource.Implicit)
                {
                    throw new InvalidOperationException(Strings.NoDataStoreConfigured);
                }
                throw new InvalidOperationException(Strings.NoDataStoreService);
            }

            if (_factories.Length > 1)
            {
                throw new InvalidOperationException(Strings.FormatMultipleDataStoresAvailable(BuildStoreNamesString(_factories)));
            }

            if (!_factories[0].IsAvailable(configuration))
            {
                throw new InvalidOperationException(Strings.NoDataStoreConfigured);
            }

            return _factories[0].GetDataStore(configuration);
        }

        private static string BuildStoreNamesString(IEnumerable<DataStoreSource> available)
        {
            return available.Select(e => e.Name).Aggregate("", (n, c) => n + "'" + c + "' ");
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Collections.Generic;
using System.Linq;
using JetBrains.Annotations;

namespace Microsoft.Data.Entity.Storage
{
    public class DataStoreSelector
    {
        private readonly DataStoreSource[] _sources;

        /// <summary>
        ///     This constructor is intended only for use when creating test doubles that will override members
        ///     with mocked or faked behavior. Use of this constructor for other purposes may result in unexpected
        ///     behavior including but not limited to throwing <see cref="NullReferenceException" />.
        /// </summary>
        protected DataStoreSelector()
        {
        }

        public DataStoreSelector([CanBeNull] IEnumerable<DataStoreSource> sources)
        {
            _sources = sources == null ? new DataStoreSource[0] : sources.ToArray();
        }

        public virtual DataStoreSource SelectDataStore([NotNull] ContextConfiguration configuration)
        {
            var configured = _sources.Where(f => f.IsConfigured(configuration)).ToArray();

            if (configured.Length == 1)
            {
                return configured[0];
            }

            if (configured.Length > 1)
            {
                throw new InvalidOperationException(Strings.FormatMultipleDataStoresConfigured(BuildStoreNamesString(configured)));
            }

            if (_sources.Length == 0)
            {
                if (configuration.ProviderSource == ContextConfiguration.ServiceProviderSource.Implicit)
                {
                    throw new InvalidOperationException(Strings.NoDataStoreConfigured);
                }
                throw new InvalidOperationException(Strings.NoDataStoreService);
            }

            if (_sources.Length > 1)
            {
                throw new InvalidOperationException(Strings.FormatMultipleDataStoresAvailable(BuildStoreNamesString(_sources)));
            }

            if (!_sources[0].IsAvailable(configuration))
            {
                throw new InvalidOperationException(Strings.NoDataStoreConfigured);
            }

            return _sources[0];
        }

        private static string BuildStoreNamesString(IEnumerable<DataStoreSource> available)
        {
            return available.Select(e => e.Name).Aggregate("", (n, c) => n + "'" + c + "' ");
        }
    }
}

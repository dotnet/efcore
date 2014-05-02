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

using System.Threading;
using System.Threading.Tasks;
using JetBrains.Annotations;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity
{
    public class Database
    {
        private readonly ContextConfiguration _configuration;

        public Database([NotNull] ContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            _configuration = configuration;
        }

        public virtual DataStoreConnection Connection
        {
            get { return _configuration.Connection; }
        }

        public virtual void Create()
        {
            _configuration.DataStoreCreator.Create(_configuration.Model);
        }

        public virtual void Delete()
        {
            _configuration.DataStoreCreator.Delete();
        }

        public virtual bool Exists()
        {
            return _configuration.DataStoreCreator.Exists();
        }

        public virtual Task CreateAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _configuration.DataStoreCreator.CreateAsync(_configuration.Model, cancellationToken);
        }

        public virtual Task DeleteAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _configuration.DataStoreCreator.DeleteAsync(cancellationToken);
        }

        public virtual Task<bool> ExistsAsync(CancellationToken cancellationToken = default(CancellationToken))
        {
            return _configuration.DataStoreCreator.ExistsAsync(cancellationToken);
        }
    }
}

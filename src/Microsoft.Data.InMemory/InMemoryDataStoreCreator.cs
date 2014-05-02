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
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public class InMemoryDataStoreCreator : DataStoreCreator
    {
        private readonly InMemoryDataStore _dataStore;

        public InMemoryDataStoreCreator([NotNull] InMemoryDataStore dataStore)
        {
            Check.NotNull(dataStore, "dataStore");

            _dataStore = dataStore;
        }

        public override Task CreateAsync(IModel model, CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(0);
        }

        public override Task<bool> ExistsAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            return Task.FromResult(true);
        }

        public override Task DeleteAsync(CancellationToken cancellationToken = new CancellationToken())
        {
            _dataStore.Database.Clear();

            return Task.FromResult(0);
        }

        public override void Create(IModel model)
        {
        }

        public override bool Exists()
        {
            return true;
        }

        public override void Delete()
        {
            _dataStore.Database.Clear();
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.AzureTableStorage
{
    public class AtsDataStoreCreator : DataStoreCreator
    {
        public override bool EnsureDeleted(IModel model)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> EnsureDeletedAsync(IModel model, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }

        public override bool EnsureCreated(IModel model)
        {
            throw new NotImplementedException();
        }

        public override Task<bool> EnsureCreatedAsync(IModel model, CancellationToken cancellationToken = new CancellationToken())
        {
            throw new NotImplementedException();
        }
    }
}

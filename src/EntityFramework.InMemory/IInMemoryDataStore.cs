// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using JetBrains.Annotations;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Storage;

namespace Microsoft.Data.Entity.InMemory
{
    public interface IInMemoryDataStore : IDataStore
    {
        InMemoryDatabase Database { get; }
        bool EnsureDatabaseCreated([NotNull] IModel model);
    }
}

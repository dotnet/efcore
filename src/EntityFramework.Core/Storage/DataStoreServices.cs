// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Microsoft.Data.Entity.Metadata.Builders;
using Microsoft.Data.Entity.Query;
using Microsoft.Data.Entity.ValueGeneration;

namespace Microsoft.Data.Entity.Storage
{
    public interface IDataStoreServices
    {
        IDataStore Store { get; }
        IDataStoreCreator Creator { get; }
        IDataStoreConnection Connection { get; }
        IValueGeneratorSelector ValueGeneratorSelector { get; }
        IDatabaseFactory DatabaseFactory { get; }
        IModelBuilderFactory ModelBuilderFactory { get; }
        IModelSource ModelSource { get; }
        IQueryContextFactory QueryContextFactory { get; }
    }
}

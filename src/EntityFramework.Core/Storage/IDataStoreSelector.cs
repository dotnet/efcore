// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Internal;

namespace Microsoft.Data.Entity.Storage
{
    public interface IDataStoreSelector
    {
        IDataStoreServices SelectDataStore(DbContextServices.ServiceProviderSource providerSource);
    }
}

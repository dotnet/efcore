// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public class InMemoryDataStoreSource
        : DataStoreSource<InMemoryDataStore, InMemoryConfigurationExtension, InMemoryDataStoreCreator, InMemoryConnection>
    {
        public override bool IsAvailable(ContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return true;
        }

        public override string Name
        {
            get { return typeof(InMemoryDataStore).Name; }
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity;
using Microsoft.Data.Entity.Storage;
using Microsoft.Data.InMemory.Utilities;

namespace Microsoft.Data.InMemory
{
    public class InMemoryDataStoreSource : DataStoreSource
    {
        public override DataStore GetDataStore(ContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Use GetRequiredService, by sharing source if possible
            return configuration.Services.ServiceProvider.GetService<InMemoryDataStore>();
        }

        public override bool IsConfigured(ContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.Annotations.HasAnnotations(typeof(InMemoryDataStore))
                   && configuration.Annotations[typeof(InMemoryDataStore)][InMemoryDataStore.ModeKey] != null;
        }

        public override bool IsAvailable(ContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return true;
        }
    }
}

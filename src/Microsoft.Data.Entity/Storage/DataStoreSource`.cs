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

using System.Linq;
using Microsoft.AspNet.DependencyInjection;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Utilities;

namespace Microsoft.Data.Entity.Storage
{
    public abstract class DataStoreSource<TDataStore, TConfiguration, TCreator, TConnection> : DataStoreSource
        where TDataStore : DataStore
        where TConfiguration : EntityConfigurationExtension
        where TCreator : DataStoreCreator
        where TConnection : DataStoreConnection
    {
        public override DataStore GetStore(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Use GetRequiredService, by sharing source if possible
            return configuration.Services.ServiceProvider.GetService<TDataStore>();
        }

        public override DataStoreCreator GetCreator(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Use GetRequiredService, by sharing source if possible
            return configuration.Services.ServiceProvider.GetService<TCreator>();
        }

        public override DataStoreConnection GetConnection(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            // TODO: Use GetRequiredService, by sharing source if possible
            return configuration.Services.ServiceProvider.GetService<TConnection>();
        }

        public override bool IsConfigured(DbContextConfiguration configuration)
        {
            Check.NotNull(configuration, "configuration");

            return configuration.ContextOptions.Extensions.OfType<TConfiguration>().Any();
        }
    }
}

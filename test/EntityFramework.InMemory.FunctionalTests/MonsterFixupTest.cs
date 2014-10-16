// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels;
using Microsoft.Framework.DependencyInjection;
using Microsoft.Framework.DependencyInjection.Advanced;
using Microsoft.Framework.DependencyInjection.Fallback;
using Microsoft.Framework.Logging;

namespace Microsoft.Data.Entity.InMemory.FunctionalTests
{
    public class MonsterFixupTest : MonsterFixupTestBase
    {
        protected override IServiceProvider CreateServiceProvider(bool throwingStateManager = false)
        {
            var serviceCollection = new ServiceCollection().AddEntityFramework().AddInMemoryStore().UseLoggerFactory<LoggerFactory>().ServiceCollection;

            if (throwingStateManager)
            {
                serviceCollection.AddScoped<StateManager, ThrowingMonsterStateManager>();
            }

            return serviceCollection.BuildServiceProvider();
        }

        protected override DbContextOptions CreateOptions(string databaseName)
        {
            return new DbContextOptions().UseInMemoryStore();
        }

        protected override Task CreateAndSeedDatabase(string databaseName, Func<MonsterContext> createContext)
        {
            using (var context = createContext())
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
                context.SeedUsingFKs();
            }

            return Task.FromResult(0);
        }

        // TODO: Temporary means to disable use of candidate keys on SQL Server. See GitHub #537
        protected override bool SupportsCandidateKeys
        {
            get { return true; }
        }
    }
}

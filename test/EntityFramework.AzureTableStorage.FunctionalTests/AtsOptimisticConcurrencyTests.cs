// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.AzureTableStorage.FunctionalTests.TestModels;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.FunctionalTests;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class AtsOptimisticConcurrencyTests : OptimisticConcurrencyTestBase<AtsTestStore>
    {
        public override Task<AtsTestStore> CreateTestStoreAsync()
        {
            return AtsF1Context.CreateMutableTestStoreAsync();
        }

        public override F1Context CreateF1Context(AtsTestStore testStore)
        {
            return AtsF1Context.Create(testStore);
        }

        protected override void ResolveConcurrencyTokens(StateEntry stateEntry)
        {
            var property = stateEntry.EntityType.GetProperty("ETag");
            stateEntry[property] = "*";
            //TODO use the actual ETag instead of force rewrite. This will require refactoring the test base to read shadow state properties
        }
    }
}

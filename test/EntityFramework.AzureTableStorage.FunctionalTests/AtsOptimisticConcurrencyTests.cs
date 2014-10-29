// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.FunctionalTests;

namespace Microsoft.Data.Entity.AzureTableStorage.FunctionalTests
{
    [RunIfConfigured]
    public class AtsOptimisticConcurrencyTests : OptimisticConcurrencyTestBase<AtsTestStore, AtsF1Fixture>
    {
        public AtsOptimisticConcurrencyTests(AtsF1Fixture fixture)
            : base(fixture)
        {
        }

        protected override void ResolveConcurrencyTokens(StateEntry stateEntry)
        {
            var property = stateEntry.EntityType.GetProperty("ETag");
            stateEntry[property] = "*";
            //TODO use the actual ETag instead of force rewrite. This will require refactoring the test base to read shadow state properties
        }
    }
}

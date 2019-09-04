// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public class SpatialInMemoryTest : SpatialTestBase<SpatialInMemoryFixture>
    {
        public SpatialInMemoryTest(SpatialInMemoryFixture fixture)
            : base(fixture)
        {
        }

        [ConditionalFact(Skip = "Issue#14042")]
        public override void Mutation_of_tracked_values_does_not_mutate_values_in_store()
        {
            base.Mutation_of_tracked_values_does_not_mutate_values_in_store();
        }

        protected override void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }
    }
}

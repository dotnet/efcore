// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public class ManyToManyTrackingInMemoryTest
        : ManyToManyTrackingTestBase<ManyToManyTrackingInMemoryTest.ManyToManyTrackingInMemoryFixture>
    {
        public ManyToManyTrackingInMemoryTest(ManyToManyTrackingInMemoryFixture fixture)
            : base(fixture)
        {
        }

        protected override void ExecuteWithStrategyInTransaction(
            Action<ManyToManyContext> testOperation,
            Action<ManyToManyContext> nestedTestOperation1 = null,
            Action<ManyToManyContext> nestedTestOperation2 = null,
            Action<ManyToManyContext> nestedTestOperation3 = null)
        {
            base.ExecuteWithStrategyInTransaction(testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);
            Fixture.Reseed();
        }

        protected override bool SupportsDatabaseDefaults
            => false;

        public class ManyToManyTrackingInMemoryFixture : ManyToManyTrackingFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => InMemoryTestStoreFactory.Instance;

            public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
                => base.AddOptions(builder)
                    .ConfigureWarnings(w => w.Log(InMemoryEventId.TransactionIgnoredWarning))
                    .UseQueryTrackingBehavior(QueryTrackingBehavior.TrackAll);
        }
    }
}

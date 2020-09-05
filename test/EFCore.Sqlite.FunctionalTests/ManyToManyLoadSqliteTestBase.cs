// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Collections.Generic;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class ManyToManyLoadSqliteTestBase<TFixture> : ManyToManyLoadTestBase<TFixture>
        where TFixture : ManyToManyLoadSqliteTestBase<TFixture>.ManyToManyLoadSqliteFixtureBase
    {
        protected ManyToManyLoadSqliteTestBase(TFixture fixture)
            : base(fixture)
        {
        }

        public class ManyToManyLoadSqliteFixtureBase : ManyToManyLoadFixtureBase
        {
            protected override ITestStoreFactory TestStoreFactory
                => SqliteTestStoreFactory.Instance;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder
                    .Entity<JoinOneSelfPayload>()
                    .Property(e => e.Payload)
                    .HasDefaultValueSql("CURRENT_TIMESTAMP");

                modelBuilder
                    .SharedTypeEntity<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                    .IndexerProperty<string>("Payload")
                    .HasDefaultValue("Generated");

                modelBuilder
                    .Entity<JoinOneToThreePayloadFull>()
                    .Property(e => e.Payload)
                    .HasDefaultValue("Generated");
            }
        }
    }
}

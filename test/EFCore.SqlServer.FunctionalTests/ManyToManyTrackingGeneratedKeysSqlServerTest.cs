// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore
{
    public class ManyToManyTrackingGeneratedKeysSqlServerTest
        : ManyToManyTrackingSqlServerTestBase<ManyToManyTrackingGeneratedKeysSqlServerTest.ManyToManyTrackingGeneratedKeysSqlServerFixture>
    {
        public ManyToManyTrackingGeneratedKeysSqlServerTest(ManyToManyTrackingGeneratedKeysSqlServerFixture fixture)
            : base(fixture)
        {
        }

        public class ManyToManyTrackingGeneratedKeysSqlServerFixture : ManyToManyTrackingSqlServerFixtureBase
        {
            protected override string StoreName { get; } = "ManyToManyTrackingGeneratedKeys";

            public override bool UseGeneratedKeys
                => true;

            protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
            {
                base.OnModelCreating(modelBuilder, context);

                modelBuilder.Entity<EntityOne>().Property(e => e.Id).ValueGeneratedOnAdd();
                modelBuilder.Entity<EntityTwo>().Property(e => e.Id).ValueGeneratedOnAdd();
                modelBuilder.Entity<EntityThree>().Property(e => e.Id).ValueGeneratedOnAdd();
                modelBuilder.Entity<EntityCompositeKey>().Property(e => e.Key1).ValueGeneratedOnAdd();
                modelBuilder.Entity<EntityRoot>().Property(e => e.Id).ValueGeneratedOnAdd();
                modelBuilder.SharedTypeEntity<ProxyableSharedType>("PST").IndexerProperty<int>("Id").ValueGeneratedOnAdd();
                modelBuilder.Entity<ImplicitManyToManyA>().Property(e => e.Id).ValueGeneratedOnAdd();
                modelBuilder.Entity<ImplicitManyToManyB>().Property(e => e.Id).ValueGeneratedOnAdd();
            }
        }
    }
}

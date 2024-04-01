// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class ManyToManyTrackingGeneratedKeysSqlServerTest(ManyToManyTrackingGeneratedKeysSqlServerTest.ManyToManyTrackingGeneratedKeysSqlServerFixture fixture)
    : ManyToManyTrackingSqlServerTestBase<ManyToManyTrackingGeneratedKeysSqlServerTest.ManyToManyTrackingGeneratedKeysSqlServerFixture>(fixture)
{
    public class ManyToManyTrackingGeneratedKeysSqlServerFixture : ManyToManyTrackingSqlServerFixtureBase
    {
        protected override string StoreName
            => "ManyToManyTrackingGeneratedKeys";

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
            modelBuilder.Entity<EntityTableSharing1>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<EntityTableSharing2>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.SharedTypeEntity<ProxyableSharedType>("PST").IndexerProperty<int>("Id").ValueGeneratedOnAdd();
            modelBuilder.Entity<ImplicitManyToManyA>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<ImplicitManyToManyB>().Property(e => e.Id).ValueGeneratedOnAdd();

            modelBuilder.Entity<UnidirectionalEntityOne>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UnidirectionalEntityTwo>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UnidirectionalEntityThree>().Property(e => e.Id).ValueGeneratedOnAdd();
            modelBuilder.Entity<UnidirectionalEntityCompositeKey>().Property(e => e.Key1).ValueGeneratedOnAdd();
            modelBuilder.Entity<UnidirectionalEntityRoot>().Property(e => e.Id).ValueGeneratedOnAdd();
        }
    }
}

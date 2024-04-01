// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public class TpcManyToManyTrackingSqlServerTest(TpcManyToManyTrackingSqlServerTest.TpcManyToManyTrackingSqlServerFixture fixture)
    : ManyToManyTrackingSqlServerTestBase<TpcManyToManyTrackingSqlServerTest.TpcManyToManyTrackingSqlServerFixture>(fixture)
{
    public class TpcManyToManyTrackingSqlServerFixture : ManyToManyTrackingSqlServerFixtureBase
    {
        protected override string StoreName
            => "TpcManyToManyTrackingSqlServerTest";

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                w => w.Log(RelationalEventId.ForeignKeyTpcPrincipalWarning));

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<EntityRoot>().UseTpcMappingStrategy();
            modelBuilder.Entity<EntityRoot>().ToTable("Roots");
            modelBuilder.Entity<EntityBranch>().ToTable("Branches");
            modelBuilder.Entity<EntityLeaf>().ToTable("Leaves");
            modelBuilder.Entity<EntityBranch2>().ToTable("Branch2s");
            modelBuilder.Entity<EntityLeaf2>().ToTable("Leaf2s");

            modelBuilder.Entity<UnidirectionalEntityRoot>().UseTpcMappingStrategy();
            modelBuilder.Entity<UnidirectionalEntityRoot>().ToTable("UnidirectionalRoots");
            modelBuilder.Entity<UnidirectionalEntityBranch>().ToTable("UnidirectionalBranches");
            modelBuilder.Entity<UnidirectionalEntityLeaf>().ToTable("UnidirectionalLeaves");
        }
    }
}

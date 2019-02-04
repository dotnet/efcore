// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsWeakQueryRelationalFixtureBase : ComplexNavigationsWeakQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory => (TestSqlLoggerFactory)ListLoggerFactory;

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            base.OnModelCreating(modelBuilder, context);

            modelBuilder.Entity<Level1>(eb => eb.ToTable(nameof(Level1)));
        }

        protected override void Configure(OwnedNavigationBuilder<Level1, Level2> l2)
        {
            base.Configure(l2);

            l2.ToTable(nameof(Level1));
            l2.Property(l => l.Date).HasColumnName("OneToOne_Required_PK_Date");
        }

        protected override void Configure(OwnedNavigationBuilder<Level2, Level3> l3)
        {
            base.Configure(l3);

            l3.ToTable(nameof(Level1));
        }

        protected override void Configure(OwnedNavigationBuilder<Level3, Level4> l4)
        {
            base.Configure(l4);

            l4.ToTable(nameof(Level1));
        }

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                c => c.Log(RelationalEventId.QueryClientEvaluationWarning));
    }
}

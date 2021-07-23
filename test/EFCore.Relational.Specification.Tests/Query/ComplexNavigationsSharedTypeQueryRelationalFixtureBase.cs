// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsSharedTypeQueryRelationalFixtureBase : ComplexNavigationsSharedTypeQueryFixtureBase
    {
        public TestSqlLoggerFactory TestSqlLoggerFactory
            => (TestSqlLoggerFactory)ListLoggerFactory;

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
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsOwnedQueryRelationalFixtureBase<TTestStore> : ComplexNavigationsOwnedQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Level1>(eb =>
                {
                    eb.ToTable(nameof(Level1));
                });
            
        }

        protected override void Configure(ReferenceOwnershipBuilder<Level1, Level2> l2)
        {
            base.Configure(l2);
            
            l2.ToTable(nameof(Level1));
            l2.Property(l => l.Date).HasColumnName("OneToOne_Required_PK_Date");
        }

        protected override void Configure(ReferenceOwnershipBuilder<Level2, Level3> l3)
        {
            base.Configure(l3);
            
            l3.ToTable(nameof(Level1));
        }

        protected override void Configure(ReferenceOwnershipBuilder<Level3, Level4> l4)
        {
            base.Configure(l4);
            
            l4.ToTable(nameof(Level1));
        }
    }
}

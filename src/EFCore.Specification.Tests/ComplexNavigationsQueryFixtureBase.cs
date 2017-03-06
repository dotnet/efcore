// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Specification.Tests
{
    public abstract class ComplexNavigationsQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        public abstract TTestStore CreateTestStore();

        public abstract ComplexNavigationsContext CreateContext(TTestStore testStore);

        protected virtual void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Level1>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Level2>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Level3>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Level4>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_Self).WithOne();
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_PK).WithOne(e => e.OneToOne_Required_PK_Inverse).HasPrincipalKey<Level1>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_PK).WithOne(e => e.OneToOne_Optional_PK_Inverse).HasPrincipalKey<Level1>(e => e.Id).IsRequired(false);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_FK).WithOne(e => e.OneToOne_Required_FK_Inverse).HasForeignKey<Level2>(e => e.Level1_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).HasForeignKey<Level2>(e => e.Level1_Optional_Id).IsRequired(false);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Required).WithOne(e => e.OneToMany_Required_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional).WithOne(e => e.OneToMany_Optional_Inverse).IsRequired(false);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Required_Self).WithOne(e => e.OneToMany_Required_Self_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).IsRequired(false);

            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_Self).WithOne();
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_PK).WithOne(e => e.OneToOne_Required_PK_Inverse).HasPrincipalKey<Level2>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_PK).WithOne(e => e.OneToOne_Optional_PK_Inverse).HasPrincipalKey<Level2>(e => e.Id).IsRequired(false);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_FK).WithOne(e => e.OneToOne_Required_FK_Inverse).HasForeignKey<Level3>(e => e.Level2_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).HasForeignKey<Level3>(e => e.Level2_Optional_Id).IsRequired(false);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_FK).WithOne(e => e.OneToOne_Required_FK_Inverse).HasForeignKey<Level3>(e => e.Level2_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).HasForeignKey<Level3>(e => e.Level2_Optional_Id).IsRequired(false);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Required).WithOne(e => e.OneToMany_Required_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Optional).WithOne(e => e.OneToMany_Optional_Inverse).IsRequired(false);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Required_Self).WithOne(e => e.OneToMany_Required_Self_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).IsRequired(false);

            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_Self).WithOne();
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Required_PK).WithOne(e => e.OneToOne_Required_PK_Inverse).HasPrincipalKey<Level3>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_PK).WithOne(e => e.OneToOne_Optional_PK_Inverse).HasPrincipalKey<Level3>(e => e.Id).IsRequired(false);
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Required_FK).WithOne(e => e.OneToOne_Required_FK_Inverse).HasForeignKey<Level4>(e => e.Level3_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).HasForeignKey<Level4>(e => e.Level3_Optional_Id).IsRequired(false);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Required).WithOne(e => e.OneToMany_Required_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Optional).WithOne(e => e.OneToMany_Optional_Inverse).IsRequired(false);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Required_Self).WithOne(e => e.OneToMany_Required_Self_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).IsRequired(false);

            modelBuilder.Entity<Level4>().HasOne(e => e.OneToOne_Optional_Self).WithOne();
            modelBuilder.Entity<Level4>().HasMany(e => e.OneToMany_Required_Self).WithOne(e => e.OneToMany_Required_Self_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level4>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).IsRequired(false);

            modelBuilder.Entity<ComplexNavigationField>().HasKey(e => e.Name);
            modelBuilder.Entity<ComplexNavigationString>().HasKey(e => e.DefaultText);
            modelBuilder.Entity<ComplexNavigationGlobalization>().HasKey(e => e.Text);
            modelBuilder.Entity<ComplexNavigationLanguage>().HasKey(e => e.Name);

            modelBuilder.Entity<ComplexNavigationField>().HasOne(f => f.Label);
            modelBuilder.Entity<ComplexNavigationField>().HasOne(f => f.Placeholder);

            modelBuilder.Entity<ComplexNavigationString>().HasMany(m => m.Globalizations);

            modelBuilder.Entity<ComplexNavigationGlobalization>().HasOne(g => g.Language);
        }
    }
}

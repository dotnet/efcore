// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsOwnedQueryFixtureBase<TTestStore> : ComplexNavigationsQueryFixtureBase<TTestStore>
        where TTestStore : TestStore
    {
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Level1>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<Level1>()
                .Ignore(e => e.OneToOne_Optional_Self)
                .Ignore(e => e.OneToMany_Required_Self)
                .Ignore(e => e.OneToMany_Required_Self_Inverse)
                .Ignore(e => e.OneToMany_Optional_Self)
                .Ignore(e => e.OneToMany_Optional_Self_Inverse)
                .Ignore(e => e.OneToMany_Required)
                .Ignore(e => e.OneToMany_Optional)
                .OwnsOne(e => e.OneToOne_Required_PK, Configure);

            modelBuilder.Entity<ComplexNavigationField>().HasKey(e => e.Name);
            modelBuilder.Entity<ComplexNavigationString>().HasKey(e => e.DefaultText);
            modelBuilder.Entity<ComplexNavigationGlobalization>().HasKey(e => e.Text);
            modelBuilder.Entity<ComplexNavigationLanguage>().HasKey(e => e.Name);

            modelBuilder.Entity<ComplexNavigationField>().HasOne(f => f.Label);
            modelBuilder.Entity<ComplexNavigationField>().HasOne(f => f.Placeholder);

            modelBuilder.Entity<ComplexNavigationString>().HasMany(m => m.Globalizations);

            modelBuilder.Entity<ComplexNavigationGlobalization>().HasOne(g => g.Language);
        }

        protected virtual void Configure(ReferenceOwnershipBuilder<Level1, Level2> l2)
        {
            l2.Ignore(e => e.OneToOne_Optional_Self)
                .Ignore(e => e.OneToMany_Required_Self)
                .Ignore(e => e.OneToMany_Required_Self_Inverse)
                .Ignore(e => e.OneToMany_Optional_Self)
                .Ignore(e => e.OneToMany_Optional_Self_Inverse)
                .Ignore(e => e.OneToMany_Required)
                .Ignore(e => e.OneToMany_Required_Inverse)
                .Ignore(e => e.OneToMany_Optional)
                .Ignore(e => e.OneToMany_Optional_Inverse);

            l2.HasForeignKey(e => e.Id)
                .OnDelete(DeleteBehavior.Restrict);

            l2.Property(e => e.Id).ValueGeneratedNever();

            l2.HasOne(e => e.OneToOne_Required_PK_Inverse)
                .WithOne(e => e.OneToOne_Required_PK)
                .Metadata.IsOwnership = false; // #9093

            l2.HasOne(e => e.OneToOne_Optional_PK_Inverse)
                .WithOne(e => e.OneToOne_Optional_PK)
                .HasPrincipalKey<Level1>(e => e.Id)
                .IsRequired(false);

            l2.HasOne(e => e.OneToOne_Required_FK_Inverse)
                .WithOne(e => e.OneToOne_Required_FK)
                .HasForeignKey<Level2>(e => e.Level1_Required_Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            l2.HasOne(e => e.OneToOne_Optional_FK_Inverse)
                .WithOne(e => e.OneToOne_Optional_FK)
                .HasForeignKey<Level2>(e => e.Level1_Optional_Id)
                .IsRequired(false);

            l2.OwnsOne(e => e.OneToOne_Required_PK, Configure);
        }

        protected virtual void Configure(ReferenceOwnershipBuilder<Level2, Level3> l3)
        {
            l3.Ignore(e => e.OneToOne_Optional_Self)
                .Ignore(e => e.OneToMany_Required_Self)
                .Ignore(e => e.OneToMany_Required_Self_Inverse)
                .Ignore(e => e.OneToMany_Optional_Self)
                .Ignore(e => e.OneToMany_Optional_Self_Inverse)
                .Ignore(e => e.OneToMany_Required)
                .Ignore(e => e.OneToMany_Required_Inverse)
                .Ignore(e => e.OneToMany_Optional)
                .Ignore(e => e.OneToMany_Optional_Inverse);

            l3.HasForeignKey(e => e.Id)
                .OnDelete(DeleteBehavior.Restrict);

            l3.Property(e => e.Id).ValueGeneratedNever();

            l3.HasOne(e => e.OneToOne_Required_PK_Inverse)
                .WithOne(e => e.OneToOne_Required_PK)
                .Metadata.IsOwnership = false; // #9093

            l3.HasOne(e => e.OneToOne_Optional_PK_Inverse)
                .WithOne(e => e.OneToOne_Optional_PK)
                .HasPrincipalKey<Level2>(e => e.Id)
                .IsRequired(false);

            l3.HasOne(e => e.OneToOne_Required_FK_Inverse)
                .WithOne(e => e.OneToOne_Required_FK)
                .HasForeignKey<Level3>(e => e.Level2_Required_Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            l3.HasOne(e => e.OneToOne_Optional_FK_Inverse)
                .WithOne(e => e.OneToOne_Optional_FK)
                .HasForeignKey<Level3>(e => e.Level2_Optional_Id)
                .IsRequired(false);

            l3.OwnsOne(e => e.OneToOne_Required_PK, Configure);
        }

        protected virtual void Configure(ReferenceOwnershipBuilder<Level3, Level4> l4)
        {
            l4.Ignore(e => e.OneToOne_Optional_Self)
                .Ignore(e => e.OneToMany_Required_Self)
                .Ignore(e => e.OneToMany_Required_Self_Inverse)
                .Ignore(e => e.OneToMany_Optional_Self)
                .Ignore(e => e.OneToMany_Optional_Self_Inverse)
                .Ignore(e => e.OneToMany_Required_Inverse)
                .Ignore(e => e.OneToMany_Optional_Inverse);

            l4.HasForeignKey(e => e.Id)
                .OnDelete(DeleteBehavior.Restrict);

            l4.Property(e => e.Id).ValueGeneratedNever();

            l4.HasOne(e => e.OneToOne_Required_PK_Inverse)
                .WithOne(e => e.OneToOne_Required_PK)
                .Metadata.IsOwnership = false; // #9093

            l4.HasOne(e => e.OneToOne_Optional_PK_Inverse)
                .WithOne(e => e.OneToOne_Optional_PK)
                .HasPrincipalKey<Level3>()
                .IsRequired(false);

            l4.HasOne(e => e.OneToOne_Required_FK_Inverse)
                .WithOne(e => e.OneToOne_Required_FK)
                .HasForeignKey<Level4>(e => e.Level3_Required_Id)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            l4.HasOne(e => e.OneToOne_Optional_FK_Inverse)
                .WithOne(e => e.OneToOne_Optional_FK)
                .HasForeignKey<Level4>(e => e.Level3_Optional_Id)
                .IsRequired(false);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsOwnedQueryFixtureBase : ComplexNavigationsQueryFixtureBase
    {
        protected override string StoreName { get; } = "ComplexNavigationsOwned";

        protected ComplexNavigationsOwnedQueryFixtureBase()
        {
            QueryAsserter.SetExtractor = new ComplexNavigationsOwnedSetExtractor();
            QueryAsserter.ExpectedData = new ComplexNavigationsOwnedData();
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Ignore<Level2>();
            modelBuilder.Ignore<Level3>();
            modelBuilder.Ignore<Level4>();

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

            modelBuilder.Entity<InheritanceBase1>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<InheritanceBase2>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<InheritanceLeaf1>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<InheritanceLeaf2>().Property(e => e.Id).ValueGeneratedNever();

            // FK name needs to be explicitly provided because issue #9310
            modelBuilder.Entity<InheritanceBase2>().HasOne(e => e.Reference).WithOne().HasForeignKey<InheritanceBase1>("InheritanceBase2Id").IsRequired(false);
            modelBuilder.Entity<InheritanceBase2>().HasMany(e => e.Collection).WithOne();

            modelBuilder.Entity<InheritanceDerived1>().HasBaseType<InheritanceBase1>();
            modelBuilder.Entity<InheritanceDerived1>().HasOne(e => e.ReferenceSameType).WithOne().HasForeignKey<InheritanceLeaf1>("SameTypeReference_InheritanceDerived1Id").IsRequired(false);
            modelBuilder.Entity<InheritanceDerived1>().HasOne(e => e.ReferenceDifferentType).WithOne().HasForeignKey<InheritanceLeaf1>("DifferentTypeReference_InheritanceDerived1Id").IsRequired(false);
            modelBuilder.Entity<InheritanceDerived1>().HasMany(e => e.CollectionSameType).WithOne().HasForeignKey("SameTypeCollection_InheritanceDerived1Id").IsRequired(false);
            modelBuilder.Entity<InheritanceDerived1>().HasMany(e => e.CollectionDifferentType).WithOne().HasForeignKey("DifferentTypeCollection_InheritanceDerived1Id").IsRequired(false);

            modelBuilder.Entity<InheritanceDerived2>().HasBaseType<InheritanceBase1>();
            modelBuilder.Entity<InheritanceDerived2>().HasOne(e => e.ReferenceSameType).WithOne().HasForeignKey<InheritanceLeaf1>("SameTypeReference_InheritanceDerived2Id").IsRequired(false);
            modelBuilder.Entity<InheritanceDerived2>().HasOne(e => e.ReferenceDifferentType).WithOne().HasForeignKey<InheritanceLeaf2>("DifferentTypeReference_InheritanceDerived2Id").IsRequired(false);
            modelBuilder.Entity<InheritanceDerived2>().HasMany(e => e.CollectionSameType).WithOne().IsRequired(false);
            modelBuilder.Entity<InheritanceDerived2>().HasMany(e => e.CollectionDifferentType).WithOne().IsRequired(false);

            modelBuilder.Entity<InheritanceLeaf2>().HasMany(e => e.BaseCollection).WithOne().IsRequired(false);

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

        protected override void Seed(ComplexNavigationsContext context)
            => ComplexNavigationsData.Seed(context, tableSplitting: true);

        private class ComplexNavigationsOwnedSetExtractor : ISetExtractor
        {
            public override IQueryable<TEntity> Set<TEntity>(DbContext context)
            {
                if (typeof(TEntity) == typeof(Level1))
                {
                    return (IQueryable<TEntity>)GetLevelOne(context);
                }

                if (typeof(TEntity) == typeof(Level2))
                {
                    return (IQueryable<TEntity>)GetLevelTwo(context);
                }

                if (typeof(TEntity) == typeof(Level3))
                {
                    return (IQueryable<TEntity>)GetLevelThree(context);
                }

                if (typeof(TEntity) == typeof(Level4))
                {
                    return (IQueryable<TEntity>)GetLevelFour(context);
                }

                return context.Set<TEntity>();
            }

            private IQueryable<Level1> GetLevelOne(DbContext context)
                => context.Set<Level1>();

            private IQueryable<Level2> GetLevelTwo(DbContext context)
                => GetLevelOne(context).Select(t => t.OneToOne_Required_PK).Where(t => t != null);

            private IQueryable<Level3> GetLevelThree(DbContext context)
                => GetLevelTwo(context).Select(t => t.OneToOne_Required_PK).Where(t => t != null);

            private IQueryable<Level4> GetLevelFour(DbContext context)
                => GetLevelThree(context).Select(t => t.OneToOne_Required_PK).Where(t => t != null);
        }

        private class ComplexNavigationsOwnedData : ComplexNavigationsData
        {
            public override IQueryable<TEntity> Set<TEntity>()
            {
                if (typeof(TEntity) == typeof(Level1))
                {
                    return (IQueryable<TEntity>)GetExpectedLevelOne();
                }

                if (typeof(TEntity) == typeof(Level2))
                {
                    return (IQueryable<TEntity>)GetExpectedLevelTwo();
                }

                if (typeof(TEntity) == typeof(Level3))
                {
                    return (IQueryable<TEntity>)GetExpectedLevelThree();
                }

                if (typeof(TEntity) == typeof(Level4))
                {
                    return (IQueryable<TEntity>)GetExpectedLevelFour();
                }

                if (typeof(TEntity) == typeof(InheritanceBase1))
                {
                    return (IQueryable<TEntity>)InheritanceBaseOnes.AsQueryable();
                }

                if (typeof(TEntity) == typeof(InheritanceBase2))
                {
                    return (IQueryable<TEntity>)InheritanceBaseTwos.AsQueryable();
                }

                throw new InvalidOperationException("Invalid entity type: " + typeof(TEntity));
            }

            private IQueryable<Level1> GetExpectedLevelOne()
                => SplitLevelOnes.AsQueryable();

            private IQueryable<Level2> GetExpectedLevelTwo()
                => GetExpectedLevelOne().Select(t => t.OneToOne_Required_PK).Where(t => t != null);

            private IQueryable<Level3> GetExpectedLevelThree()
                => GetExpectedLevelTwo().Select(t => t.OneToOne_Required_PK).Where(t => t != null);

            private IQueryable<Level4> GetExpectedLevelFour()
                => GetExpectedLevelThree().Select(t => t.OneToOne_Required_PK).Where(t => t != null);
        }
    }
}

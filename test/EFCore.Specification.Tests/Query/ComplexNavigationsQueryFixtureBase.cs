// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.TestModels.ComplexNavigationsModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Query
{
    public abstract class ComplexNavigationsQueryFixtureBase : SharedStoreFixtureBase<ComplexNavigationsContext>, IQueryFixtureBase
    {
        protected override string StoreName { get; } = "ComplexNavigations";

        public Func<DbContext> GetContextCreator()
            => () => CreateContext();

        public virtual ISetSource GetExpectedData()
            => new ComplexNavigationsDefaultData();

        public IReadOnlyDictionary<Type, object> GetEntitySorters()
            => new Dictionary<Type, Func<object, object>>
            {
                { typeof(Level1), e => ((Level1)e)?.Id },
                { typeof(Level2), e => ((Level2)e)?.Id },
                { typeof(Level3), e => ((Level3)e)?.Id },
                { typeof(Level4), e => ((Level4)e)?.Id },
                { typeof(InheritanceBase1), e => ((InheritanceBase1)e)?.Id },
                { typeof(InheritanceBase2), e => ((InheritanceBase2)e)?.Id },
                { typeof(InheritanceDerived1), e => ((InheritanceDerived1)e)?.Id },
                { typeof(InheritanceDerived2), e => ((InheritanceDerived2)e)?.Id },
                { typeof(InheritanceLeaf1), e => ((InheritanceLeaf1)e)?.Id },
                { typeof(InheritanceLeaf2), e => ((InheritanceLeaf2)e)?.Id }
            }.ToDictionary(e => e.Key, e => (object)e.Value);

        public IReadOnlyDictionary<Type, object> GetEntityAsserters()
            => new Dictionary<Type, Action<object, object>>
            {
                {
                    typeof(Level1), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (Level1)e;
                            var aa = (Level1)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                            Assert.Equal(ee.Date, aa.Date);
                        }
                    }
                },
                {
                    typeof(Level2), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (Level2)e;
                            var aa = (Level2)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                            Assert.Equal(ee.Date, aa.Date);
                            Assert.Equal(ee.Level1_Optional_Id, aa.Level1_Optional_Id);
                            Assert.Equal(ee.Level1_Required_Id, aa.Level1_Required_Id);
                        }
                    }
                },
                {
                    typeof(Level3), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (Level3)e;
                            var aa = (Level3)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                            Assert.Equal(ee.Level2_Optional_Id, aa.Level2_Optional_Id);
                            Assert.Equal(ee.Level2_Required_Id, aa.Level2_Required_Id);
                        }
                    }
                },
                {
                    typeof(Level4), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (Level4)e;
                            var aa = (Level4)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                            Assert.Equal(ee.Level3_Optional_Id, aa.Level3_Optional_Id);
                            Assert.Equal(ee.Level3_Required_Id, aa.Level3_Required_Id);
                        }
                    }
                },
                {
                    typeof(InheritanceBase1), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (InheritanceBase1)e;
                            var aa = (InheritanceBase1)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                },
                {
                    typeof(InheritanceBase2), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (InheritanceBase2)e;
                            var aa = (InheritanceBase2)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                },
                {
                    typeof(InheritanceDerived1), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (InheritanceDerived1)e;
                            var aa = (InheritanceDerived1)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                },
                {
                    typeof(InheritanceDerived2), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (InheritanceDerived2)e;
                            var aa = (InheritanceDerived2)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                },
                {
                    typeof(InheritanceLeaf1), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (InheritanceLeaf1)e;
                            var aa = (InheritanceLeaf1)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                },
                {
                    typeof(InheritanceLeaf2), (e, a) =>
                    {
                        Assert.Equal(e == null, a == null);

                        if (a != null)
                        {
                            var ee = (InheritanceLeaf2)e;
                            var aa = (InheritanceLeaf2)a;

                            Assert.Equal(ee.Id, aa.Id);
                            Assert.Equal(ee.Name, aa.Name);
                        }
                    }
                }
            }.ToDictionary(e => e.Key, e => (object)e.Value);

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Level1>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Level2>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Level3>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Level4>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_Self1).WithOne();
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_PK1).WithOne(e => e.OneToOne_Required_PK_Inverse2)
                .HasPrincipalKey<Level1>(e => e.Id).HasForeignKey<Level2>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_PK1).WithOne(e => e.OneToOne_Optional_PK_Inverse2)
                .HasPrincipalKey<Level1>(e => e.Id).IsRequired(false);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_FK1).WithOne(e => e.OneToOne_Required_FK_Inverse2)
                .HasForeignKey<Level2>(e => e.Level1_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_FK1).WithOne(e => e.OneToOne_Optional_FK_Inverse2)
                .HasForeignKey<Level2>(e => e.Level1_Optional_Id).IsRequired(false);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Required1).WithOne(e => e.OneToMany_Required_Inverse2).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional1).WithOne(e => e.OneToMany_Optional_Inverse2).IsRequired(false);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Required_Self1).WithOne(e => e.OneToMany_Required_Self_Inverse1)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional_Self1).WithOne(e => e.OneToMany_Optional_Self_Inverse1)
                .IsRequired(false);

            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_Self2).WithOne();
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_PK2).WithOne(e => e.OneToOne_Required_PK_Inverse3)
                .HasPrincipalKey<Level2>(e => e.Id).HasForeignKey<Level3>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_PK2).WithOne(e => e.OneToOne_Optional_PK_Inverse3)
                .HasPrincipalKey<Level2>(e => e.Id).IsRequired(false);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_FK2).WithOne(e => e.OneToOne_Required_FK_Inverse3)
                .HasForeignKey<Level3>(e => e.Level2_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_FK2).WithOne(e => e.OneToOne_Optional_FK_Inverse3)
                .HasForeignKey<Level3>(e => e.Level2_Optional_Id).IsRequired(false);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Required2).WithOne(e => e.OneToMany_Required_Inverse3).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Optional2).WithOne(e => e.OneToMany_Optional_Inverse3).IsRequired(false);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Required_Self2).WithOne(e => e.OneToMany_Required_Self_Inverse2)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Optional_Self2).WithOne(e => e.OneToMany_Optional_Self_Inverse2)
                .IsRequired(false);

            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_Self3).WithOne();
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Required_PK3).WithOne(e => e.OneToOne_Required_PK_Inverse4)
                .HasPrincipalKey<Level3>(e => e.Id).HasForeignKey<Level4>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_PK3).WithOne(e => e.OneToOne_Optional_PK_Inverse4)
                .HasPrincipalKey<Level3>(e => e.Id).IsRequired(false);
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Required_FK3).WithOne(e => e.OneToOne_Required_FK_Inverse4)
                .HasForeignKey<Level4>(e => e.Level3_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_FK3).WithOne(e => e.OneToOne_Optional_FK_Inverse4)
                .HasForeignKey<Level4>(e => e.Level3_Optional_Id).IsRequired(false);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Required3).WithOne(e => e.OneToMany_Required_Inverse4).IsRequired()
                .OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Optional3).WithOne(e => e.OneToMany_Optional_Inverse4).IsRequired(false);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Required_Self3).WithOne(e => e.OneToMany_Required_Self_Inverse3)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level3>().HasMany(e => e.OneToMany_Optional_Self3).WithOne(e => e.OneToMany_Optional_Self_Inverse3)
                .IsRequired(false);

            modelBuilder.Entity<Level4>().HasOne(e => e.OneToOne_Optional_Self4).WithOne();
            modelBuilder.Entity<Level4>().HasMany(e => e.OneToMany_Required_Self4).WithOne(e => e.OneToMany_Required_Self_Inverse4)
                .IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level4>().HasMany(e => e.OneToMany_Optional_Self4).WithOne(e => e.OneToMany_Optional_Self_Inverse4)
                .IsRequired(false);

            modelBuilder.Entity<InheritanceBase1>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<InheritanceBase2>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<InheritanceLeaf1>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<InheritanceLeaf2>().Property(e => e.Id).ValueGeneratedNever();

            // FK name needs to be explicitly provided because issue #9310
            modelBuilder.Entity<InheritanceBase2>().HasOne(e => e.Reference).WithOne().HasForeignKey<InheritanceBase1>("InheritanceBase2Id")
                .IsRequired(false);
            modelBuilder.Entity<InheritanceBase2>().HasMany(e => e.Collection).WithOne();

            modelBuilder.Entity<InheritanceDerived1>().HasBaseType<InheritanceBase1>();
            modelBuilder.Entity<InheritanceDerived1>().HasOne(e => e.ReferenceSameType).WithOne()
                .HasForeignKey<InheritanceLeaf1>("SameTypeReference_InheritanceDerived1Id").IsRequired(false);
            modelBuilder.Entity<InheritanceDerived1>().HasOne(e => e.ReferenceDifferentType).WithOne()
                .HasForeignKey<InheritanceLeaf1>("DifferentTypeReference_InheritanceDerived1Id").IsRequired(false);
            modelBuilder.Entity<InheritanceDerived1>().HasMany(e => e.CollectionSameType).WithOne().HasForeignKey("InheritanceDerived1Id")
                .IsRequired(false);
            modelBuilder.Entity<InheritanceDerived1>().HasMany(e => e.CollectionDifferentType).WithOne()
                .HasForeignKey("InheritanceDerived1Id").IsRequired(false);

            modelBuilder.Entity<InheritanceDerived2>().HasBaseType<InheritanceBase1>();
            modelBuilder.Entity<InheritanceDerived2>().HasOne(e => e.ReferenceSameType).WithOne()
                .HasForeignKey<InheritanceLeaf1>("SameTypeReference_InheritanceDerived2Id").IsRequired(false);
            modelBuilder.Entity<InheritanceDerived2>().HasOne(e => e.ReferenceDifferentType).WithOne()
                .HasForeignKey<InheritanceLeaf2>("DifferentTypeReference_InheritanceDerived2Id").IsRequired(false);
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

        protected override void Seed(ComplexNavigationsContext context)
            => ComplexNavigationsData.Seed(context);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                c => c
                    .Log(CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning)
                    .Log(CoreEventId.RowLimitingOperationWithoutOrderByWarning));

        public override ComplexNavigationsContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }

        protected class ComplexNavigationsDefaultData : ComplexNavigationsData
        {
            public override IQueryable<TEntity> Set<TEntity>()
            {
                if (typeof(TEntity) == typeof(Level1))
                {
                    return (IQueryable<TEntity>)LevelOnes.AsQueryable();
                }

                if (typeof(TEntity) == typeof(Level2))
                {
                    return (IQueryable<TEntity>)LevelTwos.AsQueryable();
                }

                if (typeof(TEntity) == typeof(Level3))
                {
                    return (IQueryable<TEntity>)LevelThrees.AsQueryable();
                }

                if (typeof(TEntity) == typeof(Level4))
                {
                    return (IQueryable<TEntity>)LevelFours.AsQueryable();
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
        }
    }
}

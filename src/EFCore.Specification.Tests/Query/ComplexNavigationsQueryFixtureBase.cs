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

        protected ComplexNavigationsQueryFixtureBase()
        {
            var entitySorters = new Dictionary<Type, Func<dynamic, object>>
            {
                { typeof(Level1), e => e.Id },
                { typeof(Level2), e => e.Id },
                { typeof(Level3), e => e.Id },
                { typeof(Level4), e => e.Id }
            };

            var entityAsserters = new Dictionary<Type, Action<dynamic, dynamic>>
            {
                {
                    typeof(Level1),
                    (e, a) =>
                    {
                        Assert.Equal(e.Id, a.Id);
                        Assert.Equal(e.Name, a.Name);
                        Assert.Equal(e.Date, a.Date);
                    }
                },
                {
                    typeof(Level2),
                    (e, a) =>
                        {
                            Assert.Equal(e.Id, a.Id);
                            Assert.Equal(e.Name, a.Name);
                            Assert.Equal(e.Date, a.Date);
                            Assert.Equal(e.Level1_Optional_Id, a.Level1_Optional_Id);
                            Assert.Equal(e.Level1_Required_Id, a.Level1_Required_Id);
                        }
                },
                {
                    typeof(Level3),
                    (e, a) =>
                        {
                            Assert.Equal(e.Id, a.Id);
                            Assert.Equal(e.Name, a.Name);
                            Assert.Equal(e.Level2_Optional_Id, a.Level2_Optional_Id);
                            Assert.Equal(e.Level2_Required_Id, a.Level2_Required_Id);
                        }
                },
                {
                    typeof(Level4),
                    (e, a) =>
                        {
                            Assert.Equal(e.Id, a.Id);
                            Assert.Equal(e.Name, a.Name);
                            Assert.Equal(e.Level3_Optional_Id, a.Level3_Optional_Id);
                            Assert.Equal(e.Level3_Required_Id, a.Level3_Required_Id);
                        }
                }
            };

            QueryAsserter = new QueryAsserter<ComplexNavigationsContext>(
                CreateContext,
                new ComplexNavigationsDefaultData(),
                entitySorters,
                entityAsserters);
        }

        public QueryAsserterBase QueryAsserter { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Level1>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Level2>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Level3>().Property(e => e.Id).ValueGeneratedNever();
            modelBuilder.Entity<Level4>().Property(e => e.Id).ValueGeneratedNever();

            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_Self).WithOne();
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_PK).WithOne(e => e.OneToOne_Required_PK_Inverse).HasPrincipalKey<Level1>(e => e.Id).HasForeignKey<Level2>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_PK).WithOne(e => e.OneToOne_Optional_PK_Inverse).HasPrincipalKey<Level1>(e => e.Id).IsRequired(false);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Required_FK).WithOne(e => e.OneToOne_Required_FK_Inverse).HasForeignKey<Level2>(e => e.Level1_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).HasForeignKey<Level2>(e => e.Level1_Optional_Id).IsRequired(false);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Required).WithOne(e => e.OneToMany_Required_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional).WithOne(e => e.OneToMany_Optional_Inverse).IsRequired(false);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Required_Self).WithOne(e => e.OneToMany_Required_Self_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level1>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).IsRequired(false);

            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_Self).WithOne();
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_PK).WithOne(e => e.OneToOne_Required_PK_Inverse).HasPrincipalKey<Level2>(e => e.Id).HasForeignKey<Level3>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_PK).WithOne(e => e.OneToOne_Optional_PK_Inverse).HasPrincipalKey<Level2>(e => e.Id).IsRequired(false);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Required_FK).WithOne(e => e.OneToOne_Required_FK_Inverse).HasForeignKey<Level3>(e => e.Level2_Required_Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasOne(e => e.OneToOne_Optional_FK).WithOne(e => e.OneToOne_Optional_FK_Inverse).HasForeignKey<Level3>(e => e.Level2_Optional_Id).IsRequired(false);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Required).WithOne(e => e.OneToMany_Required_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Optional).WithOne(e => e.OneToMany_Optional_Inverse).IsRequired(false);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Required_Self).WithOne(e => e.OneToMany_Required_Self_Inverse).IsRequired().OnDelete(DeleteBehavior.Restrict);
            modelBuilder.Entity<Level2>().HasMany(e => e.OneToMany_Optional_Self).WithOne(e => e.OneToMany_Optional_Self_Inverse).IsRequired(false);

            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Optional_Self).WithOne();
            modelBuilder.Entity<Level3>().HasOne(e => e.OneToOne_Required_PK).WithOne(e => e.OneToOne_Required_PK_Inverse).HasPrincipalKey<Level3>(e => e.Id).HasForeignKey<Level4>(e => e.Id).IsRequired().OnDelete(DeleteBehavior.Restrict);
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

        protected override void Seed(ComplexNavigationsContext context) => ComplexNavigationsData.Seed(context);

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(c => c
                .Log(CoreEventId.PossibleUnintendedCollectionNavigationNullComparisonWarning));
        
        public override ComplexNavigationsContext CreateContext()
        {
            var context = base.CreateContext();
            context.ChangeTracker.QueryTrackingBehavior = QueryTrackingBehavior.NoTracking;
            return context;
        }

        private class ComplexNavigationsDefaultData : ComplexNavigationsData
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

                throw new NotImplementedException();
            }
        }
    }
}

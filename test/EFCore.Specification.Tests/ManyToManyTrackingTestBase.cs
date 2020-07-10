// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Query;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Xunit;

namespace Microsoft.EntityFrameworkCore
{
    public abstract class ManyToManyTrackingTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : ManyToManyTrackingTestBase<TFixture>.ManyToManyTrackingFixtureBase
    {
        [ConditionalFact]
        public virtual void Can_insert_many_to_many()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = new[]
                    {
                        new EntityOne { Id = 7711, TwoSkip = new List<EntityTwo>() },
                        new EntityOne { Id = 7712, TwoSkip = new List<EntityTwo>() },
                        new EntityOne { Id = 7713, TwoSkip = new List<EntityTwo>() }
                    };
                    var rightEntities = new[]
                    {
                        new EntityTwo { Id = 7721, OneSkip = new List<EntityOne>() },
                        new EntityTwo { Id = 7722, OneSkip = new List<EntityOne>() },
                        new EntityTwo { Id = 7723, OneSkip = new List<EntityOne>() }
                    };

                    leftEntities[0].TwoSkip.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].TwoSkip.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].TwoSkip.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkip.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkip.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkip.Add(leftEntities[2]); // 21 - 13

                    context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);

                    Assert.Equal(11, context.ChangeTracker.Entries().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<EntityTwo>().Count());
                    Assert.Equal(5, context.ChangeTracker.Entries<JoinOneToTwo>().Count());

                    Assert.Equal(3, leftEntities[0].TwoSkip.Count);
                    Assert.Single(leftEntities[1].TwoSkip);
                    Assert.Single(leftEntities[2].TwoSkip);

                    Assert.Equal(3, rightEntities[0].OneSkip.Count);
                    Assert.Single(rightEntities[1].OneSkip);
                    Assert.Single(rightEntities[2].OneSkip);

                    context.SaveChanges();

                    Assert.Equal(11, context.ChangeTracker.Entries().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<EntityTwo>().Count());
                    Assert.Equal(5, context.ChangeTracker.Entries<JoinOneToTwo>().Count());

                    Assert.Equal(3, leftEntities[0].TwoSkip.Count);
                    Assert.Single(leftEntities[1].TwoSkip);
                    Assert.Single(leftEntities[2].TwoSkip);

                    Assert.Equal(3, rightEntities[0].OneSkip.Count);
                    Assert.Single(rightEntities[1].OneSkip);
                    Assert.Single(rightEntities[2].OneSkip);
                },
                context =>
                {
                    var results = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.TwoSkip).ToList();
                    Assert.Equal(3, results.Count);

                    Assert.Equal(11, context.ChangeTracker.Entries().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<EntityTwo>().Count());
                    Assert.Equal(5, context.ChangeTracker.Entries<JoinOneToTwo>().Count());

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    Assert.Equal(3, leftEntities[0].TwoSkip.Count);
                    Assert.Single(leftEntities[1].TwoSkip);
                    Assert.Single(leftEntities[2].TwoSkip);

                    Assert.Equal(3, rightEntities[0].OneSkip.Count);
                    Assert.Single(rightEntities[1].OneSkip);
                    Assert.Single(rightEntities[2].OneSkip);
                });
        }

        [ConditionalTheory]
        [InlineData(new[] { 1, 2, 3 })]
        [InlineData(new[] { 2, 1, 3 })]
        [InlineData(new[] { 3, 1, 2 })]
        [InlineData(new[] { 3, 2, 1 })]
        [InlineData(new[] { 1, 3, 2 })]
        [InlineData(new[] { 2, 3, 1 })]
        public virtual void Can_load_entities_in_any_order(int[] order)
        {
            using var context = CreateContext();

            foreach (var i in order)
            {
                (i switch
                {
                    1 => (IQueryable<object>)context.Set<EntityOne>(),
                    2 => context.Set<EntityTwo>(),
                    3 => context.Set<JoinOneToTwo>(),
                    _ => throw new ArgumentException()
                }).Load();
            }

            ValidateCounts(context, 152, 20, 20, 112);
        }

        private static void ValidateCounts(DbContext context, int total, int ones, int twos, int associations)
        {
            Assert.Equal(total, context.ChangeTracker.Entries().Count());
            Assert.Equal(ones, context.ChangeTracker.Entries<EntityOne>().Count());
            Assert.Equal(twos, context.ChangeTracker.Entries<EntityTwo>().Count());
            Assert.Equal(associations, context.ChangeTracker.Entries<JoinOneToTwo>().Count());

            var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
            var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

            var joinCount = 0;
            foreach (var left in leftEntities)
            {
                foreach (var right in rightEntities)
                {
                    if (left.TwoSkip?.Contains(right) == true)
                    {
                        Assert.Contains(left, right.OneSkip);
                        joinCount++;
                    }

                    if (right.OneSkip?.Contains(left) == true)
                    {
                        Assert.Contains(right, left.TwoSkip);
                        joinCount++;
                    }
                }
            }

            var deleted = context.ChangeTracker.Entries<JoinOneToTwo>().Count(e => e.State == EntityState.Deleted);
            Assert.Equal(associations, (joinCount / 2) + deleted);
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var ones = context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Id).ToList();
                    var twos = context.Set<EntityTwo>().Include(e => e.OneSkip).ToList();

                    ones[0].TwoSkip.AddRange(new[]
                    {
                        new EntityTwo { Id = 7721 },
                        new EntityTwo { Id = 7722 },
                        new EntityTwo { Id = 7723 }
                    });

                    twos[0].OneSkip.AddRange(new[]
                    {
                        new EntityOne { Id = 7711 },
                        new EntityOne { Id = 7712 },
                        new EntityOne { Id = 7713 }
                    });

                    ones[1].TwoSkip.Remove(ones[1].TwoSkip.Single(e => e.Id == 1));
                    twos[1].OneSkip.Remove(twos[1].OneSkip.Single(e => e.Id == 1));

                    ones[2].TwoSkip.Remove(ones[2].TwoSkip.Single(e => e.Id == 1));
                    ones[2].TwoSkip.Add(new EntityTwo { Id = 7724 });

                    twos[2].OneSkip.Remove(twos[2].OneSkip.Single(e => e.Id == 1));
                    twos[2].OneSkip.Add(new EntityOne { Id = 7714 });

                    context.ChangeTracker.DetectChanges();

                    ValidateCounts(context, 168, 24, 24, 120);
                    ValidateNavigations(ones, twos);

                    context.SaveChanges();

                    ValidateCounts(context, 164, 24, 24, 116);
                    ValidateNavigations(ones, twos);
                },
                context =>
                {
                    var ones = context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Id).ToList();
                    var twos = context.Set<EntityTwo>().Include(e => e.OneSkip).ToList();

                    ValidateCounts(context, 164, 24, 24, 116);
                    ValidateNavigations(ones, twos);
                });

            void ValidateNavigations(List<EntityOne> ones, List<EntityTwo> twos)
            {
                Assert.Contains(ones[0].TwoSkip, e => e.Id == 7721);
                Assert.Contains(ones[0].TwoSkip, e => e.Id == 7722);
                Assert.Contains(ones[0].TwoSkip, e => e.Id == 7723);

                Assert.Contains(twos[0].OneSkip, e => e.Id == 7711);
                Assert.Contains(twos[0].OneSkip, e => e.Id == 7712);
                Assert.Contains(twos[0].OneSkip, e => e.Id == 7713);

                Assert.DoesNotContain(ones[1].TwoSkip, e => e.Id == 1);
                Assert.DoesNotContain(twos[1].OneSkip, e => e.Id == 1);

                Assert.DoesNotContain(ones[2].TwoSkip, e => e.Id == 1);
                Assert.Contains(ones[2].TwoSkip, e => e.Id == 7724);

                Assert.DoesNotContain(twos[2].OneSkip, e => e.Id == 1);
                Assert.Contains(twos[2].OneSkip, e => e.Id == 7714);
            }
        }

        [ConditionalFact]
        public virtual void Can_delete_with_many_to_many()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var ones = context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Id).ToList();
                    var twos = context.Set<EntityTwo>().Include(e => e.OneSkip).ToList();

                    // Make sure other related entities are loaded for delete fixup
                    context.Set<EntityThree>().Load();
                    context.Set<JoinOneToThreePayloadFull>().Load();
                    context.Set<JoinOneSelfPayload>().Load();

                    context.Remove(context.Find<EntityOne>(1));
                    context.Remove(context.Find<EntityTwo>(1));

                    context.ChangeTracker.DetectChanges();

                    ValidateNavigations(ones, twos);

                    Assert.All(
                        context.ChangeTracker.Entries<JoinOneToTwo>(), e => Assert.Equal(
                            e.Entity.OneId == 1
                            || e.Entity.TwoId == 1
                                ? EntityState.Deleted
                                : EntityState.Unchanged, e.State));

                    context.SaveChanges();

                    ValidateNavigations(ones, twos);
                    Assert.DoesNotContain(context.ChangeTracker.Entries<JoinOneToTwo>(), e => e.Entity.OneId == 1 || e.Entity.TwoId == 1);
                },
                context =>
                {
                    var ones = context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Id).ToList();
                    var twos = context.Set<EntityTwo>().Include(e => e.OneSkip).ToList();

                    ValidateNavigations(ones, twos);
                    Assert.DoesNotContain(context.ChangeTracker.Entries<JoinOneToTwo>(), e => e.Entity.OneId == 1 || e.Entity.TwoId == 1);
                });

            void ValidateNavigations(List<EntityOne> ones, List<EntityTwo> twos)
            {
                foreach (var one in ones)
                {
                    if (one.TwoSkip != null)
                    {
                        Assert.DoesNotContain(one.TwoSkip, e => e.Id == 1);
                    }
                }

                foreach (var two in twos)
                {
                    if (two.OneSkip != null)
                    {
                        Assert.DoesNotContain(two.OneSkip, e => e.Id == 1);
                    }
                }
            }
        }

        protected ManyToManyTrackingTestBase(TFixture fixture) => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected virtual void ExecuteWithStrategyInTransaction(
            Action<DbContext> testOperation,
            Action<DbContext> nestedTestOperation1 = null,
            Action<DbContext> nestedTestOperation2 = null,
            Action<DbContext> nestedTestOperation3 = null)
            => TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        protected DbContext CreateContext() => Fixture.CreateContext();

        public abstract class ManyToManyTrackingFixtureBase : ManyToManyQueryFixtureBase
        {
            protected override string StoreName { get; } = "ManyToManyTracking";
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
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
        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_composite_with_navs(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7711;
                                e.Key2 = "7711";
                                e.Key3 = new DateTime(7711, 1, 1);
                            }),
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7712;
                                e.Key2 = "7712";
                                e.Key3 = new DateTime(7712, 1, 1);
                            }),
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7713;
                                e.Key2 = "7713";
                                e.Key3 = new DateTime(7713, 1, 1);
                            }),
                    };
                    var rightEntities = new[]
                    {
                        context.Set<EntityLeaf>().CreateInstance((e, p) => e.Id = 7721),
                        context.Set<EntityLeaf>().CreateInstance((e, p) => e.Id = 7722),
                        context.Set<EntityLeaf>().CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].LeafSkipFull = CreateCollection<EntityLeaf>();

                    leftEntities[0].LeafSkipFull.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].LeafSkipFull.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].LeafSkipFull.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].CompositeKeySkipFull = CreateCollection<EntityCompositeKey>();

                    rightEntities[0].CompositeKeySkipFull.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].CompositeKeySkipFull.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].CompositeKeySkipFull.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<EntityCompositeKey>().Where(e => e.Key1 > 7700).Include(e => e.LeafSkipFull);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityCompositeKey>()
                        .Select(e => e.Entity).OrderBy(e => e.Key1).ToList();

                    var rightEntities = context.ChangeTracker.Entries<EntityLeaf>()
                        .Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            static void ValidateFixup(DbContext context, IList<EntityCompositeKey> leftEntities, IList<EntityLeaf> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityCompositeKey>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityLeaf>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>().Count());

                Assert.Equal(3, leftEntities[0].LeafSkipFull.Count);
                Assert.Single(leftEntities[1].LeafSkipFull);
                Assert.Single(leftEntities[2].LeafSkipFull);

                Assert.Equal(3, rightEntities[0].CompositeKeySkipFull.Count);
                Assert.Single(rightEntities[1].CompositeKeySkipFull);
                Assert.Single(rightEntities[2].CompositeKeySkipFull);

                foreach (var joinEntity in context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                    Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                    Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                    Assert.Equal(joinEntity.Leaf.Id, joinEntity.LeafId);

                    Assert.Contains(joinEntity, joinEntity.Composite.JoinLeafFull);
                    Assert.Contains(joinEntity, joinEntity.Leaf.JoinCompositeKeyFull);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_composite_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityCompositeKey>().Include(e => e.LeafSkipFull).OrderBy(e => e.Key1).ToList();
                    var rightEntities = context.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull).ToList();

                    leftEntities[0].LeafSkipFull.Add(context.Set<EntityLeaf>().CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].LeafSkipFull.Add(context.Set<EntityLeaf>().CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].LeafSkipFull.Add(context.Set<EntityLeaf>().CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].CompositeKeySkipFull.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7711;
                                e.Key2 = "7711";
                                e.Key3 = new DateTime(7711, 1, 1);
                            }));
                    rightEntities[0].CompositeKeySkipFull.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7712;
                                e.Key2 = "7712";
                                e.Key3 = new DateTime(7712, 1, 1);
                            }));
                    rightEntities[0].CompositeKeySkipFull.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7713;
                                e.Key2 = "7713";
                                e.Key3 = new DateTime(7713, 1, 1);
                            }));

                    leftEntities[0].LeafSkipFull.Remove(leftEntities[0].LeafSkipFull.Single(e => e.Id == 21));
                    rightEntities[1].CompositeKeySkipFull.Remove(rightEntities[1].CompositeKeySkipFull.Single(e => e.Key2 == "3_1"));

                    leftEntities[2].LeafSkipFull.Remove(leftEntities[2].LeafSkipFull.Single(e => e.Id == 23));
                    leftEntities[2].LeafSkipFull.Add(context.Set<EntityLeaf>().CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[2].CompositeKeySkipFull.Remove(rightEntities[2].CompositeKeySkipFull.Single(e => e.Key2 == "8_3"));
                    rightEntities[2].CompositeKeySkipFull.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7714;
                                e.Key2 = "7714";
                                e.Key3 = new DateTime(7714, 1, 1);
                            }));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, 24, 8, 39);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 8, 39 - 4);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityCompositeKey>().Include(e => e.LeafSkipFull).OrderBy(e => e.Key1).ToList();
                    var rightEntities = context.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 8, 39 - 4);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityCompositeKey> leftEntities,
                List<EntityLeaf> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityCompositeKey>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityLeaf>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].LeafSkipFull, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].LeafSkipFull, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].LeafSkipFull, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Key1 == 7711);
                Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Key1 == 7712);
                Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Key1 == 7713);

                Assert.DoesNotContain(leftEntities[0].LeafSkipFull, e => e.Id == 21);
                Assert.DoesNotContain(rightEntities[1].CompositeKeySkipFull, e => e.Key2 == "3_1");

                Assert.DoesNotContain(leftEntities[2].LeafSkipFull, e => e.Id == 23);
                Assert.Contains(leftEntities[2].LeafSkipFull, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].CompositeKeySkipFull, e => e.Key2 == "8_1");
                Assert.Contains(rightEntities[2].CompositeKeySkipFull, e => e.Key1 == 7714);

                foreach (var joinEntry in context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>().ToList())
                {
                    var joinEntity = joinEntry.Entity;

                    Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                    Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                    Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                    Assert.Equal(joinEntity.Leaf.Id, joinEntity.LeafId);

                    Assert.Contains(joinEntity, joinEntity.Composite.JoinLeafFull);
                    Assert.Contains(joinEntity, joinEntity.Leaf.JoinCompositeKeyFull);
                }

                var allLeft = context.ChangeTracker.Entries<EntityCompositeKey>().Select(e => e.Entity).OrderBy(e => e.Key1).ToList();
                var allRight = context.ChangeTracker.Entries<EntityLeaf>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.LeafSkipFull?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.CompositeKeySkipFull);
                            count++;
                        }

                        if (right.CompositeKeySkipFull?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.LeafSkipFull);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_delete_with_many_to_many_composite_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var ones = context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2).ToList();
                    var threes = context.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull).ToList();

                    // Make sure other related entities are loaded for delete fixup
                    context.Set<JoinThreeToCompositeKeyFull>().Load();

                    var toRemoveOne = context.Find<EntityCompositeKey>(6, "6_1", new DateTime(2006, 1, 1));
                    var refCountOnes = threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne);

                    var toRemoveThree = context.Find<EntityLeaf>(23);
                    var refCountThrees = ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree);

                    foreach (var joinEntity in context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>().Select(e => e.Entity).ToList())
                    {
                        Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                        Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                        Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                        Assert.Equal(joinEntity.Leaf.Id, joinEntity.LeafId);
                        Assert.Contains(joinEntity, joinEntity.Composite.JoinLeafFull);
                        Assert.Contains(joinEntity, joinEntity.Leaf.JoinCompositeKeyFull);
                    }

                    context.Remove(toRemoveOne);
                    context.Remove(toRemoveThree);

                    Assert.Equal(refCountOnes, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountThrees, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    Assert.Equal(refCountOnes, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountThrees, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    Assert.All(
                        context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>(), e => Assert.Equal(
                            (e.Entity.CompositeId1 == 6
                                && e.Entity.CompositeId2 == "6_1"
                                && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                            || e.Entity.LeafId == 23
                                ? EntityState.Deleted
                                : EntityState.Unchanged, e.State));

                    context.SaveChanges();

                    Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    ones.Remove(toRemoveOne);
                    threes.Remove(toRemoveThree);

                    Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                    Assert.DoesNotContain(
                        context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>(),
                        e => (e.Entity.CompositeId1 == 6
                                && e.Entity.CompositeId2 == "6_1"
                                && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                            || e.Entity.LeafId == 23);
                },
                context =>
                {
                    var ones = context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2).ToList();
                    var threes = context.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull).ToList();

                    ValidateNavigations(ones, threes);

                    Assert.DoesNotContain(
                        context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>(),
                        e => (e.Entity.CompositeId1 == 6
                                && e.Entity.CompositeId2 == "6_1"
                                && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                            || e.Entity.LeafId == 23);
                });

            static void ValidateNavigations(List<EntityCompositeKey> ones, List<EntityLeaf> threes)
            {
                foreach (var one in ones)
                {
                    if (one.RootSkipShared != null)
                    {
                        Assert.DoesNotContain(one.RootSkipShared, e => e.Id == 23);
                    }

                    if (one.JoinLeafFull != null)
                    {
                        Assert.DoesNotContain(
                            one.JoinLeafFull,
                            e => e.CompositeId1 == 6
                                && e.CompositeId2 == "6_1"
                                && e.CompositeId3 == new DateTime(2006, 1, 1));

                        Assert.DoesNotContain(one.JoinLeafFull, e => e.LeafId == 23);
                    }
                }

                foreach (var three in threes)
                {
                    if (three.CompositeKeySkipFull != null)
                    {
                        Assert.DoesNotContain(
                            three.CompositeKeySkipFull,
                            e => e.Key1 == 6
                                && e.Key2 == "6_1"
                                && e.Key3 == new DateTime(2006, 1, 1));
                    }

                    if (three.JoinCompositeKeyFull != null)
                    {
                        Assert.DoesNotContain(
                            three.JoinCompositeKeyFull,
                            e => e.CompositeId1 == 6
                                && e.CompositeId2 == "6_1"
                                && e.CompositeId3 == new DateTime(2006, 1, 1));

                        Assert.DoesNotContain(three.JoinCompositeKeyFull, e => e.LeafId == 23);
                    }
                }
            }

            static void ValidateJoinNavigations(DbContext context)
            {
                foreach (var joinEntity in context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                    Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                    Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                    Assert.Equal(joinEntity.Leaf.Id, joinEntity.LeafId);

                    Assert.Contains(joinEntity, joinEntity.Composite.JoinLeafFull);
                    Assert.Contains(joinEntity, joinEntity.Leaf.JoinCompositeKeyFull);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_composite_shared_with_navs(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7711;
                                e.Key2 = "7711";
                                e.Key3 = new DateTime(7711, 1, 1);
                            }),
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7712;
                                e.Key2 = "7712";
                                e.Key3 = new DateTime(7712, 1, 1);
                            }),
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7713;
                                e.Key2 = "7713";
                                e.Key3 = new DateTime(7713, 1, 1);
                            }),
                    };
                    var rightEntities = new[]
                    {
                        context.Set<EntityRoot>().CreateInstance((e, p) => e.Id = 7721),
                        context.Set<EntityRoot>().CreateInstance((e, p) => e.Id = 7722),
                        context.Set<EntityRoot>().CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].RootSkipShared = CreateCollection<EntityRoot>();

                    leftEntities[0].RootSkipShared.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].RootSkipShared.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].RootSkipShared.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].CompositeKeySkipShared = CreateCollection<EntityCompositeKey>();

                    rightEntities[0].CompositeKeySkipShared.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].CompositeKeySkipShared.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].CompositeKeySkipShared.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<EntityCompositeKey>().Where(e => e.Key1 > 7700).Include(e => e.RootSkipShared);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityCompositeKey>()
                        .Select(e => e.Entity).OrderBy(e => e.Key1).ToList();

                    var rightEntities = context.ChangeTracker.Entries<EntityRoot>()
                        .Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            static void ValidateFixup(DbContext context, IList<EntityCompositeKey> leftEntities, IList<EntityRoot> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityCompositeKey>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityRoot>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                Assert.Equal(3, leftEntities[0].RootSkipShared.Count);
                Assert.Single(leftEntities[1].RootSkipShared);
                Assert.Single(leftEntities[2].RootSkipShared);

                Assert.Equal(3, rightEntities[0].CompositeKeySkipShared.Count);
                Assert.Single(rightEntities[1].CompositeKeySkipShared);
                Assert.Single(rightEntities[2].CompositeKeySkipShared);
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_composite_shared_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key1).ToList();
                    var rightEntities = context.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared).ToList();

                    leftEntities[0].RootSkipShared.Add(context.Set<EntityRoot>().CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].RootSkipShared.Add(context.Set<EntityRoot>().CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].RootSkipShared.Add(context.Set<EntityRoot>().CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].CompositeKeySkipShared.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7711;
                                e.Key2 = "7711";
                                e.Key3 = new DateTime(7711, 1, 1);
                            }));
                    rightEntities[0].CompositeKeySkipShared.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7712;
                                e.Key2 = "7712";
                                e.Key3 = new DateTime(7712, 1, 1);
                            }));
                    rightEntities[0].CompositeKeySkipShared.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7713;
                                e.Key2 = "7713";
                                e.Key3 = new DateTime(7713, 1, 1);
                            }));

                    leftEntities[0].RootSkipShared.Remove(leftEntities[0].RootSkipShared.Single(e => e.Id == 9));
                    rightEntities[1].CompositeKeySkipShared.Remove(rightEntities[1].CompositeKeySkipShared.Single(e => e.Key2 == "8_3"));

                    leftEntities[2].RootSkipShared.Remove(leftEntities[2].RootSkipShared.Single(e => e.Id == 16));
                    leftEntities[2].RootSkipShared.Add(context.Set<EntityRoot>().CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[2].CompositeKeySkipShared.Remove(rightEntities[2].CompositeKeySkipShared.Single(e => e.Key2 == "8_2"));
                    rightEntities[2].CompositeKeySkipShared.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7714;
                                e.Key2 = "7714";
                                e.Key3 = new DateTime(7714, 1, 1);
                            }));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 47);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 47 - 4);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key1).ToList();
                    var rightEntities = context.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 47 - 4);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityCompositeKey> leftEntities,
                List<EntityRoot> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityCompositeKey>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityRoot>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].RootSkipShared, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].RootSkipShared, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].RootSkipShared, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].CompositeKeySkipShared, e => e.Key1 == 7711);
                Assert.Contains(rightEntities[0].CompositeKeySkipShared, e => e.Key1 == 7712);
                Assert.Contains(rightEntities[0].CompositeKeySkipShared, e => e.Key1 == 7713);

                Assert.DoesNotContain(leftEntities[0].RootSkipShared, e => e.Id == 9);
                Assert.DoesNotContain(rightEntities[1].CompositeKeySkipShared, e => e.Key2 == "8_3");

                Assert.DoesNotContain(leftEntities[2].RootSkipShared, e => e.Id == 16);
                Assert.Contains(leftEntities[2].RootSkipShared, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].CompositeKeySkipShared, e => e.Key2 == "8_2");
                Assert.Contains(rightEntities[2].CompositeKeySkipShared, e => e.Key1 == 7714);

                var allLeft = context.ChangeTracker.Entries<EntityCompositeKey>().Select(e => e.Entity).OrderBy(e => e.Key1).ToList();
                var allRight = context.ChangeTracker.Entries<EntityRoot>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.RootSkipShared?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.CompositeKeySkipShared);
                            count++;
                        }

                        if (right.CompositeKeySkipShared?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.RootSkipShared);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<Dictionary<string, object>>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_delete_with_many_to_many_composite_shared_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var ones = context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2).ToList();
                    var threes = context.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared).ToList();

                    // Make sure other related entities are loaded for delete fixup
                    context.Set<JoinThreeToCompositeKeyFull>().Load();

                    var toRemoveOne = context.Find<EntityCompositeKey>(6, "6_1", new DateTime(2006, 1, 1));
                    var refCountOnes = threes.SelectMany(e => e.CompositeKeySkipShared).Count(e => e == toRemoveOne);

                    var toRemoveThree = context.Find<EntityRoot>(23);
                    var refCountThrees = ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree);

                    context.Remove(toRemoveOne);
                    context.Remove(toRemoveThree);

                    Assert.Equal(refCountOnes, threes.SelectMany(e => e.CompositeKeySkipShared).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountThrees, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    Assert.Equal(refCountOnes, threes.SelectMany(e => e.CompositeKeySkipShared).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountThrees, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                    Assert.All(
                        context.ChangeTracker.Entries<Dictionary<string, object>>(), e => Assert.Equal(
                            ((int)e.Entity["CompositeId1"] == 6
                                && (string)e.Entity["CompositeId2"] == "6_1"
                                && (DateTime)e.Entity["CompositeId3"] == new DateTime(2006, 1, 1))
                            || (int)e.Entity["RootId"] == 23
                                ? EntityState.Deleted
                                : EntityState.Unchanged, e.State));

                    context.SaveChanges();

                    Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipShared).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                    ones.Remove(toRemoveOne);
                    threes.Remove(toRemoveThree);

                    Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipShared).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                    Assert.DoesNotContain(
                        context.ChangeTracker.Entries<Dictionary<string, object>>(),
                        e => ((int)e.Entity["CompositeId1"] == 6
                                && (string)e.Entity["CompositeId2"] == "6_1"
                                && (DateTime)e.Entity["CompositeId3"] == new DateTime(2006, 1, 1))
                            || (int)e.Entity["RootId"] == 23);
                },
                context =>
                {
                    var ones = context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2).ToList();
                    var threes = context.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared).ToList();

                    ValidateNavigations(ones, threes);

                    Assert.DoesNotContain(
                        context.ChangeTracker.Entries<Dictionary<string, object>>(),
                        e => ((int)e.Entity["CompositeId1"] == 6
                                && (string)e.Entity["CompositeId2"] == "6_1"
                                && (DateTime)e.Entity["CompositeId3"] == new DateTime(2006, 1, 1))
                            || (int)e.Entity["RootId"] == 23);
                });

            static void ValidateNavigations(List<EntityCompositeKey> ones, List<EntityRoot> threes)
            {
                foreach (var one in ones)
                {
                    if (one.RootSkipShared != null)
                    {
                        Assert.DoesNotContain(one.RootSkipShared, e => e.Id == 23);
                    }
                }

                foreach (var three in threes)
                {
                    if (three.CompositeKeySkipShared != null)
                    {
                        Assert.DoesNotContain(
                            three.CompositeKeySkipShared,
                            e => e.Key1 == 6
                                && e.Key2 == "6_1"
                                && e.Key3 == new DateTime(2006, 1, 1));
                    }
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_composite_additional_pk_with_navs(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7711;
                                e.Key2 = "7711";
                                e.Key3 = new DateTime(7711, 1, 1);
                            }),
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7712;
                                e.Key2 = "7712";
                                e.Key3 = new DateTime(7712, 1, 1);
                            }),
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7713;
                                e.Key2 = "7713";
                                e.Key3 = new DateTime(7713, 1, 1);
                            }),
                    };
                    var rightEntities = new[]
                    {
                        context.Set<EntityThree>().CreateInstance((e, p) => e.Id = 7721),
                        context.Set<EntityThree>().CreateInstance((e, p) => e.Id = 7722),
                        context.Set<EntityThree>().CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].ThreeSkipFull = CreateCollection<EntityThree>();

                    leftEntities[0].ThreeSkipFull.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].ThreeSkipFull.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].ThreeSkipFull.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].CompositeKeySkipFull = CreateCollection<EntityCompositeKey>();

                    rightEntities[0].CompositeKeySkipFull.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].CompositeKeySkipFull.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].CompositeKeySkipFull.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<EntityCompositeKey>().Where(e => e.Key1 > 7700).Include(e => e.ThreeSkipFull);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityCompositeKey>()
                        .Select(e => e.Entity).OrderBy(e => e.Key1).ToList();

                    var rightEntities = context.ChangeTracker.Entries<EntityThree>()
                        .Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            static void ValidateFixup(DbContext context, IList<EntityCompositeKey> leftEntities, IList<EntityThree> rightEntities)
            {
                var entries = context.ChangeTracker.Entries();
                Assert.Equal(11, entries.Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityCompositeKey>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>().Count());

                Assert.Equal(3, leftEntities[0].ThreeSkipFull.Count);
                Assert.Single(leftEntities[1].ThreeSkipFull);
                Assert.Single(leftEntities[2].ThreeSkipFull);

                Assert.Equal(3, rightEntities[0].CompositeKeySkipFull.Count);
                Assert.Single(rightEntities[1].CompositeKeySkipFull);
                Assert.Single(rightEntities[2].CompositeKeySkipFull);

                foreach (var joinEntity in context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                    Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                    Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);

                    Assert.Contains(joinEntity, joinEntity.Composite.JoinThreeFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinCompositeKeyFull);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_composite_additional_pk_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key1).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.CompositeKeySkipFull).ToList();

                    leftEntities[0].ThreeSkipFull.Add(context.Set<EntityThree>().CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].ThreeSkipFull.Add(context.Set<EntityThree>().CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].ThreeSkipFull.Add(context.Set<EntityThree>().CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].CompositeKeySkipFull.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7711;
                                e.Key2 = "7711";
                                e.Key3 = new DateTime(7711, 1, 1);
                            }));
                    rightEntities[0].CompositeKeySkipFull.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7712;
                                e.Key2 = "7712";
                                e.Key3 = new DateTime(7712, 1, 1);
                            }));
                    rightEntities[0].CompositeKeySkipFull.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7713;
                                e.Key2 = "7713";
                                e.Key3 = new DateTime(7713, 1, 1);
                            }));

                    leftEntities[0].ThreeSkipFull.Remove(leftEntities[0].ThreeSkipFull.Single(e => e.Id == 2));
                    rightEntities[1].CompositeKeySkipFull.Remove(rightEntities[1].CompositeKeySkipFull.Single(e => e.Key2 == "9_2"));

                    leftEntities[3].ThreeSkipFull.Remove(leftEntities[3].ThreeSkipFull.Single(e => e.Id == 7));
                    leftEntities[3].ThreeSkipFull.Add(context.Set<EntityThree>().CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[2].CompositeKeySkipFull.Remove(rightEntities[2].CompositeKeySkipFull.Single(e => e.Key2 == "6_1"));
                    rightEntities[2].CompositeKeySkipFull.Add(
                        context.EntityCompositeKeys.CreateInstance(
                            (e, p) =>
                            {
                                e.Key1 = 7714;
                                e.Key2 = "7714";
                                e.Key3 = new DateTime(7714, 1, 1);
                            }));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53 - 4);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key1).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.CompositeKeySkipFull).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53 - 4);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityCompositeKey> leftEntities,
                List<EntityThree> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityCompositeKey>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Key1 == 7711);
                Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Key1 == 7712);
                Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Key1 == 7713);

                Assert.DoesNotContain(leftEntities[0].ThreeSkipFull, e => e.Id == 2);
                Assert.DoesNotContain(rightEntities[1].CompositeKeySkipFull, e => e.Key2 == "9_2");

                Assert.DoesNotContain(leftEntities[3].ThreeSkipFull, e => e.Id == 23);
                Assert.Contains(leftEntities[3].ThreeSkipFull, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].CompositeKeySkipFull, e => e.Key2 == "6_1");
                Assert.Contains(rightEntities[2].CompositeKeySkipFull, e => e.Key1 == 7714);

                foreach (var joinEntry in context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>().ToList())
                {
                    var joinEntity = joinEntry.Entity;

                    Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                    Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                    Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);

                    Assert.Contains(joinEntity, joinEntity.Composite.JoinThreeFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinCompositeKeyFull);
                }

                var allLeft = context.ChangeTracker.Entries<EntityCompositeKey>().Select(e => e.Entity).OrderBy(e => e.Key1).ToList();
                var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.ThreeSkipFull?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.CompositeKeySkipFull);
                            count++;
                        }

                        if (right.CompositeKeySkipFull?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.ThreeSkipFull);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_delete_with_many_to_many_composite_additional_pk_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var ones = context.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key2).ToList();
                    var threes = context.Set<EntityThree>().Include(e => e.CompositeKeySkipFull).ToList();

                    // Make sure other related entities are loaded for delete fixup
                    context.Set<JoinThreeToCompositeKeyFull>().Load();

                    var toRemoveOne = context.Find<EntityCompositeKey>(6, "6_1", new DateTime(2006, 1, 1));
                    var refCountOnes = threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne);

                    var toRemoveThree = context.Find<EntityThree>(17);
                    var refCountThrees = ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree);

                    foreach (var joinEntity in context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>().Select(e => e.Entity).ToList())
                    {
                        Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                        Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                        Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                        Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                        Assert.Contains(joinEntity, joinEntity.Composite.JoinThreeFull);
                        Assert.Contains(joinEntity, joinEntity.Three.JoinCompositeKeyFull);
                    }

                    context.Remove(toRemoveOne);
                    context.Remove(toRemoveThree);

                    Assert.Equal(refCountOnes, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountThrees, ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    Assert.Equal(refCountOnes, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountThrees, ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    Assert.All(
                        context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>(), e => Assert.Equal(
                            (e.Entity.CompositeId1 == 6
                                && e.Entity.CompositeId2 == "6_1"
                                && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                            || e.Entity.ThreeId == 17
                                ? EntityState.Deleted
                                : EntityState.Unchanged, e.State));

                    context.SaveChanges();

                    Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    ones.Remove(toRemoveOne);
                    threes.Remove(toRemoveThree);

                    Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree));

                    Assert.DoesNotContain(
                        context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>(),
                        e => (e.Entity.CompositeId1 == 6
                                && e.Entity.CompositeId2 == "6_1"
                                && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                            || e.Entity.ThreeId == 17);
                },
                context =>
                {
                    var ones = context.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key2).ToList();
                    var threes = context.Set<EntityThree>().Include(e => e.CompositeKeySkipFull).ToList();

                    ValidateNavigations(ones, threes);

                    Assert.DoesNotContain(
                        context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>(),
                        e => (e.Entity.CompositeId1 == 6
                                && e.Entity.CompositeId2 == "6_1"
                                && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                            || e.Entity.ThreeId == 17);
                });

            static void ValidateNavigations(List<EntityCompositeKey> ones, List<EntityThree> threes)
            {
                foreach (var one in ones)
                {
                    if (one.ThreeSkipFull != null)
                    {
                        Assert.DoesNotContain(one.ThreeSkipFull, e => e.Id == 17);
                    }

                    if (one.JoinThreeFull != null)
                    {
                        Assert.DoesNotContain(
                            one.JoinThreeFull,
                            e => e.CompositeId1 == 6
                                && e.CompositeId2 == "6_1"
                                && e.CompositeId3 == new DateTime(2006, 1, 1));

                        Assert.DoesNotContain(one.JoinThreeFull, e => e.ThreeId == 17);
                    }
                }

                foreach (var three in threes)
                {
                    if (three.CompositeKeySkipFull != null)
                    {
                        Assert.DoesNotContain(
                            three.CompositeKeySkipFull,
                            e => e.Key1 == 6
                                && e.Key2 == "6_1"
                                && e.Key3 == new DateTime(2006, 1, 1));
                    }

                    if (three.JoinCompositeKeyFull != null)
                    {
                        Assert.DoesNotContain(
                            three.JoinCompositeKeyFull,
                            e => e.CompositeId1 == 6
                                && e.CompositeId2 == "6_1"
                                && e.CompositeId3 == new DateTime(2006, 1, 1));

                        Assert.DoesNotContain(three.JoinCompositeKeyFull, e => e.ThreeId == 17);
                    }
                }
            }

            static void ValidateJoinNavigations(DbContext context)
            {
                foreach (var joinEntity in context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                    Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                    Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);

                    Assert.Contains(joinEntity, joinEntity.Composite.JoinThreeFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinCompositeKeyFull);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_self_shared(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7711),
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7712),
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7713)
                    };
                    var rightEntities = new[]
                    {
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7721),
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7722),
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].SelfSkipSharedLeft = CreateCollection<EntityTwo>();

                    leftEntities[0].SelfSkipSharedLeft.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].SelfSkipSharedLeft.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].SelfSkipSharedLeft.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].SelfSkipSharedRight = CreateCollection<EntityTwo>();

                    rightEntities[0].SelfSkipSharedRight.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].SelfSkipSharedRight.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].SelfSkipSharedRight.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<EntityTwo>().Where(e => e.Id > 7700).Include(e => e.SelfSkipSharedLeft);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(6, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityTwo>()
                        .Select(e => e.Entity)
                        .Where(e => e.Id < 7720)
                        .OrderBy(e => e.Id)
                        .ToList();

                    var rightEntities = context.ChangeTracker.Entries<EntityTwo>()
                        .Select(e => e.Entity)
                        .Where(e => e.Id > 7720)
                        .OrderBy(e => e.Id)
                        .ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            static void ValidateFixup(DbContext context, IList<EntityTwo> leftEntities, IList<EntityTwo> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(6, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                Assert.Equal(3, leftEntities[0].SelfSkipSharedLeft.Count);
                Assert.Single(leftEntities[1].SelfSkipSharedLeft);
                Assert.Single(leftEntities[2].SelfSkipSharedLeft);

                Assert.Equal(3, rightEntities[0].SelfSkipSharedRight.Count);
                Assert.Single(rightEntities[1].SelfSkipSharedRight);
                Assert.Single(rightEntities[2].SelfSkipSharedRight);
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_self()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityTwo>().Include(e => e.SelfSkipSharedRight).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.SelfSkipSharedLeft).ToList();

                    leftEntities[0].SelfSkipSharedRight.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].SelfSkipSharedRight.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].SelfSkipSharedRight.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].SelfSkipSharedLeft.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7711));
                    rightEntities[0].SelfSkipSharedLeft.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7712));
                    rightEntities[0].SelfSkipSharedLeft.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7713));

                    leftEntities[2].SelfSkipSharedRight.Remove(leftEntities[2].SelfSkipSharedRight.Single(e => e.Id == 5));
                    rightEntities[1].SelfSkipSharedLeft.Remove(rightEntities[1].SelfSkipSharedLeft.Single(e => e.Id == 8));

                    leftEntities[4].SelfSkipSharedRight.Remove(leftEntities[4].SelfSkipSharedRight.Single(e => e.Id == 8));
                    leftEntities[4].SelfSkipSharedRight.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[3].SelfSkipSharedLeft.Remove(rightEntities[3].SelfSkipSharedLeft.Single(e => e.Id == 9));
                    rightEntities[3].SelfSkipSharedLeft.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7714));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, 28, 42);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 28, 42 - 4);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityTwo>().Include(e => e.SelfSkipSharedRight).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.SelfSkipSharedLeft).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 28, 42 - 4);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityTwo> leftEntities,
                List<EntityTwo> rightEntities,
                int count,
                int joinCount)
            {
                Assert.Equal(count, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());
                Assert.Equal(count + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].SelfSkipSharedLeft, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].SelfSkipSharedLeft, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].SelfSkipSharedLeft, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[2].SelfSkipSharedRight, e => e.Id == 5);
                Assert.DoesNotContain(rightEntities[1].SelfSkipSharedLeft, e => e.Id == 8);

                Assert.DoesNotContain(leftEntities[4].SelfSkipSharedRight, e => e.Id == 8);
                Assert.Contains(leftEntities[4].SelfSkipSharedRight, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[3].SelfSkipSharedLeft, e => e.Id == 9);
                Assert.Contains(rightEntities[3].SelfSkipSharedLeft, e => e.Id == 7714);

                var allLeft = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var joins = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.SelfSkipSharedRight?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.SelfSkipSharedLeft);
                            joins++;
                        }

                        if (right.SelfSkipSharedLeft?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.SelfSkipSharedRight);
                            joins++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<Dictionary<string, object>>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (joins / 2) + deleted);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_with_navs(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7711),
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7712),
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7713)
                    };
                    var rightEntities = new[]
                    {
                        context.EntityThrees.CreateInstance((e, p) => e.Id = 7721),
                        context.EntityThrees.CreateInstance((e, p) => e.Id = 7722),
                        context.EntityThrees.CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].ThreeSkipFull = CreateCollection<EntityThree>();

                    leftEntities[0].ThreeSkipFull.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].ThreeSkipFull.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].ThreeSkipFull.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].TwoSkipFull = CreateCollection<EntityTwo>();

                    rightEntities[0].TwoSkipFull.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].TwoSkipFull.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].TwoSkipFull.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<EntityTwo>().Where(e => e.Id > 7700).Include(e => e.ThreeSkipFull);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            static void ValidateFixup(DbContext context, IList<EntityTwo> leftEntities, IList<EntityThree> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinTwoToThree>().Count());

                Assert.Equal(3, leftEntities[0].ThreeSkipFull.Count);
                Assert.Single(leftEntities[1].ThreeSkipFull);
                Assert.Single(leftEntities[2].ThreeSkipFull);

                Assert.Equal(3, rightEntities[0].TwoSkipFull.Count);
                Assert.Single(rightEntities[1].TwoSkipFull);
                Assert.Single(rightEntities[2].TwoSkipFull);

                foreach (var joinEntity in context.ChangeTracker.Entries<JoinTwoToThree>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.Two.Id, joinEntity.TwoId);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                    Assert.Contains(joinEntity, joinEntity.Two.JoinThreeFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinTwoFull);
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityTwo>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.TwoSkipFull).ToList();

                    leftEntities[0].ThreeSkipFull.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].ThreeSkipFull.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].ThreeSkipFull.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].TwoSkipFull.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7711));
                    rightEntities[0].TwoSkipFull.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7712));
                    rightEntities[0].TwoSkipFull.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7713));

                    leftEntities[1].ThreeSkipFull.Remove(leftEntities[1].ThreeSkipFull.Single(e => e.Id == 9));
                    rightEntities[1].TwoSkipFull.Remove(rightEntities[1].TwoSkipFull.Single(e => e.Id == 4));

                    leftEntities[2].ThreeSkipFull.Remove(leftEntities[2].ThreeSkipFull.Single(e => e.Id == 11));
                    leftEntities[2].ThreeSkipFull.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[2].TwoSkipFull.Remove(rightEntities[2].TwoSkipFull.Single(e => e.Id == 6));
                    rightEntities[2].TwoSkipFull.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7714));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 60);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 60 - 4);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityTwo>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.TwoSkipFull).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 60 - 4);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityTwo> leftEntities,
                List<EntityThree> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinTwoToThree>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].TwoSkipFull, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].TwoSkipFull, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].TwoSkipFull, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[1].ThreeSkipFull, e => e.Id == 9);
                Assert.DoesNotContain(rightEntities[1].TwoSkipFull, e => e.Id == 4);

                Assert.DoesNotContain(leftEntities[2].ThreeSkipFull, e => e.Id == 11);
                Assert.Contains(leftEntities[2].ThreeSkipFull, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].TwoSkipFull, e => e.Id == 6);
                Assert.Contains(rightEntities[2].TwoSkipFull, e => e.Id == 7714);

                foreach (var joinEntry in context.ChangeTracker.Entries<JoinTwoToThree>().ToList())
                {
                    var joinEntity = joinEntry.Entity;
                    Assert.Equal(joinEntity.Two.Id, joinEntity.TwoId);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                    Assert.Contains(joinEntity, joinEntity.Two.JoinThreeFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinTwoFull);
                }

                var allLeft = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.ThreeSkipFull?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.TwoSkipFull);
                            count++;
                        }

                        if (right.TwoSkipFull?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.ThreeSkipFull);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinTwoToThree>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_with_inheritance(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7711),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7712),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7713),
                    };
                    var rightEntities = new[]
                    {
                        context.Set<EntityBranch>().CreateInstance((e, p) => e.Id = 7721),
                        context.Set<EntityBranch>().CreateInstance((e, p) => e.Id = 7722),
                        context.Set<EntityBranch>().CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].BranchSkip = CreateCollection<EntityBranch>();

                    leftEntities[0].BranchSkip.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].BranchSkip.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].BranchSkip.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkip = CreateCollection<EntityOne>();

                    rightEntities[0].OneSkip.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkip.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkip.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.BranchSkip);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityBranch>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            static void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityBranch> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityBranch>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinOneToBranch>().Count());

                Assert.Equal(3, leftEntities[0].BranchSkip.Count);
                Assert.Single(leftEntities[1].BranchSkip);
                Assert.Single(leftEntities[2].BranchSkip);

                Assert.Equal(3, rightEntities[0].OneSkip.Count);
                Assert.Single(rightEntities[1].OneSkip);
                Assert.Single(rightEntities[2].OneSkip);
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_with_inheritance()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.BranchSkip).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityBranch>().Include(e => e.OneSkip).ToList();

                    leftEntities[0].BranchSkip.Add(context.Set<EntityBranch>().CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].BranchSkip.Add(context.Set<EntityBranch>().CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].BranchSkip.Add(context.Set<EntityBranch>().CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].OneSkip.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7711));
                    rightEntities[0].OneSkip.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7712));
                    rightEntities[0].OneSkip.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7713));

                    leftEntities[1].BranchSkip.Remove(leftEntities[1].BranchSkip.Single(e => e.Id == 16));
                    rightEntities[1].OneSkip.Remove(rightEntities[1].OneSkip.Single(e => e.Id == 9));

                    leftEntities[2].BranchSkip.Remove(leftEntities[2].BranchSkip.Single(e => e.Id == 14));
                    leftEntities[2].BranchSkip.Add(context.Set<EntityBranch>().CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[2].OneSkip.Remove(rightEntities[2].OneSkip.Single(e => e.Id == 8));
                    rightEntities[2].OneSkip.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7714));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, 24, 14, 55);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 14, 55 - 4);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.BranchSkip).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityBranch>().Include(e => e.OneSkip).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 14, 55 - 4);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityBranch> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityBranch>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinOneToBranch>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].BranchSkip, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].BranchSkip, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].BranchSkip, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[1].BranchSkip, e => e.Id == 1);
                Assert.DoesNotContain(rightEntities[1].OneSkip, e => e.Id == 1);

                Assert.DoesNotContain(leftEntities[2].BranchSkip, e => e.Id == 1);
                Assert.Contains(leftEntities[2].BranchSkip, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].OneSkip, e => e.Id == 1);
                Assert.Contains(rightEntities[2].OneSkip, e => e.Id == 7714);

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityBranch>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.BranchSkip?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.OneSkip);
                            count++;
                        }

                        if (right.OneSkip?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.BranchSkip);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinOneToBranch>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_self_with_payload(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7711),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7712),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7713)
                    };
                    var rightEntities = new[]
                    {
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7721),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7722),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].SelfSkipPayloadLeft = CreateCollection<EntityOne>();

                    leftEntities[0].SelfSkipPayloadLeft.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].SelfSkipPayloadLeft.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].SelfSkipPayloadLeft.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].SelfSkipPayloadRight = CreateCollection<EntityOne>();

                    rightEntities[0].SelfSkipPayloadRight.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].SelfSkipPayloadRight.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].SelfSkipPayloadRight.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities, postSave: false);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                },
                async context =>
                {
                    var queryable = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.SelfSkipPayloadLeft);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(6, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>()
                        .Select(e => e.Entity)
                        .Where(e => e.Id < 7720)
                        .OrderBy(e => e.Id)
                        .ToList();

                    var rightEntities = context.ChangeTracker.Entries<EntityOne>()
                        .Select(e => e.Entity)
                        .Where(e => e.Id > 7720)
                        .OrderBy(e => e.Id)
                        .ToList();

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                });

            void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityOne> rightEntities, bool postSave)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(6, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinOneSelfPayload>().Count());

                Assert.Equal(3, leftEntities[0].SelfSkipPayloadLeft.Count);
                Assert.Single(leftEntities[1].SelfSkipPayloadLeft);
                Assert.Single(leftEntities[2].SelfSkipPayloadLeft);

                Assert.Equal(3, rightEntities[0].SelfSkipPayloadRight.Count);
                Assert.Single(rightEntities[1].SelfSkipPayloadRight);
                Assert.Single(rightEntities[2].SelfSkipPayloadRight);

                foreach (var joinEntity in context.ChangeTracker.Entries<JoinOneSelfPayload>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.Left.Id, joinEntity.LeftId);
                    Assert.Equal(joinEntity.Right.Id, joinEntity.RightId);
                    Assert.Contains(joinEntity, joinEntity.Left.JoinSelfPayloadLeft);
                    Assert.Contains(joinEntity, joinEntity.Right.JoinSelfPayloadRight);

                    if (postSave
                        && SupportsDatabaseDefaults)
                    {
                        Assert.True(joinEntity.Payload >= DateTime.Now - new TimeSpan(7, 0, 0, 0));
                    }
                    else
                    {
                        Assert.Equal(default, joinEntity.Payload);
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_self_with_payload()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.SelfSkipPayloadRight).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityOne>().Include(e => e.SelfSkipPayloadLeft).ToList();

                    leftEntities[0].SelfSkipPayloadRight.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].SelfSkipPayloadRight.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].SelfSkipPayloadRight.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].SelfSkipPayloadLeft.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7711));
                    rightEntities[0].SelfSkipPayloadLeft.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7712));
                    rightEntities[0].SelfSkipPayloadLeft.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7713));

                    leftEntities[2].SelfSkipPayloadRight.Remove(leftEntities[2].SelfSkipPayloadRight.Single(e => e.Id == 6));
                    rightEntities[1].SelfSkipPayloadLeft.Remove(rightEntities[1].SelfSkipPayloadLeft.Single(e => e.Id == 13));

                    leftEntities[4].SelfSkipPayloadRight.Remove(leftEntities[4].SelfSkipPayloadRight.Single(e => e.Id == 3));
                    leftEntities[4].SelfSkipPayloadRight.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[4].SelfSkipPayloadLeft.Remove(rightEntities[4].SelfSkipPayloadLeft.Single(e => e.Id == 6));
                    rightEntities[4].SelfSkipPayloadLeft.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7714));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    context.Find<JoinOneSelfPayload>(7712, 1).Payload = new DateTime(1973, 9, 3);
                    context.Find<JoinOneSelfPayload>(20, 16).Payload = new DateTime(1969, 8, 3);

                    ValidateFixup(context, leftEntities, rightEntities, 28, 37, postSave: false);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 28, 37 - 4, postSave: true);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.SelfSkipPayloadRight).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityOne>().Include(e => e.SelfSkipPayloadLeft).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 28, 37 - 4, postSave: true);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityOne> rightEntities,
                int count,
                int joinCount,
                bool postSave)
            {
                Assert.Equal(count, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinOneSelfPayload>().Count());
                Assert.Equal(count + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].SelfSkipPayloadRight, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].SelfSkipPayloadRight, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].SelfSkipPayloadRight, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[2].SelfSkipPayloadRight, e => e.Id == 6);
                Assert.DoesNotContain(rightEntities[1].SelfSkipPayloadLeft, e => e.Id == 13);

                Assert.DoesNotContain(leftEntities[4].SelfSkipPayloadRight, e => e.Id == 3);
                Assert.Contains(leftEntities[4].SelfSkipPayloadRight, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[4].SelfSkipPayloadLeft, e => e.Id == 6);
                Assert.Contains(rightEntities[4].SelfSkipPayloadLeft, e => e.Id == 7714);

                foreach (var joinEntry in context.ChangeTracker.Entries<JoinOneSelfPayload>().ToList())
                {
                    var joinEntity = joinEntry.Entity;
                    Assert.Equal(joinEntity.Left.Id, joinEntity.LeftId);
                    Assert.Equal(joinEntity.Right.Id, joinEntity.RightId);
                    Assert.Contains(joinEntity, joinEntity.Left.JoinSelfPayloadLeft);
                    Assert.Contains(joinEntity, joinEntity.Right.JoinSelfPayloadRight);

                    if (joinEntity.LeftId == 7712
                        && joinEntity.RightId == 1)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Added, joinEntry.State);
                        Assert.Equal(new DateTime(1973, 9, 3), joinEntity.Payload);
                    }
                    else if (joinEntity.LeftId == 20
                        && joinEntity.RightId == 20)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Modified, joinEntry.State);
                        Assert.Equal(!postSave, joinEntry.Property(e => e.Payload).IsModified);
                        Assert.Equal(new DateTime(1969, 8, 3), joinEntity.Payload);
                    }
                }

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var joins = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.SelfSkipPayloadRight?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.SelfSkipPayloadLeft);
                            joins++;
                        }

                        if (right.SelfSkipPayloadLeft?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.SelfSkipPayloadRight);
                            joins++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinOneSelfPayload>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (joins / 2) + deleted);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_shared_with_payload(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7711),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7712),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7713)
                    };
                    var rightEntities = new[]
                    {
                        context.EntityThrees.CreateInstance((e, p) => e.Id = 7721),
                        context.EntityThrees.CreateInstance((e, p) => e.Id = 7722),
                        context.EntityThrees.CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].ThreeSkipPayloadFullShared = CreateCollection<EntityThree>();

                    leftEntities[0].ThreeSkipPayloadFullShared.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].ThreeSkipPayloadFullShared.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].ThreeSkipPayloadFullShared.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkipPayloadFullShared = CreateCollection<EntityOne>();

                    rightEntities[0].OneSkipPayloadFullShared.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkipPayloadFullShared.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkipPayloadFullShared.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities, postSave: false);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                },
                async context =>
                {
                    var queryable = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.ThreeSkipPayloadFullShared);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                });

            void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityThree> rightEntities, bool postSave)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                Assert.Equal(3, leftEntities[0].ThreeSkipPayloadFullShared.Count);
                Assert.Single(leftEntities[1].ThreeSkipPayloadFullShared);
                Assert.Single(leftEntities[2].ThreeSkipPayloadFullShared);

                Assert.Equal(3, rightEntities[0].OneSkipPayloadFullShared.Count);
                Assert.Single(rightEntities[1].OneSkipPayloadFullShared);
                Assert.Single(rightEntities[2].OneSkipPayloadFullShared);

                if (postSave
                    && SupportsDatabaseDefaults)
                {
                    foreach (var joinEntity in context.ChangeTracker
                        .Entries<Dictionary<string, object>>().Select(e => e.Entity).ToList())
                    {
                        Assert.Equal("Generated", joinEntity["Payload"]);
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_shared_with_payload()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFullShared).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared).ToList();

                    leftEntities[0].ThreeSkipPayloadFullShared.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].ThreeSkipPayloadFullShared.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].ThreeSkipPayloadFullShared.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].OneSkipPayloadFullShared.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7711));
                    rightEntities[0].OneSkipPayloadFullShared.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7712));
                    rightEntities[0].OneSkipPayloadFullShared.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7713));

                    leftEntities[2].ThreeSkipPayloadFullShared.Remove(leftEntities[2].ThreeSkipPayloadFullShared.Single(e => e.Id == 2));
                    rightEntities[4].OneSkipPayloadFullShared.Remove(rightEntities[4].OneSkipPayloadFullShared.Single(e => e.Id == 4));

                    leftEntities[3].ThreeSkipPayloadFullShared.Remove(leftEntities[3].ThreeSkipPayloadFullShared.Single(e => e.Id == 18));
                    leftEntities[3].ThreeSkipPayloadFullShared.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[2].OneSkipPayloadFullShared.Remove(rightEntities[2].OneSkipPayloadFullShared.Single(e => e.Id == 13));
                    rightEntities[2].OneSkipPayloadFullShared.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7714));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared").Find(7712, 1)["Payload"] = "Set!";
                    context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared").Find(20, 16)["Payload"] = "Changed!";

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48, postSave: false);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48 - 4, postSave: true);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFullShared).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48 - 4, postSave: true);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityThree> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount,
                bool postSave)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].OneSkipPayloadFullShared, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].OneSkipPayloadFullShared, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].OneSkipPayloadFullShared, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[2].ThreeSkipPayloadFullShared, e => e.Id == 2);
                Assert.DoesNotContain(rightEntities[4].OneSkipPayloadFullShared, e => e.Id == 4);

                Assert.DoesNotContain(leftEntities[3].ThreeSkipPayloadFullShared, e => e.Id == 18);
                Assert.Contains(leftEntities[3].ThreeSkipPayloadFullShared, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].OneSkipPayloadFullShared, e => e.Id == 13);
                Assert.Contains(rightEntities[2].OneSkipPayloadFullShared, e => e.Id == 7714);

                foreach (var joinEntry in context.ChangeTracker.Entries<Dictionary<string, object>>().ToList())
                {
                    var joinEntity = joinEntry.Entity;

                    if ((int)joinEntity["OneId"] == 7712
                        && (int)joinEntity["ThreeId"] == 1)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Added, joinEntry.State);
                        Assert.Equal("Set!", joinEntity["Payload"]);
                    }
                    else if ((int)joinEntity["OneId"] == 20
                        && (int)joinEntity["ThreeId"] == 20)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Modified, joinEntry.State);
                        Assert.Equal(!postSave, joinEntry.Property("Payload").IsModified);
                        Assert.Equal("Changed!", joinEntity["Payload"]);
                    }
                }

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.ThreeSkipPayloadFullShared?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.OneSkipPayloadFullShared);
                            count++;
                        }

                        if (right.OneSkipPayloadFullShared?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.ThreeSkipPayloadFullShared);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<Dictionary<string, object>>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_shared(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7711),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7712),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7713)
                    };
                    var rightEntities = new[]
                    {
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7721),
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7722),
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].TwoSkipShared = CreateCollection<EntityTwo>();

                    leftEntities[0].TwoSkipShared.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].TwoSkipShared.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].TwoSkipShared.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkipShared = CreateCollection<EntityOne>();

                    rightEntities[0].OneSkipShared.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkipShared.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkipShared.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.TwoSkipShared);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            static void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityTwo> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                Assert.Equal(3, leftEntities[0].TwoSkipShared.Count);
                Assert.Single(leftEntities[1].TwoSkipShared);
                Assert.Single(leftEntities[2].TwoSkipShared);

                Assert.Equal(3, rightEntities[0].OneSkipShared.Count);
                Assert.Single(rightEntities[1].OneSkipShared);
                Assert.Single(rightEntities[2].OneSkipShared);
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_shared()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.TwoSkipShared).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.OneSkipShared).ToList();

                    leftEntities[0].TwoSkipShared.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].TwoSkipShared.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].TwoSkipShared.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].OneSkipShared.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7711));
                    rightEntities[0].OneSkipShared.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7712));
                    rightEntities[0].OneSkipShared.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7713));

                    leftEntities[1].TwoSkipShared.Remove(leftEntities[1].TwoSkipShared.Single(e => e.Id == 3));
                    rightEntities[1].OneSkipShared.Remove(rightEntities[1].OneSkipShared.Single(e => e.Id == 5));

                    leftEntities[2].TwoSkipShared.Remove(leftEntities[2].TwoSkipShared.Single(e => e.Id == 10));
                    leftEntities[2].TwoSkipShared.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[2].OneSkipShared.Remove(rightEntities[2].OneSkipShared.Single(e => e.Id == 7));
                    rightEntities[2].OneSkipShared.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7714));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 49);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.TwoSkipShared).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.OneSkipShared).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 49);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityTwo> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].OneSkipShared, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].OneSkipShared, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].OneSkipShared, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[1].TwoSkipShared, e => e.Id == 3);
                Assert.DoesNotContain(rightEntities[1].OneSkipShared, e => e.Id == 6);

                Assert.DoesNotContain(leftEntities[2].TwoSkipShared, e => e.Id == 10);
                Assert.Contains(leftEntities[2].TwoSkipShared, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].OneSkipShared, e => e.Id == 7);
                Assert.Contains(rightEntities[2].OneSkipShared, e => e.Id == 7714);

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.TwoSkipShared?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.OneSkipShared);
                            count++;
                        }

                        if (right.OneSkipShared?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.TwoSkipShared);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<Dictionary<string, object>>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_with_payload(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7711),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7712),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7713)
                    };
                    var rightEntities = new[]
                    {
                        context.EntityThrees.CreateInstance((e, p) => e.Id = 7721),
                        context.EntityThrees.CreateInstance((e, p) => e.Id = 7722),
                        context.EntityThrees.CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].ThreeSkipPayloadFull = CreateCollection<EntityThree>();

                    leftEntities[0].ThreeSkipPayloadFull.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].ThreeSkipPayloadFull.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].ThreeSkipPayloadFull.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkipPayloadFull = CreateCollection<EntityOne>();

                    rightEntities[0].OneSkipPayloadFull.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkipPayloadFull.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkipPayloadFull.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities, postSave: false);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                },
                async context =>
                {
                    var queryable = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.ThreeSkipPayloadFull);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, postSave: true);
                });

            void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityThree> rightEntities, bool postSave)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Count());

                Assert.Equal(3, leftEntities[0].ThreeSkipPayloadFull.Count);
                Assert.Single(leftEntities[1].ThreeSkipPayloadFull);
                Assert.Single(leftEntities[2].ThreeSkipPayloadFull);

                Assert.Equal(3, rightEntities[0].OneSkipPayloadFull.Count);
                Assert.Single(rightEntities[1].OneSkipPayloadFull);
                Assert.Single(rightEntities[2].OneSkipPayloadFull);

                foreach (var joinEntity in context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.One.Id, joinEntity.OneId);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                    Assert.Contains(joinEntity, joinEntity.One.JoinThreePayloadFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinOnePayloadFull);

                    if (postSave && SupportsDatabaseDefaults)
                    {
                        Assert.Equal("Generated", joinEntity.Payload);
                    }
                }
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many_with_payload()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ToList();

                    leftEntities[0].ThreeSkipPayloadFull.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].ThreeSkipPayloadFull.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].ThreeSkipPayloadFull.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].OneSkipPayloadFull.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7711));
                    rightEntities[0].OneSkipPayloadFull.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7712));
                    rightEntities[0].OneSkipPayloadFull.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7713));

                    leftEntities[1].ThreeSkipPayloadFull.Remove(leftEntities[1].ThreeSkipPayloadFull.Single(e => e.Id == 9));
                    rightEntities[1].OneSkipPayloadFull.Remove(rightEntities[1].OneSkipPayloadFull.Single(e => e.Id == 4));

                    leftEntities[2].ThreeSkipPayloadFull.Remove(leftEntities[2].ThreeSkipPayloadFull.Single(e => e.Id == 5));
                    leftEntities[2].ThreeSkipPayloadFull.Add(context.EntityThrees.CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[2].OneSkipPayloadFull.Remove(rightEntities[2].OneSkipPayloadFull.Single(e => e.Id == 8));
                    rightEntities[2].OneSkipPayloadFull.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7714));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    context.Find<JoinOneToThreePayloadFull>(7712, 1).Payload = "Set!";
                    context.Find<JoinOneToThreePayloadFull>(20, 20).Payload = "Changed!";

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 123, postSave: false);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 123 - 4, postSave: true);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 123 - 4, postSave: true);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityThree> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount,
                bool postSave)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityThree>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].ThreeSkipPayloadFull, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].ThreeSkipPayloadFull, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].ThreeSkipPayloadFull, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].OneSkipPayloadFull, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].OneSkipPayloadFull, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].OneSkipPayloadFull, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[1].ThreeSkipPayloadFull, e => e.Id == 9);
                Assert.DoesNotContain(rightEntities[1].OneSkipPayloadFull, e => e.Id == 4);

                Assert.DoesNotContain(leftEntities[2].ThreeSkipPayloadFull, e => e.Id == 5);
                Assert.Contains(leftEntities[2].ThreeSkipPayloadFull, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].OneSkipPayloadFull, e => e.Id == 8);
                Assert.Contains(rightEntities[2].OneSkipPayloadFull, e => e.Id == 7714);

                foreach (var joinEntry in context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().ToList())
                {
                    var joinEntity = joinEntry.Entity;
                    Assert.Equal(joinEntity.One.Id, joinEntity.OneId);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                    Assert.Contains(joinEntity, joinEntity.One.JoinThreePayloadFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinOnePayloadFull);

                    if (joinEntity.OneId == 7712
                        && joinEntity.ThreeId == 1)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Added, joinEntry.State);
                        Assert.Equal("Set!", joinEntity.Payload);
                    }
                    else if (joinEntity.OneId == 20
                        && joinEntity.ThreeId == 20)
                    {
                        Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Modified, joinEntry.State);
                        Assert.Equal(!postSave, joinEntry.Property(e => e.Payload).IsModified);
                        Assert.Equal("Changed!", joinEntity.Payload);
                    }
                }

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.ThreeSkipPayloadFull?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.OneSkipPayloadFull);
                            count++;
                        }

                        if (right.OneSkipPayloadFull?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.ThreeSkipPayloadFull);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
            }
        }

        [ConditionalFact]
        public virtual void Can_delete_with_many_to_many_with_navs()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var ones = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Id).ToList();
                    var threes = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ToList();

                    // Make sure other related entities are loaded for delete fixup
                    context.Set<EntityTwo>().Load();
                    context.Set<JoinOneSelfPayload>().Load();
                    context.Set<JoinOneToTwo>().Load();

                    var toRemoveOne = context.Find<EntityOne>(1);
                    var refCountOnes = threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne);

                    var toRemoveThree = context.Find<EntityThree>(1);
                    var refCountThrees = ones.SelectMany(e => e.ThreeSkipPayloadFull).Count(e => e == toRemoveThree);

                    foreach (var joinEntity in context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Select(e => e.Entity).ToList())
                    {
                        Assert.Equal(joinEntity.One.Id, joinEntity.OneId);
                        Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                        Assert.Contains(joinEntity, joinEntity.One.JoinThreePayloadFull);
                        Assert.Contains(joinEntity, joinEntity.Three.JoinOnePayloadFull);
                    }

                    context.Remove(toRemoveOne);
                    context.Remove(toRemoveThree);

                    Assert.Equal(refCountOnes, threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountThrees, ones.SelectMany(e => e.ThreeSkipPayloadFull).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    Assert.Equal(refCountOnes, threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountThrees, ones.SelectMany(e => e.ThreeSkipPayloadFull).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    Assert.All(
                        context.ChangeTracker.Entries<JoinOneToThreePayloadFull>(), e => Assert.Equal(
                            e.Entity.OneId == 1
                            || e.Entity.ThreeId == 1
                                ? EntityState.Deleted
                                : EntityState.Unchanged, e.State));

                    context.SaveChanges();

                    Assert.Equal(0, threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.ThreeSkipPayloadFull).Count(e => e == toRemoveThree));

                    ValidateJoinNavigations(context);

                    ones.Remove(toRemoveOne);
                    threes.Remove(toRemoveThree);

                    Assert.Equal(0, threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.ThreeSkipPayloadFull).Count(e => e == toRemoveThree));

                    Assert.DoesNotContain(
                        context.ChangeTracker.Entries<JoinOneToThreePayloadFull>(),
                        e => e.Entity.OneId == 1 || e.Entity.ThreeId == 1);
                },
                context =>
                {
                    var ones = context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Id).ToList();
                    var threes = context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).ToList();

                    ValidateNavigations(ones, threes);

                    Assert.DoesNotContain(
                        context.ChangeTracker.Entries<JoinOneToThreePayloadFull>(),
                        e => e.Entity.OneId == 1 || e.Entity.ThreeId == 1);
                });

            static void ValidateNavigations(List<EntityOne> ones, List<EntityThree> threes)
            {
                foreach (var one in ones)
                {
                    if (one.ThreeSkipPayloadFull != null)
                    {
                        Assert.DoesNotContain(one.ThreeSkipPayloadFull, e => e.Id == 1);
                    }

                    if (one.JoinThreePayloadFull != null)
                    {
                        Assert.DoesNotContain(one.JoinThreePayloadFull, e => e.OneId == 1);
                        Assert.DoesNotContain(one.JoinThreePayloadFull, e => e.ThreeId == 1);
                    }
                }

                foreach (var three in threes)
                {
                    if (three.OneSkipPayloadFull != null)
                    {
                        Assert.DoesNotContain(three.OneSkipPayloadFull, e => e.Id == 1);
                    }

                    if (three.JoinOnePayloadFull != null)
                    {
                        Assert.DoesNotContain(three.JoinOnePayloadFull, e => e.OneId == 1);
                        Assert.DoesNotContain(three.JoinOnePayloadFull, e => e.ThreeId == 1);
                    }
                }
            }

            static void ValidateJoinNavigations(DbContext context)
            {
                foreach (var joinEntity in context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Select(e => e.Entity).ToList())
                {
                    Assert.Equal(joinEntity.One.Id, joinEntity.OneId);
                    Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                    Assert.Contains(joinEntity, joinEntity.One.JoinThreePayloadFull);
                    Assert.Contains(joinEntity, joinEntity.Three.JoinOnePayloadFull);
                }
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7711),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7712),
                        context.EntityOnes.CreateInstance((e, p) => e.Id = 7713)
                    };
                    var rightEntities = new[]
                    {
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7721),
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7722),
                        context.EntityTwos.CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].TwoSkip = CreateCollection<EntityTwo>();

                    leftEntities[0].TwoSkip.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].TwoSkip.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].TwoSkip.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].OneSkip = CreateCollection<EntityOne>();

                    rightEntities[0].OneSkip.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].OneSkip.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].OneSkip.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<EntityOne>().Where(e => e.Id > 7700).Include(e => e.TwoSkip);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            static void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityTwo> rightEntities)
            {
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
            }
        }

        [ConditionalFact]
        public virtual void Can_update_many_to_many()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.OneSkip).ToList();

                    leftEntities[0].TwoSkip.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7721));
                    leftEntities[0].TwoSkip.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7722));
                    leftEntities[0].TwoSkip.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7723));

                    rightEntities[0].OneSkip.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7711));
                    rightEntities[0].OneSkip.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7712));
                    rightEntities[0].OneSkip.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7713));

                    leftEntities[1].TwoSkip.Remove(leftEntities[1].TwoSkip.Single(e => e.Id == 1));
                    rightEntities[1].OneSkip.Remove(rightEntities[1].OneSkip.Single(e => e.Id == 1));

                    leftEntities[2].TwoSkip.Remove(leftEntities[2].TwoSkip.Single(e => e.Id == 1));
                    leftEntities[2].TwoSkip.Add(context.EntityTwos.CreateInstance((e, p) => e.Id = 7724));

                    rightEntities[2].OneSkip.Remove(rightEntities[2].OneSkip.Single(e => e.Id == 1));
                    rightEntities[2].OneSkip.Add(context.EntityOnes.CreateInstance((e, p) => e.Id = 7714));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 120);

                    context.SaveChanges();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 116);
                },
                context =>
                {
                    var leftEntities = context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Id).ToList();
                    var rightEntities = context.Set<EntityTwo>().Include(e => e.OneSkip).ToList();

                    ValidateFixup(context, leftEntities, rightEntities, 24, 24, 116);
                });

            static void ValidateFixup(
                DbContext context,
                List<EntityOne> leftEntities,
                List<EntityTwo> rightEntities,
                int leftCount,
                int rightCount,
                int joinCount)
            {
                Assert.Equal(leftCount, context.ChangeTracker.Entries<EntityOne>().Count());
                Assert.Equal(rightCount, context.ChangeTracker.Entries<EntityTwo>().Count());
                Assert.Equal(joinCount, context.ChangeTracker.Entries<JoinOneToTwo>().Count());
                Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

                Assert.Contains(leftEntities[0].TwoSkip, e => e.Id == 7721);
                Assert.Contains(leftEntities[0].TwoSkip, e => e.Id == 7722);
                Assert.Contains(leftEntities[0].TwoSkip, e => e.Id == 7723);

                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7711);
                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7712);
                Assert.Contains(rightEntities[0].OneSkip, e => e.Id == 7713);

                Assert.DoesNotContain(leftEntities[1].TwoSkip, e => e.Id == 1);
                Assert.DoesNotContain(rightEntities[1].OneSkip, e => e.Id == 1);

                Assert.DoesNotContain(leftEntities[2].TwoSkip, e => e.Id == 1);
                Assert.Contains(leftEntities[2].TwoSkip, e => e.Id == 7724);

                Assert.DoesNotContain(rightEntities[2].OneSkip, e => e.Id == 1);
                Assert.Contains(rightEntities[2].OneSkip, e => e.Id == 7714);

                var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();
                var allRight = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Id).ToList();

                var count = 0;
                foreach (var left in allLeft)
                {
                    foreach (var right in allRight)
                    {
                        if (left.TwoSkip?.Contains(right) == true)
                        {
                            Assert.Contains(left, right.OneSkip);
                            count++;
                        }

                        if (right.OneSkip?.Contains(left) == true)
                        {
                            Assert.Contains(right, left.TwoSkip);
                            count++;
                        }
                    }
                }

                var deleted = context.ChangeTracker.Entries<JoinOneToTwo>().Count(e => e.State == EntityState.Deleted);
                Assert.Equal(joinCount, (count / 2) + deleted);
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

                    var toRemoveOne = context.Find<EntityOne>(1);
                    var refCountOnes = twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne);

                    var toRemoveTwo = context.Find<EntityTwo>(1);
                    var refCountTwos = ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo);

                    context.Remove(toRemoveOne);
                    context.Remove(toRemoveTwo);

                    Assert.Equal(refCountOnes, twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountTwos, ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo));

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    Assert.Equal(refCountOnes, twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne));
                    Assert.Equal(refCountTwos, ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo));

                    Assert.All(
                        context.ChangeTracker.Entries<JoinOneToTwo>(), e => Assert.Equal(
                            e.Entity.OneId == 1
                            || e.Entity.TwoId == 1
                                ? EntityState.Deleted
                                : EntityState.Unchanged, e.State));

                    context.SaveChanges();

                    Assert.Equal(1, twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne));
                    Assert.Equal(1, ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo));

                    ones.Remove(toRemoveOne);
                    twos.Remove(toRemoveTwo);

                    Assert.Equal(0, twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne));
                    Assert.Equal(0, ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo));

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

            static void ValidateNavigations(List<EntityOne> ones, List<EntityTwo> twos)
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

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_fully_by_convention(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = 7711),
                        context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = 7712),
                        context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = 7713)
                    };
                    var rightEntities = new[]
                    {
                        context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = 7721),
                        context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = 7722),
                        context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].Bs.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].Bs.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].Bs.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].As.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].As.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].As.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<ImplicitManyToManyA>().Where(e => e.Id > 7700).Include(e => e.Bs);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    Assert.Equal(11, context.ChangeTracker.Entries().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyA>().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyB>().Count());
                    Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                    var leftEntities = context.ChangeTracker.Entries<ImplicitManyToManyA>().Select(e => e.Entity).OrderBy(e => e.Id)
                        .ToList();
                    var rightEntities = context.ChangeTracker.Entries<ImplicitManyToManyB>().Select(e => e.Entity).OrderBy(e => e.Id)
                        .ToList();

                    Assert.Equal(3, leftEntities[0].Bs.Count);
                    Assert.Single(leftEntities[1].Bs);
                    Assert.Single(leftEntities[2].Bs);

                    Assert.Equal(3, rightEntities[0].As.Count);
                    Assert.Single(rightEntities[1].As);
                    Assert.Single(rightEntities[2].As);
                });

            static void ValidateFixup(DbContext context, IList<ImplicitManyToManyA> leftEntities, IList<ImplicitManyToManyB> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyA>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyB>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                Assert.Equal(3, leftEntities[0].Bs.Count);
                Assert.Single(leftEntities[1].Bs);
                Assert.Single(leftEntities[2].Bs);

                Assert.Equal(3, rightEntities[0].As.Count);
                Assert.Single(rightEntities[1].As);
                Assert.Single(rightEntities[2].As);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Can_insert_many_to_many_fully_by_convention_generated_keys(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.Set<GeneratedKeysLeft>().CreateInstance(),
                        context.Set<GeneratedKeysLeft>().CreateInstance(),
                        context.Set<GeneratedKeysLeft>().CreateInstance()
                    };
                    var rightEntities = new[]
                    {
                        context.Set<GeneratedKeysRight>().CreateInstance(),
                        context.Set<GeneratedKeysRight>().CreateInstance(),
                        context.Set<GeneratedKeysRight>().CreateInstance()
                    };

                    leftEntities[0].Rights.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].Rights.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].Rights.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].Lefts.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].Lefts.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].Lefts.Add(leftEntities[2]); // 21 - 13

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                        await context.AddRangeAsync(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                        context.AddRange(rightEntities[0], rightEntities[1], rightEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<GeneratedKeysLeft>().Include(e => e.Rights);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    Assert.Equal(11, context.ChangeTracker.Entries().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<GeneratedKeysLeft>().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<GeneratedKeysRight>().Count());
                    Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                    var leftEntities = context.ChangeTracker.Entries<GeneratedKeysLeft>().Select(e => e.Entity).OrderBy(e => e.Id)
                        .ToList();
                    var rightEntities = context.ChangeTracker.Entries<GeneratedKeysRight>().Select(e => e.Entity).OrderBy(e => e.Id)
                        .ToList();

                    Assert.Equal(3, leftEntities[0].Rights.Count);
                    Assert.Single(leftEntities[1].Rights);
                    Assert.Single(leftEntities[2].Rights);

                    Assert.Equal(3, rightEntities[0].Lefts.Count);
                    Assert.Single(rightEntities[1].Lefts);
                    Assert.Single(rightEntities[2].Lefts);
                });

            static void ValidateFixup(DbContext context, IList<GeneratedKeysLeft> leftEntities, IList<GeneratedKeysRight> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<GeneratedKeysLeft>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<GeneratedKeysRight>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                Assert.Equal(3, leftEntities[0].Rights.Count);
                Assert.Single(leftEntities[1].Rights);
                Assert.Single(leftEntities[2].Rights);

                Assert.Equal(3, rightEntities[0].Lefts.Count);
                Assert.Single(rightEntities[1].Lefts);
                Assert.Single(rightEntities[2].Lefts);
            }
        }

        [ConditionalTheory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        [InlineData(false, true)]
        public virtual async Task Can_Attach_or_Update_a_many_to_many_with_mixed_set_and_unset_keys(bool useUpdate, bool async)
        {
            var existingLeftId = -1;
            var existingRightId = -1;
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var left = context.Set<GeneratedKeysLeft>().CreateInstance();
                    var right = context.Set<GeneratedKeysRight>().CreateInstance();

                    if (!useUpdate)
                    {
                        left.Rights.Add(right);
                    }

                    if (async)
                    {
                        await context.AddRangeAsync(left, right);
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.AddRange(left, right);
                        context.SaveChanges();
                    }

                    existingLeftId = left.Id;
                    existingRightId = right.Id;
                },
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.Set<GeneratedKeysLeft>().CreateInstance((e, p) => e.Id = existingLeftId),
                        context.Set<GeneratedKeysLeft>().CreateInstance(),
                        context.Set<GeneratedKeysLeft>().CreateInstance()
                    };
                    var rightEntities = new[]
                    {
                        context.Set<GeneratedKeysRight>().CreateInstance((e, p) => e.Id = existingRightId),
                        context.Set<GeneratedKeysRight>().CreateInstance(),
                        context.Set<GeneratedKeysRight>().CreateInstance()
                    };

                    leftEntities[0].Rights.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].Rights.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].Rights.Add(rightEntities[2]); // 11 - 23

                    rightEntities[0].Lefts.Add(leftEntities[0]); // 21 - 11 (Dupe)
                    rightEntities[0].Lefts.Add(leftEntities[1]); // 21 - 12
                    rightEntities[0].Lefts.Add(leftEntities[2]); // 21 - 13

                    if (useUpdate)
                    {
                        context.Update(leftEntities[0]);
                    }
                    else
                    {
                        context.Attach(leftEntities[0]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    var entityEntries = context.ChangeTracker.Entries<Dictionary<string, object>>().ToList();
                    foreach (var joinEntry in entityEntries)
                    {
                        Assert.Equal(
                            !useUpdate
                            && joinEntry.Property<int>("RightsId").CurrentValue == existingRightId
                            && joinEntry.Property<int>("LeftsId").CurrentValue == existingLeftId
                                ? EntityState.Unchanged
                                : EntityState.Added, joinEntry.State);
                    }

                    foreach (var leftEntry in context.ChangeTracker.Entries<GeneratedKeysLeft>())
                    {
                        Assert.Equal(
                            leftEntry.Entity.Id == existingLeftId
                                ? useUpdate
                                    ? EntityState.Modified
                                    : EntityState.Unchanged
                                : EntityState.Added, leftEntry.State);
                    }

                    foreach (var rightEntry in context.ChangeTracker.Entries<GeneratedKeysRight>())
                    {
                        Assert.Equal(
                            rightEntry.Entity.Id == existingRightId
                                ? useUpdate
                                    ? EntityState.Modified
                                    : EntityState.Unchanged
                                : EntityState.Added, rightEntry.State);
                    }

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<GeneratedKeysLeft>().Include(e => e.Rights);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    Assert.Equal(11, context.ChangeTracker.Entries().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<GeneratedKeysLeft>().Count());
                    Assert.Equal(3, context.ChangeTracker.Entries<GeneratedKeysRight>().Count());
                    Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                    var leftEntities = context.ChangeTracker.Entries<GeneratedKeysLeft>().Select(e => e.Entity).OrderBy(e => e.Id)
                        .ToList();
                    var rightEntities = context.ChangeTracker.Entries<GeneratedKeysRight>().Select(e => e.Entity).OrderBy(e => e.Id)
                        .ToList();

                    Assert.Equal(3, leftEntities[0].Rights.Count);
                    Assert.Single(leftEntities[1].Rights);
                    Assert.Single(leftEntities[2].Rights);

                    Assert.Equal(3, rightEntities[0].Lefts.Count);
                    Assert.Single(rightEntities[1].Lefts);
                    Assert.Single(rightEntities[2].Lefts);
                });

            static void ValidateFixup(DbContext context, IList<GeneratedKeysLeft> leftEntities, IList<GeneratedKeysRight> rightEntities)
            {
                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<GeneratedKeysLeft>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<GeneratedKeysRight>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                Assert.Equal(3, leftEntities[0].Rights.Count);
                Assert.Single(leftEntities[1].Rights);
                Assert.Single(leftEntities[2].Rights);

                Assert.Equal(3, rightEntities[0].Lefts.Count);
                Assert.Single(rightEntities[1].Lefts);
                Assert.Single(rightEntities[2].Lefts);
            }
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Initial_tracking_uses_skip_navigations(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var leftEntities = new[]
                    {
                        context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = 7711),
                        context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = 7712),
                        context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = 7713)
                    };
                    var rightEntities = new[]
                    {
                        context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = 7721),
                        context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = 7722),
                        context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = 7723)
                    };

                    leftEntities[0].Bs.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].Bs.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].Bs.Add(rightEntities[2]); // 11 - 23

                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    }

                    ValidateFixup(context, leftEntities, rightEntities);

                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }

                    ValidateFixup(context, leftEntities, rightEntities);
                },
                async context =>
                {
                    var queryable = context.Set<ImplicitManyToManyA>().Where(e => e.Id > 7700).Include(e => e.Bs);
                    var results = async ? await queryable.ToListAsync() : queryable.ToList();
                    Assert.Equal(3, results.Count);

                    var leftEntities = context.ChangeTracker.Entries<ImplicitManyToManyA>().Select(e => e.Entity).OrderBy(e => e.Id)
                        .ToList();
                    var rightEntities = context.ChangeTracker.Entries<ImplicitManyToManyB>().Select(e => e.Entity).OrderBy(e => e.Id)
                        .ToList();

                    ValidateFixup(context, leftEntities, rightEntities);
                });

            static void ValidateFixup(DbContext context, IList<ImplicitManyToManyA> leftEntities, IList<ImplicitManyToManyB> rightEntities)
            {
                Assert.Equal(9, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyA>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyB>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                Assert.Equal(3, leftEntities[0].Bs.Count);
                Assert.Empty(leftEntities[1].Bs);
                Assert.Empty(leftEntities[2].Bs);

                Assert.Single(rightEntities[0].As);
                Assert.Single(rightEntities[1].As);
                Assert.Single(rightEntities[2].As);
            }
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
                    // ReSharper disable once RedundantCast
                    1 => (IQueryable<object>)context.Set<EntityOne>(),
                    2 => context.Set<EntityTwo>(),
                    3 => context.Set<JoinOneToTwo>(),
                    _ => throw new ArgumentException()
                }).Load();
            }

            Assert.Equal(152, context.ChangeTracker.Entries().Count());
            Assert.Equal(20, context.ChangeTracker.Entries<EntityOne>().Count());
            Assert.Equal(20, context.ChangeTracker.Entries<EntityTwo>().Count());
            Assert.Equal(112, context.ChangeTracker.Entries<JoinOneToTwo>().Count());

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
            Assert.Equal(112, (joinCount / 2) + deleted);
        }

        [ConditionalFact]
        public virtual void Can_insert_update_delete_shared_type_entity_type()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared").CreateInstance(
                        (e, p) =>
                        {
                            e["OneId"] = 1;
                            e["ThreeId"] = 1;
                            e["Payload"] = "NewlyAdded";
                        });
                    context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared").Add(entity);

                    context.SaveChanges();
                },
                context =>
                {
                    var entity = context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                        .Single(e => (int)e["OneId"] == 1 && (int)e["ThreeId"] == 1);

                    Assert.Equal("NewlyAdded", (string)entity["Payload"]);

                    entity["Payload"] = "AlreadyUpdated";

                    context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared").Update(entity);

                    context.SaveChanges();
                },
                context =>
                {
                    var entity = context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                        .Single(e => (int)e["OneId"] == 1 && (int)e["ThreeId"] == 1);

                    Assert.Equal("AlreadyUpdated", (string)entity["Payload"]);

                    context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared").Remove(entity);

                    context.SaveChanges();

                    Assert.False(
                        context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                            .Any(e => (int)e["OneId"] == 1 && (int)e["ThreeId"] == 1));
                });
        }

        [ConditionalFact]
        public virtual void Can_insert_update_delete_proxyable_shared_type_entity_type()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var entity = context.Set<ProxyableSharedType>("PST").CreateInstance(
                        (e, p) =>
                        {
                            e["Id"] = 1;
                            e["Payload"] = "NewlyAdded";
                        });

                    context.Set<ProxyableSharedType>("PST").Add(entity);

                    context.SaveChanges();
                },
                context =>
                {
                    var entity = context.Set<ProxyableSharedType>("PST").Single(e => (int)e["Id"] == 1);

                    Assert.Equal("NewlyAdded", (string)entity["Payload"]);

                    entity["Payload"] = "AlreadyUpdated";

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }

                    context.SaveChanges();
                },
                context =>
                {
                    var entity = context.Set<ProxyableSharedType>("PST").Single(e => (int)e["Id"] == 1);

                    Assert.Equal("AlreadyUpdated", (string)entity["Payload"]);

                    context.Set<ProxyableSharedType>("PST").Remove(entity);

                    context.SaveChanges();

                    Assert.False(context.Set<ProxyableSharedType>("PST").Any(e => (int)e["Id"] == 1));
                });
        }

        private ICollection<TEntity> CreateCollection<TEntity>()
            => RequiresDetectChanges ? (ICollection<TEntity>)new List<TEntity>() : new ObservableCollection<TEntity>();

        protected ManyToManyTrackingTestBase(TFixture fixture)
            => Fixture = fixture;

        protected TFixture Fixture { get; }

        protected virtual void ExecuteWithStrategyInTransaction(
            Action<ManyToManyContext> testOperation,
            Action<ManyToManyContext> nestedTestOperation1 = null,
            Action<ManyToManyContext> nestedTestOperation2 = null,
            Action<ManyToManyContext> nestedTestOperation3 = null)
            => TestHelpers.ExecuteWithStrategyInTransaction(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

        protected virtual Task ExecuteWithStrategyInTransactionAsync(
            Func<ManyToManyContext, Task> testOperation,
            Func<ManyToManyContext, Task> nestedTestOperation1 = null,
            Func<ManyToManyContext, Task> nestedTestOperation2 = null,
            Func<ManyToManyContext, Task> nestedTestOperation3 = null)
            => TestHelpers.ExecuteWithStrategyInTransactionAsync(
                CreateContext, UseTransaction,
                testOperation, nestedTestOperation1, nestedTestOperation2, nestedTestOperation3);

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        protected ManyToManyContext CreateContext()
            => Fixture.CreateContext();

        protected virtual bool SupportsDatabaseDefaults
            => true;

        protected virtual bool RequiresDetectChanges
            => true;

        public abstract class ManyToManyTrackingFixtureBase : ManyToManyQueryFixtureBase
        {
            public override ManyToManyContext CreateContext()
            {
                var context = base.CreateContext();
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                return context;
            }

            protected override string StoreName { get; } = "ManyToManyTracking";
        }
    }
}

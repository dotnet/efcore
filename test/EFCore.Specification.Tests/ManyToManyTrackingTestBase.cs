// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class ManyToManyTrackingTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : ManyToManyTrackingTestBase<TFixture>.ManyToManyTrackingFixtureBase
{
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_insert_many_to_many_composite_with_navs(bool async)
    {
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Key2 = "7711";
                            e.Key3 = new DateTime(7711, 1, 1);
                        }),
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Key2 = "7712";
                            e.Key3 = new DateTime(7712, 1, 1);
                        }),
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Key2 = "7713";
                            e.Key3 = new DateTime(7713, 1, 1);
                        }),
                };
                var rightEntities = new[]
                {
                    context.Set<EntityLeaf>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.Set<EntityLeaf>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.Set<EntityLeaf>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                keys = leftEntities.Select(e => e.Key1).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityCompositeKey>().Where(e => keys.Contains(e.Key1)).Include(e => e.LeafSkipFull);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityCompositeKey>()
                    .Select(e => e.Entity).OrderBy(e => e.Key2).ToList();

                var rightEntities = context.ChangeTracker.Entries<EntityLeaf>()
                    .Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityCompositeKey> leftEntities, IList<EntityLeaf> rightEntities)
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

            var joinEntities = context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>().Select(e => e.Entity).ToList();
            foreach (var joinEntity in joinEntities)
            {
                Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                Assert.Equal(joinEntity.Leaf.Id, joinEntity.LeafId);

                Assert.Contains(joinEntity, joinEntity.Composite.JoinLeafFull);
                Assert.Contains(joinEntity, joinEntity.Leaf.JoinCompositeKeyFull);
            }

            VerifyRelationshipSnapshots(context, joinEntities);
            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_composite_with_navs()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityCompositeKey>().Include(e => e.LeafSkipFull).OrderBy(e => e.Key2).ToListAsync();
                var rightEntities = await context.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name).ToListAsync();

                leftEntities[0].LeafSkipFull.Add(
                    context.Set<EntityLeaf>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }));
                leftEntities[0].LeafSkipFull.Add(
                    context.Set<EntityLeaf>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }));
                leftEntities[0].LeafSkipFull.Add(
                    context.Set<EntityLeaf>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }));

                rightEntities[0].CompositeKeySkipFull.Add(
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Key2 = "7711";
                            e.Key3 = new DateTime(7711, 1, 1);
                            e.Name = "Z7711";
                        }));
                rightEntities[0].CompositeKeySkipFull.Add(
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Key2 = "7712";
                            e.Key3 = new DateTime(7712, 1, 1);
                            e.Name = "Z7712";
                        }));
                rightEntities[0].CompositeKeySkipFull.Add(
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Key2 = "7713";
                            e.Key3 = new DateTime(7713, 1, 1);
                            e.Name = "Z7713";
                        }));

                leftEntities[0].LeafSkipFull.Remove(leftEntities[0].LeafSkipFull.Single(e => e.Name == "Leaf 1"));
                rightEntities[1].CompositeKeySkipFull.Remove(rightEntities[1].CompositeKeySkipFull.Single(e => e.Key2 == "3_1"));

                leftEntities[2].LeafSkipFull.Remove(leftEntities[2].LeafSkipFull.Single(e => e.Name == "Leaf 3"));
                leftEntities[2].LeafSkipFull.Add(
                    context.Set<EntityLeaf>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }));

                rightEntities[2].CompositeKeySkipFull.Remove(rightEntities[2].CompositeKeySkipFull.Single(e => e.Key2 == "8_3"));
                rightEntities[2].CompositeKeySkipFull.Add(
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Key2 = "7714";
                            e.Key3 = new DateTime(7714, 1, 1);
                            e.Name = "Z7714";
                        }));

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                ValidateFixup(context, leftEntities, rightEntities, 24, 8, 39);

                await context.SaveChangesAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 8, 39 - 4);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityCompositeKey>().Include(e => e.LeafSkipFull).OrderBy(e => e.Key2).ToListAsync();
                var rightEntities = await context.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 8, 39 - 4);
            });

        void ValidateFixup(
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

            Assert.Contains(leftEntities[0].LeafSkipFull, e => e.Name == "Z7721");
            Assert.Contains(leftEntities[0].LeafSkipFull, e => e.Name == "Z7722");
            Assert.Contains(leftEntities[0].LeafSkipFull, e => e.Name == "Z7723");

            Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Name == "Z7711");
            Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Name == "Z7712");
            Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Name == "Z7713");

            Assert.DoesNotContain(leftEntities[0].LeafSkipFull, e => e.Name == "Leaf 1");
            Assert.DoesNotContain(rightEntities[1].CompositeKeySkipFull, e => e.Key2 == "3_1");

            Assert.DoesNotContain(leftEntities[2].LeafSkipFull, e => e.Name == "Leaf 3");
            Assert.Contains(leftEntities[2].LeafSkipFull, e => e.Name == "Z7724");

            Assert.DoesNotContain(rightEntities[2].CompositeKeySkipFull, e => e.Key2 == "8_1");
            Assert.Contains(rightEntities[2].CompositeKeySkipFull, e => e.Key2 == "7714");

            var joinEntries = context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>().ToList();
            foreach (var joinEntry in joinEntries)
            {
                var joinEntity = joinEntry.Entity;

                Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                Assert.Equal(joinEntity.Leaf.Id, joinEntity.LeafId);

                Assert.Contains(joinEntity, joinEntity.Composite.JoinLeafFull);
                Assert.Contains(joinEntity, joinEntity.Leaf.JoinCompositeKeyFull);
            }

            var allLeft = context.ChangeTracker.Entries<EntityCompositeKey>().Select(e => e.Entity).OrderBy(e => e.Key2).ToList();
            var allRight = context.ChangeTracker.Entries<EntityLeaf>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, joinEntries.Select(e => e.Entity));
            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
    public virtual Task Can_delete_with_many_to_many_composite_with_navs()
    {
        var key1 = 0;
        var key2 = "";
        var key3 = default(DateTime);
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var ones = await context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2).ToListAsync();
                var threes = await context.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name).ToListAsync();

                // Make sure other related entities are loaded for delete fixup
                await context.Set<JoinThreeToCompositeKeyFull>().LoadAsync();

                var toRemoveOne = context.EntityCompositeKeys.Single(e => e.Name == "Composite 6");
                key1 = toRemoveOne.Key1;
                key2 = toRemoveOne.Key2;
                key3 = toRemoveOne.Key3;
                var refCountOnes = threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne);

                var toRemoveThree = (EntityLeaf)context.EntityRoots.Single(e => e.Name == "Leaf 3");
                id = toRemoveThree.Id;
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
                        (e.Entity.CompositeId1 == key1
                            && e.Entity.CompositeId2 == key2
                            && e.Entity.CompositeId3 == key3)
                        || e.Entity.LeafId == id
                            ? EntityState.Deleted
                            : EntityState.Unchanged, e.State));

                await context.SaveChangesAsync();

                Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                Assert.Equal(0, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                ValidateJoinNavigations(context);

                ones.Remove(toRemoveOne);
                threes.Remove(toRemoveThree);

                Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                Assert.Equal(0, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>(),
                    e => (e.Entity.CompositeId1 == key1
                            && e.Entity.CompositeId2 == key2
                            && e.Entity.CompositeId3 == key3)
                        || e.Entity.LeafId == id);
            }, async context =>
            {
                var ones = await context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2).ToListAsync();
                var threes = await context.Set<EntityLeaf>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name).ToListAsync();

                ValidateNavigations(ones, threes);

                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>(),
                    e => (e.Entity.CompositeId1 == key1
                            && e.Entity.CompositeId2 == key2
                            && e.Entity.CompositeId3 == key3)
                        || e.Entity.LeafId == id);
            });

        void ValidateNavigations(List<EntityCompositeKey> ones, List<EntityLeaf> threes)
        {
            foreach (var one in ones)
            {
                if (one.RootSkipShared != null)
                {
                    Assert.DoesNotContain(one.RootSkipShared, e => e.Id == id);
                }

                if (one.JoinLeafFull != null)
                {
                    Assert.DoesNotContain(
                        one.JoinLeafFull,
                        e => e.CompositeId1 == key1
                            && e.CompositeId2 == key2
                            && e.CompositeId3 == key3);

                    Assert.DoesNotContain(one.JoinLeafFull, e => e.LeafId == id);
                }
            }

            foreach (var three in threes)
            {
                if (three.CompositeKeySkipFull != null)
                {
                    Assert.DoesNotContain(
                        three.CompositeKeySkipFull,
                        e => e.Key1 == key1
                            && e.Key2 == key2
                            && e.Key3 == key3);
                }

                if (three.JoinCompositeKeyFull != null)
                {
                    Assert.DoesNotContain(
                        three.JoinCompositeKeyFull,
                        e => e.CompositeId1 == key1
                            && e.CompositeId2 == key2
                            && e.CompositeId3 == key3);

                    Assert.DoesNotContain(three.JoinCompositeKeyFull, e => e.LeafId == id);
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
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Key2 = "7711";
                            e.Key3 = new DateTime(7711, 1, 1);
                        }),
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Key2 = "7712";
                            e.Key3 = new DateTime(7712, 1, 1);
                        }),
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Key2 = "7713";
                            e.Key3 = new DateTime(7713, 1, 1);
                        }),
                };
                var rightEntities = new[]
                {
                    context.Set<EntityRoot>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.Set<EntityRoot>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.Set<EntityRoot>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                keys = leftEntities.Select(e => e.Key1).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityCompositeKey>().Where(e => keys.Contains(e.Key1)).Include(e => e.RootSkipShared);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityCompositeKey>()
                    .Select(e => e.Entity).OrderBy(e => e.Key2).ToList();

                var rightEntities = context.ChangeTracker.Entries<EntityRoot>()
                    .Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityCompositeKey> leftEntities, IList<EntityRoot> rightEntities)
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

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_composite_shared_with_navs()
    {
        List<int> rootKeys = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2).ToListAsync();
                var rightEntities = await context.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared).OrderBy(e => e.Name).ToListAsync();

                var roots = new[]
                {
                    context.Set<EntityRoot>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.Set<EntityRoot>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.Set<EntityRoot>().CreateInstance(
                        (e, p) =>
                        {
                            Assert.True(e != null, nameof(e) + " != null");
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }),
                    context.Set<EntityRoot>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        })
                };

                leftEntities[0].RootSkipShared.Add(roots[0]);
                leftEntities[0].RootSkipShared.Add(roots[1]);
                leftEntities[0].RootSkipShared.Add(roots[2]);

                rightEntities[0].CompositeKeySkipShared.Add(
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Key2 = "Z7711";
                            e.Key3 = new DateTime(7711, 1, 1);
                        }));
                rightEntities[0].CompositeKeySkipShared.Add(
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Key2 = "Z7712";
                            e.Key3 = new DateTime(7712, 1, 1);
                        }));
                rightEntities[0].CompositeKeySkipShared.Add(
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Key2 = "Z7713";
                            e.Key3 = new DateTime(7713, 1, 1);
                        }));

                leftEntities[0].RootSkipShared.Remove(leftEntities[0].RootSkipShared.Single(e => e.Name == "Root 9"));
                rightEntities[1].CompositeKeySkipShared.Remove(rightEntities[1].CompositeKeySkipShared.Single(e => e.Key2 == "8_2"));

                leftEntities[2].RootSkipShared.Remove(leftEntities[2].RootSkipShared.Single(e => e.Name == "Branch 6"));
                leftEntities[2].RootSkipShared.Add(roots[3]);

                rightEntities[3].CompositeKeySkipShared.Remove(rightEntities[3].CompositeKeySkipShared.Single(e => e.Key2 == "8_5"));
                rightEntities[3].CompositeKeySkipShared.Add(
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Key2 = "Z7714";
                            e.Key3 = new DateTime(7714, 1, 1);
                        }));

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                rootKeys = roots.Select(e => context.Entry(e).Property(e => e.Id).CurrentValue).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 47);

                await context.SaveChangesAsync();

                rootKeys = roots.Select(e => e.Id).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 47 - 4);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2).ToListAsync();
                var rightEntities = await context.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 47 - 4);
            });

        void ValidateFixup(
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

            Assert.Contains(leftEntities[0].RootSkipShared, e => context.Entry(e).Property(e => e.Id).CurrentValue == rootKeys[0]);
            Assert.Contains(leftEntities[0].RootSkipShared, e => context.Entry(e).Property(e => e.Id).CurrentValue == rootKeys[1]);
            Assert.Contains(leftEntities[0].RootSkipShared, e => context.Entry(e).Property(e => e.Id).CurrentValue == rootKeys[2]);

            Assert.Contains(rightEntities[0].CompositeKeySkipShared, e => e.Key2 == "Z7711");
            Assert.Contains(rightEntities[0].CompositeKeySkipShared, e => e.Key2 == "Z7712");
            Assert.Contains(rightEntities[0].CompositeKeySkipShared, e => e.Key2 == "Z7713");

            Assert.DoesNotContain(leftEntities[0].RootSkipShared, e => e.Name == "Root 9");
            Assert.DoesNotContain(rightEntities[1].CompositeKeySkipShared, e => e.Key2 == "8_2");

            Assert.DoesNotContain(leftEntities[2].RootSkipShared, e => e.Name == "Branch 6");
            Assert.Contains(leftEntities[2].RootSkipShared, e => context.Entry(e).Property(e => e.Id).CurrentValue == rootKeys[3]);

            Assert.DoesNotContain(rightEntities[3].CompositeKeySkipShared, e => e.Key2 == "8_5");
            Assert.Contains(rightEntities[3].CompositeKeySkipShared, e => e.Key2 == "Z7714");

            var allLeft = context.ChangeTracker.Entries<EntityCompositeKey>().Select(e => e.Entity).OrderBy(e => e.Key2).ToList();
            var allRight = context.ChangeTracker.Entries<EntityRoot>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
    public virtual Task Can_delete_with_many_to_many_composite_shared_with_navs()
    {
        var key1 = 0;
        var key2 = "";
        var key3 = default(DateTime);
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var ones = await context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2).ToListAsync();
                var threes = await context.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared).OrderBy(e => e.Name).ToListAsync();

                // Make sure other related entities are loaded for delete fixup
                context.Set<JoinThreeToCompositeKeyFull>().Load();

                var toRemoveOne = context.EntityCompositeKeys.Single(e => e.Name == "Composite 6");
                key1 = toRemoveOne.Key1;
                key2 = toRemoveOne.Key2;
                key3 = toRemoveOne.Key3;
                var refCountOnes = threes.SelectMany(e => e.CompositeKeySkipShared).Count(e => e == toRemoveOne);

                var toRemoveThree = context.EntityRoots.Single(e => e.Name == "Leaf 3");
                id = toRemoveThree.Id;
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
                        ((int)e.Entity["CompositeKeySkipSharedKey1"] == key1
                            && (string)e.Entity["CompositeKeySkipSharedKey2"] == key2
                            && (DateTime)e.Entity["CompositeKeySkipSharedKey3"] == key3)
                        || (int)e.Entity["RootSkipSharedId"] == id
                            ? EntityState.Deleted
                            : EntityState.Unchanged, e.State));

                await context.SaveChangesAsync();

                Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipShared).Count(e => e == toRemoveOne));
                Assert.Equal(0, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                ones.Remove(toRemoveOne);
                threes.Remove(toRemoveThree);

                Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipShared).Count(e => e == toRemoveOne));
                Assert.Equal(0, ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree));

                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<Dictionary<string, object>>(),
                    e => ((int)e.Entity["CompositeKeySkipSharedKey1"] == key1
                            && (string)e.Entity["CompositeKeySkipSharedKey2"] == key2
                            && (DateTime)e.Entity["CompositeKeySkipSharedKey3"] == key3)
                        || (int)e.Entity["RootSkipSharedId"] == id);
            }, async context =>
            {
                var ones = await context.Set<EntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2).ToListAsync();
                var threes = await context.Set<EntityRoot>().Include(e => e.CompositeKeySkipShared).OrderBy(e => e.Name).ToListAsync();

                ValidateNavigations(ones, threes);

                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<Dictionary<string, object>>(),
                    e => ((int)e.Entity["CompositeKeySkipSharedKey1"] == key1
                            && (string)e.Entity["CompositeKeySkipSharedKey2"] == key2
                            && (DateTime)e.Entity["CompositeKeySkipSharedKey3"] == key3)
                        || (int)e.Entity["RootSkipSharedId"] == id);
            });

        void ValidateNavigations(
            List<EntityCompositeKey> ones,
            List<EntityRoot> threes)
        {
            foreach (var one in ones)
            {
                if (one.RootSkipShared != null)
                {
                    Assert.DoesNotContain(one.RootSkipShared, e => e.Id == id);
                }
            }

            foreach (var three in threes)
            {
                if (three.CompositeKeySkipShared != null)
                {
                    Assert.DoesNotContain(
                        three.CompositeKeySkipShared,
                        e => e.Key1 == key1
                            && e.Key2 == key2
                            && e.Key3 == key3);
                }
            }
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_insert_many_to_many_composite_additional_pk_with_navs(bool async)
    {
        List<string> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Key2 = "7711";
                            e.Key3 = new DateTime(7711, 1, 1);
                        }),
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Key2 = "7712";
                            e.Key3 = new DateTime(7712, 1, 1);
                        }),
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Key2 = "7713";
                            e.Key3 = new DateTime(7713, 1, 1);
                        }),
                };
                var rightEntities = new[]
                {
                    context.Set<EntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.Set<EntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.Set<EntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        })
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

                keys = leftEntities.Select(e => e.Key2).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityCompositeKey>().Where(e => keys.Contains(e.Key2)).Include(e => e.ThreeSkipFull);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityCompositeKey>()
                    .Select(e => e.Entity).OrderBy(e => e.Key2).ToList();

                var rightEntities = context.ChangeTracker.Entries<EntityThree>()
                    .Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities, postSave: true);
            });

        void ValidateFixup(DbContext context, IList<EntityCompositeKey> leftEntities, IList<EntityThree> rightEntities, bool postSave)
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

            var joinEntities = context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>().Select(e => e.Entity).ToList();
            foreach (var joinEntity in joinEntities)
            {
                Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);

                Assert.Contains(joinEntity, joinEntity.Composite.JoinThreeFull);
                Assert.Contains(joinEntity, joinEntity.Three.JoinCompositeKeyFull);
            }

            VerifyRelationshipSnapshots(context, joinEntities);
            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);

            foreach (var entry in context.ChangeTracker.Entries())
            {
                Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Added, entry.State);
            }
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_composite_additional_pk_with_navs()
    {
        List<int> threeIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key2).ToListAsync();
                var rightEntities = await context.Set<EntityThree>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name).ToListAsync();

                var threes = new[]
                {
                    context.Set<EntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.Set<EntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.Set<EntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }),
                    context.Set<EntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        })
                };

                var composites = new[]
                {
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Key2 = "Z7711";
                            e.Key3 = new DateTime(7711, 1, 1);
                        }),
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Key2 = "Z7712";
                            e.Key3 = new DateTime(7712, 1, 1);
                        }),
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Key2 = "Z7713";
                            e.Key3 = new DateTime(7713, 1, 1);
                        }),
                    context.EntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Key2 = "Z7714";
                            e.Key3 = new DateTime(7714, 1, 1);
                        })
                };

                leftEntities[0].ThreeSkipFull.Add(threes[0]);
                leftEntities[0].ThreeSkipFull.Add(threes[1]);
                leftEntities[0].ThreeSkipFull.Add(threes[2]);

                rightEntities[0].CompositeKeySkipFull.Add(composites[0]);
                rightEntities[0].CompositeKeySkipFull.Add(composites[1]);
                rightEntities[0].CompositeKeySkipFull.Add(composites[2]);

                leftEntities[0].ThreeSkipFull.Remove(leftEntities[0].ThreeSkipFull.Single(e => e.Name == "EntityThree 2"));
                rightEntities[1].CompositeKeySkipFull
                    .Remove(rightEntities[1].CompositeKeySkipFull.Single(e => e.Name == "Composite 16"));

                leftEntities[3].ThreeSkipFull.Remove(leftEntities[3].ThreeSkipFull.Single(e => e.Name == "EntityThree 7"));
                leftEntities[3].ThreeSkipFull.Add(threes[3]);

                rightEntities[2].CompositeKeySkipFull
                    .Remove(rightEntities[2].CompositeKeySkipFull.Single(e => e.Name == "Composite 7"));
                rightEntities[2].CompositeKeySkipFull.Add(composites[3]);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                threeIds = threes.Select(e => context.Entry(e).Property(e => e.Id).CurrentValue).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53);

                await context.SaveChangesAsync();

                threeIds = threes.Select(e => e.Id).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53 - 4);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key2).ToListAsync();
                var rightEntities = await context.Set<EntityThree>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53 - 4);
            });

        void ValidateFixup(
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

            Assert.Contains(leftEntities[0].ThreeSkipFull, e => context.Entry(e).Property(e => e.Id).CurrentValue == threeIds[0]);
            Assert.Contains(leftEntities[0].ThreeSkipFull, e => context.Entry(e).Property(e => e.Id).CurrentValue == threeIds[1]);
            Assert.Contains(leftEntities[0].ThreeSkipFull, e => context.Entry(e).Property(e => e.Id).CurrentValue == threeIds[2]);

            Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Key2 == "Z7711");
            Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Key2 == "Z7712");
            Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Key2 == "Z7713");

            Assert.DoesNotContain(leftEntities[0].ThreeSkipFull, e => e.Name == "EntityThree 9");
            Assert.DoesNotContain(rightEntities[1].CompositeKeySkipFull, e => e.Key2 == "9_2");

            Assert.DoesNotContain(leftEntities[3].ThreeSkipFull, e => e.Name == "EntityThree 23");
            Assert.Contains(leftEntities[3].ThreeSkipFull, e => context.Entry(e).Property(e => e.Id).CurrentValue == threeIds[3]);

            Assert.DoesNotContain(rightEntities[2].CompositeKeySkipFull, e => e.Key2 == "6_1");
            Assert.Contains(rightEntities[2].CompositeKeySkipFull, e => e.Key2 == "Z7714");

            var joinEntries = context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>().ToList();
            foreach (var joinEntry in joinEntries)
            {
                var joinEntity = joinEntry.Entity;

                Assert.Equal(joinEntity.Composite.Key1, joinEntity.CompositeId1);
                Assert.Equal(joinEntity.Composite.Key2, joinEntity.CompositeId2);
                Assert.Equal(joinEntity.Composite.Key3, joinEntity.CompositeId3);
                Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);

                Assert.Contains(joinEntity, joinEntity.Composite.JoinThreeFull);
                Assert.Contains(joinEntity, joinEntity.Three.JoinCompositeKeyFull);
            }

            var allLeft = context.ChangeTracker.Entries<EntityCompositeKey>().Select(e => e.Entity).OrderBy(e => e.Key2).ToList();
            var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, joinEntries.Select(e => e.Entity));
            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
    public virtual Task Can_delete_with_many_to_many_composite_additional_pk_with_navs()
    {
        var threeId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var ones = await context.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key2).ToListAsync();
                var threes = await context.Set<EntityThree>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name).ToListAsync();

                // Make sure other related entities are loaded for delete fixup
                await context.Set<JoinThreeToCompositeKeyFull>().LoadAsync();

                var toRemoveOne = context.EntityCompositeKeys.Single(e => e.Name == "Composite 6");
                var refCountOnes = threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne);

                var toRemoveThree = context.EntityThrees.Single(e => e.Name == "EntityThree 17");
                threeId = toRemoveThree.Id;
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
                        (e.Entity.CompositeId2 == "6_1"
                            && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                        || e.Entity.ThreeId == threeId
                            ? EntityState.Deleted
                            : EntityState.Unchanged, e.State));

                await context.SaveChangesAsync();

                Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                Assert.Equal(0, ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree));

                ValidateJoinNavigations(context);

                ones.Remove(toRemoveOne);
                threes.Remove(toRemoveThree);

                Assert.Equal(0, threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne));
                Assert.Equal(0, ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree));

                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>(),
                    e => (e.Entity.CompositeId2 == "6_1"
                            && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                        || e.Entity.ThreeId == threeId);
            }, async context =>
            {
                var ones = await context.Set<EntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key2).ToListAsync();
                var threes = await context.Set<EntityThree>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name).ToListAsync();

                ValidateNavigations(ones, threes);

                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>(),
                    e => (e.Entity.CompositeId2 == "6_1"
                            && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                        || e.Entity.ThreeId == threeId);
            });

        void ValidateNavigations(List<EntityCompositeKey> ones, List<EntityThree> threes)
        {
            foreach (var one in ones)
            {
                if (one.ThreeSkipFull != null)
                {
                    Assert.DoesNotContain(one.ThreeSkipFull, e => e.Id == threeId);
                }

                if (one.JoinThreeFull != null)
                {
                    Assert.DoesNotContain(
                        one.JoinThreeFull,
                        e => e.CompositeId2 == "6_1"
                            && e.CompositeId3 == new DateTime(2006, 1, 1));

                    Assert.DoesNotContain(one.JoinThreeFull, e => e.ThreeId == threeId);
                }
            }

            foreach (var three in threes)
            {
                if (three.CompositeKeySkipFull != null)
                {
                    Assert.DoesNotContain(
                        three.CompositeKeySkipFull,
                        e => e.Key2 == "6_1"
                            && e.Key3 == new DateTime(2006, 1, 1));
                }

                if (three.JoinCompositeKeyFull != null)
                {
                    Assert.DoesNotContain(
                        three.JoinCompositeKeyFull,
                        e => e.CompositeId2 == "6_1"
                            && e.CompositeId3 == new DateTime(2006, 1, 1));

                    Assert.DoesNotContain(three.JoinCompositeKeyFull, e => e.ThreeId == threeId);
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
        List<int> leftKeys = null;
        List<int> rightKeys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                leftKeys = leftEntities.Select(e => e.Id).ToList();
                rightKeys = rightEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityTwo>()
                    .Where(e => leftKeys.Contains(e.Id) || rightKeys.Contains(e.Id))
                    .Include(e => e.SelfSkipSharedLeft);

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(6, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityTwo>()
                    .Select(e => e.Entity)
                    .Where(e => leftKeys.Contains(e.Id))
                    .OrderBy(e => e.Name)
                    .ToList();

                var rightEntities = context.ChangeTracker.Entries<EntityTwo>()
                    .Select(e => e.Entity)
                    .Where(e => rightKeys.Contains(e.Id))
                    .OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityTwo> leftEntities, IList<EntityTwo> rightEntities)
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

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_self()
    {
        List<int> ids = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityTwo>().Include(e => e.SelfSkipSharedRight).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityTwo>().Include(e => e.SelfSkipSharedLeft).OrderBy(e => e.Name).ToListAsync();

                var twos = new[]
                {
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        })
                };

                leftEntities[0].SelfSkipSharedRight.Add(twos[0]);
                leftEntities[0].SelfSkipSharedRight.Add(twos[1]);
                leftEntities[0].SelfSkipSharedRight.Add(twos[2]);

                rightEntities[0].SelfSkipSharedLeft.Add(twos[4]);
                rightEntities[0].SelfSkipSharedLeft.Add(twos[5]);
                rightEntities[0].SelfSkipSharedLeft.Add(twos[6]);

                leftEntities[0].SelfSkipSharedRight.Remove(leftEntities[0].SelfSkipSharedRight.Single(e => e.Name == "EntityTwo 9"));
                rightEntities[1].SelfSkipSharedLeft.Remove(rightEntities[1].SelfSkipSharedLeft.Single(e => e.Name == "EntityTwo 1"));

                leftEntities[4].SelfSkipSharedRight.Remove(leftEntities[4].SelfSkipSharedRight.Single(e => e.Name == "EntityTwo 18"));
                leftEntities[4].SelfSkipSharedRight.Add(twos[3]);

                rightEntities[5].SelfSkipSharedLeft.Remove(rightEntities[5].SelfSkipSharedLeft.Single(e => e.Name == "EntityTwo 12"));
                rightEntities[5].SelfSkipSharedLeft.Add(twos[7]);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                ids = twos.Select(e => context.Entry(e).Property(e => e.Id).CurrentValue).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 28, 42);

                await context.SaveChangesAsync();

                ids = twos.Select(e => e.Id).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 28, 42 - 4);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityTwo>().Include(e => e.SelfSkipSharedRight).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityTwo>().Include(e => e.SelfSkipSharedLeft).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 28, 42 - 4);
            });

        void ValidateFixup(
            DbContext context,
            List<EntityTwo> leftEntities,
            List<EntityTwo> rightEntities,
            int count,
            int joinCount)
        {
            Assert.Equal(count, context.ChangeTracker.Entries<EntityTwo>().Count());
            Assert.Equal(joinCount, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());
            Assert.Equal(count + joinCount, context.ChangeTracker.Entries().Count());

            Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[0]);
            Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[1]);
            Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[2]);

            Assert.Contains(rightEntities[0].SelfSkipSharedLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[4]);
            Assert.Contains(rightEntities[0].SelfSkipSharedLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[5]);
            Assert.Contains(rightEntities[0].SelfSkipSharedLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[6]);

            Assert.DoesNotContain(leftEntities[0].SelfSkipSharedRight, e => e.Name == "EntityTwo 9");
            Assert.DoesNotContain(rightEntities[1].SelfSkipSharedLeft, e => e.Name == "EntityTwo 1");

            Assert.DoesNotContain(leftEntities[4].SelfSkipSharedRight, e => e.Name == "EntityTwo 18");
            Assert.Contains(leftEntities[4].SelfSkipSharedRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[3]);

            Assert.DoesNotContain(rightEntities[5].SelfSkipSharedLeft, e => e.Name == "EntityTwo 12");
            Assert.Contains(rightEntities[5].SelfSkipSharedLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[7]);

            var allLeft = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityTwo>().Where(e => keys.Contains(e.Id)).Include(e => e.ThreeSkipFull);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
                var rightEntities = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityTwo> leftEntities, IList<EntityThree> rightEntities)
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

            var joinEntities = context.ChangeTracker.Entries<JoinTwoToThree>().Select(e => e.Entity).ToList();
            foreach (var joinEntity in joinEntities)
            {
                Assert.Equal(joinEntity.Two.Id, joinEntity.TwoId);
                Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                Assert.Contains(joinEntity, joinEntity.Two.JoinThreeFull);
                Assert.Contains(joinEntity, joinEntity.Three.JoinTwoFull);
            }

            VerifyRelationshipSnapshots(context, joinEntities);
            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_with_navs()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityTwo>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityThree>().Include(e => e.TwoSkipFull).OrderBy(e => e.Name).ToListAsync();

                leftEntities[0].ThreeSkipFull.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }));
                leftEntities[0].ThreeSkipFull.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }));
                leftEntities[0].ThreeSkipFull.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }));

                rightEntities[0].TwoSkipFull.Add(
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }));
                rightEntities[0].TwoSkipFull.Add(
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }));
                rightEntities[0].TwoSkipFull.Add(
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }));

                leftEntities[1].ThreeSkipFull.Remove(leftEntities[1].ThreeSkipFull.Single(e => e.Name == "EntityThree 17"));
                rightEntities[1].TwoSkipFull.Remove(rightEntities[1].TwoSkipFull.Single(e => e.Name == "EntityTwo 6"));

                leftEntities[2].ThreeSkipFull.Remove(leftEntities[2].ThreeSkipFull.Single(e => e.Name == "EntityThree 13"));
                leftEntities[2].ThreeSkipFull.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }));

                rightEntities[2].TwoSkipFull.Remove(rightEntities[2].TwoSkipFull.Single(e => e.Name == "EntityTwo 3"));
                rightEntities[2].TwoSkipFull.Add(
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        }));

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 60);

                await context.SaveChangesAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 60 - 4);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityTwo>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityThree>().Include(e => e.TwoSkipFull).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 60 - 4);
            });

        void ValidateFixup(
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

            Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Name == "Z7721");
            Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Name == "Z7722");
            Assert.Contains(leftEntities[0].ThreeSkipFull, e => e.Name == "Z7723");

            Assert.Contains(rightEntities[0].TwoSkipFull, e => e.Name == "Z7711");
            Assert.Contains(rightEntities[0].TwoSkipFull, e => e.Name == "Z7712");
            Assert.Contains(rightEntities[0].TwoSkipFull, e => e.Name == "Z7713");

            Assert.DoesNotContain(leftEntities[1].ThreeSkipFull, e => e.Name == "EntityThree 17");
            Assert.DoesNotContain(rightEntities[1].TwoSkipFull, e => e.Name == "EntityTwo 6");

            Assert.DoesNotContain(leftEntities[2].ThreeSkipFull, e => e.Name == "EntityThree 13");
            Assert.Contains(leftEntities[2].ThreeSkipFull, e => e.Name == "Z7724");

            Assert.DoesNotContain(rightEntities[2].TwoSkipFull, e => e.Name == "EntityTwo 3");
            Assert.Contains(rightEntities[2].TwoSkipFull, e => e.Name == "Z7714");

            var joinEntries = context.ChangeTracker.Entries<JoinTwoToThree>().ToList();
            foreach (var joinEntry in joinEntries)
            {
                var joinEntity = joinEntry.Entity;
                Assert.Equal(joinEntity.Two.Id, joinEntity.TwoId);
                Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                Assert.Contains(joinEntity, joinEntity.Two.JoinThreeFull);
                Assert.Contains(joinEntity, joinEntity.Three.JoinTwoFull);
            }

            var allLeft = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, joinEntries.Select(e => e.Entity));
            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713),
                };
                var rightEntities = new[]
                {
                    context.Set<EntityBranch>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.Set<EntityBranch>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.Set<EntityBranch>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>().Where(e => keys.Contains(e.Id)).Include(e => e.BranchSkip);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
                var rightEntities = context.ChangeTracker.Entries<EntityBranch>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityBranch> rightEntities)
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

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_with_inheritance()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.BranchSkip).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityBranch>().Include(e => e.OneSkip).OrderBy(e => e.Name).ToListAsync();

                leftEntities[0].BranchSkip.Add(
                    context.Set<EntityBranch>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }));
                leftEntities[0].BranchSkip.Add(
                    context.Set<EntityBranch>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }));
                leftEntities[0].BranchSkip.Add(
                    context.Set<EntityBranch>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }));

                rightEntities[0].OneSkip.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }));
                rightEntities[0].OneSkip.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }));
                rightEntities[0].OneSkip.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }));

                leftEntities[1].BranchSkip.Remove(leftEntities[1].BranchSkip.Single(e => e.Name == "Branch 4"));
                rightEntities[1].OneSkip.Remove(rightEntities[1].OneSkip.Single(e => e.Name == "EntityOne 9"));

                leftEntities[4].BranchSkip.Remove(leftEntities[4].BranchSkip.Single(e => e.Name == "Branch 5"));
                leftEntities[2].BranchSkip.Add(
                    context.Set<EntityBranch>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }));

                rightEntities[2].OneSkip.Remove(rightEntities[2].OneSkip.Single(e => e.Name == "EntityOne 8"));
                rightEntities[2].OneSkip.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        }));

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                ValidateFixup(context, leftEntities, rightEntities, 24, 14, 55);

                await context.SaveChangesAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 14, 55 - 4);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.BranchSkip).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityBranch>().Include(e => e.OneSkip).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 14, 55 - 4);
            });

        void ValidateFixup(
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

            Assert.Contains(leftEntities[0].BranchSkip, e => e.Name == "Z7721");
            Assert.Contains(leftEntities[0].BranchSkip, e => e.Name == "Z7722");
            Assert.Contains(leftEntities[0].BranchSkip, e => e.Name == "Z7723");

            Assert.Contains(rightEntities[0].OneSkip, e => e.Name == "Z7711");
            Assert.Contains(rightEntities[0].OneSkip, e => e.Name == "Z7712");
            Assert.Contains(rightEntities[0].OneSkip, e => e.Name == "Z7713");

            Assert.DoesNotContain(leftEntities[1].BranchSkip, e => e.Name == "Branch 4");
            Assert.DoesNotContain(rightEntities[1].OneSkip, e => e.Name == "EntityOne 9");

            Assert.DoesNotContain(leftEntities[4].BranchSkip, e => e.Name == "Branch 5");
            Assert.Contains(leftEntities[2].BranchSkip, e => e.Name == "Z7724");

            Assert.DoesNotContain(rightEntities[2].OneSkip, e => e.Name == "EntityOne 8");
            Assert.Contains(rightEntities[2].OneSkip, e => e.Name == "Z7714");

            var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<EntityBranch>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
        List<int> leftKeys = null;
        List<int> rightKeys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                leftKeys = leftEntities.Select(e => e.Id).ToList();
                rightKeys = rightEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>()
                    .Where(e => leftKeys.Contains(e.Id) || rightKeys.Contains(e.Id))
                    .Include(e => e.SelfSkipPayloadLeft);

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(6, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityOne>()
                    .Select(e => e.Entity)
                    .Where(e => leftKeys.Contains(e.Id))
                    .OrderBy(e => e.Name)
                    .ToList();

                var rightEntities = context.ChangeTracker.Entries<EntityOne>()
                    .Select(e => e.Entity)
                    .Where(e => rightKeys.Contains(e.Id))
                    .OrderBy(e => e.Name)
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

            var joinEntities = context.ChangeTracker.Entries<JoinOneSelfPayload>().Select(e => e.Entity).ToList();
            foreach (var joinEntity in joinEntities)
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

            VerifyRelationshipSnapshots(context, joinEntities);
            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_self_with_payload()
    {
        List<int> keys = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.SelfSkipPayloadRight).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityOne>().Include(e => e.SelfSkipPayloadLeft).OrderBy(e => e.Name).ToListAsync();

                var ones = new[]
                {
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        })
                };

                leftEntities[0].SelfSkipPayloadRight.Add(ones[0]);
                leftEntities[0].SelfSkipPayloadRight.Add(ones[1]);
                leftEntities[0].SelfSkipPayloadRight.Add(ones[2]);

                rightEntities[0].SelfSkipPayloadLeft.Add(ones[4]);
                rightEntities[0].SelfSkipPayloadLeft.Add(ones[5]);
                rightEntities[0].SelfSkipPayloadLeft.Add(ones[6]);

                leftEntities[7].SelfSkipPayloadRight.Remove(leftEntities[7].SelfSkipPayloadRight.Single(e => e.Name == "EntityOne 6"));
                rightEntities[11].SelfSkipPayloadLeft
                    .Remove(rightEntities[11].SelfSkipPayloadLeft.Single(e => e.Name == "EntityOne 13"));

                leftEntities[4].SelfSkipPayloadRight.Remove(leftEntities[4].SelfSkipPayloadRight.Single(e => e.Name == "EntityOne 18"));
                leftEntities[4].SelfSkipPayloadRight.Add(ones[3]);

                rightEntities[4].SelfSkipPayloadLeft.Remove(rightEntities[4].SelfSkipPayloadLeft.Single(e => e.Name == "EntityOne 6"));
                rightEntities[4].SelfSkipPayloadLeft.Add(ones[7]);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                keys = ones.Select(e => context.Entry(e).Property(e => e.Id).CurrentValue).ToList();

                (await context.FindAsync<JoinOneSelfPayload>(
                        keys[5],
                        context.Entry(context.EntityOnes.Local.Single(e => e.Name == "EntityOne 1")).Property(e => e.Id).CurrentValue))!
                    .Payload = new DateTime(1973, 9, 3);

                (await context.FindAsync<JoinOneSelfPayload>(
                        context.Entry(context.EntityOnes.Local.Single(e => e.Name == "EntityOne 20")).Property(e => e.Id).CurrentValue,
                        context.Entry(context.EntityOnes.Local.Single(e => e.Name == "EntityOne 16")).Property(e => e.Id).CurrentValue))!
                    .Payload = new DateTime(1969, 8, 3);

                ValidateFixup(context, leftEntities, rightEntities, 28, 37, postSave: false);

                await context.SaveChangesAsync();

                keys = ones.Select(e => e.Id).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 28, 37 - 4, postSave: true);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.SelfSkipPayloadRight).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityOne>().Include(e => e.SelfSkipPayloadLeft).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 28, 37 - 4, postSave: true);
            });

        void ValidateFixup(
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

            Assert.Contains(leftEntities[0].SelfSkipPayloadRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[0]);
            Assert.Contains(leftEntities[0].SelfSkipPayloadRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[1]);
            Assert.Contains(leftEntities[0].SelfSkipPayloadRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[2]);

            Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[4]);
            Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[5]);
            Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[6]);

            Assert.DoesNotContain(leftEntities[7].SelfSkipPayloadRight, e => e.Name == "EntityOne 6");
            Assert.DoesNotContain(rightEntities[11].SelfSkipPayloadLeft, e => e.Name == "EntityOne 13");

            Assert.DoesNotContain(leftEntities[4].SelfSkipPayloadRight, e => e.Name == "EntityOne 2");
            Assert.Contains(leftEntities[4].SelfSkipPayloadRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[3]);

            Assert.DoesNotContain(rightEntities[4].SelfSkipPayloadLeft, e => e.Name == "EntityOne 6");
            Assert.Contains(rightEntities[4].SelfSkipPayloadLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[7]);

            var joinEntries = context.ChangeTracker.Entries<JoinOneSelfPayload>().ToList();
            foreach (var joinEntry in joinEntries)
            {
                var joinEntity = joinEntry.Entity;
                Assert.Equal(joinEntity.Left.Id, joinEntity.LeftId);
                Assert.Equal(joinEntity.Right.Id, joinEntity.RightId);
                Assert.Contains(joinEntity, joinEntity.Left.JoinSelfPayloadLeft);
                Assert.Contains(joinEntity, joinEntity.Right.JoinSelfPayloadRight);

                if (joinEntity.LeftId == keys[5]
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

            var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, joinEntries.Select(e => e.Entity));
            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>().Where(e => keys.Contains(e.Id)).Include(e => e.ThreeSkipPayloadFullShared);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
                var rightEntities = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

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

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);

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
    public virtual Task Can_update_many_to_many_shared_with_payload()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFullShared).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared).OrderBy(e => e.Name).ToListAsync();

                leftEntities[0].ThreeSkipPayloadFullShared.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }));
                leftEntities[0].ThreeSkipPayloadFullShared.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }));
                leftEntities[0].ThreeSkipPayloadFullShared.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }));

                rightEntities[0].OneSkipPayloadFullShared.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }));
                rightEntities[0].OneSkipPayloadFullShared.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }));
                rightEntities[0].OneSkipPayloadFullShared.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }));

                leftEntities[2].ThreeSkipPayloadFullShared
                    .Remove(leftEntities[2].ThreeSkipPayloadFullShared.Single(e => e.Name == "EntityThree 10"));
                rightEntities[4].OneSkipPayloadFullShared
                    .Remove(rightEntities[4].OneSkipPayloadFullShared.Single(e => e.Name == "EntityOne 6"));

                leftEntities[3].ThreeSkipPayloadFullShared
                    .Remove(leftEntities[3].ThreeSkipPayloadFullShared.Single(e => e.Name == "EntityThree 17"));
                leftEntities[3].ThreeSkipPayloadFullShared
                    .Add(
                        context.EntityThrees.CreateInstance(
                            (e, p) =>
                            {
                                e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                                e.Name = "Z7724";
                            }));

                rightEntities[2].OneSkipPayloadFullShared
                    .Remove(rightEntities[2].OneSkipPayloadFullShared.Single(e => e.Name == "EntityOne 12"));
                rightEntities[2].OneSkipPayloadFullShared
                    .Add(
                        context.EntityOnes.CreateInstance(
                            (e, p) =>
                            {
                                e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                                e.Name = "Z7714";
                            }));

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                var joinSet = context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared");
                (await joinSet.FindAsync(
                    GetEntityOneId(context, "Z7712"), GetEntityThreeId(context, "EntityThree 1")))!["Payload"] = "Set!";
                (await joinSet.FindAsync(
                    GetEntityOneId(context, "EntityOne 20"), GetEntityThreeId(context, "EntityThree 16")))!["Payload"] = "Changed!";

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48, postSave: false);

                await context.SaveChangesAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48 - 4, postSave: true);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFullShared).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityThree>().Include(e => e.OneSkipPayloadFullShared).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48 - 4, postSave: true);
            });

        static int GetEntityOneId(ManyToManyContext context, string name)
            => context.Entry(context.EntityOnes.Local.Single(e => e.Name == name)).Property(e => e.Id).CurrentValue;

        static int GetEntityThreeId(ManyToManyContext context, string name)
            => context.Entry(context.EntityThrees.Local.Single(e => e.Name == name)).Property(e => e.Id).CurrentValue;

        void ValidateFixup(
            ManyToManyContext context,
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

            Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Name == "Z7721");
            Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Name == "Z7722");
            Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Name == "Z7723");

            Assert.Contains(rightEntities[0].OneSkipPayloadFullShared, e => e.Name == "Z7711");
            Assert.Contains(rightEntities[0].OneSkipPayloadFullShared, e => e.Name == "Z7712");
            Assert.Contains(rightEntities[0].OneSkipPayloadFullShared, e => e.Name == "Z7713");

            Assert.DoesNotContain(leftEntities[2].ThreeSkipPayloadFullShared, e => e.Name == "EntityThree 10");
            Assert.DoesNotContain(rightEntities[4].OneSkipPayloadFullShared, e => e.Name == "EntityOne 6");

            Assert.DoesNotContain(leftEntities[3].ThreeSkipPayloadFullShared, e => e.Name == "EntityThree 17");
            Assert.Contains(leftEntities[3].ThreeSkipPayloadFullShared, e => e.Name == "Z7724");

            Assert.DoesNotContain(rightEntities[2].OneSkipPayloadFullShared, e => e.Name == "EntityOne 12");
            Assert.Contains(rightEntities[2].OneSkipPayloadFullShared, e => e.Name == "Z7714");

            var oneId1 = GetEntityOneId(context, "Z7712");
            var threeId1 = GetEntityThreeId(context, "EntityThree 1");
            var oneId2 = GetEntityOneId(context, "EntityOne 20");
            var threeId2 = GetEntityThreeId(context, "EntityThree 20");

            var joinEntries = context.ChangeTracker.Entries<Dictionary<string, object>>().ToList();
            foreach (var joinEntry in joinEntries)
            {
                var joinEntity = joinEntry.Entity;

                if (context.Entry(joinEntity).Property<int>("OneId").CurrentValue == oneId1
                    && context.Entry(joinEntity).Property<int>("ThreeId").CurrentValue == threeId1)
                {
                    Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Added, joinEntry.State);
                    Assert.Equal("Set!", joinEntity["Payload"]);
                }
                else if (context.Entry(joinEntity).Property<int>("OneId").CurrentValue == oneId2
                         && context.Entry(joinEntity).Property<int>("ThreeId").CurrentValue == threeId2)
                {
                    Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Modified, joinEntry.State);
                    Assert.Equal(!postSave, joinEntry.Property("Payload").IsModified);
                    Assert.Equal("Changed!", joinEntity["Payload"]);
                }
            }

            var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, joinEntries.Select(e => e.Entity));
            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>().Where(e => keys.Contains(e.Id)).Include(e => e.TwoSkipShared);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
                var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityTwo> rightEntities)
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

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_shared()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.TwoSkipShared).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityTwo>().Include(e => e.OneSkipShared).OrderBy(e => e.Name).ToListAsync();

                var twos = new[]
                {
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }),
                };

                var ones = new[]
                {
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        }),
                };

                leftEntities[0].TwoSkipShared.Add(twos[0]);
                leftEntities[0].TwoSkipShared.Add(twos[1]);
                leftEntities[0].TwoSkipShared.Add(twos[2]);

                rightEntities[0].OneSkipShared.Add(ones[0]);
                rightEntities[0].OneSkipShared.Add(ones[1]);
                rightEntities[0].OneSkipShared.Add(ones[2]);

                leftEntities[1].TwoSkipShared.Remove(leftEntities[1].TwoSkipShared.Single(e => e.Name == "EntityTwo 17"));
                rightEntities[1].OneSkipShared.Remove(rightEntities[1].OneSkipShared.Single(e => e.Name == "EntityOne 3"));

                leftEntities[2].TwoSkipShared.Remove(leftEntities[2].TwoSkipShared.Single(e => e.Name == "EntityTwo 18"));
                leftEntities[2].TwoSkipShared.Add(twos[3]);

                rightEntities[2].OneSkipShared.Remove(rightEntities[2].OneSkipShared.Single(e => e.Name == "EntityOne 9"));
                rightEntities[2].OneSkipShared.Add(ones[3]);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53);

                await context.SaveChangesAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 49);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.TwoSkipShared).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityTwo>().Include(e => e.OneSkipShared).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 49);
            });

        void ValidateFixup(
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

            Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Name == "Z7721");
            Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Name == "Z7722");
            Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Name == "Z7723");

            Assert.Contains(rightEntities[0].OneSkipShared, e => e.Name == "Z7711");
            Assert.Contains(rightEntities[0].OneSkipShared, e => e.Name == "Z7712");
            Assert.Contains(rightEntities[0].OneSkipShared, e => e.Name == "Z7713");

            Assert.DoesNotContain(leftEntities[1].TwoSkipShared, e => e.Name == "EntityTwo 17");
            Assert.DoesNotContain(rightEntities[1].OneSkipShared, e => e.Name == "EntityOne 3");

            Assert.DoesNotContain(leftEntities[2].TwoSkipShared, e => e.Name == "EntityTwo 18");
            Assert.Contains(leftEntities[2].TwoSkipShared, e => e.Name == "Z7724");

            Assert.DoesNotContain(rightEntities[2].OneSkipShared, e => e.Name == "EntityOne 9");
            Assert.Contains(rightEntities[2].OneSkipShared, e => e.Name == "Z7714");

            var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>().Where(e => keys.Contains(e.Id)).Include(e => e.ThreeSkipPayloadFull);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
                var rightEntities = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

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

            var joinEntities = context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Select(e => e.Entity).ToList();
            foreach (var joinEntity in joinEntities)
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

            VerifyRelationshipSnapshots(context, joinEntities);
            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_with_payload()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).OrderBy(e => e.Name).ToListAsync();

                leftEntities[0].ThreeSkipPayloadFull.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }));
                leftEntities[0].ThreeSkipPayloadFull.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }));
                leftEntities[0].ThreeSkipPayloadFull.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }));

                rightEntities[0].OneSkipPayloadFull.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }));
                rightEntities[0].OneSkipPayloadFull.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }));
                rightEntities[0].OneSkipPayloadFull.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }));

                leftEntities[1].ThreeSkipPayloadFull
                    .Remove(leftEntities[1].ThreeSkipPayloadFull.Single(e => e.Name == "EntityThree 10"));
                rightEntities[1].OneSkipPayloadFull.Remove(rightEntities[1].OneSkipPayloadFull.Single(e => e.Name == "EntityOne 7"));

                leftEntities[2].ThreeSkipPayloadFull
                    .Remove(leftEntities[2].ThreeSkipPayloadFull.Single(e => e.Name == "EntityThree 13"));
                leftEntities[2].ThreeSkipPayloadFull.Add(
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }));

                rightEntities[2].OneSkipPayloadFull.Remove(rightEntities[2].OneSkipPayloadFull.Single(e => e.Name == "EntityOne 15"));
                rightEntities[2].OneSkipPayloadFull.Add(
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        }));

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                (await context.FindAsync<JoinOneToThreePayloadFull>(
                    GetEntityOneId(context, "Z7712"),
                    GetEntityThreeId(context, "EntityThree 1")))!.Payload = "Set!";

                (await context.FindAsync<JoinOneToThreePayloadFull>(
                    GetEntityOneId(context, "EntityOne 20"),
                    GetEntityThreeId(context, "EntityThree 20")))!.Payload = "Changed!";

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 123, postSave: false);

                await context.SaveChangesAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 123 - 4, postSave: true);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 123 - 4, postSave: true);
            });

        static int GetEntityOneId(ManyToManyContext context, string name)
            => context.Entry(context.EntityOnes.Local.Single(e => e.Name == name)).Property(e => e.Id).CurrentValue;

        static int GetEntityThreeId(ManyToManyContext context, string name)
            => context.Entry(context.EntityThrees.Local.Single(e => e.Name == name)).Property(e => e.Id).CurrentValue;

        void ValidateFixup(
            ManyToManyContext context,
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

            Assert.Contains(leftEntities[0].ThreeSkipPayloadFull, e => e.Name == "Z7721");
            Assert.Contains(leftEntities[0].ThreeSkipPayloadFull, e => e.Name == "Z7722");
            Assert.Contains(leftEntities[0].ThreeSkipPayloadFull, e => e.Name == "Z7723");

            Assert.Contains(rightEntities[0].OneSkipPayloadFull, e => e.Name == "Z7711");
            Assert.Contains(rightEntities[0].OneSkipPayloadFull, e => e.Name == "Z7712");
            Assert.Contains(rightEntities[0].OneSkipPayloadFull, e => e.Name == "Z7713");

            Assert.DoesNotContain(leftEntities[1].ThreeSkipPayloadFull, e => e.Name == "EntityThree 10");
            Assert.DoesNotContain(rightEntities[1].OneSkipPayloadFull, e => e.Name == "EntityOne 7");

            Assert.DoesNotContain(leftEntities[2].ThreeSkipPayloadFull, e => e.Name == "EntityThree 13");
            Assert.Contains(leftEntities[2].ThreeSkipPayloadFull, e => e.Name == "Z7724");

            Assert.DoesNotContain(rightEntities[2].OneSkipPayloadFull, e => e.Name == "EntityOne 15");
            Assert.Contains(rightEntities[2].OneSkipPayloadFull, e => e.Name == "Z7714");

            var oneId1 = GetEntityOneId(context, "Z7712");
            var threeId1 = GetEntityThreeId(context, "EntityThree 1");
            var oneId2 = GetEntityOneId(context, "EntityOne 20");
            var threeId2 = GetEntityThreeId(context, "EntityThree 20");

            var joinEntries = context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().ToList();
            foreach (var joinEntry in joinEntries)
            {
                var joinEntity = joinEntry.Entity;
                Assert.Equal(joinEntity.One.Id, joinEntity.OneId);
                Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                Assert.Contains(joinEntity, joinEntity.One.JoinThreePayloadFull);
                Assert.Contains(joinEntity, joinEntity.Three.JoinOnePayloadFull);

                if (context.Entry(joinEntity).Property(e => e.OneId).CurrentValue == oneId1
                    && context.Entry(joinEntity).Property(e => e.ThreeId).CurrentValue == threeId1)
                {
                    Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Added, joinEntry.State);
                    Assert.Equal("Set!", joinEntity.Payload);
                }
                else if (context.Entry(joinEntity).Property(e => e.OneId).CurrentValue == oneId2
                         && context.Entry(joinEntity).Property(e => e.ThreeId).CurrentValue == threeId2)
                {
                    Assert.Equal(postSave ? EntityState.Unchanged : EntityState.Modified, joinEntry.State);
                    Assert.Equal(!postSave, joinEntry.Property(e => e.Payload).IsModified);
                    Assert.Equal("Changed!", joinEntity.Payload);
                }
            }

            var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, joinEntries.Select(e => e.Entity));
            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
    public virtual Task Can_delete_with_many_to_many_with_navs()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var ones = await context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Name).ToListAsync();
                var threes = await context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).OrderBy(e => e.Name).ToListAsync();

                // Make sure other related entities are loaded for delete fixup
                await context.Set<EntityTwo>().LoadAsync();
                await context.Set<JoinOneSelfPayload>().LoadAsync();
                await context.Set<JoinOneToTwo>().LoadAsync();

                var toRemoveOne = context.EntityOnes.Single(e => e.Name == "EntityOne 1");
                var refCountOnes = threes.SelectMany(e => e.OneSkipPayloadFull).Count(e => e == toRemoveOne);

                var toRemoveThree = context.EntityThrees.Single(e => e.Name == "EntityThree 1");
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

                await context.SaveChangesAsync();

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
            }, async context =>
            {
                var ones = await context.Set<EntityOne>().Include(e => e.ThreeSkipPayloadFull).OrderBy(e => e.Name).ToListAsync();
                var threes = await context.Set<EntityThree>().Include(e => e.OneSkipPayloadFull).OrderBy(e => e.Name).ToListAsync();

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
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>().Where(e => keys.Contains(e.Id)).Include(e => e.TwoSkip);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
                var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityTwo> rightEntities)
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

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public virtual async Task Can_insert_many_to_many_with_suspected_dangling_join(
        bool async,
        bool useTrackGraph,
        bool useDetectChanges)
    {
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                leftEntities[0].TwoSkip = CreateCollection<EntityTwo>();

                if (!useDetectChanges)
                {
                    leftEntities[0].TwoSkip.Add(rightEntities[0]); // 11 - 21
                }

                rightEntities[0].OneSkip = CreateCollection<EntityOne>();

                rightEntities[0].OneSkip.Add(leftEntities[0]); // 21 - 11 (Dupe)
                rightEntities[0].OneSkip.Add(leftEntities[1]); // 21 - 12
                rightEntities[0].OneSkip.Add(leftEntities[2]); // 21 - 13

                var joinEntities = new[]
                {
                    context.Set<JoinOneToTwo>().CreateInstance(
                        (e, p) =>
                        {
                            e.One = leftEntities[0];
                            e.Two = rightEntities[0];
                        }),
                    context.Set<JoinOneToTwo>().CreateInstance(
                        (e, p) =>
                        {
                            e.One = leftEntities[0];
                            e.Two = rightEntities[1];
                        }),
                    context.Set<JoinOneToTwo>().CreateInstance(
                        (e, p) =>
                        {
                            e.One = leftEntities[0];
                            e.Two = rightEntities[2];
                        }),
                    context.Set<JoinOneToTwo>().CreateInstance(
                        (e, p) =>
                        {
                            e.One = leftEntities[1];
                            e.Two = rightEntities[0];
                        }),
                    context.Set<JoinOneToTwo>().CreateInstance(
                        (e, p) =>
                        {
                            e.One = leftEntities[2];
                            e.Two = rightEntities[0];
                        }),
                };

                var extra = context.Set<JoinOneToTwoExtra>().CreateInstance(
                    (e, p) =>
                    {
                        e.JoinEntities = new ObservableCollection<JoinOneToTwo>
                        {
                            joinEntities[0],
                            joinEntities[1],
                            joinEntities[2],
                            joinEntities[3],
                            joinEntities[4],
                        };
                    });

                rightEntities[0].Extra = extra;
                rightEntities[1].Extra = extra;
                rightEntities[2].Extra = extra;

                if (useTrackGraph)
                {
                    foreach (var leftEntity in leftEntities)
                    {
                        context.ChangeTracker.TrackGraph(leftEntity, n => n.Entry.State = EntityState.Added);
                    }
                }
                else
                {
                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    }
                }

                if (useDetectChanges)
                {
                    leftEntities[0].TwoSkip.Add(rightEntities[0]); // 11 - 21

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
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

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>()
                    .Where(e => keys.Contains(e.Id))
                    .Include(e => e.TwoSkip)
                    .ThenInclude(e => e.Extra);

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
                var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityTwo> rightEntities)
        {
            Assert.Equal(12, context.ChangeTracker.Entries().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<EntityOne>().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<EntityTwo>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<JoinOneToTwo>().Count());
            Assert.Single(context.ChangeTracker.Entries<JoinOneToTwoExtra>());

            Assert.Equal(3, leftEntities[0].TwoSkip.Count);
            Assert.Single(leftEntities[1].TwoSkip);
            Assert.Single(leftEntities[2].TwoSkip);

            Assert.Equal(3, rightEntities[0].OneSkip.Count);
            Assert.Single(rightEntities[1].OneSkip);
            Assert.Single(rightEntities[2].OneSkip);

            var extra = context.ChangeTracker.Entries<JoinOneToTwoExtra>().Select(e => e.Entity).Single();
            Assert.Equal(5, extra.JoinEntities.Count);

            foreach (var joinEntity in extra.JoinEntities)
            {
                Assert.NotNull(joinEntity.One);
                Assert.NotNull(joinEntity.Two);
            }

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public virtual async Task Can_insert_many_to_many_with_dangling_join(bool async, bool useTrackGraph, bool useDetectChanges)
    {
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                leftEntities[0].TwoSkip = CreateCollection<EntityTwo>();

                if (!useDetectChanges)
                {
                    leftEntities[0].TwoSkip.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].TwoSkip.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].TwoSkip.Add(rightEntities[2]); // 11 - 23
                }

                rightEntities[0].OneSkip = CreateCollection<EntityOne>();

                rightEntities[0].OneSkip.Add(leftEntities[0]); // 21 - 11 (Dupe)
                rightEntities[0].OneSkip.Add(leftEntities[1]); // 21 - 12
                rightEntities[0].OneSkip.Add(leftEntities[2]); // 21 - 13

                if (useTrackGraph)
                {
                    foreach (var leftEntity in leftEntities)
                    {
                        context.ChangeTracker.TrackGraph(leftEntity, n => n.Entry.State = EntityState.Added);
                    }
                }
                else
                {
                    if (async)
                    {
                        await context.AddRangeAsync(leftEntities[0], leftEntities[1], leftEntities[2]);
                    }
                    else
                    {
                        context.AddRange(leftEntities[0], leftEntities[1], leftEntities[2]);
                    }
                }

                if (useDetectChanges)
                {
                    leftEntities[0].TwoSkip.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].TwoSkip.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].TwoSkip.Add(rightEntities[2]); // 11 - 23

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
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

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>()
                    .Where(e => keys.Contains(e.Id))
                    .Include(e => e.TwoSkip)
                    .ThenInclude(e => e.Extra);

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
                var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<EntityOne> leftEntities, IList<EntityTwo> rightEntities)
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

            var joinEntities = context.ChangeTracker.Entries<JoinOneToTwo>().Select(e => e.Entity).ToList();
            Assert.Equal(5, joinEntities.Count);

            foreach (var joinEntity in joinEntities)
            {
                Assert.NotNull(joinEntity.One);
                Assert.NotNull(joinEntity.Two);
            }

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many()
    {
        List<int> oneIds = null;
        List<int> twoIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityTwo>().Include(e => e.OneSkip).OrderBy(e => e.Name).ToListAsync();

                var twos = new[]
                {
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        })
                };

                var ones = new[]
                {
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }),
                    context.EntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        }),
                };

                leftEntities[0].TwoSkip.Add(twos[0]);
                leftEntities[0].TwoSkip.Add(twos[1]);
                leftEntities[0].TwoSkip.Add(twos[2]);

                rightEntities[0].OneSkip.Add(ones[0]);
                rightEntities[0].OneSkip.Add(ones[1]);
                rightEntities[0].OneSkip.Add(ones[2]);

                leftEntities[1].TwoSkip.Remove(leftEntities[1].TwoSkip.Single(e => e.Name == "EntityTwo 1"));
                rightEntities[1].OneSkip.Remove(rightEntities[1].OneSkip.Single(e => e.Name == "EntityOne 1"));

                leftEntities[2].TwoSkip.Remove(leftEntities[2].TwoSkip.Single(e => e.Name == "EntityTwo 1"));
                leftEntities[2].TwoSkip.Add(twos[3]);

                rightEntities[2].OneSkip.Remove(rightEntities[2].OneSkip.Single(e => e.Name == "EntityOne 1"));
                rightEntities[2].OneSkip.Add(ones[3]);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                oneIds = ones.Select(e => context.Entry(e).Property(e => e.Id).CurrentValue).ToList();
                twoIds = twos.Select(e => context.Entry(e).Property(e => e.Id).CurrentValue).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 120);

                await context.SaveChangesAsync();

                oneIds = ones.Select(e => e.Id).ToList();
                twoIds = twos.Select(e => e.Id).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 116);
            }, async context =>
            {
                var leftEntities = await context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Name).ToListAsync();
                var rightEntities = await context.Set<EntityTwo>().Include(e => e.OneSkip).OrderBy(e => e.Name).ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 116);
            });

        void ValidateFixup(
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

            Assert.Contains(leftEntities[0].TwoSkip, e => context.Entry(e).Property(e => e.Id).CurrentValue == twoIds[0]);
            Assert.Contains(leftEntities[0].TwoSkip, e => context.Entry(e).Property(e => e.Id).CurrentValue == twoIds[1]);
            Assert.Contains(leftEntities[0].TwoSkip, e => context.Entry(e).Property(e => e.Id).CurrentValue == twoIds[2]);

            Assert.Contains(rightEntities[0].OneSkip, e => context.Entry(e).Property(e => e.Id).CurrentValue == oneIds[0]);
            Assert.Contains(rightEntities[0].OneSkip, e => context.Entry(e).Property(e => e.Id).CurrentValue == oneIds[1]);
            Assert.Contains(rightEntities[0].OneSkip, e => context.Entry(e).Property(e => e.Id).CurrentValue == oneIds[2]);

            Assert.DoesNotContain(leftEntities[1].TwoSkip, e => e.Name == "EntityTwo 1");
            Assert.DoesNotContain(rightEntities[1].OneSkip, e => e.Name == "EntityOne 1");

            Assert.DoesNotContain(leftEntities[2].TwoSkip, e => e.Name == "EntityTwo 1");
            Assert.Contains(leftEntities[2].TwoSkip, e => context.Entry(e).Property(e => e.Id).CurrentValue == twoIds[3]);

            Assert.DoesNotContain(rightEntities[2].OneSkip, e => e.Name == "EntityOne 1");
            Assert.Contains(rightEntities[2].OneSkip, e => context.Entry(e).Property(e => e.Id).CurrentValue == oneIds[3]);

            var allLeft = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

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
    public virtual Task Can_delete_with_many_to_many()
    {
        var oneId = 0;
        var twoId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var ones = await context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Name).ToListAsync();
                var twos = await context.Set<EntityTwo>().Include(e => e.OneSkip).OrderBy(e => e.Name).ToListAsync();

                // Make sure other related entities are loaded for delete fixup
                context.Set<EntityThree>().Load();
                context.Set<JoinOneToThreePayloadFull>().Load();
                context.Set<JoinOneSelfPayload>().Load();

                var toRemoveOne = await context.EntityOnes.SingleAsync(e => e.Name == "EntityOne 1");
                oneId = toRemoveOne.Id;
                var refCountOnes = twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne);

                var toRemoveTwo = await context.EntityTwos.SingleAsync(e => e.Name == "EntityTwo 1");
                twoId = toRemoveTwo.Id;
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
                        e.Entity.OneId == oneId
                        || e.Entity.TwoId == twoId
                            ? EntityState.Deleted
                            : EntityState.Unchanged, e.State));

                await context.SaveChangesAsync();

                Assert.Equal(1, twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne));
                Assert.Equal(1, ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo));

                ones.Remove(toRemoveOne);
                twos.Remove(toRemoveTwo);

                Assert.Equal(0, twos.SelectMany(e => e.OneSkip).Count(e => e == toRemoveOne));
                Assert.Equal(0, ones.SelectMany(e => e.TwoSkip).Count(e => e == toRemoveTwo));

                ValidateNavigations(ones, twos);

                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<JoinOneToTwo>(), e => e.Entity.OneId == oneId || e.Entity.TwoId == twoId);
            }, async context =>
            {
                var ones = await context.Set<EntityOne>().Include(e => e.TwoSkip).OrderBy(e => e.Name).ToListAsync();
                var twos = await context.Set<EntityTwo>().Include(e => e.OneSkip).OrderBy(e => e.Name).ToListAsync();

                ValidateNavigations(ones, twos);
                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<JoinOneToTwo>(), e => e.Entity.OneId == oneId || e.Entity.TwoId == twoId);
            });

        void ValidateNavigations(List<EntityOne> ones, List<EntityTwo> twos)
        {
            foreach (var one in ones)
            {
                if (one.TwoSkip != null)
                {
                    Assert.DoesNotContain(one.TwoSkip, e => e.Id == twoId);
                }
            }

            foreach (var two in twos)
            {
                if (two.OneSkip != null)
                {
                    Assert.DoesNotContain(two.OneSkip, e => e.Id == oneId);
                }
            }
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_insert_many_to_many_fully_by_convention(bool async)
    {
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<ImplicitManyToManyA>().Where(e => keys.Contains(e.Id)).Include(e => e.Bs);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                Assert.Equal(11, context.ChangeTracker.Entries().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyA>().Count());
                Assert.Equal(3, context.ChangeTracker.Entries<ImplicitManyToManyB>().Count());
                Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

                var leftEntities = context.ChangeTracker.Entries<ImplicitManyToManyA>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();
                var rightEntities = context.ChangeTracker.Entries<ImplicitManyToManyB>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();

                Assert.Equal(3, leftEntities[0].Bs.Count);
                Assert.Single(leftEntities[1].Bs);
                Assert.Single(leftEntities[2].Bs);

                Assert.Equal(3, rightEntities[0].As.Count);
                Assert.Single(rightEntities[1].As);
                Assert.Single(rightEntities[2].As);
            });

        void ValidateFixup(DbContext context, IList<ImplicitManyToManyA> leftEntities, IList<ImplicitManyToManyB> rightEntities)
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

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
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

                var leftEntities = context.ChangeTracker.Entries<GeneratedKeysLeft>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();
                var rightEntities = context.ChangeTracker.Entries<GeneratedKeysRight>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();

                Assert.Equal(3, leftEntities[0].Rights.Count);
                Assert.Single(leftEntities[1].Rights);
                Assert.Single(leftEntities[2].Rights);

                Assert.Equal(3, rightEntities[0].Lefts.Count);
                Assert.Single(rightEntities[1].Lefts);
                Assert.Single(rightEntities[2].Lefts);
            });

        void ValidateFixup(DbContext context, IList<GeneratedKeysLeft> leftEntities, IList<GeneratedKeysRight> rightEntities)
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

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
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

                var leftEntities = context.ChangeTracker.Entries<GeneratedKeysLeft>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();
                var rightEntities = context.ChangeTracker.Entries<GeneratedKeysRight>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();

                Assert.Equal(3, leftEntities[0].Rights.Count);
                Assert.Single(leftEntities[1].Rights);
                Assert.Single(leftEntities[2].Rights);

                Assert.Equal(3, rightEntities[0].Lefts.Count);
                Assert.Single(rightEntities[1].Lefts);
                Assert.Single(rightEntities[2].Lefts);
            });

        void ValidateFixup(DbContext context, IList<GeneratedKeysLeft> leftEntities, IList<GeneratedKeysRight> rightEntities)
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

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Initial_tracking_uses_skip_navigations(bool async)
    {
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.Set<ImplicitManyToManyA>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.Set<ImplicitManyToManyB>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
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

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<ImplicitManyToManyA>().Where(e => keys.Contains(e.Id)).Include(e => e.Bs);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<ImplicitManyToManyA>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();
                var rightEntities = context.ChangeTracker.Entries<ImplicitManyToManyB>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<ImplicitManyToManyA> leftEntities, IList<ImplicitManyToManyB> rightEntities)
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

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
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

        var leftEntities = context.ChangeTracker.Entries<EntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
        var rightEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

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
    public virtual Task Can_insert_update_delete_shared_type_entity_type()
        => ExecuteWithStrategyInTransactionAsync(
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

                return context.SaveChangesAsync();
            }, async context =>
            {
                var entity = await context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                    .SingleAsync(e => (int)e["OneId"] == 1 && (int)e["ThreeId"] == 1);

                Assert.Equal("NewlyAdded", (string)entity["Payload"]);

                entity["Payload"] = "AlreadyUpdated";

                context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared").Update(entity);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var entity = await context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                    .SingleAsync(e => (int)e["OneId"] == 1 && (int)e["ThreeId"] == 1);

                Assert.Equal("AlreadyUpdated", (string)entity["Payload"]);

                context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared").Remove(entity);

                await context.SaveChangesAsync();

                Assert.False(
                    await context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                        .AnyAsync(e => (int)e["OneId"] == 1 && (int)e["ThreeId"] == 1));
            });

    [ConditionalFact]
    public virtual Task Can_insert_update_delete_proxyable_shared_type_entity_type()
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = context.Set<ProxyableSharedType>("PST").CreateInstance(
                    (e, p) =>
                    {
                        e["Id"] = Fixture.UseGeneratedKeys ? null : 1;
                        e["Payload"] = "NewlyAdded";
                    });

                context.Set<ProxyableSharedType>("PST").Add(entity);

                await context.SaveChangesAsync();

                id = (int)entity["Id"];
            }, async context =>
            {
                var entity = await context.Set<ProxyableSharedType>("PST").SingleAsync(e => (int)e["Id"] == id);

                Assert.Equal("NewlyAdded", (string)entity["Payload"]);

                entity["Payload"] = "AlreadyUpdated";

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                await context.SaveChangesAsync();
            }, async context =>
            {
                var entity = await context.Set<ProxyableSharedType>("PST").SingleAsync(e => (int)e["Id"] == id);

                Assert.Equal("AlreadyUpdated", (string)entity["Payload"]);

                context.Set<ProxyableSharedType>("PST").Remove(entity);

                await context.SaveChangesAsync();

                Assert.False(await context.Set<ProxyableSharedType>("PST").AnyAsync(e => (int)e["Id"] == id));
            });
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_insert_many_to_many_with_navs_by_join_entity(bool async)
    {
        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }),
                    context.EntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        })
                };
                var rightEntities = new[]
                {
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.EntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        })
                };

                var joinEntities = new[]
                {
                    context.Set<JoinTwoToThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Two = leftEntities[0];
                            e.Three = rightEntities[0];
                        }),
                    context.Set<JoinTwoToThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Two = leftEntities[0];
                            e.Three = rightEntities[1];
                        }),
                    context.Set<JoinTwoToThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Two = leftEntities[0];
                            e.Three = rightEntities[2];
                        }),
                    context.Set<JoinTwoToThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Two = leftEntities[1];
                            e.Three = rightEntities[0];
                        }),
                    context.Set<JoinTwoToThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Two = leftEntities[2];
                            e.Three = rightEntities[0];
                        })
                };

                if (async)
                {
                    await context.AddRangeAsync(joinEntities);
                }
                else
                {
                    context.AddRange(joinEntities);
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
                var queryable = context.Set<EntityTwo>()
                    .Where(e => e.Name.StartsWith("Z"))
                    .OrderBy(e => e.Name)
                    .Include(e => e.ThreeSkipFull);

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
                var rightEntities = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

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

    [ConditionalTheory]
    [InlineData(false, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, true, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, true, true)]
    public virtual Task Can_add_and_remove_a_new_relationship(bool modifyLeft, bool modifyRight, bool useJoin, bool useNavs)
    {
        var leftId = -1;
        var rightId = -1;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var left = await context.Set<EntityOne>().Where(e => !e.TwoSkip.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityTwo>().OrderBy(e => e.Id).FirstAsync();

                if (modifyLeft)
                {
                    context.Entry(left).State = EntityState.Modified;
                }

                if (modifyRight)
                {
                    context.Entry(right).State = EntityState.Modified;
                }

                leftId = left.Id;
                rightId = right.Id;

                if (useJoin)
                {
                    context.Add(
                        RequiresDetectChanges
                            ? useNavs
                                ? new JoinOneToTwo { One = left, Two = right }
                                : new JoinOneToTwo { OneId = leftId, TwoId = rightId }
                            : useNavs
                                ? context.CreateProxy<JoinOneToTwo>(
                                    e =>
                                    {
                                        e.One = left;
                                        e.Two = right;
                                    })
                                : context.CreateProxy<JoinOneToTwo>(
                                    e =>
                                    {
                                        e.OneId = leftId;
                                        e.TwoId = rightId;
                                    }));
                }
                else
                {
                    left.TwoSkip ??= CreateCollection<EntityTwo>();
                    left.TwoSkip.Add(right);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Same(right, left.TwoSkip.Single());
                Assert.Same(left, right.OneSkip.Single());

                var joinEntry = context.ChangeTracker.Entries<JoinOneToTwo>().Single();
                Assert.Equal(EntityState.Added, joinEntry.State);
                Assert.Equal(left.Id, joinEntry.Entity.OneId);
                Assert.Equal(right.Id, joinEntry.Entity.TwoId);
                Assert.Same(left, joinEntry.Entity.One);
                Assert.Same(right, joinEntry.Entity.Two);

                if (useJoin)
                {
                    joinEntry.State = EntityState.Detached;
                }
                else
                {
                    right.OneSkip.Remove(left);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Empty(left.TwoSkip);
                Assert.Empty(right.OneSkip);

                Assert.Equal(EntityState.Detached, joinEntry.State);
                Assert.Equal(leftId, joinEntry.Entity.OneId);
                Assert.Equal(rightId, joinEntry.Entity.TwoId);
                Assert.Same(left, joinEntry.Entity.One);
                Assert.Same(right, joinEntry.Entity.Two);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var left = await context.Set<EntityOne>().Where(e => !e.TwoSkip.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityTwo>().OrderBy(e => e.Id).FirstAsync();

                Assert.Equal(leftId, left.Id);
                Assert.Equal(rightId, right.Id);
            });
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public virtual Task Can_add_and_remove_a_new_relationship_self(bool modifyLeft, bool modifyRight, bool useJoin)
    {
        var leftId = -1;
        var rightId = -1;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var left = await context.Set<EntityTwo>().Where(e => !e.SelfSkipSharedRight.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityTwo>().Where(e => !e.SelfSkipSharedLeft.Any()).OrderBy(e => e.Id).FirstAsync();

                if (modifyLeft)
                {
                    context.Entry(left).State = EntityState.Modified;
                }

                if (modifyRight)
                {
                    context.Entry(right).State = EntityState.Modified;
                }

                leftId = left.Id;
                rightId = right.Id;

                if (useJoin && RequiresDetectChanges)
                {
                    context.Set<Dictionary<string, object>>("EntityTwoEntityTwo")
                        .Add(new Dictionary<string, object> { ["SelfSkipSharedLeftId"] = leftId, ["SelfSkipSharedRightId"] = rightId });
                }
                else
                {
                    left.SelfSkipSharedRight ??= CreateCollection<EntityTwo>();
                    left.SelfSkipSharedRight.Add(right);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Same(right, left.SelfSkipSharedRight.Single());
                Assert.Same(left, right.SelfSkipSharedLeft.Single());

                var joinEntry = context.ChangeTracker.Entries<Dictionary<string, object>>().Single();
                Assert.Equal(EntityState.Added, joinEntry.State);
                Assert.Equal(left.Id, joinEntry.Entity["SelfSkipSharedLeftId"]);
                Assert.Equal(right.Id, joinEntry.Entity["SelfSkipSharedRightId"]);

                if (useJoin)
                {
                    joinEntry.State = EntityState.Detached;
                }
                else
                {
                    left.SelfSkipSharedRight.Remove(right);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Empty(left.SelfSkipSharedRight);
                Assert.Empty(right.SelfSkipSharedLeft);

                Assert.Equal(EntityState.Detached, joinEntry.State);
                Assert.Equal(left.Id, joinEntry.Entity["SelfSkipSharedLeftId"]);
                Assert.Equal(right.Id, joinEntry.Entity["SelfSkipSharedRightId"]);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var left = await context.Set<EntityTwo>().Where(e => !e.SelfSkipSharedRight.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityTwo>().Where(e => !e.SelfSkipSharedLeft.Any()).OrderBy(e => e.Id).FirstAsync();

                Assert.Equal(leftId, left.Id);
                Assert.Equal(rightId, right.Id);
            });
    }

    [ConditionalTheory]
    [InlineData(false, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, true, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, true, true)]
    public virtual Task Can_add_and_remove_a_new_relationship_composite_with_navs(
        bool modifyLeft,
        bool modifyRight,
        bool useJoin,
        bool useNavs)
    {
        var leftKey1 = -1;
        var leftKey2 = "-1";
        var leftKey3 = new DateTime();
        var rightId = -1;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var left = await context.Set<EntityCompositeKey>()
                    .Where(e => !e.LeafSkipFull.Any())
                    .OrderBy(e => e.Key1)
                    .ThenBy(e => e.Key2)
                    .ThenBy(e => e.Key3)
                    .FirstAsync();

                var right = await context.Set<EntityLeaf>().OrderBy(e => e.Id).FirstAsync();

                if (modifyLeft)
                {
                    context.Entry(left).State = EntityState.Modified;
                }

                if (modifyRight)
                {
                    context.Entry(right).State = EntityState.Modified;
                }

                leftKey1 = left.Key1;
                leftKey2 = left.Key2;
                leftKey3 = left.Key3;
                rightId = right.Id;

                if (useJoin)
                {
                    context.Add(
                        RequiresDetectChanges
                            ? useNavs
                                ? new JoinCompositeKeyToLeaf { Composite = left, Leaf = right }
                                : new JoinCompositeKeyToLeaf
                                {
                                    CompositeId1 = leftKey1,
                                    CompositeId2 = leftKey2,
                                    CompositeId3 = leftKey3,
                                    LeafId = rightId
                                }
                            : useNavs
                                ? context.CreateProxy<JoinCompositeKeyToLeaf>(
                                    e =>
                                    {
                                        e.Composite = left;
                                        e.Leaf = right;
                                    })
                                : context.CreateProxy<JoinCompositeKeyToLeaf>(
                                    e =>
                                    {
                                        e.CompositeId1 = leftKey1;
                                        e.CompositeId2 = leftKey2;
                                        e.CompositeId3 = leftKey3;
                                        e.LeafId = rightId;
                                    }));
                }
                else
                {
                    left.LeafSkipFull ??= CreateCollection<EntityLeaf>();
                    left.LeafSkipFull.Add(right);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Same(right, left.LeafSkipFull.Single());
                Assert.Same(left, right.CompositeKeySkipFull.Single());

                var joinEntry = context.ChangeTracker.Entries<JoinCompositeKeyToLeaf>().Single();
                Assert.Equal(EntityState.Added, joinEntry.State);
                Assert.Equal(left.Key1, joinEntry.Entity.CompositeId1);
                Assert.Equal(left.Key2, joinEntry.Entity.CompositeId2);
                Assert.Equal(left.Key3, joinEntry.Entity.CompositeId3);
                Assert.Equal(right.Id, joinEntry.Entity.LeafId);
                Assert.Same(left, joinEntry.Entity.Composite);
                Assert.Same(right, joinEntry.Entity.Leaf);
                Assert.Contains(joinEntry.Entity, left.JoinLeafFull);
                Assert.Contains(joinEntry.Entity, right.JoinCompositeKeyFull);

                if (useJoin)
                {
                    joinEntry.State = EntityState.Detached;
                }
                else
                {
                    right.CompositeKeySkipFull.Remove(left);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Empty(left.LeafSkipFull);
                Assert.Empty(right.CompositeKeySkipFull);

                Assert.Equal(EntityState.Detached, joinEntry.State);
                Assert.Equal(left.Key1, joinEntry.Entity.CompositeId1);
                Assert.Equal(left.Key2, joinEntry.Entity.CompositeId2);
                Assert.Equal(left.Key3, joinEntry.Entity.CompositeId3);
                Assert.Equal(right.Id, joinEntry.Entity.LeafId);
                Assert.Same(left, joinEntry.Entity.Composite);
                Assert.Same(right, joinEntry.Entity.Leaf);
                Assert.DoesNotContain(joinEntry.Entity, left.JoinLeafFull);
                Assert.DoesNotContain(joinEntry.Entity, right.JoinCompositeKeyFull);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var left = await context.Set<EntityCompositeKey>()
                    .Where(e => !e.LeafSkipFull.Any())
                    .OrderBy(e => e.Key1)
                    .ThenBy(e => e.Key2)
                    .ThenBy(e => e.Key3)
                    .FirstAsync();

                var right = await context.Set<EntityLeaf>().OrderBy(e => e.Id).FirstAsync();

                Assert.Equal(leftKey1, left.Key1);
                Assert.Equal(leftKey2, left.Key2);
                Assert.Equal(leftKey3, left.Key3);
                Assert.Equal(rightId, right.Id);
            });
    }

    [ConditionalTheory]
    [InlineData(false, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, true, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, true, true)]
    public virtual Task Can_add_and_remove_a_new_relationship_composite_additional_pk_with_navs(
        bool modifyLeft,
        bool modifyRight,
        bool useJoin,
        bool useNavs)
    {
        var leftKey1 = -1;
        var leftKey2 = "-1";
        var leftKey3 = new DateTime();
        var rightId = -1;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var left = await context.Set<EntityCompositeKey>()
                    .Where(e => !e.ThreeSkipFull.Any())
                    .OrderBy(e => e.Key1)
                    .ThenBy(e => e.Key2)
                    .ThenBy(e => e.Key3)
                    .FirstAsync();

                var right = await context.Set<EntityThree>().OrderBy(e => e.Id).FirstAsync();

                if (modifyLeft)
                {
                    context.Entry(left).State = EntityState.Modified;
                }

                if (modifyRight)
                {
                    context.Entry(right).State = EntityState.Modified;
                }

                leftKey1 = left.Key1;
                leftKey2 = left.Key2;
                leftKey3 = left.Key3;
                rightId = right.Id;

                if (useJoin)
                {
                    context.Add(
                        RequiresDetectChanges
                            ? useNavs
                                ? new JoinThreeToCompositeKeyFull { Composite = left, Three = right }
                                : new JoinThreeToCompositeKeyFull
                                {
                                    CompositeId1 = leftKey1,
                                    CompositeId2 = leftKey2,
                                    CompositeId3 = leftKey3,
                                    ThreeId = rightId
                                }
                            : useNavs
                                ? context.CreateProxy<JoinThreeToCompositeKeyFull>(
                                    e =>
                                    {
                                        e.Composite = left;
                                        e.Three = right;
                                    })
                                : context.CreateProxy<JoinThreeToCompositeKeyFull>(
                                    e =>
                                    {
                                        e.CompositeId1 = leftKey1;
                                        e.CompositeId2 = leftKey2;
                                        e.CompositeId3 = leftKey3;
                                        e.ThreeId = rightId;
                                    }));
                }
                else
                {
                    left.ThreeSkipFull ??= CreateCollection<EntityThree>();
                    left.ThreeSkipFull.Add(right);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Same(right, left.ThreeSkipFull.Single());
                Assert.Same(left, right.CompositeKeySkipFull.Single());

                var joinEntry = context.ChangeTracker.Entries<JoinThreeToCompositeKeyFull>().Single();
                Assert.Equal(EntityState.Added, joinEntry.State);
                Assert.Equal(left.Key1, joinEntry.Entity.CompositeId1);
                Assert.Equal(left.Key2, joinEntry.Entity.CompositeId2);
                Assert.Equal(left.Key3, joinEntry.Entity.CompositeId3);
                Assert.Equal(right.Id, joinEntry.Entity.ThreeId);
                Assert.Same(left, joinEntry.Entity.Composite);
                Assert.Same(right, joinEntry.Entity.Three);
                Assert.Contains(joinEntry.Entity, left.JoinThreeFull);
                Assert.Contains(joinEntry.Entity, right.JoinCompositeKeyFull);

                if (useJoin)
                {
                    joinEntry.State = EntityState.Detached;
                }
                else
                {
                    right.CompositeKeySkipFull.Remove(left);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Empty(left.ThreeSkipFull);
                Assert.Empty(right.CompositeKeySkipFull);

                Assert.Equal(EntityState.Detached, joinEntry.State);
                Assert.Equal(left.Key1, joinEntry.Entity.CompositeId1);
                Assert.Equal(left.Key2, joinEntry.Entity.CompositeId2);
                Assert.Equal(left.Key3, joinEntry.Entity.CompositeId3);
                Assert.Equal(right.Id, joinEntry.Entity.ThreeId);
                Assert.Same(left, joinEntry.Entity.Composite);
                Assert.Same(right, joinEntry.Entity.Three);
                Assert.DoesNotContain(joinEntry.Entity, left.JoinThreeFull);
                Assert.DoesNotContain(joinEntry.Entity, right.JoinCompositeKeyFull);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var left = await context.Set<EntityCompositeKey>()
                    .Where(e => !e.ThreeSkipFull.Any())
                    .OrderBy(e => e.Key1)
                    .ThenBy(e => e.Key2)
                    .ThenBy(e => e.Key3)
                    .FirstAsync();

                var right = await context.Set<EntityThree>().OrderBy(e => e.Id).FirstAsync();

                Assert.Equal(leftKey1, left.Key1);
                Assert.Equal(leftKey2, left.Key2);
                Assert.Equal(leftKey3, left.Key3);
                Assert.Equal(rightId, right.Id);
            });
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public virtual Task Can_add_and_remove_a_new_relationship_with_inheritance(bool modifyLeft, bool modifyRight, bool useJoin)
    {
        var leftId = -1;
        var rightId = -1;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var left = await context.Set<EntityOne>().Where(e => !e.BranchSkip.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityBranch>().OrderBy(e => e.Id).FirstAsync();

                if (modifyLeft)
                {
                    context.Entry(left).State = EntityState.Modified;
                }

                if (modifyRight)
                {
                    context.Entry(right).State = EntityState.Modified;
                }

                leftId = left.Id;
                rightId = right.Id;

                if (useJoin)
                {
                    context.Add(
                        RequiresDetectChanges
                            ? new JoinOneToBranch { EntityOneId = leftId, EntityBranchId = rightId }
                            : context.CreateProxy<JoinOneToBranch>(
                                e =>
                                {
                                    e.EntityOneId = leftId;
                                    e.EntityBranchId = rightId;
                                }));
                }
                else
                {
                    left.BranchSkip ??= CreateCollection<EntityBranch>();
                    left.BranchSkip.Add(right);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Same(right, left.BranchSkip.Single());
                Assert.Same(left, right.OneSkip.Single());

                var joinEntry = context.ChangeTracker.Entries<JoinOneToBranch>().Single();
                Assert.Equal(EntityState.Added, joinEntry.State);
                Assert.Equal(left.Id, joinEntry.Entity.EntityOneId);
                Assert.Equal(right.Id, joinEntry.Entity.EntityBranchId);

                if (useJoin)
                {
                    joinEntry.State = EntityState.Detached;
                }
                else
                {
                    right.OneSkip.Remove(left);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Empty(left.BranchSkip);
                Assert.Empty(right.OneSkip);

                Assert.Equal(EntityState.Detached, joinEntry.State);
                Assert.Equal(leftId, joinEntry.Entity.EntityOneId);
                Assert.Equal(rightId, joinEntry.Entity.EntityBranchId);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var left = await context.Set<EntityOne>().Where(e => !e.BranchSkip.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityBranch>().OrderBy(e => e.Id).FirstAsync();

                Assert.Equal(leftId, left.Id);
                Assert.Equal(rightId, right.Id);
            });
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public virtual Task Can_add_and_remove_a_new_relationship_shared_with_payload(bool modifyLeft, bool modifyRight, bool useJoin)
    {
        var leftId = -1;
        var rightId = -1;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var left = await context.Set<EntityOne>().Where(e => !e.ThreeSkipPayloadFullShared.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityThree>().OrderBy(e => e.Id).FirstAsync();

                if (modifyLeft)
                {
                    context.Entry(left).State = EntityState.Modified;
                }

                if (modifyRight)
                {
                    context.Entry(right).State = EntityState.Modified;
                }

                leftId = left.Id;
                rightId = right.Id;

                if (useJoin && RequiresDetectChanges)
                {
                    context.Set<Dictionary<string, object>>("JoinOneToThreePayloadFullShared")
                        .Add(new Dictionary<string, object> { ["OneId"] = leftId, ["ThreeId"] = rightId });
                }
                else
                {
                    left.ThreeSkipPayloadFullShared ??= CreateCollection<EntityThree>();
                    left.ThreeSkipPayloadFullShared.Add(right);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Same(right, left.ThreeSkipPayloadFullShared.Single());
                Assert.Same(left, right.OneSkipPayloadFullShared.Single());

                var joinEntry = context.ChangeTracker.Entries<Dictionary<string, object>>().Single();
                Assert.Equal(EntityState.Added, joinEntry.State);
                Assert.Equal(left.Id, (int)joinEntry.Entity["OneId"]);
                Assert.Equal(right.Id, (int)joinEntry.Entity["ThreeId"]);
                Assert.Contains(joinEntry.Entity, left.JoinThreePayloadFullShared);
                Assert.Contains(joinEntry.Entity, right.JoinOnePayloadFullShared);

                if (useJoin)
                {
                    joinEntry.State = EntityState.Detached;
                }
                else
                {
                    right.OneSkipPayloadFullShared.Remove(left);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Empty(left.ThreeSkipPayloadFullShared);
                Assert.Empty(right.OneSkipPayloadFullShared);
                Assert.Equal(left.Id, (int)joinEntry.Entity["OneId"]);
                Assert.Equal(right.Id, (int)joinEntry.Entity["ThreeId"]);
                Assert.DoesNotContain(joinEntry.Entity, left.JoinThreePayloadFullShared);
                Assert.DoesNotContain(joinEntry.Entity, right.JoinOnePayloadFullShared);

                Assert.Equal(EntityState.Detached, joinEntry.State);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var left = await context.Set<EntityOne>().Where(e => !e.ThreeSkipPayloadFullShared.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityThree>().OrderBy(e => e.Id).FirstAsync();

                Assert.Equal(leftId, left.Id);
                Assert.Equal(rightId, right.Id);
            });
    }

    [ConditionalTheory]
    [InlineData(false, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(true, true, true)]
    public virtual Task Can_add_and_remove_a_new_relationship_shared(bool modifyLeft, bool modifyRight, bool useJoin)
    {
        var leftId = -1;
        var rightId = -1;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var left = await context.Set<EntityOne>().Where(e => !e.TwoSkipShared.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityTwo>().OrderBy(e => e.Id).FirstAsync();

                if (modifyLeft)
                {
                    context.Entry(left).State = EntityState.Modified;
                }

                if (modifyRight)
                {
                    context.Entry(right).State = EntityState.Modified;
                }

                leftId = left.Id;
                rightId = right.Id;

                if (useJoin && RequiresDetectChanges)
                {
                    context.Set<Dictionary<string, object>>("EntityOneEntityTwo")
                        .Add(new Dictionary<string, object> { ["OneSkipSharedId"] = leftId, ["TwoSkipSharedId"] = rightId });
                }
                else
                {
                    left.TwoSkipShared ??= CreateCollection<EntityTwo>();
                    left.TwoSkipShared.Add(right);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Same(right, left.TwoSkipShared.Single());
                Assert.Same(left, right.OneSkipShared.Single());

                var joinEntry = context.ChangeTracker.Entries<Dictionary<string, object>>().Single();
                Assert.Equal(EntityState.Added, joinEntry.State);
                Assert.Equal(left.Id, (int)joinEntry.Entity["OneSkipSharedId"]);
                Assert.Equal(right.Id, (int)joinEntry.Entity["TwoSkipSharedId"]);

                if (useJoin)
                {
                    joinEntry.State = EntityState.Detached;
                }
                else
                {
                    right.OneSkipShared.Remove(left);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Empty(left.TwoSkipShared);
                Assert.Empty(right.OneSkipShared);

                Assert.Equal(EntityState.Detached, joinEntry.State);
                Assert.Equal(left.Id, (int)joinEntry.Entity["OneSkipSharedId"]);
                Assert.Equal(right.Id, (int)joinEntry.Entity["TwoSkipSharedId"]);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var left = await context.Set<EntityOne>().Where(e => !e.TwoSkipShared.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityTwo>().OrderBy(e => e.Id).FirstAsync();

                Assert.Equal(leftId, left.Id);
                Assert.Equal(rightId, right.Id);
            });
    }

    [ConditionalTheory]
    [InlineData(false, false, false, false)]
    [InlineData(false, true, false, false)]
    [InlineData(true, false, false, false)]
    [InlineData(true, true, false, false)]
    [InlineData(false, false, true, false)]
    [InlineData(false, true, true, false)]
    [InlineData(true, false, true, false)]
    [InlineData(true, true, true, false)]
    [InlineData(false, false, true, true)]
    [InlineData(false, true, true, true)]
    [InlineData(true, false, true, true)]
    [InlineData(true, true, true, true)]
    public virtual Task Can_add_and_remove_a_new_relationship_with_payload(
        bool modifyLeft,
        bool modifyRight,
        bool useJoin,
        bool useNavs)
    {
        var leftId = -1;
        var rightId = -1;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var left = await context.Set<EntityOne>().Where(e => !e.ThreeSkipPayloadFull.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityThree>().OrderBy(e => e.Id).FirstAsync();

                if (modifyLeft)
                {
                    context.Entry(left).State = EntityState.Modified;
                }

                if (modifyRight)
                {
                    context.Entry(right).State = EntityState.Modified;
                }

                leftId = left.Id;
                rightId = right.Id;

                if (useJoin)
                {
                    context.Add(
                        RequiresDetectChanges
                            ? useNavs
                                ? new JoinOneToThreePayloadFull { One = left, Three = right }
                                : new JoinOneToThreePayloadFull { OneId = leftId, ThreeId = rightId }
                            : useNavs
                                ? context.CreateProxy<JoinOneToThreePayloadFull>(
                                    e =>
                                    {
                                        e.One = left;
                                        e.Three = right;
                                    })
                                : context.CreateProxy<JoinOneToThreePayloadFull>(
                                    e =>
                                    {
                                        e.OneId = leftId;
                                        e.ThreeId = rightId;
                                    }));
                }
                else
                {
                    left.ThreeSkipPayloadFull ??= CreateCollection<EntityThree>();
                    left.ThreeSkipPayloadFull.Add(right);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Same(right, left.ThreeSkipPayloadFull.Single());
                Assert.Same(left, right.OneSkipPayloadFull.Single());

                var joinEntry = context.ChangeTracker.Entries<JoinOneToThreePayloadFull>().Single();
                Assert.Equal(EntityState.Added, joinEntry.State);
                Assert.Equal(left.Id, joinEntry.Entity.OneId);
                Assert.Equal(right.Id, joinEntry.Entity.ThreeId);
                Assert.Same(left, joinEntry.Entity.One);
                Assert.Same(right, joinEntry.Entity.Three);
                Assert.Contains(joinEntry.Entity, left.JoinThreePayloadFull);
                Assert.Contains(joinEntry.Entity, right.JoinOnePayloadFull);

                if (useJoin)
                {
                    joinEntry.State = EntityState.Detached;
                }
                else
                {
                    right.OneSkipPayloadFull.Remove(left);

                    if (RequiresDetectChanges)
                    {
                        context.ChangeTracker.DetectChanges();
                    }
                }

                Assert.Empty(left.ThreeSkipPayloadFull);
                Assert.Empty(right.OneSkipPayloadFull);

                Assert.Equal(EntityState.Detached, joinEntry.State);
                Assert.Equal(leftId, joinEntry.Entity.OneId);
                Assert.Equal(rightId, joinEntry.Entity.ThreeId);
                Assert.Same(left, joinEntry.Entity.One);
                Assert.Same(right, joinEntry.Entity.Three);
                Assert.DoesNotContain(joinEntry.Entity, left.JoinThreePayloadFull);
                Assert.DoesNotContain(joinEntry.Entity, right.JoinOnePayloadFull);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var left = await context.Set<EntityOne>().Where(e => !e.ThreeSkipPayloadFull.Any()).OrderBy(e => e.Id).FirstAsync();
                var right = await context.Set<EntityThree>().OrderBy(e => e.Id).FirstAsync();

                Assert.Equal(leftId, left.Id);
                Assert.Equal(rightId, right.Id);
            });
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public virtual async Task Can_replace_dependent_with_many_to_many(bool createNewCollection, bool async)
    {
        var principalKey = 0;
        var newRightKey = 0;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var principal = context.EntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711);
                var leftEntity = context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721);
                var rightEntity = context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7731);

                principal.Reference = leftEntity;

                if (leftEntity.ThreeSkipFull == null || createNewCollection)
                {
                    leftEntity.ThreeSkipFull = CreateCollection<EntityThree>();
                }

                leftEntity.ThreeSkipFull.Add(rightEntity);

                _ = async
                    ? await context.AddAsync(principal)
                    : context.Add(principal);

                ValidateFixup(context, principal, leftEntity, rightEntity);

                _ = async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges();

                ValidateFixup(context, principal, leftEntity, rightEntity);

                principalKey = principal.Id;
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>().Where(e => principalKey == e.Id).Include(e => e.Reference.ThreeSkipFull);
                var principal = async ? await queryable.FirstAsync() : queryable.First();

                var leftEntity = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).Single();
                var rightEntity = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).Single();

                ValidateFixup(context, principal, leftEntity, rightEntity);

                var newLeftEntity = context.EntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722);
                var newRightEntity = context.EntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7732);

                principal.Reference = newLeftEntity;

                if (newLeftEntity.ThreeSkipFull == null || createNewCollection)
                {
                    newLeftEntity.ThreeSkipFull = CreateCollection<EntityThree>();
                }

                newLeftEntity.ThreeSkipFull.Add(newRightEntity);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                context.Remove(leftEntity);
                context.Remove(rightEntity);

                _ = async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges();

                ValidateFixup(context, principal, newLeftEntity, newRightEntity);
                newRightKey = newRightEntity.Id;
            },
            async context =>
            {
                var queryable = context.Set<EntityOne>().Where(e => principalKey == e.Id).Include(e => e.Reference.ThreeSkipFull);
                var principal = async ? await queryable.FirstAsync() : queryable.First();

                var leftEntity = context.ChangeTracker.Entries<EntityTwo>().Select(e => e.Entity).Single();
                var rightEntity = context.ChangeTracker.Entries<EntityThree>().Select(e => e.Entity).Single();

                ValidateFixup(context, principal, leftEntity, rightEntity);

                Assert.Equal(newRightKey, principal.Reference.ThreeSkipFull.Single().Id);
            });

        static void ValidateFixup(DbContext context, EntityOne principal, EntityTwo leftEntity, EntityThree rightEntity)
        {
            Assert.Equal(4, context.ChangeTracker.Entries().Count());
            Assert.Single(context.ChangeTracker.Entries<EntityOne>());
            Assert.Single(context.ChangeTracker.Entries<EntityTwo>());
            Assert.Single(context.ChangeTracker.Entries<EntityThree>());
            Assert.Single(context.ChangeTracker.Entries<JoinTwoToThree>());

            Assert.Same(leftEntity, principal.Reference);
            Assert.Same(principal, leftEntity.ReferenceInverse);

            Assert.Same(rightEntity, leftEntity.ThreeSkipFull.Single());
            Assert.Same(leftEntity, rightEntity.TwoSkipFull.Single());

            VerifyRelationshipSnapshots(context, new[] { principal });
            VerifyRelationshipSnapshots(context, new[] { leftEntity });
            VerifyRelationshipSnapshots(context, new[] { rightEntity });
        }
    }

    protected static void VerifyRelationshipSnapshots(DbContext context, IEnumerable<object> entities)
    {
        var detectChanges = context.ChangeTracker.AutoDetectChangesEnabled;
        try
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;

            foreach (var entity in entities)
            {
                var entityEntry = context.Entry(entity).GetInfrastructure();
                var entityType = entityEntry.EntityType;

                if (entityEntry.HasRelationshipSnapshot)
                {
                    foreach (var property in entityType.GetForeignKeys().SelectMany(e => e.Properties))
                    {
                        if (property.GetRelationshipIndex() >= 0)
                        {
                            Assert.Equal(entityEntry.GetRelationshipSnapshotValue(property), entityEntry[property]);
                        }
                    }

                    foreach (var navigation in entityType.GetNavigations()
                                 .Concat((IEnumerable<INavigationBase>)entityType.GetSkipNavigations()))
                    {
                        if (navigation.GetRelationshipIndex() >= 0)
                        {
                            var snapshot = entityEntry.GetRelationshipSnapshotValue(navigation);
                            var current = entityEntry[navigation];

                            if (navigation.IsCollection)
                            {
                                var currentCollection = ((IEnumerable<object>)current)?.ToList();
                                var snapshotCollection = ((IEnumerable<object>)snapshot)?.ToList();

                                if (snapshot == null)
                                {
                                    Assert.True(current == null || !currentCollection.Any());
                                }
                                else if (current == null)
                                {
                                    Assert.True(snapshot == null || !snapshotCollection.Any());
                                }
                                else
                                {
                                    Assert.Equal(snapshotCollection.Count, currentCollection.Count);

                                    foreach (var related in snapshotCollection)
                                    {
                                        Assert.Contains(currentCollection, c => ReferenceEquals(c, related));
                                    }
                                }
                            }
                            else
                            {
                                Assert.Same(snapshot, current);
                            }
                        }
                    }
                }
            }
        }
        finally
        {
            context.ChangeTracker.AutoDetectChangesEnabled = detectChanges;
        }
    }

    private ICollection<TEntity> CreateCollection<TEntity>()
        => RequiresDetectChanges ? new List<TEntity>() : new ObservableCollection<TEntity>();

    protected ManyToManyTrackingTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

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

        public override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
            => base.AddOptions(builder).ConfigureWarnings(
                w => w.Ignore(
                    CoreEventId.MappedEntityTypeIgnoredWarning,
                    CoreEventId.MappedPropertyIgnoredWarning,
                    CoreEventId.MappedNavigationIgnoredWarning));

        protected override string StoreName
            => "ManyToManyTracking";
    }
}

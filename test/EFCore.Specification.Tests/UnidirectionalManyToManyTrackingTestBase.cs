// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.TestModels.ManyToManyModel;

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class ManyToManyTrackingTestBase<TFixture>
{
    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_insert_many_to_many_composite_with_navs_unidirectional(bool async)
    {
        List<int> leftKeys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Key2 = "7711";
                            e.Key3 = new DateTime(7711, 1, 1);
                        }),
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Key2 = "7712";
                            e.Key3 = new DateTime(7712, 1, 1);
                        }),
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Key2 = "7713";
                            e.Key3 = new DateTime(7713, 1, 1);
                        }),
                };
                var rightEntities = new[]
                {
                    context.Set<UnidirectionalEntityLeaf>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.Set<UnidirectionalEntityLeaf>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.Set<UnidirectionalEntityLeaf>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                rightEntities[0].CompositeKeySkipFull = CreateCollection<UnidirectionalEntityCompositeKey>();
                rightEntities[1].CompositeKeySkipFull = CreateCollection<UnidirectionalEntityCompositeKey>();
                rightEntities[2].CompositeKeySkipFull = CreateCollection<UnidirectionalEntityCompositeKey>();

                rightEntities[0].CompositeKeySkipFull.Add(leftEntities[0]); // 21 - 11 (Dupe)
                rightEntities[1].CompositeKeySkipFull.Add(leftEntities[0]); // 22 - 11
                rightEntities[2].CompositeKeySkipFull.Add(leftEntities[0]); // 23 - 11
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
                    await context.SaveChangesAsync();
                }

                ValidateFixup(context, leftEntities, rightEntities);

                leftKeys = leftEntities.Select(e => e.Key1).ToList();
            },
            async context =>
            {
                var queryable = context.Set<UnidirectionalEntityCompositeKey>().Where(e => leftKeys.Contains(e.Key1));
                context.Set<UnidirectionalJoinCompositeKeyToLeaf>().Where(e => leftKeys.Contains(e.CompositeId1)).Include(e => e.Leaf)
                    .Load();

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<UnidirectionalEntityCompositeKey>()
                    .Select(e => e.Entity).OrderBy(e => e.Key2).ToList();

                var rightEntities = context.ChangeTracker.Entries<UnidirectionalEntityLeaf>()
                    .Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(
            DbContext context,
            IList<UnidirectionalEntityCompositeKey> leftEntities,
            IList<UnidirectionalEntityLeaf> rightEntities)
        {
            Assert.Equal(11, context.ChangeTracker.Entries().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityCompositeKey>().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityLeaf>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<UnidirectionalJoinCompositeKeyToLeaf>().Count());

            Assert.Equal(3, rightEntities[0].CompositeKeySkipFull.Count);
            Assert.Single(rightEntities[1].CompositeKeySkipFull);
            Assert.Single(rightEntities[2].CompositeKeySkipFull);

            var joinEntities = context.ChangeTracker.Entries<UnidirectionalJoinCompositeKeyToLeaf>().Select(e => e.Entity).ToList();
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
    public virtual Task Can_update_many_to_many_composite_with_navs_unidirectional()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityCompositeKey>().ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityLeaf>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name)
                    .ToListAsync();

                rightEntities[0].CompositeKeySkipFull.Add(
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Key2 = "7711";
                            e.Key3 = new DateTime(7711, 1, 1);
                            e.Name = "Z7711";
                        }));
                rightEntities[0].CompositeKeySkipFull.Add(
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Key2 = "7712";
                            e.Key3 = new DateTime(7712, 1, 1);
                            e.Name = "Z7712";
                        }));
                rightEntities[0].CompositeKeySkipFull.Add(
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Key2 = "7713";
                            e.Key3 = new DateTime(7713, 1, 1);
                            e.Name = "Z7713";
                        }));

                rightEntities[1].CompositeKeySkipFull.Remove(rightEntities[1].CompositeKeySkipFull.Single(e => e.Key2 == "3_1"));

                rightEntities[2].CompositeKeySkipFull.Remove(rightEntities[2].CompositeKeySkipFull.Single(e => e.Key2 == "8_3"));
                rightEntities[2].CompositeKeySkipFull.Add(
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
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

                ValidateFixup(context, leftEntities, rightEntities, 24, 4, 35);

                await context.SaveChangesAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 4, 35 - 2);
            },
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityCompositeKey>().ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityLeaf>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name)
                    .ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 4, 35 - 2);
            });

        void ValidateFixup(
            DbContext context,
            List<UnidirectionalEntityCompositeKey> leftEntities,
            List<UnidirectionalEntityLeaf> rightEntities,
            int leftCount,
            int rightCount,
            int joinCount)
        {
            Assert.Equal(leftCount, context.ChangeTracker.Entries<UnidirectionalEntityCompositeKey>().Count());
            Assert.Equal(rightCount, context.ChangeTracker.Entries<UnidirectionalEntityLeaf>().Count());
            Assert.Equal(joinCount, context.ChangeTracker.Entries<UnidirectionalJoinCompositeKeyToLeaf>().Count());
            Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

            Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Name == "Z7711");
            Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Name == "Z7712");
            Assert.Contains(rightEntities[0].CompositeKeySkipFull, e => e.Name == "Z7713");

            Assert.DoesNotContain(rightEntities[1].CompositeKeySkipFull, e => e.Key2 == "3_1");

            Assert.DoesNotContain(rightEntities[2].CompositeKeySkipFull, e => e.Key2 == "8_1");
            Assert.Contains(rightEntities[2].CompositeKeySkipFull, e => e.Key2 == "7714");

            var joinEntries = context.ChangeTracker.Entries<UnidirectionalJoinCompositeKeyToLeaf>().ToList();
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

            var allLeft = context.ChangeTracker.Entries<UnidirectionalEntityCompositeKey>().Select(e => e.Entity).OrderBy(e => e.Key2)
                .ToList();
            var allRight = context.ChangeTracker.Entries<UnidirectionalEntityLeaf>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, joinEntries.Select(e => e.Entity));
            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

            var count = 0;
            foreach (var left in allLeft)
            {
                foreach (var right in allRight)
                {
                    if (right.CompositeKeySkipFull?.Contains(left) == true)
                    {
                        count++;
                    }
                }
            }

            var deleted = context.ChangeTracker.Entries<UnidirectionalJoinCompositeKeyToLeaf>().Count(e => e.State == EntityState.Deleted);
            Assert.Equal(joinCount, count + deleted);
        }
    }

    [ConditionalFact]
    public virtual Task Can_delete_with_many_to_many_composite_with_navs_unidirectional()
    {
        var key1 = 0;
        var key2 = "";
        var key3 = default(DateTime);
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var ones = await context.Set<UnidirectionalEntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2)
                    .ToListAsync();
                var threes = await context.Set<UnidirectionalEntityLeaf>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name)
                    .ToListAsync();

                // Make sure other related entities are loaded for delete fixup
                context.Set<UnidirectionalJoinThreeToCompositeKeyFull>().Load();

                var toRemoveOne = context.UnidirectionalEntityCompositeKeys.Single(e => e.Name == "Composite 6");
                key1 = toRemoveOne.Key1;
                key2 = toRemoveOne.Key2;
                key3 = toRemoveOne.Key3;
                var refCountOnes = threes.SelectMany(e => e.CompositeKeySkipFull).Count(e => e == toRemoveOne);

                var toRemoveThree = (UnidirectionalEntityLeaf)context.UnidirectionalEntityRoots.Single(e => e.Name == "Leaf 3");
                id = toRemoveThree.Id;
                var refCountThrees = ones.SelectMany(e => e.RootSkipShared).Count(e => e == toRemoveThree);

                foreach (var joinEntity in context.ChangeTracker.Entries<UnidirectionalJoinCompositeKeyToLeaf>().Select(e => e.Entity)
                             .ToList())
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
                    context.ChangeTracker.Entries<UnidirectionalJoinCompositeKeyToLeaf>(), e => Assert.Equal(
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
                    context.ChangeTracker.Entries<UnidirectionalJoinCompositeKeyToLeaf>(),
                    e => (e.Entity.CompositeId1 == key1
                            && e.Entity.CompositeId2 == key2
                            && e.Entity.CompositeId3 == key3)
                        || e.Entity.LeafId == id);
            },
            async context =>
            {
                var ones = await context.Set<UnidirectionalEntityCompositeKey>().Include(e => e.RootSkipShared).OrderBy(e => e.Key2)
                    .ToListAsync();
                var threes = await context.Set<UnidirectionalEntityLeaf>().Include(e => e.CompositeKeySkipFull).OrderBy(e => e.Name)
                    .ToListAsync();

                ValidateNavigations(ones, threes);

                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<UnidirectionalJoinCompositeKeyToLeaf>(),
                    e => (e.Entity.CompositeId1 == key1
                            && e.Entity.CompositeId2 == key2
                            && e.Entity.CompositeId3 == key3)
                        || e.Entity.LeafId == id);
            });

        void ValidateNavigations(List<UnidirectionalEntityCompositeKey> ones, List<UnidirectionalEntityLeaf> threes)
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
            foreach (var joinEntity in context.ChangeTracker.Entries<UnidirectionalJoinCompositeKeyToLeaf>().Select(e => e.Entity).ToList())
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
    public virtual async Task Can_insert_many_to_many_composite_additional_pk_with_navs_unidirectional(bool async)
    {
        List<string> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Key2 = "7711";
                            e.Key3 = new DateTime(7711, 1, 1);
                        }),
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Key2 = "7712";
                            e.Key3 = new DateTime(7712, 1, 1);
                        }),
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Key2 = "7713";
                            e.Key3 = new DateTime(7713, 1, 1);
                        }),
                };
                var rightEntities = new[]
                {
                    context.Set<UnidirectionalEntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.Set<UnidirectionalEntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.Set<UnidirectionalEntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        })
                };

                leftEntities[0].ThreeSkipFull = CreateCollection<UnidirectionalEntityThree>();
                leftEntities[1].ThreeSkipFull = CreateCollection<UnidirectionalEntityThree>();
                leftEntities[2].ThreeSkipFull = CreateCollection<UnidirectionalEntityThree>();

                leftEntities[0].ThreeSkipFull.Add(rightEntities[0]); // 11 - 21
                leftEntities[1].ThreeSkipFull.Add(rightEntities[0]); // 12 - 21
                leftEntities[2].ThreeSkipFull.Add(rightEntities[0]); // 13 - 21
                leftEntities[0].ThreeSkipFull.Add(rightEntities[1]); // 11 - 22
                leftEntities[0].ThreeSkipFull.Add(rightEntities[2]); // 11 - 23

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
                    await context.SaveChangesAsync();
                }

                ValidateFixup(context, leftEntities, rightEntities, postSave: true);

                keys = leftEntities.Select(e => e.Key2).ToList();
            },
            async context =>
            {
                var queryable = context.Set<UnidirectionalEntityCompositeKey>().Where(e => keys.Contains(e.Key2))
                    .Include(e => e.ThreeSkipFull);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<UnidirectionalEntityCompositeKey>()
                    .Select(e => e.Entity).OrderBy(e => e.Key2).ToList();

                var rightEntities = context.ChangeTracker.Entries<UnidirectionalEntityThree>()
                    .Select(e => e.Entity).OrderBy(e => e.Name).ToList();

                ValidateFixup(context, leftEntities, rightEntities, postSave: true);
            });

        void ValidateFixup(
            DbContext context,
            IList<UnidirectionalEntityCompositeKey> leftEntities,
            IList<UnidirectionalEntityThree> rightEntities,
            bool postSave)
        {
            var entries = context.ChangeTracker.Entries();
            Assert.Equal(11, entries.Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityCompositeKey>().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityThree>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<UnidirectionalJoinThreeToCompositeKeyFull>().Count());

            Assert.Equal(3, leftEntities[0].ThreeSkipFull.Count);
            Assert.Single(leftEntities[1].ThreeSkipFull);
            Assert.Single(leftEntities[2].ThreeSkipFull);

            Assert.Equal(
                3, context.Entry(rightEntities[0]).Collection("UnidirectionalEntityCompositeKey").CurrentValue!.Cast<object>().Count());
            Assert.Single(context.Entry(rightEntities[1]).Collection("UnidirectionalEntityCompositeKey").CurrentValue!.Cast<object>());
            Assert.Single(context.Entry(rightEntities[2]).Collection("UnidirectionalEntityCompositeKey").CurrentValue!.Cast<object>());

            var joinEntities = context.ChangeTracker.Entries<UnidirectionalJoinThreeToCompositeKeyFull>().Select(e => e.Entity).ToList();
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
    public virtual Task Can_update_many_to_many_composite_additional_pk_with_navs_unidirectional()
    {
        List<int> threeIds = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key2)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityThree>().Include("UnidirectionalEntityCompositeKey")
                    .OrderBy(e => e.Name).ToListAsync();

                var threes = new[]
                {
                    context.Set<UnidirectionalEntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.Set<UnidirectionalEntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.Set<UnidirectionalEntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }),
                    context.Set<UnidirectionalEntityThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        })
                };

                var composites = new[]
                {
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Key2 = "Z7711";
                            e.Key3 = new DateTime(7711, 1, 1);
                        }),
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Key2 = "Z7712";
                            e.Key3 = new DateTime(7712, 1, 1);
                        }),
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
                        (e, p) =>
                        {
                            e.Key1 = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Key2 = "Z7713";
                            e.Key3 = new DateTime(7713, 1, 1);
                        }),
                    context.UnidirectionalEntityCompositeKeys.CreateInstance(
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

                var rightNav0 = (ICollection<UnidirectionalEntityCompositeKey>)context.Entry(rightEntities[0])
                    .Collection("UnidirectionalEntityCompositeKey").CurrentValue!;
                rightNav0.Add(composites[0]);
                rightNav0.Add(composites[1]);
                rightNav0.Add(composites[2]);

                leftEntities[0].ThreeSkipFull.Remove(leftEntities[0].ThreeSkipFull.Single(e => e.Name == "EntityThree 2"));
                var rightNav1 = (ICollection<UnidirectionalEntityCompositeKey>)context.Entry(rightEntities[1])
                    .Collection("UnidirectionalEntityCompositeKey").CurrentValue!;
                rightNav1.Remove(rightNav1.Single(e => e.Name == "Composite 16"));

                leftEntities[3].ThreeSkipFull.Remove(leftEntities[3].ThreeSkipFull.Single(e => e.Name == "EntityThree 7"));
                leftEntities[3].ThreeSkipFull.Add(threes[3]);

                var rightNav2 = (ICollection<UnidirectionalEntityCompositeKey>)context.Entry(rightEntities[2])
                    .Collection<UnidirectionalEntityCompositeKey>("UnidirectionalEntityCompositeKey").CurrentValue!;
                rightNav2.Remove(rightNav2.Single(e => e.Name == "Composite 7"));
                rightNav2.Add(composites[3]);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                threeIds = threes.Select(e => context.Entry(e).Property(e => e.Id).CurrentValue).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53);

                await context.SaveChangesAsync();

                threeIds = threes.Select(e => e.Id).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53 - 4);
            },
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key2)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityThree>().Include("UnidirectionalEntityCompositeKey")
                    .OrderBy(e => e.Name)
                    .ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53 - 4);
            });

        void ValidateFixup(
            DbContext context,
            List<UnidirectionalEntityCompositeKey> leftEntities,
            List<UnidirectionalEntityThree> rightEntities,
            int leftCount,
            int rightCount,
            int joinCount)
        {
            Assert.Equal(leftCount, context.ChangeTracker.Entries<UnidirectionalEntityCompositeKey>().Count());
            Assert.Equal(rightCount, context.ChangeTracker.Entries<UnidirectionalEntityThree>().Count());
            Assert.Equal(joinCount, context.ChangeTracker.Entries<UnidirectionalJoinThreeToCompositeKeyFull>().Count());
            Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

            Assert.Contains(leftEntities[0].ThreeSkipFull, e => context.Entry(e).Property(e => e.Id).CurrentValue == threeIds[0]);
            Assert.Contains(leftEntities[0].ThreeSkipFull, e => context.Entry(e).Property(e => e.Id).CurrentValue == threeIds[1]);
            Assert.Contains(leftEntities[0].ThreeSkipFull, e => context.Entry(e).Property(e => e.Id).CurrentValue == threeIds[2]);

            var rightNav0 = context.Entry(rightEntities[0])
                .Collection<UnidirectionalEntityCompositeKey>("UnidirectionalEntityCompositeKey").CurrentValue!;
            Assert.Contains(rightNav0, e => e.Key2 == "Z7711");
            Assert.Contains(rightNav0, e => e.Key2 == "Z7712");
            Assert.Contains(rightNav0, e => e.Key2 == "Z7713");

            Assert.DoesNotContain(leftEntities[0].ThreeSkipFull, e => e.Name == "EntityThree 9");
            var rightNav1 = context.Entry(rightEntities[1])
                .Collection<UnidirectionalEntityCompositeKey>("UnidirectionalEntityCompositeKey").CurrentValue;
            if (rightNav1 != null)
            {
                Assert.DoesNotContain(rightNav1, e => e.Key2 == "9_2");
            }

            Assert.DoesNotContain(leftEntities[3].ThreeSkipFull, e => e.Name == "EntityThree 23");
            Assert.Contains(leftEntities[3].ThreeSkipFull, e => context.Entry(e).Property(e => e.Id).CurrentValue == threeIds[3]);

            var rightNav2 = context.Entry(rightEntities[2])
                .Collection<UnidirectionalEntityCompositeKey>("UnidirectionalEntityCompositeKey").CurrentValue!;
            Assert.DoesNotContain(rightNav2, e => e.Key2 == "6_1");
            Assert.Contains(rightNav2, e => e.Key2 == "Z7714");

            var joinEntries = context.ChangeTracker.Entries<UnidirectionalJoinThreeToCompositeKeyFull>().ToList();
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

            var allLeft = context.ChangeTracker.Entries<UnidirectionalEntityCompositeKey>().Select(e => e.Entity).OrderBy(e => e.Key2)
                .ToList();
            var allRight = context.ChangeTracker.Entries<UnidirectionalEntityThree>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, joinEntries.Select(e => e.Entity));
            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

            var count = 0;
            foreach (var left in allLeft)
            {
                foreach (var right in allRight)
                {
                    var rightNav = context.Entry(right)
                        .Collection<UnidirectionalEntityCompositeKey>("UnidirectionalEntityCompositeKey").CurrentValue;
                    if (left.ThreeSkipFull?.Contains(right) == true)
                    {
                        Assert.Contains(left, rightNav!);
                        count++;
                    }

                    if (rightNav?.Contains(left) == true)
                    {
                        Assert.Contains(right, left.ThreeSkipFull);
                        count++;
                    }
                }
            }

            var deleted = context.ChangeTracker.Entries<UnidirectionalJoinThreeToCompositeKeyFull>()
                .Count(e => e.State == EntityState.Deleted);
            Assert.Equal(joinCount, (count / 2) + deleted);
        }
    }

    [ConditionalFact]
    public virtual Task Can_delete_with_many_to_many_composite_additional_pk_with_navs_unidirectional()
    {
        var threeId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var ones = await context.Set<UnidirectionalEntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key2)
                    .ToListAsync();
                var threes = await context.Set<UnidirectionalEntityThree>().Include("UnidirectionalEntityCompositeKey").OrderBy(e => e.Name)
                    .ToListAsync();

                // Make sure other related entities are loaded for delete fixup
                context.Set<UnidirectionalJoinThreeToCompositeKeyFull>().Load();

                var toRemoveOne = context.UnidirectionalEntityCompositeKeys.Single(e => e.Name == "Composite 6");

                var toRemoveThree = context.UnidirectionalEntityThrees.Single(e => e.Name == "EntityThree 17");
                threeId = toRemoveThree.Id;
                var refCountThrees = ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree);

                foreach (var joinEntity in context.ChangeTracker.Entries<UnidirectionalJoinThreeToCompositeKeyFull>().Select(e => e.Entity)
                             .ToList())
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

                Assert.Equal(refCountThrees, ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree));

                ValidateJoinNavigations(context);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                Assert.Equal(refCountThrees, ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree));

                ValidateJoinNavigations(context);

                Assert.All(
                    context.ChangeTracker.Entries<UnidirectionalJoinThreeToCompositeKeyFull>(), e => Assert.Equal(
                        (e.Entity.CompositeId2 == "6_1"
                            && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                        || e.Entity.ThreeId == threeId
                            ? EntityState.Deleted
                            : EntityState.Unchanged, e.State));

                await context.SaveChangesAsync();

                Assert.Equal(0, ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree));

                ValidateJoinNavigations(context);

                ones.Remove(toRemoveOne);
                threes.Remove(toRemoveThree);

                Assert.Equal(0, ones.SelectMany(e => e.ThreeSkipFull).Count(e => e == toRemoveThree));

                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<UnidirectionalJoinThreeToCompositeKeyFull>(),
                    e => (e.Entity.CompositeId2 == "6_1"
                            && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                        || e.Entity.ThreeId == threeId);
            },
            async context =>
            {
                var ones = await context.Set<UnidirectionalEntityCompositeKey>().Include(e => e.ThreeSkipFull).OrderBy(e => e.Key2)
                    .ToListAsync();
                var threes = await context.Set<UnidirectionalEntityThree>().Include("UnidirectionalEntityCompositeKey").OrderBy(e => e.Name)
                    .ToListAsync();

                ValidateNavigations(context, ones, threes);

                Assert.DoesNotContain(
                    context.ChangeTracker.Entries<UnidirectionalJoinThreeToCompositeKeyFull>(),
                    e => (e.Entity.CompositeId2 == "6_1"
                            && e.Entity.CompositeId3 == new DateTime(2006, 1, 1))
                        || e.Entity.ThreeId == threeId);
            });

        void ValidateNavigations(DbContext context, List<UnidirectionalEntityCompositeKey> ones, List<UnidirectionalEntityThree> threes)
        {
            foreach (var one in ones)
            {
                if (one.ThreeSkipFull != null)
                {
                    Assert.DoesNotContain(one.ThreeSkipFull, e => e.Id == threeId);
                }
            }

            foreach (var three in threes)
            {
                if (three.JoinCompositeKeyFull != null)
                {
                    Assert.DoesNotContain(
                        three.JoinCompositeKeyFull,
                        e => e.CompositeId2 == "6_1"
                            && e.CompositeId3 == new DateTime(2006, 1, 1));

                    Assert.DoesNotContain(three.JoinCompositeKeyFull, e => e.ThreeId == threeId);
                }
            }

            foreach (var three in threes)
            {
                var threeNav = context.Entry(three)
                    .Collection<UnidirectionalEntityCompositeKey>("UnidirectionalEntityCompositeKey").CurrentValue;

                if (threeNav != null)
                {
                    Assert.DoesNotContain(
                        threeNav,
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
            foreach (var joinEntity in context.ChangeTracker.Entries<UnidirectionalJoinThreeToCompositeKeyFull>().Select(e => e.Entity)
                         .ToList())
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
    public virtual async Task Can_insert_many_to_many_self_shared_unidirectional(bool async)
    {
        List<int> leftKeys = null;
        List<int> rightKeys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                var collectionEntry = context.Entry(leftEntities[0]).Collection("UnidirectionalEntityTwo");
                collectionEntry.CurrentValue = CreateCollection<UnidirectionalEntityTwo>();
                var nav0 = (ICollection<UnidirectionalEntityTwo>)collectionEntry.CurrentValue!;

                nav0.Add(rightEntities[0]); // 11 - 21
                nav0.Add(rightEntities[1]); // 11 - 22
                nav0.Add(rightEntities[2]); // 11 - 23

                rightEntities[0].SelfSkipSharedRight = CreateCollection<UnidirectionalEntityTwo>();

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
                    await context.SaveChangesAsync();
                }

                ValidateFixup(context, leftEntities, rightEntities);

                leftKeys = leftEntities.Select(e => e.Id).ToList();
                rightKeys = rightEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<UnidirectionalEntityTwo>()
                    .Where(e => leftKeys.Contains(e.Id) || rightKeys.Contains(e.Id))
                    .Include("UnidirectionalEntityTwo");

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(6, results.Count);

                var leftEntities = context.ChangeTracker.Entries<UnidirectionalEntityTwo>()
                    .Select(e => e.Entity)
                    .Where(e => leftKeys.Contains(e.Id))
                    .OrderBy(e => e.Name)
                    .ToList();

                var rightEntities = context.ChangeTracker.Entries<UnidirectionalEntityTwo>()
                    .Select(e => e.Entity)
                    .Where(e => rightKeys.Contains(e.Id))
                    .OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<UnidirectionalEntityTwo> leftEntities, IList<UnidirectionalEntityTwo> rightEntities)
        {
            Assert.Equal(11, context.ChangeTracker.Entries().Count());
            Assert.Equal(6, context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

            Assert.Equal(
                3, context.Entry(leftEntities[0]).Collection<UnidirectionalEntityTwo>("UnidirectionalEntityTwo").CurrentValue!.Count());
            Assert.Single(context.Entry(leftEntities[1]).Collection<UnidirectionalEntityTwo>("UnidirectionalEntityTwo").CurrentValue!);
            Assert.Single(context.Entry(leftEntities[2]).Collection<UnidirectionalEntityTwo>("UnidirectionalEntityTwo").CurrentValue!);

            Assert.Equal(3, rightEntities[0].SelfSkipSharedRight.Count);
            Assert.Single(rightEntities[1].SelfSkipSharedRight);
            Assert.Single(rightEntities[2].SelfSkipSharedRight);

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_self_unidirectional()
    {
        List<int> ids = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityTwo>().Include(e => e.SelfSkipSharedRight).OrderBy(e => e.Name)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityTwo>().Include("UnidirectionalEntityTwo").OrderBy(e => e.Name)
                    .ToListAsync();

                var twos = new[]
                {
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        })
                };

                leftEntities[0].SelfSkipSharedRight.Add(twos[0]);
                leftEntities[0].SelfSkipSharedRight.Add(twos[1]);
                leftEntities[0].SelfSkipSharedRight.Add(twos[2]);

                var nav0 = (ICollection<UnidirectionalEntityTwo>)context.Entry(rightEntities[0])
                    .Collection<UnidirectionalEntityTwo>("UnidirectionalEntityTwo").CurrentValue!;
                nav0.Add(twos[4]);
                nav0.Add(twos[5]);
                nav0.Add(twos[6]);

                leftEntities[0].SelfSkipSharedRight.Remove(leftEntities[0].SelfSkipSharedRight.Single(e => e.Name == "EntityTwo 9"));
                var nav1 = (ICollection<UnidirectionalEntityTwo>)context.Entry(rightEntities[1])
                    .Collection<UnidirectionalEntityTwo>("UnidirectionalEntityTwo").CurrentValue!;
                nav1.Remove(nav1.Single(e => e.Name == "EntityTwo 1"));

                leftEntities[4].SelfSkipSharedRight.Remove(leftEntities[4].SelfSkipSharedRight.Single(e => e.Name == "EntityTwo 18"));
                leftEntities[4].SelfSkipSharedRight.Add(twos[3]);

                var nav5 = (ICollection<UnidirectionalEntityTwo>)context.Entry(rightEntities[5])
                    .Collection<UnidirectionalEntityTwo>("UnidirectionalEntityTwo").CurrentValue!;
                nav5.Remove(nav5.Single(e => e.Name == "EntityTwo 12"));
                nav5.Add(twos[7]);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                ids = twos.Select(e => context.Entry(e).Property(e => e.Id).CurrentValue).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 28, 42);

                await context.SaveChangesAsync();

                ids = twos.Select(e => e.Id).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 28, 42 - 4);
            },
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityTwo>().Include(e => e.SelfSkipSharedRight).OrderBy(e => e.Name)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityTwo>().Include("UnidirectionalEntityTwo").OrderBy(e => e.Name)
                    .ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 28, 42 - 4);
            });

        void ValidateFixup(
            DbContext context,
            List<UnidirectionalEntityTwo> leftEntities,
            List<UnidirectionalEntityTwo> rightEntities,
            int count,
            int joinCount)
        {
            Assert.Equal(count, context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Count());
            Assert.Equal(joinCount, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());
            Assert.Equal(count + joinCount, context.ChangeTracker.Entries().Count());

            Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[0]);
            Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[1]);
            Assert.Contains(leftEntities[0].SelfSkipSharedRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[2]);

            var nav0 = context.Entry(rightEntities[0]).Collection<UnidirectionalEntityTwo>("UnidirectionalEntityTwo").CurrentValue!;
            Assert.Contains(nav0, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[4]);
            Assert.Contains(nav0, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[5]);
            Assert.Contains(nav0, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[6]);

            var nav1 = context.Entry(rightEntities[1]).Collection<UnidirectionalEntityTwo>("UnidirectionalEntityTwo").CurrentValue!;
            Assert.DoesNotContain(leftEntities[0].SelfSkipSharedRight, e => e.Name == "EntityTwo 9");
            Assert.DoesNotContain(nav1, e => e.Name == "EntityTwo 1");

            Assert.DoesNotContain(leftEntities[4].SelfSkipSharedRight, e => e.Name == "EntityTwo 18");
            Assert.Contains(leftEntities[4].SelfSkipSharedRight, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[3]);

            var nav5 = context.Entry(rightEntities[5]).Collection<UnidirectionalEntityTwo>("UnidirectionalEntityTwo").CurrentValue!;
            Assert.DoesNotContain(nav5, e => e.Name == "EntityTwo 12");
            Assert.Contains(nav5, e => context.Entry(e).Property(e => e.Id).CurrentValue == ids[7]);

            var allLeft = context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

            var joins = 0;
            foreach (var left in allLeft)
            {
                foreach (var right in allRight)
                {
                    var rightNav = context.Entry(right).Collection<UnidirectionalEntityTwo>("UnidirectionalEntityTwo").CurrentValue;
                    if (left.SelfSkipSharedRight?.Contains(right) == true)
                    {
                        Assert.Contains(left, rightNav!);
                        joins++;
                    }

                    if (rightNav?.Contains(left) == true)
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
    public virtual async Task Can_insert_many_to_many_with_inheritance_unidirectional(bool async)
    {
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713),
                };
                var rightEntities = new[]
                {
                    context.Set<UnidirectionalEntityBranch>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.Set<UnidirectionalEntityBranch>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.Set<UnidirectionalEntityBranch>().CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                leftEntities[0].BranchSkip = CreateCollection<UnidirectionalEntityBranch>();
                leftEntities[1].BranchSkip = CreateCollection<UnidirectionalEntityBranch>();
                leftEntities[2].BranchSkip = CreateCollection<UnidirectionalEntityBranch>();

                leftEntities[0].BranchSkip.Add(rightEntities[0]); // 11 - 21
                leftEntities[1].BranchSkip.Add(rightEntities[0]); // 12 - 21
                leftEntities[2].BranchSkip.Add(rightEntities[0]); // 13 - 21
                leftEntities[0].BranchSkip.Add(rightEntities[1]); // 11 - 22
                leftEntities[0].BranchSkip.Add(rightEntities[2]); // 11 - 23

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
                    await context.SaveChangesAsync();
                }

                ValidateFixup(context, leftEntities, rightEntities);

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<UnidirectionalEntityOne>().Where(e => keys.Contains(e.Id)).Include(e => e.BranchSkip);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<UnidirectionalEntityOne>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();
                var rightEntities = context.ChangeTracker.Entries<UnidirectionalEntityBranch>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<UnidirectionalEntityOne> leftEntities, IList<UnidirectionalEntityBranch> rightEntities)
        {
            Assert.Equal(11, context.ChangeTracker.Entries().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityOne>().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityBranch>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<UnidirectionalJoinOneToBranch>().Count());

            Assert.Equal(3, leftEntities[0].BranchSkip.Count);
            Assert.Single(leftEntities[1].BranchSkip);
            Assert.Single(leftEntities[2].BranchSkip);

            Assert.Equal(3, context.Entry(rightEntities[0]).Collection("UnidirectionalEntityOne").CurrentValue!.Cast<object>().Count());
            Assert.Single(context.Entry(rightEntities[1]).Collection("UnidirectionalEntityOne").CurrentValue!.Cast<object>());
            Assert.Single(context.Entry(rightEntities[2]).Collection("UnidirectionalEntityOne").CurrentValue!.Cast<object>());

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_with_inheritance_unidirectional()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityOne>().Include(e => e.BranchSkip).OrderBy(e => e.Name)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityBranch>().Include("UnidirectionalEntityOne").OrderBy(e => e.Name)
                    .ToListAsync();

                leftEntities[0].BranchSkip.Add(
                    context.Set<UnidirectionalEntityBranch>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }));
                leftEntities[0].BranchSkip.Add(
                    context.Set<UnidirectionalEntityBranch>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }));
                leftEntities[0].BranchSkip.Add(
                    context.Set<UnidirectionalEntityBranch>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }));

                var rightNav0 = (ICollection<UnidirectionalEntityOne>)context.Entry(rightEntities[0])
                    .Collection("UnidirectionalEntityOne").CurrentValue!;

                rightNav0.Add(
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }));
                rightNav0.Add(
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }));
                rightNav0.Add(
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }));

                leftEntities[1].BranchSkip.Remove(leftEntities[1].BranchSkip.Single(e => e.Name == "Branch 4"));
                var rightNav1 = (ICollection<UnidirectionalEntityOne>)context.Entry(rightEntities[1])
                    .Collection("UnidirectionalEntityOne").CurrentValue!;
                rightNav1.Remove(rightNav1.Single(e => e.Name == "EntityOne 9"));

                leftEntities[4].BranchSkip.Remove(leftEntities[4].BranchSkip.Single(e => e.Name == "Branch 5"));
                leftEntities[2].BranchSkip.Add(
                    context.Set<UnidirectionalEntityBranch>().CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }));

                var rightNav2 = (ICollection<UnidirectionalEntityOne>)context.Entry(rightEntities[2])
                    .Collection("UnidirectionalEntityOne").CurrentValue!;
                rightNav2.Remove(rightNav2.Single(e => e.Name == "EntityOne 8"));
                rightNav2.Add(
                    context.UnidirectionalEntityOnes.CreateInstance(
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
            },
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityOne>().Include(e => e.BranchSkip).OrderBy(e => e.Name)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityBranch>().Include("UnidirectionalEntityOne").OrderBy(e => e.Name)
                    .ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 14, 55 - 4);
            });

        void ValidateFixup(
            DbContext context,
            List<UnidirectionalEntityOne> leftEntities,
            List<UnidirectionalEntityBranch> rightEntities,
            int leftCount,
            int rightCount,
            int joinCount)
        {
            Assert.Equal(leftCount, context.ChangeTracker.Entries<UnidirectionalEntityOne>().Count());
            Assert.Equal(rightCount, context.ChangeTracker.Entries<UnidirectionalEntityBranch>().Count());
            Assert.Equal(joinCount, context.ChangeTracker.Entries<UnidirectionalJoinOneToBranch>().Count());
            Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

            Assert.Contains(leftEntities[0].BranchSkip, e => e.Name == "Z7721");
            Assert.Contains(leftEntities[0].BranchSkip, e => e.Name == "Z7722");
            Assert.Contains(leftEntities[0].BranchSkip, e => e.Name == "Z7723");

            var rightNav0 = context.Entry(rightEntities[0]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!;
            Assert.Contains(rightNav0, e => e.Name == "Z7711");
            Assert.Contains(rightNav0, e => e.Name == "Z7712");
            Assert.Contains(rightNav0, e => e.Name == "Z7713");

            Assert.DoesNotContain(leftEntities[1].BranchSkip, e => e.Name == "Branch 4");
            var rightNav1 = context.Entry(rightEntities[1]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!;
            Assert.DoesNotContain(rightNav1, e => e.Name == "EntityOne 9");

            Assert.DoesNotContain(leftEntities[4].BranchSkip, e => e.Name == "Branch 5");
            Assert.Contains(leftEntities[2].BranchSkip, e => e.Name == "Z7724");

            var allLeft = context.ChangeTracker.Entries<UnidirectionalEntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<UnidirectionalEntityBranch>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

            var count = 0;
            foreach (var left in allLeft)
            {
                foreach (var right in allRight)
                {
                    var rightNav = context.Entry(right).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue;
                    if (left.BranchSkip?.Contains(right) == true)
                    {
                        Assert.Contains(left, rightNav);
                        count++;
                    }

                    if (rightNav?.Contains(left) == true)
                    {
                        Assert.Contains(right, left.BranchSkip);
                        count++;
                    }
                }
            }

            var deleted = context.ChangeTracker.Entries<UnidirectionalJoinOneToBranch>().Count(e => e.State == EntityState.Deleted);
            Assert.Equal(joinCount, (count / 2) + deleted);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_insert_many_to_many_self_with_payload_unidirectional(bool async)
    {
        List<int> leftKeys = null;
        List<int> rightKeys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                leftEntities[0].SelfSkipPayloadLeft = CreateCollection<UnidirectionalEntityOne>();

                leftEntities[0].SelfSkipPayloadLeft.Add(rightEntities[0]); // 11 - 21
                leftEntities[0].SelfSkipPayloadLeft.Add(rightEntities[1]); // 11 - 22
                leftEntities[0].SelfSkipPayloadLeft.Add(rightEntities[2]); // 11 - 23

                var rightNav0 = new ObservableCollection<UnidirectionalEntityOne>();
                context.Entry(rightEntities[0]).Collection("UnidirectionalEntityOne").CurrentValue = rightNav0;

                rightNav0.Add(leftEntities[0]); // 21 - 11 (Dupe)
                rightNav0.Add(leftEntities[1]); // 21 - 12
                rightNav0.Add(leftEntities[2]); // 21 - 13

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
                    await context.SaveChangesAsync();
                }

                ValidateFixup(context, leftEntities, rightEntities, postSave: true);

                leftKeys = leftEntities.Select(e => e.Id).ToList();
                rightKeys = rightEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<UnidirectionalEntityOne>()
                    .Where(e => leftKeys.Contains(e.Id) || rightKeys.Contains(e.Id))
                    .Include(e => e.SelfSkipPayloadLeft);

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(6, results.Count);

                var leftEntities = context.ChangeTracker.Entries<UnidirectionalEntityOne>()
                    .Select(e => e.Entity)
                    .Where(e => leftKeys.Contains(e.Id))
                    .OrderBy(e => e.Name)
                    .ToList();

                var rightEntities = context.ChangeTracker.Entries<UnidirectionalEntityOne>()
                    .Select(e => e.Entity)
                    .Where(e => rightKeys.Contains(e.Id))
                    .OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities, postSave: true);
            });

        void ValidateFixup(
            DbContext context,
            IList<UnidirectionalEntityOne> leftEntities,
            IList<UnidirectionalEntityOne> rightEntities,
            bool postSave)
        {
            Assert.Equal(11, context.ChangeTracker.Entries().Count());
            Assert.Equal(6, context.ChangeTracker.Entries<UnidirectionalEntityOne>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<UnidirectionalJoinOneSelfPayload>().Count());

            Assert.Equal(3, leftEntities[0].SelfSkipPayloadLeft.Count);
            Assert.Single(leftEntities[1].SelfSkipPayloadLeft);
            Assert.Single(leftEntities[2].SelfSkipPayloadLeft);

            Assert.Equal(
                3, context.Entry(rightEntities[0]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!.Count());
            Assert.Single(context.Entry(rightEntities[1]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!);
            Assert.Single(context.Entry(rightEntities[2]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!);

            var joinEntities = context.ChangeTracker.Entries<UnidirectionalJoinOneSelfPayload>().Select(e => e.Entity).ToList();
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
    public virtual Task Can_update_many_to_many_self_with_payload_unidirectional()
    {
        List<int> keys = null;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityOne>().Include("UnidirectionalEntityOne").OrderBy(e => e.Name)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityOne>().Include(e => e.SelfSkipPayloadLeft).OrderBy(e => e.Name)
                    .ToListAsync();

                var ones = new[]
                {
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }),
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }),
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }),
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }),
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }),
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        })
                };

                var leftNav0 = (ICollection<UnidirectionalEntityOne>)context.Entry(leftEntities[0])
                    .Collection("UnidirectionalEntityOne").CurrentValue!;
                leftNav0.Add(ones[0]);
                leftNav0.Add(ones[1]);
                leftNav0.Add(ones[2]);

                rightEntities[0].SelfSkipPayloadLeft.Add(ones[4]);
                rightEntities[0].SelfSkipPayloadLeft.Add(ones[5]);
                rightEntities[0].SelfSkipPayloadLeft.Add(ones[6]);

                var leftNav7 = (ICollection<UnidirectionalEntityOne>)context.Entry(leftEntities[7])
                    .Collection("UnidirectionalEntityOne").CurrentValue!;
                leftNav7.Remove(leftNav7.Single(e => e.Name == "EntityOne 6"));
                rightEntities[11].SelfSkipPayloadLeft
                    .Remove(rightEntities[11].SelfSkipPayloadLeft.Single(e => e.Name == "EntityOne 13"));

                var leftNav4 = (ICollection<UnidirectionalEntityOne>)context.Entry(leftEntities[4])
                    .Collection("UnidirectionalEntityOne").CurrentValue!;
                leftNav4.Remove(leftNav4.Single(e => e.Name == "EntityOne 18"));
                leftNav4.Add(ones[3]);

                rightEntities[4].SelfSkipPayloadLeft.Remove(rightEntities[4].SelfSkipPayloadLeft.Single(e => e.Name == "EntityOne 6"));
                rightEntities[4].SelfSkipPayloadLeft.Add(ones[7]);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                keys = ones.Select(e => context.Entry(e).Property(e => e.Id).CurrentValue).ToList();

                context.Find<UnidirectionalJoinOneSelfPayload>(
                        keys[5],
                        context.Entry(context.UnidirectionalEntityOnes.Local.Single(e => e.Name == "EntityOne 1")).Property(e => e.Id)
                            .CurrentValue)
                    .Payload = new DateTime(1973, 9, 3);

                context.Find<UnidirectionalJoinOneSelfPayload>(
                        context.Entry(context.UnidirectionalEntityOnes.Local.Single(e => e.Name == "EntityOne 20")).Property(e => e.Id)
                            .CurrentValue,
                        context.Entry(context.UnidirectionalEntityOnes.Local.Single(e => e.Name == "EntityOne 16")).Property(e => e.Id)
                            .CurrentValue)
                    .Payload = new DateTime(1969, 8, 3);

                ValidateFixup(context, leftEntities, rightEntities, 28, 37, postSave: false);

                await context.SaveChangesAsync();

                keys = ones.Select(e => e.Id).ToList();

                ValidateFixup(context, leftEntities, rightEntities, 28, 37 - 4, postSave: true);
            },
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityOne>().Include("UnidirectionalEntityOne").OrderBy(e => e.Name)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityOne>().Include(e => e.SelfSkipPayloadLeft).OrderBy(e => e.Name)
                    .ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 28, 37 - 4, postSave: true);
            });

        void ValidateFixup(
            DbContext context,
            List<UnidirectionalEntityOne> leftEntities,
            List<UnidirectionalEntityOne> rightEntities,
            int count,
            int joinCount,
            bool postSave)
        {
            Assert.Equal(count, context.ChangeTracker.Entries<UnidirectionalEntityOne>().Count());
            Assert.Equal(joinCount, context.ChangeTracker.Entries<UnidirectionalJoinOneSelfPayload>().Count());
            Assert.Equal(count + joinCount, context.ChangeTracker.Entries().Count());

            var leftNav0 = context.Entry(leftEntities[0]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!;
            Assert.Contains(leftNav0, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[0]);
            Assert.Contains(leftNav0, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[1]);
            Assert.Contains(leftNav0, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[2]);

            Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[4]);
            Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[5]);
            Assert.Contains(rightEntities[0].SelfSkipPayloadLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[6]);

            var leftNav7 = context.Entry(leftEntities[7]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!;
            Assert.DoesNotContain(leftNav7, e => e.Name == "EntityOne 6");
            Assert.DoesNotContain(rightEntities[11].SelfSkipPayloadLeft, e => e.Name == "EntityOne 13");

            var leftNav4 = context.Entry(leftEntities[4]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!;
            Assert.DoesNotContain(leftNav4, e => e.Name == "EntityOne 2");
            Assert.Contains(leftNav4, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[3]);

            Assert.DoesNotContain(rightEntities[4].SelfSkipPayloadLeft, e => e.Name == "EntityOne 6");
            Assert.Contains(rightEntities[4].SelfSkipPayloadLeft, e => context.Entry(e).Property(e => e.Id).CurrentValue == keys[7]);

            var joinEntries = context.ChangeTracker.Entries<UnidirectionalJoinOneSelfPayload>().ToList();
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

            var allLeft = context.ChangeTracker.Entries<UnidirectionalEntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<UnidirectionalEntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, joinEntries.Select(e => e.Entity));
            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

            var joins = 0;
            foreach (var left in allLeft)
            {
                var leftNav = context.Entry(left).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue;
                foreach (var right in allRight)
                {
                    if (leftNav?.Contains(right) == true)
                    {
                        Assert.Contains(left, right.SelfSkipPayloadLeft);
                        joins++;
                    }

                    if (right.SelfSkipPayloadLeft?.Contains(left) == true)
                    {
                        Assert.Contains(right, leftNav!);
                        joins++;
                    }
                }
            }

            var deleted = context.ChangeTracker.Entries<UnidirectionalJoinOneSelfPayload>().Count(e => e.State == EntityState.Deleted);
            Assert.Equal(joinCount, (joins / 2) + deleted);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_insert_many_to_many_shared_with_payload_unidirectional(bool async)
    {
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.UnidirectionalEntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.UnidirectionalEntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.UnidirectionalEntityThrees.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                leftEntities[0].ThreeSkipPayloadFullShared = CreateCollection<UnidirectionalEntityThree>();

                leftEntities[0].ThreeSkipPayloadFullShared.Add(rightEntities[0]); // 11 - 21
                leftEntities[0].ThreeSkipPayloadFullShared.Add(rightEntities[1]); // 11 - 22
                leftEntities[0].ThreeSkipPayloadFullShared.Add(rightEntities[2]); // 11 - 23

                var rightNav0 = (ICollection<UnidirectionalEntityOne>)context.Entry(rightEntities[0])
                    .Collection("UnidirectionalEntityOne1").CurrentValue!;
                rightNav0.Add(leftEntities[0]); // 21 - 11 (Dupe)
                rightNav0.Add(leftEntities[1]); // 21 - 12
                rightNav0.Add(leftEntities[2]); // 21 - 13

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
                    await context.SaveChangesAsync();
                }

                ValidateFixup(context, leftEntities, rightEntities, postSave: true);

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<UnidirectionalEntityOne>().Where(e => keys.Contains(e.Id))
                    .Include(e => e.ThreeSkipPayloadFullShared);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<UnidirectionalEntityOne>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();
                var rightEntities = context.ChangeTracker.Entries<UnidirectionalEntityThree>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities, postSave: true);
            });

        void ValidateFixup(
            DbContext context,
            IList<UnidirectionalEntityOne> leftEntities,
            IList<UnidirectionalEntityThree> rightEntities,
            bool postSave)
        {
            Assert.Equal(11, context.ChangeTracker.Entries().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityOne>().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityThree>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

            Assert.Equal(3, leftEntities[0].ThreeSkipPayloadFullShared.Count);
            Assert.Single(leftEntities[1].ThreeSkipPayloadFullShared);
            Assert.Single(leftEntities[2].ThreeSkipPayloadFullShared);

            Assert.Equal(3, context.Entry(rightEntities[0]).Collection("UnidirectionalEntityOne1").CurrentValue!.Cast<object>().Count());
            Assert.Single(context.Entry(rightEntities[1]).Collection("UnidirectionalEntityOne1").CurrentValue!.Cast<object>());
            Assert.Single(context.Entry(rightEntities[2]).Collection("UnidirectionalEntityOne1").CurrentValue!.Cast<object>());

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
    public virtual Task Can_update_many_to_many_shared_with_payload_unidirectional()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityOne>().Include(e => e.ThreeSkipPayloadFullShared)
                    .OrderBy(e => e.Name)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityThree>().Include("UnidirectionalEntityOne1").OrderBy(e => e.Name)
                    .ToListAsync();

                leftEntities[0].ThreeSkipPayloadFullShared.Add(
                    context.UnidirectionalEntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }));
                leftEntities[0].ThreeSkipPayloadFullShared.Add(
                    context.UnidirectionalEntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }));
                leftEntities[0].ThreeSkipPayloadFullShared.Add(
                    context.UnidirectionalEntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }));

                var rightNav0 = (ICollection<UnidirectionalEntityOne>)context.Entry(rightEntities[0])
                    .Collection("UnidirectionalEntityOne1").CurrentValue!;

                rightNav0.Add(
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }));
                rightNav0.Add(
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }));
                rightNav0.Add(
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }));

                leftEntities[2].ThreeSkipPayloadFullShared
                    .Remove(leftEntities[2].ThreeSkipPayloadFullShared.Single(e => e.Name == "EntityThree 10"));
                var rightNav4 = (ICollection<UnidirectionalEntityOne>)context.Entry(rightEntities[4])
                    .Collection("UnidirectionalEntityOne1").CurrentValue!;
                rightNav4.Remove(rightNav4.Single(e => e.Name == "EntityOne 6"));

                leftEntities[3].ThreeSkipPayloadFullShared
                    .Remove(leftEntities[3].ThreeSkipPayloadFullShared.Single(e => e.Name == "EntityThree 17"));
                leftEntities[3].ThreeSkipPayloadFullShared
                    .Add(
                        context.UnidirectionalEntityThrees.CreateInstance(
                            (e, p) =>
                            {
                                e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                                e.Name = "Z7724";
                            }));

                var rightNav2 = (ICollection<UnidirectionalEntityOne>)context.Entry(rightEntities[2])
                    .Collection("UnidirectionalEntityOne1").CurrentValue!;
                rightNav2.Remove(rightNav2.Single(e => e.Name == "EntityOne 12"));
                rightNav2.Add(
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        }));

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                var joinSet = context.Set<Dictionary<string, object>>("UnidirectionalJoinOneToThreePayloadFullShared");
                (await joinSet.FindAsync(GetEntityOneId(context, "Z7712"), GetEntityThreeId(context, "EntityThree 1")))!["Payload"] =
                    "Set!";
                (await joinSet.FindAsync(
                    GetEntityOneId(context, "EntityOne 20"), GetEntityThreeId(context, "EntityThree 16")))!["Payload"] = "Changed!";

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48, postSave: false);

                await context.SaveChangesAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48 - 4, postSave: true);
            },
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityOne>().Include(e => e.ThreeSkipPayloadFullShared)
                    .OrderBy(e => e.Name)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityThree>().Include("UnidirectionalEntityOne1").OrderBy(e => e.Name)
                    .ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 48 - 4, postSave: true);
            });

        static int GetEntityOneId(ManyToManyContext context, string name)
            => context.Entry(context.UnidirectionalEntityOnes.Local.Single(e => e.Name == name)).Property(e => e.Id).CurrentValue;

        static int GetEntityThreeId(ManyToManyContext context, string name)
            => context.Entry(context.UnidirectionalEntityThrees.Local.Single(e => e.Name == name)).Property(e => e.Id).CurrentValue;

        void ValidateFixup(
            ManyToManyContext context,
            List<UnidirectionalEntityOne> leftEntities,
            List<UnidirectionalEntityThree> rightEntities,
            int leftCount,
            int rightCount,
            int joinCount,
            bool postSave)
        {
            Assert.Equal(leftCount, context.ChangeTracker.Entries<UnidirectionalEntityOne>().Count());
            Assert.Equal(rightCount, context.ChangeTracker.Entries<UnidirectionalEntityThree>().Count());
            Assert.Equal(joinCount, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());
            Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

            Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Name == "Z7721");
            Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Name == "Z7722");
            Assert.Contains(leftEntities[0].ThreeSkipPayloadFullShared, e => e.Name == "Z7723");

            var rightNav0 = context.Entry(rightEntities[0]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne1").CurrentValue!;
            Assert.Contains(rightNav0, e => e.Name == "Z7711");
            Assert.Contains(rightNav0, e => e.Name == "Z7712");
            Assert.Contains(rightNav0, e => e.Name == "Z7713");

            Assert.DoesNotContain(leftEntities[2].ThreeSkipPayloadFullShared, e => e.Name == "EntityThree 10");
            var rightNav4 = context.Entry(rightEntities[4]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne1").CurrentValue!;
            Assert.DoesNotContain(rightNav4, e => e.Name == "EntityOne 6");

            Assert.DoesNotContain(leftEntities[3].ThreeSkipPayloadFullShared, e => e.Name == "EntityThree 17");
            Assert.Contains(leftEntities[3].ThreeSkipPayloadFullShared, e => e.Name == "Z7724");

            var rightNav2 = context.Entry(rightEntities[2]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne1").CurrentValue!;
            Assert.DoesNotContain(rightNav2, e => e.Name == "EntityOne 12");
            Assert.Contains(rightNav2, e => e.Name == "Z7714");

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

            var allLeft = context.ChangeTracker.Entries<UnidirectionalEntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<UnidirectionalEntityThree>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, joinEntries.Select(e => e.Entity));
            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

            var count = 0;
            foreach (var left in allLeft)
            {
                foreach (var right in allRight)
                {
                    var rightNav = context.Entry(right).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne1").CurrentValue;
                    if (left.ThreeSkipPayloadFullShared?.Contains(right) == true)
                    {
                        Assert.Contains(left, rightNav!);
                        count++;
                    }

                    if (rightNav?.Contains(left) == true)
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
    public virtual async Task Can_insert_many_to_many_shared_unidirectional(bool async)
    {
        List<int> keys = null;

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                leftEntities[0].TwoSkipShared = CreateCollection<UnidirectionalEntityTwo>();
                leftEntities[1].TwoSkipShared = CreateCollection<UnidirectionalEntityTwo>();
                leftEntities[2].TwoSkipShared = CreateCollection<UnidirectionalEntityTwo>();

                leftEntities[0].TwoSkipShared.Add(rightEntities[0]); // 11 - 21
                leftEntities[1].TwoSkipShared.Add(rightEntities[0]); // 12 - 21
                leftEntities[2].TwoSkipShared.Add(rightEntities[0]); // 13 - 21
                leftEntities[0].TwoSkipShared.Add(rightEntities[1]); // 11 - 22
                leftEntities[0].TwoSkipShared.Add(rightEntities[2]); // 11 - 23

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
                    await context.SaveChangesAsync();
                }

                ValidateFixup(context, leftEntities, rightEntities);

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<UnidirectionalEntityOne>().Where(e => keys.Contains(e.Id)).Include(e => e.TwoSkipShared);
                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<UnidirectionalEntityOne>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();
                var rightEntities = context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<UnidirectionalEntityOne> leftEntities, IList<UnidirectionalEntityTwo> rightEntities)
        {
            Assert.Equal(11, context.ChangeTracker.Entries().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityOne>().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());

            Assert.Equal(3, leftEntities[0].TwoSkipShared.Count);
            Assert.Single(leftEntities[1].TwoSkipShared);
            Assert.Single(leftEntities[2].TwoSkipShared);

            Assert.Equal(3, context.Entry(rightEntities[0]).Collection("UnidirectionalEntityOne").CurrentValue!.Cast<object>().Count());
            Assert.Single(context.Entry(rightEntities[1]).Collection("UnidirectionalEntityOne").CurrentValue!.Cast<object>());
            Assert.Single(context.Entry(rightEntities[2]).Collection("UnidirectionalEntityOne").CurrentValue!.Cast<object>());

            VerifyRelationshipSnapshots(context, leftEntities);
            VerifyRelationshipSnapshots(context, rightEntities);
        }
    }

    [ConditionalFact]
    public virtual Task Can_update_many_to_many_shared_unidirectional()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityOne>().Include(e => e.TwoSkipShared).OrderBy(e => e.Name)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityTwo>().Include("UnidirectionalEntityOne").OrderBy(e => e.Name)
                    .ToListAsync();

                var twos = new[]
                {
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7724;
                            e.Name = "Z7724";
                        }),
                };

                var ones = new[]
                {
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }),
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }),
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        }),
                    context.UnidirectionalEntityOnes.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7714;
                            e.Name = "Z7714";
                        }),
                };

                leftEntities[0].TwoSkipShared.Add(twos[0]);
                leftEntities[0].TwoSkipShared.Add(twos[1]);
                leftEntities[0].TwoSkipShared.Add(twos[2]);

                var oneSkipNav0 = (ICollection<UnidirectionalEntityOne>)context.Entry(rightEntities[0])
                    .Collection("UnidirectionalEntityOne").CurrentValue!;
                oneSkipNav0.Add(ones[0]);
                oneSkipNav0.Add(ones[1]);
                oneSkipNav0.Add(ones[2]);

                leftEntities[1].TwoSkipShared.Remove(leftEntities[1].TwoSkipShared.Single(e => e.Name == "EntityTwo 17"));
                var oneSkipNav1 = (ICollection<UnidirectionalEntityOne>)context.Entry(rightEntities[1])
                    .Collection("UnidirectionalEntityOne").CurrentValue!;
                oneSkipNav1.Remove(oneSkipNav1.Single(e => e.Name == "EntityOne 3"));

                leftEntities[2].TwoSkipShared.Remove(leftEntities[2].TwoSkipShared.Single(e => e.Name == "EntityTwo 18"));
                leftEntities[2].TwoSkipShared.Add(twos[3]);

                var oneSkipNav2 = (ICollection<UnidirectionalEntityOne>)context.Entry(rightEntities[2])
                    .Collection("UnidirectionalEntityOne").CurrentValue!;
                oneSkipNav2.Remove(oneSkipNav2.Single(e => e.Name == "EntityOne 9"));
                oneSkipNav2.Add(ones[3]);

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 53);

                await context.SaveChangesAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 49);
            },
            async context =>
            {
                var leftEntities = await context.Set<UnidirectionalEntityOne>().Include(e => e.TwoSkipShared).OrderBy(e => e.Name)
                    .ToListAsync();
                var rightEntities = await context.Set<UnidirectionalEntityTwo>().Include("UnidirectionalEntityOne").OrderBy(e => e.Name)
                    .ToListAsync();

                ValidateFixup(context, leftEntities, rightEntities, 24, 24, 49);
            });

        void ValidateFixup(
            DbContext context,
            List<UnidirectionalEntityOne> leftEntities,
            List<UnidirectionalEntityTwo> rightEntities,
            int leftCount,
            int rightCount,
            int joinCount)
        {
            Assert.Equal(leftCount, context.ChangeTracker.Entries<UnidirectionalEntityOne>().Count());
            Assert.Equal(rightCount, context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Count());
            Assert.Equal(joinCount, context.ChangeTracker.Entries<Dictionary<string, object>>().Count());
            Assert.Equal(leftCount + rightCount + joinCount, context.ChangeTracker.Entries().Count());

            Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Name == "Z7721");
            Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Name == "Z7722");
            Assert.Contains(leftEntities[0].TwoSkipShared, e => e.Name == "Z7723");

            var oneSkipNav0 = context.Entry(rightEntities[0]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!;
            Assert.Contains(oneSkipNav0, e => e.Name == "Z7711");
            Assert.Contains(oneSkipNav0, e => e.Name == "Z7712");
            Assert.Contains(oneSkipNav0, e => e.Name == "Z7713");

            Assert.DoesNotContain(leftEntities[1].TwoSkipShared, e => e.Name == "EntityTwo 17");

            var oneSkipNav1 = context.Entry(rightEntities[1]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!;
            Assert.DoesNotContain(oneSkipNav1, e => e.Name == "EntityOne 3");

            Assert.DoesNotContain(leftEntities[2].TwoSkipShared, e => e.Name == "EntityTwo 18");
            Assert.Contains(leftEntities[2].TwoSkipShared, e => e.Name == "Z7724");

            var oneSkipNav2 = context.Entry(rightEntities[2]).Collection<UnidirectionalEntityOne>("UnidirectionalEntityOne").CurrentValue!;
            Assert.DoesNotContain(oneSkipNav2, e => e.Name == "EntityOne 9");
            Assert.Contains(oneSkipNav2, e => e.Name == "Z7714");

            var allLeft = context.ChangeTracker.Entries<UnidirectionalEntityOne>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();
            var allRight = context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name).ToList();

            VerifyRelationshipSnapshots(context, allLeft);
            VerifyRelationshipSnapshots(context, allRight);

            var count = 0;
            foreach (var left in allLeft)
            {
                foreach (var right in allRight)
                {
                    var oneSkipNav = context.Entry(right).Collection("UnidirectionalEntityOne").CurrentValue?.Cast<object>();
                    if (left.TwoSkipShared?.Contains(right) == true)
                    {
                        Assert.Contains(left, oneSkipNav!);
                        count++;
                    }

                    if (oneSkipNav?.Contains(left) == true)
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
    [InlineData(false, false, false)]
    [InlineData(true, false, false)]
    [InlineData(false, true, false)]
    [InlineData(true, true, false)]
    [InlineData(false, false, true)]
    [InlineData(true, false, true)]
    [InlineData(false, true, true)]
    [InlineData(true, true, true)]
    public virtual async Task Can_insert_many_to_many_with_suspected_dangling_join_unidirectional(
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
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                leftEntities[0].TwoSkip = CreateCollection<UnidirectionalEntityTwo>();
                leftEntities[1].TwoSkip = CreateCollection<UnidirectionalEntityTwo>();
                leftEntities[2].TwoSkip = CreateCollection<UnidirectionalEntityTwo>();

                if (!useDetectChanges)
                {
                    leftEntities[0].TwoSkip.Add(rightEntities[0]); // 11 - 21
                    leftEntities[1].TwoSkip.Add(rightEntities[0]); // 12 - 21
                    leftEntities[2].TwoSkip.Add(rightEntities[0]); // 13 - 21
                }

                var joinEntities = new[]
                {
                    context.Set<UnidirectionalJoinOneToTwo>().CreateInstance(
                        (e, p) =>
                        {
                            e.One = leftEntities[0];
                            e.Two = rightEntities[0];
                        }),
                    context.Set<UnidirectionalJoinOneToTwo>().CreateInstance(
                        (e, p) =>
                        {
                            e.One = leftEntities[0];
                            e.Two = rightEntities[1];
                        }),
                    context.Set<UnidirectionalJoinOneToTwo>().CreateInstance(
                        (e, p) =>
                        {
                            e.One = leftEntities[0];
                            e.Two = rightEntities[2];
                        }),
                    context.Set<UnidirectionalJoinOneToTwo>().CreateInstance(
                        (e, p) =>
                        {
                            e.One = leftEntities[1];
                            e.Two = rightEntities[0];
                        }),
                    context.Set<UnidirectionalJoinOneToTwo>().CreateInstance(
                        (e, p) =>
                        {
                            e.One = leftEntities[2];
                            e.Two = rightEntities[0];
                        }),
                };

                var extra = context.Set<UnidirectionalJoinOneToTwoExtra>().CreateInstance(
                    (e, p) =>
                    {
                        e.JoinEntities = new ObservableCollection<UnidirectionalJoinOneToTwo>
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
                    await context.SaveChangesAsync();
                }

                ValidateFixup(context, leftEntities, rightEntities);

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<UnidirectionalEntityOne>()
                    .Where(e => keys.Contains(e.Id))
                    .Include(e => e.TwoSkip)
                    .ThenInclude(e => e.Extra);

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<UnidirectionalEntityOne>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();
                var rightEntities = context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<UnidirectionalEntityOne> leftEntities, IList<UnidirectionalEntityTwo> rightEntities)
        {
            Assert.Equal(12, context.ChangeTracker.Entries().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityOne>().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<UnidirectionalJoinOneToTwo>().Count());
            Assert.Single(context.ChangeTracker.Entries<UnidirectionalJoinOneToTwoExtra>());

            Assert.Equal(3, leftEntities[0].TwoSkip.Count);
            Assert.Single(leftEntities[1].TwoSkip);
            Assert.Single(leftEntities[2].TwoSkip);

            var nav = context.Entry(rightEntities[0]).Collection("UnidirectionalEntityOne1").CurrentValue;

            Assert.Equal(3, context.Entry(rightEntities[0]).Collection("UnidirectionalEntityOne1").CurrentValue!.Cast<object>().Count());
            Assert.Single(context.Entry(rightEntities[1]).Collection("UnidirectionalEntityOne1").CurrentValue!);
            Assert.Single(context.Entry(rightEntities[2]).Collection("UnidirectionalEntityOne1").CurrentValue!);

            var extra = context.ChangeTracker.Entries<UnidirectionalJoinOneToTwoExtra>().Select(e => e.Entity).Single();
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
    public virtual async Task Can_insert_many_to_many_with_dangling_join_unidirectional(
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
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7711),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7712),
                    context.UnidirectionalEntityOnes.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7713)
                };
                var rightEntities = new[]
                {
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7721),
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7722),
                    context.UnidirectionalEntityTwos.CreateInstance((e, p) => e.Id = Fixture.UseGeneratedKeys ? 0 : 7723)
                };

                leftEntities[0].TwoSkip = CreateCollection<UnidirectionalEntityTwo>();
                leftEntities[1].TwoSkip = CreateCollection<UnidirectionalEntityTwo>();
                leftEntities[2].TwoSkip = CreateCollection<UnidirectionalEntityTwo>();

                if (!useDetectChanges)
                {
                    leftEntities[0].TwoSkip.Add(rightEntities[0]); // 11 - 21
                    leftEntities[0].TwoSkip.Add(rightEntities[1]); // 11 - 22
                    leftEntities[0].TwoSkip.Add(rightEntities[2]); // 11 - 23
                }

                leftEntities[1].TwoSkip.Add(rightEntities[0]); // 12 - 21
                leftEntities[2].TwoSkip.Add(rightEntities[0]); // 13 - 21

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
                    await context.SaveChangesAsync();
                }

                ValidateFixup(context, leftEntities, rightEntities);

                keys = leftEntities.Select(e => e.Id).ToList();
            },
            async context =>
            {
                var queryable = context.Set<UnidirectionalEntityOne>()
                    .Where(e => keys.Contains(e.Id))
                    .Include(e => e.TwoSkip)
                    .ThenInclude(e => e.Extra);

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<UnidirectionalEntityOne>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();
                var rightEntities = context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        void ValidateFixup(DbContext context, IList<UnidirectionalEntityOne> leftEntities, IList<UnidirectionalEntityTwo> rightEntities)
        {
            Assert.Equal(11, context.ChangeTracker.Entries().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityOne>().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<UnidirectionalJoinOneToTwo>().Count());

            Assert.Equal(3, leftEntities[0].TwoSkip.Count);
            Assert.Single(leftEntities[1].TwoSkip);
            Assert.Single(leftEntities[2].TwoSkip);

            Assert.Equal(
                3,
                ((IEnumerable<object>)context.Entry(rightEntities[0]).Collection("UnidirectionalEntityOne1").CurrentValue!).Count());
            Assert.Single((IEnumerable<object>)context.Entry(rightEntities[1]).Collection("UnidirectionalEntityOne1").CurrentValue!);
            Assert.Single((IEnumerable<object>)context.Entry(rightEntities[2]).Collection("UnidirectionalEntityOne1").CurrentValue!);

            var joinEntities = context.ChangeTracker.Entries<UnidirectionalJoinOneToTwo>().Select(e => e.Entity).ToList();
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
    public virtual Task Can_insert_update_delete_proxyable_shared_type_entity_type_unidirectional()
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
            },
            async context =>
            {
                var entity = await context.Set<ProxyableSharedType>("PST").SingleAsync(e => (int)e["Id"] == id);

                Assert.Equal("NewlyAdded", (string)entity["Payload"]);

                entity["Payload"] = "AlreadyUpdated";

                if (RequiresDetectChanges)
                {
                    context.ChangeTracker.DetectChanges();
                }

                await context.SaveChangesAsync();
            },
            async context =>
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
    public virtual async Task Can_insert_many_to_many_with_navs_by_join_entity_unidirectional(bool async)
    {
        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var leftEntities = new[]
                {
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7711;
                            e.Name = "Z7711";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7712;
                            e.Name = "Z7712";
                        }),
                    context.UnidirectionalEntityTwos.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7713;
                            e.Name = "Z7713";
                        })
                };
                var rightEntities = new[]
                {
                    context.UnidirectionalEntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7721;
                            e.Name = "Z7721";
                        }),
                    context.UnidirectionalEntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7722;
                            e.Name = "Z7722";
                        }),
                    context.UnidirectionalEntityThrees.CreateInstance(
                        (e, p) =>
                        {
                            e.Id = Fixture.UseGeneratedKeys ? 0 : 7723;
                            e.Name = "Z7723";
                        })
                };

                var joinEntities = new[]
                {
                    context.Set<UnidirectionalJoinTwoToThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Two = leftEntities[0];
                            e.Three = rightEntities[0];
                        }),
                    context.Set<UnidirectionalJoinTwoToThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Two = leftEntities[0];
                            e.Three = rightEntities[1];
                        }),
                    context.Set<UnidirectionalJoinTwoToThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Two = leftEntities[0];
                            e.Three = rightEntities[2];
                        }),
                    context.Set<UnidirectionalJoinTwoToThree>().CreateInstance(
                        (e, p) =>
                        {
                            e.Two = leftEntities[1];
                            e.Three = rightEntities[0];
                        }),
                    context.Set<UnidirectionalJoinTwoToThree>().CreateInstance(
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
                    await context.SaveChangesAsync();
                }

                ValidateFixup(context, leftEntities, rightEntities);
            },
            async context =>
            {
                var queryable = context.Set<UnidirectionalEntityTwo>()
                    .Where(e => e.Name.StartsWith("Z"))
                    .OrderBy(e => e.Name)
                    .Include("UnidirectionalEntityThree");

                var results = async ? await queryable.ToListAsync() : queryable.ToList();
                Assert.Equal(3, results.Count);

                var leftEntities = context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();
                var rightEntities = context.ChangeTracker.Entries<UnidirectionalEntityThree>().Select(e => e.Entity).OrderBy(e => e.Name)
                    .ToList();

                ValidateFixup(context, leftEntities, rightEntities);
            });

        static void ValidateFixup(
            DbContext context,
            IList<UnidirectionalEntityTwo> leftEntities,
            IList<UnidirectionalEntityThree> rightEntities)
        {
            Assert.Equal(11, context.ChangeTracker.Entries().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityTwo>().Count());
            Assert.Equal(3, context.ChangeTracker.Entries<UnidirectionalEntityThree>().Count());
            Assert.Equal(5, context.ChangeTracker.Entries<UnidirectionalJoinTwoToThree>().Count());

            Assert.Equal(
                3, context.Entry(leftEntities[0]).Collection<UnidirectionalEntityThree>("UnidirectionalEntityThree").CurrentValue!.Count());
            Assert.Single(context.Entry(leftEntities[1]).Collection<UnidirectionalEntityThree>("UnidirectionalEntityThree").CurrentValue!);
            Assert.Single(context.Entry(leftEntities[2]).Collection<UnidirectionalEntityThree>("UnidirectionalEntityThree").CurrentValue!);

            Assert.Equal(3, rightEntities[0].TwoSkipFull.Count);
            Assert.Single(rightEntities[1].TwoSkipFull);
            Assert.Single(rightEntities[2].TwoSkipFull);

            foreach (var joinEntity in context.ChangeTracker.Entries<UnidirectionalJoinTwoToThree>().Select(e => e.Entity).ToList())
            {
                Assert.Equal(joinEntity.Two.Id, joinEntity.TwoId);
                Assert.Equal(joinEntity.Three.Id, joinEntity.ThreeId);
                Assert.Contains(joinEntity, joinEntity.Two.JoinThreeFull);
                Assert.Contains(joinEntity, joinEntity.Three.JoinTwoFull);
            }
        }
    }
}

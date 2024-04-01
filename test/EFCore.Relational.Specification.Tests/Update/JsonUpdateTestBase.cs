// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Update;

#nullable disable

public abstract class JsonUpdateTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : JsonUpdateFixtureBase, new()
{
    public TFixture Fixture { get; }

    protected JsonUpdateTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    public JsonQueryContext CreateContext()
        => Fixture.CreateContext();

    [ConditionalFact]
    public virtual Task Add_entity_with_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var newEntity = new JsonEntityBasic
                {
                    Id = 2,
                    Name = "NewEntity",
                    OwnedCollectionRoot = [],
                    OwnedReferenceRoot = new JsonOwnedRoot
                    {
                        Name = "RootName",
                        Number = 42,
                        OwnedCollectionBranch = [],
                        OwnedReferenceBranch = new JsonOwnedBranch
                        {
                            Date = new DateTime(2010, 10, 10),
                            Enum = JsonEnum.Three,
                            Fraction = 42.42m,
                            OwnedCollectionLeaf =
                            [
                                new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" }
                            ],
                            OwnedReferenceLeaf = new JsonOwnedLeaf { SomethingSomething = "ss3" }
                        }
                    },
                };

                context.Set<JsonEntityBasic>().Add(newEntity);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                Assert.Equal(2, query.Count);

                var newEntity = query.Where(e => e.Id == 2).Single();
                Assert.Equal("NewEntity", newEntity.Name);
                Assert.Empty(newEntity.OwnedCollectionRoot);
                Assert.Equal("RootName", newEntity.OwnedReferenceRoot.Name);
                Assert.Equal(42, newEntity.OwnedReferenceRoot.Number);
                Assert.Empty(newEntity.OwnedReferenceRoot.OwnedCollectionBranch);
                Assert.Equal(new DateTime(2010, 10, 10), newEntity.OwnedReferenceRoot.OwnedReferenceBranch.Date);
                Assert.Equal(JsonEnum.Three, newEntity.OwnedReferenceRoot.OwnedReferenceBranch.Enum);
                Assert.Equal(42.42m, newEntity.OwnedReferenceRoot.OwnedReferenceBranch.Fraction);

                Assert.Equal(42.42m, newEntity.OwnedReferenceRoot.OwnedReferenceBranch.Fraction);
                Assert.Equal("ss3", newEntity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething);

                var collectionLeaf = newEntity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf;
                Assert.Equal(2, collectionLeaf.Count);
                Assert.Equal("ss1", collectionLeaf[0].SomethingSomething);
                Assert.Equal("ss2", collectionLeaf[1].SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Add_entity_with_json_null_navigations()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var newEntity = new JsonEntityBasic
                {
                    Id = 2,
                    Name = "NewEntity",
                    OwnedCollectionRoot = null,
                    OwnedReferenceRoot = new JsonOwnedRoot
                    {
                        Name = "RootName",
                        Number = 42,
                        //OwnedCollectionBranch missing on purpose
                        OwnedReferenceBranch = new JsonOwnedBranch
                        {
                            Date = new DateTime(2010, 10, 10),
                            Enum = JsonEnum.Three,
                            Fraction = 42.42m,
                            OwnedCollectionLeaf =
                            [
                                new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" }
                            ],
                            OwnedReferenceLeaf = null,
                        }
                    },
                };

                context.Set<JsonEntityBasic>().Add(newEntity);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                Assert.Equal(2, query.Count);

                var newEntity = query.Where(e => e.Id == 2).Single();
                Assert.Equal("NewEntity", newEntity.Name);
                Assert.Null(newEntity.OwnedCollectionRoot);
                Assert.Equal("RootName", newEntity.OwnedReferenceRoot.Name);
                Assert.Equal(42, newEntity.OwnedReferenceRoot.Number);
                Assert.Null(newEntity.OwnedReferenceRoot.OwnedCollectionBranch);
                Assert.Equal(new DateTime(2010, 10, 10), newEntity.OwnedReferenceRoot.OwnedReferenceBranch.Date);
                Assert.Equal(JsonEnum.Three, newEntity.OwnedReferenceRoot.OwnedReferenceBranch.Enum);
                Assert.Equal(42.42m, newEntity.OwnedReferenceRoot.OwnedReferenceBranch.Fraction);

                Assert.Equal(42.42m, newEntity.OwnedReferenceRoot.OwnedReferenceBranch.Fraction);
                Assert.Null(newEntity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf);

                var collectionLeaf = newEntity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf;
                Assert.Equal(2, collectionLeaf.Count);
                Assert.Equal("ss1", collectionLeaf[0].SomethingSomething);
                Assert.Equal("ss2", collectionLeaf[1].SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Add_json_reference_root()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();

                Assert.Null(entity.OwnedReferenceRoot);
                entity.OwnedReferenceRoot = new JsonOwnedRoot
                {
                    Name = "RootName",
                    Number = 42,
                    OwnedCollectionBranch = [],
                    OwnedReferenceBranch = new JsonOwnedBranch
                    {
                        Date = new DateTime(2010, 10, 10),
                        Enum = JsonEnum.Three,
                        Fraction = 42.42m,
                        OwnedCollectionLeaf =
                        [
                            new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" }
                        ],
                        OwnedReferenceLeaf = new JsonOwnedLeaf { SomethingSomething = "ss3" }
                    }
                };
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var updatedEntity = await context.JsonEntitiesBasic.SingleAsync();
                var updatedReference = updatedEntity.OwnedReferenceRoot;
                Assert.Equal("RootName", updatedReference.Name);
                Assert.Equal(42, updatedReference.Number);
                Assert.Empty(updatedReference.OwnedCollectionBranch);
                Assert.Equal(new DateTime(2010, 10, 10), updatedReference.OwnedReferenceBranch.Date);
                Assert.Equal(JsonEnum.Three, updatedReference.OwnedReferenceBranch.Enum);
                Assert.Equal(42.42m, updatedReference.OwnedReferenceBranch.Fraction);
                Assert.Equal("ss3", updatedReference.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething);
                var collectionLeaf = updatedReference.OwnedReferenceBranch.OwnedCollectionLeaf;
                Assert.Equal(2, collectionLeaf.Count);
                Assert.Equal("ss1", collectionLeaf[0].SomethingSomething);
                Assert.Equal("ss2", collectionLeaf[1].SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Add_json_reference_leaf()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedReferenceLeaf = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();

                Assert.Null(entity.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedReferenceLeaf);
                var newLeaf = new JsonOwnedLeaf { SomethingSomething = "ss3" };
                entity.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedReferenceLeaf = newLeaf;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var updatedEntity = await context.JsonEntitiesBasic.SingleAsync();
                var updatedReference = updatedEntity.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedReferenceLeaf;
                Assert.Equal("ss3", updatedReference.SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Add_element_to_json_collection_root()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();

                var newRoot = new JsonOwnedRoot
                {
                    Name = "new Name",
                    Number = 142,
                    OwnedCollectionBranch = [],
                    OwnedReferenceBranch = new JsonOwnedBranch
                    {
                        Date = new DateTime(2010, 10, 10),
                        Enum = JsonEnum.Three,
                        Fraction = 42.42m,
                        OwnedCollectionLeaf =
                        [
                            new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" }
                        ],
                        OwnedReferenceLeaf = new JsonOwnedLeaf { SomethingSomething = "ss3" }
                    }
                };

                entity.OwnedCollectionRoot.Add(newRoot);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var updatedEntity = await context.JsonEntitiesBasic.SingleAsync();
                var updatedCollection = updatedEntity.OwnedCollectionRoot;
                Assert.Equal(3, updatedCollection.Count);
                Assert.Equal("new Name", updatedCollection[2].Name);
                Assert.Equal(142, updatedCollection[2].Number);
                Assert.Empty(updatedCollection[2].OwnedCollectionBranch);
                Assert.Equal(new DateTime(2010, 10, 10), updatedCollection[2].OwnedReferenceBranch.Date);
                Assert.Equal(JsonEnum.Three, updatedCollection[2].OwnedReferenceBranch.Enum);
                Assert.Equal(42.42m, updatedCollection[2].OwnedReferenceBranch.Fraction);
                Assert.Equal("ss3", updatedCollection[2].OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething);
                var collectionLeaf = updatedCollection[2].OwnedReferenceBranch.OwnedCollectionLeaf;
                Assert.Equal(2, collectionLeaf.Count);
                Assert.Equal("ss1", collectionLeaf[0].SomethingSomething);
                Assert.Equal("ss2", collectionLeaf[1].SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Add_element_to_json_collection_root_null_navigations()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();

                var newRoot = new JsonOwnedRoot
                {
                    Name = "new Name",
                    Number = 142,
                    OwnedCollectionBranch = null,
                    OwnedReferenceBranch = new JsonOwnedBranch
                    {
                        Date = new DateTime(2010, 10, 10),
                        Enum = JsonEnum.Three,
                        Fraction = 42.42m,
                        OwnedReferenceLeaf = null
                    }
                };

                entity.OwnedCollectionRoot.Add(newRoot);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var updatedEntity = await context.JsonEntitiesBasic.SingleAsync();
                var updatedCollection = updatedEntity.OwnedCollectionRoot;
                Assert.Equal(3, updatedCollection.Count);
                Assert.Equal("new Name", updatedCollection[2].Name);
                Assert.Equal(142, updatedCollection[2].Number);
                Assert.Null(updatedCollection[2].OwnedCollectionBranch);
                Assert.Equal(new DateTime(2010, 10, 10), updatedCollection[2].OwnedReferenceBranch.Date);
                Assert.Equal(JsonEnum.Three, updatedCollection[2].OwnedReferenceBranch.Enum);
                Assert.Equal(42.42m, updatedCollection[2].OwnedReferenceBranch.Fraction);
                Assert.Null(updatedCollection[2].OwnedReferenceBranch.OwnedReferenceLeaf);
                Assert.Null(updatedCollection[2].OwnedReferenceBranch.OwnedCollectionLeaf);
            });

    [ConditionalFact]
    public virtual Task Add_element_to_json_collection_branch()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                var newBranch = new JsonOwnedBranch
                {
                    Date = new DateTime(2010, 10, 10),
                    Enum = JsonEnum.Three,
                    Fraction = 42.42m,
                    OwnedCollectionLeaf =
                    [
                        new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" }
                    ],
                    OwnedReferenceLeaf = new JsonOwnedLeaf { SomethingSomething = "ss3" }
                };

                entity.OwnedReferenceRoot.OwnedCollectionBranch.Add(newBranch);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var updatedEntity = await context.JsonEntitiesBasic.SingleAsync();
                var updatedCollection = updatedEntity.OwnedReferenceRoot.OwnedCollectionBranch;
                Assert.Equal(3, updatedCollection.Count);
                Assert.Equal(new DateTime(2010, 10, 10), updatedCollection[2].Date);
                Assert.Equal(JsonEnum.Three, updatedCollection[2].Enum);
                Assert.Equal(42.42m, updatedCollection[2].Fraction);
                Assert.Equal("ss3", updatedCollection[2].OwnedReferenceLeaf.SomethingSomething);
                var collectionLeaf = updatedCollection[2].OwnedCollectionLeaf;
                Assert.Equal(2, collectionLeaf.Count);
                Assert.Equal("ss1", collectionLeaf[0].SomethingSomething);
                Assert.Equal("ss2", collectionLeaf[1].SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Add_element_to_json_collection_leaf()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                var newLeaf = new JsonOwnedLeaf { SomethingSomething = "ss1" };
                entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf.Add(newLeaf);
                ClearLog();
                await context.SaveChangesAsync();

                // Do SaveChanges again, see #28813
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var updatedEntity = await context.JsonEntitiesBasic.SingleAsync();
                var updatedCollection = updatedEntity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf;
                Assert.Equal(3, updatedCollection.Count);
                Assert.Equal("ss1", updatedCollection[2].SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Delete_entity_with_json()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();

                context.Set<JsonEntityBasic>().Remove(entity);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().CountAsync();

                Assert.Equal(0, result);
            });

    [ConditionalFact]
    public virtual Task Delete_json_reference_root()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot = null;
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var updatedEntity = await context.JsonEntitiesBasic.SingleAsync();
                Assert.Null(updatedEntity.OwnedReferenceRoot);
            });

    [ConditionalFact]
    public virtual Task Delete_json_reference_leaf()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf = null;
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var updatedEntity = await context.JsonEntitiesBasic.SingleAsync();
                Assert.Null(updatedEntity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf);
            });

    [ConditionalFact]
    public virtual Task Delete_json_collection_root()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedCollectionRoot = null;
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Null(result.OwnedCollectionRoot);
            });

    [ConditionalFact]
    public virtual Task Delete_json_collection_branch()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedCollectionBranch = null;
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Null(result.OwnedReferenceRoot.OwnedCollectionBranch);
            });

    [ConditionalFact]
    public virtual Task Edit_element_in_json_collection_root1()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedCollectionRoot[0].Name = "Modified";
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                var resultCollection = result.OwnedCollectionRoot;
                Assert.Equal(2, resultCollection.Count);
                Assert.Equal("Modified", resultCollection[0].Name);
            });

    [ConditionalFact]
    public virtual Task Edit_element_in_json_collection_root2()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedCollectionRoot[1].Name = "Modified";
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                var resultCollection = result.OwnedCollectionRoot;
                Assert.Equal(2, resultCollection.Count);
                Assert.Equal("Modified", resultCollection[1].Name);
            });

    [ConditionalFact]
    public virtual Task Edit_element_in_json_collection_branch()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedCollectionRoot[0].OwnedCollectionBranch[0].Date = new DateTime(2111, 11, 11);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal(new DateTime(2111, 11, 11), result.OwnedCollectionRoot[0].OwnedCollectionBranch[0].Date);
            });

    [ConditionalFact]
    public virtual Task Add_element_to_json_collection_on_derived()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesInheritance.OfType<JsonEntityInheritanceDerived>().ToListAsync();
                var entity = query.Single();

                var newBranch = new JsonOwnedBranch
                {
                    Date = new DateTime(2010, 10, 10),
                    Enum = JsonEnum.Three,
                    Fraction = 42.42m,
                    OwnedCollectionLeaf =
                    [
                        new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" }
                    ],
                    OwnedReferenceLeaf = new JsonOwnedLeaf { SomethingSomething = "ss3" }
                };

                entity.CollectionOnDerived.Add(newBranch);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.JsonEntitiesInheritance.OfType<JsonEntityInheritanceDerived>().SingleAsync();
                var updatedCollection = result.CollectionOnDerived;

                Assert.Equal(new DateTime(2010, 10, 10), updatedCollection[2].Date);
                Assert.Equal(JsonEnum.Three, updatedCollection[2].Enum);
                Assert.Equal(42.42m, updatedCollection[2].Fraction);
                Assert.Equal("ss3", updatedCollection[2].OwnedReferenceLeaf.SomethingSomething);
                var collectionLeaf = updatedCollection[2].OwnedCollectionLeaf;
                Assert.Equal(2, collectionLeaf.Count);
                Assert.Equal("ss1", collectionLeaf[0].SomethingSomething);
                Assert.Equal("ss2", collectionLeaf[1].SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Edit_element_in_json_multiple_levels_partial_update()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedReferenceBranch.Date = new DateTime(2111, 11, 11);
                entity.OwnedReferenceRoot.Name = "edit";
                entity.OwnedCollectionRoot[0].OwnedCollectionBranch[1].OwnedCollectionLeaf[0].SomethingSomething = "yet another change";
                entity.OwnedCollectionRoot[0].OwnedCollectionBranch[1].OwnedCollectionLeaf[1].SomethingSomething = "and another";
                entity.OwnedCollectionRoot[0].OwnedCollectionBranch[0].OwnedCollectionLeaf[0].SomethingSomething = "...and another";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal(new DateTime(2111, 11, 11), result.OwnedReferenceRoot.OwnedReferenceBranch.Date);
                Assert.Equal("edit", result.OwnedReferenceRoot.Name);
                Assert.Equal(
                    "yet another change", result.OwnedCollectionRoot[0].OwnedCollectionBranch[1].OwnedCollectionLeaf[0].SomethingSomething);
                Assert.Equal(
                    "and another", result.OwnedCollectionRoot[0].OwnedCollectionBranch[1].OwnedCollectionLeaf[1].SomethingSomething);
                Assert.Equal(
                    "...and another", result.OwnedCollectionRoot[0].OwnedCollectionBranch[0].OwnedCollectionLeaf[0].SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Edit_element_in_json_branch_collection_and_add_element_to_the_same_collection()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedCollectionBranch[0].Fraction = 4321.3m;
                entity.OwnedReferenceRoot.OwnedCollectionBranch.Add(
                    new JsonOwnedBranch
                    {
                        Date = new DateTime(2222, 11, 11),
                        Enum = JsonEnum.Three,
                        Fraction = 45.32m,
                        OwnedReferenceLeaf = new JsonOwnedLeaf { SomethingSomething = "cc" },
                    });

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal(4321.3m, result.OwnedReferenceRoot.OwnedCollectionBranch[0].Fraction);

                Assert.Equal(new DateTime(2222, 11, 11), result.OwnedReferenceRoot.OwnedCollectionBranch[2].Date);
                Assert.Equal(JsonEnum.Three, result.OwnedReferenceRoot.OwnedCollectionBranch[2].Enum);
                Assert.Equal(45.32m, result.OwnedReferenceRoot.OwnedCollectionBranch[2].Fraction);
                Assert.Equal("cc", result.OwnedReferenceRoot.OwnedCollectionBranch[2].OwnedReferenceLeaf.SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Edit_two_elements_in_the_same_json_collection()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[0].SomethingSomething = "edit1";
                entity.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[1].SomethingSomething = "edit2";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal("edit1", result.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[0].SomethingSomething);
                Assert.Equal("edit2", result.OwnedReferenceRoot.OwnedCollectionBranch[0].OwnedCollectionLeaf[1].SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Edit_two_elements_in_the_same_json_collection_at_the_root()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedCollectionRoot[0].Name = "edit1";
                entity.OwnedCollectionRoot[1].Name = "edit2";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal("edit1", result.OwnedCollectionRoot[0].Name);
                Assert.Equal("edit2", result.OwnedCollectionRoot[1].Name);
            });

    [ConditionalFact]
    public virtual Task Edit_collection_element_and_reference_at_once()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedCollectionBranch[1].OwnedCollectionLeaf[0].SomethingSomething = "edit1";
                entity.OwnedReferenceRoot.OwnedCollectionBranch[1].OwnedReferenceLeaf.SomethingSomething = "edit2";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal("edit1", result.OwnedReferenceRoot.OwnedCollectionBranch[1].OwnedCollectionLeaf[0].SomethingSomething);
                Assert.Equal("edit2", result.OwnedReferenceRoot.OwnedCollectionBranch[1].OwnedReferenceLeaf.SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Edit_single_enum_property()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedReferenceBranch.Enum = JsonEnum.Two;
                entity.OwnedCollectionRoot[1].OwnedCollectionBranch[1].Enum = JsonEnum.Two;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal(JsonEnum.Two, result.OwnedReferenceRoot.OwnedReferenceBranch.Enum);
                Assert.Equal(JsonEnum.Two, result.OwnedCollectionRoot[1].OwnedCollectionBranch[1].Enum);
            });

    [ConditionalFact]
    public virtual Task Edit_single_numeric_property()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.Number = 999;
                entity.OwnedCollectionRoot[1].Number = 1024;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal(999, result.OwnedReferenceRoot.Number);
                Assert.Equal(1024, result.OwnedCollectionRoot[1].Number);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_bool()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestBoolean = false;
                entity.Collection[0].TestBoolean = true;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.False(result.Reference.TestBoolean);
                Assert.True(result.Collection[0].TestBoolean);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_byte()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestByte = 25;
                entity.Collection[0].TestByte = 14;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(25, result.Reference.TestByte);
                Assert.Equal(14, result.Collection[0].TestByte);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_char()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestCharacter = 't';
                entity.Collection[0].TestCharacter = 'h';

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal('t', result.Reference.TestCharacter);
                Assert.Equal('h', result.Collection[0].TestCharacter);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_datetime()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestDateTime = DateTime.Parse("01/01/3000 12:34:56");
                entity.Collection[0].TestDateTime = DateTime.Parse("01/01/3000 12:34:56");

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(DateTime.Parse("01/01/3000 12:34:56"), result.Reference.TestDateTime);
                Assert.Equal(DateTime.Parse("01/01/3000 12:34:56"), result.Collection[0].TestDateTime);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_datetimeoffset()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestDateTimeOffset = new DateTimeOffset(DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-4.0));
                entity.Collection[0].TestDateTimeOffset = new DateTimeOffset(
                    DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-4.0));

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new DateTimeOffset(DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-4.0)),
                    result.Reference.TestDateTimeOffset);
                Assert.Equal(
                    new DateTimeOffset(DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-4.0)),
                    result.Collection[0].TestDateTimeOffset);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_decimal()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestDecimal = -13579.01M;
                entity.Collection[0].TestDecimal = -13579.01M;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(-13579.01M, result.Reference.TestDecimal);
                Assert.Equal(-13579.01M, result.Collection[0].TestDecimal);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_double()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestDouble = -1.23579;
                entity.Collection[0].TestDouble = -1.23579;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(-1.23579, result.Reference.TestDouble);
                Assert.Equal(-1.23579, result.Collection[0].TestDouble);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_guid()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestGuid = new Guid("12345678-1234-4321-5555-987654321000");
                entity.Collection[0].TestGuid = new Guid("12345678-1234-4321-5555-987654321000");

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new Guid("12345678-1234-4321-5555-987654321000"), result.Reference.TestGuid);
                Assert.Equal(new Guid("12345678-1234-4321-5555-987654321000"), result.Collection[0].TestGuid);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_int16()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestInt16 = -3234;
                entity.Collection[0].TestInt16 = -3234;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(-3234, result.Reference.TestInt16);
                Assert.Equal(-3234, result.Collection[0].TestInt16);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_int32()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestInt32 = -3234;
                entity.Collection[0].TestInt32 = -3234;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(-3234, result.Reference.TestInt32);
                Assert.Equal(-3234, result.Collection[0].TestInt32);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_int64()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestInt64 = -3234;
                entity.Collection[0].TestInt64 = -3234;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(-3234, result.Reference.TestInt64);
                Assert.Equal(-3234, result.Collection[0].TestInt64);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_signed_byte()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestSignedByte = -108;
                entity.Collection[0].TestSignedByte = -108;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(-108, result.Reference.TestSignedByte);
                Assert.Equal(-108, result.Collection[0].TestSignedByte);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_single()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestSingle = -7.234F;
                entity.Collection[0].TestSingle = -7.234F;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(-7.234F, result.Reference.TestSingle);
                Assert.Equal(-7.234F, result.Collection[0].TestSingle);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_timespan()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestTimeSpan = new TimeSpan(0, 10, 1, 1, 7);
                entity.Collection[0].TestTimeSpan = new TimeSpan(0, 10, 1, 1, 7);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new TimeSpan(0, 10, 1, 1, 7), result.Reference.TestTimeSpan);
                Assert.Equal(new TimeSpan(0, 10, 1, 1, 7), result.Collection[0].TestTimeSpan);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_dateonly()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestDateOnly = new DateOnly(1023, 1, 1);
                entity.Collection[0].TestDateOnly = new DateOnly(2000, 2, 4);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new DateOnly(1023, 1, 1), result.Reference.TestDateOnly);
                Assert.Equal(new DateOnly(2000, 2, 4), result.Collection[0].TestDateOnly);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_timeonly()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestTimeOnly = new TimeOnly(1, 1, 7);
                entity.Collection[0].TestTimeOnly = new TimeOnly(1, 1, 7);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new TimeOnly(1, 1, 7), result.Reference.TestTimeOnly);
                Assert.Equal(new TimeOnly(1, 1, 7), result.Collection[0].TestTimeOnly);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_uint16()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestUnsignedInt16 = 1534;
                entity.Collection[0].TestUnsignedInt16 = 1534;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(1534, result.Reference.TestUnsignedInt16);
                Assert.Equal(1534, result.Collection[0].TestUnsignedInt16);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_uint32()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestUnsignedInt32 = 1237775789U;
                entity.Collection[0].TestUnsignedInt32 = 1237775789U;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(1237775789U, result.Reference.TestUnsignedInt32);
                Assert.Equal(1237775789U, result.Collection[0].TestUnsignedInt32);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_uint64()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestUnsignedInt64 = 1234555555123456789UL;
                entity.Collection[0].TestUnsignedInt64 = 1234555555123456789UL;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(1234555555123456789UL, result.Reference.TestUnsignedInt64);
                Assert.Equal(1234555555123456789UL, result.Collection[0].TestUnsignedInt64);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_nullable_int32()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableInt32 = 64528;
                entity.Collection[0].TestNullableInt32 = 122354;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(64528, result.Reference.TestNullableInt32);
                Assert.Equal(122354, result.Collection[0].TestNullableInt32);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_nullable_int32_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableInt32 = null;
                entity.Collection[0].TestNullableInt32 = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.Reference.TestNullableInt32);
                Assert.Null(result.Collection[0].TestNullableInt32);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_enum()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestEnum = JsonEnum.Three;
                entity.Collection[0].TestEnum = JsonEnum.Three;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(JsonEnum.Three, result.Reference.TestEnum);
                Assert.Equal(JsonEnum.Three, result.Collection[0].TestEnum);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_enum_with_int_converter()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestEnumWithIntConverter = JsonEnum.Three;
                entity.Collection[0].TestEnumWithIntConverter = JsonEnum.Three;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(JsonEnum.Three, result.Reference.TestEnumWithIntConverter);
                Assert.Equal(JsonEnum.Three, result.Collection[0].TestEnumWithIntConverter);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_nullable_enum()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestEnum = JsonEnum.Three;
                entity.Collection[0].TestEnum = JsonEnum.Three;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(JsonEnum.Three, result.Reference.TestEnum);
                Assert.Equal(JsonEnum.Three, result.Collection[0].TestEnum);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_nullable_enum_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableEnum = null;
                entity.Collection[0].TestNullableEnum = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.Reference.TestNullableEnum);
                Assert.Null(result.Collection[0].TestNullableEnum);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_nullable_enum_with_int_converter()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableEnumWithIntConverter = JsonEnum.Three;
                entity.Collection[0].TestNullableEnumWithIntConverter = JsonEnum.One;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(JsonEnum.Three, result.Reference.TestNullableEnumWithIntConverter);
                Assert.Equal(JsonEnum.One, result.Collection[0].TestNullableEnumWithIntConverter);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_nullable_enum_with_int_converter_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableEnumWithIntConverter = null;
                entity.Collection[0].TestNullableEnumWithIntConverter = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.Reference.TestNullableEnumWithIntConverter);
                Assert.Null(result.Collection[0].TestNullableEnumWithIntConverter);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_nullable_enum_with_converter_that_handles_nulls()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableEnumWithConverterThatHandlesNulls = JsonEnum.One;
                entity.Collection[0].TestNullableEnumWithConverterThatHandlesNulls = JsonEnum.Three;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(JsonEnum.One, result.Reference.TestNullableEnumWithConverterThatHandlesNulls);
                Assert.Equal(JsonEnum.Three, result.Collection[0].TestNullableEnumWithConverterThatHandlesNulls);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_nullable_enum_with_converter_that_handles_nulls_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableEnumWithConverterThatHandlesNulls = null;
                entity.Collection[0].TestNullableEnumWithConverterThatHandlesNulls = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.Reference.TestNullableEnumWithConverterThatHandlesNulls);
                Assert.Null(result.Collection[0].TestNullableEnumWithConverterThatHandlesNulls);
            });

    [ConditionalFact]
    public virtual Task Edit_two_properties_on_same_entity_updates_the_entire_entity()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestInt32 = 32;
                entity.Reference.TestInt64 = 64;
                entity.Collection[0].TestInt32 = 32;
                entity.Collection[0].TestInt64 = 64;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(32, result.Reference.TestInt32);
                Assert.Equal(64, result.Reference.TestInt64);
                Assert.Equal(32, result.Collection[0].TestInt32);
                Assert.Equal(64, result.Collection[0].TestInt64);
            });

    [ConditionalFact]
    public virtual Task Edit_a_scalar_property_and_reference_navigation_on_the_same_entity()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedReferenceBranch.Fraction = 123.532M;
                entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedReferenceBranch.Fraction = 523.532M;
                entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf = new JsonOwnedLeaf { SomethingSomething = "edit" };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal(523.532M, result.OwnedReferenceRoot.OwnedReferenceBranch.Fraction);
                Assert.Equal("edit", result.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Edit_a_scalar_property_and_collection_navigation_on_the_same_entity()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedReferenceBranch.Fraction = 123.532M;
                entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedReferenceBranch.Fraction = 523.532M;
                entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf = [new() { SomethingSomething = "edit" }];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal(523.532M, result.OwnedReferenceRoot.OwnedReferenceBranch.Fraction);
                Assert.Equal("edit", result.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf[0].SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Edit_a_scalar_property_and_another_property_behind_reference_navigation_on_the_same_entity()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.OwnedReferenceBranch.Fraction = 523.532M;
                entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething = "edit";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal(523.532M, result.OwnedReferenceRoot.OwnedReferenceBranch.Fraction);
                Assert.Equal("edit", result.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf.SomethingSomething);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_with_converter_bool_to_int_zero_one()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesConverters.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.BoolConvertedToIntZeroOne = false;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityConverters>().SingleAsync(x => x.Id == 1);
                Assert.False(result.Reference.BoolConvertedToIntZeroOne);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_with_converter_bool_to_string_True_False()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesConverters.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.BoolConvertedToStringTrueFalse = true;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityConverters>().SingleAsync(x => x.Id == 1);
                Assert.True(result.Reference.BoolConvertedToStringTrueFalse);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_with_converter_bool_to_string_Y_N()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesConverters.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.BoolConvertedToStringYN = false;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityConverters>().SingleAsync(x => x.Id == 1);
                Assert.False(result.Reference.BoolConvertedToStringYN);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_with_converter_int_zero_one_to_bool()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesConverters.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.IntZeroOneConvertedToBool = 1;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityConverters>().SingleAsync(x => x.Id == 1);
                Assert.Equal(1, result.Reference.IntZeroOneConvertedToBool);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_with_converter_string_True_False_to_bool()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesConverters.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.StringTrueFalseConvertedToBool = "False";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityConverters>().SingleAsync(x => x.Id == 1);
                Assert.Equal("False", result.Reference.StringTrueFalseConvertedToBool);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_with_converter_string_Y_N_to_bool()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesConverters.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.StringYNConvertedToBool = "Y";

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityConverters>().SingleAsync(x => x.Id == 1);
                Assert.Equal("Y", result.Reference.StringYNConvertedToBool);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_numeric()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.Numbers = [999, 997];
                entity.OwnedCollectionRoot[1].Numbers = [1024, 2048];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal(new[] { 999, 997 }, result.OwnedReferenceRoot.Numbers);
                Assert.Equal(new[] { 1024, 2048 }, result.OwnedCollectionRoot[1].Numbers);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_string()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();
                entity.OwnedReferenceRoot.Names = ["999", "997"];
                entity.OwnedCollectionRoot[1].Names = ["1024", "2048"];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityBasic>().SingleAsync();
                Assert.Equal(new[] { "999", "997" }, result.OwnedReferenceRoot.Names);
                Assert.Equal(new[] { "1024", "2048" }, result.OwnedCollectionRoot[1].Names);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_bool()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestBooleanCollection = new[] { true, true, false };
                entity.Collection[0].TestBooleanCollection = new[] { true, true, true, false };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { true, true, false }, result.Reference.TestBooleanCollection);
                Assert.Equal(new[] { true, true, true, false }, result.Collection[0].TestBooleanCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_byte()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestByteCollection = [25, 26];
                entity.Collection[0].TestByteCollection = [14];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new byte[] { 25, 26 }, result.Reference.TestByteCollection);
                Assert.Equal(new byte[] { 14 }, result.Collection[0].TestByteCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_char()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestCharacterCollection =
                [
                    'E',
                    'F',
                    'C',
                    'ö',
                    'r',
                    'E',
                    '\"',
                    '\\'
                ];
                entity.Collection[0].TestCharacterCollection.Add((char)0);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { 'E', 'F', 'C', 'ö', 'r', 'E', '\"', '\\' }, result.Reference.TestCharacterCollection);
                Assert.Equal(new[] { 'A', 'B', '\"', (char)0 }, result.Collection[0].TestCharacterCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_datetime()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestDateTimeCollection.Add(DateTime.Parse("01/01/3000 12:34:56"));
                entity.Collection[0].TestDateTimeCollection.Add(DateTime.Parse("01/01/3000 12:34:56"));

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new[]
                    {
                        DateTime.Parse("01/01/2000 12:34:56"),
                        DateTime.Parse("01/01/3000 12:34:56"),
                        DateTime.Parse("01/01/3000 12:34:56")
                    }, result.Reference.TestDateTimeCollection);
                Assert.Equal(
                    new[]
                    {
                        DateTime.Parse("01/01/2000 12:34:56"),
                        DateTime.Parse("01/01/3000 12:34:56"),
                        DateTime.Parse("01/01/3000 12:34:56")
                    }, result.Collection[0].TestDateTimeCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_datetimeoffset()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestDateTimeOffsetCollection = new List<DateTimeOffset>
                {
                    new(DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-4.0))
                };
                entity.Collection[0].TestDateTimeOffsetCollection = new List<DateTimeOffset>
                {
                    new(DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-4.0))
                };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new List<DateTimeOffset> { new(DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-4.0)) },
                    result.Reference.TestDateTimeOffsetCollection);
                Assert.Equal(
                    new List<DateTimeOffset> { new(DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-4.0)) },
                    result.Collection[0].TestDateTimeOffsetCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_decimal()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestDecimalCollection = [-13579.01M];
                entity.Collection[0].TestDecimalCollection = [-13579.01M];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { -13579.01M }, result.Reference.TestDecimalCollection);
                Assert.Equal(new[] { -13579.01M }, result.Collection[0].TestDecimalCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_double()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestDoubleCollection.Add(-1.23579);
                entity.Collection[0].TestDoubleCollection.Add(-1.23579);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { -1.23456789, 1.23456789, 0.0, -1.23579 }, result.Reference.TestDoubleCollection);
                Assert.Equal(new[] { -1.23456789, 1.23456789, 0.0, -1.23579 }, result.Collection[0].TestDoubleCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_guid()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestGuidCollection = [new("12345678-1234-4321-5555-987654321000")];
                entity.Collection[0].TestGuidCollection = [new("12345678-1234-4321-5555-987654321000")];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal([new("12345678-1234-4321-5555-987654321000")], result.Reference.TestGuidCollection);
                Assert.Equal([new("12345678-1234-4321-5555-987654321000")], result.Collection[0].TestGuidCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_int16()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestInt16Collection = new short[] { -3234 };
                entity.Collection[0].TestInt16Collection = new short[] { -3234 };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new short[] { -3234 }, result.Reference.TestInt16Collection);
                Assert.Equal(new short[] { -3234 }, result.Collection[0].TestInt16Collection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_int32()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestInt32Collection = [-3234];
                entity.Collection[0].TestInt32Collection = [-3234];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { -3234 }, result.Reference.TestInt32Collection);
                Assert.Equal(new[] { -3234 }, result.Collection[0].TestInt32Collection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_int64()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestInt64Collection.Clear();
                entity.Collection[0].TestInt64Collection.Clear();

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Empty(result.Reference.TestInt64Collection);
                Assert.Empty(result.Collection[0].TestInt64Collection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_signed_byte()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestSignedByteCollection = [-108];
                entity.Collection[0].TestSignedByteCollection = [-108];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new sbyte[] { -108 }, result.Reference.TestSignedByteCollection);
                Assert.Equal(new sbyte[] { -108 }, result.Collection[0].TestSignedByteCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_single()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestSingleCollection.RemoveAt(0);
                entity.Collection[0].TestSingleCollection.RemoveAt(1);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { 0.0F, -1.234F }, result.Reference.TestSingleCollection);
                Assert.Equal(new[] { -1.234F, -1.234F }, result.Collection[0].TestSingleCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_timespan()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestTimeSpanCollection[0] = new TimeSpan(0, 10, 1, 1, 7);
                entity.Collection[0].TestTimeSpanCollection[1] = new TimeSpan(0, 10, 1, 1, 7);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new[] { new TimeSpan(0, 10, 1, 1, 7), new TimeSpan(0, -10, 9, 8, 7) }, result.Reference.TestTimeSpanCollection);
                Assert.Equal(
                    new[] { new TimeSpan(0, 10, 9, 8, 7), new TimeSpan(0, 10, 1, 1, 7) }, result.Collection[0].TestTimeSpanCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_dateonly()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestDateOnlyCollection[0] = new DateOnly(1, 1, 7);
                entity.Collection[0].TestDateOnlyCollection[1] = new DateOnly(1, 1, 7);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new[] { new DateOnly(1, 1, 7), new DateOnly(4321, 1, 21) }, result.Reference.TestDateOnlyCollection);
                Assert.Equal(
                    new[] { new DateOnly(3234, 1, 23), new DateOnly(1, 1, 7) }, result.Collection[0].TestDateOnlyCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_timeonly()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestTimeOnlyCollection[0] = new TimeOnly(1, 1, 7);
                entity.Collection[0].TestTimeOnlyCollection[1] = new TimeOnly(1, 1, 7);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new[] { new TimeOnly(1, 1, 7), new TimeOnly(7, 17, 27) }, result.Reference.TestTimeOnlyCollection);
                Assert.Equal(
                    new[] { new TimeOnly(13, 42, 23), new TimeOnly(1, 1, 7) }, result.Collection[0].TestTimeOnlyCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_uint16()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestUnsignedInt16Collection = new List<ushort> { 1534 };
                entity.Collection[0].TestUnsignedInt16Collection = new List<ushort> { 1534 };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new List<ushort> { 1534 }, result.Reference.TestUnsignedInt16Collection);
                Assert.Equal(new List<ushort> { 1534 }, result.Collection[0].TestUnsignedInt16Collection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_uint32()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestUnsignedInt32Collection = [1237775789U];
                entity.Collection[0].TestUnsignedInt32Collection = [1237775789U];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { 1237775789U }, result.Reference.TestUnsignedInt32Collection);
                Assert.Equal(new[] { 1237775789U }, result.Collection[0].TestUnsignedInt32Collection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_uint64()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestUnsignedInt64Collection = [1234555555123456789UL];
                entity.Collection[0].TestUnsignedInt64Collection = [1234555555123456789UL];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { 1234555555123456789UL }, result.Reference.TestUnsignedInt64Collection);
                Assert.Equal(new[] { 1234555555123456789UL }, result.Collection[0].TestUnsignedInt64Collection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_nullable_int32()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableInt32Collection.Add(77);
                entity.Reference.TestNullableInt32Collection.Add(null);
                entity.Collection[0].TestNullableInt32Collection = [null, 77];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new int?[] { null, int.MinValue, 0, null, int.MaxValue, null, 77, null }, result.Reference.TestNullableInt32Collection);
                Assert.Equal(new int?[] { null, 77 }, result.Collection[0].TestNullableInt32Collection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_nullable_int32_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableInt32Collection = null;
                entity.Collection[0].TestNullableInt32Collection = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.Reference.TestNullableInt32Collection);
                Assert.Null(result.Collection[0].TestNullableInt32Collection);

                Assert.True(result.Reference.NewCollectionSet); // Set to null.
                Assert.True(result.Collection[0].NewCollectionSet); // Set to null.
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_enum()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestEnumCollection = new[] { JsonEnum.Three };
                entity.Collection[0].TestEnumCollection = new[] { JsonEnum.Three };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { JsonEnum.Three }, result.Reference.TestEnumCollection);
                Assert.Equal(new[] { JsonEnum.Three }, result.Collection[0].TestEnumCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_enum_with_int_converter()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestEnumWithIntConverterCollection = [JsonEnum.Three];
                entity.Collection[0].TestEnumWithIntConverterCollection = [JsonEnum.Three];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal([JsonEnum.Three], result.Reference.TestEnumWithIntConverterCollection);
                Assert.Equal([JsonEnum.Three], result.Collection[0].TestEnumWithIntConverterCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_nullable_enum()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestEnumCollection = new[] { JsonEnum.Three };
                entity.Collection[0].TestEnumCollection = new[] { JsonEnum.Three };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { JsonEnum.Three }, result.Reference.TestEnumCollection);
                Assert.Equal(new[] { JsonEnum.Three }, result.Collection[0].TestEnumCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_nullable_enum_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableEnumCollection = null;
                entity.Collection[0].TestNullableEnumCollection = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.Reference.TestNullableEnumCollection);
                Assert.Null(result.Collection[0].TestNullableEnumCollection);

                Assert.True(result.Reference.NewCollectionSet); // Set to null.
                Assert.True(result.Collection[0].NewCollectionSet); // Set to null.
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_nullable_enum_with_int_converter()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableEnumWithIntConverterCollection.Add(JsonEnum.Two);
                entity.Reference.TestNullableEnumWithIntConverterCollection.RemoveAt(1);
                entity.Collection[0].TestNullableEnumWithIntConverterCollection.Add(JsonEnum.Two);
                entity.Collection[0].TestNullableEnumWithIntConverterCollection.RemoveAt(2);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new JsonEnum?[] { JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7), JsonEnum.Two },
                    result.Reference.TestNullableEnumWithIntConverterCollection);
                Assert.Equal(
                    new JsonEnum?[] { JsonEnum.One, null, (JsonEnum)(-7), JsonEnum.Two },
                    result.Collection[0].TestNullableEnumWithIntConverterCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_nullable_enum_with_int_converter_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableEnumWithIntConverterCollection = null;
                entity.Collection[0].TestNullableEnumWithIntConverterCollection = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.Reference.TestNullableEnumWithIntConverterCollection);
                Assert.Null(result.Collection[0].TestNullableEnumWithIntConverterCollection);

                Assert.True(result.Reference.NewCollectionSet); // Set to null.
                Assert.True(result.Collection[0].NewCollectionSet); // Set to null.
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableEnumWithConverterThatHandlesNullsCollection = [JsonEnum.One];
                entity.Collection[0].TestNullableEnumWithConverterThatHandlesNullsCollection = [JsonEnum.Three];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal([JsonEnum.One], result.Reference.TestNullableEnumWithConverterThatHandlesNullsCollection);
                Assert.Equal(
                    [JsonEnum.Three], result.Collection[0].TestNullableEnumWithConverterThatHandlesNullsCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_collection_of_nullable_enum_with_converter_that_handles_nulls_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.Reference.TestNullableEnumWithConverterThatHandlesNullsCollection = null;
                entity.Collection[0].TestNullableEnumWithConverterThatHandlesNullsCollection = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.Reference.TestNullableEnumWithConverterThatHandlesNullsCollection);
                Assert.Null(result.Collection[0].TestNullableEnumWithConverterThatHandlesNullsCollection);

                Assert.False(result.Reference.NewCollectionSet);
                Assert.False(result.Collection[0].NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_bool()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestBooleanCollection = new[] { true, true, false };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { true, true, false }, result.TestBooleanCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_byte()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestByteCollection = [25, 26];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new byte[] { 25, 26 }, result.TestByteCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_char()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestCharacterCollection =
                [
                    'E',
                    'F',
                    'C',
                    'ö',
                    'r',
                    'E',
                    '\"',
                    '\\'
                ];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { 'E', 'F', 'C', 'ö', 'r', 'E', '\"', '\\' }, result.TestCharacterCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_datetime()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestDateTimeCollection.Add(DateTime.Parse("01/01/3000 12:34:56"));

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new[]
                    {
                        DateTime.Parse("01/01/2000 12:34:56"),
                        DateTime.Parse("01/01/3000 12:34:56"),
                        DateTime.Parse("01/01/3000 12:34:56")
                    }, result.TestDateTimeCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_datetimeoffset()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestDateTimeOffsetCollection = new List<DateTimeOffset>
                {
                    new(DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-4.0))
                };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new List<DateTimeOffset> { new(DateTime.Parse("01/01/3000 12:34:56"), TimeSpan.FromHours(-4.0)) },
                    result.TestDateTimeOffsetCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_decimal()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestDecimalCollection = [-13579.01M];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { -13579.01M }, result.TestDecimalCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_double()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestDoubleCollection.Add(-1.23579);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { -1.23456789, 1.23456789, 0.0, -1.23579 }, result.TestDoubleCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_guid()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestGuidCollection = [new("12345678-1234-4321-5555-987654321000")];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal([new("12345678-1234-4321-5555-987654321000")], result.TestGuidCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_int16()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestInt16Collection = new short[] { -3234 };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new short[] { -3234 }, result.TestInt16Collection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_int32()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestInt32Collection = [-3234];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { -3234 }, result.TestInt32Collection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_int64()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestInt64Collection.Clear();

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Empty(result.TestInt64Collection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_signed_byte()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestSignedByteCollection = [-108];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new sbyte[] { -108 }, result.TestSignedByteCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_single()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestSingleCollection.RemoveAt(0);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { 0.0F, -1.234F }, result.TestSingleCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_timespan()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestTimeSpanCollection[0] = new TimeSpan(0, 10, 1, 1, 7);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new[] { new TimeSpan(0, 10, 1, 1, 7), new TimeSpan(0, 7, 9, 8, 7) }, result.TestTimeSpanCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_uint16()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestUnsignedInt16Collection = new List<ushort> { 1534 };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new List<ushort> { 1534 }, result.TestUnsignedInt16Collection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_uint32()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestUnsignedInt32Collection = [1237775789U];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { 1237775789U }, result.TestUnsignedInt32Collection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_uint64()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestUnsignedInt64Collection = [1234555555123456789UL];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { 1234555555123456789UL }, result.TestUnsignedInt64Collection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_nullable_int32()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestNullableInt32Collection.Add(77);
                entity.TestNullableInt32Collection.Add(null);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new int?[] { null, int.MinValue, 0, null, int.MaxValue, null, 77, null }, result.TestNullableInt32Collection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_nullable_int32_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestNullableInt32Collection = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.TestNullableInt32Collection);

                Assert.True(result.NewCollectionSet); // Set to null.
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_enum()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestEnumCollection = new[] { JsonEnum.Three };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { JsonEnum.Three }, result.TestEnumCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_enum_with_int_converter()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestEnumWithIntConverterCollection = [JsonEnum.Three];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal([JsonEnum.Three], result.TestEnumWithIntConverterCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_nullable_enum()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestEnumCollection = new[] { JsonEnum.Three };

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(new[] { JsonEnum.Three }, result.TestEnumCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_nullable_enum_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestNullableEnumCollection = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.TestNullableEnumCollection);

                Assert.True(result.NewCollectionSet); // Set to null.
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_nullable_enum_with_int_converter()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestNullableEnumWithIntConverterCollection.Add(JsonEnum.Two);
                entity.TestNullableEnumWithIntConverterCollection.RemoveAt(1);

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal(
                    new JsonEnum?[] { JsonEnum.One, JsonEnum.Three, (JsonEnum)(-7), JsonEnum.Two },
                    result.TestNullableEnumWithIntConverterCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_nullable_enum_with_int_converter_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestNullableEnumWithIntConverterCollection = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.TestNullableEnumWithIntConverterCollection);

                Assert.True(result.NewCollectionSet); // Set to null.
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_nullable_enum_with_converter_that_handles_nulls()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestNullableEnumWithConverterThatHandlesNullsCollection = [JsonEnum.One];

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Equal([JsonEnum.One], result.TestNullableEnumWithConverterThatHandlesNullsCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual Task Edit_single_property_relational_collection_of_nullable_enum_with_converter_that_handles_nulls_set_to_null()
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var query = await context.JsonEntitiesAllTypes.ToListAsync();
                var entity = query.Single(x => x.Id == 1);
                entity.TestNullableEnumWithConverterThatHandlesNullsCollection = null;

                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var result = await context.Set<JsonEntityAllTypes>().SingleAsync(x => x.Id == 1);
                Assert.Null(result.TestNullableEnumWithConverterThatHandlesNullsCollection);

                Assert.False(result.NewCollectionSet);
            });

    [ConditionalFact]
    public virtual async Task SaveChanges_throws_when_required_primitive_collection_is_null()
        => await TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var entity = new JsonEntityAllTypes { TestGuidCollection = null };
                context.Add(entity);

                Assert.Equal(
                    CoreStrings.NullRequiredPrimitiveCollection(nameof(JsonEntityAllTypes), nameof(JsonEntityAllTypes.TestGuidCollection)),
                    (await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.SaveChangesAsync())).Message);
            });

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    [InlineData(null)]
    public virtual Task Add_and_update_top_level_optional_owned_collection_to_JSON(bool? value)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var newEntity = new JsonEntityBasic
                {
                    Id = 2,
                    Name = "NewEntity",
                    OwnedCollectionRoot =
                        value.HasValue
                            ? value.Value
                                ? [new()]
                                : []
                            : null
                };

                context.Add(newEntity);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var newEntity = await context.JsonEntitiesBasic.SingleAsync(e => e.Id == 2);

                if (value.HasValue)
                {
                    if (value.Value)
                    {
                        Assert.Single(newEntity.OwnedCollectionRoot!);
                        newEntity.OwnedCollectionRoot = null;
                    }
                    else
                    {
                        Assert.Empty(newEntity.OwnedCollectionRoot!);
                        newEntity.OwnedCollectionRoot.Add(new JsonOwnedRoot());
                    }
                }
                else
                {
                    Assert.Null(newEntity.OwnedCollectionRoot);
                    newEntity.OwnedCollectionRoot = [];

                    // Because just setting the navigation to an empty collection currently doesn't mark it as modified.
                    context.Entry(newEntity).State = EntityState.Modified;
                }
                await context.SaveChangesAsync();

                var saved = context.Database.SqlQueryRaw<string>("select OwnedCollectionRoot from JsonEntitiesBasic where Id = 2").ToList();
            },
            async context =>
            {
                var newEntity = await context.JsonEntitiesBasic.SingleAsync(e => e.Id == 2);

                if (value.HasValue)
                {
                    if (value.Value)
                    {
                        Assert.Null(newEntity.OwnedCollectionRoot);
                    }
                    else
                    {
                        Assert.Single(newEntity.OwnedCollectionRoot!);
                    }
                }
                else
                {
                    Assert.Empty(newEntity.OwnedCollectionRoot);
                }
            });

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    [InlineData(null)]
    public virtual Task Add_and_update_nested_optional_owned_collection_to_JSON(bool? value)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var newEntity = new JsonEntityBasic
                {
                    Id = 2,
                    Name = "NewEntity",
                    OwnedReferenceRoot = new JsonOwnedRoot()
                    {
                        OwnedCollectionBranch =
                            value.HasValue
                                ? value.Value
                                    ? [new()]
                                    : []
                                : null
                    }
                };

                context.Add(newEntity);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var newEntity = await context.JsonEntitiesBasic.SingleAsync(e => e.Id == 2);

                if (value.HasValue)
                {
                    if (value.Value)
                    {
                        Assert.Single(newEntity.OwnedReferenceRoot.OwnedCollectionBranch!);
                        newEntity.OwnedReferenceRoot.OwnedCollectionBranch = null;
                    }
                    else
                    {
                        Assert.Empty(newEntity.OwnedReferenceRoot.OwnedCollectionBranch!);
                        newEntity.OwnedReferenceRoot.OwnedCollectionBranch.Add(new JsonOwnedBranch());
                    }
                }
                else
                {
                    Assert.Null(newEntity.OwnedReferenceRoot.OwnedCollectionBranch);
                    newEntity.OwnedReferenceRoot.OwnedCollectionBranch = [];

                    // Because just setting the navigation to an empty collection currently doesn't mark it as modified.
                    context.Entry(newEntity).Reference(e => e.OwnedReferenceRoot).TargetEntry!.State = EntityState.Modified;
                }
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var newEntity = await context.JsonEntitiesBasic.SingleAsync(e => e.Id == 2);

                if (value.HasValue)
                {
                    if (value.Value)
                    {
                        Assert.Null(newEntity.OwnedReferenceRoot.OwnedCollectionBranch);
                    }
                    else
                    {
                        Assert.Single(newEntity.OwnedReferenceRoot.OwnedCollectionBranch!);
                    }
                }
                else
                {
                    Assert.Empty(newEntity.OwnedReferenceRoot.OwnedCollectionBranch);
                }
            });


    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    [InlineData(null)]
    public virtual Task Add_and_update_nested_optional_primitive_collection(bool? value)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext,
            UseTransaction,
            async context =>
            {
                var newEntity = new JsonEntityAllTypes
                {
                    Id = 7624,
                    TestDefaultStringCollection = [],
                    TestMaxLengthStringCollection = [],
                    TestBooleanCollection = [],
                    TestCharacterCollection = [],
                    TestDateTimeCollection = [],
                    TestDateTimeOffsetCollection = [],
                    TestDoubleCollection = [],
                    TestDecimalCollection = [],
                    TestGuidCollection = [],
                    TestInt16Collection = [],
                    TestInt32Collection = [],
                    TestInt64Collection = [],
                    TestSignedByteCollection = [],
                    TestSingleCollection = [],
                    TestTimeSpanCollection = [],
                    TestUnsignedInt16Collection = new List<ushort>(),
                    TestUnsignedInt32Collection = [],
                    TestUnsignedInt64Collection = [],
                    TestNullableInt32Collection = [],
                    TestEnumCollection = [],
                    TestEnumWithIntConverterCollection = [],
                    TestNullableEnumCollection = [],
                    TestNullableEnumWithIntConverterCollection = [],
                    TestNullableEnumWithConverterThatHandlesNullsCollection = Array.Empty<JsonEnum?>(),
                    Collection =
                    [
                        new()
                        {
                            TestDefaultStringCollection = [],
                            TestMaxLengthStringCollection = [],
                            TestBooleanCollection = [],
                            TestDateTimeCollection = [],
                            TestDateTimeOffsetCollection = [],
                            TestDoubleCollection = [],
                            TestDecimalCollection = [],
                            TestGuidCollection = [],
                            TestInt16Collection = [],
                            TestInt32Collection = [],
                            TestInt64Collection = [],
                            TestSignedByteCollection = [],
                            TestSingleCollection = [],
                            TestTimeSpanCollection = [],
                            TestDateOnlyCollection = [],
                            TestTimeOnlyCollection = [],
                            TestUnsignedInt16Collection = new List<ushort>(),
                            TestUnsignedInt32Collection = [],
                            TestUnsignedInt64Collection = [],
                            TestNullableInt32Collection = [],
                            TestEnumCollection = [],
                            TestEnumWithIntConverterCollection = [],
                            TestNullableEnumCollection = [],
                            TestNullableEnumWithIntConverterCollection = [],
                            TestNullableEnumWithConverterThatHandlesNullsCollection = Array.Empty<JsonEnum?>(),
                            TestCharacterCollection =
                                value.HasValue
                                    ? value.Value
                                        ? ['A']
                                        : []
                                    : null
                        }
                    ]
                };

                context.Add(newEntity);
                ClearLog();
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var newEntity = await context.Set<JsonEntityAllTypes>().SingleAsync(e => e.Id == 7624);

                if (value.HasValue)
                {
                    if (value.Value)
                    {
                        Assert.Single(newEntity.Collection!.Single().TestCharacterCollection!);
                        newEntity.Collection!.Single().TestCharacterCollection = null;
                    }
                    else
                    {
                        Assert.Empty(newEntity.Collection!.Single().TestCharacterCollection!);
                        newEntity.Collection!.Single().TestCharacterCollection.Add('Z');
                    }
                }
                else
                {
                    Assert.Null(newEntity.Collection!.Single().TestCharacterCollection);
                    newEntity.Collection!.Single().TestCharacterCollection = [];
                }

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var newEntity = await context.Set<JsonEntityAllTypes>().SingleAsync(e => e.Id == 7624);

                if (value.HasValue)
                {
                    if (value.Value)
                    {
                        Assert.Null(newEntity.Collection!.Single().TestCharacterCollection);
                    }
                    else
                    {
                        Assert.Single(newEntity.Collection!.Single().TestCharacterCollection!);
                    }
                }
                else
                {
                    Assert.Empty(newEntity.Collection!.Single().TestCharacterCollection);
                }
            });

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected abstract void ClearLog();
}

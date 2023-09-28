// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Update;

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
                    OwnedCollectionRoot = new List<JsonOwnedRoot>(),
                    OwnedReferenceRoot = new JsonOwnedRoot
                    {
                        Name = "RootName",
                        Number = 42,
                        OwnedCollectionBranch = new List<JsonOwnedBranch>(),
                        OwnedReferenceBranch = new JsonOwnedBranch
                        {
                            Date = new DateTime(2010, 10, 10),
                            Enum = JsonEnum.Three,
                            Fraction = 42.42m,
                            OwnedCollectionLeaf = new List<JsonOwnedLeaf>
                            {
                                new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" },
                            },
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
                Assert.Null(newEntity.OwnedCollectionRoot);
                Assert.Equal("RootName", newEntity.OwnedReferenceRoot.Name);
                Assert.Equal(42, newEntity.OwnedReferenceRoot.Number);
                Assert.Null(newEntity.OwnedReferenceRoot.OwnedCollectionBranch);
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
                    OwnedCollectionBranch = new List<JsonOwnedBranch>(),
                    OwnedReferenceBranch = new JsonOwnedBranch
                    {
                        Date = new DateTime(2010, 10, 10),
                        Enum = JsonEnum.Three,
                        Fraction = 42.42m,
                        OwnedCollectionLeaf = new List<JsonOwnedLeaf>
                        {
                            new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" },
                        },
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
                Assert.Null(updatedReference.OwnedCollectionBranch);
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
                    OwnedCollectionBranch = new List<JsonOwnedBranch>(),
                    OwnedReferenceBranch = new JsonOwnedBranch
                    {
                        Date = new DateTime(2010, 10, 10),
                        Enum = JsonEnum.Three,
                        Fraction = 42.42m,
                        OwnedCollectionLeaf = new List<JsonOwnedLeaf>
                        {
                            new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" },
                        },
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
                Assert.Null(updatedCollection[2].OwnedCollectionBranch);
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
                    OwnedCollectionLeaf = new List<JsonOwnedLeaf>
                    {
                        new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" },
                    },
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
                    OwnedCollectionLeaf = new List<JsonOwnedLeaf>
                    {
                        new() { SomethingSomething = "ss1" }, new() { SomethingSomething = "ss2" },
                    },
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
                Assert.Equal(false, result.Reference.TestBoolean);
                Assert.Equal(true, result.Collection[0].TestBoolean);
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
                Assert.Equal(null, result.Reference.TestNullableInt32);
                Assert.Equal(null, result.Collection[0].TestNullableInt32);
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
                Assert.Equal(null, result.Reference.TestNullableEnum);
                Assert.Equal(null, result.Collection[0].TestNullableEnum);
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
            Assert.Equal(null, result.Reference.TestNullableEnumWithIntConverter);
            Assert.Equal(null, result.Collection[0].TestNullableEnumWithIntConverter);
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
                Assert.Equal(null, result.Reference.TestNullableEnumWithConverterThatHandlesNulls);
                Assert.Equal(null, result.Collection[0].TestNullableEnumWithConverterThatHandlesNulls);
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
                entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf = new List<JsonOwnedLeaf>
                {
                    new() { SomethingSomething = "edit" }
                };

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

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected abstract void ClearLog();
}

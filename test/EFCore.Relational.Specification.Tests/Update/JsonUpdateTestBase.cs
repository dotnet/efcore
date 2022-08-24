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

    public JsonQueryContext CreateContext() => Fixture.CreateContext();

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
                            new JsonOwnedLeaf { SomethingSomething = "ss1" },
                            new JsonOwnedLeaf { SomethingSomething = "ss2" },
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
                            new JsonOwnedLeaf { SomethingSomething = "ss1" },
                            new JsonOwnedLeaf { SomethingSomething = "ss2" },
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
                        new JsonOwnedLeaf { SomethingSomething = "ss1" },
                        new JsonOwnedLeaf { SomethingSomething = "ss2" },
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
                    new JsonOwnedLeaf { SomethingSomething = "ss1" },
                    new JsonOwnedLeaf { SomethingSomething = "ss2" },
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
                            new JsonOwnedLeaf { SomethingSomething = "ss1" },
                            new JsonOwnedLeaf { SomethingSomething = "ss2" },
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
                Assert.Equal("yet another change", result.OwnedCollectionRoot[0].OwnedCollectionBranch[1].OwnedCollectionLeaf[0].SomethingSomething);
                Assert.Equal("and another", result.OwnedCollectionRoot[0].OwnedCollectionBranch[1].OwnedCollectionLeaf[1].SomethingSomething);
                Assert.Equal("...and another", result.OwnedCollectionRoot[0].OwnedCollectionBranch[0].OwnedCollectionLeaf[0].SomethingSomething);
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
                entity.OwnedReferenceRoot.OwnedCollectionBranch.Add(new JsonOwnedBranch
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

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected abstract void ClearLog();
}

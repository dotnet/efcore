// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.JsonQuery;

namespace Microsoft.EntityFrameworkCore.Update;

public abstract class JsonUpdateTestBase: SharedStoreFixtureBase<JsonQueryContext>
{
    protected override string StoreName => "JsonUpdateTest";

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
                entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf = null;
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var query = await context.JsonEntitiesBasic.ToListAsync();
                var entity = query.Single();

                Assert.Null(entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedCollectionLeaf);
                var newLeaf = new JsonOwnedLeaf { SomethingSomething = "ss3" };
                entity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf = newLeaf;
                
                await context.SaveChangesAsync();
            },
            async context =>
            {
                var updatedEntity = await context.JsonEntitiesBasic.SingleAsync();
                var updatedReference = updatedEntity.OwnedReferenceRoot.OwnedReferenceBranch.OwnedReferenceLeaf;
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

    public void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        => facade.UseTransaction(transaction.GetDbTransaction());

    protected override void Seed(JsonQueryContext context)
    {
        var jsonEntitiesBasic = JsonQueryData.CreateJsonEntitiesBasic();
        var jsonEntitiesInheritance = JsonQueryData.CreateJsonEntitiesInheritance();

        context.JsonEntitiesBasic.AddRange(jsonEntitiesBasic);
        context.JsonEntitiesInheritance.AddRange(jsonEntitiesInheritance);
        context.SaveChanges();
    }

    protected override void Clean(DbContext context)
    {
        base.Clean(context);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
    {
        modelBuilder.Entity<JsonEntityBasic>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntityBasic>().OwnsOne(x => x.OwnedReferenceRoot, b =>
        {
            b.ToJson();
            b.WithOwner(x => x.Owner);
            b.OwnsOne(x => x.OwnedReferenceBranch, bb =>
            {
                bb.Property(x => x.Fraction).HasPrecision(18, 2);
                bb.OwnsOne(x => x.OwnedReferenceLeaf).WithOwner(x => x.Parent);
                bb.OwnsMany(x => x.OwnedCollectionLeaf);
            });
            b.OwnsMany(x => x.OwnedCollectionBranch, bb =>
            {
                bb.Property(x => x.Fraction).HasPrecision(18, 2);
                bb.OwnsOne(x => x.OwnedReferenceLeaf);
                bb.Navigation(x => x.OwnedReferenceLeaf).IsRequired(false);
                bb.OwnsMany(x => x.OwnedCollectionLeaf).WithOwner(x => x.Parent);
            });
        });

        modelBuilder.Entity<JsonEntityBasic>().Navigation(x => x.OwnedReferenceRoot).IsRequired(false);

        modelBuilder.Entity<JsonEntityBasic>().OwnsMany(x => x.OwnedCollectionRoot, b =>
        {
            b.OwnsOne(x => x.OwnedReferenceBranch, bb =>
            {
                bb.Property(x => x.Fraction).HasPrecision(18, 2);
                bb.OwnsOne(x => x.OwnedReferenceLeaf);
                bb.OwnsMany(x => x.OwnedCollectionLeaf).WithOwner(x => x.Parent);
            });

            b.OwnsMany(x => x.OwnedCollectionBranch, bb =>
            {
                bb.Property(x => x.Fraction).HasPrecision(18, 2);
                bb.OwnsOne(x => x.OwnedReferenceLeaf).WithOwner(x => x.Parent);
                bb.OwnsMany(x => x.OwnedCollectionLeaf);
            });
            b.ToJson();
        });

        modelBuilder.Entity<JsonEntityInheritanceBase>().Property(x => x.Id).ValueGeneratedNever();
        modelBuilder.Entity<JsonEntityInheritanceBase>(b =>
        {
            b.OwnsOne(x => x.ReferenceOnBase, bb =>
            {
                bb.ToJson();
                bb.OwnsOne(x => x.OwnedReferenceLeaf);
                bb.OwnsMany(x => x.OwnedCollectionLeaf);
                bb.Property(x => x.Fraction).HasPrecision(18, 2);
            });

            b.OwnsMany(x => x.CollectionOnBase, bb =>
            {
                bb.ToJson();
                bb.OwnsOne(x => x.OwnedReferenceLeaf);
                bb.OwnsMany(x => x.OwnedCollectionLeaf);
                bb.Property(x => x.Fraction).HasPrecision(18, 2);
            });
        });

        modelBuilder.Entity<JsonEntityInheritanceDerived>(b =>
        {
            b.HasBaseType<JsonEntityInheritanceBase>();
            b.OwnsOne(x => x.ReferenceOnDerived, bb =>
            {
                bb.ToJson();
                bb.OwnsOne(x => x.OwnedReferenceLeaf);
                bb.OwnsMany(x => x.OwnedCollectionLeaf);
                bb.Property(x => x.Fraction).HasPrecision(18, 2);
            });

            b.OwnsMany(x => x.CollectionOnDerived, bb =>
            {
                bb.ToJson();
                bb.OwnsOne(x => x.OwnedReferenceLeaf);
                bb.OwnsMany(x => x.OwnedCollectionLeaf);
                bb.Property(x => x.Fraction).HasPrecision(18, 2);
            });
        });

        modelBuilder.Ignore<JsonEntityCustomNaming>();
        modelBuilder.Ignore<JsonEntitySingleOwned>();

        base.OnModelCreating(modelBuilder, context);
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Update;

public abstract class UpdatesTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : UpdatesTestBase<TFixture>.UpdatesFixtureBase
{
    protected UpdatesTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    public static IEnumerable<object[]> IsAsyncData = new object[][] { [false], [true] };

    [ConditionalTheory] // Issue #25905
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_delete_and_add_for_same_key(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var rodney1 = new Rodney { Id = "SnotAndMarmite", Concurrency = new DateTime(1973, 9, 3) };
                if (async)
                {
                    await context.AddAsync(rodney1);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(rodney1);
                    await context.SaveChangesAsync();
                }

                context.Remove(rodney1);

                var rodney2 = new Rodney { Id = "SnotAndMarmite", Concurrency = new DateTime(1973, 9, 4) };
                if (async)
                {
                    await context.AddAsync(rodney2);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(rodney2);
                    await context.SaveChangesAsync();
                }

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
                Assert.Equal(EntityState.Unchanged, context.Entry(rodney2).State);
                Assert.Equal(EntityState.Detached, context.Entry(rodney1).State);
            });

    [ConditionalTheory] // Issue #29789
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_change_type_of_pk_to_pk_dependent_by_replacing_with_new_dependent(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var gift = new Gift { Recipient = "Alice", Obscurer = new GiftPaper { Pattern = "Stripes" } };
                await context.AddAsync(gift);
                _ = async ? await context.SaveChangesAsync() : await context.SaveChangesAsync();
            },
            async context =>
            {
                var gift = await context.Set<Gift>().Include(e => e.Obscurer).SingleAsync();
                var bag = new GiftBag { Pattern = "Gold stars" };
                gift.Obscurer = bag;
                _ = async ? await context.SaveChangesAsync() : await context.SaveChangesAsync();
            },
            async context =>
            {
                var gift = await context.Set<Gift>().Include(e => e.Obscurer).SingleAsync();

                Assert.IsType<GiftBag>(gift.Obscurer);
                Assert.Equal(gift.Id, gift.Obscurer.Id);
                Assert.Single(context.Set<GiftObscurer>());
            });

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_change_type_of__dependent_by_replacing_with_new_dependent(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var lift = new Lift { Recipient = "Alice", Obscurer = new LiftPaper { Pattern = "Stripes" } };
                await context.AddAsync(lift);
                _ = async ? await context.SaveChangesAsync() : await context.SaveChangesAsync();
            },
            async context =>
            {
                var lift = await context.Set<Lift>().Include(e => e.Obscurer).SingleAsync();
                var bag = new LiftBag { Pattern = "Gold stars" };
                lift.Obscurer = bag;
                _ = async ? await context.SaveChangesAsync() : await context.SaveChangesAsync();
            },
            async context =>
            {
                var lift = await context.Set<Lift>().Include(e => e.Obscurer).SingleAsync();

                Assert.IsType<LiftBag>(lift.Obscurer);
                Assert.Equal(lift.Id, lift.Obscurer.LiftId);
                Assert.Single(context.Set<LiftObscurer>());
            });

    [ConditionalFact]
    public virtual Task Mutation_of_tracked_values_does_not_mutate_values_in_store()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var bytes = new byte[] { 1, 2, 3, 4 };

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.AFewBytes.AddRange(
                    new AFewBytes { Id = id1, Bytes = bytes },
                    new AFewBytes { Id = id2, Bytes = bytes });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                bytes[1] = 22;

                var fromStore1 = await context.AFewBytes.FirstAsync(p => p.Id == id1);
                var fromStore2 = await context.AFewBytes.FirstAsync(p => p.Id == id2);

                Assert.Equal(2, fromStore1.Bytes[1]);
                Assert.Equal(2, fromStore2.Bytes[1]);

                fromStore1.Bytes[1] = 222;
                fromStore2.Bytes[1] = 222;

                context.Entry(fromStore1).State = EntityState.Modified;

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var fromStore1 = await context.AFewBytes.FirstAsync(p => p.Id == id1);
                var fromStore2 = await context.AFewBytes.FirstAsync(p => p.Id == id2);

                Assert.Equal(222, fromStore1.Bytes[1]);
                Assert.Equal(2, fromStore2.Bytes[1]);
            });
    }

    [ConditionalFact]
    public virtual Task Save_partial_update()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entry = context.Products.Attach(
                    new Product { Id = productId, Price = 1.49M });

                entry.Property(c => c.Price).CurrentValue = 1.99M;
                entry.Property(p => p.Price).IsModified = true;

                Assert.False(entry.Property(p => p.DependentId).IsModified);
                Assert.False(entry.Property(p => p.Name).IsModified);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var product = await context.Products.FirstAsync(p => p.Id == productId);

                Assert.Equal(1.99M, product.Price);
                Assert.Equal("Apple Cider", product.Name);
            });
    }

    [ConditionalFact]
    public virtual Task Save_partial_update_on_missing_record_throws()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entry = context.Products.Attach(
                    new Product { Id = new Guid("3d1302c5-4cf8-4043-9758-de9398f6fe10"), Name = "Apple Fritter" });

                entry.Property(c => c.Name).IsModified = true;

                Assert.Equal(
                    UpdateConcurrencyMessage,
                    (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                        () => context.SaveChangesAsync())).Message);
            });

    [ConditionalFact]
    public virtual Task Save_partial_update_on_concurrency_token_original_value_mismatch_throws()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entry = context.Products.Attach(
                    new Product
                    {
                        Id = productId,
                        Name = "Apple Fritter",
                        Price = 3.49M // Not the same as the value stored in the database
                    });

                entry.Property(c => c.Name).IsModified = true;

                Assert.Equal(
                    UpdateConcurrencyTokenMessage,
                    (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                        () => context.SaveChangesAsync())).Message);
            });
    }

    [ConditionalFact]
    public virtual Task Update_on_bytes_concurrency_token_original_value_mismatch_throws()
    {
        var productId = Guid.NewGuid();

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [8, 7, 6, 5, 4, 3, 2, 1]
                    });

                entry.Entity.Name = "GigaChips";

                await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    () => context.SaveChangesAsync());
            },
            async context => Assert.Equal("MegaChips", (await context.ProductWithBytes.FindAsync(productId))!.Name));
    }

    [ConditionalFact]
    public virtual Task Update_on_bytes_concurrency_token_original_value_matches_does_not_throw()
    {
        var productId = Guid.NewGuid();

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                entry.Entity.Name = "GigaChips";

                Assert.Equal(1, await context.SaveChangesAsync());
            },
            async context => Assert.Equal("GigaChips", (await context.ProductWithBytes.FindAsync(productId))!.Name));
    }

    [ConditionalFact]
    public virtual Task Remove_on_bytes_concurrency_token_original_value_mismatch_throws()
    {
        var productId = Guid.NewGuid();

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [8, 7, 6, 5, 4, 3, 2, 1]
                    });

                entry.State = EntityState.Deleted;

                await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                    () => context.SaveChangesAsync());
            },
            async context => Assert.Equal("MegaChips", (await context.ProductWithBytes.FindAsync(productId))!.Name));
    }

    [ConditionalFact]
    public virtual Task Remove_on_bytes_concurrency_token_original_value_matches_does_not_throw()
    {
        var productId = Guid.NewGuid();

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = [1, 2, 3, 4, 5, 6, 7, 8]
                    });

                entry.State = EntityState.Deleted;

                Assert.Equal(1, await context.SaveChangesAsync());
            },
            async context => Assert.Null(await context.ProductWithBytes.FindAsync(productId)));
    }

    [ConditionalFact]
    public virtual Task Can_add_and_remove_self_refs()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var parent = new Person("1", null);
                var child1 = new Person("2", parent);
                var child2 = new Person("3", parent);
                var grandchild1 = new Person("4", child1);
                var grandchild2 = new Person("5", child1);
                var grandchild3 = new Person("6", child2);
                var grandchild4 = new Person("7", child2);

                context.Add(parent);
                context.Add(child1);
                context.Add(child2);
                context.Add(grandchild1);
                context.Add(grandchild2);
                context.Add(grandchild3);
                context.Add(grandchild4);

                await context.SaveChangesAsync();

                context.Remove(parent);
                context.Remove(child1);
                context.Remove(child2);
                context.Remove(grandchild1);
                context.Remove(grandchild2);
                context.Remove(grandchild3);
                context.Remove(grandchild4);

                parent = new Person("1", null);
                child1 = new Person("2", parent);
                child2 = new Person("3", parent);
                grandchild1 = new Person("4", child1);
                grandchild2 = new Person("5", child1);
                grandchild3 = new Person("6", child2);
                grandchild4 = new Person("7", child2);

                context.Add(parent);
                context.Add(child1);
                context.Add(child2);
                context.Add(grandchild1);
                context.Add(grandchild2);
                context.Add(grandchild3);
                context.Add(grandchild4);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var people = await context.Set<Person>()
                    .Include(p => p.Parent!).ThenInclude(c => c.Parent!).ThenInclude(c => c.Parent)
                    .ToListAsync();
                Assert.Equal(7, people.Count);
                Assert.Equal("1", people.Single(p => p.Parent == null).Name);
            });

    [ConditionalFact]
    public virtual Task Can_change_enums_with_conversion()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var person = new Person("1", null)
                {
                    Address = new Address { Country = Country.Eswatini, City = "Bulembu" }, Country = "Eswatini"
                };

                context.Add(person);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var person = context.Set<Person>().Single();
                person.Address = new Address
                {
                    Country = Country.Türkiye,
                    City = "Konya",
                    ZipCode = 42100
                };
                person.Country = "Türkiye";
                person.ZipCode = "42100";

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var person = await context.Set<Person>().SingleAsync();

                Assert.Equal(Country.Türkiye, person.Address!.Country);
                Assert.Equal("Konya", person.Address.City);
                Assert.Equal(42100, person.Address.ZipCode);
                Assert.Equal("Türkiye", person.Country);
                Assert.Equal("42100", person.ZipCode);
            });

    [ConditionalFact]
    public virtual Task Can_remove_partial()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Products.Remove(
                    new Product { Id = productId, Price = 1.49M });

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var product = await context.Products.FirstOrDefaultAsync(f => f.Id == productId);

                Assert.Null(product);
            });
    }

    [ConditionalFact]
    public virtual Task Remove_partial_on_missing_record_throws()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Products.Remove(
                    new Product { Id = new Guid("3d1302c5-4cf8-4043-9758-de9398f6fe10") });

                Assert.Equal(
                    UpdateConcurrencyMessage,
                    (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                        () => context.SaveChangesAsync())).Message);
            });

    [ConditionalFact]
    public virtual Task Remove_partial_on_concurrency_token_original_value_mismatch_throws()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Products.Remove(
                    new Product
                    {
                        Id = productId, Price = 3.49M // Not the same as the value stored in the database
                    });

                Assert.Equal(
                    UpdateConcurrencyTokenMessage,
                    (await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                        () => context.SaveChangesAsync())).Message);
            });
    }

    [ConditionalFact]
    public virtual Task Save_replaced_principal()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var category = context.Categories.AsNoTracking().Single();
                var products = context.Products.AsNoTracking().Where(p => p.DependentId == category.PrincipalId).ToList();

                Assert.Equal(2, products.Count);

                var newCategory = new Category
                {
                    Id = category.Id,
                    PrincipalId = category.PrincipalId,
                    Name = "New Category"
                };
                context.Remove(category);
                context.Add(newCategory);

                await context.SaveChangesAsync();
            },
            async context =>
            {
                var category = await context.Categories.SingleAsync();
                var products = await context.Products.Where(p => p.DependentId == category.PrincipalId).ToListAsync();

                Assert.Equal("New Category", category.Name);
                Assert.Equal(2, products.Count);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task SaveChanges_processes_all_tracked_entities(bool async)
    {
        var categoryId = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                categoryId = (await context.Categories.SingleAsync()).Id;
            },
            async context =>
            {
                var stateManager = context.GetService<IStateManager>();

                var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
                var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

                var entry1 = stateManager.GetOrCreateEntry(
                    new SpecialCategory { PrincipalId = 777 });
                var entry2 = stateManager.GetOrCreateEntry(
                    new Category { Id = categoryId, PrincipalId = 778 });
                var entry3 = stateManager.GetOrCreateEntry(
                    new Product { Id = productId1 });
                var entry4 = stateManager.GetOrCreateEntry(
                    new Product { Id = productId2, Price = 2.49M });

                entry1.SetEntityState(EntityState.Added);
                entry2.SetEntityState(EntityState.Modified);
                entry3.SetEntityState(EntityState.Unchanged);
                entry4.SetEntityState(EntityState.Deleted);

                var processedEntities = 0;
                if (async)
                {
                    processedEntities = await stateManager.SaveChangesAsync(true);
                }
                else
                {
                    processedEntities = stateManager.SaveChanges(true);
                }

                // Assert.Equal(3, processedEntities);
                Assert.Equal(3, stateManager.Entries.Count());
                Assert.Contains(entry1, stateManager.Entries);
                Assert.Contains(entry2, stateManager.Entries);
                Assert.Contains(entry3, stateManager.Entries);

                Assert.Equal(EntityState.Unchanged, entry1.EntityState);
                Assert.Equal(EntityState.Unchanged, entry2.EntityState);
                Assert.Equal(EntityState.Unchanged, entry3.EntityState);

                Assert.True(((SpecialCategory)entry1.Entity).Id > 0);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task SaveChanges_false_processes_all_tracked_entities_without_calling_AcceptAllChanges(bool async)
    {
        var categoryId = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                categoryId = (await context.Categories.SingleAsync()).Id;
            },
            async context =>
            {
                var stateManager = context.GetService<IStateManager>();

                var productId1 = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");
                var productId2 = new Guid("0edc9136-7eed-463b-9b97-bdb9648ab877");

                var entry1 = stateManager.GetOrCreateEntry(
                    new SpecialCategory { PrincipalId = 777 });
                var entry2 = stateManager.GetOrCreateEntry(
                    new Category { Id = categoryId, PrincipalId = 778 });
                var entry3 = stateManager.GetOrCreateEntry(
                    new Product { Id = productId1 });
                var entry4 = stateManager.GetOrCreateEntry(
                    new Product { Id = productId2, Price = 2.49M });

                entry1.SetEntityState(EntityState.Added);
                entry2.SetEntityState(EntityState.Modified);
                entry3.SetEntityState(EntityState.Unchanged);
                entry4.SetEntityState(EntityState.Deleted);
                var generatedId = ((SpecialCategory)entry1.Entity).Id;

                var processedEntities = 0;
                if (async)
                {
                    processedEntities = await stateManager.SaveChangesAsync(false);
                }
                else
                {
                    processedEntities = stateManager.SaveChanges(false);
                }

                //Assert.Equal(3, processedEntities);
                Assert.Equal(4, stateManager.Entries.Count());
                Assert.Contains(entry1, stateManager.Entries);
                Assert.Contains(entry2, stateManager.Entries);
                Assert.Contains(entry3, stateManager.Entries);
                Assert.Contains(entry4, stateManager.Entries);

                Assert.Equal(EntityState.Added, entry1.EntityState);
                Assert.Equal(EntityState.Modified, entry2.EntityState);
                Assert.Equal(EntityState.Unchanged, entry3.EntityState);
                Assert.Equal(EntityState.Deleted, entry4.EntityState);

                Assert.Equal(generatedId, ((SpecialCategory)entry1.Entity).Id);
            });
    }

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task Ignore_before_save_property_is_still_generated(bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entities = new List<object>
                {
                    new CupCake
                    {
                        Id = -1102,
                        Name = "B2",
                        CakeName = "C2",
                        CupCakeName = "CC2"
                    }
                };

                if (async)
                {
                    await context.AddRangeAsync(entities);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.AddRange(entities);
                    await context.SaveChangesAsync();
                }
            },
            async context =>
            {
                var query = context.Set<Baked>().Include(e => e.Tin).Include(e => e.Ingredients).Include(e => ((Muffin)e).Top);
                var bakedGoods = async ? await query.ToListAsync() : query.ToList();

                Assert.Equal(1, bakedGoods.Count);
                Assert.Equal("B2", bakedGoods[0].Name);
                Assert.Equal("C2", ((Cake)bakedGoods[0]).CakeName);
                Assert.Equal("CC2", ((CupCake)bakedGoods[0]).CupCakeName);
            });

    [ConditionalTheory]
    [MemberData(nameof(IsAsyncData))]
    public Task Ignore_before_save_property_is_still_generated_graph(bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entities = new List<object>
                {
                    new Baked { Id = -100, Name = "B0" },
                    new Tin
                    {
                        Id = -1000,
                        BakedId = -100,
                        TinName = "B0"
                    },
                    new Ingredient
                    {
                        Id = -2000,
                        BakedId = -100,
                        IngredientName = "B00"
                    },
                    new Ingredient
                    {
                        Id = -2001,
                        BakedId = -100,
                        IngredientName = "B01"
                    },
                    new Cake
                    {
                        Id = -101,
                        Name = "B1",
                        CakeName = "C1"
                    },
                    new Tin
                    {
                        Id = -1001,
                        BakedId = -101,
                        TinName = "B1"
                    },
                    new Ingredient
                    {
                        Id = -2010,
                        BakedId = -101,
                        IngredientName = "B10"
                    },
                    new Ingredient
                    {
                        Id = -2011,
                        BakedId = -101,
                        IngredientName = "B11"
                    },
                    new CupCake
                    {
                        Id = -102,
                        Name = "B2",
                        CakeName = "C2",
                        CupCakeName = "CC2"
                    },
                    new Tin
                    {
                        Id = -1002,
                        BakedId = -102,
                        TinName = "B2"
                    },
                    new Ingredient
                    {
                        Id = -2020,
                        BakedId = -102,
                        IngredientName = "B20"
                    },
                    new Ingredient
                    {
                        Id = -2021,
                        BakedId = -102,
                        IngredientName = "B21"
                    },
                    new Muffin
                    {
                        Id = -103,
                        Name = "B3",
                        MuffinName = "M1"
                    },
                    new Tin
                    {
                        Id = -1003,
                        BakedId = -103,
                        TinName = "B3"
                    },
                    new Ingredient
                    {
                        Id = -2030,
                        BakedId = -103,
                        IngredientName = "B30"
                    },
                    new Ingredient
                    {
                        Id = -2031,
                        BakedId = -103,
                        IngredientName = "B31"
                    },
                    new Top
                    {
                        Id = -3003,
                        MuffinId = -103,
                        TopName = "M1"
                    }
                };

                if (async)
                {
                    await context.AddRangeAsync(entities);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.AddRange(entities);
                    await context.SaveChangesAsync();
                }
            },
            async context =>
            {
                var query = context.Set<Baked>().Include(e => e.Tin).Include(e => e.Ingredients).Include(e => ((Muffin)e).Top);
                var bakedGoods = async ? await query.ToListAsync() : query.ToList();

                Assert.Equal(4, bakedGoods.Count);
                AssertFixup("B0");
                AssertFixup("B1");
                AssertFixup("B2");
                AssertFixup("B3");

                var muffin = bakedGoods.OfType<Muffin>().Single();
                Assert.Equal("M1", muffin.MuffinName);
                Assert.Equal("M1", muffin.Top.TopName);

                void AssertFixup(string bakedName)
                {
                    var b0 = bakedGoods.Single(e => e.Name == bakedName);
                    Assert.Equal(bakedName, b0.Tin!.TinName);
                    Assert.Equal(2, b0.Ingredients.Count);
                    Assert.Contains(bakedName + "0", b0.Ingredients.Select(e => e.IngredientName));
                    Assert.Contains(bakedName + "1", b0.Ingredients.Select(e => e.IngredientName));
                }
            });

    protected class Baked
    {
        public long Id { get; set; }
        public required string Name { get; set; }
        public Tin? Tin { get; set; }
        public List<Ingredient> Ingredients { get; } = [];
    }

    protected class Cake : Baked
    {
        public required string CakeName { get; set; }
    }

    protected class CupCake : Cake
    {
        public required string CupCakeName { get; set; }
    }

    protected class Muffin : Baked
    {
        public required string MuffinName { get; set; }
        public Top Top { get; set; } = null!;

    }

    protected class Tin
    {
        public long Id { get; set; }
        public long? BakedId { get; set; }
        public string? TinName { get; set; }
        public Baked? Baked { get; set; }
    }

    protected class Ingredient
    {
        public long Id { get; set; }
        public long BakedId { get; set; }
        public string? IngredientName { get; set; }
        public Baked Baked { get; set; } = null!;
    }

    protected class Top
    {
        public long Id { get; set; }
        public long MuffinId { get; set; }
        public string? TopName { get; set; }
        public Muffin Muffin { get; set; } = null!;
    }

    protected abstract string UpdateConcurrencyMessage { get; }

    protected abstract string UpdateConcurrencyTokenMessage { get; }

    protected virtual Task ExecuteWithStrategyInTransactionAsync(
        Func<UpdatesContext, Task> testOperation,
        Func<UpdatesContext, Task>? nestedTestOperation1 = null,
        Func<UpdatesContext, Task>? nestedTestOperation2 = null)
        => TestHelpers.ExecuteWithStrategyInTransactionAsync(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2);

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    protected UpdatesContext CreateContext()
        => Fixture.CreateContext();

    public abstract class UpdatesFixtureBase : SharedStoreFixtureBase<UpdatesContext>
    {
        protected override string StoreName
            => "UpdateTest";

        protected override void OnModelCreating(ModelBuilder modelBuilder, DbContext context)
        {
            modelBuilder.Entity<Baked>().Property(e => e.Id).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            modelBuilder.Entity<Cake>();
            modelBuilder.Entity<CupCake>();
            modelBuilder.Entity<Muffin>();
            modelBuilder.Entity<Tin>().Property(e => e.Id).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            modelBuilder.Entity<Ingredient>().Property(e => e.Id).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);
            modelBuilder.Entity<Top>().Property(e => e.Id).Metadata.SetBeforeSaveBehavior(PropertySaveBehavior.Ignore);

            modelBuilder.Entity<Product>().HasMany(e => e.ProductCategories).WithOne()
                .HasForeignKey(e => e.ProductId);
            modelBuilder.Entity<ProductWithBytes>().HasMany(e => e.ProductCategories).WithOne()
                .HasForeignKey(e => e.ProductId);

            modelBuilder.Entity<ProductCategory>()
                .HasKey(p => new { p.CategoryId, p.ProductId });

            modelBuilder.Entity<Product>().HasOne(p => p.DefaultCategory).WithMany()
                .HasForeignKey(e => e.DependentId)
                .HasPrincipalKey(e => e.PrincipalId);

            modelBuilder.Entity<Person>(
                pb =>
                {
                    pb.HasOne(p => p.Parent)
                        .WithMany()
                        .OnDelete(DeleteBehavior.Restrict);
                    pb.OwnsOne(p => p.Address)
                        .Property(p => p.Country)
                        .HasConversion<string>();
                    pb.Property(p => p.ZipCode)
                        .HasConversion<int?>(v => v == null ? null : int.Parse(v), v => v == null ? null : v.ToString()!);
                });

            modelBuilder.Entity<Category>().HasMany(e => e.ProductCategories).WithOne(e => e.Category)
                .HasForeignKey(e => e.CategoryId);

            modelBuilder.Entity<SpecialCategory>();

            modelBuilder.Entity<AFewBytes>()
                .Property(e => e.Id)
                .ValueGeneratedNever();

            modelBuilder
                .Entity<
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectly
                >(
                    eb =>
                    {
                        eb.HasKey(
                            l => new
                            {
                                l.ProfileId,
                                l.ProfileId1,
                                l.ProfileId3,
                                l.ProfileId4,
                                l.ProfileId5,
                                l.ProfileId6,
                                l.ProfileId7,
                                l.ProfileId8,
                                l.ProfileId9,
                                l.ProfileId10,
                                l.ProfileId11,
                                l.ProfileId12,
                                l.ProfileId13,
                                l.ProfileId14
                            });
                        eb.HasIndex(
                            l => new
                            {
                                l.ProfileId,
                                l.ProfileId1,
                                l.ProfileId3,
                                l.ProfileId4,
                                l.ProfileId5,
                                l.ProfileId6,
                                l.ProfileId7,
                                l.ProfileId8,
                                l.ProfileId9,
                                l.ProfileId10,
                                l.ProfileId11,
                                l.ProfileId12,
                                l.ProfileId13,
                                l.ProfileId14,
                                l.ExtraProperty
                            });
                    });

            modelBuilder
                .Entity<
                    LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
                >(
                    eb =>
                    {
                        eb.HasKey(l => new { l.ProfileId });
                        eb.HasOne(d => d.Login).WithOne()
                            .HasForeignKey<
                                LoginEntityTypeWithAnExtremelyLongAndOverlyConvolutedNameThatIsUsedToVerifyThatTheStoreIdentifierGenerationLengthLimitIsWorkingCorrectlyDetails
                            >(
                                l => new
                                {
                                    l.ProfileId,
                                    l.ProfileId1,
                                    l.ProfileId3,
                                    l.ProfileId4,
                                    l.ProfileId5,
                                    l.ProfileId6,
                                    l.ProfileId7,
                                    l.ProfileId8,
                                    l.ProfileId9,
                                    l.ProfileId10,
                                    l.ProfileId11,
                                    l.ProfileId12,
                                    l.ProfileId13,
                                    l.ProfileId14
                                });
                    });

            modelBuilder.Entity<Profile>(
                pb =>
                {
                    pb.HasKey(
                        l => new
                        {
                            l.Id,
                            l.Id1,
                            l.Id3,
                            l.Id4,
                            l.Id5,
                            l.Id6,
                            l.Id7,
                            l.Id8,
                            l.Id9,
                            l.Id10,
                            l.Id11,
                            l.Id12,
                            l.Id13,
                            l.Id14
                        });
                    pb.HasOne(p => p.User)
                        .WithOne(l => l.Profile)
                        .IsRequired();
                });

            modelBuilder.Entity<Gift>();
            modelBuilder.Entity<GiftObscurer>().HasOne<Gift>().WithOne(x => x.Obscurer).HasForeignKey<GiftObscurer>(e => e.Id);
            modelBuilder.Entity<GiftBag>();
            modelBuilder.Entity<GiftPaper>();

            modelBuilder.Entity<Lift>();
            modelBuilder.Entity<LiftObscurer>().HasOne<Lift>().WithOne(x => x.Obscurer).HasForeignKey<LiftObscurer>(e => e.LiftId);
            modelBuilder.Entity<LiftBag>();
            modelBuilder.Entity<LiftPaper>();
        }

        protected override Task SeedAsync(UpdatesContext context)
            => UpdatesContext.SeedAsync(context);
    }
}

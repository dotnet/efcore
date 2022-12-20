// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

# nullable enable

using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.TestModels.UpdatesModel;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public abstract class UpdatesTestBase<TFixture> : IClassFixture<TFixture>
    where TFixture : UpdatesTestBase<TFixture>.UpdatesFixtureBase
{
    protected UpdatesTestBase(TFixture fixture)
    {
        Fixture = fixture;
    }

    protected TFixture Fixture { get; }

    public static IEnumerable<object[]> IsAsyncData = new[] { new object[] { true }, new object[] { false } };

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
                    context.SaveChanges();
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
                    context.SaveChanges();
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
                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
            },
            async context =>
            {
                var gift = await context.Set<Gift>().Include(e => e.Obscurer).SingleAsync();
                var bag = new GiftBag { Pattern = "Gold stars" };
                gift.Obscurer = bag;
                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
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
                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
            },
            async context =>
            {
                var lift = await context.Set<Lift>().Include(e => e.Obscurer).SingleAsync();
                var bag = new LiftBag { Pattern = "Gold stars" };
                lift.Obscurer = bag;
                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
            },
            async context =>
            {
                var lift = await context.Set<Lift>().Include(e => e.Obscurer).SingleAsync();

                Assert.IsType<LiftBag>(lift.Obscurer);
                Assert.Equal(lift.Id, lift.Obscurer.LiftId);
                Assert.Single(context.Set<LiftObscurer>());
            });

    [ConditionalFact]
    public virtual void Mutation_of_tracked_values_does_not_mutate_values_in_store()
    {
        var id1 = Guid.NewGuid();
        var id2 = Guid.NewGuid();
        var bytes = new byte[] { 1, 2, 3, 4 };

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.AFewBytes.AddRange(
                    new AFewBytes { Id = id1, Bytes = bytes },
                    new AFewBytes { Id = id2, Bytes = bytes });

                context.SaveChanges();
            },
            context =>
            {
                bytes[1] = 22;

                var fromStore1 = context.AFewBytes.First(p => p.Id == id1);
                var fromStore2 = context.AFewBytes.First(p => p.Id == id2);

                Assert.Equal(2, fromStore1.Bytes[1]);
                Assert.Equal(2, fromStore2.Bytes[1]);

                fromStore1.Bytes[1] = 222;
                fromStore2.Bytes[1] = 222;

                context.Entry(fromStore1).State = EntityState.Modified;

                context.SaveChanges();
            },
            context =>
            {
                var fromStore1 = context.AFewBytes.First(p => p.Id == id1);
                var fromStore2 = context.AFewBytes.First(p => p.Id == id2);

                Assert.Equal(222, fromStore1.Bytes[1]);
                Assert.Equal(2, fromStore2.Bytes[1]);
            });
    }

    [ConditionalFact]
    public virtual void Save_partial_update()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        ExecuteWithStrategyInTransaction(
            context =>
            {
                var entry = context.Products.Attach(
                    new Product { Id = productId, Price = 1.49M });

                entry.Property(c => c.Price).CurrentValue = 1.99M;
                entry.Property(p => p.Price).IsModified = true;

                Assert.False(entry.Property(p => p.DependentId).IsModified);
                Assert.False(entry.Property(p => p.Name).IsModified);

                context.SaveChanges();
            },
            context =>
            {
                var product = context.Products.First(p => p.Id == productId);

                Assert.Equal(1.99M, product.Price);
                Assert.Equal("Apple Cider", product.Name);
            });
    }

    [ConditionalFact]
    public virtual void Save_partial_update_on_missing_record_throws()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var entry = context.Products.Attach(
                    new Product { Id = new Guid("3d1302c5-4cf8-4043-9758-de9398f6fe10"), Name = "Apple Fritter" });

                entry.Property(c => c.Name).IsModified = true;

                Assert.Equal(
                    UpdateConcurrencyMessage,
                    Assert.Throws<DbUpdateConcurrencyException>(
                        () => context.SaveChanges()).Message);
            });

    [ConditionalFact]
    public virtual void Save_partial_update_on_concurrency_token_original_value_mismatch_throws()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        ExecuteWithStrategyInTransaction(
            context =>
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
                    Assert.Throws<DbUpdateConcurrencyException>(
                        () => context.SaveChanges()).Message);
            });
    }

    [ConditionalFact]
    public virtual void Update_on_bytes_concurrency_token_original_value_mismatch_throws()
    {
        var productId = Guid.NewGuid();

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                    });

                context.SaveChanges();
            },
            context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 }
                    });

                entry.Entity.Name = "GigaChips";

                Assert.Throws<DbUpdateConcurrencyException>(
                    () => context.SaveChanges());
            },
            context => Assert.Equal("MegaChips", context.ProductWithBytes.Find(productId)!.Name));
    }

    [ConditionalFact]
    public virtual void Update_on_bytes_concurrency_token_original_value_matches_does_not_throw()
    {
        var productId = Guid.NewGuid();

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                    });

                context.SaveChanges();
            },
            context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                    });

                entry.Entity.Name = "GigaChips";

                Assert.Equal(1, context.SaveChanges());
            },
            context => Assert.Equal("GigaChips", context.ProductWithBytes.Find(productId)!.Name));
    }

    [ConditionalFact]
    public virtual void Remove_on_bytes_concurrency_token_original_value_mismatch_throws()
    {
        var productId = Guid.NewGuid();

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                    });

                context.SaveChanges();
            },
            context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = new byte[] { 8, 7, 6, 5, 4, 3, 2, 1 }
                    });

                entry.State = EntityState.Deleted;

                Assert.Throws<DbUpdateConcurrencyException>(
                    () => context.SaveChanges());
            },
            context => Assert.Equal("MegaChips", context.ProductWithBytes.Find(productId)!.Name));
    }

    [ConditionalFact]
    public virtual void Remove_on_bytes_concurrency_token_original_value_matches_does_not_throw()
    {
        var productId = Guid.NewGuid();

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Add(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                    });

                context.SaveChanges();
            },
            context =>
            {
                var entry = context.ProductWithBytes.Attach(
                    new ProductWithBytes
                    {
                        Id = productId,
                        Name = "MegaChips",
                        Bytes = new byte[] { 1, 2, 3, 4, 5, 6, 7, 8 }
                    });

                entry.State = EntityState.Deleted;

                Assert.Equal(1, context.SaveChanges());
            },
            context => Assert.Null(context.ProductWithBytes.Find(productId)));
    }

    [ConditionalFact]
    public virtual void Can_add_and_remove_self_refs()
        => ExecuteWithStrategyInTransaction(
            context =>
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

                context.SaveChanges();

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

                context.SaveChanges();
            },
            context =>
            {
                var people = context.Set<Person>()
                    .Include(p => p.Parent!).ThenInclude(c => c.Parent!).ThenInclude(c => c.Parent)
                    .ToList();
                Assert.Equal(7, people.Count);
                Assert.Equal("1", people.Single(p => p.Parent == null).Name);
            });

    [ConditionalFact]
    public virtual void Can_change_enums_with_conversion()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                var person = new Person("1", null) { Address = new Address { Country = Country.Eswatini, City = "Bulembu" }, Country = "Eswatini" };

                context.Add(person);

                context.SaveChanges();
            },
            context =>
            {
                var person = context.Set<Person>().Single();
                person.Address = new Address { Country = Country.Türkiye, City = "Konya" };
                person.Country = "Türkiye";

                context.SaveChanges();
            },
            context =>
            {
                var person = context.Set<Person>().Single();

                Assert.Equal(Country.Türkiye, person.Address!.Country);
                Assert.Equal("Konya", person.Address.City);
                Assert.Equal("Türkiye", person.Country);
            });

    [ConditionalFact]
    public virtual void Can_remove_partial()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Products.Remove(
                    new Product { Id = productId, Price = 1.49M });

                context.SaveChanges();
            },
            context =>
            {
                var product = context.Products.FirstOrDefault(f => f.Id == productId);

                Assert.Null(product);
            });
    }

    [ConditionalFact]
    public virtual void Remove_partial_on_missing_record_throws()
        => ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Products.Remove(
                    new Product { Id = new Guid("3d1302c5-4cf8-4043-9758-de9398f6fe10") });

                Assert.Equal(
                    UpdateConcurrencyMessage,
                    Assert.Throws<DbUpdateConcurrencyException>(
                        () => context.SaveChanges()).Message);
            });

    [ConditionalFact]
    public virtual void Remove_partial_on_concurrency_token_original_value_mismatch_throws()
    {
        var productId = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146");

        ExecuteWithStrategyInTransaction(
            context =>
            {
                context.Products.Remove(
                    new Product
                    {
                        Id = productId, Price = 3.49M // Not the same as the value stored in the database
                    });

                Assert.Equal(
                    UpdateConcurrencyTokenMessage,
                    Assert.Throws<DbUpdateConcurrencyException>(
                        () => context.SaveChanges()).Message);
            });
    }

    [ConditionalFact]
    public virtual void Save_replaced_principal()
        => ExecuteWithStrategyInTransaction(
            context =>
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

                context.SaveChanges();
            },
            context =>
            {
                var category = context.Categories.Single();
                var products = context.Products.Where(p => p.DependentId == category.PrincipalId).ToList();

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

    protected abstract string UpdateConcurrencyMessage { get; }

    protected abstract string UpdateConcurrencyTokenMessage { get; }

    protected virtual void ExecuteWithStrategyInTransaction(
        Action<UpdatesContext> testOperation,
        Action<UpdatesContext>? nestedTestOperation1 = null,
        Action<UpdatesContext>? nestedTestOperation2 = null)
        => TestHelpers.ExecuteWithStrategyInTransaction(
            CreateContext, UseTransaction,
            testOperation, nestedTestOperation1, nestedTestOperation2);

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
            modelBuilder.Entity<Product>().HasMany(e => e.ProductCategories).WithOne()
                .HasForeignKey(e => e.ProductId);
            modelBuilder.Entity<ProductWithBytes>().HasMany(e => e.ProductCategories).WithOne()
                .HasForeignKey(e => e.ProductId);

            modelBuilder.Entity<ProductCategory>()
                .HasKey(p => new { p.CategoryId, p.ProductId });

            modelBuilder.Entity<Product>().HasOne(p => p.DefaultCategory).WithMany()
                .HasForeignKey(e => e.DependentId)
                .HasPrincipalKey(e => e.PrincipalId);

            modelBuilder.Entity<Person>()
                .HasOne(p => p.Parent)
                .WithMany()
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<Person>()
                .OwnsOne(p => p.Address)
                .Property(p => p.Country)
                .HasConversion<string>();

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

        protected override void Seed(UpdatesContext context)
            => UpdatesContext.Seed(context);
    }
}

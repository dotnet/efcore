// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable AccessToDisposedClosure
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming
// ReSharper disable AccessToModifiedClosure

namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract partial class GraphUpdatesTestBase<TFixture>
    where TFixture : GraphUpdatesTestBase<TFixture>.GraphUpdatesFixtureBase, new()
{
    [ConditionalTheory] // Issue #27299
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_insert_when_composite_FK_has_default_value_for_one_part(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var newSomething = new Something { CategoryId = 2, Name = "S" };

                if (async)
                {
                    await context.AddAsync(newSomething);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(newSomething);
                    context.SaveChanges();
                }

                var somethingOfCategoryB = new SomethingOfCategoryB { SomethingId = newSomething.Id, Name = "B" };

                if (async)
                {
                    await context.AddAsync(somethingOfCategoryB);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(somethingOfCategoryB);
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var queryable = context.Set<Something>().Include(e => e.SomethingOfCategoryB);
                var something = async ? (await queryable.SingleAsync()) : queryable.Single();

                Assert.Equal("S", something.Name);
                Assert.Equal("B", something.SomethingOfCategoryB.Name);
            });

    [ConditionalTheory] // Issue #23974
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_insert_when_FK_has_default_value(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (async)
                {
                    await context.AddAsync(new Cruiser());
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(new Cruiser());
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var queryable = context.Set<Cruiser>().Include(e => e.UserState);
                var cruiser = async ? (await queryable.SingleAsync()) : queryable.Single();
                Assert.Equal(cruiser.IdUserState, cruiser.UserState.AccessStateId);
            });

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Can_insert_when_FK_has_sentinel_value(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (async)
                {
                    await context.AddAsync(new CruiserWithSentinel { IdUserState = 667 });
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(new CruiserWithSentinel { IdUserState = 667 });
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var queryable = context.Set<CruiserWithSentinel>().Include(e => e.UserState);
                var cruiser = async ? (await queryable.SingleAsync()) : queryable.Single();
                Assert.Equal(cruiser.IdUserState, cruiser.UserState.AccessStateWithSentinelId);
            });

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual Task Can_insert_when_bool_PK_in_composite_key_has_sentinel_value(bool async, bool initialValue)
        => Can_insert_when_PK_property_in_composite_key_has_sentinel_value(async, initialValue);

    [ConditionalTheory]
    [InlineData(false, 0)]
    [InlineData(true, 0)]
    [InlineData(false, 1)]
    [InlineData(true, 1)]
    [InlineData(false, 2)]
    [InlineData(true, 2)]
    public virtual Task Can_insert_when_int_PK_in_composite_key_has_sentinel_value(bool async, int initialValue)
        => Can_insert_when_PK_property_in_composite_key_has_sentinel_value(async, initialValue);

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual Task Can_insert_when_nullable_bool_PK_in_composite_key_has_sentinel_value(bool async, bool? initialValue)
        => Can_insert_when_PK_property_in_composite_key_has_sentinel_value(async, initialValue);

    protected async Task Can_insert_when_PK_property_in_composite_key_has_sentinel_value<T>(bool async, T initialValue)
        where T : new()
    {
        var inserted = new CompositeKeyWith<T>()
        {
            SourceId = Guid.NewGuid(),
            TargetId = Guid.NewGuid(),
            PrimaryGroup = initialValue
        };

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (async)
                {
                    await context.AddAsync(inserted);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(inserted);
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var queryable = context.Set<CompositeKeyWith<T>>();
                var loaded = async ? (await queryable.SingleAsync()) : queryable.Single();
                Assert.Equal(inserted.SourceId, loaded.SourceId);
                Assert.Equal(inserted.TargetId, loaded.TargetId);
                Assert.Equal(initialValue, loaded.PrimaryGroup);
            });
    }

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual Task Throws_for_single_property_bool_key_with_default_value_generation(bool async, bool initialValue)
        => Throws_for_single_property_key_with_default_value_generation(async, initialValue);

    [ConditionalTheory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual Task Throws_for_single_property_nullable_bool_key_with_default_value_generation(bool async, bool? initialValue)
        => Throws_for_single_property_key_with_default_value_generation(async, initialValue);

    protected async Task Throws_for_single_property_key_with_default_value_generation<T>(bool async, T initialValue)
        where T : new()
    {
        var inserted = new BoolOnlyKey<T>() { PrimaryGroup = initialValue };

        await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                Assert.Equal(
                    CoreStrings.NoValueGenerator("PrimaryGroup", typeof(BoolOnlyKey<T>).ShortDisplayName(), typeof(T).ShortDisplayName()),
                    (async
                        ? (await Assert.ThrowsAsync<NotSupportedException>(async () => await context.AddAsync(inserted)))
                        : Assert.Throws<NotSupportedException>(() => context.Add(inserted))).Message);
            });
    }

    [ConditionalTheory] // Issue #23043
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Saving_multiple_modified_entities_with_the_same_key_does_not_overflow(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var city = new City { Colleges = { new College() } };

                if (async)
                {
                    await context.AddAsync(city);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(city);
                    context.SaveChanges();
                }
            }, async context =>
            {
                var city = await context.Set<City>().Include(x => x.Colleges).SingleAsync();
                var college = city.Colleges.Single();

                city.Colleges.Clear();
                city.Colleges.Add(new College { Id = college.Id });

                if (Fixture.ForceClientNoAction)
                {
                    Assert.Equal(
                        CoreStrings.RelationshipConceptualNullSensitive(nameof(City), nameof(College), $"{{CityId: {city.Id}}}"),
                        Assert.Throws<InvalidOperationException>(
                            () => context.Entry(college).State = EntityState.Modified).Message);
                }
                else
                {
                    Assert.Equal(Fixture.HasIdentityResolution ? 2 : 3, context.ChangeTracker.Entries().Count());
                    Assert.Equal(EntityState.Deleted, context.Entry(college).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(city).State);
                }
            });

    [ConditionalTheory] // Issue #22465
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Reset_unknown_original_value_when_current_value_is_set(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entityZ = new EntityZ();
                var eventZ = new EventDescriptorZ { EntityZ = entityZ };

                if (async)
                {
                    await context.AddRangeAsync(entityZ, eventZ);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.AddRange(entityZ, eventZ);
                    context.SaveChanges();
                }

                context.Entry(entityZ).State = EntityState.Detached;
                context.Entry(eventZ).State = EntityState.Detached;

                context.Entry(entityZ).State = EntityState.Deleted;
                context.Entry(eventZ).State = EntityState.Deleted;

                Assert.Same(entityZ, eventZ.EntityZ);
                Assert.Equal(entityZ.Id, context.Entry(eventZ).Property<long>("EntityZId").CurrentValue);
                Assert.Equal(entityZ.Id, context.Entry(eventZ).Property<long>("EntityZId").OriginalValue);

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                Assert.Empty(context.ChangeTracker.Entries());
            },
            async context =>
            {
                if (async)
                {
                    Assert.False(await context.Set<EventDescriptorZ>().AnyAsync());
                    Assert.False(await context.Set<EntityZ>().AnyAsync());
                }
                else
                {
                    Assert.False(context.Set<EventDescriptorZ>().Any());
                    Assert.False(context.Set<EntityZ>().Any());
                }
            });

    [ConditionalTheory] // Issue #19856
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Update_principal_with_shadow_key_owned_collection_throws(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner = new Owner { Owned = new Owned(), OwnedCollection = { new Owned(), new Owned() } };

                if (async)
                {
                    await context.AddAsync(owner);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(owner);
                    context.SaveChanges();
                }

                context.ChangeTracker.Clear();

                context.Update(owner);

                Assert.Equal(
                    CoreStrings.UnknownShadowKeyValue("Owner.OwnedCollection#Owned", "Id"),
                    (async
                        ? await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.SaveChangesAsync())
                        : Assert.Throws<InvalidOperationException>(() => context.SaveChanges())).Message);
            });

    [ConditionalTheory] // Issue #19856
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Delete_principal_with_shadow_key_owned_collection_throws(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner = new Owner { Owned = new Owned(), OwnedCollection = { new Owned(), new Owned() } };

                if (async)
                {
                    await context.AddAsync(owner);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(owner);
                    context.SaveChanges();
                }

                context.ChangeTracker.Clear();

                context.Attach(owner);
                context.Remove(owner);

                if (Fixture.ForceClientNoAction)
                {
                    if (async)
                    {
                        await Assert.ThrowsAsync<DbUpdateException>(async () => await context.SaveChangesAsync());
                    }
                    else
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                }
                else
                {
                    Assert.Equal(
                        CoreStrings.UnknownShadowKeyValue("Owner.OwnedCollection#Owned", "Id"),
                        (async
                            ? await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.SaveChangesAsync())
                            : Assert.Throws<InvalidOperationException>(() => context.SaveChanges())).Message);
                }
            });

    [ConditionalTheory] // Issue #19856
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public virtual async Task Clearing_shadow_key_owned_collection_throws(bool async, bool useUpdate, bool addNew)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner = new Owner { Owned = new Owned(), OwnedCollection = { new Owned(), new Owned() } };

                if (async)
                {
                    await context.AddAsync(owner);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(owner);
                    context.SaveChanges();
                }

                context.ChangeTracker.Clear();

                if (useUpdate)
                {
                    context.Update(owner);
                }
                else
                {
                    context.Attach(owner);
                }

                owner.OwnedCollection = addNew
                    ? [new(), new()]
                    : new List<Owned>();

                Assert.Equal(
                    CoreStrings.UnknownShadowKeyValue("Owner.OwnedCollection#Owned", "Id"),
                    (async
                        ? await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.SaveChangesAsync())
                        : Assert.Throws<InvalidOperationException>(() => context.SaveChanges())).Message);
            });

    [ConditionalTheory] // Issue #26330
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Saving_unknown_key_value_marks_it_as_unmodified(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner = new OwnerWithNonCompositeOwnedCollection();
                owner.Owned.Add(new NonCompositeOwnedCollection { Foo = "Milan" });

                if (async)
                {
                    await context.AddAsync(owner);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(owner);
                    context.SaveChanges();
                }

                owner.Owned.Remove(owner.Owned.Single());
                owner.Owned.Add(new NonCompositeOwnedCollection { Foo = "Rome" });

                if (Fixture.ForceClientNoAction)
                {
                    await Assert.ThrowsAsync<InvalidOperationException>(
                        async () =>
                            _ = async
                                ? await context.SaveChangesAsync()
                                : context.SaveChanges());
                }
                else
                {
                    _ = async
                        ? await context.SaveChangesAsync()
                        : context.SaveChanges();
                }
            },
            async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    var owner = async
                        ? await context.Set<OwnerWithNonCompositeOwnedCollection>().SingleAsync()
                        : context.Set<OwnerWithNonCompositeOwnedCollection>().Single();

                    Assert.Equal("Rome", owner.Owned.Single().Foo);
                }
            });

    [ConditionalTheory] // Issue #19856
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Update_principal_with_CLR_key_owned_collection(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner = new OwnerWithKeyedCollection
                {
                    Owned = new Owned(),
                    OwnedWithKey = new OwnedWithKey(),
                    OwnedCollection = { new OwnedWithKey(), new OwnedWithKey() },
                    OwnedCollectionPrivateKey = { new OwnedWithPrivateKey(), new OwnedWithPrivateKey() }
                };

                if (async)
                {
                    await context.AddAsync(owner);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(owner);
                    context.SaveChanges();
                }

                context.ChangeTracker.Clear();

                context.Update(owner);
                owner.Owned.Bar = "OfChocolate";
                owner.OwnedWithKey.Bar = "OfLead";
                owner.OwnedCollection.First().Bar = "OfSoap";
                owner.OwnedCollectionPrivateKey.Skip(1).First().Bar = "OfGold";

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var owner = async
                    ? await context.Set<OwnerWithKeyedCollection>().SingleAsync()
                    : context.Set<OwnerWithKeyedCollection>().Single();

                Assert.Equal("OfChocolate", owner.Owned.Bar);
                Assert.Equal("OfLead", owner.OwnedWithKey.Bar);
                Assert.Equal(2, owner.OwnedCollection.Count);
                Assert.Equal(1, owner.OwnedCollection.Count(e => e.Bar == "OfSoap"));
                Assert.Equal(2, owner.OwnedCollectionPrivateKey.Count);
                Assert.Equal(1, owner.OwnedCollectionPrivateKey.Count(e => e.Bar == "OfGold"));
            });

    [ConditionalTheory] // Issue #19856
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Delete_principal_with_CLR_key_owned_collection(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner = new OwnerWithKeyedCollection
                {
                    Owned = new Owned(),
                    OwnedWithKey = new OwnedWithKey(),
                    OwnedCollection = { new OwnedWithKey(), new OwnedWithKey() },
                    OwnedCollectionPrivateKey = { new OwnedWithPrivateKey(), new OwnedWithPrivateKey() }
                };

                if (async)
                {
                    await context.AddAsync(owner);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(owner);
                    context.SaveChanges();
                }

                context.ChangeTracker.Clear();

                context.Attach(owner);
                context.Remove(owner);

                if (Fixture.ForceClientNoAction)
                {
                    if (async)
                    {
                        await Assert.ThrowsAsync<DbUpdateException>(async () => await context.SaveChangesAsync());
                    }
                    else
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                }
                else
                {
                    if (async)
                    {
                        await context.SaveChangesAsync();
                    }
                    else
                    {
                        context.SaveChanges();
                    }
                }
            },
            async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    Assert.False(
                        async
                            ? await context.Set<OwnerWithKeyedCollection>().AnyAsync()
                            : context.Set<OwnerWithKeyedCollection>().Any());
                }
            });

    [ConditionalTheory] // Issue #19856
    [InlineData(false, false, false)]
    [InlineData(false, false, true)]
    [InlineData(false, true, false)]
    [InlineData(false, true, true)]
    [InlineData(true, false, false)]
    [InlineData(true, false, true)]
    [InlineData(true, true, false)]
    [InlineData(true, true, true)]
    public virtual async Task Clearing_CLR_key_owned_collection(bool async, bool useUpdate, bool addNew)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner = new OwnerWithKeyedCollection
                {
                    Owned = new Owned(),
                    OwnedWithKey = new OwnedWithKey(),
                    OwnedCollection = { new OwnedWithKey(), new OwnedWithKey() }
                };

                if (async)
                {
                    await context.AddAsync(owner);
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(owner);
                    context.SaveChanges();
                }

                context.ChangeTracker.Clear();

                if (useUpdate)
                {
                    context.Update(owner);
                }
                else
                {
                    context.Attach(owner);
                }

                owner.OwnedCollection = addNew
                    ? [new() { Bar = "OfGold" }, new() { Bar = "OfSoap" }]
                    : new List<OwnedWithKey>();

                owner.OwnedCollectionPrivateKey = addNew
                    ? [new() { Bar = "OfChocolate" }, new() { Bar = "OfLead" }]
                    : new List<OwnedWithPrivateKey>();

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var owner = async
                    ? await context.Set<OwnerWithKeyedCollection>().SingleAsync()
                    : context.Set<OwnerWithKeyedCollection>().Single();

                if (addNew)
                {
                    Assert.Equal(2, owner.OwnedCollection.Count);
                    Assert.Equal(1, owner.OwnedCollection.Count(e => e.Bar == "OfGold"));
                    Assert.Equal(1, owner.OwnedCollection.Count(e => e.Bar == "OfSoap"));
                    Assert.Equal(2, owner.OwnedCollectionPrivateKey.Count);
                    Assert.Equal(1, owner.OwnedCollectionPrivateKey.Count(e => e.Bar == "OfChocolate"));
                    Assert.Equal(1, owner.OwnedCollectionPrivateKey.Count(e => e.Bar == "OfLead"));
                }
                else
                {
                    Assert.False(owner.OwnedCollection.Any());
                    Assert.False(owner.OwnedCollectionPrivateKey.Any());
                }
            });

    [ConditionalTheory] // Issue #19856
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public virtual async Task Update_principal_with_non_generated_shadow_key_owned_collection_throws(bool async, bool delete)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var owner = new OwnerNoKeyGeneration { Id = 77, Owned = new OwnedNoKeyGeneration() };

                if (async)
                {
                    await context.AddAsync(owner);
                }
                else
                {
                    context.Add(owner);
                }

                context.Entry(owner.Owned).Property("OwnerNoKeyGenerationId").CurrentValue = 77;

                var owned1 = new OwnedNoKeyGeneration();
                owner.OwnedCollection.Add(owned1);
                context.ChangeTracker.DetectChanges();
                context.Entry(owned1).Property("OwnerNoKeyGenerationId").CurrentValue = 77;
                context.Entry(owned1).Property("OwnedNoKeyGenerationId").CurrentValue = 100;

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                context.ChangeTracker.Clear();

                context.Update(owner);

                if (delete)
                {
                    context.Remove(owner);
                }

                Assert.Equal(
                    CoreStrings.UnknownShadowKeyValue(
                        "OwnerNoKeyGeneration.OwnedCollection#OwnedNoKeyGeneration", "OwnedNoKeyGenerationId"),
                    (async
                        ? await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.SaveChangesAsync())
                        : Assert.Throws<InvalidOperationException>(() => context.SaveChanges())).Message);
            });

    [ConditionalTheory] // Issue #21206
    [InlineData(false)]
    [InlineData(true)]
    public async Task Discriminator_values_are_not_marked_as_unknown(bool async)
        => await ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var partner =
                    async
                        ? await context.Set<Partner>().SingleAsync()
                        : context.Set<Partner>().Single();

                var contract1 = new ProviderContract1 { Partner = partner, Details = "Provider 1 Contract Details" };
                var contract2 = new ProviderContract2 { Partner = partner, Details = "Provider 2 Contract Details" };

                if (async)
                {
                    await context.AddRangeAsync(contract1, contract2);
                }
                else
                {
                    context.AddRange(contract1, contract2);
                }

                Assert.Equal("prov1", context.Entry(contract1).Property("ProviderId").CurrentValue);
                Assert.Equal("prov2", context.Entry(contract2).Property("ProviderId").CurrentValue);

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var contracts =
                    async
                        ? await context.Set<ProviderContract>().ToListAsync()
                        : context.Set<ProviderContract>().ToList();

                Assert.Equal(2, contracts.Count);
                Assert.Equal(1, contracts.Count(e => e is ProviderContract1));
                Assert.Equal(1, contracts.Count(e => e is ProviderContract2));
            });

    [ConditionalFact]
    public virtual Task Avoid_nulling_shared_FK_property_when_deleting()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await context
                    .Set<SharedFkRoot>()
                    .Include(e => e.Parents)
                    .Include(e => e.Dependants)
                    .SingleAsync();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                var dependent = root.Dependants.Single();
                var parent = root.Parents.Single();

                Assert.Same(root, dependent.Root);
                Assert.Same(parent, dependent.Parent);
                Assert.Same(root, parent.Root);
                Assert.Same(dependent, parent.Dependant);

                Assert.Equal(root.Id, dependent.RootId);
                Assert.Equal(root.Id, parent.RootId);
                Assert.Equal(dependent.Id, parent.DependantId);

                context.Remove(dependent);

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                if (Fixture.ForceClientNoAction)
                {
                    Assert.Equal(EntityState.Unchanged, context.Entry(root).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
                    Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);

                    Assert.Same(root, dependent.Root);
                    Assert.Same(parent, dependent.Parent);
                    Assert.Same(root, parent.Root);
                    Assert.Same(dependent, parent.Dependant);

                    Assert.Equal(root.Id, dependent.RootId);
                    Assert.Equal(root.Id, parent.RootId);
                    Assert.Equal(parent.Id, parent.DependantId);

                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    Assert.Same(root, dependent.Root);
                    Assert.Same(parent, dependent.Parent);
                    Assert.Same(root, parent.Root);
                    Assert.Null(parent.Dependant);

                    Assert.Equal(root.Id, dependent.RootId);
                    Assert.Equal(root.Id, parent.RootId);
                    Assert.Null(parent.DependantId);

                    await context.SaveChangesAsync();

                    Assert.Equal(2, context.ChangeTracker.Entries().Count());

                    Assert.Equal(EntityState.Unchanged, context.Entry(root).State);
                    Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
                    Assert.Equal(EntityState.Detached, context.Entry(dependent).State);

                    Assert.Same(root, dependent.Root);
                    Assert.Same(parent, dependent.Parent);
                    Assert.Same(root, parent.Root);
                    Assert.Null(parent.Dependant);

                    Assert.Equal(root.Id, dependent.RootId);
                    Assert.Equal(root.Id, parent.RootId);
                    Assert.Null(parent.DependantId);
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction)
                {
                    var root = await context
                        .Set<SharedFkRoot>()
                        .Include(e => e.Parents)
                        .Include(e => e.Dependants)
                        .SingleAsync();

                    Assert.Equal(2, context.ChangeTracker.Entries().Count());

                    Assert.Empty(root.Dependants);
                    var parent = root.Parents.Single();

                    Assert.Same(root, parent.Root);
                    Assert.Null(parent.Dependant);

                    Assert.Equal(root.Id, parent.RootId);
                    Assert.Null(parent.DependantId);
                }
            });

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Avoid_nulling_shared_FK_property_when_nulling_navigation(bool nullPrincipal)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await context
                    .Set<SharedFkRoot>()
                    .Include(e => e.Parents)
                    .Include(e => e.Dependants)
                    .SingleAsync();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                var dependent = root.Dependants.Single();
                var parent = root.Parents.Single();

                Assert.Same(root, dependent.Root);
                Assert.Same(parent, dependent.Parent);
                Assert.Same(root, parent.Root);
                Assert.Same(dependent, parent.Dependant);

                Assert.Equal(root.Id, dependent.RootId);
                Assert.Equal(root.Id, parent.RootId);
                Assert.Equal(dependent.Id, parent.DependantId);

                if (nullPrincipal)
                {
                    dependent.Parent = null;
                }
                else
                {
                    parent.Dependant = null;
                }

                context.ChangeTracker.DetectChanges();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                Assert.Equal(EntityState.Unchanged, context.Entry(root).State);
                Assert.Equal(EntityState.Modified, context.Entry(parent).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(dependent).State);

                Assert.Same(root, dependent.Root);
                Assert.Null(dependent.Parent);
                Assert.Same(root, parent.Root);
                Assert.Null(parent.Dependant);

                Assert.Equal(root.Id, dependent.RootId);
                Assert.Equal(root.Id, parent.RootId);
                Assert.Null(parent.DependantId);

                await context.SaveChangesAsync();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                Assert.Same(root, dependent.Root);
                Assert.Null(dependent.Parent);
                Assert.Same(root, parent.Root);
                Assert.Null(parent.Dependant);

                Assert.Equal(root.Id, dependent.RootId);
                Assert.Equal(root.Id, parent.RootId);
                Assert.Null(parent.DependantId);
            }, async context =>
            {
                var root = await context
                    .Set<SharedFkRoot>()
                    .Include(e => e.Parents)
                    .Include(e => e.Dependants)
                    .SingleAsync();

                Assert.Equal(3, context.ChangeTracker.Entries().Count());

                var dependent = root.Dependants.Single();
                var parent = root.Parents.Single();

                Assert.Same(root, dependent.Root);
                Assert.Null(dependent.Parent);
                Assert.Same(root, parent.Root);
                Assert.Null(parent.Dependant);

                Assert.Equal(root.Id, dependent.RootId);
                Assert.Equal(root.Id, parent.RootId);
                Assert.Null(parent.DependantId);
            });

    [ConditionalFact]
    public virtual Task Mutating_discriminator_value_throws_by_convention()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var instance = await context.Set<OptionalSingle1Derived>().FirstAsync();

                var propertyEntry = context.Entry(instance).Property("Discriminator");

                Assert.Equal(nameof(OptionalSingle1Derived), propertyEntry.CurrentValue);

                propertyEntry.CurrentValue = nameof(OptionalSingle1MoreDerived);

                Assert.Equal(
                    CoreStrings.PropertyReadOnlyAfterSave("Discriminator", nameof(OptionalSingle1Derived)),
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => context.SaveChangesAsync())).Message);
            });

    [ConditionalFact]
    public virtual Task Mutating_discriminator_value_can_be_configured_to_allow_mutation()
    {
        var id = 0;
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var instance = await context.Set<OptionalSingle2Derived>().FirstAsync();
                var propertyEntry = context.Entry(instance).Property(e => e.Disc);
                id = instance.Id;

                Assert.IsType<OptionalSingle2Derived>(instance);
                Assert.Equal(2, propertyEntry.CurrentValue.Value);

                propertyEntry.CurrentValue = new MyDiscriminator(1);

                await context.SaveChangesAsync();
            }, async context =>
            {
                var instance = await context.Set<OptionalSingle2>().FirstAsync(e => e.Id == id);
                var propertyEntry = context.Entry(instance).Property(e => e.Disc);

                Assert.IsType<OptionalSingle2>(instance);
                Assert.Equal(1, propertyEntry.CurrentValue.Value);
            });
    }

    [ConditionalTheory]
    [InlineData((int)ChangeMechanism.Fk)]
    [InlineData((int)ChangeMechanism.Dependent)]
    [InlineData((int)(ChangeMechanism.Dependent | ChangeMechanism.Fk))]
    public virtual Task Changes_to_Added_relationships_are_picked_up(ChangeMechanism changeMechanism)
    {
        var id = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entity = new OptionalSingle1();

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    entity.RootId = 5545;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    entity.Root = new Root();
                }

                context.Add(entity);

                if ((changeMechanism & ChangeMechanism.Fk) != 0)
                {
                    entity.RootId = null;
                }

                if ((changeMechanism & ChangeMechanism.Dependent) != 0)
                {
                    entity.Root = null;
                }

                context.ChangeTracker.DetectChanges();

                Assert.Null(entity.RootId);
                Assert.Null(entity.Root);

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                id = entity.Id;
            }, async context =>
            {
                var entity = await context.Set<OptionalSingle1>().Include(e => e.Root).SingleAsync(e => e.Id == id);

                Assert.Null(entity.Root);
                Assert.Null(entity.RootId);
            });
    }

    [ConditionalTheory]
    [InlineData(false, CascadeTiming.OnSaveChanges)]
    [InlineData(false, CascadeTiming.Immediate)]
    [InlineData(false, CascadeTiming.Never)]
    [InlineData(false, null)]
    [InlineData(true, CascadeTiming.OnSaveChanges)]
    [InlineData(true, CascadeTiming.Immediate)]
    [InlineData(true, CascadeTiming.Never)]
    [InlineData(true, null)]
    public virtual Task New_FK_is_not_cleared_on_old_dependent_delete(
        bool loadNewParent,
        CascadeTiming? deleteOrphansTiming)
    {
        var removedId = 0;
        var childId = 0;
        int? newFk = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var removed = await context.Set<Optional1>().OrderBy(e => e.Id).FirstAsync();
                var child = await context.Set<Optional2>().OrderBy(e => e.Id).FirstAsync(e => e.ParentId == removed.Id);

                removedId = removed.Id;
                childId = child.Id;

                newFk = (await context.Set<Optional1>().AsNoTracking().SingleAsync(e => e.Id != removed.Id)).Id;

                var newParent = loadNewParent ? context.Set<Optional1>().Find(newFk) : null;

                child.ParentId = newFk;

                context.Remove(removed);

                Assert.True(context.ChangeTracker.HasChanges());

                if (Fixture.ForceClientNoAction)
                {
                    await Assert.ThrowsAsync<DbUpdateException>(() => context.SaveChangesAsync());
                }
                else
                {
                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(removed).State);
                    Assert.Equal(newFk, child.ParentId);

                    if (loadNewParent)
                    {
                        Assert.Same(newParent, child.Parent);
                        Assert.Contains(child, newParent.Children);
                    }
                    else
                    {
                        Assert.Null((child.Parent));
                    }
                }
            }, async context =>
            {
                if (!Fixture.ForceClientNoAction
                    && !Fixture.NoStoreCascades)
                {
                    Assert.Null(await context.Set<Optional1>().FindAsync(removedId));

                    var child = await context.Set<Optional2>().FindAsync(childId);
                    var newParent = loadNewParent ? await context.Set<Optional1>().FindAsync(newFk) : null;

                    Assert.Equal(newFk, child.ParentId);

                    if (loadNewParent)
                    {
                        Assert.Same(newParent, child.Parent);
                        Assert.Contains(child, newParent.Children);
                    }
                    else
                    {
                        Assert.Null((child.Parent));
                    }

                    Assert.False(context.ChangeTracker.HasChanges());
                }
            });
    }

    [ConditionalTheory]
    [InlineData(CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never)]
    [InlineData(null)]
    public virtual async Task No_fixup_to_Deleted_entities(
        CascadeTiming? deleteOrphansTiming)
    {
        using var context = CreateContext();
        context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

        var root = await LoadOptionalGraphAsync(context);
        var existing = root.OptionalChildren.OrderBy(e => e.Id).First();

        Assert.False(context.ChangeTracker.HasChanges());

        existing.Parent = null;
        existing.ParentId = null;
        ((ICollection<Optional1>)root.OptionalChildren).Remove(existing);

        context.Entry(existing).State = EntityState.Deleted;

        Assert.True(context.ChangeTracker.HasChanges());

        var queried = await context.Set<Optional1>().ToListAsync();

        Assert.Null(existing.Parent);
        Assert.Null(existing.ParentId);
        Assert.Single(root.OptionalChildren);
        Assert.DoesNotContain(existing, root.OptionalChildren);

        Assert.Equal(2, queried.Count);
        Assert.Contains(existing, queried);
    }

    [ConditionalFact]
    public virtual Task Notification_entities_can_have_indexes()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var produce = new Produce { Name = "Apple", BarCode = 77 };
                context.Add(produce);

                Assert.Equal(EntityState.Added, context.Entry(produce).State);

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Unchanged, context.Entry(produce).State);
                Assert.NotEqual(Guid.Empty, context.Entry(produce).Property(e => e.ProduceId).OriginalValue);
                Assert.Equal(77, context.Entry(produce).Property(e => e.BarCode).OriginalValue);

                context.Remove(produce);
                Assert.Equal(EntityState.Deleted, context.Entry(produce).State);
                Assert.NotEqual(Guid.Empty, context.Entry(produce).Property(e => e.ProduceId).OriginalValue);
                Assert.Equal(77, context.Entry(produce).Property(e => e.BarCode).OriginalValue);

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal(EntityState.Detached, context.Entry(produce).State);
            });

    [ConditionalFact]
    public virtual Task Resetting_a_deleted_reference_fixes_up_again()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var bloog = await context.Set<Bloog>().Include(e => e.Poosts).SingleAsync();
                var poost1 = bloog.Poosts.First();
                var poost2 = bloog.Poosts.Skip(1).First();

                Assert.Equal(2, bloog.Poosts.Count());
                Assert.Same(bloog, poost1.Bloog);
                Assert.Same(bloog, poost2.Bloog);

                context.Remove(bloog);

                Assert.True(context.ChangeTracker.HasChanges());

                Assert.Equal(2, bloog.Poosts.Count());

                if (Fixture.ForceClientNoAction)
                {
                    Assert.Same(bloog, poost1.Bloog);
                    Assert.Same(bloog, poost2.Bloog);
                }
                else
                {
                    Assert.Null(poost1.Bloog);
                    Assert.Null(poost2.Bloog);
                }

                poost1.Bloog = bloog;

                Assert.Equal(2, bloog.Poosts.Count());

                if (Fixture.ForceClientNoAction)
                {
                    Assert.Same(bloog, poost1.Bloog);
                    Assert.Same(bloog, poost2.Bloog);
                }
                else
                {
                    Assert.Same(bloog, poost1.Bloog);
                    Assert.Null(poost2.Bloog);
                }

                poost1.Bloog = null;

                Assert.Equal(2, bloog.Poosts.Count());

                if (Fixture.ForceClientNoAction)
                {
                    Assert.Null(poost1.Bloog);
                    Assert.Same(bloog, poost2.Bloog);
                }
                else
                {
                    Assert.Null(poost1.Bloog);
                    Assert.Null(poost2.Bloog);
                }

                if (!Fixture.ForceClientNoAction)
                {
                    Assert.True(context.ChangeTracker.HasChanges());

                    await context.SaveChangesAsync();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(2, bloog.Poosts.Count());
                    Assert.Null(poost1.Bloog);
                    Assert.Null(poost2.Bloog);
                }
            });

    [ConditionalFact]
    public virtual Task Detaching_principal_entity_will_remove_references_to_it()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await LoadOptionalGraphAsync(context);
                await LoadRequiredGraphAsync(context);
                await LoadOptionalAkGraphAsync(context);
                await LoadRequiredAkGraphAsync(context);
                await LoadRequiredCompositeGraphAsync(context);
                await LoadRequiredNonPkGraphAsync(context);
                await LoadOptionalOneToManyGraphAsync(context);
                await LoadRequiredNonPkAkGraphAsync(context);

                var optionalSingle = root.OptionalSingle;
                var requiredSingle = root.RequiredSingle;
                var optionalSingleAk = root.OptionalSingleAk;
                var optionalSingleDerived = root.OptionalSingleDerived;
                var requiredSingleAk = root.RequiredSingleAk;
                var optionalSingleAkDerived = root.OptionalSingleAkDerived;
                var optionalSingleMoreDerived = root.OptionalSingleMoreDerived;
                var requiredNonPkSingle = root.RequiredNonPkSingle;
                var optionalSingleAkMoreDerived = root.OptionalSingleAkMoreDerived;
                var requiredNonPkSingleAk = root.RequiredNonPkSingleAk;
                var requiredNonPkSingleDerived = root.RequiredNonPkSingleDerived;
                var requiredNonPkSingleAkDerived = root.RequiredNonPkSingleAkDerived;
                var requiredNonPkSingleMoreDerived = root.RequiredNonPkSingleMoreDerived;
                var requiredNonPkSingleAkMoreDerived = root.RequiredNonPkSingleAkMoreDerived;

                Assert.Same(root, optionalSingle.Root);
                Assert.Same(root, requiredSingle.Root);
                Assert.Same(root, optionalSingleAk.Root);
                Assert.Same(root, optionalSingleDerived.DerivedRoot);
                Assert.Same(root, requiredSingleAk.Root);
                Assert.Same(root, optionalSingleAkDerived.DerivedRoot);
                Assert.Same(root, optionalSingleMoreDerived.MoreDerivedRoot);
                Assert.Same(root, requiredNonPkSingle.Root);
                Assert.Same(root, optionalSingleAkMoreDerived.MoreDerivedRoot);
                Assert.Same(root, requiredNonPkSingleAk.Root);
                Assert.Same(root, requiredNonPkSingleDerived.DerivedRoot);
                Assert.Same(root, requiredNonPkSingleAkDerived.DerivedRoot);
                Assert.Same(root, requiredNonPkSingleMoreDerived.MoreDerivedRoot);
                Assert.Same(root, requiredNonPkSingleAkMoreDerived.MoreDerivedRoot);

                Assert.True(root.OptionalChildren.All(e => e.Parent == root));
                Assert.True(root.RequiredChildren.All(e => e.Parent == root));
                Assert.True(root.OptionalChildrenAk.All(e => e.Parent == root));
                Assert.True(root.RequiredChildrenAk.All(e => e.Parent == root));
                Assert.True(root.RequiredCompositeChildren.All(e => e.Parent == root));

                Assert.False(context.ChangeTracker.HasChanges());

                context.Entry(optionalSingle).State = EntityState.Detached;
                context.Entry(requiredSingle).State = EntityState.Detached;
                context.Entry(optionalSingleAk).State = EntityState.Detached;
                context.Entry(optionalSingleDerived).State = EntityState.Detached;
                context.Entry(requiredSingleAk).State = EntityState.Detached;
                context.Entry(optionalSingleAkDerived).State = EntityState.Detached;
                context.Entry(optionalSingleMoreDerived).State = EntityState.Detached;
                context.Entry(requiredNonPkSingle).State = EntityState.Detached;
                context.Entry(optionalSingleAkMoreDerived).State = EntityState.Detached;
                context.Entry(requiredNonPkSingleAk).State = EntityState.Detached;
                context.Entry(requiredNonPkSingleDerived).State = EntityState.Detached;
                context.Entry(requiredNonPkSingleAkDerived).State = EntityState.Detached;
                context.Entry(requiredNonPkSingleMoreDerived).State = EntityState.Detached;
                context.Entry(requiredNonPkSingleAkMoreDerived).State = EntityState.Detached;

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.NotNull(optionalSingle.Root);
                Assert.NotNull(requiredSingle.Root);
                Assert.NotNull(optionalSingleAk.Root);
                Assert.NotNull(optionalSingleDerived.DerivedRoot);
                Assert.NotNull(requiredSingleAk.Root);
                Assert.NotNull(optionalSingleAkDerived.DerivedRoot);
                Assert.NotNull(optionalSingleMoreDerived.MoreDerivedRoot);
                Assert.NotNull(requiredNonPkSingle.Root);
                Assert.NotNull(optionalSingleAkMoreDerived.MoreDerivedRoot);
                Assert.NotNull(requiredNonPkSingleAk.Root);
                Assert.NotNull(requiredNonPkSingleDerived.DerivedRoot);
                Assert.NotNull(requiredNonPkSingleAkDerived.DerivedRoot);
                Assert.NotNull(requiredNonPkSingleMoreDerived.MoreDerivedRoot);
                Assert.NotNull(requiredNonPkSingleAkMoreDerived.MoreDerivedRoot);

                Assert.True(root.OptionalChildren.All(e => e.Parent != null));
                Assert.True(root.RequiredChildren.All(e => e.Parent != null));
                Assert.True(root.OptionalChildrenAk.All(e => e.Parent != null));
                Assert.True(root.RequiredChildrenAk.All(e => e.Parent != null));
                Assert.True(root.RequiredCompositeChildren.All(e => e.Parent != null));
            });

    [ConditionalFact]
    public virtual Task Detaching_dependent_entity_will_not_remove_references_to_it()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var root = await LoadOptionalGraphAsync(context);
                await LoadRequiredGraphAsync(context);
                await LoadOptionalAkGraphAsync(context);
                await LoadRequiredAkGraphAsync(context);
                await LoadRequiredCompositeGraphAsync(context);
                await LoadRequiredNonPkGraphAsync(context);
                await LoadOptionalOneToManyGraphAsync(context);
                await LoadRequiredNonPkAkGraphAsync(context);

                var optionalSingle = root.OptionalSingle;
                var requiredSingle = root.RequiredSingle;
                var optionalSingleAk = root.OptionalSingleAk;
                var optionalSingleDerived = root.OptionalSingleDerived;
                var requiredSingleAk = root.RequiredSingleAk;
                var optionalSingleAkDerived = root.OptionalSingleAkDerived;
                var optionalSingleMoreDerived = root.OptionalSingleMoreDerived;
                var requiredNonPkSingle = root.RequiredNonPkSingle;
                var optionalSingleAkMoreDerived = root.OptionalSingleAkMoreDerived;
                var requiredNonPkSingleAk = root.RequiredNonPkSingleAk;
                var requiredNonPkSingleDerived = root.RequiredNonPkSingleDerived;
                var requiredNonPkSingleAkDerived = root.RequiredNonPkSingleAkDerived;
                var requiredNonPkSingleMoreDerived = root.RequiredNonPkSingleMoreDerived;
                var requiredNonPkSingleAkMoreDerived = root.RequiredNonPkSingleAkMoreDerived;

                var optionalChildren = root.OptionalChildren;
                var requiredChildren = root.RequiredChildren;
                var optionalChildrenAk = root.OptionalChildrenAk;
                var requiredChildrenAk = root.RequiredChildrenAk;
                var requiredCompositeChildren = root.RequiredCompositeChildren;
                var optionalChild = optionalChildren.First();
                var requiredChild = requiredChildren.First();
                var optionalChildAk = optionalChildrenAk.First();
                var requieredChildAk = requiredChildrenAk.First();
                var requiredCompositeChild = requiredCompositeChildren.First();

                Assert.Same(root, optionalSingle.Root);
                Assert.Same(root, requiredSingle.Root);
                Assert.Same(root, optionalSingleAk.Root);
                Assert.Same(root, optionalSingleDerived.DerivedRoot);
                Assert.Same(root, requiredSingleAk.Root);
                Assert.Same(root, optionalSingleAkDerived.DerivedRoot);
                Assert.Same(root, optionalSingleMoreDerived.MoreDerivedRoot);
                Assert.Same(root, requiredNonPkSingle.Root);
                Assert.Same(root, optionalSingleAkMoreDerived.MoreDerivedRoot);
                Assert.Same(root, requiredNonPkSingleAk.Root);
                Assert.Same(root, requiredNonPkSingleDerived.DerivedRoot);
                Assert.Same(root, requiredNonPkSingleAkDerived.DerivedRoot);
                Assert.Same(root, requiredNonPkSingleMoreDerived.MoreDerivedRoot);
                Assert.Same(root, requiredNonPkSingleAkMoreDerived.MoreDerivedRoot);

                Assert.True(optionalChildren.All(e => e.Parent == root));
                Assert.True(requiredChildren.All(e => e.Parent == root));
                Assert.True(optionalChildrenAk.All(e => e.Parent == root));
                Assert.True(requiredChildrenAk.All(e => e.Parent == root));
                Assert.True(requiredCompositeChildren.All(e => e.Parent == root));

                Assert.False(context.ChangeTracker.HasChanges());

                context.Entry(optionalSingle).State = EntityState.Detached;
                context.Entry(requiredSingle).State = EntityState.Detached;
                context.Entry(optionalSingleAk).State = EntityState.Detached;
                context.Entry(optionalSingleDerived).State = EntityState.Detached;
                context.Entry(requiredSingleAk).State = EntityState.Detached;
                context.Entry(optionalSingleAkDerived).State = EntityState.Detached;
                context.Entry(optionalSingleMoreDerived).State = EntityState.Detached;
                context.Entry(requiredNonPkSingle).State = EntityState.Detached;
                context.Entry(optionalSingleAkMoreDerived).State = EntityState.Detached;
                context.Entry(requiredNonPkSingleAk).State = EntityState.Detached;
                context.Entry(requiredNonPkSingleDerived).State = EntityState.Detached;
                context.Entry(requiredNonPkSingleAkDerived).State = EntityState.Detached;
                context.Entry(requiredNonPkSingleMoreDerived).State = EntityState.Detached;
                context.Entry(requiredNonPkSingleAkMoreDerived).State = EntityState.Detached;
                context.Entry(optionalChild).State = EntityState.Detached;
                context.Entry(requiredChild).State = EntityState.Detached;
                context.Entry(optionalChildAk).State = EntityState.Detached;
                context.Entry(requieredChildAk).State = EntityState.Detached;

                foreach (var overlappingEntry in context.ChangeTracker.Entries<OptionalOverlapping2>())
                {
                    overlappingEntry.State = EntityState.Detached;
                }

                context.Entry(requiredCompositeChild).State = EntityState.Detached;

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Same(root, optionalSingle.Root);
                Assert.Same(root, requiredSingle.Root);
                Assert.Same(root, optionalSingleAk.Root);
                Assert.Same(root, optionalSingleDerived.DerivedRoot);
                Assert.Same(root, requiredSingleAk.Root);
                Assert.Same(root, optionalSingleAkDerived.DerivedRoot);
                Assert.Same(root, optionalSingleMoreDerived.MoreDerivedRoot);
                Assert.Same(root, requiredNonPkSingle.Root);
                Assert.Same(root, optionalSingleAkMoreDerived.MoreDerivedRoot);
                Assert.Same(root, requiredNonPkSingleAk.Root);
                Assert.Same(root, requiredNonPkSingleDerived.DerivedRoot);
                Assert.Same(root, requiredNonPkSingleAkDerived.DerivedRoot);
                Assert.Same(root, requiredNonPkSingleMoreDerived.MoreDerivedRoot);
                Assert.Same(root, requiredNonPkSingleAkMoreDerived.MoreDerivedRoot);

                Assert.True(optionalChildren.All(e => e.Parent == root));
                Assert.True(requiredChildren.All(e => e.Parent == root));
                Assert.True(optionalChildrenAk.All(e => e.Parent == root));
                Assert.True(requiredChildrenAk.All(e => e.Parent == root));
                Assert.True(requiredCompositeChildren.All(e => e.Parent == root));

                Assert.NotNull(root.OptionalSingle);
                Assert.NotNull(root.RequiredSingle);
                Assert.NotNull(root.OptionalSingleAk);
                Assert.NotNull(root.OptionalSingleDerived);
                Assert.NotNull(root.RequiredSingleAk);
                Assert.NotNull(root.OptionalSingleAkDerived);
                Assert.NotNull(root.OptionalSingleMoreDerived);
                Assert.NotNull(root.RequiredNonPkSingle);
                Assert.NotNull(root.OptionalSingleAkMoreDerived);
                Assert.NotNull(root.RequiredNonPkSingleAk);
                Assert.NotNull(root.RequiredNonPkSingleDerived);
                Assert.NotNull(root.RequiredNonPkSingleAkDerived);
                Assert.NotNull(root.RequiredNonPkSingleMoreDerived);
                Assert.NotNull(root.RequiredNonPkSingleAkMoreDerived);

                Assert.Contains(optionalChild, root.OptionalChildren);
                Assert.Contains(requiredChild, root.RequiredChildren);
                Assert.Contains(optionalChildAk, root.OptionalChildrenAk);
                Assert.Contains(requieredChildAk, root.RequiredChildrenAk);
                Assert.Contains(requiredCompositeChild, root.RequiredCompositeChildren);
            });

    [ConditionalTheory]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.OnSaveChanges, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Immediate, CascadeTiming.Never)]
    [InlineData(CascadeTiming.Never, CascadeTiming.OnSaveChanges)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Immediate)]
    [InlineData(CascadeTiming.Never, CascadeTiming.Never)]
    [InlineData(null, null)]
    public virtual Task Re_childing_parent_to_new_child_with_delete(
        CascadeTiming? cascadeDeleteTiming,
        CascadeTiming? deleteOrphansTiming)
    {
        var oldId = 0;
        var newId = 0;

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                var parent = await context.Set<ParentAsAChild>().Include(p => p.ChildAsAParent).SingleAsync();

                var oldChild = parent.ChildAsAParent;
                oldId = oldChild.Id;

                context.Remove(oldChild);

                var newChild = new ChildAsAParent();
                parent.ChildAsAParent = newChild;

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                if (cascadeDeleteTiming == null)
                {
                    context.ChangeTracker.CascadeChanges();
                }

                newId = newChild.Id;
                Assert.NotEqual(newId, oldId);

                Assert.Equal(newId, parent.ChildAsAParentId);
                Assert.Same(newChild, parent.ChildAsAParent);

                Assert.Equal(EntityState.Detached, context.Entry(oldChild).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(newChild).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
            }, async context =>
            {
                var parent = await context.Set<ParentAsAChild>().Include(p => p.ChildAsAParent).SingleAsync();

                Assert.Equal(newId, parent.ChildAsAParentId);
                Assert.Equal(newId, parent.ChildAsAParent.Id);
                Assert.Null(context.Set<ChildAsAParent>().Find(oldId));
            });
    }

    [ConditionalFact]
    public virtual Task Sometimes_not_calling_DetectChanges_when_required_does_not_throw_for_null_ref()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var dependent = await context.Set<BadOrder>().SingleAsync();

                dependent.BadCustomerId = null;

                var principal = await context.Set<BadCustomer>().SingleAsync();

                principal.Status++;

                Assert.Null(dependent.BadCustomerId);
                Assert.Null(dependent.BadCustomer);
                Assert.Empty(principal.BadOrders);

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Null(dependent.BadCustomerId);
                Assert.Null(dependent.BadCustomer);
                Assert.Empty(principal.BadOrders);
            }, async context =>
            {
                var dependent = await context.Set<BadOrder>().SingleAsync();
                var principal = await context.Set<BadCustomer>().SingleAsync();

                Assert.Null(dependent.BadCustomerId);
                Assert.Null(dependent.BadCustomer);
                Assert.Empty(principal.BadOrders);
            });

    [ConditionalFact]
    public virtual Task Can_add_valid_first_dependent_when_multiple_possible_principal_sides()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var quizTask = new QuizTask();
                quizTask.Choices.Add(new TaskChoice());

                context.Add(quizTask);

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());
            }, async context =>
            {
                var quizTask = await context.Set<QuizTask>().Include(e => e.Choices).SingleAsync();

                Assert.Equal(quizTask.Id, quizTask.Choices.Single().QuestTaskId);

                Assert.Same(quizTask.Choices.Single(), await context.Set<TaskChoice>().SingleAsync());

                Assert.Empty(context.Set<HiddenAreaTask>().Include(e => e.Choices));
            });

    [ConditionalFact]
    public virtual Task Can_add_valid_second_dependent_when_multiple_possible_principal_sides()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var hiddenAreaTask = new HiddenAreaTask();
                hiddenAreaTask.Choices.Add(new TaskChoice());

                context.Add(hiddenAreaTask);

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());
            }, async context =>
            {
                var hiddenAreaTask = await context.Set<HiddenAreaTask>().Include(e => e.Choices).SingleAsync();

                Assert.Equal(hiddenAreaTask.Id, hiddenAreaTask.Choices.Single().QuestTaskId);

                Assert.Same(hiddenAreaTask.Choices.Single(), await context.Set<TaskChoice>().SingleAsync());

                Assert.Empty(context.Set<QuizTask>().Include(e => e.Choices));
            });

    [ConditionalFact]
    public virtual Task Can_add_multiple_dependents_when_multiple_possible_principal_sides()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var quizTask = new QuizTask();
                quizTask.Choices.Add(new TaskChoice());
                quizTask.Choices.Add(new TaskChoice());

                context.Add(quizTask);

                var hiddenAreaTask = new HiddenAreaTask();
                hiddenAreaTask.Choices.Add(new TaskChoice());
                hiddenAreaTask.Choices.Add(new TaskChoice());

                context.Add(hiddenAreaTask);

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());
            }, async context =>
            {
                var quizTask = await context.Set<QuizTask>().Include(e => e.Choices).SingleAsync();
                var hiddenAreaTask = await context.Set<HiddenAreaTask>().Include(e => e.Choices).SingleAsync();

                Assert.Equal(2, quizTask.Choices.Count);
                foreach (var quizTaskChoice in quizTask.Choices)
                {
                    Assert.Equal(quizTask.Id, quizTaskChoice.QuestTaskId);
                }

                Assert.Equal(2, hiddenAreaTask.Choices.Count);
                foreach (var hiddenAreaTaskChoice in hiddenAreaTask.Choices)
                {
                    Assert.Equal(hiddenAreaTask.Id, hiddenAreaTaskChoice.QuestTaskId);
                }

                foreach (var taskChoice in context.Set<TaskChoice>())
                {
                    Assert.Equal(
                        1,
                        quizTask.Choices.Count(e => e.Id == taskChoice.Id)
                        + hiddenAreaTask.Choices.Count(e => e.Id == taskChoice.Id));
                }
            });

    [ConditionalTheory] // Issue #30122
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Sever_relationship_that_will_later_be_deleted(bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var swedes = await context.Set<Parsnip>()
                    .Include(x => x.Carrot)
                    .ThenInclude(x => x.Turnips)
                    .Include(x => x.Swede)
                    .ThenInclude(x => x.TurnipSwedes)
                    .SingleAsync(x => x.Id == 1);

                swedes.Carrot.Turnips.Clear();
                swedes.Swede.TurnipSwedes.Clear();

                _ = async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges();

                var entries = context.ChangeTracker.Entries();
                Assert.Equal(3, entries.Count());
                Assert.All(entries, e => Assert.Equal(EntityState.Unchanged, e.State));
                Assert.Contains(entries, e => e.Entity.GetType() == typeof(Carrot));
                Assert.Contains(entries, e => e.Entity.GetType() == typeof(Parsnip));
                Assert.Contains(entries, e => e.Entity.GetType() == typeof(Swede));
            });

    [ConditionalFact] // Issue #32168
    public virtual Task Save_changed_owned_one_to_one()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Add(CreateOwnerRoot());
                await context.SaveChangesAsync();
            }, async context =>
            {
                var root = await context.Set<OwnerRoot>().SingleAsync();

                if (Fixture.ForceClientNoAction)
                {
                    context.Entry(root.OptionalSingle.Single).State = EntityState.Deleted;
                    context.Entry(root.OptionalSingle).State = EntityState.Deleted;
                    context.Entry(root.RequiredSingle.Single).State = EntityState.Deleted;
                    context.Entry(root.RequiredSingle).State = EntityState.Deleted;
                }

                root.OptionalSingle = new() { Name = "OS`", Single = new() { Name = "OS2`" } };
                root.RequiredSingle = new() { Name = "RS`", Single = new() { Name = "RS2`" } };

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                Assert.Equal("OS`", root.OptionalSingle.Name);
                Assert.Equal("OS2`", root.OptionalSingle.Single.Name);
                Assert.Equal("RS`", root.RequiredSingle.Name);
                Assert.Equal("RS2`", root.RequiredSingle.Single.Name);
            }, async context =>
            {
                var root = await context.Set<OwnerRoot>().SingleAsync();
                Assert.Equal("OS`", root.OptionalSingle.Name);
                Assert.Equal("OS2`", root.OptionalSingle.Single.Name);
                Assert.Equal("RS`", root.RequiredSingle.Name);
                Assert.Equal("RS2`", root.RequiredSingle.Single.Name);
            });

    [ConditionalFact]
    public virtual Task Save_changed_owned_one_to_many()
    {
        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                context.Add(CreateOwnerRoot());
                await context.SaveChangesAsync();
            }, async context =>
            {
                var root = await context.Set<OwnerRoot>().SingleAsync();
                var optionalChildren = root.OptionalChildren.Single(e => e.Name == "OC1");
                var requiredChildren = root.RequiredChildren.Single(e => e.Name == "RC1");

                if (Fixture.ForceClientNoAction)
                {
                    optionalChildren.Children.ForEach(c => context.Entry(c).State = EntityState.Deleted);
                    context.Entry(optionalChildren).State = EntityState.Deleted;
                    requiredChildren.Children.ForEach(c => context.Entry(c).State = EntityState.Deleted);
                    context.Entry(requiredChildren).State = EntityState.Deleted;
                }

                root.OptionalChildren.Remove(optionalChildren);
                root.RequiredChildren.Remove(requiredChildren);
                root.OptionalChildren.First().Children.Add(new() { Name = "OCC3" });
                root.OptionalChildren.Add(new() { Name = "OC3", Children = { new() { Name = "OCC4" }, new() { Name = "OCC5" } } });
                root.RequiredChildren.First().Children.Add(new() { Name = "RCC3" });
                root.RequiredChildren.Add(new() { Name = "RC3", Children = { new() { Name = "RCC4" }, new() { Name = "RCC5" } } });

                Assert.True(context.ChangeTracker.HasChanges());

                await context.SaveChangesAsync();

                Assert.False(context.ChangeTracker.HasChanges());

                AssertGraph(root);
            }, async context =>
            {
                var root = await context.Set<OwnerRoot>().SingleAsync();

                AssertGraph(root);
            });

        void AssertGraph(OwnerRoot ownerRoot)
        {
            Assert.Equal(2, ownerRoot.OptionalChildren.Count);
            Assert.Contains("OC2", ownerRoot.OptionalChildren.Select(e => e.Name));
            Assert.Contains("OC3", ownerRoot.OptionalChildren.Select(e => e.Name));

            var oc2Children = ownerRoot.OptionalChildren.Single(e => e.Name == "OC2").Children;
            Assert.Equal(3, oc2Children.Count);
            Assert.Contains("OCC1", oc2Children.Select(e => e.Name));
            Assert.Contains("OCC2", oc2Children.Select(e => e.Name));
            Assert.Contains("OCC3", oc2Children.Select(e => e.Name));

            var oc3Children = ownerRoot.OptionalChildren.Single(e => e.Name == "OC3").Children;
            Assert.Equal(2, oc3Children.Count);
            Assert.Contains("OCC4", oc3Children.Select(e => e.Name));
            Assert.Contains("OCC5", oc3Children.Select(e => e.Name));

            Assert.Equal(2, ownerRoot.RequiredChildren.Count);
            Assert.Contains("RC2", ownerRoot.RequiredChildren.Select(e => e.Name));
            Assert.Contains("RC3", ownerRoot.RequiredChildren.Select(e => e.Name));

            var rc2Children = ownerRoot.RequiredChildren.Single(e => e.Name == "RC2").Children;
            Assert.Equal(1, rc2Children.Count);
            Assert.Contains("RCC3", rc2Children.Select(e => e.Name));

            var rc3Children = ownerRoot.RequiredChildren.Single(e => e.Name == "RC3").Children;
            Assert.Equal(2, rc3Children.Count);
            Assert.Contains("RCC4", rc3Children.Select(e => e.Name));
            Assert.Contains("RCC5", rc3Children.Select(e => e.Name));
        }
    }

    [ConditionalTheory] // Issue #30135
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Update_root_by_collection_replacement_of_inserted_first_level(bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await PopulateGraphAsync(context);
                var newRoot = BuildNewRoot(firstLevel1: true, secondLevel1: true, thirdLevel1: true, firstLevel2: true);

                Assert.Equal(1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));

                if (await UpdateRoot(context, newRoot, async))
                {
                    Assert.Equal(
                        Fixture.HasIdentityResolution || !Fixture.AutoDetectChanges ? 1 : 2,
                        context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                }
            });

    [ConditionalTheory] // Issue #30135
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Update_root_by_collection_replacement_of_deleted_first_level(bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await PopulateGraphAsync(context);
                var newRoot = BuildNewRoot();

                Assert.Equal(1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));

                if (await UpdateRoot(context, newRoot, async))
                {
                    Assert.Equal(Fixture.AutoDetectChanges ? 0 : 1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                }
            });

    [ConditionalTheory] // Issue #30135
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Update_root_by_collection_replacement_of_inserted_second_level(bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await PopulateGraphAsync(context);
                var newRoot = BuildNewRoot(firstLevel1: true, secondLevel1: true, thirdLevel1: true, firstLevel2: true, secondLevel2: true);

                Assert.Equal(1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                Assert.Equal(1, context.Set<SecondLaw>().Count(x => x.FirstLawId == 11));

                if (await UpdateRoot(context, newRoot, async))
                {
                    if (Fixture.AutoDetectChanges)
                    {
                        Assert.Equal(Fixture.HasIdentityResolution ? 1 : 2, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                        Assert.Equal(Fixture.HasIdentityResolution ? 0 : 2, context.Set<SecondLaw>().Count(x => x.FirstLawId == 11));
                    }
                    else
                    {
                        Assert.Equal(1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                        Assert.Equal(1, context.Set<SecondLaw>().Count(x => x.FirstLawId == 11));
                    }
                }
            });

    [ConditionalTheory] // Issue #30135
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Update_root_by_collection_replacement_of_deleted_second_level(
        bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await PopulateGraphAsync(context);
                var newRoot = BuildNewRoot(firstLevel1: true);

                Assert.Equal(1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                Assert.Equal(1, context.Set<SecondLaw>().Count(x => x.FirstLawId == 11));

                if (await UpdateRoot(context, newRoot, async))
                {
                    Assert.Equal(Fixture.HasIdentityResolution ? 0 : 1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                    Assert.Equal(Fixture.AutoDetectChanges ? 0 : 1, context.Set<SecondLaw>().Count(x => x.FirstLawId == 11));
                }
            });

    [ConditionalTheory] // Issue #30135
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Update_root_by_collection_replacement_of_inserted_first_level_level(bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await PopulateGraphAsync(context);
                var newRoot = BuildNewRoot(
                    firstLevel1: true, secondLevel1: true, thirdLevel1: true, firstLevel2: true, secondLevel2: true, thirdLevel2: true);

                Assert.Equal(1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                Assert.Equal(1, context.Set<SecondLaw>().Count(x => x.FirstLawId == 11));
                Assert.Equal(1, context.Set<ThirdLaw>().Count(x => x.SecondLawId == 111));

                if (await UpdateRoot(context, newRoot, async))
                {
                    if (Fixture.AutoDetectChanges)
                    {
                        Assert.Equal(Fixture.HasIdentityResolution ? 1 : 2, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                        Assert.Equal(Fixture.HasIdentityResolution ? 0 : 2, context.Set<SecondLaw>().Count(x => x.FirstLawId == 11));
                        Assert.Equal(Fixture.HasIdentityResolution ? 0 : 2, context.Set<ThirdLaw>().Count(x => x.SecondLawId == 111));
                    }
                    else
                    {
                        Assert.Equal(1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                        Assert.Equal(1, context.Set<SecondLaw>().Count(x => x.FirstLawId == 11));
                        Assert.Equal(1, context.Set<ThirdLaw>().Count(x => x.SecondLawId == 111));
                    }
                }
            });

    [ConditionalTheory] // Issue #30135
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Update_root_by_collection_replacement_of_deleted_third_level(bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                await PopulateGraphAsync(context);
                var newRoot = BuildNewRoot(firstLevel1: true, secondLevel1: true);

                Assert.Equal(1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                Assert.Equal(1, context.Set<SecondLaw>().Count(x => x.FirstLawId == 11));
                Assert.Equal(1, context.Set<ThirdLaw>().Count(x => x.SecondLawId == 111));

                if (await UpdateRoot(context, newRoot, async))
                {
                    Assert.Equal(Fixture.HasIdentityResolution ? 0 : 1, context.Set<FirstLaw>().Count(x => x.BayazId == 1));
                    Assert.Equal(Fixture.HasIdentityResolution ? 0 : 1, context.Set<SecondLaw>().Count(x => x.FirstLawId == 11));
                    Assert.Equal(Fixture.AutoDetectChanges ? 0 : 1, context.Set<ThirdLaw>().Count(x => x.SecondLawId == 111));
                }
            });

    protected async Task<bool> UpdateRoot(DbContext context, Bayaz newRoot, bool async)
    {
        var existingRoot = await context.Set<Bayaz>()
            .Include(x => x.FirstLaw)
            .ThenInclude(x => x.SecondLaw)
            .ThenInclude(x => x.ThirdLaw)
            .SingleAsync(x => x.BayazId == newRoot.BayazId);

        existingRoot.BayazName = newRoot.BayazName;
        existingRoot.FirstLaw = newRoot.FirstLaw;

        if (Fixture.ForceClientNoAction)
        {
            Assert.Equal(
                CoreStrings.RelationshipConceptualNullSensitive(nameof(Bayaz), nameof(FirstLaw), "{BayazId: 1}"),
                (await Assert.ThrowsAsync<InvalidOperationException>(
                    async () =>
                    {
                        _ = async
                            ? await context.SaveChangesAsync()
                            : context.SaveChanges();
                    })).Message);

            return false;
        }

        _ = async
            ? await context.SaveChangesAsync()
            : context.SaveChanges();

        return true;
    }

    protected Task PopulateGraphAsync(DbContext context)
    {
        context.Add(new Bayaz { BayazId = 1, BayazName = "bayaz" });

        context.Add(
            new FirstLaw
            {
                FirstLawId = 11,
                FirstLawName = "firstLaw1",
                BayazId = 1
            });

        context.Add(
            new SecondLaw
            {
                SecondLawId = 111,
                SecondLawName = "secondLaw1",
                FirstLawId = 11
            });

        context.Add(
            new ThirdLaw
            {
                ThirdLawId = 1111,
                ThirdLawName = "thirdLaw1",
                SecondLawId = 111
            });

        return context.SaveChangesAsync();
    }

    protected Bayaz BuildNewRoot(
        bool firstLevel1 = false,
        bool firstLevel2 = false,
        bool secondLevel1 = false,
        bool secondLevel2 = false,
        bool thirdLevel1 = false,
        bool thirdLevel2 = false)
    {
        var root = new Bayaz { BayazId = 1, BayazName = "bayaz" };

        if (firstLevel1)
        {
            root.FirstLaw.Add(AddFirstLevel(secondLevel1, secondLevel2, thirdLevel1, thirdLevel2));
        }

        if (firstLevel2)
        {
            root.FirstLaw.Add(
                new FirstLaw
                {
                    FirstLawId = 12,
                    FirstLawName = "firstLaw2",
                    BayazId = 1
                });
        }

        return root;
    }

    private FirstLaw AddFirstLevel(bool secondLevel1, bool secondLevel2, bool thirdLevel1, bool thirdLevel2)
    {
        var firstLevel = new FirstLaw
        {
            FirstLawId = 11,
            FirstLawName = "firstLaw1",
            BayazId = 1
        };

        if (secondLevel1)
        {
            firstLevel.SecondLaw.Add(AddSecondLevel(thirdLevel1, thirdLevel2));
        }

        if (secondLevel2)
        {
            firstLevel.SecondLaw.Add(
                new SecondLaw
                {
                    SecondLawId = 112,
                    SecondLawName = "secondLaw2",
                    FirstLawId = 11
                });
        }

        return firstLevel;
    }

    private static SecondLaw AddSecondLevel(bool thirdLevel1, bool thirdLevel2)
    {
        var secondLevel = new SecondLaw
        {
            SecondLawId = 111,
            SecondLawName = "secondLaw1",
            FirstLawId = 11
        };

        if (thirdLevel1)
        {
            secondLevel.ThirdLaw.Add(
                new ThirdLaw
                {
                    ThirdLawId = 1111,
                    ThirdLawName = "thirdLaw1",
                    SecondLawId = 111
                });
        }

        if (thirdLevel2)
        {
            secondLevel.ThirdLaw.Add(
                new ThirdLaw
                {
                    ThirdLawId = 1112,
                    ThirdLawName = "thirdLaw2",
                    SecondLawId = 111
                });
        }

        return secondLevel;
    }

    [ConditionalTheory] // Issue #28961 and Issue #32385
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Alternate_key_over_foreign_key_doesnt_bypass_delete_behavior(bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var parent = new NaiveParent { Children = { new SneakyChild() } };
                context.Add(parent);

                _ = async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges();

                Assert.Equal(2, context.ChangeTracker.Entries().Count());

                parent.Children.Remove(parent.Children.First());
                _ = async
                    ? await context.SaveChangesAsync()
                    : context.SaveChanges();

                Assert.Equal(1, context.ChangeTracker.Entries().Count());
            });

    [ConditionalTheory] // Issue #30764
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Shadow_skip_navigation_in_base_class_is_handled(bool async)
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var entities = async
                    ? await context.Set<Lettuce2>().ToListAsync()
                    : context.Set<Lettuce2>().ToList();

                Assert.Equal(1, entities.Count);
                Assert.Equal(nameof(Lettuce2), context.Entry(entities[0]).Property<string>("Discriminator").CurrentValue);
            });

    [ConditionalTheory] // Issue #32084
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual Task Mark_explicitly_set_dependent_appropriately_with_any_inheritance_and_stable_generator(bool async, bool useAdd)
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (async)
                {
                    await context.AddAsync(new ParentEntity32084 { Id = parentId });
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(new ParentEntity32084 { Id = parentId });
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var parent = async
                    ? await context.FindAsync<ParentEntity32084>(parentId)
                    : context.Find<ParentEntity32084>(parentId);

                var child = new ChildEntity32084
                {
                    Id = childId,
                    ParentId = parent!.Id,
                    ChildValue = "test value"
                };

                if (useAdd)
                {
                    _ = async ? await context.AddAsync(child) : context.Add(child);
                }
                else
                {
                    parent.Child = child;
                    context.ChangeTracker.DetectChanges();
                }

                Assert.Equal(2, context.ChangeTracker.Entries().Count());
                Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);

                if (useAdd) // If we call Add explicitly, then the key value is forced to Added
                {
                    Assert.Equal(EntityState.Added, context.Entry(child).State);
                    _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
                }
                else
                {
                    Assert.Equal(EntityState.Modified, context.Entry(child).State);
                    await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
                        async () => _ = async ? await context.SaveChangesAsync() : context.SaveChanges());
                }
            });
    }

    [ConditionalTheory] // Issue #32084
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual Task Mark_explicitly_set_stable_dependent_appropriately(bool async, bool useAdd)
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (async)
                {
                    await context.AddAsync(new StableParent32084 { Id = parentId });
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(new StableParent32084 { Id = parentId });
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var parent = async
                    ? await context.FindAsync<StableParent32084>(parentId)
                    : context.Find<StableParent32084>(parentId);

                var child = new StableChild32084()
                {
                    Id = childId, ParentId = parent!.Id,
                };

                if (useAdd)
                {
                    _ = async ? await context.AddAsync(child) : context.Add(child);
                }
                else
                {
                    parent.Child = child;
                    context.ChangeTracker.DetectChanges();
                }

                Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
                Assert.Equal(2, context.ChangeTracker.Entries().Count());
                Assert.Equal(EntityState.Added, context.Entry(child).State);

                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
            });
    }

    [ConditionalTheory] // Issue #32084
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    [InlineData(true, true)]
    public virtual Task Mark_explicitly_set_stable_dependent_appropriately_when_deep_in_graph(bool async, bool useAdd)
    {
        var parentId = Guid.NewGuid();
        var childId = Guid.NewGuid();
        var brotherId = Guid.NewGuid();

        return ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                if (async)
                {
                    await context.AddAsync(new SneakyUncle32084 { Id = brotherId });
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.Add(new SneakyUncle32084 { Id = brotherId });
                    context.SaveChanges();
                }
            },
            async context =>
            {
                var brother = async
                    ? (await context.FindAsync<SneakyUncle32084>(brotherId))!
                    : context.Find<SneakyUncle32084>(brotherId)!;

                var child = new StableChild32084 { Id = childId };
                var parent = new StableParent32084 { Id = parentId, Child = child };

                if (useAdd)
                {
                    brother.BrotherId = parentId;
                    _ = async ? await context.AddAsync(parent) : context.Add(parent);
                }
                else
                {
                    brother.Brother = parent;
                    context.ChangeTracker.DetectChanges();
                }

                Assert.Equal(3, context.ChangeTracker.Entries().Count());
                Assert.Equal(EntityState.Modified, context.Entry(brother).State);
                Assert.Equal(EntityState.Added, context.Entry(parent).State);
                Assert.Equal(EntityState.Added, context.Entry(child).State);

                _ = async ? await context.SaveChangesAsync() : context.SaveChanges();
            });
    }
}

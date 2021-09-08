// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Xunit;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable PossibleMultipleEnumeration
// ReSharper disable InconsistentNaming
// ReSharper disable AccessToModifiedClosure
namespace Microsoft.EntityFrameworkCore
{
    public abstract partial class GraphUpdatesTestBase<TFixture>
        where TFixture : GraphUpdatesTestBase<TFixture>.GraphUpdatesFixtureBase, new()
    {
        [ConditionalTheory] // Issue #23043
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Saving_multiple_modified_entities_with_the_same_key_does_not_overflow(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var city = new City
                    {
                        Colleges =
                        {
                            new()
                        }
                    };

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
                },
                context =>
                {
                    var city = context.Set<City>().Include(x => x.Colleges).Single();
                    var college = city.Colleges.Single();

                    city.Colleges.Clear();
                    city.Colleges.Add(new() { Id = college.Id });

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Equal(
                            CoreStrings.RelationshipConceptualNullSensitive(nameof(City), nameof(College), $"{{CityId: {city.Id}}}"),
                            Assert.Throws<InvalidOperationException>(
                                () => context.Entry(college).State = EntityState.Modified).Message);
                    }
                    else
                    {
                        Assert.Equal(
                            CoreStrings.IdentityConflictSensitive(nameof(College), $"{{Id: {college.Id}}}"),
                            Assert.Throws<InvalidOperationException>(
                                () => context.Entry(college).State = EntityState.Modified).Message);
                    }

                    return Task.CompletedTask;
                });
        }

        [ConditionalTheory] // Issue #22465
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Reset_unknown_original_value_when_current_value_is_set(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var entityZ = new EntityZ();
                    var eventZ = new EventDescriptorZ {EntityZ = entityZ};

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
        }

        [ConditionalTheory] // Issue #19856
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Update_principal_with_shadow_key_owned_collection_throws(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var owner = new Owner { Owned = new(), OwnedCollection = { new(), new() } };

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
        }

        [ConditionalTheory] // Issue #19856
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Delete_principal_with_shadow_key_owned_collection_throws(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var owner = new Owner
                    {
                        Owned = new(),
                        OwnedCollection = { new(), new() }
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
                        Assert.Equal(
                            CoreStrings.UnknownShadowKeyValue("Owner.OwnedCollection#Owned", "Id"),
                            (async
                                ? await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.SaveChangesAsync())
                                : Assert.Throws<InvalidOperationException>(() => context.SaveChanges())).Message);
                    }
                });
        }

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
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var owner = new Owner
                    {
                        Owned = new(),
                        OwnedCollection = { new(), new() }
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
                        ? new List<Owned> { new(), new() }
                        : new List<Owned>();

                    Assert.Equal(
                        CoreStrings.UnknownShadowKeyValue("Owner.OwnedCollection#Owned", "Id"),
                        (async
                            ? await Assert.ThrowsAsync<InvalidOperationException>(async () => await context.SaveChangesAsync())
                            : Assert.Throws<InvalidOperationException>(() => context.SaveChanges())).Message);
                });
        }

        [ConditionalTheory] // Issue #19856
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Update_principal_with_CLR_key_owned_collection(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var owner = new OwnerWithKeyedCollection
                    {
                        Owned = new(),
                        OwnedWithKey = new(),
                        OwnedCollection = { new(), new() },
                        OwnedCollectionPrivateKey = { new(), new() }
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
        }

        [ConditionalTheory] // Issue #19856
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Delete_principal_with_CLR_key_owned_collection(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var owner = new OwnerWithKeyedCollection
                    {
                        Owned = new(),
                        OwnedWithKey = new(),
                        OwnedCollection = { new(), new() },
                        OwnedCollectionPrivateKey = { new(), new() }
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
        }

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
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var owner = new OwnerWithKeyedCollection
                    {
                        Owned = new(),
                        OwnedWithKey = new(),
                        OwnedCollection = { new(), new() }
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
                        ? new List<OwnedWithKey> { new() { Bar = "OfGold" }, new() { Bar = "OfSoap" } }
                        : new List<OwnedWithKey>();

                    owner.OwnedCollectionPrivateKey = addNew
                        ? new List<OwnedWithPrivateKey> { new() { Bar = "OfChocolate" }, new() { Bar = "OfLead" } }
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
        }

        [ConditionalTheory] // Issue #19856
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public virtual async Task Update_principal_with_non_generated_shadow_key_owned_collection_throws(bool async, bool delete)
        {
            await ExecuteWithStrategyInTransactionAsync(
                async context =>
                {
                    var owner = new OwnerNoKeyGeneration { Id = 77, Owned = new() };

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
        }

        [ConditionalTheory] // Issue #21206
        [InlineData(false)]
        [InlineData(true)]
        public async Task Discriminator_values_are_not_marked_as_unknown(bool async)
        {
            await ExecuteWithStrategyInTransactionAsync(
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
        }

        [ConditionalFact]
        public virtual void Avoid_nulling_shared_FK_property_when_deleting()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = context
                        .Set<SharedFkRoot>()
                        .Include(e => e.Parents)
                        .Include(e => e.Dependants)
                        .Single();

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

                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
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

                        context.SaveChanges();

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
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction)
                    {
                        var root = context
                            .Set<SharedFkRoot>()
                            .Include(e => e.Parents)
                            .Include(e => e.Dependants)
                            .Single();

                        Assert.Equal(2, context.ChangeTracker.Entries().Count());

                        Assert.Empty(root.Dependants);
                        var parent = root.Parents.Single();

                        Assert.Same(root, parent.Root);
                        Assert.Null(parent.Dependant);

                        Assert.Equal(root.Id, parent.RootId);
                        Assert.Null(parent.DependantId);
                    }
                });
        }

        [ConditionalTheory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual void Avoid_nulling_shared_FK_property_when_nulling_navigation(bool nullPrincipal)
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = context
                        .Set<SharedFkRoot>()
                        .Include(e => e.Parents)
                        .Include(e => e.Dependants)
                        .Single();

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

                    context.SaveChanges();

                    Assert.Equal(3, context.ChangeTracker.Entries().Count());

                    Assert.Same(root, dependent.Root);
                    Assert.Null(dependent.Parent);
                    Assert.Same(root, parent.Root);
                    Assert.Null(parent.Dependant);

                    Assert.Equal(root.Id, dependent.RootId);
                    Assert.Equal(root.Id, parent.RootId);
                    Assert.Null(parent.DependantId);
                },
                context =>
                {
                    var root = context
                        .Set<SharedFkRoot>()
                        .Include(e => e.Parents)
                        .Include(e => e.Dependants)
                        .Single();

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
        }

        [ConditionalFact]
        public virtual void Mutating_discriminator_value_throws_by_convention()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var instance = context.Set<OptionalSingle1Derived>().First();

                    var propertyEntry = context.Entry(instance).Property("Discriminator");

                    Assert.Equal(nameof(OptionalSingle1Derived), propertyEntry.CurrentValue);

                    propertyEntry.CurrentValue = nameof(OptionalSingle1MoreDerived);

                    Assert.Equal(
                        CoreStrings.PropertyReadOnlyAfterSave("Discriminator", nameof(OptionalSingle1Derived)),
                        Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message);
                });
        }

        [ConditionalFact]
        public virtual void Mutating_discriminator_value_can_be_configured_to_allow_mutation()
        {
            var id = 0;
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var instance = context.Set<OptionalSingle2Derived>().First();
                    var propertyEntry = context.Entry(instance).Property(e => e.Disc);
                    id = instance.Id;

                    Assert.IsType<OptionalSingle2Derived>(instance);
                    Assert.Equal(2, propertyEntry.CurrentValue.Value);

                    propertyEntry.CurrentValue = new MyDiscriminator(1);

                    context.SaveChanges();
                },
                context =>
                {
                    var instance = context.Set<OptionalSingle2>().First(e => e.Id == id);
                    var propertyEntry = context.Entry(instance).Property(e => e.Disc);

                    Assert.IsType<OptionalSingle2>(instance);
                    Assert.Equal(1, propertyEntry.CurrentValue.Value);
                });
        }

        [ConditionalTheory]
        [InlineData((int)ChangeMechanism.Fk)]
        [InlineData((int)ChangeMechanism.Dependent)]
        [InlineData((int)(ChangeMechanism.Dependent | ChangeMechanism.Fk))]
        public virtual void Changes_to_Added_relationships_are_picked_up(ChangeMechanism changeMechanism)
        {
            var id = 0;

            ExecuteWithStrategyInTransaction(
                context =>
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

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    id = entity.Id;
                },
                context =>
                {
                    var entity = context.Set<OptionalSingle1>().Include(e => e.Root).Single(e => e.Id == id);

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
        public virtual void New_FK_is_not_cleared_on_old_dependent_delete(
            bool loadNewParent,
            CascadeTiming? deleteOrphansTiming)
        {
            var removedId = 0;
            var childId = 0;
            int? newFk = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var removed = context.Set<Optional1>().OrderBy(e => e.Id).First();
                    var child = context.Set<Optional2>().OrderBy(e => e.Id).First(e => e.ParentId == removed.Id);

                    removedId = removed.Id;
                    childId = child.Id;

                    newFk = context.Set<Optional1>().AsNoTracking().Single(e => e.Id != removed.Id).Id;

                    var newParent = loadNewParent ? context.Set<Optional1>().Find(newFk) : null;

                    child.ParentId = newFk;

                    context.Remove(removed);

                    Assert.True(context.ChangeTracker.HasChanges());

                    if (Fixture.ForceClientNoAction)
                    {
                        Assert.Throws<DbUpdateException>(() => context.SaveChanges());
                    }
                    else
                    {
                        context.SaveChanges();

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
                },
                context =>
                {
                    if (!Fixture.ForceClientNoAction
                        && !Fixture.NoStoreCascades)
                    {
                        Assert.Null(context.Set<Optional1>().Find(removedId));

                        var child = context.Set<Optional2>().Find(childId);
                        var newParent = loadNewParent ? context.Set<Optional1>().Find(newFk) : null;

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
        public virtual void No_fixup_to_Deleted_entities(
            CascadeTiming? deleteOrphansTiming)
        {
            using var context = CreateContext();
            context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

            var root = LoadOptionalGraph(context);
            var existing = root.OptionalChildren.OrderBy(e => e.Id).First();

            Assert.False(context.ChangeTracker.HasChanges());

            existing.Parent = null;
            existing.ParentId = null;
            ((ICollection<Optional1>)root.OptionalChildren).Remove(existing);

            context.Entry(existing).State = EntityState.Deleted;

            Assert.True(context.ChangeTracker.HasChanges());

            var queried = context.Set<Optional1>().ToList();

            Assert.Null(existing.Parent);
            Assert.Null(existing.ParentId);
            Assert.Single(root.OptionalChildren);
            Assert.DoesNotContain(existing, root.OptionalChildren);

            Assert.Equal(2, queried.Count);
            Assert.Contains(existing, queried);
        }

        [ConditionalFact]
        public virtual void Notification_entities_can_have_indexes()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var produce = new Produce { Name = "Apple", BarCode = 77 };
                    context.Add(produce);

                    Assert.Equal(EntityState.Added, context.Entry(produce).State);

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Unchanged, context.Entry(produce).State);
                    Assert.NotEqual(Guid.Empty, context.Entry(produce).Property(e => e.ProduceId).OriginalValue);
                    Assert.Equal(77, context.Entry(produce).Property(e => e.BarCode).OriginalValue);

                    context.Remove(produce);
                    Assert.Equal(EntityState.Deleted, context.Entry(produce).State);
                    Assert.NotEqual(Guid.Empty, context.Entry(produce).Property(e => e.ProduceId).OriginalValue);
                    Assert.Equal(77, context.Entry(produce).Property(e => e.BarCode).OriginalValue);

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Equal(EntityState.Detached, context.Entry(produce).State);
                });
        }

        [ConditionalFact]
        public virtual void Resetting_a_deleted_reference_fixes_up_again()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var bloog = context.Set<Bloog>().Include(e => e.Poosts).Single();
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

                        context.SaveChanges();

                        Assert.False(context.ChangeTracker.HasChanges());

                        Assert.Equal(2, bloog.Poosts.Count());
                        Assert.Null(poost1.Bloog);
                        Assert.Null(poost2.Bloog);
                    }
                });
        }

        [ConditionalFact]
        public virtual void Detaching_principal_entity_will_remove_references_to_it()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadOptionalGraph(context);
                    LoadRequiredGraph(context);
                    LoadOptionalAkGraph(context);
                    LoadRequiredAkGraph(context);
                    LoadRequiredCompositeGraph(context);
                    LoadRequiredNonPkGraph(context);
                    LoadOptionalOneToManyGraph(context);
                    LoadRequiredNonPkAkGraph(context);

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
        }

        [ConditionalFact]
        public virtual void Detaching_dependent_entity_will_not_remove_references_to_it()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var root = LoadOptionalGraph(context);
                    LoadRequiredGraph(context);
                    LoadOptionalAkGraph(context);
                    LoadRequiredAkGraph(context);
                    LoadRequiredCompositeGraph(context);
                    LoadRequiredNonPkGraph(context);
                    LoadOptionalOneToManyGraph(context);
                    LoadRequiredNonPkAkGraph(context);

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
        }

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
        public virtual void Re_childing_parent_to_new_child_with_delete(
            CascadeTiming? cascadeDeleteTiming,
            CascadeTiming? deleteOrphansTiming)
        {
            var oldId = 0;
            var newId = 0;

            ExecuteWithStrategyInTransaction(
                context =>
                {
                    context.ChangeTracker.CascadeDeleteTiming = cascadeDeleteTiming ?? CascadeTiming.Never;
                    context.ChangeTracker.DeleteOrphansTiming = deleteOrphansTiming ?? CascadeTiming.Never;

                    var parent = context.Set<ParentAsAChild>().Include(p => p.ChildAsAParent).Single();

                    var oldChild = parent.ChildAsAParent;
                    oldId = oldChild.Id;

                    context.Remove(oldChild);

                    var newChild = new ChildAsAParent();
                    parent.ChildAsAParent = newChild;

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

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
                },
                context =>
                {
                    var parent = context.Set<ParentAsAChild>().Include(p => p.ChildAsAParent).Single();

                    Assert.Equal(newId, parent.ChildAsAParentId);
                    Assert.Equal(newId, parent.ChildAsAParent.Id);
                    Assert.Null(context.Set<ChildAsAParent>().Find(oldId));
                });
        }

        [ConditionalFact]
        public virtual void Sometimes_not_calling_DetectChanges_when_required_does_not_throw_for_null_ref()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var dependent = context.Set<BadOrder>().Single();

                    dependent.BadCustomerId = null;

                    var principal = context.Set<BadCustomer>().Single();

                    principal.Status++;

                    Assert.Null(dependent.BadCustomerId);
                    Assert.Null(dependent.BadCustomer);
                    Assert.Empty(principal.BadOrders);

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());

                    Assert.Null(dependent.BadCustomerId);
                    Assert.Null(dependent.BadCustomer);
                    Assert.Empty(principal.BadOrders);
                },
                context =>
                {
                    var dependent = context.Set<BadOrder>().Single();
                    var principal = context.Set<BadCustomer>().Single();

                    Assert.Null(dependent.BadCustomerId);
                    Assert.Null(dependent.BadCustomer);
                    Assert.Empty(principal.BadOrders);
                });
        }

        [ConditionalFact]
        public virtual void Can_add_valid_first_dependent_when_multiple_possible_principal_sides()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var quizTask = new QuizTask();
                    quizTask.Choices.Add(new TaskChoice());

                    context.Add(quizTask);

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());
                },
                context =>
                {
                    var quizTask = context.Set<QuizTask>().Include(e => e.Choices).Single();

                    Assert.Equal(quizTask.Id, quizTask.Choices.Single().QuestTaskId);

                    Assert.Same(quizTask.Choices.Single(), context.Set<TaskChoice>().Single());

                    Assert.Empty(context.Set<HiddenAreaTask>().Include(e => e.Choices));
                });
        }

        [ConditionalFact]
        public virtual void Can_add_valid_second_dependent_when_multiple_possible_principal_sides()
        {
            ExecuteWithStrategyInTransaction(
                context =>
                {
                    var hiddenAreaTask = new HiddenAreaTask();
                    hiddenAreaTask.Choices.Add(new TaskChoice());

                    context.Add(hiddenAreaTask);

                    Assert.True(context.ChangeTracker.HasChanges());

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());
                },
                context =>
                {
                    var hiddenAreaTask = context.Set<HiddenAreaTask>().Include(e => e.Choices).Single();

                    Assert.Equal(hiddenAreaTask.Id, hiddenAreaTask.Choices.Single().QuestTaskId);

                    Assert.Same(hiddenAreaTask.Choices.Single(), context.Set<TaskChoice>().Single());

                    Assert.Empty(context.Set<QuizTask>().Include(e => e.Choices));
                });
        }

        [ConditionalFact]
        public virtual void Can_add_multiple_dependents_when_multiple_possible_principal_sides()
        {
            ExecuteWithStrategyInTransaction(
                context =>
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

                    context.SaveChanges();

                    Assert.False(context.ChangeTracker.HasChanges());
                },
                context =>
                {
                    var quizTask = context.Set<QuizTask>().Include(e => e.Choices).Single();
                    var hiddenAreaTask = context.Set<HiddenAreaTask>().Include(e => e.Choices).Single();

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
        }
    }
}

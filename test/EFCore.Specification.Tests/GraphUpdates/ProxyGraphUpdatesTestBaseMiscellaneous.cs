// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
// ReSharper disable AccessToModifiedClosure
// ReSharper disable PossibleMultipleEnumeration

namespace Microsoft.EntityFrameworkCore;

public abstract partial class ProxyGraphUpdatesTestBase<TFixture>
    where TFixture : ProxyGraphUpdatesTestBase<TFixture>.ProxyGraphUpdatesFixtureBase, new()
{
    [ConditionalFact]
    public virtual Task Save_two_entity_cycle_with_lazy_loading()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                context.AddRange(
                    context.CreateProxy<Car>(
                        car =>
                        {
                            car.Owner = context.CreateProxy<Person>();
                            car.Id = Guid.NewGuid();
                        }),
                    context.CreateProxy<Car>(
                        car =>
                        {
                            car.Owner = context.CreateProxy<Person>();
                            car.Id = Guid.NewGuid();
                        }));

                context.SaveChanges();
                return Task.CompletedTask;
            },
            context =>
            {
                var cars = context.Set<Car>().ToList();

                var owner0 = cars[0].Owner;
                var owner1 = cars[1].Owner;

                (cars[1].Owner, cars[0].Owner) = (cars[0].Owner, cars[1].Owner);

                cars[0].Owner.Vehicle = cars[0];
                cars[1].Owner.Vehicle = cars[1];

                if (context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory")
                {
                    context.SaveChanges();
                    Assert.Same(owner0, cars[1].Owner);
                    Assert.Same(owner1, cars[0].Owner);
                    Assert.Same(cars[0], cars[0].Owner.Vehicle);
                    Assert.Same(cars[1], cars[1].Owner.Vehicle);
                }
                else
                {
                    var message = Assert.Throws<InvalidOperationException>(() => context.SaveChanges()).Message;
                    Assert.StartsWith(CoreStrings.CircularDependency("").Substring(0, 30), message);
                }

                return Task.CompletedTask;
            });

    [ConditionalFact]
    public virtual Task Can_use_record_proxies_with_base_types_to_load_reference()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                context.AddRange(
                    context.CreateProxy<RecordCar>(
                        car =>
                        {
                            car.Owner = context.CreateProxy<RecordPerson>();
                        }));

                context.SaveChanges();
                return Task.CompletedTask;
            },
            context =>
            {
                var car = context.Set<RecordCar>().Single();
                if (!DoesLazyLoading)
                {
                    context.Entry(car).Reference(e => e.Owner).Load();
                }

                Assert.Equal(car.Owner.Id, car.OwnerId);
                Assert.Same(car, car.Owner.Vehicles.Single());
                return Task.CompletedTask;
            });

    [ConditionalFact]
    public virtual Task Can_use_record_proxies_with_base_types_to_load_collection()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                context.AddRange(
                    context.CreateProxy<RecordCar>(
                        car =>
                        {
                            car.Owner = context.CreateProxy<RecordPerson>();
                        }));

                context.SaveChanges();
                return Task.CompletedTask;
            },
            context =>
            {
                var owner = context.Set<RecordPerson>().Single();
                if (!DoesLazyLoading)
                {
                    context.Entry(owner).Collection(e => e.Vehicles).Load();
                }

                Assert.Equal(owner.Id, owner.Vehicles.Single().Id);
                Assert.Same(owner, owner.Vehicles.Single().Owner);
                return Task.CompletedTask;
            });

    [ConditionalFact]
    public virtual Task Avoid_nulling_shared_FK_property_when_deleting()
        => ExecuteWithStrategyInTransactionAsync(
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

                Assert.Equal(EntityState.Unchanged, context.Entry(root).State);
                Assert.Equal(EntityState.Modified, context.Entry(parent).State);
                Assert.Equal(EntityState.Deleted, context.Entry(dependent).State);

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
                return Task.CompletedTask;
            },
            context =>
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
                return Task.CompletedTask;
            });

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual Task Avoid_nulling_shared_FK_property_when_nulling_navigation(bool nullPrincipal)
        => ExecuteWithStrategyInTransactionAsync(
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
                return Task.CompletedTask;
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
                return Task.CompletedTask;
            });

    [ConditionalFact]
    public virtual async Task No_fixup_to_Deleted_entities()
    {
        using var context = CreateContext();

        var root = await LoadRootAsync(context);
        if (!DoesLazyLoading)
        {
            context.Entry(root).Collection(e => e.OptionalChildren).Load();
        }

        var existing = root.OptionalChildren.OrderBy(e => e.Id).First();

        existing.Parent = null;
        existing.ParentId = null;
        ((ICollection<Optional1>)root.OptionalChildren).Remove(existing);

        context.Entry(existing).State = EntityState.Deleted;

        var queried = context.Set<Optional1>().ToList();

        Assert.Null(existing.Parent);
        Assert.Null(existing.ParentId);
        Assert.Single(root.OptionalChildren);
        Assert.DoesNotContain(existing, root.OptionalChildren);

        Assert.Equal(2, queried.Count);
        Assert.Contains(existing, queried);
    }

    [ConditionalFact]
    public virtual Task Sometimes_not_calling_DetectChanges_when_required_does_not_throw_for_null_ref()
        => ExecuteWithStrategyInTransactionAsync(
            context =>
            {
                var dependent = context.Set<BadOrder>().Single();

                dependent.BadCustomerId = null;

                var principal = context.Set<BadCustomer>().Single();

                principal.Status++;

                Assert.Null(dependent.BadCustomerId);
                Assert.Null(dependent.BadCustomer);
                Assert.Empty(principal.BadOrders);

                context.SaveChanges();

                Assert.Null(dependent.BadCustomerId);
                Assert.Null(dependent.BadCustomer);
                Assert.Empty(principal.BadOrders);
                return Task.CompletedTask;
            },
            context =>
            {
                var dependent = context.Set<BadOrder>().Single();
                var principal = context.Set<BadCustomer>().Single();

                Assert.Null(dependent.BadCustomerId);
                Assert.Null(dependent.BadCustomer);
                Assert.Empty(principal.BadOrders);
                return Task.CompletedTask;
            });

    [ConditionalFact]
    public virtual Task Can_attach_full_required_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadRequiredGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryRequiredGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public virtual Task Can_attach_full_optional_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadOptionalGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryOptionalGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public virtual Task Can_attach_full_required_non_PK_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadRequiredNonPkGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryRequiredNonPkGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public virtual Task Can_attach_full_required_AK_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadRequiredAkGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryRequiredAkGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public virtual Task Can_attach_full_optional_AK_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadOptionalAkGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryOptionalAkGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public virtual Task Can_attach_full_required_non_PK_AK_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadRequiredNonPkAkGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryRequiredNonPkAkGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public virtual Task Can_attach_full_required_one_to_many_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadOptionalOneToManyGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryOptionalOneToManyGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Equal(0, context.SaveChanges());
            });

    [ConditionalFact]
    public virtual Task Can_attach_full_required_composite_graph_of_duplicates()
        => ExecuteWithStrategyInTransactionAsync(
            async context =>
            {
                var trackedRoot = await LoadRequiredCompositeGraphAsync(context);
                var entries = context.ChangeTracker.Entries().ToList();

                context.Attach(QueryRequiredCompositeGraph(context).AsNoTracking().Single(IsTheRoot));

                AssertEntries(entries, context.ChangeTracker.Entries().ToList());

                Assert.Equal(0, context.SaveChanges());
            });
}

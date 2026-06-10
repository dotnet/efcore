// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

#nullable disable

public abstract class OptimisticConcurrencyTestBase<TFixture, TRowVersion> : IClassFixture<TFixture>
    where TFixture : F1FixtureBase<TRowVersion>, new()
{
    protected OptimisticConcurrencyTestBase(TFixture fixture)
    {
        Fixture = fixture;
        fixture.ListLoggerFactory.Clear();
    }

    protected TFixture Fixture { get; }

    [ConditionalFact]
    public virtual void External_model_builder_uses_validation()
    {
        var modelBuilder = Fixture.CreateModelBuilder();
        modelBuilder.Entity("Dummy");

        var context = new F1Context(new DbContextOptionsBuilder(Fixture.CreateOptions()).UseModel((IModel)modelBuilder.Model).Options);

        Assert.Equal(
            CoreStrings.EntityRequiresKey("Dummy (Dictionary<string, object>)"),
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    [ConditionalFact]
    public virtual void Nullable_client_side_concurrency_token_can_be_used()
    {
        string originalName;
        var newName = "New name";
        using var c = CreateF1Context();
        c.Database.CreateExecutionStrategy().Execute(
            c, context =>
            {
                using var transaction = BeginTransaction(context.Database);
                var sponsor = context.Sponsors.Single(s => s.Id == 1);
                Assert.IsType<Sponsor.SponsorDoubleProxy>(sponsor);
                Assert.Null(context.Entry(sponsor).Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
                originalName = sponsor.Name;
                sponsor.Name = "New name";
                context.Entry(sponsor).Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue = 1;
                context.SaveChanges();

                using var innerContext = CreateF1Context();
                UseTransaction(innerContext.Database, transaction);
                sponsor = innerContext.Sponsors.Single(s => s.Id == 1);
                Assert.IsType<Sponsor.SponsorDoubleProxy>(sponsor);
                Assert.Equal(1, innerContext.Entry(sponsor).Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
                Assert.Equal("Intercepted: " + newName, sponsor.Name);
                sponsor.Name = originalName;
                innerContext.Entry(sponsor).Property<int?>(Sponsor.ClientTokenPropertyName).OriginalValue = null;
                Assert.Throws<DbUpdateConcurrencyException>(() => innerContext.SaveChanges());
            });
    }

    #region Concurrency resolution with FK associations

    [ConditionalFact]
    public virtual Task Simple_concurrency_exception_can_be_resolved_with_client_values()
        => ConcurrencyTestAsync(
            ClientPodiums, async (_, ex) =>
            {
                var driverEntry = ex.Entries.Single();
                driverEntry.OriginalValues.SetValues(await driverEntry.GetDatabaseValuesAsync());
                ResolveConcurrencyTokens(driverEntry);
            });

    [ConditionalFact]
    public virtual Task Simple_concurrency_exception_can_be_resolved_with_store_values()
        => ConcurrencyTestAsync(
            StorePodiums, async (_, ex) =>
            {
                var driverEntry = ex.Entries.Single();
                var storeValues = await driverEntry.GetDatabaseValuesAsync();
                driverEntry.CurrentValues.SetValues(storeValues);
                driverEntry.OriginalValues.SetValues(storeValues);
                ResolveConcurrencyTokens(driverEntry);
            });

    [ConditionalFact]
    public virtual Task Simple_concurrency_exception_can_be_resolved_with_new_values()
        => ConcurrencyTestAsync(
            10, async (_, ex) =>
            {
                var driverEntry = ex.Entries.Single();
                driverEntry.OriginalValues.SetValues(await driverEntry.GetDatabaseValuesAsync());
                ResolveConcurrencyTokens(driverEntry);
                ((Driver)driverEntry.Entity).Podiums = 10;
            });

    [ConditionalFact]
    public virtual Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_equivalent_of_accept_changes()
        => ConcurrencyTestAsync(
            StorePodiums, async (_, ex) =>
            {
                var driverEntry = ex.Entries.Single();
                var storeValues = await driverEntry.GetDatabaseValuesAsync();
                driverEntry.CurrentValues.SetValues(storeValues);
                driverEntry.OriginalValues.SetValues(storeValues);
                driverEntry.State = EntityState.Unchanged;
            });

    [ConditionalFact]
    public virtual Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_Reload()
        => ConcurrencyTestAsync(StorePodiums, (_, ex) => ex.Entries.Single().ReloadAsync());

    [ConditionalFact]
    public virtual Task Two_concurrency_issues_in_one_to_one_related_entities_can_be_handled_by_dealing_with_dependent_first()
        => ConcurrencyTestAsync(
            async c =>
            {
                var chassis = await c.Set<Chassis>().SingleAsync(c => c.Name == "MP4-25");
                var team = await c.Teams.SingleAsync(t => t.Id == Team.McLaren);
                chassis.Name = "MP4-25b";
                team.Principal = "Larry David";
            },
            async c =>
            {
                var chassis = await c.Set<Chassis>().SingleAsync(c => c.Name == "MP4-25");
                var team = await c.Teams.SingleAsync(t => t.Id == Team.McLaren);
                chassis.Name = "MP4-25c";
                team.Principal = "Jerry Seinfeld";
            },
            async (c, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<Chassis>(entry.Entity);
                await entry.ReloadAsync();

                try
                {
                    await c.SaveChangesAsync();
                    Assert.Fail("Expected second exception due to conflict in principals.");
                }
                catch (DbUpdateConcurrencyException ex2)
                {
                    Assert.Equal(
                        LogLevel.Debug, Fixture.ListLoggerFactory.Log.Single(
                            l =>
                                l.Id == CoreEventId.OptimisticConcurrencyException).Level);

                    var entry2 = ex2.Entries.Single();
                    Assert.IsAssignableFrom<Team>(entry2.Entity);
                    await entry2.ReloadAsync();
                }
            },
            async c =>
            {
                var team = await c.Teams.SingleAsync(t => t.Id == Team.McLaren);
                Assert.Equal("MP4-25b", team.Chassis.Name);
                Assert.Equal("Larry David", team.Principal);
            });

    [ConditionalFact]
    public virtual Task Two_concurrency_issues_in_one_to_many_related_entities_can_be_handled_by_dealing_with_dependent_first()
        => ConcurrencyTestAsync(
            async c =>
            {
                var driver = await c.Drivers.SingleAsync(d => d.Name == "Jenson Button");
                var team = await c.Teams.SingleAsync(t => t.Id == Team.McLaren);
                driver.Poles = 1;
                team.Principal = "Larry David";
            },
            async c =>
            {
                var driver = await c.Drivers.SingleAsync(d => d.Name == "Jenson Button");
                var team = await c.Teams.SingleAsync(t => t.Id == Team.McLaren);
                driver.Poles = 2;
                team.Principal = "Jerry Seinfeld";
            },
            async (c, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<Driver>(entry.Entity);
                await entry.ReloadAsync();

                try
                {
                    await c.SaveChangesAsync();
                    Assert.Fail("Expected second exception due to conflict in principals.");
                }
                catch (DbUpdateConcurrencyException ex2)
                {
                    Assert.Equal(
                        LogLevel.Debug, Fixture.ListLoggerFactory.Log.Single(
                            l =>
                                l.Id == CoreEventId.OptimisticConcurrencyException).Level);

                    var entry2 = ex2.Entries.Single();
                    Assert.IsAssignableFrom<Team>(entry2.Entity);
                    await entry2.ReloadAsync();
                }
            },
            async c =>
            {
                var team = await c.Teams.SingleAsync(t => t.Id == Team.McLaren);
                Assert.Equal(1, team.Drivers.Single(d => d.Name == "Jenson Button").Poles);
                Assert.Equal("Larry David", team.Principal);
            });

    [ConditionalFact]
    public virtual Task Concurrency_issue_where_the_FK_is_the_concurrency_token_can_be_handled()
        => ConcurrencyTestAsync(
            async c => (await c.Engines.SingleAsync(e => e.Name == "056")).EngineSupplierId =
                (await c.EngineSuppliers.SingleAsync(s => s.Name == "Cosworth")).Name,
            async c => (await c.Engines.SingleAsync(e => e.Name == "056")).EngineSupplier =
                await c.EngineSuppliers.SingleAsync(s => s.Name == "Renault"),
            async (c, ex) =>
            {
                var entry = ex.Entries.Single(e => e.Metadata.ClrType == typeof(Engine));
                Assert.IsAssignableFrom<Engine>(entry.Entity);
                await entry.ReloadAsync();
            },
            async c =>
                Assert.Equal(
                    "Cosworth",
                    (await c.Engines.SingleAsync(e => e.Name == "056")).EngineSupplier.Name));

    #endregion

    #region Concurrency exceptions with shadow FK associations

    [ConditionalFact]
    public virtual Task Change_in_independent_association_results_in_independent_association_exception()
        => ConcurrencyTestAsync(
            async c => (await c.Teams.SingleAsync(t => t.Id == Team.Ferrari)).Engine =
                await c.Engines.SingleAsync(s => s.Name == "FO 108X"),
            (_, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<Team>(entry.Entity);
                return Task.CompletedTask;
            },
            null);

    [ConditionalFact]
    public virtual Task
        Change_in_independent_association_after_change_in_different_concurrency_token_results_in_independent_association_exception()
        => ConcurrencyTestAsync(
            async c => (await c.Teams.SingleAsync(t => t.Id == Team.Ferrari)).FastestLaps = 0, async c =>
                (await c.Teams.SingleAsync(t => t.Constructor == "Ferrari")).Engine =
                await c.Engines.SingleAsync(s => s.Name == "FO 108X"),
            (_, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<Team>(entry.Entity);
                return Task.CompletedTask;
            },
            null);

    [ConditionalFact]
    public virtual Task Attempting_to_delete_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        => ConcurrencyTestAsync(
            async c =>
            {
                await c.Teams.Include(e => e.Sponsors).LoadAsync();
                (await c.Teams.SingleAsync(t => t.Id == Team.McLaren)).Sponsors.Remove(
                    await c.Sponsors.SingleAsync(s => s.Name.Contains("FIA")));
            },
            (_, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<TeamSponsor>(entry.Entity);
                return Task.CompletedTask;
            },
            null);

    [ConditionalFact]
    public virtual Task Attempting_to_add_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
    {
        return ConcurrencyTestAsync<DbUpdateException>(
            Change,
            Change,
            (c, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<TeamSponsor>(entry.Entity);
                return Task.CompletedTask;
            },
            null);

        async Task Change(F1Context c)
        {
            await c.Teams.Include(e => e.Sponsors).LoadAsync();
            (await c.Teams.SingleAsync(t => t.Id == Team.McLaren)).Sponsors.Add(
                await c.Sponsors.SingleAsync(s => s.Name.Contains("Shell")));
        }
    }

    #endregion

    #region Concurrency exceptions with complex types

    // Depends on an aggregate-friendly Reload, see #13890
    [ConditionalFact(Skip = "Issue#13890")]
    public virtual Task Concurrency_issue_where_a_complex_type_nested_member_is_the_concurrency_token_can_be_handled()
        => ConcurrencyTestAsync(
            async c => (await c.Engines.SingleAsync(s => s.Name == "CA2010")).StorageLocation.Latitude = 47.642576,
            (_, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<Location>(entry.Entity);
                entry.Reload();
                return Task.CompletedTask;
            }, async c =>
                Assert.Equal(47.642576, (await c.Engines.SingleAsync(s => s.Name == "CA2010")).StorageLocation.Latitude));

    #endregion

    #region Tests for update exceptions involving adding and deleting entities

    [ConditionalFact]
    public virtual async Task Adding_the_same_entity_twice_results_in_DbUpdateException()
    {
        using var c = CreateF1Context();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using var transaction = BeginTransaction(context.Database);
                context.Teams.Add(
                    new Team
                    {
                        Id = -1,
                        Name = "Wubbsy Racing",
                        Chassis = new Chassis { TeamId = -1, Name = "Wubbsy" }
                    });

                using var innerContext = CreateF1Context();
                UseTransaction(innerContext.Database, transaction);
                innerContext.Teams.Add(
                    new Team
                    {
                        Id = -1,
                        Name = "Wubbsy Racing",
                        Chassis = new Chassis { TeamId = -1, Name = "Wubbsy" }
                    });

                await innerContext.SaveChangesAsync();

                await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
            });
    }

    [ConditionalFact]
    public virtual Task Deleting_the_same_entity_twice_results_in_DbUpdateConcurrencyException()
        => ConcurrencyTestAsync(
            async c => c.Drivers.Remove(await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")),
            async (_, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<Driver>(entry.Entity);
                await entry.ReloadAsync();
            }, async c => Assert.Null(await c.Drivers.SingleOrDefaultAsync(d => d.Name == "Fernando Alonso")));

    [ConditionalFact]
    public virtual Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException()
        => ConcurrencyTestAsync(
            async c => (await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")).Wins = 1,
            async c => c.Drivers.Remove(await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")),
            async (_, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<Driver>(entry.Entity);
                await entry.ReloadAsync();
            },
            async c => Assert.Equal(1, (await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")).Wins));

    [ConditionalFact]
    public virtual Task
        Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        => ConcurrencyTestAsync(
            async c => (await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")).Wins = 1,
            async c => c.Drivers.Remove(await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")),
            async (_, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<Driver>(entry.Entity);

                entry.State = EntityState.Unchanged;
                var storeValues = await entry.GetDatabaseValuesAsync();
                entry.OriginalValues.SetValues(storeValues);
                entry.CurrentValues.SetValues(storeValues);
                ResolveConcurrencyTokens(entry);
            },
            async c => Assert.Equal(1, (await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")).Wins));

    [ConditionalFact]
    public virtual Task Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException()
        => ConcurrencyTestAsync(
            async c => c.Drivers.Remove(await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")),
            async c => (await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")).Wins = 1,
            async (_, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<Driver>(entry.Entity);
                await entry.ReloadAsync();
            },
            async c => Assert.Null(await c.Drivers.SingleOrDefaultAsync(d => d.Name == "Fernando Alonso")));

    [ConditionalFact]
    public virtual Task
        Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        => ConcurrencyTestAsync(
            async c => c.Drivers.Remove(await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")),
            async c => (await c.Drivers.SingleAsync(d => d.Name == "Fernando Alonso")).Wins = 1,
            async (_, ex) =>
            {
                var entry = ex.Entries.Single();
                Assert.IsAssignableFrom<Driver>(entry.Entity);
                var storeValues = await entry.GetDatabaseValuesAsync();
                Assert.Null(storeValues);
                entry.State = EntityState.Detached;
            },
            async c => Assert.Null(await c.Drivers.SingleOrDefaultAsync(d => d.Name == "Fernando Alonso")));

    #endregion

    #region Tests for calling Reload on an entity in various states

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_an_Added_entity_that_is_not_in_database_is_no_op(bool async)
    {
        using var c = CreateF1Context();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using (BeginTransaction(context.Database))
                {
                    var entry = context.Drivers.Add(
                        new Driver { Name = "Larry David", TeamId = Team.Ferrari });

                    if (async)
                    {
                        await entry.ReloadAsync();
                    }
                    else
                    {
                        entry.Reload();
                    }

                    Assert.Equal(EntityState.Added, entry.State);
                }
            });
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_an_Unchanged_entity_that_is_not_in_database_detaches_it(bool async)
        => await TestReloadGone(EntityState.Unchanged, async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_a_Modified_entity_that_is_not_in_database_detaches_it(bool async)
        => await TestReloadGone(EntityState.Modified, async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_a_Deleted_entity_that_is_not_in_database_detaches_it(bool async)
        => await TestReloadGone(EntityState.Deleted, async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_a_Detached_entity_that_is_not_in_database_detaches_it(bool async)
        => await TestReloadGone(EntityState.Detached, async);

    private async Task TestReloadGone(EntityState state, bool async)
    {
        using var c = CreateF1Context();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using (BeginTransaction(context.Database))
                {
                    var entry = context.Drivers.Add(
                        new Driver
                        {
                            Id = 676,
                            Name = "Larry David",
                            TeamId = Team.Ferrari
                        });

                    entry.State = state;

                    if (async)
                    {
                        await entry.ReloadAsync();
                    }
                    else
                    {
                        entry.Reload();
                    }

                    Assert.Equal(EntityState.Detached, entry.State);
                }
            });
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_an_Unchanged_entity_makes_the_entity_unchanged(bool async)
        => await TestReloadPositive(EntityState.Unchanged, async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_a_Modified_entity_makes_the_entity_unchanged(bool async)
        => await TestReloadPositive(EntityState.Modified, async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_a_Deleted_entity_makes_the_entity_unchanged(bool async)
        => await TestReloadPositive(EntityState.Deleted, async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_an_Added_entity_that_was_saved_elsewhere_makes_the_entity_unchanged(bool async)
        => await TestReloadPositive(EntityState.Added, async);

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_a_Detached_entity_makes_the_entity_unchanged(bool async)
        => await TestReloadPositive(EntityState.Detached, async);

    private async Task TestReloadPositive(EntityState state, bool async)
    {
        using var c = CreateF1Context();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using (BeginTransaction(context.Database))
                {
                    var larry = async
                        ? await context.Drivers.SingleAsync(d => d.Name == "Jenson Button")
                        : context.Drivers.Single(d => d.Name == "Jenson Button");

                    larry.Name = "Rory Gilmore";
                    var entry = context.Entry(larry);
                    entry.Property(e => e.Name).CurrentValue = "Emily Gilmore";
                    entry.State = state;

                    if (async)
                    {
                        await entry.ReloadAsync();
                    }
                    else
                    {
                        entry.Reload();
                    }

                    Assert.Equal(EntityState.Unchanged, entry.State);
                    Assert.Equal("Jenson Button", larry.Name);
                    Assert.Equal("Jenson Button", entry.Property(e => e.Name).CurrentValue);
                }
            });
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_GetDatabaseValues_on_owned_entity_works(bool async)
    {
        using var c = CreateF1Context();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using var transaction = BeginTransaction(context.Database);
                var titleSponsor = async
                    ? await context.Set<TitleSponsor>().SingleAsync(t => t.Name == "Vodafone")
                    : context.Set<TitleSponsor>().Single(t => t.Name == "Vodafone");

                var ownerEntry = context.Entry(titleSponsor);
                var ownedEntry = ownerEntry.Reference(e => e.Details).TargetEntry;

                using var innerContext = CreateF1Context();
                UseTransaction(innerContext.Database, transaction);

                var innerTitleSponsor = innerContext.Set<TitleSponsor>().Single(t => t.Name == "Vodafone");
                innerTitleSponsor.Details.Days = 5;

                await innerContext.SaveChangesAsync();

                var databaseValues = async
                    ? await ownedEntry.GetDatabaseValuesAsync()
                    : ownedEntry.GetDatabaseValues();
                Assert.Equal(5, databaseValues.GetValue<int>("Days"));
            });
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public virtual async Task Calling_Reload_on_owned_entity_works(bool async)
    {
        using var c = CreateF1Context();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using var transaction = BeginTransaction(context.Database);
                var titleSponsor = async
                    ? await context.Set<TitleSponsor>().SingleAsync(t => t.Name == "Vodafone")
                    : context.Set<TitleSponsor>().Single(t => t.Name == "Vodafone");

                var ownerEntry = context.Entry(titleSponsor);
                var ownedEntry = ownerEntry.Reference(e => e.Details).TargetEntry;

                using var innerContext = CreateF1Context();
                UseTransaction(innerContext.Database, transaction);

                var innerTitleSponsor = innerContext.Set<TitleSponsor>().Single(t => t.Name == "Vodafone");
                innerTitleSponsor.Details.Days = 5;

                await innerContext.SaveChangesAsync();

                if (async)
                {
                    await ownedEntry.ReloadAsync();
                }
                else
                {
                    ownedEntry.Reload();
                }

                Assert.Equal(5, ownedEntry.Property(e => e.Days).CurrentValue);
            });
    }

    #endregion

    #region Helpers

    protected const int StorePodiums = 20;
    protected const int ClientPodiums = 30;

    protected virtual void ResolveConcurrencyTokens(EntityEntry entry)
    {
        // default do nothing. Allow provider-specific entry reset
    }

    protected F1Context CreateF1Context()
        => Fixture.CreateContext();

    private Task ConcurrencyTestAsync(int expectedPodiums, Func<F1Context, DbUpdateConcurrencyException, Task> resolver)
        => ConcurrencyTestAsync(
            async c => (await c.Drivers.SingleAsync(d => d.CarNumber == 1)).Podiums = StorePodiums,
            async c => (await c.Drivers.SingleAsync(d => d.CarNumber == 1)).Podiums = ClientPodiums,
            resolver,
            async c => Assert.Equal(expectedPodiums, (await c.Drivers.SingleAsync(d => d.CarNumber == 1)).Podiums));

    /// <summary>
    ///     Runs the same action twice inside a transaction scope but with two different contexts and calling
    ///     SaveChanges such that first time it will succeed and then the second time it will result in a
    ///     concurrency exception.
    ///     After the exception is caught the resolver action is called, after which SaveChanges is called
    ///     again.  Finally, a new context is created and the validator is called so that the state of
    ///     the database at the end of the process can be validated.
    /// </summary>
    private Task ConcurrencyTestAsync(
        Func<F1Context, Task> change,
        Func<F1Context, DbUpdateConcurrencyException, Task> resolver,
        Func<F1Context, Task> validator)
        => ConcurrencyTestAsync(change, change, resolver, validator);

    /// <summary>
    ///     Runs the two actions with two different contexts and calling
    ///     SaveChanges such that storeChange will succeed and the store will reflect this change, and
    ///     then clientChange will result in a concurrency exception.
    ///     After the exception is caught the resolver action is called, after which SaveChanges is called
    ///     again. Finally, a new context is created and the validator is called so that the state of
    ///     the database at the end of the process can be validated.
    /// </summary>
    protected virtual Task ConcurrencyTestAsync(
        Func<F1Context, Task> storeChange,
        Func<F1Context, Task> clientChange,
        Func<F1Context, DbUpdateConcurrencyException, Task> resolver,
        Func<F1Context, Task> validator)
        => ConcurrencyTestAsync<DbUpdateConcurrencyException>(storeChange, clientChange, resolver, validator);

    /// <summary>
    ///     Runs the two actions with two different contexts and calling
    ///     SaveChanges such that storeChange will succeed and the store will reflect this change, and
    ///     then clientChange will result in a concurrency exception.
    ///     After the exception is caught the resolver action is called, after which SaveChanges is called
    ///     again. Finally, a new context is created and the validator is called so that the state of
    ///     the database at the end of the process can be validated.
    /// </summary>
    protected virtual async Task ConcurrencyTestAsync<TException>(
        Func<F1Context, Task> storeChange,
        Func<F1Context, Task> clientChange,
        Func<F1Context, TException, Task> resolver,
        Func<F1Context, Task> validator)
        where TException : DbUpdateException
    {
        using var c = CreateF1Context();
        await c.Database.CreateExecutionStrategy().ExecuteAsync(
            c, async context =>
            {
                using var transaction = BeginTransaction(context.Database);
                await clientChange(context);

                using var innerContext = CreateF1Context();
                UseTransaction(innerContext.Database, transaction);
                await storeChange(innerContext);
                await innerContext.SaveChangesAsync();

                var updateException =
                    await Assert.ThrowsAnyAsync<TException>(() => context.SaveChangesAsync());

                if (typeof(TException) == typeof(DbUpdateConcurrencyException))
                {
                    Assert.Equal(
                        LogLevel.Debug, Fixture.ListLoggerFactory.Log.Single(
                            l => l.Id == CoreEventId.OptimisticConcurrencyException).Level);
                }

                Fixture.ListLoggerFactory.Clear();

                await resolver(context, updateException);

                using var validationContext = CreateF1Context();
                UseTransaction(validationContext.Database, transaction);
                if (validator != null)
                {
                    await context.SaveChangesAsync();

                    await validator(validationContext);
                }
            });
    }

    protected virtual IDbContextTransaction BeginTransaction(DatabaseFacade facade)
        => facade.BeginTransaction();

    protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
    {
    }

    #endregion
}

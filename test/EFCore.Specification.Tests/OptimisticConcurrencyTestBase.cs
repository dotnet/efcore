// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.TestModels.ConcurrencyModel;
using Microsoft.EntityFrameworkCore.TestUtilities.Xunit;
using Microsoft.Extensions.Logging;
using Xunit;

// ReSharper disable AccessToDisposedClosure
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore
{
    public abstract class OptimisticConcurrencyTestBase<TFixture> : IClassFixture<TFixture>
        where TFixture : F1FixtureBase, new()
    {
        protected OptimisticConcurrencyTestBase(TFixture fixture)
        {
            Fixture = fixture;
            fixture.ListLoggerFactory.Clear();
        }

        protected TFixture Fixture { get; }

        [Fact]
        public virtual void External_model_builder_uses_validation()
        {
            var modelBuilder = Fixture.CreateModelBuilder();
            modelBuilder.Entity("Dummy");

            Assert.Equal(
                CoreStrings.ShadowEntity("Dummy"),
                Assert.Throws<InvalidOperationException>
                    (() => modelBuilder.FinalizeModel()).Message);
        }

        [Fact]
        public virtual void Nullable_client_side_concurrency_token_can_be_used()
        {
            string originalName;
            var newName = "New name";
            using (var c = CreateF1Context())
            {
                c.Database.CreateExecutionStrategy().Execute(
                    c, context =>
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            var sponsor = context.Sponsors.Single(s => s.Id == 1);
                            Assert.Null(context.Entry(sponsor).Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
                            originalName = sponsor.Name;
                            sponsor.Name = "New name";
                            context.Entry(sponsor).Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue = 1;
                            context.SaveChanges();

                            using (var innerContext = CreateF1Context())
                            {
                                UseTransaction(innerContext.Database, transaction);
                                sponsor = innerContext.Sponsors.Single(s => s.Id == 1);
                                Assert.Equal(1, innerContext.Entry(sponsor).Property<int?>(Sponsor.ClientTokenPropertyName).CurrentValue);
                                Assert.Equal(newName, sponsor.Name);
                                sponsor.Name = originalName;
                                innerContext.Entry(sponsor).Property<int?>(Sponsor.ClientTokenPropertyName).OriginalValue = null;
                                Assert.Throws<DbUpdateConcurrencyException>(() => innerContext.SaveChanges());
                            }
                        }
                    });
            }
        }

        #region Concurrency resolution with FK associations

        [Fact]
        public virtual Task Simple_concurrency_exception_can_be_resolved_with_client_values()
        {
            return ConcurrencyTestAsync(
                ClientPodiums, (c, ex) =>
                {
                    var driverEntry = ex.Entries.Single();
                    driverEntry.OriginalValues.SetValues(driverEntry.GetDatabaseValues());
                    ResolveConcurrencyTokens(driverEntry);
                });
        }

        [Fact]
        public virtual Task Simple_concurrency_exception_can_be_resolved_with_store_values()
        {
            return ConcurrencyTestAsync(
                StorePodiums, (c, ex) =>
                {
                    var driverEntry = ex.Entries.Single();
                    var storeValues = driverEntry.GetDatabaseValues();
                    driverEntry.CurrentValues.SetValues(storeValues);
                    driverEntry.OriginalValues.SetValues(storeValues);
                    ResolveConcurrencyTokens(driverEntry);
                });
        }

        [Fact]
        public virtual Task Simple_concurrency_exception_can_be_resolved_with_new_values()
        {
            return ConcurrencyTestAsync(
                10, (c, ex) =>
                {
                    var driverEntry = ex.Entries.Single();
                    driverEntry.OriginalValues.SetValues(driverEntry.GetDatabaseValues());
                    ResolveConcurrencyTokens(driverEntry);
                    ((Driver)driverEntry.Entity).Podiums = 10;
                });
        }

        [Fact]
        public virtual Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_equivalent_of_accept_changes()
        {
            return ConcurrencyTestAsync(
                StorePodiums, (c, ex) =>
                {
                    var driverEntry = ex.Entries.Single();
                    var storeValues = driverEntry.GetDatabaseValues();
                    driverEntry.CurrentValues.SetValues(storeValues);
                    driverEntry.OriginalValues.SetValues(storeValues);
                    driverEntry.State = EntityState.Unchanged;
                });
        }

        [Fact]
        public virtual Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_Reload()
        {
            return ConcurrencyTestAsync(StorePodiums, (c, ex) => ex.Entries.Single().Reload());
        }

        [Fact]
        public virtual Task Two_concurrency_issues_in_one_to_one_related_entities_can_be_handled_by_dealing_with_dependent_first()
        {
            return ConcurrencyTestAsync(
                c =>
                {
                    var team = c.Teams.Single(t => t.Id == Team.McLaren);
                    team.Chassis.Name = "MP4-25b";
                    team.Principal = "Larry David";
                },
                c =>
                {
                    var team = c.Teams.Single(t => t.Id == Team.McLaren);
                    team.Chassis.Name = "MP4-25c";
                    team.Principal = "Jerry Seinfeld";
                },
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Chassis>(entry.Entity);
                    entry.Reload();

                    try
                    {
                        c.SaveChanges();
                        Assert.True(false, "Expected second exception due to conflict in principals.");
                    }
                    catch (DbUpdateConcurrencyException ex2)
                    {
                        Assert.Equal(
                            LogLevel.Debug, Fixture.ListLoggerFactory.Log.Single(
                                l =>
                                    l.Id == CoreEventId.OptimisticConcurrencyException).Level);

                        var entry2 = ex2.Entries.Single();
                        Assert.IsAssignableFrom<Team>(entry2.Entity);
                        entry2.Reload();
                    }
                },
                c =>
                {
                    var team = c.Teams.Single(t => t.Id == Team.McLaren);
                    Assert.Equal("MP4-25b", team.Chassis.Name);
                    Assert.Equal("Larry David", team.Principal);
                });
        }

        [Fact]
        public virtual Task Two_concurrency_issues_in_one_to_many_related_entities_can_be_handled_by_dealing_with_dependent_first()
        {
            return ConcurrencyTestAsync(
                c =>
                {
                    var team = c.Teams.Single(t => t.Id == Team.McLaren);
                    team.Drivers.Single(d => d.Name == "Jenson Button").Poles = 1;
                    team.Principal = "Larry David";
                },
                c =>
                {
                    var team = c.Teams.Single(t => t.Id == Team.McLaren);
                    team.Drivers.Single(d => d.Name == "Jenson Button").Poles = 2;
                    team.Principal = "Jerry Seinfeld";
                },
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Driver>(entry.Entity);
                    entry.Reload();

                    try
                    {
                        c.SaveChanges();
                        Assert.True(false, "Expected second exception due to conflict in principals.");
                    }
                    catch (DbUpdateConcurrencyException ex2)
                    {
                        Assert.Equal(
                            LogLevel.Debug, Fixture.ListLoggerFactory.Log.Single(
                                l =>
                                    l.Id == CoreEventId.OptimisticConcurrencyException).Level);

                        var entry2 = ex2.Entries.Single();
                        Assert.IsAssignableFrom<Team>(entry2.Entity);
                        entry2.Reload();
                    }
                },
                c =>
                {
                    var team = c.Teams.Single(t => t.Id == Team.McLaren);
                    Assert.Equal(1, team.Drivers.Single(d => d.Name == "Jenson Button").Poles);
                    Assert.Equal("Larry David", team.Principal);
                });
        }

        [Fact]
        public virtual Task Concurrency_issue_where_the_FK_is_the_concurrency_token_can_be_handled()
        {
            return ConcurrencyTestAsync(
                c => c.Engines.Single(e => e.Name == "056").EngineSupplierId =
                    c.EngineSuppliers.Single(s => s.Name == "Cosworth").Id,
                c => c.Engines.Single(e => e.Name == "056").EngineSupplier =
                    c.EngineSuppliers.Single(s => s.Name == "Renault"),
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Engine>(entry.Entity);
                    entry.Reload();
                },
                c =>
                    Assert.Equal(
                        "Cosworth",
                        c.Engines.Single(e => e.Name == "056").EngineSupplier.Name));
        }

        #endregion

        #region Concurrency exceptions with shadow FK associations

        [Fact]
        public virtual Task Change_in_independent_association_results_in_independent_association_exception()
        {
            return ConcurrencyTestAsync(
                c => c.Teams.Single(t => t.Id == Team.Ferrari).Engine = c.Engines.Single(s => s.Name == "FO 108X"),
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Team>(entry.Entity);
                },
                null);
        }

        [Fact]
        public virtual Task
            Change_in_independent_association_after_change_in_different_concurrency_token_results_in_independent_association_exception()
        {
            return ConcurrencyTestAsync(
                c => c.Teams.Single(t => t.Id == Team.Ferrari).FastestLaps = 0,
                c =>
                    c.Teams.Single(t => t.Constructor == "Ferrari").Engine =
                        c.Engines.Single(s => s.Name == "FO 108X"),
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Team>(entry.Entity);
                },
                null);
        }

        [ConditionalFact(Skip = "Many-to-many relationships are not supported without CLR class for join table.")]
        // TODO: See issue#1368
        public virtual Task
            Attempting_to_delete_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        {
            return ConcurrencyTestAsync(
                c =>
                    c.Teams.Single(t => t.Id == Team.McLaren).Sponsors.Add(c.Sponsors.Single(s => s.Name.Contains("Shell"))),
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Team>(entry.Entity);
                },
                null);
        }

        [ConditionalFact(Skip = "Many-to-many relationships are not supported without CLR class for join table.")]
        // TODO: See issue#1368
        public virtual Task
            Attempting_to_add_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        {
            return ConcurrencyTestAsync(
                c =>
                    c.Teams.Single(t => t.Id == Team.McLaren).Sponsors.Remove(c.Sponsors.Single(s => s.Name.Contains("FIA"))),
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Team>(entry.Entity);
                },
                null);
        }

        #endregion

        #region Concurrency exceptions with complex types

        // Depends on an aggregate-friendly Reload, see #13890
        //[Fact]
        public virtual Task Concurrency_issue_where_a_complex_type_nested_member_is_the_concurrency_token_can_be_handled()
        {
            return ConcurrencyTestAsync(
                c => c.Engines.Single(s => s.Name == "CA2010").StorageLocation.Latitude = 47.642576,
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Location>(entry.Entity);
                    entry.Reload();
                },
                c =>
                    Assert.Equal(47.642576, c.Engines.Single(s => s.Name == "CA2010").StorageLocation.Latitude));
        }

        #endregion

        #region Tests for update exceptions involving adding and deleting entities

        [Fact]
        public virtual async Task Adding_the_same_entity_twice_results_in_DbUpdateException()
        {
            using (var c = CreateF1Context())
            {
                await c.Database.CreateExecutionStrategy().ExecuteAsync(
                    c, async context =>
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            context.Teams.Add(
                                new Team
                                {
                                    Id = -1,
                                    Name = "Wubbsy Racing",
                                    Chassis = new Chassis
                                    {
                                        TeamId = -1,
                                        Name = "Wubbsy"
                                    }
                                });

                            using (var innerContext = CreateF1Context())
                            {
                                UseTransaction(innerContext.Database, transaction);
                                innerContext.Teams.Add(
                                    new Team
                                    {
                                        Id = -1,
                                        Name = "Wubbsy Racing",
                                        Chassis = new Chassis
                                        {
                                            TeamId = -1,
                                            Name = "Wubbsy"
                                        }
                                    });

                                await innerContext.SaveChangesAsync();

                                await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());
                            }
                        }
                    });
            }
        }

        [Fact]
        public virtual Task Deleting_the_same_entity_twice_results_in_DbUpdateConcurrencyException()
        {
            return ConcurrencyTestAsync(
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Driver>(entry.Entity);
                    entry.Reload();
                },
                c => Assert.Null(c.Drivers.SingleOrDefault(d => d.Name == "Fernando Alonso")));
        }

        [Fact]
        public virtual Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException()
        {
            return ConcurrencyTestAsync(
                c => c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins = 1,
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Driver>(entry.Entity);
                    entry.Reload();
                },
                c => Assert.Equal(1, c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins));
        }

        [Fact]
        public virtual Task
            Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        {
            return ConcurrencyTestAsync(
                c => c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins = 1,
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Driver>(entry.Entity);

                    entry.State = EntityState.Unchanged;
                    var storeValues = entry.GetDatabaseValues();
                    entry.OriginalValues.SetValues(storeValues);
                    entry.CurrentValues.SetValues(storeValues);
                    ResolveConcurrencyTokens(entry);
                },
                c => Assert.Equal(1, c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins));
        }

        [Fact]
        public virtual Task Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException()
        {
            return ConcurrencyTestAsync(
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                c => c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins = 1,
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Driver>(entry.Entity);
                    entry.Reload();
                },
                c => Assert.Null(c.Drivers.SingleOrDefault(d => d.Name == "Fernando Alonso")));
        }

        [Fact]
        public virtual Task
            Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        {
            return ConcurrencyTestAsync(
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                c => c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins = 1,
                (c, ex) =>
                {
                    var entry = ex.Entries.Single();
                    Assert.IsAssignableFrom<Driver>(entry.Entity);
                    var storeValues = entry.GetDatabaseValues();
                    Assert.Null(storeValues);
                    entry.State = EntityState.Detached;
                },
                c => Assert.Null(c.Drivers.SingleOrDefault(d => d.Name == "Fernando Alonso")));
        }

        #endregion

        #region Tests for calling Reload on an entity in various states

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Calling_Reload_on_an__Added_entity_that_is_not_in_database_is_no_op(bool async)
        {
            using (var c = CreateF1Context())
            {
                await c.Database.CreateExecutionStrategy().ExecuteAsync(
                    c, async context =>
                    {
                        using (context.Database.BeginTransaction())
                        {
                            var entry = context.Drivers.Add(
                                new Driver
                                {
                                    Name = "Larry David",
                                    TeamId = Team.Ferrari
                                });

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
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Calling_Reload_on_an_Unchanged_entity_that_is_not_in_database_detaches_it(bool async)
            => await TestReloadGone(EntityState.Unchanged, async);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Calling_Reload_on_a_Modified_entity_that_is_not_in_database_detaches_it(bool async)
            => await TestReloadGone(EntityState.Modified, async);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Calling_Reload_on_a_Deleted_entity_that_is_not_in_database_detaches_it(bool async)
            => await TestReloadGone(EntityState.Deleted, async);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Calling_Reload_on_a_Detached_entity_that_is_not_in_database_detaches_it(bool async)
            => await TestReloadGone(EntityState.Detached, async);

        private async Task TestReloadGone(EntityState state, bool async)
        {
            using (var c = CreateF1Context())
            {
                await c.Database.CreateExecutionStrategy().ExecuteAsync(
                    c, async context =>
                    {
                        using (context.Database.BeginTransaction())
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
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Calling_Reload_on_an_Unchanged_entity_makes_the_entity_unchanged(bool async)
            => await TestReloadPositive(EntityState.Unchanged, async);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Calling_Reload_on_a_Modified_entity_makes_the_entity_unchanged(bool async)
            => await TestReloadPositive(EntityState.Modified, async);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Calling_Reload_on_a_Deleted_entity_makes_the_entity_unchanged(bool async)
            => await TestReloadPositive(EntityState.Deleted, async);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Calling_Reload_on_an_Added_entity_that_was_saved_elsewhere_makes_the_entity_unchanged(bool async)
            => await TestReloadPositive(EntityState.Added, async);

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public virtual async Task Calling_Reload_on_a_Detached_entity_makes_the_entity_unchanged(bool async)
            => await TestReloadPositive(EntityState.Detached, async);

        private async Task TestReloadPositive(EntityState state, bool async)
        {
            using (var c = CreateF1Context())
            {
                await c.Database.CreateExecutionStrategy().ExecuteAsync(
                    c, async context =>
                    {
                        using (context.Database.BeginTransaction())
                        {
                            var larry = context.Drivers.Single(d => d.Name == "Jenson Button");
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
        }

        #endregion

        #region Helpers

        protected const int StorePodiums = 20;
        protected const int ClientPodiums = 30;

        protected virtual void ResolveConcurrencyTokens(EntityEntry entry)
        {
            // default do nothing. Allow provider-specific entry reset
        }

        protected F1Context CreateF1Context() => Fixture.CreateContext();

        private Task ConcurrencyTestAsync(int expectedPodiums, Action<F1Context, DbUpdateConcurrencyException> resolver)
            => ConcurrencyTestAsync(
                c => c.Drivers.Single(d => d.CarNumber == 1).Podiums = StorePodiums,
                c => c.Drivers.Single(d => d.CarNumber == 1).Podiums = ClientPodiums,
                resolver,
                c => Assert.Equal(expectedPodiums, c.Drivers.Single(d => d.CarNumber == 1).Podiums));

        /// <summary>
        ///     Runs the same action twice inside a transaction scope but with two different contexts and calling
        ///     SaveChanges such that first time it will succeed and then the second time it will result in a
        ///     concurrency exception.
        ///     After the exception is caught the resolver action is called, after which SaveChanges is called
        ///     again.  Finally, a new context is created and the validator is called so that the state of
        ///     the database at the end of the process can be validated.
        /// </summary>
        private Task ConcurrencyTestAsync(
            Action<F1Context> change, Action<F1Context, DbUpdateConcurrencyException> resolver,
            Action<F1Context> validator) => ConcurrencyTestAsync(change, change, resolver, validator);

        /// <summary>
        ///     Runs the two actions with two different contexts and calling
        ///     SaveChanges such that storeChange will succeed and the store will reflect this change, and
        ///     then clientChange will result in a concurrency exception.
        ///     After the exception is caught the resolver action is called, after which SaveChanges is called
        ///     again. Finally, a new context is created and the validator is called so that the state of
        ///     the database at the end of the process can be validated.
        /// </summary>
        protected virtual async Task ConcurrencyTestAsync(
            Action<F1Context> storeChange, Action<F1Context> clientChange,
            Action<F1Context, DbUpdateConcurrencyException> resolver, Action<F1Context> validator)
        {
            using (var c = CreateF1Context())
            {
                await c.Database.CreateExecutionStrategy().ExecuteAsync(
                    c, async context =>
                    {
                        using (var transaction = context.Database.BeginTransaction())
                        {
                            clientChange(context);

                            using (var innerContext = CreateF1Context())
                            {
                                UseTransaction(innerContext.Database, transaction);
                                storeChange(innerContext);
                                await innerContext.SaveChangesAsync();

                                var updateException =
                                    await Assert.ThrowsAnyAsync<DbUpdateConcurrencyException>(() => context.SaveChangesAsync());

                                Assert.Equal(
                                    LogLevel.Debug, Fixture.ListLoggerFactory.Log.Single(
                                        l => l.Id == CoreEventId.OptimisticConcurrencyException).Level);
                                Fixture.ListLoggerFactory.Clear();

                                resolver(context, updateException);

                                using (var validationContext = CreateF1Context())
                                {
                                    UseTransaction(validationContext.Database, transaction);
                                    if (validator != null)
                                    {
                                        await context.SaveChangesAsync();

                                        validator(validationContext);
                                    }
                                }
                            }
                        }
                    });
            }
        }

        protected virtual void UseTransaction(DatabaseFacade facade, IDbContextTransaction transaction)
        {
        }

        #endregion
    }
}

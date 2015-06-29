// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Data.Entity.ChangeTracking;
using Microsoft.Data.Entity.ChangeTracking.Internal;
using Microsoft.Data.Entity.FunctionalTests.TestModels.ConcurrencyModel;
using Microsoft.Data.Entity.Infrastructure;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.FunctionalTests
{
    // TODO: Remove these once available in the product
    internal static class TestExtensions
    {
        public static void SetValues(this InternalEntityEntry internalEntry, Dictionary<IProperty, object> values)
        {
            foreach (var value in values)
            {
                var property = value.Key;

                internalEntry[property] = value.Value;

                if (property.IsReadOnlyAfterSave)
                {
                    internalEntry.SetPropertyModified(property, isModified: false);
                }
            }
        }

        public static void SetOriginalValues(this InternalEntityEntry internalEntry, Dictionary<IProperty, object> values)
        {
            var originalValues = internalEntry.OriginalValues;

            foreach (var value in values)
            {
                var property = value.Key;

                originalValues[property] = value.Value;

                if (property.IsReadOnlyAfterSave)
                {
                    // Prevent DetectChanges from marking this property as Modified
                    internalEntry[property] = value.Value;
                    internalEntry.SetPropertyModified(property, isModified: false);
                }
            }
        }

        public static void Reload(this InternalEntityEntry internalEntry, DbContext context)
        {
            internalEntry.ReloadAsync(context).Wait();
        }

        public static Task ReloadAsync(this InternalEntityEntry internalEntry, DbContext context)
        {
            if (internalEntry.EntityState == EntityState.Detached)
            {
                throw new InvalidOperationException("Can't reload an unknown entity");
            }

            if (internalEntry.EntityState == EntityState.Added)
            {
                throw new InvalidOperationException("Can't reload an added entity");
            }

            var storeValues = internalEntry.GetDatabaseValues(context);
            if (storeValues == null)
            {
                internalEntry.SetEntityState(EntityState.Detached);
            }
            else
            {
                internalEntry.SetValues(storeValues);
                internalEntry.SetOriginalValues(storeValues);
                internalEntry.SetEntityState(EntityState.Unchanged);
            }
            return Task.FromResult(false);
        }

        public static void Reload(this EntityEntry entityEntry, DbContext context)
        {
            entityEntry.GetService().Reload(context);
        }

        public static Task ReloadAsync(this EntityEntry entityEntry, DbContext context)
        {
            return entityEntry.GetService().ReloadAsync(context);
        }

        public static Dictionary<IProperty, object> GetDatabaseValues(this InternalEntityEntry internalEntry, DbContext context)
        {
            if (internalEntry.EntityType.ClrType == typeof(Driver))
            {
                var id = (int)internalEntry.GetPrimaryKeyValue().Value;
                return context.Set<Driver>()
                    .Where(d => d.Id == id)
                    .Select(d => d.GetValues(internalEntry.EntityType))
                    .SingleOrDefault();
            }

            if (internalEntry.EntityType.ClrType == typeof(Engine))
            {
                var id = (int)internalEntry.GetPrimaryKeyValue().Value;
                return context.Set<Engine>()
                    .Where(d => d.Id == id)
                    .Select(d => d.GetValues(internalEntry.EntityType))
                    .SingleOrDefault();
            }

            return null;
        }

        private static Dictionary<IProperty, object> GetValues(this Driver driver, IEntityType entityType)
        {
            var result = new Dictionary<IProperty, object>();
            result.Add(entityType.GetProperty("CarNumber"), driver.CarNumber);
            result.Add(entityType.GetProperty("Championships"), driver.Championships);
            result.Add(entityType.GetProperty("Id"), driver.Id);
            result.Add(entityType.GetProperty("FastestLaps"), driver.FastestLaps);
            result.Add(entityType.GetProperty("Name"), driver.Name);
            result.Add(entityType.GetProperty("Podiums"), driver.Podiums);
            result.Add(entityType.GetProperty("Poles"), driver.Poles);
            result.Add(entityType.GetProperty("Races"), driver.Races);
            result.Add(entityType.GetProperty("TeamId"), driver.TeamId);
            result.Add(entityType.GetProperty("Version"), driver.Version);
            result.Add(entityType.GetProperty("Wins"), driver.Wins);
            return result;
        }

        private static Dictionary<IProperty, object> GetValues(this Engine engine, IEntityType entityType)
        {
            var result = new Dictionary<IProperty, object>();
            result.Add(entityType.GetProperty("EngineSupplierId"), engine.EngineSupplierId);
            result.Add(entityType.GetProperty("Id"), engine.Id);
            result.Add(entityType.GetProperty("Name"), engine.Name);
            return result;
        }
    }

    public abstract class OptimisticConcurrencyTestBase<TTestStore, TFixture> : IClassFixture<TFixture>, IDisposable
        where TTestStore : TestStore
        where TFixture : F1FixtureBase<TTestStore>, new()
    {
        #region Concurrency resolution with FK associations

        [Fact]
        public virtual Task Simple_concurrency_exception_can_be_resolved_with_client_values()
        {
            return ConcurrencyTestAsync(
                ClientPodiums, (c, ex) =>
                    {
                        var driverEntry = ex.Entries.Single();
                        driverEntry.SetOriginalValues(driverEntry.GetDatabaseValues(c));
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
                        var storeValues = driverEntry.GetDatabaseValues(c);
                        driverEntry.SetValues(storeValues);
                        driverEntry.SetOriginalValues(storeValues);
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
                        driverEntry.SetOriginalValues(driverEntry.GetDatabaseValues(c));
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
                        var storeValues = driverEntry.GetDatabaseValues(c);
                        driverEntry.SetValues(storeValues);
                        driverEntry.SetOriginalValues(storeValues);
                        driverEntry.SetEntityState(EntityState.Unchanged);
                    });
        }

        [Fact]
        public virtual Task Simple_concurrency_exception_can_be_resolved_with_store_values_using_Reload()
        {
            return ConcurrencyTestAsync(StorePodiums, (c, ex) => ex.Entries.Single().Reload(c));
        }

        // TODO: Uncomment the tests below when lazy loading works
        // [Fact]
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
                    }, (c, ex) =>
                        {
                            Assert.IsType<DbUpdateConcurrencyException>(ex);

                            var entry = ex.Entries.Single();
                            Assert.IsAssignableFrom<Chassis>(entry.Entity);
                            entry.Reload(c);

                            try
                            {
                                c.SaveChanges();
                                Assert.True(false, "Expected second exception due to conflict in principals.");
                            }
                            catch (DbUpdateConcurrencyException ex2)
                            {
                                var entry2 = ex2.Entries.Single();
                                Assert.IsAssignableFrom<Team>(entry2.Entity);
                                entry2.Reload(c);
                            }
                        },
                c =>
                    {
                        var team = c.Teams.Single(t => t.Id == Team.McLaren);
                        Assert.Equal("MP4-25b", team.Chassis.Name);
                        Assert.Equal("Larry David", team.Principal);
                    });
        }

        //[Fact]
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
                    }, (c, ex) =>
                        {
                            Assert.IsType<DbUpdateConcurrencyException>(ex);

                            var entry = ex.Entries.Single();
                            Assert.IsAssignableFrom<Driver>(entry.Entity);
                            entry.Reload(c);

                            try
                            {
                                c.SaveChanges();
                                Assert.True(false, "Expected second exception due to conflict in principals.");
                            }
                            catch (DbUpdateConcurrencyException ex2)
                            {
                                var entry2 = ex2.Entries.Single();
                                Assert.IsAssignableFrom<Team>(entry2.Entity);
                                entry2.Reload(c);
                            }
                        },
                c =>
                    {
                        var team = c.Teams.Single(t => t.Id == Team.McLaren);
                        Assert.Equal(1, team.Drivers.Single(d => d.Name == "Jenson Button").Poles);
                        Assert.Equal("Larry David", team.Principal);
                    });
        }

        //TODO: Uncomment when Include is implemented
        //[Fact]
        public virtual Task Concurrency_issue_where_the_FK_is_the_concurrency_token_can_be_handled()
        {
            return ConcurrencyTestAsync(
                c =>
                    c.Engines.Single(e => e.Name == "056").EngineSupplierId =
                        c.EngineSuppliers.Single(s => s.Name == "Cosworth").Id,
                c =>
                    c.Engines.Single(e => e.Name == "056").EngineSupplier =
                        c.EngineSuppliers.Single(s => s.Name == "Renault"),
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Engine>(entry.Entity);
                        entry.Reload(c);
                    },
                c =>
                    Assert.Equal(
                        "Cosworth",
                        c.Engines.Include(e => e.EngineSupplier).Single(e => e.Name == "056").EngineSupplier.Name));
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
                        Assert.IsType<DbUpdateConcurrencyException>(ex);
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
                        Assert.IsType<DbUpdateConcurrencyException>(ex);
                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Team>(entry.Entity);
                    },
                null);
        }

        // TODO: Many to Many
        //[Fact]
        public virtual Task
            Attempting_to_delete_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        {
            return ConcurrencyTestAsync(
                c =>
                    c.Teams.Single(t => t.Id == Team.McLaren).Sponsors.Add(c.Sponsors.Single(s => s.Name.Contains("Shell"))),
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);
                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Team>(entry.Entity);
                    },
                null);
        }

        // TODO: Many to Many
        //[Fact]
        public virtual Task
            Attempting_to_add_same_relationship_twice_for_many_to_many_results_in_independent_association_exception()
        {
            return ConcurrencyTestAsync(
                c =>
                    c.Teams.Single(t => t.Id == Team.McLaren).Sponsors.Remove(c.Sponsors.Single(s => s.Name.Contains("FIA"))),
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);
                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Team>(entry.Entity);
                    },
                null);
        }

        #endregion

        #region Concurrency exceptions with complex types

        // TODO: Complex types
        //[Fact]
        public virtual Task Concurrency_issue_where_a_complex_type_nested_member_is_the_concurrency_token_can_be_handled()
        {
            return ConcurrencyTestAsync(
                c => c.Engines.Single(s => s.Name == "CA2010").StorageLocation.Latitude = 47.642576,
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Engine>(entry.Entity);
                        entry.Reload(c);
                    },
                c =>
                    Assert.Equal(47.642576, c.Engines.Single(s => s.Name == "CA2010").StorageLocation.Latitude));
        }

        #endregion

        #region Tests for update exceptions involving adding and deleting entities

        [Fact]
        public virtual Task Adding_the_same_entity_twice_results_in_DbUpdateException()
        {
            return ConcurrencyTestAsync(
                c =>
                    c.Teams.Add(
                        new Team
                            {
                                Id = -1,
                                Name = "Wubbsy Racing",
                                Chassis = new Chassis
                                    {
                                        TeamId = -1,
                                        Name = "Wubbsy"
                                    }
                            }),
                (c, ex) => Assert.IsType<DbUpdateException>(ex),
                null);
        }

        [Fact]
        public virtual Task Deleting_the_same_entity_twice_results_in_DbUpdateConcurrencyException()
        {
            return ConcurrencyTestAsync(
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Driver>(entry.Entity);
                        entry.Reload(c);
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
                        Assert.IsType<DbUpdateConcurrencyException>(ex);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Driver>(entry.Entity);
                        entry.Reload(c);
                    },
                c => Assert.Equal(1, c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins));
        }

        [Fact]
        public virtual Task Updating_then_deleting_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        {
            return ConcurrencyTestAsync(
                c => c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins = 1,
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Driver>(entry.Entity);

                        entry.SetEntityState(EntityState.Unchanged);
                        var storeValues = entry.GetDatabaseValues(c);
                        entry.SetOriginalValues(storeValues);
                        entry.SetValues(storeValues);
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
                        Assert.IsType<DbUpdateConcurrencyException>(ex);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Driver>(entry.Entity);
                        entry.Reload(c);
                    },
                c => Assert.Null(c.Drivers.SingleOrDefault(d => d.Name == "Fernando Alonso")));
        }

        [Fact]
        public virtual Task Deleting_then_updating_the_same_entity_results_in_DbUpdateConcurrencyException_which_can_be_resolved_with_store_values()
        {
            return ConcurrencyTestAsync(
                c => c.Drivers.Remove(c.Drivers.Single(d => d.Name == "Fernando Alonso")),
                c => c.Drivers.Single(d => d.Name == "Fernando Alonso").Wins = 1,
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);

                        var entry = ex.Entries.Single();
                        Assert.IsAssignableFrom<Driver>(entry.Entity);
                        var storeValues = entry.GetDatabaseValues(c);
                        Assert.Null(storeValues);
                        entry.SetEntityState(EntityState.Detached);
                    },
                c => Assert.Null(c.Drivers.SingleOrDefault(d => d.Name == "Fernando Alonso")));
        }

        #endregion

        #region Tests for calling Reload on an entity in various states

        [Fact]
        public virtual void Calling_Reload_on_an_Added_entity_throws()
        {
            using (var context = CreateF1Context())
            {
                var entry = context.Drivers.Add(
                    new Driver
                        {
                            Name = "Larry David",
                            TeamId = Team.Ferrari
                        });

                Assert.Equal("Can't reload an added entity",
                    Assert.Throws<InvalidOperationException>(() => entry.Reload(context)).Message);
            }
        }

        [Fact]
        public virtual void Calling_Reload_on_a_detached_entity_throws()
        {
            using (var context = CreateF1Context())
            {
                var entry = context.Drivers.Add(
                    new Driver
                        {
                            Name = "Larry David",
                            TeamId = Team.Ferrari
                        });
                entry.State = EntityState.Detached;

                Assert.Equal("Can't reload an unknown entity",
                    Assert.Throws<InvalidOperationException>(() => entry.Reload(context)).Message);
            }
        }

        [Fact]
        public virtual void Calling_Reload_on_a_Unchanged_entity_makes_the_entity_unchanged()
        {
            TestReloadPositive(EntityState.Unchanged);
        }

        [Fact]
        public virtual void Calling_Reload_on_a_Modified_entity_makes_the_entity_unchanged()
        {
            TestReloadPositive(EntityState.Modified);
        }

        [Fact]
        public virtual void Calling_Reload_on_a_Deleted_entity_makes_the_entity_unchanged()
        {
            TestReloadPositive(EntityState.Deleted);
        }

        private void TestReloadPositive(EntityState state)
        {
            using (var context = CreateF1Context())
            {
                var larry = context.Drivers.Single(d => d.Name == "Jenson Button");
                var entry = context.Entry(larry);
                entry.State = state;

                entry.Reload(context);

                Assert.Equal(EntityState.Unchanged, entry.State);
            }
        }

        #endregion

        #region Tests for calling ReloadAsync on an entity in various states

        // TODO: Reload

        [Fact]
        public virtual async Task Calling_ReloadAsync_on_an_Added_entity_throws()
        {
            using (var context = CreateF1Context())
            {
                var entry = context.Drivers.Add(
                    new Driver
                        {
                            Name = "Larry David",
                            TeamId = Team.Ferrari
                        });

                Assert.Equal("Can't reload an added entity",
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => entry.ReloadAsync(context))).Message);
            }
        }

        [Fact]
        public virtual async Task Calling_ReloadAsync_on_a_detached_entity_throws()
        {
            using (var context = CreateF1Context())
            {
                var entry = context.Drivers.Add(
                    new Driver
                        {
                            Name = "Larry David",
                            TeamId = Team.Ferrari
                        });
                entry.State = EntityState.Detached;

                Assert.Equal("Can't reload an unknown entity",
                    (await Assert.ThrowsAsync<InvalidOperationException>(() => entry.ReloadAsync(context))).Message);
            }
        }

        [Fact]
        public virtual Task Calling_ReloadAsync_on_a_Unchanged_entity_makes_the_entity_unchanged()
        {
            return TestReloadAsyncPositive(EntityState.Unchanged);
        }

        [Fact]
        public virtual Task Calling_ReloadAsync_on_a_Modified_entity_makes_the_entity_unchanged()
        {
            return TestReloadAsyncPositive(EntityState.Modified);
        }

        [Fact]
        public virtual Task Calling_ReloadAsync_on_a_Deleted_entity_makes_the_entity_unchanged()
        {
            return TestReloadAsyncPositive(EntityState.Deleted);
        }

        private async Task TestReloadAsyncPositive(EntityState state)
        {
            using (var context = CreateF1Context())
            {
                var larry = context.Drivers.Single(d => d.Name == "Jenson Button");
                var entry = context.Entry(larry);
                entry.State = state;

                await entry.ReloadAsync(context);

                Assert.Equal(EntityState.Unchanged, entry.State);
            }
        }

        #endregion

        #region Helpers

        private const int StorePodiums = 20;
        private const int ClientPodiums = 30;

        protected virtual void ResolveConcurrencyTokens(InternalEntityEntry internalEntry)
        {
            // default do nothing. Allow provider-specific entry reset
        }

        protected F1Context CreateF1Context()
        {
            return Fixture.CreateContext(TestStore);
        }

        protected OptimisticConcurrencyTestBase(TFixture fixture)
        {
            Fixture = fixture;

            TestStore = Fixture.CreateTestStore();
        }

        public void Dispose()
        {
            TestStore.Dispose();
        }

        protected TFixture Fixture { get; }

        protected TTestStore TestStore { get; }

        private Task ConcurrencyTestAsync(int expectedPodiums, Action<F1Context, DbUpdateConcurrencyException> resolver)
        {
            return ConcurrencyTestAsync(
                c => c.Drivers.Single(d => d.CarNumber == 1).Podiums = StorePodiums,
                c => c.Drivers.Single(d => d.CarNumber == 1).Podiums = ClientPodiums,
                (c, ex) =>
                    {
                        Assert.IsType<DbUpdateConcurrencyException>(ex);

                        resolver(c, (DbUpdateConcurrencyException)ex);
                    },
                c => Assert.Equal(expectedPodiums, c.Drivers.Single(d => d.CarNumber == 1).Podiums));
        }

        /// <summary>
        ///     Runs the same action twice inside a transaction scope but with two different contexts and calling
        ///     SaveChanges such that first time it will succeed and then the second time it will result in a
        ///     concurrency exception.
        ///     After the exception is caught the resolver action is called, after which SaveChanges is called
        ///     again.  Finally, a new context is created and the validator is called so that the state of
        ///     the database at the end of the process can be validated.
        /// </summary>
        private Task ConcurrencyTestAsync(
            Action<F1Context> change, Action<F1Context, DbUpdateException> resolver,
            Action<F1Context> validator)
        {
            return ConcurrencyTestAsync(change, change, resolver, validator);
        }

        /// <summary>
        ///     Runs the two actions with two different contexts and calling
        ///     SaveChanges such that storeChange will succeed and the store will reflect this change, and
        ///     then clientChange will result in a concurrency exception.
        ///     After the exception is caught the resolver action is called, after which SaveChanges is called
        ///     again.  Finally, a new context is created and the validator is called so that the state of
        ///     the database at the end of the process can be validated.
        /// </summary>
        private async Task ConcurrencyTestAsync(
            Action<F1Context> storeChange, Action<F1Context> clientChange,
            Action<F1Context, DbUpdateException> resolver, Action<F1Context> validator)
        {
            using (var context = CreateF1Context())
            {
                clientChange(context);

                using (var innerContext = CreateF1Context())
                {
                    storeChange(innerContext);
                    await innerContext.SaveChangesAsync();
                }

                var updateException = await Assert.ThrowsAnyAsync<DbUpdateException>(() => context.SaveChangesAsync());

                using (var resolverContext = CreateF1Context())
                {
                    // TODO: pass in 'context' when no tracking queries are available
                    resolver(resolverContext, updateException);
                }

                using (var validationContext = CreateF1Context())
                {
                    if (validator != null)
                    {
                        await context.SaveChangesAsync();

                        validator(validationContext);
                    }
                }
            }
        }

        #endregion
    }
}

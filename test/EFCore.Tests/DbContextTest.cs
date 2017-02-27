// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.InMemory.FunctionalTests;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.EntityFrameworkCore.Storage.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests
{
    public class DbContextTest
    {
        [Fact]
        public void Set_throws_for_type_not_in_model()
        {
            var optionsBuilder = new DbContextOptionsBuilder();
            optionsBuilder
                .UseTransientInMemoryDatabase()
                .UseInternalServiceProvider(InMemoryTestHelpers.Instance.CreateServiceProvider());

            using (var context = new DbContext(optionsBuilder.Options))
            {
                var ex = Assert.Throws<InvalidOperationException>(() => context.Set<Category>());
                Assert.Equal(CoreStrings.InvalidSetType(nameof(Category)), ex.Message);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_services()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

            IServiceProvider contextServices;
            using (var context = new EarlyLearningCenter(serviceProvider))
            {
                contextServices = ((IInfrastructure<IServiceProvider>)context).Instance;
                Assert.Same(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }

            using (var context = new EarlyLearningCenter(serviceProvider))
            {
                Assert.NotSame(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_services_with_explicit_config()
        {
            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider();

            var options = new DbContextOptionsBuilder().UseInternalServiceProvider(serviceProvider).UseTransientInMemoryDatabase().Options;

            IServiceProvider contextServices;
            using (var context = new DbContext(options))
            {
                contextServices = ((IInfrastructure<IServiceProvider>)context).Instance;
                Assert.Same(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }

            using (var context = new DbContext(options))
            {
                Assert.NotSame(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }
        }

        [Fact]
        public void Each_context_gets_new_scoped_services_with_implicit_services_and_explicit_config()
        {
            var options = new DbContextOptionsBuilder().UseTransientInMemoryDatabase().Options;

            IServiceProvider contextServices;
            using (var context = new DbContext(options))
            {
                contextServices = ((IInfrastructure<IServiceProvider>)context).Instance;
                Assert.Same(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }

            using (var context = new DbContext(options))
            {
                Assert.NotSame(contextServices, ((IInfrastructure<IServiceProvider>)context).Instance);
            }
        }

        [Fact]
        public void SaveChanges_calls_DetectChanges()
        {
            var services = new ServiceCollection()
                .AddScoped<IStateManager, FakeStateManager>()
                .AddScoped<IChangeDetector, FakeChangeDetector>();

            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider(services);

            using (var context = new DbContext(
                new DbContextOptionsBuilder()
                    .UseInternalServiceProvider(serviceProvider)
                    .UseTransientInMemoryDatabase()
                    .Options))
            {
                var changeDetector = (FakeChangeDetector)context.GetService<IChangeDetector>();

                Assert.False(changeDetector.DetectChangesCalled);

                context.SaveChanges();

                Assert.True(changeDetector.DetectChangesCalled);
            }
        }

        [Fact]
        public void Entry_methods_check_arguments()
        {
            var services = new ServiceCollection()
                .AddScoped<IStateManager, FakeStateManager>();

            var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider(services);

            using (var context = new EarlyLearningCenter(serviceProvider))
            {
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(() => context.Entry(null)).ParamName);
                Assert.Equal(
                    "entity",
                    // ReSharper disable once AssignNullToNotNullAttribute
                    Assert.Throws<ArgumentNullException>(() => context.Entry<Random>(null)).ParamName);
            }
        }

        private class FakeStateManager : IStateManager
        {
            public IEnumerable<InternalEntityEntry> InternalEntries { get; set; }
            public bool SaveChangesCalled { get; set; }
            public bool SaveChangesAsyncCalled { get; set; }

            public TrackingQueryMode GetTrackingQueryMode(IEntityType entityType) => TrackingQueryMode.Multiple;

            public void EndSingleQueryMode()
            {
            }

            public void Reset()
            {
            }

            public void Unsubscribe()
            {
            }

            public void UpdateIdentityMap(InternalEntityEntry entry, IKey principalKey)
            {
                throw new NotImplementedException();
            }

            public void UpdateDependentMap(InternalEntityEntry entry, IForeignKey foreignKey)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<InternalEntityEntry> GetDependents(InternalEntityEntry principalEntry, IForeignKey foreignKey)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<InternalEntityEntry> GetDependentsUsingRelationshipSnapshot(InternalEntityEntry principalEntry, IForeignKey foreignKey)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<InternalEntityEntry> GetDependentsFromNavigation(InternalEntityEntry principalEntry, IForeignKey foreignKey)
            {
                throw new NotImplementedException();
            }

            public int SaveChanges(bool acceptAllChangesOnSuccess)
            {
                SaveChangesCalled = true;
                return 1;
            }

            public Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = new CancellationToken())
            {
                SaveChangesAsyncCalled = true;
                return Task.FromResult(1);
            }

            public virtual void AcceptAllChanges()
            {
                throw new NotImplementedException();
            }

            public InternalEntityEntry GetOrCreateEntry(object entity)
            {
                throw new NotImplementedException();
            }

            public InternalEntityEntry GetOrCreateEntry(object entity, IEntityType entityType)
            {
                throw new NotImplementedException();
            }

            public InternalEntityEntry StartTrackingFromQuery(
                IEntityType baseEntityType,
                object entity,
                ValueBuffer valueBuffer,
                ISet<IForeignKey> handledForeignKeys)
            {
                throw new NotImplementedException();
            }

            public void BeginTrackingQuery()
            {
                throw new NotImplementedException();
            }

            public InternalEntityEntry TryGetEntry(IKey key, object[] keyValues)
            {
                throw new NotImplementedException();
            }

            public InternalEntityEntry TryGetEntry(IKey key, ValueBuffer valueBuffer, bool throwOnNullKey)
            {
                throw new NotImplementedException();
            }

            public InternalEntityEntry TryGetEntry(object entity)
            {
                throw new NotImplementedException();
            }

            public InternalEntityEntry TryGetEntry(object entity, IEntityType type)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<InternalEntityEntry> Entries => Entries ?? Enumerable.Empty<InternalEntityEntry>();

            public int ChangedCount { get; set; }

            public IInternalEntityEntryNotifier Notify
            {
                get { throw new NotImplementedException(); }
            }

            public IValueGenerationManager ValueGeneration
            {
                get { throw new NotImplementedException(); }
            }

            public InternalEntityEntry StartTracking(InternalEntityEntry entry)
            {
                throw new NotImplementedException();
            }

            public void StopTracking(InternalEntityEntry entry)
            {
                throw new NotImplementedException();
            }

            public void RecordReferencedUntrackedEntity(object referencedEntity, INavigation navigation, InternalEntityEntry referencedFromEntry)
            {
                throw new NotImplementedException();
            }

            public IEnumerable<Tuple<INavigation, InternalEntityEntry>> GetRecordedReferers(object referencedEntity, bool clear)
            {
                throw new NotImplementedException();
            }

            public InternalEntityEntry GetPrincipal(InternalEntityEntry entityEntry, IForeignKey foreignKey)
            {
                throw new NotImplementedException();
            }

            public InternalEntityEntry GetPrincipalUsingPreStoreGeneratedValues(InternalEntityEntry entityEntry, IForeignKey foreignKey)
            {
                throw new NotImplementedException();
            }

            public InternalEntityEntry GetPrincipalUsingRelationshipSnapshot(InternalEntityEntry entityEntry, IForeignKey foreignKey)
            {
                throw new NotImplementedException();
            }

            public DbContext Context
            {
                get { throw new NotImplementedException(); }
            }
        }

        private class FakeChangeDetector : IChangeDetector
        {
            public bool DetectChangesCalled { get; set; }

            public void DetectChanges(IStateManager stateManager)
            {
                DetectChangesCalled = true;
            }

            public void DetectChanges(InternalEntityEntry entry)
            {
            }

            public void PropertyChanged(InternalEntityEntry entry, IPropertyBase property, bool setModifed)
            {
            }

            public void PropertyChanging(InternalEntityEntry entry, IPropertyBase property)
            {
            }

            public virtual void Suspend()
            {
            }

            public virtual void Resume()
            {
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Can_change_navigation_while_attaching_entities(bool async)
        {
            using (var context = new ActiveAddContext())
            {
                context.Database.EnsureDeleted();

                context.AddRange(new User { Id = 3 }, new User { Id = 4 });
                context.SaveChanges();
            }

            using (var context = new ActiveAddContext())
            {
                var questions = new List<Question>
                {
                    new Question
                    {
                        Author = context.Users.First(),
                        Answers = new List<Answer>
                        {
                            new Answer
                            {
                                Author = context.Users.Last(),
                            }
                        }
                    },
                };

                if (async)
                {
                    await context.AddRangeAsync(questions);
                }
                else
                {
                    context.AddRange(questions);
                }
            }
        }

        public class Question
        {
            public int Id { get; set; }
            public int AuthorId { get; set; }
            public virtual User Author { get; set; }
            public virtual ICollection<Answer> Answers { get; set; }
        }

        public class Answer
        {
            public int Id { get; set; }
            public int QuestionId { get; set; }
            public int AuthorId { get; set; }
            public virtual Question Question { get; set; }
            public virtual User Author { get; set; }
        }

        public class User
        {
            public int Id { get; set; }
            public virtual ICollection<Answer> Answers { get; set; } = new List<Answer>();
            public virtual ICollection<Question> Questions { get; set; } = new List<Question>();
        }

        public class ActiveAddContext : DbContext
        {
            public DbSet<User> Users { get; set; }
            public DbSet<Answer> Answers { get; set; }
            public DbSet<Question> Questions { get; set; }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder.UseInMemoryDatabase(databaseName: "issue7119");

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<Question>(b =>
                {
                    b.HasOne(x => x.Author).WithMany(x => x.Questions).HasForeignKey(x => x.AuthorId);
                });

                modelBuilder.Entity<Answer>(b =>
                {
                    b.HasOne(x => x.Author).WithMany(x => x.Answers).HasForeignKey(x => x.AuthorId);
                    b.HasOne(x => x.Question).WithMany(x => x.Answers).HasForeignKey(x => x.AuthorId);
                });
            }
        }

        [Fact]
        public async Task Can_add_existing_entities_to_context_to_be_deleted()
        {
            await TrackEntitiesTest((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);
        }

        [Fact]
        public async Task Can_add_new_entities_to_context_with_graph_method()
        {
            await TrackEntitiesTest((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_new_entities_to_context_with_graph_method_async()
        {
            await TrackEntitiesTest((c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_existing_entities_to_context_to_be_attached_with_graph_method()
        {
            await TrackEntitiesTest((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Unchanged);
        }

        [Fact]
        public async Task Can_add_existing_entities_to_context_to_be_updated_with_graph_method()
        {
            await TrackEntitiesTest((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);
        }

        private static Task TrackEntitiesTest(
            Func<DbContext, Category, EntityEntry<Category>> categoryAdder,
            Func<DbContext, Product, EntityEntry<Product>> productAdder, EntityState expectedState)
            => TrackEntitiesTest(
                (c, e) => Task.FromResult(categoryAdder(c, e)),
                (c, e) => Task.FromResult(productAdder(c, e)),
                expectedState);

        private static async Task TrackEntitiesTest(
            Func<DbContext, Category, Task<EntityEntry<Category>>> categoryAdder,
            Func<DbContext, Product, Task<EntityEntry<Product>>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var relatedDependent = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var principal = new Category { Id = 1, Name = "Beverages", Products = new List<Product> { relatedDependent } };

                var relatedPrincipal = new Category { Id = 2, Name = "Foods" };
                var dependent = new Product { Id = 2, Name = "Bovril", Price = 4.99m, Category = relatedPrincipal };

                var principalEntry = await categoryAdder(context, principal);
                var dependentEntry = await productAdder(context, dependent);

                var relatedPrincipalEntry = context.Entry(relatedPrincipal);
                var relatedDependentEntry = context.Entry(relatedDependent);

                Assert.Same(principal, principalEntry.Entity);
                Assert.Same(relatedPrincipal, relatedPrincipalEntry.Entity);
                Assert.Same(relatedDependent, relatedDependentEntry.Entity);
                Assert.Same(dependent, dependentEntry.Entity);

                var expectedRelatedState = expectedState == EntityState.Deleted ? EntityState.Unchanged : expectedState;

                Assert.Same(principal, principalEntry.Entity);
                Assert.Equal(expectedState, principalEntry.State);
                Assert.Same(relatedPrincipal, relatedPrincipalEntry.Entity);
                Assert.Equal(expectedRelatedState, relatedPrincipalEntry.State);

                Assert.Same(relatedDependent, relatedDependentEntry.Entity);
                Assert.Equal(expectedRelatedState, relatedDependentEntry.State);
                Assert.Same(dependent, dependentEntry.Entity);
                Assert.Equal(expectedState, dependentEntry.State);

                Assert.Same(principalEntry.GetInfrastructure(), context.Entry(principal).GetInfrastructure());
                Assert.Same(relatedPrincipalEntry.GetInfrastructure(), context.Entry(relatedPrincipal).GetInfrastructure());
                Assert.Same(relatedDependentEntry.GetInfrastructure(), context.Entry(relatedDependent).GetInfrastructure());
                Assert.Same(dependentEntry.GetInfrastructure(), context.Entry(dependent).GetInfrastructure());
            }
        }

        [Fact]
        public async Task Can_add_multiple_new_entities_to_context()
        {
            await TrackMultipleEntitiesTest((c, e) => c.AddRange(e[0], e[1]), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_new_entities_to_context_async()
        {
            await TrackMultipleEntitiesTest((c, e) => c.AddRangeAsync(e[0], e[1]), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_to_context_to_be_attached()
        {
            await TrackMultipleEntitiesTest((c, e) => c.AttachRange(e[0], e[1]), EntityState.Unchanged);
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_to_context_to_be_updated()
        {
            await TrackMultipleEntitiesTest((c, e) => c.UpdateRange(e[0], e[1]), EntityState.Modified);
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_to_context_to_be_deleted()
        {
            await TrackMultipleEntitiesTest((c, e) => c.RemoveRange(e[0], e[1]), EntityState.Deleted);
        }

        private static Task TrackMultipleEntitiesTest(
            Action<DbContext, object[]> adder,
            EntityState expectedState)
            => TrackMultipleEntitiesTest(
                (c, e) =>
                    {
                        adder(c, e);
                        return Task.FromResult(0);
                    },
                expectedState);

        private static async Task TrackMultipleEntitiesTest(
            Func<DbContext, object[], Task> adder,
            EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var relatedDependent = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var principal = new Category { Id = 1, Name = "Beverages", Products = new List<Product> { relatedDependent } };

                var relatedPrincipal = new Category { Id = 2, Name = "Foods" };
                var dependent = new Product { Id = 2, Name = "Bovril", Price = 4.99m, Category = relatedPrincipal };

                await adder(context, new object[] { principal, dependent });

                Assert.Same(principal, context.Entry(principal).Entity);
                Assert.Same(relatedPrincipal, context.Entry(relatedPrincipal).Entity);
                Assert.Same(relatedDependent, context.Entry(relatedDependent).Entity);
                Assert.Same(dependent, context.Entry(dependent).Entity);

                var expectedRelatedState = expectedState == EntityState.Deleted ? EntityState.Unchanged : expectedState;

                Assert.Same(principal, context.Entry(principal).Entity);
                Assert.Equal(expectedState, context.Entry(principal).State);
                Assert.Same(relatedPrincipal, context.Entry(relatedPrincipal).Entity);
                Assert.Equal(expectedRelatedState, context.Entry(relatedPrincipal).State);

                Assert.Same(relatedDependent, context.Entry(relatedDependent).Entity);
                Assert.Equal(expectedRelatedState, context.Entry(relatedDependent).State);
                Assert.Same(dependent, context.Entry(dependent).Entity);
                Assert.Equal(expectedState, context.Entry(dependent).State);
            }
        }

        [Fact]
        public async Task Can_add_existing_entities_with_default_value_to_context_to_be_deleted()
        {
            await TrackEntitiesDefaultValueTest((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);
        }

        [Fact]
        public async Task Can_add_new_entities_with_default_value_to_context_with_graph_method()
        {
            await TrackEntitiesDefaultValueTest((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_new_entities_with_default_value_to_context_with_graph_method_async()
        {
            await TrackEntitiesDefaultValueTest((c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_existing_entities_with_default_value_to_context_to_be_attached_with_graph_method()
        {
            await TrackEntitiesDefaultValueTest((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_existing_entities_with_default_value_to_context_to_be_updated_with_graph_method()
        {
            await TrackEntitiesDefaultValueTest((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Added);
        }

        private static Task TrackEntitiesDefaultValueTest(
            Func<DbContext, Category, EntityEntry<Category>> categoryAdder,
            Func<DbContext, Product, EntityEntry<Product>> productAdder, EntityState expectedState)
            => TrackEntitiesDefaultValueTest(
                (c, e) => Task.FromResult(categoryAdder(c, e)),
                (c, e) => Task.FromResult(productAdder(c, e)),
                expectedState);

        // Issue #3890
        private static async Task TrackEntitiesDefaultValueTest(
            Func<DbContext, Category, Task<EntityEntry<Category>>> categoryAdder,
            Func<DbContext, Product, Task<EntityEntry<Product>>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category1 = new Category { Id = 0, Name = "Beverages" };
                var product1 = new Product { Id = 0, Name = "Marmite", Price = 7.99m };

                var categoryEntry1 = await categoryAdder(context, category1);
                var productEntry1 = await productAdder(context, product1);

                Assert.Same(category1, categoryEntry1.Entity);
                Assert.Same(product1, productEntry1.Entity);

                Assert.Same(category1, categoryEntry1.Entity);
                Assert.Equal(expectedState, categoryEntry1.State);

                Assert.Same(product1, productEntry1.Entity);
                Assert.Equal(expectedState, productEntry1.State);

                Assert.Same(categoryEntry1.GetInfrastructure(), context.Entry(category1).GetInfrastructure());
                Assert.Same(productEntry1.GetInfrastructure(), context.Entry(product1).GetInfrastructure());
            }
        }

        [Fact]
        public async Task Can_add_multiple_new_entities_with_default_values_to_context()
        {
            await TrackMultipleEntitiesDefaultValuesTest((c, e) => c.AddRange(e[0]), (c, e) => c.AddRange(e[0]), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_new_entities_with_default_values_to_context_async()
        {
            await TrackMultipleEntitiesDefaultValuesTest((c, e) => c.AddRangeAsync(e[0]), (c, e) => c.AddRangeAsync(e[0]), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_attached()
        {
            await TrackMultipleEntitiesDefaultValuesTest((c, e) => c.AttachRange(e[0]), (c, e) => c.AttachRange(e[0]), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_updated()
        {
            await TrackMultipleEntitiesDefaultValuesTest((c, e) => c.UpdateRange(e[0]), (c, e) => c.UpdateRange(e[0]), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_deleted()
        {
            await TrackMultipleEntitiesDefaultValuesTest((c, e) => c.RemoveRange(e[0]), (c, e) => c.RemoveRange(e[0]), EntityState.Deleted);
        }

        private static Task TrackMultipleEntitiesDefaultValuesTest(
            Action<DbContext, object[]> categoryAdder,
            Action<DbContext, object[]> productAdder, EntityState expectedState)
            => TrackMultipleEntitiesDefaultValuesTest(
                (c, e) =>
                    {
                        categoryAdder(c, e);
                        return Task.FromResult(0);
                    },
                (c, e) =>
                    {
                        productAdder(c, e);
                        return Task.FromResult(0);
                    },
                expectedState);

        // Issue #3890
        private static async Task TrackMultipleEntitiesDefaultValuesTest(
            Func<DbContext, object[], Task> categoryAdder,
            Func<DbContext, object[], Task> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category1 = new Category { Id = 0, Name = "Beverages" };
                var product1 = new Product { Id = 0, Name = "Marmite", Price = 7.99m };

                await categoryAdder(context, new[] { category1 });
                await productAdder(context, new[] { product1 });

                Assert.Same(category1, context.Entry(category1).Entity);
                Assert.Same(product1, context.Entry(product1).Entity);

                Assert.Same(category1, context.Entry(category1).Entity);
                Assert.Equal(expectedState, context.Entry(category1).State);

                Assert.Same(product1, context.Entry(product1).Entity);
                Assert.Equal(expectedState, context.Entry(product1).State);
            }
        }

        [Fact]
        public void Can_add_no_new_entities_to_context()
        {
            TrackNoEntitiesTest(c => c.AddRange(), c => c.AddRange());
        }

        [Fact]
        public async Task Can_add_no_new_entities_to_context_async()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                await context.AddRangeAsync();
                await context.AddRangeAsync();
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_attached()
        {
            TrackNoEntitiesTest(c => c.AttachRange(), c => c.AttachRange());
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_updated()
        {
            TrackNoEntitiesTest(c => c.UpdateRange(), c => c.UpdateRange());
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_deleted()
        {
            TrackNoEntitiesTest(c => c.RemoveRange(), c => c.RemoveRange());
        }

        private static void TrackNoEntitiesTest(Action<DbContext> categoryAdder, Action<DbContext> productAdder)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                categoryAdder(context);
                productAdder(context);
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [Fact]
        public async Task Can_add_existing_entities_to_context_to_be_deleted_non_generic()
        {
            await TrackEntitiesTestNonGeneric((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);
        }

        [Fact]
        public async Task Can_add_new_entities_to_context_non_generic_graph()
        {
            await TrackEntitiesTestNonGeneric((c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_new_entities_to_context_non_generic_graph_async()
        {
            await TrackEntitiesTestNonGeneric((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_existing_entities_to_context_to_be_attached_non_generic_graph()
        {
            await TrackEntitiesTestNonGeneric((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Unchanged);
        }

        [Fact]
        public async Task Can_add_existing_entities_to_context_to_be_updated_non_generic_graph()
        {
            await TrackEntitiesTestNonGeneric((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Modified);
        }

        private static Task TrackEntitiesTestNonGeneric(
            Func<DbContext, object, EntityEntry> categoryAdder,
            Func<DbContext, object, EntityEntry> productAdder, EntityState expectedState)
            => TrackEntitiesTestNonGeneric(
                (c, e) => Task.FromResult(categoryAdder(c, e)),
                (c, e) => Task.FromResult(productAdder(c, e)),
                expectedState);

        private static async Task TrackEntitiesTestNonGeneric(
            Func<DbContext, object, Task<EntityEntry>> categoryAdder,
            Func<DbContext, object, Task<EntityEntry>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var relatedDependent = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var principal = new Category { Id = 1, Name = "Beverages", Products = new List<Product> { relatedDependent } };

                var relatedPrincipal = new Category { Id = 2, Name = "Foods" };
                var dependent = new Product { Id = 2, Name = "Bovril", Price = 4.99m, Category = relatedPrincipal };

                var principalEntry = await categoryAdder(context, principal);
                var dependentEntry = await productAdder(context, dependent);

                var relatedPrincipalEntry = context.Entry(relatedPrincipal);
                var relatedDependentEntry = context.Entry(relatedDependent);

                Assert.Same(principal, principalEntry.Entity);
                Assert.Same(relatedPrincipal, relatedPrincipalEntry.Entity);
                Assert.Same(relatedDependent, relatedDependentEntry.Entity);
                Assert.Same(dependent, dependentEntry.Entity);

                var expectedRelatedState = expectedState == EntityState.Deleted ? EntityState.Unchanged : expectedState;

                Assert.Same(principal, principalEntry.Entity);
                Assert.Equal(expectedState, principalEntry.State);
                Assert.Same(relatedPrincipal, relatedPrincipalEntry.Entity);
                Assert.Equal(expectedRelatedState, relatedPrincipalEntry.State);

                Assert.Same(relatedDependent, relatedDependentEntry.Entity);
                Assert.Equal(expectedRelatedState, relatedDependentEntry.State);
                Assert.Same(dependent, dependentEntry.Entity);
                Assert.Equal(expectedState, dependentEntry.State);

                Assert.Same(principalEntry.GetInfrastructure(), context.Entry(principal).GetInfrastructure());
                Assert.Same(relatedPrincipalEntry.GetInfrastructure(), context.Entry(relatedPrincipal).GetInfrastructure());
                Assert.Same(relatedDependentEntry.GetInfrastructure(), context.Entry(relatedDependent).GetInfrastructure());
                Assert.Same(dependentEntry.GetInfrastructure(), context.Entry(dependent).GetInfrastructure());
            }
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_to_context_to_be_deleted_Enumerable()
        {
            await TrackMultipleEntitiesTestEnumerable((c, e) => c.RemoveRange(e), EntityState.Deleted);
        }

        [Fact]
        public async Task Can_add_multiple_new_entities_to_context_Enumerable_graph()
        {
            await TrackMultipleEntitiesTestEnumerable((c, e) => c.AddRange(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_new_entities_to_context_Enumerable_graph_async()
        {
            await TrackMultipleEntitiesTestEnumerable((c, e) => c.AddRangeAsync(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_to_context_to_be_attached_Enumerable_graph()
        {
            await TrackMultipleEntitiesTestEnumerable((c, e) => c.AttachRange(e), EntityState.Unchanged);
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_to_context_to_be_updated_Enumerable_graph()
        {
            await TrackMultipleEntitiesTestEnumerable((c, e) => c.UpdateRange(e), EntityState.Modified);
        }

        private static Task TrackMultipleEntitiesTestEnumerable(
            Action<DbContext, IEnumerable<object>> adder,
            EntityState expectedState)
            => TrackMultipleEntitiesTestEnumerable(
                (c, e) =>
                    {
                        adder(c, e);
                        return Task.FromResult(0);
                    },
                expectedState);

        private static async Task TrackMultipleEntitiesTestEnumerable(
            Func<DbContext, IEnumerable<object>, Task> adder,
            EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var relatedDependent = new Product { Id = 1, Name = "Marmite", Price = 7.99m };
                var principal = new Category { Id = 1, Name = "Beverages", Products = new List<Product> { relatedDependent } };

                var relatedPrincipal = new Category { Id = 2, Name = "Foods" };
                var dependent = new Product { Id = 2, Name = "Bovril", Price = 4.99m, Category = relatedPrincipal };

                await adder(context, new object[] { principal, dependent });

                Assert.Same(principal, context.Entry(principal).Entity);
                Assert.Same(relatedPrincipal, context.Entry(relatedPrincipal).Entity);
                Assert.Same(relatedDependent, context.Entry(relatedDependent).Entity);
                Assert.Same(dependent, context.Entry(dependent).Entity);

                var expectedRelatedState = expectedState == EntityState.Deleted ? EntityState.Unchanged : expectedState;

                Assert.Same(principal, context.Entry(principal).Entity);
                Assert.Equal(expectedState, context.Entry(principal).State);
                Assert.Same(relatedPrincipal, context.Entry(relatedPrincipal).Entity);
                Assert.Equal(expectedRelatedState, context.Entry(relatedPrincipal).State);

                Assert.Same(relatedDependent, context.Entry(relatedDependent).Entity);
                Assert.Equal(expectedRelatedState, context.Entry(relatedDependent).State);
                Assert.Same(dependent, context.Entry(dependent).Entity);
                Assert.Equal(expectedState, context.Entry(dependent).State);
            }
        }

        [Fact]
        public async Task Can_add_existing_entities_with_default_value_to_context_to_be_deleted_non_generic()
        {
            await TrackEntitiesDefaultValuesTestNonGeneric((c, e) => c.Remove(e), (c, e) => c.Remove(e), EntityState.Deleted);
        }

        [Fact]
        public async Task Can_add_new_entities_with_default_value_to_context_non_generic_graph()
        {
            await TrackEntitiesDefaultValuesTestNonGeneric((c, e) => c.Add(e), (c, e) => c.Add(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_new_entities_with_default_value_to_context_non_generic_graph_async()
        {
            await TrackEntitiesDefaultValuesTestNonGeneric((c, e) => c.AddAsync(e), (c, e) => c.AddAsync(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_existing_entities_with_default_value_to_context_to_be_attached_non_generic_graph()
        {
            await TrackEntitiesDefaultValuesTestNonGeneric((c, e) => c.Attach(e), (c, e) => c.Attach(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_existing_entities_with_default_value_to_context_to_be_updated_non_generic_graph()
        {
            await TrackEntitiesDefaultValuesTestNonGeneric((c, e) => c.Update(e), (c, e) => c.Update(e), EntityState.Added);
        }

        private static Task TrackEntitiesDefaultValuesTestNonGeneric(
            Func<DbContext, object, EntityEntry> categoryAdder,
            Func<DbContext, object, EntityEntry> productAdder, EntityState expectedState)
            => TrackEntitiesDefaultValuesTestNonGeneric(
                (c, e) => Task.FromResult(categoryAdder(c, e)),
                (c, e) => Task.FromResult(productAdder(c, e)),
                expectedState);

        // Issue #3890
        private static async Task TrackEntitiesDefaultValuesTestNonGeneric(
            Func<DbContext, object, Task<EntityEntry>> categoryAdder,
            Func<DbContext, object, Task<EntityEntry>> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category1 = new Category { Id = 0, Name = "Beverages" };
                var product1 = new Product { Id = 0, Name = "Marmite", Price = 7.99m };

                var categoryEntry1 = await categoryAdder(context, category1);
                var productEntry1 = await productAdder(context, product1);

                Assert.Same(category1, categoryEntry1.Entity);
                Assert.Same(product1, productEntry1.Entity);

                Assert.Same(category1, categoryEntry1.Entity);
                Assert.Equal(expectedState, categoryEntry1.State);

                Assert.Same(product1, productEntry1.Entity);
                Assert.Equal(expectedState, productEntry1.State);

                Assert.Same(categoryEntry1.GetInfrastructure(), context.Entry(category1).GetInfrastructure());
                Assert.Same(productEntry1.GetInfrastructure(), context.Entry(product1).GetInfrastructure());
            }
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_deleted_Enumerable()
        {
            await TrackMultipleEntitiesDefaultValueTestEnumerable((c, e) => c.RemoveRange(e), (c, e) => c.RemoveRange(e), EntityState.Deleted);
        }

        [Fact]
        public async Task Can_add_multiple_new_entities_with_default_values_to_context_Enumerable_graph()
        {
            await TrackMultipleEntitiesDefaultValueTestEnumerable((c, e) => c.AddRange(e), (c, e) => c.AddRange(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_new_entities_with_default_values_to_context_Enumerable_graph_async()
        {
            await TrackMultipleEntitiesDefaultValueTestEnumerable((c, e) => c.AddRangeAsync(e), (c, e) => c.AddRangeAsync(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_attached_Enumerable_graph()
        {
            await TrackMultipleEntitiesDefaultValueTestEnumerable((c, e) => c.AttachRange(e), (c, e) => c.AttachRange(e), EntityState.Added);
        }

        [Fact]
        public async Task Can_add_multiple_existing_entities_with_default_values_to_context_to_be_updated_Enumerable_graph()
        {
            await TrackMultipleEntitiesDefaultValueTestEnumerable((c, e) => c.UpdateRange(e), (c, e) => c.UpdateRange(e), EntityState.Added);
        }

        private static Task TrackMultipleEntitiesDefaultValueTestEnumerable(
            Action<DbContext, IEnumerable<object>> categoryAdder,
            Action<DbContext, IEnumerable<object>> productAdder, EntityState expectedState)
            => TrackMultipleEntitiesDefaultValueTestEnumerable(
                (c, e) =>
                    {
                        categoryAdder(c, e);
                        return Task.FromResult(0);
                    },
                (c, e) =>
                    {
                        productAdder(c, e);
                        return Task.FromResult(0);
                    },
                expectedState);

        // Issue #3890
        private static async Task TrackMultipleEntitiesDefaultValueTestEnumerable(
            Func<DbContext, IEnumerable<object>, Task> categoryAdder,
            Func<DbContext, IEnumerable<object>, Task> productAdder, EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category1 = new Category { Id = 0, Name = "Beverages" };
                var product1 = new Product { Id = 0, Name = "Marmite", Price = 7.99m };

                await categoryAdder(context, new List<Category> { category1 });
                await productAdder(context, new List<Product> { product1 });

                Assert.Same(category1, context.Entry(category1).Entity);
                Assert.Same(product1, context.Entry(product1).Entity);

                Assert.Same(category1, context.Entry(category1).Entity);
                Assert.Equal(expectedState, context.Entry(category1).State);

                Assert.Same(product1, context.Entry(product1).Entity);
                Assert.Equal(expectedState, context.Entry(product1).State);
            }
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_deleted_Enumerable()
        {
            TrackNoEntitiesTestEnumerable((c, e) => c.RemoveRange(e), (c, e) => c.RemoveRange(e));
        }

        [Fact]
        public void Can_add_no_new_entities_to_context_Enumerable_graph()
        {
            TrackNoEntitiesTestEnumerable((c, e) => c.AddRange(e), (c, e) => c.AddRange(e));
        }

        [Fact]
        public async Task Can_add_no_new_entities_to_context_Enumerable_graph_async()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                await context.AddRangeAsync(new HashSet<Category>());
                await context.AddRangeAsync(new HashSet<Product>());
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_attached_Enumerable_graph()
        {
            TrackNoEntitiesTestEnumerable((c, e) => c.AttachRange(e), (c, e) => c.AttachRange(e));
        }

        [Fact]
        public void Can_add_no_existing_entities_to_context_to_be_updated_Enumerable_graph()
        {
            TrackNoEntitiesTestEnumerable((c, e) => c.UpdateRange(e), (c, e) => c.UpdateRange(e));
        }

        private static void TrackNoEntitiesTestEnumerable(
            Action<DbContext, IEnumerable<object>> categoryAdder,
            Action<DbContext, IEnumerable<object>> productAdder)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                categoryAdder(context, new HashSet<Category>());
                productAdder(context, new HashSet<Product>());
                Assert.Empty(context.ChangeTracker.Entries());
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task Can_add_new_entities_to_context_with_key_generation_graph(bool async)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var gu1 = new TheGu { ShirtColor = "Red" };
                var gu2 = new TheGu { ShirtColor = "Still Red" };

                if (async)
                {
                    Assert.Same(gu1, (await context.AddAsync(gu1)).Entity);
                    Assert.Same(gu2, (await context.AddAsync(gu2)).Entity);
                }
                else
                {
                    Assert.Same(gu1, context.Add(gu1).Entity);
                    Assert.Same(gu2, context.Add(gu2).Entity);
                }

                Assert.NotEqual(default(Guid), gu1.Id);
                Assert.NotEqual(default(Guid), gu2.Id);
                Assert.NotEqual(gu1.Id, gu2.Id);

                var categoryEntry = context.Entry(gu1);
                Assert.Same(gu1, categoryEntry.Entity);
                Assert.Equal(EntityState.Added, categoryEntry.State);

                categoryEntry = context.Entry(gu2);
                Assert.Same(gu2, categoryEntry.Entity);
                Assert.Equal(EntityState.Added, categoryEntry.State);
            }
        }

        [Fact]
        public async Task Can_use_Remove_to_change_entity_state()
        {
            await ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Detached, EntityState.Deleted);
            await ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Unchanged, EntityState.Deleted);
            await ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Deleted, EntityState.Deleted);
            await ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Modified, EntityState.Deleted);
            await ChangeStateWithMethod((c, e) => c.Remove(e), EntityState.Added, EntityState.Detached);
        }

        [Fact]
        public async Task Can_use_graph_Add_to_change_entity_state()
        {
            await ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Detached, EntityState.Added);
            await ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Unchanged, EntityState.Added);
            await ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Deleted, EntityState.Added);
            await ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Modified, EntityState.Added);
            await ChangeStateWithMethod((c, e) => c.Add(e), EntityState.Added, EntityState.Added);
        }

        [Fact]
        public async Task Can_use_graph_Add_to_change_entity_state_async()
        {
            await ChangeStateWithMethod((c, e) => c.AddAsync(e), EntityState.Detached, EntityState.Added);
            await ChangeStateWithMethod((c, e) => c.AddAsync(e), EntityState.Unchanged, EntityState.Added);
            await ChangeStateWithMethod((c, e) => c.AddAsync(e), EntityState.Deleted, EntityState.Added);
            await ChangeStateWithMethod((c, e) => c.AddAsync(e), EntityState.Modified, EntityState.Added);
            await ChangeStateWithMethod((c, e) => c.AddAsync(e), EntityState.Added, EntityState.Added);
        }

        [Fact]
        public async Task Can_use_graph_Attach_to_change_entity_state()
        {
            await ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Detached, EntityState.Unchanged);
            await ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Unchanged, EntityState.Unchanged);
            await ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Deleted, EntityState.Unchanged);
            await ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Modified, EntityState.Unchanged);
            await ChangeStateWithMethod((c, e) => c.Attach(e), EntityState.Added, EntityState.Unchanged);
        }

        [Fact]
        public async Task Can_use_graph_Update_to_change_entity_state()
        {
            await ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Detached, EntityState.Modified);
            await ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Unchanged, EntityState.Modified);
            await ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Deleted, EntityState.Modified);
            await ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Modified, EntityState.Modified);
            await ChangeStateWithMethod((c, e) => c.Update(e), EntityState.Added, EntityState.Modified);
        }

        private Task ChangeStateWithMethod(
            Action<DbContext, object> action,
            EntityState initialState,
            EntityState expectedState)
            => ChangeStateWithMethod((c, e) =>
                    {
                        action(c, e);
                        return Task.FromResult(0);
                    },
                initialState,
                expectedState);

        private async Task ChangeStateWithMethod(
            Func<DbContext, object, Task> action,
            EntityState initialState,
            EntityState expectedState)
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var entity = new Category { Id = 1, Name = "Beverages" };
                var entry = context.Entry(entity);

                entry.State = initialState;

                await action(context, entity);

                Assert.Equal(expectedState, entry.State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_fully_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                // Dependent is Unchanged here because the FK change happened before it was attached
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_fully_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_collection_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Attach(category);

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_collection_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_reference_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Null(product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_reference_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Attach(product);

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Null(product.Category);
                Assert.Equal(EntityState.Detached, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_fully_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                // Dependent is Unchanged here because the FK change happened before it was attached
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_fully_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Detached, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_collection_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_collection_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Detached, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_reference_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Null(product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_reference_not_fixed_up()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Null(product.Category);
                Assert.Equal(EntityState.Detached, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_fully_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Same(product, category.Products.Single());
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);

                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_fully_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Attach(product);

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Detached, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_collection_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Attach(category);

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_collection_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Attach(product);

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Detached, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_principal_first_reference_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Null(product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Attach(product);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Same(product, category.Products.Single());
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_attach_with_inconsistent_FK_dependent_first_reference_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Attach(product);

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Detached, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Attach(category);

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_fully_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Same(product, category.Products.Single());
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_fully_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product> { product };

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Detached, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_collection_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category, product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_collection_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite", Category = category };
                category.Products = new List<Product>();

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Detached, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Empty(category.Products);
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_principal_first_reference_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Null(product.Category);
                Assert.Empty(category7.Products);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Detached, context.Entry(product).State);

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Same(product, category.Products.Single());
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact] // Issue #1246
        public void Can_set_set_to_Unchanged_with_inconsistent_FK_dependent_first_reference_not_fixed_up_with_tracked_FK_match()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var category7 = context.Attach(new Category { Id = 7, Products = new List<Product>() }).Entity;

                var category = new Category { Id = 1, Name = "Beverages" };
                var product = new Product { Id = 1, CategoryId = 7, Name = "Marmite" };
                category.Products = new List<Product> { product };

                context.Entry(product).State = EntityState.Unchanged;

                Assert.Equal(7, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category7, product.Category);
                Assert.Same(product, category7.Products.Single());
                Assert.Equal(EntityState.Detached, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);

                context.Entry(category).State = EntityState.Unchanged;

                Assert.Equal(1, product.CategoryId);
                Assert.Same(product, category.Products.Single());
                Assert.Same(category, product.Category);
                Assert.Equal(EntityState.Unchanged, context.Entry(category).State);
                Assert.Equal(EntityState.Unchanged, context.Entry(product).State);
            }
        }

        [Fact]
        public void Context_can_build_model_using_DbSet_properties()
        {
            using (var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                Assert.Equal(
                    new[] { typeof(Category).FullName, typeof(Product).FullName, typeof(TheGu).FullName },
                    context.Model.GetEntityTypes().Select(e => e.Name).ToArray());

                var categoryType = context.Model.FindEntityType(typeof(Category));
                Assert.Equal("Id", categoryType.FindPrimaryKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "Name" },
                    categoryType.GetProperties().Select(p => p.Name).ToArray());

                var productType = context.Model.FindEntityType(typeof(Product));
                Assert.Equal("Id", productType.FindPrimaryKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "CategoryId", "Name", "Price" },
                    productType.GetProperties().Select(p => p.Name).ToArray());

                var guType = context.Model.FindEntityType(typeof(TheGu));
                Assert.Equal("Id", guType.FindPrimaryKey().Properties.Single().Name);
                Assert.Equal(
                    new[] { "Id", "ShirtColor" },
                    guType.GetProperties().Select(p => p.Name).ToArray());
            }
        }

        [Fact]
        public void Context_will_use_explicit_model_if_set_in_config()
        {
            var model = new Model();
            model.AddEntityType(typeof(TheGu));

            using (var context = new EarlyLearningCenter(
                InMemoryTestHelpers.Instance.CreateServiceProvider(),
                new DbContextOptionsBuilder().UseModel(model).Options))
            {
                Assert.Equal(
                    new[] { typeof(TheGu).FullName },
                    context.Model.GetEntityTypes().Select(e => e.Name).ToArray());
            }
        }

        [Fact]
        public void Context_initializes_all_DbSet_properties_with_setters()
        {
            using (var context = new ContextWithSets())
            {
                Assert.NotNull(context.Products);
                Assert.NotNull(context.Categories);
                Assert.NotNull(context.GetGus());
                Assert.Null(context.NoSetter);
            }
        }

        private class ContextWithSets : DbContext
        {
            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; private set; }
            private DbSet<TheGu> Gus { get; set; }

            public DbSet<Random> NoSetter { get; } = null;

            public DbSet<TheGu> GetGus()
            {
                return Gus;
            }
        }

        [Fact]
        public void Default_services_are_registered_when_parameterless_constructor_used()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.IsType<DbSetFinder>(context.GetService<IDbSetFinder>());
            }
        }

        [Fact]
        public void Default_context_scoped_services_are_registered_when_parameterless_constructor_used()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.IsType<InternalEntityEntryFactory>(context.GetService<IInternalEntityEntryFactory>());
            }
        }

        [Fact]
        public void Can_get_singleton_service_from_scoped_configuration()
        {
            using (var context = new EarlyLearningCenter())
            {
                Assert.IsType<StateManager>(context.GetService<IStateManager>());
            }
        }

        [Fact]
        public void Can_start_with_custom_services_by_passing_in_base_service_provider()
        {
            var factory = Mock.Of<INavigationFixer>();

            var provider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(factory)
                .BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                Assert.Same(factory, context.GetService<INavigationFixer>());
            }
        }

        [Fact]
        public void Required_low_level_services_are_added_if_needed()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();
            var provider = serviceCollection.BuildServiceProvider();

            Assert.IsType<LoggerFactory>(provider.GetRequiredService<ILoggerFactory>());
        }

        [Fact]
        public void Required_low_level_services_are_not_added_if_already_present()
        {
            var serviceCollection = new ServiceCollection();
            var loggerFactory = new FakeLoggerFactory();

            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);

            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

            var provider = serviceCollection.BuildServiceProvider();

            Assert.Same(loggerFactory, provider.GetRequiredService<ILoggerFactory>());
        }

        [Fact]
        public void Low_level_services_can_be_replaced_after_being_added()
        {
            var serviceCollection = new ServiceCollection();
            var loggerFactory = new FakeLoggerFactory();

            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

            serviceCollection.AddSingleton<ILoggerFactory>(loggerFactory);

            var provider = serviceCollection.BuildServiceProvider();

            Assert.Same(loggerFactory, provider.GetRequiredService<ILoggerFactory>());
        }

        [Fact]
        public void Can_replace_already_registered_service_with_new_service()
        {
            var factory = Mock.Of<INavigationFixer>();
            var provider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton(factory)
                .BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                Assert.Same(factory, context.GetService<INavigationFixer>());
            }
        }

        [Fact]
        public void Can_set_known_singleton_services_using_instance_sugar()
        {
            var modelSource = Mock.Of<IModelSource>();

            var services = new ServiceCollection()
                .AddSingleton(modelSource);

            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider(services);

            using (var context = new EarlyLearningCenter(provider))
            {
                Assert.Same(modelSource, context.GetService<IModelSource>());
            }
        }

        [Fact]
        public void Can_set_known_singleton_services_using_type_activation()
        {
            var services = new ServiceCollection()
                .AddSingleton<IModelSource, FakeModelSource>();

            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider(services);

            using (var context = new EarlyLearningCenter(provider))
            {
                Assert.IsType<FakeModelSource>(context.GetService<IModelSource>());
            }
        }

        [Fact]
        public void Can_set_known_context_scoped_services_using_type_activation()
        {
            var services = new ServiceCollection()
                .AddScoped<IStateManager, FakeStateManager>();

            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider(services);

            using (var context = new EarlyLearningCenter(provider))
            {
                Assert.IsType<FakeStateManager>(context.GetService<IStateManager>());
            }
        }

        [Fact]
        public void Replaced_services_are_scoped_appropriately()
        {
            var provider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<IModelSource, FakeModelSource>()
                .AddScoped<IStateManager, FakeStateManager>()
                .BuildServiceProvider();

            var context = new EarlyLearningCenter(provider);

            var modelSource = context.GetService<IModelSource>();

            context.Dispose();

            context = new EarlyLearningCenter(provider);

            var stateManager = context.GetService<IStateManager>();

            Assert.Same(stateManager, context.GetService<IStateManager>());

            Assert.Same(modelSource, context.GetService<IModelSource>());

            context.Dispose();

            context = new EarlyLearningCenter(provider);

            Assert.NotSame(stateManager, context.GetService<IStateManager>());

            Assert.Same(modelSource, context.GetService<IModelSource>());

            context.Dispose();
        }

        [Fact]
        public void Can_get_replaced_singleton_service_from_scoped_configuration()
        {
            var provider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddSingleton<IEntityMaterializerSource, FakeEntityMaterializerSource>()
                .BuildServiceProvider();

            using (var context = new EarlyLearningCenter(provider))
            {
                Assert.IsType<FakeEntityMaterializerSource>(context.GetService<IEntityMaterializerSource>());
            }
        }

        private class Category
        {
            public int Id { get; set; }
            public string Name { get; set; }

            public List<Product> Products { get; set; }
        }

        private class Product
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public decimal Price { get; set; }

            public int CategoryId { get; set; }
            public Category Category { get; set; }
        }

        private class TheGu
        {
            public Guid Id { get; set; }
            public string ShirtColor { get; set; }
        }

        private class EarlyLearningCenter : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public EarlyLearningCenter()
            {
            }

            public EarlyLearningCenter(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public EarlyLearningCenter(IServiceProvider serviceProvider, DbContextOptions options)
                : base(options)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Product> Products { get; set; }
            public DbSet<Category> Categories { get; set; }
            public DbSet<TheGu> Gus { get; set; }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseTransientInMemoryDatabase()
                    .UseInternalServiceProvider(_serviceProvider);

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder
                    .Entity<Category>().HasMany(e => e.Products).WithOne(e => e.Category);
            }
        }

        private class FakeEntityMaterializerSource : EntityMaterializerSource
        {
        }

        private class FakeLoggerFactory : ILoggerFactory
        {
            public ILogger CreateLogger(string name) => null;

            public void AddProvider(ILoggerProvider provider)
            {
            }

            public void Dispose()
            {
            }
        }

        private class FakeModelSource : IModelSource
        {
            public virtual IModel GetModel(DbContext context, IConventionSetBuilder conventionSetBuilder, IModelValidator validator = null)
                => null;
        }

        [Fact]
        public void Can_use_derived_context()
        {
            var singleton = new object[3];

            using (var context = new ConstructorTestContextWithOC1A())
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var context = new ConstructorTestContextWithOC1A())
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
            }
        }

        [Fact]
        public void Can_use_derived_context_with_external_services()
        {
            var appServiceProivder = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider();

            var loggerFactory = new WrappingLoggerFactory(appServiceProivder.GetService<ILoggerFactory>());
            var memoryCache = appServiceProivder.GetService<IMemoryCache>();

            IInMemoryStoreCache singleton;

            using (var context = new ConstructorTestContextWithOC1B(loggerFactory, memoryCache))
            {
                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
                Assert.Contains("System.Random", loggerFactory.CreatedLoggers);
            }

            using (var context = new ConstructorTestContextWithOC1B(loggerFactory, memoryCache))
            {
                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
            }
        }

        [Fact]
        public void Can_use_derived_context_with_options()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseTransientInMemoryDatabase()
                .Options;

            var singleton = new object[3];

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_use_derived_context_with_options_and_external_services()
        {
            var appServiceProivder = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider();

            var loggerFactory = new WrappingLoggerFactory(appServiceProivder.GetService<ILoggerFactory>());
            var memoryCache = appServiceProivder.GetService<IMemoryCache>();

            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseTransientInMemoryDatabase()
                .UseLoggerFactory(loggerFactory)
                .UseMemoryCache(memoryCache)
                .Options;

            IInMemoryStoreCache singleton;

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
                Assert.Contains("System.Random", loggerFactory.CreatedLoggers);
            }

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_use_derived_context_controlling_internal_services()
        {
            var internalServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var singleton = new object[3];

            using (var context = new ConstructorTestContextWithOC2A(internalServiceProivder))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());

                Assert.NotNull(context.GetService<ILogger<Random>>());

                Assert.Same(singleton[0], internalServiceProivder.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], internalServiceProivder.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], internalServiceProivder.GetService<IMemoryCache>());
            }

            using (var context = new ConstructorTestContextWithOC2A(internalServiceProivder))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
            }
        }

        [Fact]
        public void Can_use_derived_context_controlling_internal_services_with_options()
        {
            var internalServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseTransientInMemoryDatabase()
                .UseInternalServiceProvider(internalServiceProivder)
                .Options;

            var singleton = new object[3];

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());

                Assert.Same(singleton[0], internalServiceProivder.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], internalServiceProivder.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], internalServiceProivder.GetService<IMemoryCache>());
            }

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_use_derived_context_with_options_no_OnConfiguring()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseTransientInMemoryDatabase()
                .Options;

            var singleton = new object[3];

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_use_derived_context_with_options_and_external_services_no_OnConfiguring()
        {
            var appServiceProivder = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider();

            var loggerFactory = new WrappingLoggerFactory(appServiceProivder.GetService<ILoggerFactory>());
            var memoryCache = appServiceProivder.GetService<IMemoryCache>();

            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseTransientInMemoryDatabase()
                .UseLoggerFactory(loggerFactory)
                .UseMemoryCache(memoryCache)
                .Options;

            IInMemoryStoreCache singleton;

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
                Assert.Contains("System.Random", loggerFactory.CreatedLoggers);
            }

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_use_derived_context_controlling_internal_services_with_options_no_OnConfiguring()
        {
            var internalServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseTransientInMemoryDatabase()
                .UseInternalServiceProvider(internalServiceProivder)
                .Options;

            var singleton = new object[3];

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());

                Assert.Same(singleton[0], internalServiceProivder.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], internalServiceProivder.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], internalServiceProivder.GetService<IMemoryCache>());
            }

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_use_non_derived_context_with_options()
        {
            var options = new DbContextOptionsBuilder()
                .UseTransientInMemoryDatabase()
                .Options;

            var singleton = new object[3];

            using (var context = new DbContext(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var context = new DbContext(options))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_use_non_derived_context_with_options_and_external_services()
        {
            var appServiceProivder = new ServiceCollection()
                .AddLogging()
                .AddMemoryCache()
                .BuildServiceProvider();

            var loggerFactory = new WrappingLoggerFactory(appServiceProivder.GetService<ILoggerFactory>());
            var memoryCache = appServiceProivder.GetService<IMemoryCache>();

            var options = new DbContextOptionsBuilder()
                .UseTransientInMemoryDatabase()
                .UseLoggerFactory(loggerFactory)
                .UseMemoryCache(memoryCache)
                .Options;

            IInMemoryStoreCache singleton;

            using (var context = new DbContext(options))
            {
                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
                Assert.Contains("System.Random", loggerFactory.CreatedLoggers);
            }

            using (var context = new DbContext(options))
            {
                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_use_non_derived_context_controlling_internal_services_with_options()
        {
            var internalServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var options = new DbContextOptionsBuilder()
                .UseTransientInMemoryDatabase()
                .UseInternalServiceProvider(internalServiceProivder)
                .Options;

            var singleton = new object[3];

            using (var context = new DbContext(options))
            {
                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());

                Assert.Same(singleton[0], internalServiceProivder.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], internalServiceProivder.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], internalServiceProivder.GetService<IMemoryCache>());
            }

            using (var context = new DbContext(options))
            {
                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_derived_context()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC1A>()
                .BuildServiceProvider();

            var singleton = new object[3];
            DbContext context1;
            DbContext context2;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                context1 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<ILogger<Random>>());
            }

            Assert.Throws<ObjectDisposedException>(() => context1.Model);

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                context2 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                Assert.Same(singleton[0], context2.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context2.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context2.GetService<IMemoryCache>());
            }

            Assert.NotSame(context1, context2);
            Assert.Throws<ObjectDisposedException>(() => context2.Model);
        }

        [Fact]
        public void Can_add_derived_context_with_external_services()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC1B>()
                .BuildServiceProvider();

            var loggerFactory = appServiceProivder.GetService<ILoggerFactory>();
            var memoryCache = appServiceProivder.GetService<IMemoryCache>();

            IInMemoryStoreCache singleton;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1B>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1B>();

                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
            }
        }

        [Fact]
        public void Can_add_derived_context_with_options()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(b => b.UseTransientInMemoryDatabase())
                .BuildServiceProvider();

            var singleton = new object[4];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[3] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(singleton[3], context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_derived_context_with_options_and_external_services()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(b => b.UseTransientInMemoryDatabase())
                .BuildServiceProvider();

            var loggerFactory = appServiceProivder.GetService<ILoggerFactory>();
            var memoryCache = appServiceProivder.GetService<IMemoryCache>();

            IInMemoryStoreCache singleton;
            IDbContextOptions options;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotNull(options = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_derived_context_controlling_internal_services()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC2A>()
                .BuildServiceProvider();

            var singleton = new object[3];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC2A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC2A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
            }
        }

        [Fact]
        public void Can_add_derived_context_controlling_internal_services_with_options()
        {
            var internalServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    b => b.UseTransientInMemoryDatabase()
                        .UseInternalServiceProvider(internalServiceProivder))
                .BuildServiceProvider();

            var singleton = new object[4];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[3] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(singleton[3], context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_derived_context_one_service_provider_with_options()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p))
                .BuildServiceProvider();

            var singleton = new object[4];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[3] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(singleton[3], context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_derived_context_one_service_provider_with_options_and_external_services()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p))
                .BuildServiceProvider();

            var loggerFactory = appServiceProivder.GetService<ILoggerFactory>();
            var memoryCache = appServiceProivder.GetService<IMemoryCache>();

            IInMemoryStoreCache singleton;
            IDbContextOptions options;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotNull(options = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_derived_context_with_options_no_OnConfiguring()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContext1A>(b => b.UseTransientInMemoryDatabase())
                .BuildServiceProvider();

            var singleton = new object[4];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[3] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(singleton[3], context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_derived_context_with_options_and_external_services_no_OnConfiguring()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContext1A>(b => b.UseTransientInMemoryDatabase())
                .BuildServiceProvider();

            var loggerFactory = appServiceProivder.GetService<ILoggerFactory>();
            var memoryCache = appServiceProivder.GetService<IMemoryCache>();

            IInMemoryStoreCache singleton;
            IDbContextOptions options;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotNull(options = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_derived_context_controlling_internal_services_with_options_no_OnConfiguring()
        {
            var internalServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContext1A>(
                    b => b.UseTransientInMemoryDatabase()
                        .UseInternalServiceProvider(internalServiceProivder))
                .BuildServiceProvider();

            var singleton = new object[4];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[3] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(singleton[3], context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_derived_context_one_provider_with_options_no_OnConfiguring()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p))
                .BuildServiceProvider();

            var singleton = new object[4];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[3] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(singleton[3], context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_derived_context_one_provider_with_options_and_external_services_no_OnConfiguring()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p))
                .BuildServiceProvider();

            var loggerFactory = appServiceProivder.GetService<ILoggerFactory>();
            var memoryCache = appServiceProivder.GetService<IMemoryCache>();

            IInMemoryStoreCache singleton;
            IDbContextOptions options;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotNull(options = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_non_derived_context_with_options()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<DbContext>(b => b.UseTransientInMemoryDatabase())
                .BuildServiceProvider();

            var singleton = new object[4];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[3] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(singleton[3], context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_non_derived_context_with_options_and_external_services()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<DbContext>(
                    (p, b) => b.UseTransientInMemoryDatabase()
                        .UseMemoryCache(p.GetService<IMemoryCache>())
                        .UseLoggerFactory(p.GetService<ILoggerFactory>()))
                .BuildServiceProvider();

            var loggerFactory = appServiceProivder.GetService<ILoggerFactory>();
            var memoryCache = appServiceProivder.GetService<IMemoryCache>();

            IDbContextOptions options;
            IInMemoryStoreCache singleton;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(singleton = context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.NotNull(options = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton, context.GetService<IInMemoryStoreCache>());
                Assert.Same(loggerFactory, context.GetService<ILoggerFactory>());
                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
                Assert.Same(options, context.GetService<IDbContextOptions>());
            }
        }

        [Fact]
        public void Can_add_non_derived_context_controlling_internal_services_with_options()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DbContext>((p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p))
                .BuildServiceProvider();

            var singleton = new object[4];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(singleton[0] = context.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context.GetService<IMemoryCache>());
                Assert.NotNull(singleton[3] = context.GetService<IDbContextOptions>());

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());
                Assert.Same(singleton[3], context.GetService<IDbContextOptions>());
            }
        }


        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Can_add_derived_context_as_singleton(bool addSingletonFirst, bool useDbContext)
        {
            var appServiceProivder = useDbContext
                ? new ServiceCollection()
                    .AddDbContext<ConstructorTestContextWithOC1A>(ServiceLifetime.Singleton)
                    .BuildServiceProvider()
                : (addSingletonFirst
                    ? new ServiceCollection()
                        .AddSingleton<ConstructorTestContextWithOC1A>()
                        .AddDbContext<ConstructorTestContextWithOC1A>()
                        .BuildServiceProvider()
                    : new ServiceCollection()
                        .AddDbContext<ConstructorTestContextWithOC1A>()
                        .AddSingleton<ConstructorTestContextWithOC1A>()
                        .BuildServiceProvider());

            var singleton = new object[3];
            DbContext context1;
            DbContext context2;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                context1 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<ILogger<Random>>());
            }

            Assert.NotNull(context1.Model);

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                context2 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                Assert.Same(singleton[0], context2.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context2.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context2.GetService<IMemoryCache>());
            }

            Assert.Same(context1, context2);
            Assert.Same(context1.Model, context2.Model);
        }

        [Fact]
        public void Throws_when_used_with_parameterless_constructor_context()
        {
            var serviceCollection = new ServiceCollection();

            Assert.Equal(CoreStrings.DbContextMissingConstructor(nameof(ConstructorTestContextWithOC1A)),
                Assert.Throws<ArgumentException>(
                    () => serviceCollection.AddDbContext<ConstructorTestContextWithOC1A>(
                        _ => { })).Message);

            Assert.Equal(CoreStrings.DbContextMissingConstructor(nameof(ConstructorTestContextWithOC1A)),
                Assert.Throws<ArgumentException>(
                    () => serviceCollection.AddDbContext<ConstructorTestContextWithOC1A>(
                        (_, __) => { })).Message);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Can_add_derived_context_as_singleton_controlling_internal_services(bool addSingletonFirst, bool useDbContext)
        {
            var appServiceProivder = useDbContext
                ? new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddDbContext<ConstructorTestContextWithOC3A>(
                        (p, b) => b.UseInternalServiceProvider(p).UseTransientInMemoryDatabase(),
                        ServiceLifetime.Singleton)
                    .BuildServiceProvider()
                : (addSingletonFirst
                    ? new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddSingleton<ConstructorTestContextWithOC3A>()
                        .AddDbContext<ConstructorTestContextWithOC3A>((p, b) => b.UseInternalServiceProvider(p).UseTransientInMemoryDatabase())
                        .BuildServiceProvider()
                    : new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddDbContext<ConstructorTestContextWithOC3A>((p, b) => b.UseInternalServiceProvider(p).UseTransientInMemoryDatabase())
                        .AddSingleton<ConstructorTestContextWithOC3A>()
                        .BuildServiceProvider());

            var singleton = new object[3];
            DbContext context1;
            DbContext context2;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                context1 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<ILogger<Random>>());
            }

            Assert.NotNull(context1.Model);

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                context2 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton[0], context2.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context2.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context2.GetService<IMemoryCache>());
            }

            Assert.Same(context1, context2);
            Assert.Same(context1.Model, context2.Model);
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Can_add_derived_context_as_transient(bool addTransientFirst, bool useDbContext)
        {
            var appServiceProivder = useDbContext
                ? new ServiceCollection()
                    .AddDbContext<ConstructorTestContextWithOC1A>(ServiceLifetime.Transient)
                    .BuildServiceProvider()
                : (addTransientFirst
                    ? new ServiceCollection()
                        .AddTransient<ConstructorTestContextWithOC1A>()
                        .AddDbContext<ConstructorTestContextWithOC1A>()
                        .BuildServiceProvider()
                    : new ServiceCollection()
                        .AddDbContext<ConstructorTestContextWithOC1A>()
                        .AddTransient<ConstructorTestContextWithOC1A>()
                        .BuildServiceProvider());

            var singleton = new object[3];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context1 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();
                var context2 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                Assert.NotSame(context1, context2);

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<ILogger<Random>>());

                context1.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context1.Model);
                Assert.NotNull(context2.Model);

                context2.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context2.Model);
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC1A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());

                context.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context.Model);
            }
        }

        [Theory]
        [InlineData(true, false)]
        [InlineData(false, false)]
        [InlineData(true, true)]
        public void Can_add_derived_context_as_transient_controlling_internal_services(bool addTransientFirst, bool useDbContext)
        {
            var appServiceProivder = useDbContext
                ? new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .AddDbContext<ConstructorTestContextWithOC3A>(
                        (p, b) => b.UseInternalServiceProvider(p).UseTransientInMemoryDatabase(),
                        ServiceLifetime.Transient)
                    .BuildServiceProvider()
                : (addTransientFirst
                    ? new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddTransient<ConstructorTestContextWithOC3A>()
                        .AddDbContext<ConstructorTestContextWithOC3A>((p, b) => b.UseInternalServiceProvider(p).UseTransientInMemoryDatabase())
                        .BuildServiceProvider()
                    : new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .AddDbContext<ConstructorTestContextWithOC3A>((p, b) => b.UseInternalServiceProvider(p).UseTransientInMemoryDatabase())
                        .AddTransient<ConstructorTestContextWithOC3A>()
                        .BuildServiceProvider());

            var singleton = new object[3];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context1 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();
                var context2 = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotSame(context1, context2);

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<ILogger<Random>>());

                context1.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context1.Model);
                Assert.NotNull(context2.Model);

                context2.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context2.Model);
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());

                context.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context.Model);
            }
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_add_non_derived_context_as_singleton(bool addSingletonFirst)
        {
            var appServiceProivder = addSingletonFirst
                ? new ServiceCollection()
                    .AddSingleton<DbContext>()
                    .AddDbContext<DbContext>(b => b.UseTransientInMemoryDatabase())
                    .BuildServiceProvider()
                : new ServiceCollection()
                    .AddDbContext<DbContext>(b => b.UseTransientInMemoryDatabase())
                    .AddSingleton<DbContext>()
                    .BuildServiceProvider();

            var singleton = new object[3];
            DbContext context1;
            DbContext context2;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                context1 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<ILogger<Random>>());
            }

            Assert.NotNull(context1.Model);

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                context2 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton[0], context2.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context2.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context2.GetService<IMemoryCache>());
            }

            Assert.Same(context1, context2);
            Assert.Same(context1.Model, context2.Model);
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void Can_add_non_derived_context_as_singleton_controlling_internal_services(bool addSingletonFirst, bool addEfFirst)
        {
            var serviceCollection = new ServiceCollection();

            if (addEfFirst)
            {
                serviceCollection.AddEntityFrameworkInMemoryDatabase();
            }

            if (addSingletonFirst)
            {
                serviceCollection
                    .AddSingleton<DbContext>()
                    .AddDbContext<DbContext>((p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p));
            }
            else
            {
                serviceCollection
                    .AddDbContext<DbContext>((p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p))
                    .AddSingleton<DbContext>();
            }

            if (!addEfFirst)
            {
                serviceCollection.AddEntityFrameworkInMemoryDatabase();
            }

            var appServiceProivder = serviceCollection.BuildServiceProvider();

            var singleton = new object[3];
            DbContext context1;
            DbContext context2;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                context1 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<ILogger<Random>>());
            }

            Assert.NotNull(context1.Model);

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                context2 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton[0], context2.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context2.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context2.GetService<IMemoryCache>());
            }

            Assert.Same(context1, context2);
            Assert.Same(context1.Model, context2.Model);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public void Can_add_non_derived_context_as_transient(bool addTransientFirst)
        {
            var appServiceProivder = addTransientFirst
                ? new ServiceCollection()
                    .AddTransient<DbContext>()
                    .AddDbContext<DbContext>(b => b.UseTransientInMemoryDatabase())
                    .BuildServiceProvider()
                : new ServiceCollection()
                    .AddDbContext<DbContext>(b => b.UseTransientInMemoryDatabase())
                    .AddTransient<DbContext>()
                    .BuildServiceProvider();

            var singleton = new object[3];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context1 = serviceScope.ServiceProvider.GetService<DbContext>();
                var context2 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotSame(context1, context2);

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<ILogger<Random>>());

                context1.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context1.Model);
                Assert.NotNull(context2.Model);

                context2.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context2.Model);
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());

                context.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context.Model);
            }
        }

        [Theory]
        [InlineData(true, true)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(false, false)]
        public void Can_add_non_derived_context_as_transient_controlling_internal_services(bool addTransientFirst, bool addEfFirst)
        {
            var serviceCollection = new ServiceCollection();

            if (addEfFirst)
            {
                serviceCollection.AddEntityFrameworkInMemoryDatabase();
            }

            if (addTransientFirst)
            {
                serviceCollection
                    .AddTransient<DbContext>()
                    .AddDbContext<DbContext>((p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p));
            }
            else
            {
                serviceCollection
                    .AddDbContext<DbContext>((p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p))
                    .AddTransient<DbContext>();
            }

            if (!addEfFirst)
            {
                serviceCollection.AddEntityFrameworkInMemoryDatabase();
            }

            var appServiceProivder = serviceCollection.BuildServiceProvider();

            var singleton = new object[3];

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context1 = serviceScope.ServiceProvider.GetService<DbContext>();
                var context2 = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.NotSame(context1, context2);

                Assert.NotNull(singleton[0] = context1.GetService<IInMemoryStoreCache>());
                Assert.NotNull(singleton[1] = context1.GetService<ILoggerFactory>());
                Assert.NotNull(singleton[2] = context1.GetService<IMemoryCache>());

                Assert.NotNull(context1.GetService<ILogger<Random>>());

                context1.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context1.Model);
                Assert.NotNull(context2.Model);

                context2.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context2.Model);
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();

                Assert.Same(singleton[0], context.GetService<IInMemoryStoreCache>());
                Assert.Same(singleton[1], context.GetService<ILoggerFactory>());
                Assert.Same(singleton[2], context.GetService<IMemoryCache>());

                context.Dispose();
                Assert.Throws<ObjectDisposedException>(() => context.Model);
            }
        }

        [Fact]
        public void Can_use_logger_before_context_exists_and_after_disposed()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DbContext>((p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p))
                .BuildServiceProvider();

            Assert.NotNull(appServiceProivder.GetService<ILogger<Random>>());

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();
                var _ = context.Model;

                Assert.NotNull(context.GetService<ILogger<Random>>());
            }

            Assert.NotNull(appServiceProivder.GetService<ILogger<Random>>());
        }

        [Fact]
        public void Can_use_logger_before_context_exists_and_after_disposed_when_logger_factory_replaced()
        {
            WrappingLoggerFactory loggerFactory = null;

            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DbContext>((p, b) =>
                    b.UseTransientInMemoryDatabase()
                        .UseLoggerFactory(loggerFactory = new WrappingLoggerFactory(p.GetService<ILoggerFactory>())))
                .BuildServiceProvider();

            Assert.NotNull(appServiceProivder.GetService<ILogger<Random>>());
            Assert.Null(loggerFactory);

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();
                var _ = context.Model;

                Assert.NotNull(context.GetService<ILogger<Random>>());

                Assert.Equal(1, loggerFactory.CreatedLoggers.Count(n => n == "System.Random"));
            }

            Assert.NotNull(appServiceProivder.GetService<ILogger<Random>>());
            Assert.Equal(1, loggerFactory.CreatedLoggers.Count(n => n == "System.Random"));
        }

        [Fact]
        public void Can_use_memory_cache_before_context_exists_and_after_disposed()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DbContext>((p, b) => b.UseTransientInMemoryDatabase().UseInternalServiceProvider(p))
                .BuildServiceProvider();

            var memoryCache = appServiceProivder.GetService<IMemoryCache>();
            Assert.NotNull(memoryCache);

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();
                var _ = context.Model;

                Assert.Same(memoryCache, context.GetService<IMemoryCache>());
            }

            Assert.Same(memoryCache, appServiceProivder.GetService<IMemoryCache>());
        }

        [Fact]
        public void Can_use_memory_cache_before_context_exists_and_after_disposed_when_logger_factory_replaced()
        {
            var replacecMemoryCache = new MemoryCache(new MemoryCacheOptions());
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<DbContext>((p, b) =>
                    b.UseTransientInMemoryDatabase()
                        .UseMemoryCache(replacecMemoryCache))
                .BuildServiceProvider();

            var memoryCache = appServiceProivder.GetService<IMemoryCache>();
            Assert.NotSame(replacecMemoryCache, memoryCache);

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<DbContext>();
                var _ = context.Model;

                Assert.Same(replacecMemoryCache, context.GetService<IMemoryCache>());
            }

            Assert.Same(memoryCache, appServiceProivder.GetService<IMemoryCache>());
        }

        [Fact]
        public void Throws_with_new_when_no_EF_services()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithSets>()
                .UseInternalServiceProvider(new ServiceCollection().BuildServiceProvider())
                .Options;

            Assert.Equal(
                CoreStrings.NoEfServices,
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContextWithSets(options)).Message);
        }

        [Fact]
        public void Throws_with_add_when_no_EF_services()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithSets>(
                    (p, b) => b.UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                Assert.Equal(
                    CoreStrings.NoEfServices,
                    Assert.Throws<InvalidOperationException>(
                        () => serviceScope.ServiceProvider.GetService<ConstructorTestContextWithSets>()).Message);
            }
        }

        [Fact]
        public void Throws_with_new_when_no_EF_services_and_no_sets()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInternalServiceProvider(new ServiceCollection().BuildServiceProvider())
                .Options;

            Assert.Equal(
                CoreStrings.NoEfServices,
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContext1A(options)).Message);
        }

        [Fact]
        public void Throws_with_add_when_no_EF_services_and_no_sets()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b.UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                Assert.Equal(
                    CoreStrings.NoEfServices,
                    Assert.Throws<InvalidOperationException>(
                        () => serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>()).Message);
            }
        }

        [Fact]
        public void Throws_with_new_when_no_provider()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var options = new DbContextOptionsBuilder<ConstructorTestContextWithSets>()
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using (var context = new ConstructorTestContextWithSets(options))
            {
                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_with_add_when_no_provider()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

            var appServiceProivder = serviceCollection
                .AddDbContext<ConstructorTestContextWithSets>(
                    (p, b) => b.UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithSets>();

                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_with_new_when_no_provider_and_no_sets()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();
            var serviceProvider = serviceCollection.BuildServiceProvider();

            var options = new DbContextOptionsBuilder<ConstructorTestContext1A>()
                .UseInternalServiceProvider(serviceProvider)
                .Options;

            using (var context = new ConstructorTestContext1A(options))
            {
                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_with_add_when_no_provider_and_no_sets()
        {
            var serviceCollection = new ServiceCollection();
            new EntityFrameworkServicesBuilder(serviceCollection).TryAddCoreServices();

            var appServiceProivder = serviceCollection
                .AddDbContext<ConstructorTestContext1A>(
                    (p, b) => b.UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContext1A>();

                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_with_new_when_no_EF_services_because_parameterless_constructor()
        {
            using (var context = new ConstructorTestContextNoConfigurationWithSets())
            {
                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_with_add_when_no_EF_services_because_parameterless_constructor()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContextNoConfigurationWithSets>()
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextNoConfigurationWithSets>();

                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_with_new_when_no_EF_services_and_no_sets_because_parameterless_constructor()
        {
            using (var context = new ConstructorTestContextNoConfiguration())
            {
                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_with_add_when_no_EF_services_and_no_sets_because_parameterless_constructor()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContextNoConfiguration>()
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextNoConfiguration>();

                Assert.Equal(
                    CoreStrings.NoProviderConfigured,
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Can_replace_services_in_OnConfiguring()
        {
            object replacedSingleton;
            object replacedScoped;
            object replacedProviderService;

            using (var context = new ReplaceServiceContext1())
            {
                Assert.NotNull(replacedSingleton = context.GetService<IModelCustomizer>());
                Assert.IsType<CustomModelCustomizer>(replacedSingleton);

                Assert.NotNull(replacedScoped = context.GetService<IValueGeneratorSelector>());
                Assert.IsType<CustomInMemoryValueGeneratorSelector>(replacedScoped);

                Assert.NotNull(replacedProviderService = context.GetService<IInMemoryTableFactory>());
                Assert.IsType<CustomInMemoryTableFactory>(replacedProviderService);
            }

            using (var context = new ReplaceServiceContext1())
            {
                Assert.Same(replacedSingleton, context.GetService<IModelCustomizer>());
                Assert.NotSame(replacedScoped, context.GetService<IValueGeneratorSelector>());
                Assert.Same(replacedProviderService, context.GetService<IInMemoryTableFactory>());
            }
        }

        private class ReplaceServiceContext1 : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .ReplaceService<IModelCustomizer, CustomModelCustomizer>()
                    .ReplaceService<IValueGeneratorSelector, CustomInMemoryValueGeneratorSelector>()
                    .ReplaceService<IInMemoryTableFactory, CustomInMemoryTableFactory>()
                    .UseTransientInMemoryDatabase();
        }

        private class CustomModelCustomizer : ModelCustomizer
        {
            public CustomModelCustomizer(ModelCustomizerDependencies dependencies)
                : base(dependencies)
            {
            }
        }

        private class CustomInMemoryValueGeneratorSelector : InMemoryValueGeneratorSelector
        {
            public CustomInMemoryValueGeneratorSelector(ValueGeneratorSelectorDependencies dependencies)
                : base(dependencies)
            {
            }
        }

        private class CustomInMemoryTableFactory : InMemoryTableFactory
        {
        }

        [Fact]
        public void Can_replace_services_in_passed_options()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseTransientInMemoryDatabase()
                .ReplaceService<IModelCustomizer, CustomModelCustomizer>()
                .ReplaceService<IValueGeneratorSelector, CustomInMemoryValueGeneratorSelector>()
                .ReplaceService<IInMemoryTableFactory, CustomInMemoryTableFactory>()
                .Options;

            object replacedSingleton;
            object replacedScoped;
            object replacedProviderService;

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.NotNull(replacedSingleton = context.GetService<IModelCustomizer>());
                Assert.IsType<CustomModelCustomizer>(replacedSingleton);

                Assert.NotNull(replacedScoped = context.GetService<IValueGeneratorSelector>());
                Assert.IsType<CustomInMemoryValueGeneratorSelector>(replacedScoped);

                Assert.NotNull(replacedProviderService = context.GetService<IInMemoryTableFactory>());
                Assert.IsType<CustomInMemoryTableFactory>(replacedProviderService);
            }

            using (var context = new ConstructorTestContextWithOC3A(options))
            {
                Assert.Same(replacedSingleton, context.GetService<IModelCustomizer>());
                Assert.NotSame(replacedScoped, context.GetService<IValueGeneratorSelector>());
                Assert.Same(replacedProviderService, context.GetService<IInMemoryTableFactory>());
            }
        }

        [Fact]
        public void Can_replace_services_using_AddDbContext()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    b => b.ReplaceService<IModelCustomizer, CustomModelCustomizer>()
                        .ReplaceService<IValueGeneratorSelector, CustomInMemoryValueGeneratorSelector>()
                        .ReplaceService<IInMemoryTableFactory, CustomInMemoryTableFactory>()
                        .UseTransientInMemoryDatabase())
                .BuildServiceProvider();

            object replacedSingleton;
            object replacedScoped;
            object replacedProviderService;

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.NotNull(replacedSingleton = context.GetService<IModelCustomizer>());
                Assert.IsType<CustomModelCustomizer>(replacedSingleton);

                Assert.NotNull(replacedScoped = context.GetService<IValueGeneratorSelector>());
                Assert.IsType<CustomInMemoryValueGeneratorSelector>(replacedScoped);

                Assert.NotNull(replacedProviderService = context.GetService<IInMemoryTableFactory>());
                Assert.IsType<CustomInMemoryTableFactory>(replacedProviderService);
            }

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Same(replacedSingleton, context.GetService<IModelCustomizer>());
                Assert.NotSame(replacedScoped, context.GetService<IValueGeneratorSelector>());
                Assert.Same(replacedProviderService, context.GetService<IInMemoryTableFactory>());
            }
        }

        [Fact]
        public void Throws_replacing_services_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            using (var context = new ReplaceServiceContext2())
            {
                Assert.Equal(
                    CoreStrings.InvalidReplaceService(
                        nameof(DbContextOptionsBuilder.ReplaceService), nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class ReplaceServiceContext2 : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .ReplaceService<IModelCustomizer, CustomModelCustomizer>()
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider())
                    .UseTransientInMemoryDatabase();
        }

        [Fact]
        public void Throws_replacing_services_in_options_when_UseInternalServiceProvider()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseTransientInMemoryDatabase()
                .UseInternalServiceProvider(new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider())
                .ReplaceService<IInMemoryTableFactory, CustomInMemoryTableFactory>()
                .Options;

            Assert.Equal(
                CoreStrings.InvalidReplaceService(
                    nameof(DbContextOptionsBuilder.ReplaceService), nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContextWithOC3A(options)).Message);
        }

        [Fact]
        public void Throws_replacing_services_with_AddDbContext_when_UseInternalServiceProvider()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.ReplaceService<IInMemoryTableFactory, CustomInMemoryTableFactory>()
                        .UseTransientInMemoryDatabase()
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                Assert.Equal(
                    CoreStrings.InvalidReplaceService(
                        nameof(DbContextOptionsBuilder.ReplaceService), nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(
                        () => serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>()).Message);
            }
        }

        [Fact]
        public void Throws_setting_LoggerFactory_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            using (var context = new SetLoggerFactoryContext())
            {
                Assert.Equal(
                    CoreStrings.InvalidUseService(
                        nameof(DbContextOptionsBuilder.UseLoggerFactory),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                        nameof(ILoggerFactory)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class SetLoggerFactoryContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseLoggerFactory(new FakeLoggerFactory())
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider())
                    .UseTransientInMemoryDatabase();
        }

        [Fact]
        public void Throws_setting_LoggerFactory_in_options_when_UseInternalServiceProvider()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseTransientInMemoryDatabase()
                .UseInternalServiceProvider(new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider())
                .UseLoggerFactory(new FakeLoggerFactory())
                .Options;

            Assert.Equal(
                    CoreStrings.InvalidUseService(
                        nameof(DbContextOptionsBuilder.UseLoggerFactory),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                        nameof(ILoggerFactory)),
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContextWithOC3A(options)).Message);
        }

        [Fact]
        public void Throws_setting_LoggerFactory_with_AddDbContext_when_UseInternalServiceProvider()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.UseLoggerFactory(new FakeLoggerFactory())
                        .UseTransientInMemoryDatabase()
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                Assert.Equal(
                    CoreStrings.InvalidUseService(
                        nameof(DbContextOptionsBuilder.UseLoggerFactory),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                        nameof(ILoggerFactory)),
                    Assert.Throws<InvalidOperationException>(
                        () => serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>()).Message);
            }
        }

        [Fact]
        public void Throws_setting_MemoryCache_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            using (var context = new SetMemoryCacheContext())
            {
                Assert.Equal(
                    CoreStrings.InvalidUseService(
                        nameof(DbContextOptionsBuilder.UseMemoryCache),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                        nameof(IMemoryCache)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class SetMemoryCacheContext : DbContext
        {
            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseMemoryCache(new FakeMemoryCache())
                    .UseInternalServiceProvider(new ServiceCollection()
                        .AddEntityFrameworkInMemoryDatabase()
                        .BuildServiceProvider())
                    .UseTransientInMemoryDatabase();
        }

        [Fact]
        public void Throws_setting_MemoryCache_in_options_when_UseInternalServiceProvider()
        {
            var options = new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                .UseTransientInMemoryDatabase()
                .UseInternalServiceProvider(new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider())
                .UseMemoryCache(new FakeMemoryCache())
                .Options;

            Assert.Equal(
                    CoreStrings.InvalidUseService(
                        nameof(DbContextOptionsBuilder.UseMemoryCache),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                        nameof(IMemoryCache)),
                Assert.Throws<InvalidOperationException>(() => new ConstructorTestContextWithOC3A(options)).Message);
        }

        [Fact]
        public void Throws_setting_MemoryCache_with_AddDbContext_when_UseInternalServiceProvider()
        {
            var appServiceProivder = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.UseMemoryCache(new FakeMemoryCache())
                        .UseTransientInMemoryDatabase()
                        .UseInternalServiceProvider(p))
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                Assert.Equal(
                    CoreStrings.InvalidUseService(
                        nameof(DbContextOptionsBuilder.UseMemoryCache),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider),
                        nameof(IMemoryCache)),
                    Assert.Throws<InvalidOperationException>(
                        () => serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>()).Message);
            }
        }

        private class FakeMemoryCache : IMemoryCache
        {
            public void Dispose()
            {
            }

            public bool TryGetValue(object key, out object value)
            {
                throw new NotImplementedException();
            }

            public ICacheEntry CreateEntry(object key)
            {
                throw new NotImplementedException();
            }

            public void Remove(object key)
            {
            }
        }

        [Fact]
        public void Throws_changing_sensitive_data_logging_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            using (var context = new ChangeSdlCacheContext(false))
            {
                var _ = context.Model;
            }

            using (var context = new ChangeSdlCacheContext(true))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.EnableSensitiveDataLogging),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class ChangeSdlCacheContext : DbContext
        {
            private static readonly IServiceProvider _serviceProvider
                = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

            private readonly bool _on;

            public ChangeSdlCacheContext(bool on)
            {
                _on = on;
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .EnableSensitiveDataLogging(_on)
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseTransientInMemoryDatabase();
        }

        [Fact]
        public void Throws_changing_sensitive_data_logging_in_options_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new ConstructorTestContextWithOC3A(
                new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                    .UseTransientInMemoryDatabase()
                    .UseInternalServiceProvider(serviceProvider)
                    .EnableSensitiveDataLogging()
                    .Options))
            {
                var _ = context.Model;
            }

            using (var context = new ConstructorTestContextWithOC3A(
                new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                    .UseTransientInMemoryDatabase()
                    .UseInternalServiceProvider(serviceProvider)
                    .EnableSensitiveDataLogging(false)
                    .Options))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.EnableSensitiveDataLogging),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_changing_sensitive_data_logging_with_AddDbContext_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var serviceScope = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.EnableSensitiveDataLogging()
                        .UseTransientInMemoryDatabase()
                        .UseInternalServiceProvider(serviceProvider))
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                var _ = context.Model;
            }

            using (var serviceScope = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.EnableSensitiveDataLogging(false)
                        .UseTransientInMemoryDatabase()
                        .UseInternalServiceProvider(serviceProvider))
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.EnableSensitiveDataLogging),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_changing_warnings_default_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new ChangeWarningsCacheContext(serviceProvider, b => b.Default(WarningBehavior.Ignore)))
            {
                var _ = context.Model;
            }

            using (var context = new ChangeWarningsCacheContext(serviceProvider, b => b.Default(WarningBehavior.Log)))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.ConfigureWarnings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_changing_warnings_in_OnConfiguring_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new ChangeWarningsCacheContext(serviceProvider, b => b.Throw(CoreEventId.QueryPlan)))
            {
                var _ = context.Model;
            }

            using (var context = new ChangeWarningsCacheContext(serviceProvider, b => b.Log(CoreEventId.QueryPlan)))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.ConfigureWarnings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class ChangeWarningsCacheContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;
            private readonly Action<WarningsConfigurationBuilder> _configAction;

            public ChangeWarningsCacheContext(
                IServiceProvider serviceProvider,
                Action<WarningsConfigurationBuilder> configAction)
            {
                _serviceProvider = serviceProvider;
                _configAction = configAction;
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .ConfigureWarnings(_configAction)
                    .UseInternalServiceProvider(_serviceProvider)
                    .UseTransientInMemoryDatabase();
        }

        [Fact]
        public void Throws_changing_warnings_config_in_options_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new ConstructorTestContextWithOC3A(
                new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                    .UseTransientInMemoryDatabase()
                    .UseInternalServiceProvider(serviceProvider)
                    .ConfigureWarnings(b => b.Default(WarningBehavior.Throw))
                    .Options))
            {
                var _ = context.Model;
            }

            using (var context = new ConstructorTestContextWithOC3A(
                new DbContextOptionsBuilder<ConstructorTestContextWithOC3A>()
                    .UseTransientInMemoryDatabase()
                    .UseInternalServiceProvider(serviceProvider)
                    .ConfigureWarnings(b => b.Default(WarningBehavior.Log))
                    .Options))
            {
                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.ConfigureWarnings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        [Fact]
        public void Throws_changing_warnings_config_with_AddDbContext_when_UseInternalServiceProvider()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var serviceScope = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.ConfigureWarnings(wb => wb.Default(WarningBehavior.Throw))
                        .UseTransientInMemoryDatabase()
                        .UseInternalServiceProvider(serviceProvider))
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                var _ = context.Model;
            }

            using (var serviceScope = new ServiceCollection()
                .AddDbContext<ConstructorTestContextWithOC3A>(
                    (p, b) => b.ConfigureWarnings(wb => wb.Default(WarningBehavior.Ignore))
                        .UseTransientInMemoryDatabase()
                        .UseInternalServiceProvider(serviceProvider))
                .BuildServiceProvider()
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                var context = serviceScope.ServiceProvider.GetService<ConstructorTestContextWithOC3A>();

                Assert.Equal(
                    CoreStrings.SingletonOptionChanged(
                        nameof(DbContextOptionsBuilder.ConfigureWarnings),
                        nameof(DbContextOptionsBuilder.UseInternalServiceProvider)),
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class WrappingLoggerFactory : ILoggerFactory
        {
            private readonly ILoggerFactory _loggerFactory;

            public IList<string> CreatedLoggers { get; } = new List<string>();

            public WrappingLoggerFactory(ILoggerFactory loggerFactory)
            {
                _loggerFactory = loggerFactory;
            }

            public void Dispose() => _loggerFactory.Dispose();

            public ILogger CreateLogger(string categoryName)
            {
                CreatedLoggers.Add(categoryName);

                return _loggerFactory.CreateLogger(categoryName);
            }

            public void AddProvider(ILoggerProvider provider) => _loggerFactory.AddProvider(provider);
        }

        private class ConstructorTestContext1A : DbContext
        {
            public ConstructorTestContext1A(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class ConstructorTestContextWithSets : DbContext
        {
            public ConstructorTestContextWithSets(DbContextOptions options)
                : base(options)
            {
            }

            public DbSet<Product> Products { get; set; }
        }

        private class ConstructorTestContextNoConfiguration : DbContext
        {
        }

        private class ConstructorTestContextNoConfigurationWithSets : DbContext
        {
            public DbSet<Product> Products { get; set; }
        }

        private class ConstructorTestContextWithOCBase : DbContext
        {
            private readonly IServiceProvider _internalServicesProvider;
            private readonly ILoggerFactory _loggerFactory;
            private readonly IMemoryCache _memoryCache;
            private readonly bool _isConfigured;

            protected ConstructorTestContextWithOCBase(
                ILoggerFactory loggerFactory = null,
                IMemoryCache memoryCache = null)
            {
                _loggerFactory = loggerFactory;
                _memoryCache = memoryCache;
            }

            protected ConstructorTestContextWithOCBase(
                IServiceProvider internalServicesProvider,
                ILoggerFactory loggerFactory = null,
                IMemoryCache memoryCache = null)
            {
                _internalServicesProvider = internalServicesProvider;
                _loggerFactory = loggerFactory;
                _memoryCache = memoryCache;
            }

            protected ConstructorTestContextWithOCBase(
                DbContextOptions options,
                ILoggerFactory loggerFactory = null,
                IMemoryCache memoryCache = null)
                : base(options)
            {
                _loggerFactory = loggerFactory;
                _memoryCache = memoryCache;
                _isConfigured = true;
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                Assert.Equal(_isConfigured, optionsBuilder.IsConfigured);

                if (!optionsBuilder.IsConfigured)
                {
                    optionsBuilder.UseTransientInMemoryDatabase();
                }

                if (_internalServicesProvider != null)
                {
                    optionsBuilder.UseInternalServiceProvider(_internalServicesProvider);
                }

                if (_memoryCache != null)
                {
                    optionsBuilder.UseMemoryCache(_memoryCache);
                }

                if (_loggerFactory != null)
                {
                    optionsBuilder.UseLoggerFactory(_loggerFactory);
                }
            }
        }

        private class ConstructorTestContextWithOC1A : ConstructorTestContextWithOCBase
        {
        }

        private class ConstructorTestContextWithOC2A : ConstructorTestContextWithOCBase
        {
            public ConstructorTestContextWithOC2A(
                IServiceProvider internalServicesProvider)
                : base(internalServicesProvider)
            {
            }
        }

        private class ConstructorTestContextWithOC3A : ConstructorTestContextWithOCBase
        {
            public ConstructorTestContextWithOC3A(
                DbContextOptions options)
                : base(options)
            {
            }
        }

        private class ConstructorTestContextWithOC1B : ConstructorTestContextWithOCBase
        {
            public ConstructorTestContextWithOC1B(
                ILoggerFactory loggerFactory,
                IMemoryCache memoryCache)
                : base(loggerFactory, memoryCache)
            {
            }
        }

        private class ConstructorTestContextWithOC2B : ConstructorTestContextWithOCBase
        {
            public ConstructorTestContextWithOC2B(
                IServiceProvider internalServicesProvider,
                ILoggerFactory loggerFactory,
                IMemoryCache memoryCache)
                : base(internalServicesProvider, loggerFactory, memoryCache)
            {
            }
        }

        [Fact]
        public void Throws_when_wrong_DbContextOptions_used()
        {
            var options = new DbContextOptionsBuilder<NonGenericOptions1>()
                .UseInternalServiceProvider(new ServiceCollection().BuildServiceProvider())
                .Options;

            Assert.Equal(
                CoreStrings.NonGenericOptions(nameof(NonGenericOptions2)),
                Assert.Throws<InvalidOperationException>(() => new NonGenericOptions2(options)).Message);
        }

        [Fact]
        public void Throws_when_adding_two_contexts_using_non_generic_options()
        {
            var appServiceProivder = new ServiceCollection()
                .AddDbContext<NonGenericOptions2>(b => b.UseTransientInMemoryDatabase())
                .AddDbContext<NonGenericOptions1>(b => b.UseTransientInMemoryDatabase())
                .BuildServiceProvider();

            using (var serviceScope = appServiceProivder
                .GetRequiredService<IServiceScopeFactory>()
                .CreateScope())
            {
                Assert.Equal(
                    CoreStrings.NonGenericOptions(nameof(NonGenericOptions2)),
                    Assert.Throws<InvalidOperationException>(() =>
                        {
                            serviceScope.ServiceProvider.GetService<NonGenericOptions1>();
                            serviceScope.ServiceProvider.GetService<NonGenericOptions2>();
                        }).Message);
            }
        }

        private class NonGenericOptions1 : DbContext
        {
            public NonGenericOptions1(DbContextOptions options)
                : base(options)
            {
            }
        }

        private class NonGenericOptions2 : DbContext
        {
            public NonGenericOptions2(DbContextOptions options)
                : base(options)
            {
            }
        }

        [Fact]
        public void AddDbContext_adds_options_for_all_types()
        {
            var services = new ServiceCollection()
                .AddSingleton<DbContextOptions>(_ => new DbContextOptions<NonGenericOptions1>())
                .AddDbContext<NonGenericOptions1>()
                .AddDbContext<NonGenericOptions2>()
                .BuildServiceProvider();

            Assert.Equal(3, services.GetServices<DbContextOptions>().Count());
            Assert.Equal(2, services.GetServices<DbContextOptions>()
                .Select(o => o.ContextType)
                .Distinct()
                .Count());
        }

        [Fact]
        public void Last_DbContextOptions_in_serviceCollection_selected()
        {
            var services = new ServiceCollection()
                .AddDbContext<NonGenericOptions1>()
                .AddDbContext<NonGenericOptions2>()
                .BuildServiceProvider();

            Assert.Equal(typeof(NonGenericOptions2), services.GetService<DbContextOptions>().ContextType);
        }

        [Fact]
        public void Model_cannot_be_used_in_OnModelCreating()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new UseModelInOnModelCreatingContext(serviceProvider))
            {
                Assert.Equal(
                    CoreStrings.RecursiveOnModelCreating,
                    Assert.Throws<InvalidOperationException>(() => context.Model).Message);
            }
        }

        private class UseModelInOnModelCreatingContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public UseModelInOnModelCreatingContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Product> Products { get; set; }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                var _ = Model;
            }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseTransientInMemoryDatabase()
                    .UseInternalServiceProvider(_serviceProvider);
        }

        [Fact]
        public void Context_cannot_be_used_in_OnModelCreating()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new UseInOnModelCreatingContext(serviceProvider))
            {
                Assert.Equal(
                    CoreStrings.RecursiveOnModelCreating,
                    Assert.Throws<InvalidOperationException>(() => context.Products.ToList()).Message);
            }
        }

        private class UseInOnModelCreatingContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public UseInOnModelCreatingContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Product> Products { get; set; }

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
                => Products.ToList();

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseTransientInMemoryDatabase()
                    .UseInternalServiceProvider(_serviceProvider);
        }

        [Fact]
        public void Context_cannot_be_used_in_OnConfiguring()
        {
            var serviceProvider = new ServiceCollection()
                .AddEntityFrameworkInMemoryDatabase()
                .BuildServiceProvider();

            using (var context = new UseInOnConfiguringContext(serviceProvider))
            {
                Assert.Equal(
                    CoreStrings.RecursiveOnConfiguring,
                    Assert.Throws<InvalidOperationException>(() => context.Products.ToList()).Message);
            }
        }

        private class UseInOnConfiguringContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public UseInOnConfiguringContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Product> Products { get; set; }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInternalServiceProvider(_serviceProvider);

                Products.ToList();

                base.OnConfiguring(optionsBuilder);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task SaveChanges_calls_DetectChanges_by_default(bool async)
        {
            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider();

            using (var context = new ButTheHedgehogContext(provider))
            {
                Assert.True(context.ChangeTracker.AutoDetectChangesEnabled);

                var product = context.Add(new Product { Id = 1, Name = "Little Hedgehogs" }).Entity;

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                product.Name = "Cracked Cookies";

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            }

            using (var context = new ButTheHedgehogContext(provider))
            {
                Assert.Equal("Cracked Cookies", context.Products.Single().Name);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task Auto_DetectChanges_for_SaveChanges_can_be_switched_off(bool async)
        {
            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider();

            using (var context = new ButTheHedgehogContext(provider))
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;
                Assert.False(context.ChangeTracker.AutoDetectChangesEnabled);

                var product = context.Add(new Product { Id = 1, Name = "Little Hedgehogs" }).Entity;

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }

                product.Name = "Cracked Cookies";

                if (async)
                {
                    await context.SaveChangesAsync();
                }
                else
                {
                    context.SaveChanges();
                }
            }

            using (var context = new ButTheHedgehogContext(provider))
            {
                Assert.Equal("Little Hedgehogs", context.Products.Single().Name);
            }
        }

        private class ButTheHedgehogContext : DbContext
        {
            private readonly IServiceProvider _serviceProvider;

            public ButTheHedgehogContext(IServiceProvider serviceProvider)
            {
                _serviceProvider = serviceProvider;
            }

            public DbSet<Product> Products { get; set; }

            protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
                => optionsBuilder
                    .UseInMemoryDatabase(nameof(ButTheHedgehogContext))
                    .UseInternalServiceProvider(_serviceProvider);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Entry_calls_DetectChanges_by_default(bool useGenericOverload)
        {
            using (var context = new ButTheHedgehogContext(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                var entry = context.Attach(new Product { Id = 1, Name = "Little Hedgehogs" });

                entry.Entity.Name = "Cracked Cookies";

                Assert.Equal(EntityState.Unchanged, entry.State);

                if (useGenericOverload)
                {
                    context.Entry(entry.Entity);
                }
                else
                {
                    context.Entry((object)entry.Entity);
                }

                Assert.Equal(EntityState.Modified, entry.State);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void Auto_DetectChanges_for_Entry_can_be_switched_off(bool useGenericOverload)
        {
            using (var context = new ButTheHedgehogContext(InMemoryTestHelpers.Instance.CreateServiceProvider()))
            {
                context.ChangeTracker.AutoDetectChangesEnabled = false;

                var entry = context.Attach(new Product { Id = 1, Name = "Little Hedgehogs" });

                entry.Entity.Name = "Cracked Cookies";

                Assert.Equal(EntityState.Unchanged, entry.State);

                if (useGenericOverload)
                {
                    context.Entry(entry.Entity);
                }
                else
                {
                    context.Entry((object)entry.Entity);
                }

                Assert.Equal(EntityState.Unchanged, entry.State);
            }
        }

        [Fact]
        public async Task Add_Attach_Remove_Update_do_not_call_DetectChanges()
        {
            var provider = InMemoryTestHelpers.Instance.CreateServiceProvider(new ServiceCollection().AddScoped<IChangeDetector, ChangeDetectorProxy>());
            using (var context = new ButTheHedgehogContext(provider))
            {
                var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

                var id = 1;

                changeDetector.DetectChangesCalled = false;

                context.Add(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.Add((object)new Product { Id = id++, Name = "Little Hedgehogs" });
                context.AddRange(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.AddRange(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.AddRange(new List<Product> { new Product { Id = id++, Name = "Little Hedgehogs" } });
                context.AddRange(new List<object> { new Product { Id = id++, Name = "Little Hedgehogs" } });
                await context.AddAsync(new Product { Id = id++, Name = "Little Hedgehogs" });
                await context.AddAsync((object)new Product { Id = id++, Name = "Little Hedgehogs" });
                await context.AddRangeAsync(new Product { Id = id++, Name = "Little Hedgehogs" });
                await context.AddRangeAsync(new Product { Id = id++, Name = "Little Hedgehogs" });
                await context.AddRangeAsync(new List<Product> { new Product { Id = id++, Name = "Little Hedgehogs" } });
                await context.AddRangeAsync(new List<object> { new Product { Id = id++, Name = "Little Hedgehogs" } });
                context.Attach(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.Attach((object)new Product { Id = id++, Name = "Little Hedgehogs" });
                context.AttachRange(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.AttachRange(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.AttachRange(new List<Product> { new Product { Id = id++, Name = "Little Hedgehogs" } });
                context.AttachRange(new List<object> { new Product { Id = id++, Name = "Little Hedgehogs" } });
                context.Update(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.Update((object)new Product { Id = id++, Name = "Little Hedgehogs" });
                context.UpdateRange(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.UpdateRange(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.UpdateRange(new List<Product> { new Product { Id = id++, Name = "Little Hedgehogs" } });
                context.UpdateRange(new List<object> { new Product { Id = id++, Name = "Little Hedgehogs" } });
                context.Remove(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.Remove((object)new Product { Id = id++, Name = "Little Hedgehogs" });
                context.RemoveRange(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.RemoveRange(new Product { Id = id++, Name = "Little Hedgehogs" });
                context.RemoveRange(new List<Product> { new Product { Id = id++, Name = "Little Hedgehogs" } });
                context.RemoveRange(new List<object> { new Product { Id = id++, Name = "Little Hedgehogs" } });

                Assert.False(changeDetector.DetectChangesCalled);

                context.ChangeTracker.DetectChanges();

                Assert.True(changeDetector.DetectChangesCalled);
            }
        }

        private class ChangeDetectorProxy : ChangeDetector
        {
            public bool DetectChangesCalled { get; set; }

            public override void DetectChanges(InternalEntityEntry entry)
            {
                DetectChangesCalled = true;

                base.DetectChanges(entry);
            }

            public override void DetectChanges(IStateManager stateManager)
            {
                DetectChangesCalled = true;

                base.DetectChanges(stateManager);
            }
        }

        [Fact]
        public async void It_throws_object_disposed_exception()
        {
            var context = new DbContext(new DbContextOptions<DbContext>());
            context.Dispose();

            // methods (tests all paths)
            Assert.Throws<ObjectDisposedException>(() => context.Add(new object()));
            Assert.Throws<ObjectDisposedException>(() => context.Find(typeof(Random), 77));
            Assert.Throws<ObjectDisposedException>(() => context.Attach(new object()));
            Assert.Throws<ObjectDisposedException>(() => context.Update(new object()));
            Assert.Throws<ObjectDisposedException>(() => context.Remove(new object()));
            Assert.Throws<ObjectDisposedException>(() => context.SaveChanges());
            await Assert.ThrowsAsync<ObjectDisposedException>(() => context.SaveChangesAsync());
            await Assert.ThrowsAsync<ObjectDisposedException>(() => context.AddAsync(new object()));
            await Assert.ThrowsAsync<ObjectDisposedException>(() => context.FindAsync(typeof(Random), 77));

            var methodCount = typeof(DbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count();
            var expectedMethodCount = 37;
            Assert.True(
                methodCount == expectedMethodCount,
                userMessage: $"Expected {expectedMethodCount} methods on DbContext but found {methodCount}. " +
                             "Update test to ensure all methods throw ObjectDisposedException after dispose.");

            // getters
            Assert.Throws<ObjectDisposedException>(() => context.ChangeTracker);
            Assert.Throws<ObjectDisposedException>(() => context.Model);

            var expectedProperties = new List<string> { "ChangeTracker", "Database", "Model" };

            Assert.True(expectedProperties.SequenceEqual(
                    typeof(DbContext)
                        .GetProperties()
                        .Select(p => p.Name)
                        .OrderBy(s => s)
                        .ToList()),
                userMessage: "Unexpected properties on DbContext. " +
                             "Update test to ensure all getters throw ObjectDisposedException after dispose.");

            Assert.Throws<ObjectDisposedException>(() => ((IInfrastructure<IServiceProvider>)context).Instance);
        }

        [Fact]
        public void It_throws_with_derived_name()
        {
            var context = new EarlyLearningCenter();

            context.Dispose();

            var ex = Assert.Throws<ObjectDisposedException>(() => context.Model);
        }

        [Fact]
        public void It_disposes_scope()
        {
            var fakeServiceProvider = new FakeServiceProvider();
            var context = new DbContext(
                new DbContextOptionsBuilder().UseInternalServiceProvider(fakeServiceProvider).UseTransientInMemoryDatabase().Options);

            var scopeService = Assert.IsType<FakeServiceProvider.FakeServiceScope>(context.GetService<IServiceScopeFactory>().CreateScope());

            Assert.False(scopeService.Disposed);

            context.Dispose();

            Assert.True(scopeService.Disposed);

            Assert.Throws<ObjectDisposedException>(() => ((IInfrastructure<IServiceProvider>)context).Instance);
        }

        public class FakeServiceProvider : IServiceProvider, IDisposable
        {
            private readonly IServiceProvider _realProvider;

            public FakeServiceProvider()
            {
                _realProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider();
            }

            public bool Disposed { get; set; }

            public void Dispose() => Disposed = true;

            public object GetService(Type serviceType)
            {
                if (serviceType == typeof(IServiceProvider))
                {
                    return this;
                }

                if (serviceType == typeof(IServiceScopeFactory))
                {
                    return new FakeServiceScopeFactory();
                }

                return _realProvider.GetService(serviceType);
            }

            public class FakeServiceScopeFactory : IServiceScopeFactory
            {
                public static FakeServiceScope Scope { get; } = new FakeServiceScope();

                public IServiceScope CreateScope() => Scope;
            }

            public class FakeServiceScope : IServiceScope
            {
                public bool Disposed { get; set; }

                public IServiceProvider ServiceProvider { get; set; } = new FakeServiceProvider();

                public void Dispose() => Disposed = true;
            }
        }

        [Fact]
        public void Adding_entities_with_shadow_keys_should_not_throw()
        {
            using (var context = new NullShadowKeyContext())
            {
                var assembly = new TestAssembly { Name = "Assembly1" };
                var testClass = new TestClass { Assembly = assembly, Name = "Class1" };
                var test = context.Tests.Add(new Test { Class = testClass, Name = "Test1" }).Entity;

                context.SaveChanges();

                ValidateGraph(context, assembly, testClass, test);
            }

            using (var context = new NullShadowKeyContext())
            {
                var test = context.Tests.Single();
                var assembly = context.Assemblies.Single();
                var testClass = context.Classes.Single();

                ValidateGraph(context, assembly, testClass, test);
            }
        }

        private static void ValidateGraph(NullShadowKeyContext context, TestAssembly assembly, TestClass testClass, Test test)
        {
            Assert.Equal(EntityState.Unchanged, context.Entry(assembly).State);
            Assert.Equal("Assembly1", assembly.Name);
            Assert.Same(testClass, test.Class);

            Assert.Equal(EntityState.Unchanged, context.Entry(testClass).State);
            Assert.Equal("Class1", testClass.Name);
            Assert.Equal("Assembly1", context.Entry(testClass).Property("AssemblyName").CurrentValue);
            Assert.Same(test, testClass.Tests.Single());
            Assert.Same(assembly, testClass.Assembly);

            Assert.Equal(EntityState.Unchanged, context.Entry(test).State);
            Assert.Equal("Test1", test.Name);
            Assert.Equal("Assembly1", context.Entry(test).Property("AssemblyName").CurrentValue);
            Assert.Equal("Class1", context.Entry(test).Property("ClassName").CurrentValue);
            Assert.Same(testClass, assembly.Classes.Single());
        }

        private class TestAssembly
        {
            [Key]
            public string Name { get; set; }

            public ICollection<TestClass> Classes { get; } = new List<TestClass>();
        }

        private class TestClass
        {
            public TestAssembly Assembly { get; set; }
            public string Name { get; set; }
            public ICollection<Test> Tests { get; } = new List<Test>();
        }

        private class Test
        {
            public TestClass Class { get; set; }
            public string Name { get; set; }
        }

        private class NullShadowKeyContext : DbContext
        {
            public DbSet<TestAssembly> Assemblies { get; set; }
            public DbSet<TestClass> Classes { get; set; }
            public DbSet<Test> Tests { get; set; }

            protected internal override void OnConfiguring(DbContextOptionsBuilder options)
                => options.UseInMemoryDatabase(nameof(NullShadowKeyContext));

            protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            {
                modelBuilder.Entity<TestClass>(
                    x =>
                        {
                            x.Property<string>("AssemblyName");
                            x.HasKey("AssemblyName", nameof(TestClass.Name));
                            x.HasOne(c => c.Assembly).WithMany(a => a.Classes)
                                .HasForeignKey("AssemblyName");
                        });

                modelBuilder.Entity<Test>(
                    x =>
                        {
                            x.Property<string>("AssemblyName");
                            x.HasKey("AssemblyName", "ClassName", nameof(Test.Name));
                            x.HasOne(t => t.Class).WithMany(c => c.Tests)
                                .HasForeignKey("AssemblyName", "ClassName");
                        });
            }
        }
    }
}

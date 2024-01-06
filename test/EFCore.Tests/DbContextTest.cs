// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore;

public partial class DbContextTest
{
    [ConditionalFact]
    public void Set_throws_for_type_not_in_model()
    {
        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseInternalServiceProvider(InMemoryTestHelpers.Instance.CreateServiceProvider());

        using var context = new DbContext(optionsBuilder.Options);
        var ex = Assert.Throws<InvalidOperationException>(() => context.Set<Category>().Local);

        Assert.Equal(CoreStrings.InvalidSetType(nameof(Category)), ex.Message);
    }

    [ConditionalFact]
    public void Set_throws_for_type_not_in_model_same_type_with_different_namespace()
    {
        using var context = new EarlyLearningCenter();
        var ex = Assert.Throws<InvalidOperationException>(() => context.Set<DifferentNamespace.Category>().Local);

        Assert.Equal(
            CoreStrings.InvalidSetSameTypeWithDifferentNamespace(
                typeof(DifferentNamespace.Category).DisplayName(), typeof(Category).DisplayName()), ex.Message);
    }

    [ConditionalFact]
    public void Local_calls_DetectChanges()
    {
        var provider =
            InMemoryTestHelpers.Instance.CreateServiceProvider(
                new ServiceCollection().AddScoped<IChangeDetector, ChangeDetectorProxy>());

        using var context = new ButTheHedgehogContext(provider);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        changeDetector.DetectChangesCalled = false;

        var entry = context.Attach(
            new Product
            {
                Id = 1,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });

        entry.Entity.Name = "Big Hedgehogs";

        Assert.False(changeDetector.DetectChangesCalled);

        var _ = context.Set<Product>().Local;

        Assert.True(changeDetector.DetectChangesCalled);
        Assert.Equal(EntityState.Modified, entry.State);
    }

    [ConditionalFact]
    public void Local_does_not_call_DetectChanges_when_disabled()
    {
        var provider =
            InMemoryTestHelpers.Instance.CreateServiceProvider(
                new ServiceCollection().AddScoped<IChangeDetector, ChangeDetectorProxy>());

        using var context = new ButTheHedgehogContext(provider);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        context.ChangeTracker.AutoDetectChangesEnabled = false;

        changeDetector.DetectChangesCalled = false;

        var entry = context.Attach(
            new Product
            {
                Id = 1,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });

        entry.Entity.Name = "Big Hedgehogs";

        Assert.False(changeDetector.DetectChangesCalled);

        var _ = context.Set<Product>().Local;

        Assert.False(changeDetector.DetectChangesCalled);
        Assert.Equal(EntityState.Unchanged, entry.State);

        context.ChangeTracker.DetectChanges();

        Assert.True(changeDetector.DetectChangesCalled);
        Assert.Equal(EntityState.Modified, entry.State);
    }

    [ConditionalFact]
    public void Set_throws_for_shared_types()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Model.AddEntityType("SharedQuestion", typeof(Question));

        var optionsBuilder = new DbContextOptionsBuilder();
        optionsBuilder
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .UseInternalServiceProvider(InMemoryTestHelpers.Instance.CreateServiceProvider())
            .UseModel(modelBuilder.FinalizeModel());
        using var context = new DbContext(optionsBuilder.Options);
        var ex = Assert.Throws<InvalidOperationException>(() => context.Set<Question>().Local);
        Assert.Equal(CoreStrings.InvalidSetSharedType(typeof(Question).ShortDisplayName()), ex.Message);
    }

    [ConditionalFact]
    public void SaveChanges_calls_DetectChanges()
    {
        var services = new ServiceCollection()
            .AddScoped<IStateManager, FakeStateManager>()
            .AddScoped<IChangeDetector, FakeChangeDetector>();

        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<User>();

        var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider(services);

        using var context = new DbContext(
            new DbContextOptionsBuilder()
                .UseInternalServiceProvider(serviceProvider)
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseModel(modelBuilder.FinalizeModel())
                .Options);
        var changeDetector = (FakeChangeDetector)context.GetService<IChangeDetector>();

        Assert.False(changeDetector.DetectChangesCalled);

        context.SaveChanges();

        Assert.True(changeDetector.DetectChangesCalled);
    }

    [ConditionalFact]
    public async Task SaveChangesAsync_with_canceled_token()
    {
        var loggerFactory = new ListLoggerFactory();

        var provider =
            InMemoryTestHelpers.Instance.CreateServiceProvider(
                new ServiceCollection().AddSingleton<ILoggerFactory>(loggerFactory));

        using var context = new ButTheHedgehogContext(provider);
        context.Products.Add(
            new Product
            {
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => context.SaveChangesAsync(new CancellationToken(canceled: true)));

        Assert.Contains(CoreEventId.SaveChangesCanceled, loggerFactory.Log.Select(l => l.Id));
        Assert.DoesNotContain(CoreEventId.SaveChangesFailed, loggerFactory.Log.Select(l => l.Id));
    }

    [ConditionalFact]
    public void Entry_methods_check_arguments()
    {
        var services = new ServiceCollection()
            .AddScoped<IStateManager, FakeStateManager>();

        var serviceProvider = InMemoryTestHelpers.Instance.CreateServiceProvider(services);

        using var context = new EarlyLearningCenter(serviceProvider);
        Assert.Equal(
            "entity",
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => context.Entry(null)).ParamName);
        Assert.Equal(
            "entity",
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => context.Entry<Random>(null)).ParamName);
    }

    private class FakeChangeDetector : IChangeDetector
    {
        public bool DetectChangesCalled { get; set; }

        public void DetectChanges(IStateManager stateManager)
            => DetectChangesCalled = true;

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

        public (EventHandler<DetectChangesEventArgs> DetectingAllChanges,
            EventHandler<DetectedChangesEventArgs> DetectedAllChanges,
            EventHandler<DetectEntityChangesEventArgs> DetectingEntityChanges,
            EventHandler<DetectedEntityChangesEventArgs>
            DetectedEntityChanges) CaptureEvents()
            => (null, null, null, null);

        public void SetEvents(
            EventHandler<DetectChangesEventArgs> detectingAllChanges,
            EventHandler<DetectedChangesEventArgs> detectedAllChanges,
            EventHandler<DetectEntityChangesEventArgs> detectingEntityChanges,
            EventHandler<DetectedEntityChangesEventArgs> detectedEntityChanges)
        {
        }

        public event EventHandler<DetectEntityChangesEventArgs> DetectingEntityChanges;

        public void OnDetectingEntityChanges(InternalEntityEntry internalEntityEntry)
            => DetectingEntityChanges?.Invoke(null, null);

        public event EventHandler<DetectChangesEventArgs> DetectingAllChanges;

        public void OnDetectingAllChanges(IStateManager stateManager)
            => DetectingAllChanges?.Invoke(null, null);

        public event EventHandler<DetectedEntityChangesEventArgs> DetectedEntityChanges;

        public void OnDetectedEntityChanges(InternalEntityEntry internalEntityEntry, bool changesFound)
            => DetectedEntityChanges?.Invoke(null, null);

        public event EventHandler<DetectedChangesEventArgs> DetectedAllChanges;

        public void OnDetectedAllChanges(IStateManager stateManager, bool changesFound)
            => DetectedAllChanges?.Invoke(null, null);

        public void ResetState()
        {
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Can_change_navigation_while_attaching_entities(bool async)
    {
        using (var context = new ActiveAddContext())
        {
            context.Database.EnsureDeleted();

            context.AddRange(
                new User { Id = 3 }, new User { Id = 4 });
            context.SaveChanges();
        }

        using (var context = new ActiveAddContext())
        {
            var questions = new List<Question>
            {
                new() { Author = context.Users.First(), Answers = new List<Answer> { new() { Author = context.Users.Last() } } }
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
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(databaseName: "issue7119");

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<Question>(b => b.HasOne(x => x.Author).WithMany(x => x.Questions).HasForeignKey(x => x.AuthorId));

            modelBuilder.Entity<Answer>(
                b =>
                {
                    b.HasOne(x => x.Author).WithMany(x => x.Answers).HasForeignKey(x => x.AuthorId);
                    b.HasOne(x => x.Question).WithMany(x => x.Answers).HasForeignKey(x => x.AuthorId);
                });
        }
    }

    [ConditionalFact]
    public void Context_can_build_model_using_DbSet_properties()
    {
        using var context = new EarlyLearningCenter(InMemoryTestHelpers.Instance.CreateServiceProvider());
        Assert.Equal(
            new[]
            {
                typeof(Category).FullName,
                typeof(CategoryWithSentinel).FullName,
                typeof(Product).FullName,
                typeof(ProductWithSentinel).FullName,
                typeof(TheGu).FullName,
                typeof(TheGuWithSentinel).FullName
            },
            context.Model.GetEntityTypes().Select(e => e.Name).ToArray());

        var categoryType = context.Model.FindEntityType(typeof(Category))!;
        Assert.Equal("Id", categoryType.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(
            new[] { "Id", "Name" },
            categoryType.GetProperties().Select(p => p.Name).ToArray());

        var productType = context.Model.FindEntityType(typeof(Product))!;
        Assert.Equal("Id", productType.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(
            new[] { "Id", "CategoryId", "Name", "Price" },
            productType.GetProperties().Select(p => p.Name).ToArray());

        var guType = context.Model.FindEntityType(typeof(TheGu))!;
        Assert.Equal("Id", guType.FindPrimaryKey()!.Properties.Single().Name);
        Assert.Equal(
            new[] { "Id", "ShirtColor" },
            guType.GetProperties().Select(p => p.Name).ToArray());
    }

    [ConditionalFact]
    public void Context_will_use_explicit_model_if_set_in_config()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<TheGu>();

        using var context = new EarlyLearningCenter(
            InMemoryTestHelpers.Instance.CreateServiceProvider(),
            new DbContextOptionsBuilder().UseModel(modelBuilder.FinalizeModel()).Options);
        Assert.Equal(
            new[] { typeof(TheGu).FullName },
            context.Model.GetEntityTypes().Select(e => e.Name).ToArray());
    }

    [ConditionalFact]
    public void Context_initializes_all_DbSet_properties_with_setters()
    {
        using var context = new ContextWithSets();
        Assert.NotNull(context.Products);
        Assert.NotNull(context.Categories);
        Assert.NotNull(context.GetGus());
        Assert.Null(context.NoSetter);
    }

    private class ContextWithSets : DbContext
    {
        public DbSet<Product> Products { get; set; }
        public DbSet<Category> Categories { get; private set; }
        private DbSet<TheGu> Gus { get; set; }

        public DbSet<Random> NoSetter { get; } = null;

        public DbSet<TheGu> GetGus()
            => Gus;
    }

    [ConditionalFact]
    public void Model_cannot_be_used_in_OnModelCreating()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider(validateScopes: true);

        using var context = new UseModelInOnModelCreatingContext(serviceProvider);
        Assert.Equal(
            CoreStrings.RecursiveOnModelCreating,
            Assert.Throws<InvalidOperationException>(() => context.Model).Message);
    }

    private class UseModelInOnModelCreatingContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public DbSet<Product> Products { get; set; }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            // ReSharper disable once AssignmentIsFullyDiscarded
            => _ = Model;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(_serviceProvider);
    }

    [ConditionalFact]
    public void Context_cannot_be_used_in_OnModelCreating()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider(validateScopes: true);

        using var context = new UseInOnModelCreatingContext(serviceProvider);
        Assert.Equal(
            CoreStrings.RecursiveOnModelCreating,
            Assert.Throws<InvalidOperationException>(() => context.Products.ToList()).Message);
    }

    private class UseInOnModelCreatingContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public DbSet<Product> Products { get; set; }

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            => Products.ToList();

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .UseInternalServiceProvider(_serviceProvider);
    }

    [ConditionalFact]
    public void Context_cannot_be_used_in_OnConfiguring()
    {
        var serviceProvider = new ServiceCollection()
            .AddEntityFrameworkInMemoryDatabase()
            .BuildServiceProvider(validateScopes: true);

        using var context = new UseInOnConfiguringContext(serviceProvider);
        Assert.Equal(
            CoreStrings.RecursiveOnConfiguring,
            Assert.Throws<InvalidOperationException>(() => context.Products.ToList()).Message);
    }

    private class UseInOnConfiguringContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public DbSet<Product> Products { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInternalServiceProvider(_serviceProvider);

            // ReSharper disable once ReturnValueOfPureMethodIsNotUsed
            Products.ToList();

            base.OnConfiguring(optionsBuilder);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task SaveChanges_calls_DetectChanges_by_default(bool async)
    {
        var provider = InMemoryTestHelpers.Instance.CreateServiceProvider();

        using (var context = new ButTheHedgehogContext(provider))
        {
            Assert.True(context.ChangeTracker.AutoDetectChangesEnabled);

            var product = (await context.AddAsync(
                new Product
                {
                    Id = 1,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                })).Entity;

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
                Assert.Equal(1, await context.SaveChangesAsync());
            }
            else
            {
                Assert.Equal(1, context.SaveChanges());
            }
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task Auto_DetectChanges_for_SaveChanges_can_be_switched_off(bool async)
    {
        var provider = InMemoryTestHelpers.Instance.CreateServiceProvider();

        using (var context = new ButTheHedgehogContext(provider))
        {
            context.ChangeTracker.AutoDetectChangesEnabled = false;
            Assert.False(context.ChangeTracker.AutoDetectChangesEnabled);

            var product = (await context.AddAsync(
                new Product
                {
                    Id = 1,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                })).Entity;

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
                Assert.Equal(0, await context.SaveChangesAsync());
            }
            else
            {
                Assert.Equal(0, context.SaveChanges());
            }
        }
    }

    private class ButTheHedgehogContext(IServiceProvider serviceProvider) : DbContext
    {
        private readonly IServiceProvider _serviceProvider = serviceProvider;

        public DbSet<Product> Products { get; set; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(nameof(ButTheHedgehogContext))
                .UseInternalServiceProvider(_serviceProvider);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void DetectChanges_is_called_for_cascade_delete_unless_disabled(bool autoDetectChangesEnabled)
    {
        var detectedChangesFor = new List<object>();

        using var context = new EarlyLearningCenter();
        context.ChangeTracker.DetectingEntityChanges += (_, args) =>
        {
            detectedChangesFor.Add(args.Entry.Entity);
        };

        context.ChangeTracker.AutoDetectChangesEnabled = autoDetectChangesEnabled;

        var products = new List<Product>
        {
            new()
            {
                Id = 1,
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            },
            new()
            {
                Id = 2,
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }
        };
        var category = context.Attach(
            new Category
            {
                Id = 1,
                Products = products,
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            }).Entity;

        Assert.Empty(detectedChangesFor);

        context.Remove(category);

        if (autoDetectChangesEnabled)
        {
            Assert.Equal(4, detectedChangesFor.Count);
            Assert.Contains(products[0], detectedChangesFor);
            Assert.Contains(products[1], detectedChangesFor);
            Assert.Equal(2, detectedChangesFor.Count(e => ReferenceEquals(e, category)));
        }
        else
        {
            Assert.Empty(detectedChangesFor);
        }
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Entry_calls_DetectChanges_by_default(bool useGenericOverload)
    {
        using var context = new ButTheHedgehogContext(InMemoryTestHelpers.Instance.CreateServiceProvider());
        var entry = context.Attach(
            new Product
            {
                Id = 1,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });

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

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public void Auto_DetectChanges_for_Entry_can_be_switched_off(bool useGenericOverload)
    {
        using var context = new ButTheHedgehogContext(InMemoryTestHelpers.Instance.CreateServiceProvider());
        context.ChangeTracker.AutoDetectChangesEnabled = false;

        var entry = context.Attach(
            new Product
            {
                Id = 1,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });

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

    [ConditionalFact]
    public async Task Add_Attach_Remove_Update_do_not_call_DetectChanges()
    {
        var provider =
            InMemoryTestHelpers.Instance.CreateServiceProvider(
                new ServiceCollection().AddScoped<IChangeDetector, ChangeDetectorProxy>());
        using var context = new ButTheHedgehogContext(provider);
        var changeDetector = (ChangeDetectorProxy)context.GetService<IChangeDetector>();

        var id = 1;

        changeDetector.DetectChangesCalled = false;

        context.Add(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.Add(
            (object)new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.AddRange(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.AddRange(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.AddRange(
            new List<Product>
            {
                new()
                {
                    Id = id++,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                }
            });
        context.AddRange(
            new List<object>
            {
                new Product
                {
                    Id = id++,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                }
            });
        await context.AddAsync(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        await context.AddAsync(
            (object)new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        await context.AddRangeAsync(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        await context.AddRangeAsync(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        await context.AddRangeAsync(
            new List<Product>
            {
                new()
                {
                    Id = id++,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                }
            });
        await context.AddRangeAsync(
            new List<object>
            {
                new Product
                {
                    Id = id++,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                }
            });
        context.Attach(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.Attach(
            (object)new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.AttachRange(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.AttachRange(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.AttachRange(
            new List<Product>
            {
                new()
                {
                    Id = id++,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                }
            });
        context.AttachRange(
            new List<object>
            {
                new Product
                {
                    Id = id++,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                }
            });
        context.Update(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.Update(
            (object)new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.UpdateRange(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.UpdateRange(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.UpdateRange(
            new List<Product>
            {
                new()
                {
                    Id = id++,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                }
            });
        context.UpdateRange(
            new List<object>
            {
                new Product
                {
                    Id = id++,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                }
            });
        context.Remove(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.Remove(
            (object)new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.RemoveRange(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.RemoveRange(
            new Product
            {
                Id = id++,
                Name = "Little Hedgehogs",
                Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                Tag = new Tag
                {
                    Name = "Tanavast",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                    Notes = ["A", "B"]
                }
            });
        context.RemoveRange(
            new List<Product>
            {
                new()
                {
                    Id = id++,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                }
            });
        context.RemoveRange(
            new List<object>
            {
                new Product
                {
                    Id = id,
                    Name = "Little Hedgehogs",
                    Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7146") },
                    Tag = new Tag
                    {
                        Name = "Tanavast",
                        Stamp = new Stamp { Code = new Guid("984ade3c-2f7b-4651-a351-642e92ab7147") },
                        Notes = ["A", "B"]
                    }
                }
            });

        Assert.False(changeDetector.DetectChangesCalled);

        context.ChangeTracker.DetectChanges();

        Assert.True(changeDetector.DetectChangesCalled);
    }

    private class ChangeDetectorProxy(
        IDiagnosticsLogger<DbLoggerCategory.ChangeTracking> logger,
        ILoggingOptions loggingOptions) : ChangeDetector(logger, loggingOptions)
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

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
    public async Task It_throws_object_disposed_exception(bool async)
    {
        var context = new DbContext(new DbContextOptions<DbContext>());

        if (async)
        {
            await context.DisposeAsync();
        }
        else
        {
            context.Dispose();
        }

        // methods (tests all paths)
        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.Add(new object())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.Find(typeof(Random), 77)).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.Attach(new object())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.Update(new object())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.Remove(new object())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.SaveChanges()).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            (await Assert.ThrowsAsync<ObjectDisposedException>(() => context.SaveChangesAsync())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            (await Assert.ThrowsAsync<ObjectDisposedException>(() => context.AddAsync(new object()).AsTask())).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            (await Assert.ThrowsAsync<ObjectDisposedException>(() => context.FindAsync(typeof(Random), 77).AsTask())).Message);

        var methodCount = typeof(DbContext).GetMethods(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly).Count();
        var expectedMethodCount = 50;
        Assert.True(
            methodCount == expectedMethodCount,
            userMessage: $"Expected {expectedMethodCount} methods on DbContext but found {methodCount}. "
            + "Update test to ensure all methods throw ObjectDisposedException after dispose.");

        // getters
        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.ChangeTracker).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.Model).Message);

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => context.GetService<IDesignTimeModel>().Model).Message);

        var expectedProperties = new List<string>
        {
            nameof(DbContext.ChangeTracker),
            nameof(DbContext.ContextId), // By-design, does not throw for disposed context
            nameof(DbContext.Database),
            nameof(DbContext.Model)
        };

        Assert.True(
            expectedProperties.SequenceEqual(
                typeof(DbContext)
                    .GetProperties()
                    .Select(p => p.Name)
                    .OrderBy(s => s)
                    .ToList()),
            userMessage: "Unexpected properties on DbContext. "
            + "Update test to ensure all getters throw ObjectDisposedException after dispose.");

        Assert.StartsWith(
            CoreStrings.ContextDisposed,
            Assert.Throws<ObjectDisposedException>(() => ((IInfrastructure<IServiceProvider>)context).Instance).Message);
    }

    [ConditionalFact]
    public void It_throws_with_derived_name()
    {
        var context = new EarlyLearningCenter();

        context.Dispose();

        Assert.Throws<ObjectDisposedException>(() => context.Model);
    }

    public class FakeServiceProvider : IServiceProvider, IDisposable
    {
        private readonly IServiceProvider _realProvider;

        public FakeServiceProvider()
        {
            _realProvider = new ServiceCollection().AddEntityFrameworkInMemoryDatabase().BuildServiceProvider(validateScopes: true);
        }

        public bool Disposed { get; set; }

        public void Dispose()
            => Disposed = true;

        public object GetService(Type serviceType)
        {
            if (serviceType == typeof(IServiceProvider))
            {
                return this;
            }

            return serviceType == typeof(IServiceScopeFactory) ? new FakeServiceScopeFactory() : _realProvider.GetService(serviceType);
        }

        public class FakeServiceScopeFactory : IServiceScopeFactory
        {
            public static FakeServiceScope Scope { get; } = new();

            public IServiceScope CreateScope()
                => Scope;
        }

        public class FakeServiceScope : IServiceScope
        {
            public bool Disposed { get; set; }

            public IServiceProvider ServiceProvider { get; set; } = new FakeServiceProvider();

            public void Dispose()
                => Disposed = true;
        }
    }

    [ConditionalFact]
    public void Adding_entities_with_shadow_keys_should_not_throw()
    {
        using (var context = new NullShadowKeyContext())
        {
            var assembly = new TestAssembly { Name = "Assembly1" };
            var testClass = new TestClass { Assembly = assembly, Name = "Class1" };
            var test = context.Tests.Add(
                new Test { Class = testClass, Name = "Test1" }).Entity;

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
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
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
            => options
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(nameof(NullShadowKeyContext));

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
                    x.HasOne(t => t.Class).WithMany(c => c.Tests)
                        .HasForeignKey("AssemblyName", "ClassName");
                    x.HasKey("AssemblyName", "ClassName", nameof(Test.Name));
                });
        }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Query.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;

// ReSharper disable UnusedMember.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ChangeTracking.Internal;

public class ChangeDetectorTest
{
    [Fact]
    public void PropertyChanging_does_not_snapshot_if_eager_snapshots_are_in_use()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildModel());
        var entry = CreateInternalEntry<Product>(contextServices);

        Assert.True(entry.EntityType.UseEagerSnapshots());
        Assert.False(entry.HasRelationshipSnapshot);

        contextServices
            .GetRequiredService<IChangeDetector>()
            .PropertyChanging(entry, entry.EntityType.FindProperty("DependentId")!);

        Assert.False(entry.HasRelationshipSnapshot);
    }

    [Fact]
    public void PropertyChanging_snapshots_original_and_FK_value_if_lazy_snapshots_are_in_use()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildNotifyingModel());
        var entity = new NotifyingProduct { DependentId = 77 };
        var entry = CreateInternalEntry(contextServices, entity);
        entry.SetEntityState(EntityState.Unchanged);

        Assert.False(entry.EntityType.UseEagerSnapshots());
        Assert.False(entry.HasRelationshipSnapshot);

        var property = entry.EntityType.FindProperty("DependentId")!;

        contextServices
            .GetRequiredService<IChangeDetector>()
            .PropertyChanging(entry, property);

        Assert.True(entry.HasRelationshipSnapshot);

        Assert.Equal(77, entry.GetRelationshipSnapshotValue(property));
        Assert.Equal(77, entry.GetOriginalValue(property));
        Assert.Equal(77, entry.GetCurrentValue(property));

        entity.DependentId = 777;

        Assert.Equal(777, entry.GetRelationshipSnapshotValue(property)); // Because is now changed
        Assert.Equal(77, entry.GetOriginalValue(property));
        Assert.Equal(777, entry.GetCurrentValue(property));
    }

    [Fact]
    public void PropertyChanging_does_not_snapshot_original_values_for_properties_with_no_original_value_tracking()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildNotifyingModel());
        var entity = new NotifyingProduct { Name = "Cheese" };
        var entry = CreateInternalEntry(contextServices, entity);

        Assert.False(entry.EntityType.UseEagerSnapshots());

        var property = entry.EntityType.FindProperty("Name")!;

        contextServices
            .GetRequiredService<IChangeDetector>()
            .PropertyChanging(entry, property);

        Assert.Equal("Cheese", entry.GetRelationshipSnapshotValue(property));
        Assert.Equal("Cheese", entry.GetCurrentValue(property));

        entity.Name = "Pickle";

        Assert.Equal("Pickle", entry.GetRelationshipSnapshotValue(property));
        Assert.Equal("Pickle", entry.GetCurrentValue(property));
    }

    [Fact]
    public void PropertyChanging_snapshots_reference_navigations_if_lazy_snapshots_are_in_use()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildNotifyingModel());
        var category = new NotifyingCategory();
        var entity = new NotifyingProduct { Category = category };
        var entry = CreateInternalEntry(contextServices, entity);
        entry.SetEntityState(EntityState.Added);

        Assert.False(entry.EntityType.UseEagerSnapshots());
        Assert.False(entry.HasRelationshipSnapshot);

        var navigation = entry.EntityType.FindNavigation("Category")!;

        contextServices
            .GetRequiredService<IChangeDetector>()
            .PropertyChanging(entry, navigation);

        Assert.True(entry.HasRelationshipSnapshot);

        Assert.Same(category, entry.GetRelationshipSnapshotValue(navigation));
        Assert.Same(category, entry.GetCurrentValue(navigation));

        var newCategory = new NotifyingCategory { Id = 7, PrincipalId = 11 };
        entity.Category = newCategory;

        Assert.Same(newCategory, entry.GetRelationshipSnapshotValue(navigation));
        Assert.NotSame(category, entry.GetCurrentValue(navigation));
    }

    [Fact]
    public void PropertyChanging_snapshots_PK_for_relationships_if_lazy_snapshots_are_in_use()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildNotifyingModel());
        var id = Guid.NewGuid();
        var entity = new NotifyingProduct { Id = id };
        var entry = CreateInternalEntry(contextServices, entity);
        entry.SetEntityState(EntityState.Added);

        Assert.False(entry.EntityType.UseEagerSnapshots());
        Assert.False(entry.HasRelationshipSnapshot);

        var property = entry.EntityType.FindProperty("Id")!;

        contextServices
            .GetRequiredService<IChangeDetector>()
            .PropertyChanging(entry, property);

        Assert.True(entry.HasRelationshipSnapshot);

        Assert.Equal(id, entry.GetRelationshipSnapshotValue(property));
        Assert.Equal(id, entry.GetCurrentValue(property));

        var newId = Guid.NewGuid();
        entity.Id = newId;

        Assert.Equal(newId, entry.GetRelationshipSnapshotValue(property)); // Because now changed.
        Assert.Equal(newId, entry.GetCurrentValue(property));
    }

    [Fact]
    public void Detects_scalar_property_change()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildModel());

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

        var product = new Product { Id = Guid.NewGuid(), Name = "Oculus Rift" };
        var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.Name = "Gear VR";

        changeDetector.DetectChanges(entry);

        Assert.Equal(EntityState.Modified, entry.EntityState);
        Assert.True(entry.IsModified(entry.EntityType.FindProperty("Name")!));
    }

    [Theory, InlineData(true, true, true), InlineData(false, true, true), InlineData(true, false, true),
     InlineData(false, false, true), InlineData(true, true, false), InlineData(false, true, false), InlineData(true, false, false),
     InlineData(false, false, false)]
    public void Can_insert_with_array_comparer(bool useTypeMapping, bool useStateChange, bool nullValue)
    {
        using var context = useTypeMapping ? new BaxterWithMappingContext() : new BaxterContext();
        var value = nullValue ? null : new[] { 1, 2, 3, 4 };

        var baxter = new Baxter { Id = Guid.NewGuid(), Demands = value! };

        var entityEntry = context.Entry(baxter);

        if (useStateChange)
        {
            entityEntry.State = EntityState.Added;
        }
        else
        {
            context.Add(baxter);
        }

        Assert.Equal(EntityState.Added, entityEntry.State);
        Assert.Equal(value, entityEntry.Property(e => e.Demands).CurrentValue);

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, entityEntry.State);

        if (nullValue)
        {
            baxter.Demands = [1, 767, 3, 4];
        }
        else
        {
            baxter.Demands![1] = 767;
        }

        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entityEntry.State);
        Assert.Equal(new[] { 1, 767, 3, 4 }, entityEntry.Property(e => e.Demands).CurrentValue);
    }

    [Theory, InlineData(true), InlineData(false)]
    public void Detects_scalar_property_change_with_custom_comparer(bool useTypeMapping)
    {
        using var context = useTypeMapping ? new BaxterWithMappingContext() : new BaxterContext();
        var baxter = context.Attach(
            new Baxter { Id = Guid.NewGuid(), Demands = [1, 2, 3, 4] }).Entity;

        baxter.Demands![2] = 33;

        var entityEntry = context.Entry(baxter);
    #pragma warning disable CS8620
        AssertDetected(entityEntry, entityEntry.Property(e => e.Demands));
    #pragma warning restore CS8620

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, entityEntry.State);

        baxter.Demands![1] = 767;

        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entityEntry.State);
        Assert.Equal(new[] { 1, 767, 3, 4 }, entityEntry.Property(e => e.Demands).CurrentValue);
    }

    [Theory, InlineData(true), InlineData(false)]
    public void Detects_scalar_shadow_property_change_with_custom_comparer(bool useTypeMapping)
    {
        using var context = useTypeMapping ? new BaxterWithMappingContext() : new BaxterContext();
        var entityEntry = context.Entry(
            new Baxter { Id = Guid.NewGuid() });
        entityEntry.Property("ShadyDemands").CurrentValue = new[] { 1, 2, 3, 4 };
        entityEntry.State = EntityState.Unchanged;

        var propertyEntry = entityEntry.Property<int[]>("ShadyDemands");

        propertyEntry.CurrentValue[2] = 33;

        context.ChangeTracker.DetectChanges(); // Needed because array is being mutated

        AssertDetected(entityEntry, propertyEntry);

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, entityEntry.State);

        propertyEntry.CurrentValue[1] = 767;

        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entityEntry.State);
        Assert.Equal(new[] { 1, 767, 3, 4 }, propertyEntry.CurrentValue);
    }

    // ReSharper disable once ParameterOnlyUsedForPreconditionCheck.Local
    private static void AssertDetected(EntityEntry<Baxter> entityEntry, PropertyEntry<Baxter, int[]> propertyEntry)
    {
        Assert.Equal(EntityState.Modified, entityEntry.State);
        Assert.True(propertyEntry.IsModified);
        Assert.Equal(new[] { 1, 2, 3, 4 }, propertyEntry.OriginalValue);
        Assert.Equal(new[] { 1, 2, 33, 4 }, propertyEntry.CurrentValue);

        propertyEntry.IsModified = false;

        Assert.Equal(EntityState.Unchanged, entityEntry.State);
        Assert.False(propertyEntry.IsModified);
        Assert.Equal(new[] { 1, 2, 3, 4 }, propertyEntry.OriginalValue);
        Assert.Equal(new[] { 1, 2, 3, 4 }, propertyEntry.CurrentValue);
    }

    private class Baxter
    {
        public Guid Id { get; set; }
        public int[]? Demands { get; set; }
    }

    private class BaxterWithMappingContext : BaxterContext
    {
        protected override bool UseTypeMapping
            => true;
    }

    private class ConcreteTypeMapping : CoreTypeMapping
    {
        private ConcreteTypeMapping(CoreTypeMappingParameters parameters)
            : base(parameters)
        {
        }

        public ConcreteTypeMapping(Type clrType, ValueConverter converter, ValueComparer comparer)
            : base(new CoreTypeMappingParameters(clrType, converter, comparer))
        {
        }

        public override CoreTypeMapping WithComposedConverter(
            ValueConverter? converter,
            ValueComparer? comparer = null,
            ValueComparer? keyComparer = null,
            CoreTypeMapping? elementMapping = null,
            JsonValueReaderWriter? jsonValueReaderWriter = null)
            => new ConcreteTypeMapping(
                Parameters.WithComposedConverter(
                    converter, comparer, keyComparer, elementMapping, jsonValueReaderWriter));

        protected override CoreTypeMapping Clone(CoreTypeMappingParameters parameters)
            => new ConcreteTypeMapping(parameters);
    }

    private class BaxterContext : DbContext
    {
        protected virtual bool UseTypeMapping
            => false;

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInternalServiceProvider(InMemoryFixture.DefaultServiceProvider)
                .UseInMemoryDatabase(Guid.NewGuid().ToString());

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            var intArrayComparer = new ValueComparer<int[]>(
                (l, r) => (l == null || r == null) ? (l == r) : l.SequenceEqual(r),
                v => v == null ? 0 : v.Aggregate(0, (t, e) => (t * 397) ^ e),
                v => v == null ? null! : v.ToArray());

            var intArrayConverter = new ValueConverter<int[], string>(
                v => string.Join(",", v.Select(i => i.ToString())),
                v => v.Split(new[] { ',' }, StringSplitOptions.None).Select(int.Parse).ToArray());

            var property = modelBuilder.Entity<Baxter>()
                .Property(e => e.Demands)
                .Metadata;

            var shadowProperty = modelBuilder.Entity<Baxter>()
                .Property<int[]>("ShadyDemands")
                .Metadata;

            if (UseTypeMapping)
            {
                property.SetTypeMapping(
                    new ConcreteTypeMapping(typeof(int[]), intArrayConverter, intArrayComparer));

                shadowProperty.SetTypeMapping(
                    new ConcreteTypeMapping(typeof(int[]), intArrayConverter, intArrayComparer));
            }
            else
            {
                property.SetValueConverter(intArrayConverter);
                shadowProperty.SetValueConverter(intArrayConverter);

                property.SetValueComparer(intArrayComparer);
                shadowProperty.SetValueComparer(intArrayComparer);
            }
        }
    }

    [Fact]
    public void Skips_detection_of_scalar_property_change_for_notification_entities()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildModelWithChanged());

        var stateManager = contextServices.GetRequiredService<IStateManager>();
        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

        var product = new ProductWithChanged { Id = 1, Name = "Oculus Rift" };
        var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.Name = "Gear VR";

        changeDetector.DetectChanges(stateManager);

        Assert.Equal(EntityState.Unchanged, entry.EntityState);
        Assert.False(entry.IsModified(entry.EntityType.FindProperty("Name")!));
    }

    [Fact]
    public void Skips_local_detection_of_scalar_property_change_for_notification_entities()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildModelWithChanged());

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

        var category = new CategoryWithChanged { Id = 1, Name = "Oculus Rift" };
        var entry = contextServices.GetRequiredService<IStateManager>().GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        category.Name = "Gear VR";

        changeDetector.DetectChanges(entry);

        Assert.Equal(EntityState.Unchanged, entry.EntityState);
        Assert.False(entry.IsModified(entry.EntityType.FindProperty("Name")!));
    }

    [Fact]
    public void Detects_principal_key_change()
    {
        var contextServices = CreateContextServices();

        var stateManager = contextServices.GetRequiredService<IStateManager>();
        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

        var category = new Category { Id = -1, PrincipalId = 77 };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        category.PrincipalId = 78;

        changeDetector.DetectChanges(entry);

        Assert.Equal(78, entry.GetRelationshipSnapshotValue(entry.EntityType.FindProperty("PrincipalId")!));

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("PrincipalId")!, testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(77, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Detects_principal_key_changing_back_to_original_value()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new Category { Id = -1, PrincipalId = 77 };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        category.PrincipalId = 78;

        changeDetector.DetectChanges(entry);

        category.PrincipalId = 77;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("PrincipalId")!, testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(78, testListener.KeyChange.Item5);
        Assert.Equal(77, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Reacts_to_principal_key_change_in_sidecar()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new Category { Id = -1, PrincipalId = 77 };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        var property = entry.EntityType.FindProperty("PrincipalId")!;
        entry.PrepareToSave();

        entry[property] = 78;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(property, testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(77, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Detects_primary_key_change()
    {
        var contextServices = CreateContextServices();

        var stateManager = contextServices.GetRequiredService<IStateManager>();
        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

        var category = new Category
        {
            Id = -1,
            TagId = 777,
            PrincipalId = 778
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        category.Id = 78;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("Id")!, testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(-1, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Reacts_to_primary_key_change_in_sidecar()
    {
        var contextServices = CreateContextServices();

        var stateManager = contextServices.GetRequiredService<IStateManager>();
        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

        var category = new Category
        {
            Id = -1,
            TagId = 777,
            PrincipalId = 778
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        var property = entry.EntityType.FindProperty("Id")!;
        entry.PrepareToSave();

        entry[property] = 78;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("Id")!, testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(-1, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Ignores_no_change_to_principal_key()
    {
        var contextServices = CreateContextServices();

        var stateManager = contextServices.GetRequiredService<IStateManager>();
        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

        var category = new Category { Id = -1, PrincipalId = 77 };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        category.PrincipalId = 77;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Ignores_no_change_to_principal_key_in_sidecar()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new Category { Id = -1, PrincipalId = 77 };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);
        entry.PrepareToSave();

        var property = entry.EntityType.FindProperty("PrincipalId")!;

        entry[property] = 77;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Detects_foreign_key_change()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.DependentId = 78;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("DependentId")!, testListener.KeyChange.Item2);
        Assert.Empty(testListener.KeyChange.Item3);
        Assert.Single(testListener.KeyChange.Item4);
        Assert.Equal(77, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Detects_foreign_key_changing_back_to_original_value()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.DependentId = 78;

        changeDetector.DetectChanges(entry);

        product.DependentId = 77;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("DependentId")!, testListener.KeyChange.Item2);
        Assert.Empty(testListener.KeyChange.Item3);
        Assert.Single(testListener.KeyChange.Item4);
        Assert.Equal(78, testListener.KeyChange.Item5);
        Assert.Equal(77, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Reacts_to_foreign_key_change_in_sidecar()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);
        entry.PrepareToSave();

        var property = entry.EntityType.FindProperty("DependentId")!;
        entry[property] = 78;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(property, testListener.KeyChange.Item2);
        Assert.Empty(testListener.KeyChange.Item3);
        Assert.Single(testListener.KeyChange.Item4);
        Assert.Equal(77, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Ignores_no_change_to_foreign_key()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.DependentId = 77;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Ignores_no_change_to_foreign_key_in_sidecar()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);
        entry.PrepareToSave();

        var property = entry.EntityType.FindProperty("DependentId")!;
        entry[property] = 77;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Detects_reference_navigation_change()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var originalCategory = new Category { Id = 77, PrincipalId = 1 };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Category = originalCategory,
            DependentId = 1
        };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        var newCategory = new Category { Id = 1, PrincipalId = 2 };
        product.Category = newCategory!;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.ReferenceChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Category")!, testListener.ReferenceChange.Item2);
        Assert.Equal(originalCategory, testListener.ReferenceChange.Item3);
        Assert.Equal(newCategory, testListener.ReferenceChange.Item4);

        Assert.Null(testListener.CollectionChange);

        Assert.Equal("DependentId", testListener.KeyChange.Item2.Name);
        Assert.Equal(1, testListener.KeyChange.Item5);
        Assert.Equal(2, testListener.KeyChange.Item6);
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Detects_reference_navigation_changing_back_to_original_value(bool useNull)
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var originalCategory = new Category
        {
            Id = 77,
            PrincipalId = 1,
            TagId = 777
        };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Category = originalCategory,
            DependentId = 1
        };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        var newCategory = useNull
            ? null
            : new Category
            {
                Id = 99,
                PrincipalId = 2,
                TagId = 778
            };
        product.Category = newCategory!;

        changeDetector.DetectChanges(entry);

        product.Category = originalCategory;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.ReferenceChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Category")!, testListener.ReferenceChange.Item2);
        Assert.Equal(newCategory, testListener.ReferenceChange.Item3);
        Assert.Equal(originalCategory, testListener.ReferenceChange.Item4);

        Assert.Equal("DependentId", testListener.KeyChange.Item2.Name);
        Assert.Equal(useNull ? null : 2, testListener.KeyChange.Item5);
        Assert.Equal(1, testListener.KeyChange.Item6);
    }

    [Fact]
    public void Ignores_no_change_to_reference_navigation()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new Category { Id = 55, PrincipalId = 1 };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Category = category,
            DependentId = 1
        };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.Category = category;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
        Assert.Null(testListener.KeyChange);
    }

    [Fact]
    public void Detects_adding_to_collection_navigation()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product1 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var product2 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var category = new Category
        {
            Id = 1,
            PrincipalId = 77,
            Products = { product1, product2 }
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        var product3 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        category.Products.Add(product3);

        changeDetector.DetectChanges(entry);

        Assert.Equal(EntityState.Unchanged, entry.EntityState);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.CollectionChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Products")!, testListener.CollectionChange.Item2);
        Assert.Equal([product3], testListener.CollectionChange.Item3);
        Assert.Empty(testListener.CollectionChange.Item4);

        var productEntry = stateManager.GetOrCreateEntry(product3);
        Assert.Same(productEntry, testListener.ReferenceChange.Item1);
        Assert.Same(productEntry.EntityType.FindNavigation("Category")!, testListener.ReferenceChange.Item2);
        Assert.Null(testListener.ReferenceChange.Item3);
        Assert.Equal(category, testListener.ReferenceChange.Item4);

        Assert.Null(testListener.KeyChange);
    }

    [Fact]
    public void Detects_removing_from_collection_navigation()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product1 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var product2 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var category = new Category
        {
            Id = 1,
            PrincipalId = 77,
            Products = { product1, product2 }
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        category.Products.Remove(product1);

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.CollectionChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Products")!, testListener.CollectionChange.Item2);
        Assert.Empty(testListener.CollectionChange.Item3);
        Assert.Equal([product1], testListener.CollectionChange.Item4);

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
    }

    [Fact]
    public void Ignores_no_change_to_collection_navigation()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product1 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var product2 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var category = new Category
        {
            Id = 1,
            PrincipalId = 77,
            Products = { product1, product2 }
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        category.Products.Remove(product1);
        category.Products.Add(product1);

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.CollectionChange);
        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
    }

    [Fact]
    public void Skips_detecting_changes_to_primary_principal_key_for_notification_entities()
    {
        var contextServices = CreateContextServices(BuildModelWithChanged());

        var stateManager = contextServices.GetRequiredService<IStateManager>();
        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();

        var product = new ProductWithChanged { Id = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Added);

        product.Id = 78;

        changeDetector.DetectChanges(stateManager);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Skips_detecting_changes_to_foreign_key_for_notification_entities()
    {
        var contextServices = CreateContextServices(BuildModelWithChanged());

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new ProductWithChanged { Id = 1, DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.DependentId = 78;

        changeDetector.DetectChanges(stateManager);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Skips_detecting_changes_to_reference_navigation_for_notification_entities()
    {
        var contextServices = CreateContextServices(BuildModelWithChanged());

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new CategoryWithChanged { Id = 1 };
        var product = new ProductWithChanged
        {
            Id = 2,
            Category = category,
            DependentId = 1
        };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.Category = new CategoryWithChanged { Id = 2 };

        changeDetector.DetectChanges(stateManager);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
        Assert.Null(testListener.KeyChange);
    }

    [Fact]
    public void Skips_detecting_changes_to_notifying_collections()
    {
        var contextServices = CreateContextServices(BuildModelWithChanged());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product1 = new ProductWithChanged { Id = 1, DependentId = 77 };
        var product2 = new ProductWithChanged { Id = 2, DependentId = 77 };
        var category = new CategoryWithChanged { Id = 77, Products = new ObservableCollection<ProductWithChanged> { product1, product2 } };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        var product3 = new ProductWithChanged { Id = 3, DependentId = 77 };
        category.Products.Add(product3);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.CollectionChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Products")!, testListener.CollectionChange.Item2);
        Assert.Equal([product3], testListener.CollectionChange.Item3);
        Assert.Empty(testListener.CollectionChange.Item4);

        var productEntry = stateManager.GetOrCreateEntry(product3);
        Assert.Same(productEntry, testListener.ReferenceChange.Item1);
        Assert.Same(productEntry.EntityType.FindNavigation("Category")!, testListener.ReferenceChange.Item2);
        Assert.Null(testListener.ReferenceChange.Item3);
        Assert.Equal(category, testListener.ReferenceChange.Item4);

        Assert.Null(testListener.KeyChange);
    }

    [Fact]
    public void Brings_in_single_new_entity_set_on_reference_navigation()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var originalCategory = new Category { Id = 77, PrincipalId = 1 };
        var product = new Product
        {
            Id = Guid.NewGuid(),
            Category = originalCategory,
            DependentId = 1
        };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        var newCategory = new Category { PrincipalId = 2, Tag = new CategoryTag() };
        product.Category = newCategory;

        changeDetector.DetectChanges(stateManager);

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(newCategory, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Brings_in_new_entity_set_on_principal_of_one_to_one_navigation()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new Category
        {
            Id = 1,
            TagId = 77,
            PrincipalId = 778
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        var tag = new CategoryTag();
        category.Tag = tag;

        changeDetector.DetectChanges(stateManager);

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(tag, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Brings_in_new_entity_set_on_dependent_of_one_to_one_navigation()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var tag = new CategoryTag { Id = 1 };
        var entry = stateManager.GetOrCreateEntry(tag);
        entry.SetEntityState(EntityState.Unchanged);

        var category = new Category { TagId = 77, PrincipalId = 777 };
        tag.Category = category;

        changeDetector.DetectChanges(stateManager);

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(category, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Brings_in_single_new_entity_set_on_collection_navigation()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product1 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var product2 = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var category = new Category
        {
            Id = 1,
            PrincipalId = 77,
            Products = { product1, product2 }
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        var product3 = new Product { Tag = new ProductTag() };
        category.Products.Add(product3);

        changeDetector.DetectChanges(stateManager);

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(product3, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Brings_in_new_entity_set_on_principal_of_one_to_one_self_ref()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var wife = new Person();
        var entry = stateManager.GetOrCreateEntry(wife);
        entry.SetEntityState(EntityState.Added);

        var husband = new Person();
        wife.Husband = husband;

        changeDetector.DetectChanges(stateManager);

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(husband, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Brings_in_new_entity_set_on_dependent_of_one_to_one_self_ref()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var husband = new Person();
        var entry = stateManager.GetOrCreateEntry(husband);
        entry.SetEntityState(EntityState.Added);

        var wife = new Person();
        husband.Wife = wife;

        changeDetector.DetectChanges(stateManager);

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(wife, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Handles_notification_of_principal_key_change()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new NotifyingCategory { Id = -1, PrincipalId = 77 };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        category.PrincipalId = 78;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("PrincipalId")!, testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(77, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Handles_notification_of_principal_key_changing_back_to_original_value()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new NotifyingCategory { Id = -1, PrincipalId = 77 };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        category.PrincipalId = 78;
        category.PrincipalId = 77;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("PrincipalId")!, testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(78, testListener.KeyChange.Item5);
        Assert.Equal(77, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Handles_notification_of_primary_key_change()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new NotifyingCategory
        {
            Id = -1,
            TagId = 777,
            PrincipalId = 778
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        category.Id = 78;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("Id")!, testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(-1, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Handles_notification_of_no_change_to_principal_key()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new NotifyingCategory { Id = -1, PrincipalId = 77 };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        category.PrincipalId = 77;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Handles_notification_of_foreign_key_change()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.DependentId = 78;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("DependentId")!, testListener.KeyChange.Item2);
        Assert.Empty(testListener.KeyChange.Item3);
        Assert.Single(testListener.KeyChange.Item4);
        Assert.Equal(77, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Handles_notification_of_foreign_key_changing_back_to_original_value()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.DependentId = 78;
        product.DependentId = 77;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("DependentId")!, testListener.KeyChange.Item2);
        Assert.Empty(testListener.KeyChange.Item3);
        Assert.Single(testListener.KeyChange.Item4);
        Assert.Equal(78, testListener.KeyChange.Item5);
        Assert.Equal(77, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Handles_notification_of_no_change_to_foreign_key()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.DependentId = 77;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [Fact]
    public void Handles_notification_of_reference_navigation_change()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var originalCategory = new NotifyingCategory { Id = 66, PrincipalId = 1 };
        var product = new NotifyingProduct
        {
            Id = Guid.NewGuid(),
            Category = originalCategory,
            DependentId = 1
        };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        var newCategory = new NotifyingCategory { Id = 67, PrincipalId = 2 };
        product.Category = newCategory;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.ReferenceChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Category")!, testListener.ReferenceChange.Item2);
        Assert.Equal(originalCategory, testListener.ReferenceChange.Item3);
        Assert.Equal(newCategory, testListener.ReferenceChange.Item4);

        Assert.Equal("DependentId", testListener.KeyChange.Item2.Name);
        Assert.Equal(1, testListener.KeyChange.Item5);
        Assert.Equal(2, testListener.KeyChange.Item6);
    }

    [Theory, InlineData(false), InlineData(true)]
    public void Handles_notification_of_reference_navigation_changing_back_to_original_value(bool useNull)
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var originalCategory = new NotifyingCategory
        {
            Id = 77,
            PrincipalId = 1,
            TagId = 777
        };
        var product = new NotifyingProduct
        {
            Id = Guid.NewGuid(),
            Category = originalCategory,
            DependentId = 1
        };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        var newCategory = useNull
            ? null
            : new NotifyingCategory
            {
                Id = 78,
                PrincipalId = 2,
                TagId = 778
            };

        product.Category = newCategory!;
        product.Category = originalCategory;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.ReferenceChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Category")!, testListener.ReferenceChange.Item2);
        Assert.Equal(newCategory, testListener.ReferenceChange.Item3);
        Assert.Equal(originalCategory, testListener.ReferenceChange.Item4);

        Assert.Equal("DependentId", testListener.KeyChange.Item2.Name);
        Assert.Equal(useNull ? null : 2, testListener.KeyChange.Item5);
        Assert.Equal(1, testListener.KeyChange.Item6);
    }

    [Fact]
    public void Handles_notification_of_no_change_to_reference_navigation()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new NotifyingCategory { Id = 77, PrincipalId = 1 };
        var product = new NotifyingProduct
        {
            Id = Guid.NewGuid(),
            Category = category,
            DependentId = 1
        };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        product.Category = category;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
        Assert.Null(testListener.KeyChange);
    }

    [Fact]
    public void Handles_notification_of_adding_to_collection_navigation()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product1 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
        var product2 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
        var category = new NotifyingCategory
        {
            Id = 1,
            PrincipalId = 77,
            Products = { product1, product2 }
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        var product3 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
        category.Products.Add(product3);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.CollectionChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Products")!, testListener.CollectionChange.Item2);
        Assert.Equal([product3], testListener.CollectionChange.Item3);
        Assert.Empty(testListener.CollectionChange.Item4);

        var productEntry = stateManager.GetOrCreateEntry(product3);
        Assert.Same(productEntry, testListener.ReferenceChange.Item1);
        Assert.Same(productEntry.EntityType.FindNavigation("Category")!, testListener.ReferenceChange.Item2);
        Assert.Null(testListener.ReferenceChange.Item3);
        Assert.Equal(category, testListener.ReferenceChange.Item4);

        Assert.Null(testListener.KeyChange);
    }

    [Fact]
    public void Handles_notification_of_removing_from_collection_navigation()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product1 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
        var product2 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
        var category = new NotifyingCategory
        {
            Id = 1,
            PrincipalId = 77,
            Products = { product1, product2 }
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        product1.Category = category;
        product2.Category = category;

        category.Products.Remove(product1);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.CollectionChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Products")!, testListener.CollectionChange.Item2);
        Assert.Empty(testListener.CollectionChange.Item3);
        Assert.Equal([product1], testListener.CollectionChange.Item4);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.KeyChange);
    }

    [Fact]
    public void Brings_in_single_new_entity_on_notification_of_set_on_reference_navigation()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var originalCategory = new NotifyingCategory { PrincipalId = 1 };
        var product = new NotifyingProduct
        {
            Id = Guid.NewGuid(),
            Category = originalCategory,
            DependentId = 1
        };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);

        var newCategory = new NotifyingCategory { PrincipalId = 2, Tag = new NotifyingCategoryTag() };
        product.Category = newCategory;

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(newCategory, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Brings_in_new_entity_on_notification_of_set_on_principal_of_one_to_one_navigation()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new NotifyingCategory
        {
            Id = 1,
            TagId = 77,
            PrincipalId = 777
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        var tag = new NotifyingCategoryTag { Id = 2 };
        category.Tag = tag;

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(tag, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Brings_in_new_entity_on_notification_of_set_on_dependent_of_one_to_one_navigation()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var tag = new NotifyingCategoryTag { Id = 1 };
        var entry = stateManager.GetOrCreateEntry(tag);
        entry.SetEntityState(EntityState.Unchanged);

        var category = new NotifyingCategory { PrincipalId = 777, TagId = 77 };
        tag.Category = category;

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(category, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Brings_in_single_new_entity_on_notification_of_set_on_collection_navigation()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product1 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
        var product2 = new NotifyingProduct { Id = Guid.NewGuid(), DependentId = 77 };
        var category = new NotifyingCategory
        {
            Id = 1,
            PrincipalId = 77,
            Products = { product1, product2 }
        };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Unchanged);

        var product3 = new NotifyingProduct { Tag = new NotifyingProductTag() };
        category.Products.Add(product3);

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(product3, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Brings_in_new_entity_on_notification_of_set_on_principal_of_one_to_one_self_ref()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var wife = new NotifyingPerson();
        var entry = stateManager.GetOrCreateEntry(wife);
        entry.SetEntityState(EntityState.Added);

        var husband = new NotifyingPerson();
        wife.Husband = husband;

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(husband, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Fact]
    public void Brings_in_new_entity_on_notification_of_set_on_dependent_of_one_to_one_self_ref()
    {
        var contextServices = CreateContextServices(BuildNotifyingModel());

        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var husband = new NotifyingPerson();
        var entry = stateManager.GetOrCreateEntry(husband);
        entry.SetEntityState(EntityState.Added);

        var wife = new NotifyingPerson();
        husband.Wife = wife;

        var testAttacher = (TestAttacher)contextServices.GetRequiredService<IEntityGraphAttacher>();

        Assert.Same(wife, testAttacher.Attached.Item1.Entity);
        Assert.Equal(EntityState.Added, testAttacher.Attached.Item2);
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void Detects_change_of_nullable_collection_navigation_between_null_and_non_null(bool startNull, bool withChild)
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent
        {
            Id = 1,
            Children = startNull ? null : withChild ? [new NullableCollectionChild { Id = 2 }] : []
        };
        context.Attach(parent);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);

        parent.Children = startNull ? [] : null;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Ignores_replacement_of_nullable_collection_navigation_by_another_non_null_collection()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);

        parent.Children = [new NullableCollectionChild { Id = 2 }];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Ignores_change_from_empty_to_null_of_collection_navigation_that_is_not_nullable()
    {
        using var context = new NotNullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Reverting_a_nullness_change_before_saving_keeps_the_modification_like_for_a_property()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, context.Entry(parent).State);

        parent.Children = [];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Unmodifying_a_nullable_collection_navigation_through_the_internal_entry_reverts_and_is_not_detected_again(
        bool detectChanges)
    {
        using var context = new NullableCollectionNavigationContext();
        var children = new List<NullableCollectionChild>();
        var parent = new NullableCollectionParent { Id = 1, Children = children };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, context.Entry(parent).State);

        var entry = context.Entry(parent);
        entry.GetInfrastructure().SetPropertyModified(
            (INavigation)entry.Metadata.FindNavigation(nameof(NullableCollectionParent.Children))!, false);
        if (detectChanges)
        {
            context.ChangeTracker.DetectChanges();
        }

        Assert.Same(children, parent.Children);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Unmodifying_a_nullable_collection_navigation_can_accept_the_current_value_instead_of_reverting()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        var entry = context.Entry(parent);
        entry.GetInfrastructure().SetPropertyModified(
            (INavigation)entry.Metadata.FindNavigation(nameof(NullableCollectionParent.Children))!, false, acceptChanges: true);
        context.ChangeTracker.DetectChanges();

        Assert.Null(parent.Children);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Unmodifying_an_unrelated_property_does_not_revert_a_nullness_change()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Name = "A", Children = [] };
        context.Attach(parent);

        parent.Name = "B";
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Modified, context.Entry(parent).State);

        context.Entry(parent).Property(p => p.Name).IsModified = false;

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Detecting_nullness_changes_repeatedly_keeps_the_same_result()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Accepting_a_nullness_change_resets_the_baseline(bool save)
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        if (save)
        {
            context.Add(parent);
            context.SaveChanges();
            Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        }
        else
        {
            context.Attach(parent);
        }

        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        if (save)
        {
            context.SaveChanges();
        }
        else
        {
            context.ChangeTracker.AcceptAllChanges();
        }

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));

        context.ChangeTracker.DetectChanges();
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);

        parent.Children = [];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Setting_state_to_Modified_flags_a_nullable_collection_navigation(bool empty)
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = empty ? [] : null };
        context.Attach(parent);
        Assert.False(IsChildrenModified(context, parent));

        context.Entry(parent).State = EntityState.Modified;

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void Update_flags_a_collection_navigation_of_a_disconnected_entity_only_if_nullable(bool nullable)
    {
        using DbContext context = nullable
            ? new NullableCollectionNavigationContext()
            : new NotNullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = null };

        context.Update(parent);

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.Equal(nullable, IsChildrenModified(context, parent));
    }

    [Fact]
    public void Flag_of_a_nullable_collection_navigation_set_by_an_explicit_Modified_state_survives_DetectChanges()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = null };
        context.Update(parent);

        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(EntityState.Unchanged)]
    [InlineData(EntityState.Added)]
    public void Setting_state_from_Modified_clears_the_flag_of_a_nullable_collection_navigation(EntityState state)
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = null };
        context.Update(parent);
        Assert.True(IsChildrenModified(context, parent));

        context.Entry(parent).State = state;

        Assert.Equal(state, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
        Assert.False(context.Entry(parent).Collection(p => p.Children!).IsModified);
    }

    [Fact]
    public void Saving_an_explicitly_modified_entity_clears_the_flag_of_a_nullable_collection_navigation()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Add(parent);
        context.SaveChanges();

        context.Entry(parent).State = EntityState.Modified;
        Assert.True(IsChildrenModified(context, parent));

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Setting_the_original_value_of_a_nullable_collection_navigation_to_null_detects_a_nullness_change(bool startNull)
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = startNull ? null : [] };
        context.Attach(parent);
        Assert.False(IsChildrenModified(context, parent));

        var entry = context.Entry(parent);
        entry.GetInfrastructure().SetOriginalValue(entry.Metadata.FindNavigation(nameof(NullableCollectionParent.Children))!, null);

        // Setting the original value to match the current value is not a change.
        Assert.Equal(startNull ? EntityState.Unchanged : EntityState.Modified, context.Entry(parent).State);
        Assert.Equal(!startNull, IsChildrenModified(context, parent));
    }

    [Fact]
    public void Setting_the_original_value_of_a_nullable_collection_navigation_does_not_unmodify_a_detected_change()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        var entry = context.Entry(parent);
        entry.GetInfrastructure().SetOriginalValue(entry.Metadata.FindNavigation(nameof(NullableCollectionParent.Children))!, null);

        // Like for properties, changing the original value does not unmodify what was already modified.
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Setting_state_to_Unchanged_after_a_detected_nullness_change_is_not_detected_again()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [new NullableCollectionChild { Id = 2 }] };
        context.Attach(parent);
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        context.Entry(parent).State = EntityState.Unchanged;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Original_value_of_a_nullable_collection_navigation_is_not_a_copy_of_the_collection()
    {
        using var context = new NullableCollectionNavigationContext();
        var children = new List<NullableCollectionChild> { new() { Id = 2 } };
        var parent = new NullableCollectionParent { Id = 1, Children = children };
        context.Attach(parent);

        var entry = context.Entry(parent);
        var original = entry.GetInfrastructure().GetOriginalValue(
            entry.Metadata.FindNavigation(nameof(NullableCollectionParent.Children))!);

        Assert.Same(children, original);
    }

    [Fact]
    public void Detects_nullness_change_of_a_navigation_declared_on_a_base_type_for_a_derived_entity()
    {
        using var context = new DerivedNullableCollectionNavigationContext();
        var parent = new DerivedNullableCollectionParent { Id = 1, Children = [], Extra = "X" };
        context.Attach(parent);
        Assert.False(IsChildrenModified(context, parent));

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Nullness_change_of_a_derived_entity_does_not_affect_its_other_properties()
    {
        using var context = new DerivedNullableCollectionNavigationContext();
        var parent = new DerivedNullableCollectionParent { Id = 1, Children = [], Extra = "X" };
        context.Attach(parent);

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        var entry = context.Entry(parent);
        Assert.False(entry.Property(p => p.Extra).IsModified);
        Assert.False(entry.Property(p => p.Name).IsModified);
    }

    [Fact]
    public void Changing_the_nullness_of_an_Added_entity_does_not_flag_the_navigation()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = null };
        context.Add(parent);

        parent.Children = [];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Added, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Deleting_an_entity_with_a_detected_nullness_change_saves_without_error()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Add(parent);
        context.SaveChanges();
        parent.Children = null;
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        context.Remove(parent);
        context.SaveChanges();

        Assert.Equal(EntityState.Detached, context.Entry(parent).State);
    }

    [Fact]
    public void Ignores_nullness_change_on_a_Deleted_entity()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);
        context.Entry(parent).State = EntityState.Deleted;

        parent.Children = null;
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Deleted, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Pending_change_from_null_to_empty_is_kept_by_a_second_DetectChanges()
    {
        using var context = new NullableCollectionNavigationContext();
        var parent = new NullableCollectionParent { Id = 1, Children = null };
        context.Attach(parent);
        parent.Children = [];
        context.ChangeTracker.DetectChanges();
        Assert.True(IsChildrenModified(context, parent));

        parent.Children = [new NullableCollectionChild { Id = 5 }];
        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications, false)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications, false)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues, false)]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications, true)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications, true)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues, true)]
    public void Handles_notification_of_nullable_collection_navigation_change_between_null_and_empty(
        ChangeTrackingStrategy strategy,
        bool startNull)
    {
        using var context = NotifyingNullableCollectionContext.Create(strategy);
        var parent = new NotifyingNullableCollectionParent { Id = 1, Children = startNull ? null : [] };
        context.Attach(parent);
        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);

        parent.Children = startNull ? [] : null;

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Fact]
    public void Handles_notification_of_replacing_a_nullable_collection_navigation_with_another_collection_as_no_nullness_change()
    {
        using var context = NotifyingNullableCollectionContext.Create(ChangeTrackingStrategy.ChangedNotifications);
        var parent = new NotifyingNullableCollectionParent { Id = 1, Children = [] };
        context.Attach(parent);

        parent.Children = [new NotifyingNullableCollectionChild { Id = 2 }];

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications, true)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications, true)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues, true)]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications, false)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications, false)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues, false)]
    public void Brings_in_elements_on_notification_of_a_populated_collection_assigned_to_a_nullable_collection_navigation(
        ChangeTrackingStrategy strategy,
        bool startNull)
    {
        using var context = NotifyingNullableCollectionContext.Create(strategy);
        var tracked = new NotifyingNullableCollectionChild { Id = 100 };
        var parent = new NotifyingNullableCollectionParent { Id = 1, Children = startNull ? null : [tracked] };
        context.Attach(parent);
        if (startNull)
        {
            context.Attach(tracked);
        }

        // The keys are generated, so these are tracked as new
        var child = new NotifyingNullableCollectionChild();
        parent.Children = [tracked, child];

        Assert.Equal(EntityState.Added, context.Entry(child).State);
        Assert.Equal(startNull ? EntityState.Modified : EntityState.Unchanged, context.Entry(tracked).State);
        Assert.Equal(1, context.Entry(child).Property<int?>("NotifyingNullableCollectionParentId").CurrentValue);
        Assert.Equal(1, context.Entry(tracked).Property<int?>("NotifyingNullableCollectionParentId").CurrentValue);
        // Without original values, any assignment marks the navigation as modified
        Assert.Equal(
            startNull || strategy == ChangeTrackingStrategy.ChangingAndChangedNotifications,
            IsChildrenModified(context, parent));

        // Adding to the assigned collection is also tracked
        var another = new NotifyingNullableCollectionChild();
        parent.Children!.Add(another);
        Assert.Equal(EntityState.Added, context.Entry(another).State);
    }

    [Theory]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
    public void Update_flags_a_notifying_nullable_collection_navigation_of_a_disconnected_entity(ChangeTrackingStrategy strategy)
    {
        using var context = NotifyingNullableCollectionContext.Create(strategy);
        var parent = new NotifyingNullableCollectionParent { Id = 1, Children = null };

        context.Update(parent);

        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    [Theory]
    [InlineData(ChangeTrackingStrategy.ChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotifications)]
    [InlineData(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)]
    public void Saving_a_notifying_nullness_change_resets_the_baseline(ChangeTrackingStrategy strategy)
    {
        using var context = NotifyingNullableCollectionContext.Create(strategy);
        var parent = new NotifyingNullableCollectionParent { Id = 1, Children = [] };
        context.Add(parent);
        context.SaveChanges();

        parent.Children = null;
        Assert.True(IsChildrenModified(context, parent));
        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, context.Entry(parent).State);
        Assert.False(IsChildrenModified(context, parent));

        parent.Children = [];
        Assert.Equal(EntityState.Modified, context.Entry(parent).State);
        Assert.True(IsChildrenModified(context, parent));
    }

    private static bool IsChildrenModified(DbContext context, object parent)
    {
        var entry = context.Entry(parent);
        return ((IUpdateEntry)entry.GetInfrastructure()).IsModified(
            (INavigation)entry.Metadata.FindNavigation("Children")!);
    }

    private class NullableCollectionParent
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public List<NullableCollectionChild>? Children { get; set; }
    }

    private class DerivedNullableCollectionParent : NullableCollectionParent
    {
        public string? Extra { get; set; }
    }

    private class NullableCollectionChild
    {
        public int Id { get; set; }
    }

    private class NullableCollectionNavigationContext : NullableCollectionContext
    {
        protected override bool Nullable
            => true;
    }

    private class NotNullableCollectionNavigationContext : NullableCollectionContext
    {
        protected override bool Nullable
            => false;
    }

    private class DerivedNullableCollectionNavigationContext : NullableCollectionNavigationContext
    {
        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<DerivedNullableCollectionParent>();
        }
    }

    private abstract class NullableCollectionContext : DbContext
    {
        protected NullableCollectionContext()
            // Entry() and SaveChanges() would otherwise run DetectChanges implicitly and mask what these tests observe.
            => ChangeTracker.AutoDetectChangesEnabled = false;

        protected abstract bool Nullable { get; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                // The in-memory provider rejects nullable collection navigations, but change tracking is what is tested here.
                .ReplaceService<IModelValidator, ModelValidator>();

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<NullableCollectionParent>().HasMany(p => p.Children).WithOne();

            if (Nullable)
            {
                modelBuilder.Model.FindEntityType(typeof(NullableCollectionParent))!
                    .FindNavigation(nameof(NullableCollectionParent.Children))!.SetIsNullable(true);
            }
        }
    }

    private class NotifyingNullableCollectionParent : NotifyingEntity
    {
        private int _id;
        private ObservableHashSet<NotifyingNullableCollectionChild>? _children;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public ObservableHashSet<NotifyingNullableCollectionChild>? Children
        {
            get => _children;
            set => SetWithNotify(value, ref _children);
        }
    }

    private class NotifyingNullableCollectionChild : NotifyingEntity
    {
        private int _id;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }
    }

    private abstract class NotifyingNullableCollectionContext : DbContext
    {
        // The model is cached per context type, so each strategy needs its own type.
        public static NotifyingNullableCollectionContext Create(ChangeTrackingStrategy strategy)
            => strategy switch
            {
                ChangeTrackingStrategy.ChangedNotifications => new ChangedNotificationsNullableCollectionContext(),
                ChangeTrackingStrategy.ChangingAndChangedNotifications => new ChangingAndChangedNotificationsNullableCollectionContext(),
                ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues =>
                    new ChangingAndChangedNotificationsWithOriginalValuesNullableCollectionContext(),
                _ => throw new ArgumentOutOfRangeException(nameof(strategy))
            };

        protected abstract ChangeTrackingStrategy Strategy { get; }

        protected internal override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            => optionsBuilder
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                // The in-memory provider rejects nullable collection navigations, but change tracking is what is tested here.
                .ReplaceService<IModelValidator, ModelValidator>();

        protected internal override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.HasChangeTrackingStrategy(Strategy);
            modelBuilder.Entity<NotifyingNullableCollectionParent>().HasMany(p => p.Children).WithOne();

            modelBuilder.Model.FindEntityType(typeof(NotifyingNullableCollectionParent))!
                .FindNavigation(nameof(NotifyingNullableCollectionParent.Children))!.SetIsNullable(true);
        }
    }

    private class ChangedNotificationsNullableCollectionContext : NotifyingNullableCollectionContext
    {
        protected override ChangeTrackingStrategy Strategy
            => ChangeTrackingStrategy.ChangedNotifications;
    }

    private class ChangingAndChangedNotificationsNullableCollectionContext : NotifyingNullableCollectionContext
    {
        protected override ChangeTrackingStrategy Strategy
            => ChangeTrackingStrategy.ChangingAndChangedNotifications;
    }

    private class ChangingAndChangedNotificationsWithOriginalValuesNullableCollectionContext : NotifyingNullableCollectionContext
    {
        protected override ChangeTrackingStrategy Strategy
            => ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues;
    }

    private class Category
    {
        public int Id { get; set; }
        public int? PrincipalId { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<Product> Products { get; } = new List<Product>();

        public int TagId { get; set; }
        public CategoryTag Tag { get; set; } = null!;
    }

    private class CategoryTag
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;
    }

    private class Product
    {
        public Guid Id { get; set; }
        public int? DependentId { get; set; }
        public string Name { get; set; } = null!;

        public virtual Category Category { get; set; } = null!;

        public int TagId { get; set; }
        public ProductTag Tag { get; set; } = null!;
    }

    private class ProductTag
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }

    private class Person
    {
        public int Id { get; set; }

        public int HusbandId { get; set; }
        public Person Husband { get; set; } = null!;
        public Person Wife { get; set; } = null!;
    }

    private static IModel BuildModel()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<Product>(b =>
        {
            b.HasOne(e => e.Tag).WithOne(e => e.Product)
                .HasPrincipalKey<Product>(e => e.TagId)
                .HasForeignKey<ProductTag>(e => e.ProductId);
            b.Property(e => e.TagId).ValueGeneratedNever();
        });

        builder.Entity<Category>(b =>
        {
            b.HasMany(e => e.Products).WithOne(e => e.Category)
                .HasForeignKey(e => e.DependentId)
                .HasPrincipalKey(e => e.PrincipalId);
            b.Property(e => e.PrincipalId).ValueGeneratedNever();

            b.HasOne(e => e.Tag).WithOne(e => e.Category)
                .HasForeignKey<CategoryTag>(e => e.CategoryId)
                .HasPrincipalKey<Category>(e => e.TagId);
            b.Property(e => e.TagId).ValueGeneratedNever();
        });

        builder.Entity<Person>()
            .HasOne(e => e.Husband).WithOne(e => e.Wife)
            .HasForeignKey<Person>(e => e.HusbandId);

        return builder.Model.FinalizeModel();
    }

    private class NotifyingCategory : NotifyingEntity
    {
        private int _id;
        private int? _principalId;
        private string _name = null!;
        private int _tagId;
        private NotifyingCategoryTag _tag = null!;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int? PrincipalId
        {
            get => _principalId;
            set => SetWithNotify(value, ref _principalId);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public virtual ICollection<NotifyingProduct> Products { get; } = new ObservableCollection<NotifyingProduct>();

        public int TagId
        {
            get => _tagId;
            set => SetWithNotify(value, ref _tagId);
        }

        public NotifyingCategoryTag Tag
        {
            get => _tag;
            set => SetWithNotify(value, ref _tag);
        }
    }

    private class NotifyingCategoryTag : NotifyingEntity
    {
        private int _id;
        private int _categoryId;
        private NotifyingCategory _category = null!;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int CategoryId
        {
            get => _categoryId;
            set => SetWithNotify(value, ref _categoryId);
        }

        public NotifyingCategory Category
        {
            get => _category;
            set => SetWithNotify(value, ref _category);
        }
    }

    private class NotifyingProduct : NotifyingEntity
    {
        private Guid _id;
        private int? _dependentId;
        private string _name = null!;
        private NotifyingCategory _category = null!;
        private int _tagId;
        private NotifyingProductTag _tag = null!;

        public Guid Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int? DependentId
        {
            get => _dependentId;
            set => SetWithNotify(value, ref _dependentId);
        }

        public string Name
        {
            get => _name;
            set => SetWithNotify(value, ref _name);
        }

        public NotifyingCategory Category
        {
            get => _category;
            set => SetWithNotify(value, ref _category);
        }

        public int TagId
        {
            get => _tagId;
            set => SetWithNotify(value, ref _tagId);
        }

        public NotifyingProductTag Tag
        {
            get => _tag;
            set => SetWithNotify(value, ref _tag);
        }
    }

    private class NotifyingProductTag : NotifyingEntity
    {
        private int _id;
        private int _productId;
        private NotifyingProduct _product = null!;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int ProductId
        {
            get => _productId;
            set => SetWithNotify(value, ref _productId);
        }

        public NotifyingProduct Product
        {
            get => _product;
            set => SetWithNotify(value, ref _product);
        }
    }

    private class NotifyingPerson : NotifyingEntity
    {
        private int _id;
        private int _husbandId;
        private NotifyingPerson _husband = null!;
        private NotifyingPerson _wife = null!;

        public int Id
        {
            get => _id;
            set => SetWithNotify(value, ref _id);
        }

        public int HusbandId
        {
            get => _husbandId;
            set => SetWithNotify(value, ref _husbandId);
        }

        public NotifyingPerson Husband
        {
            get => _husband;
            set => SetWithNotify(value, ref _husband);
        }

        public NotifyingPerson Wife
        {
            get => _wife;
            set => SetWithNotify(value, ref _wife);
        }
    }

    private class NotifyingEntity : INotifyPropertyChanging, INotifyPropertyChanged
    {
        protected void SetWithNotify<T>(T value, ref T field, [CallerMemberName] string propertyName = "")
        {
            // Intentionally not checking if new value is different for robustness of handler code
            NotifyChanging(propertyName);
            field = value;
            NotifyChanged(propertyName);
        }

        public event PropertyChangingEventHandler? PropertyChanging;
        public event PropertyChangedEventHandler? PropertyChanged;

        private void NotifyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void NotifyChanging(string propertyName)
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    private static IModel BuildNotifyingModel()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder()
            .HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

        builder.Entity<NotifyingProduct>(b =>
        {
            b.HasOne(e => e.Tag).WithOne(e => e.Product)
                .HasPrincipalKey<NotifyingProduct>(e => e.TagId)
                .HasForeignKey<NotifyingProductTag>(e => e.ProductId);
            b.Property(e => e.TagId).ValueGeneratedNever();
        });

        builder.Entity<NotifyingCategory>(b =>
        {
            b.HasMany(e => e.Products).WithOne(e => e.Category)
                .HasForeignKey(e => e.DependentId)
                .HasPrincipalKey(e => e.PrincipalId);
            b.Property(e => e.PrincipalId).ValueGeneratedNever();

            b.HasOne(e => e.Tag).WithOne(e => e.Category)
                .HasForeignKey<NotifyingCategoryTag>(e => e.CategoryId)
                .HasPrincipalKey<NotifyingCategory>(e => e.TagId);
            b.Property(e => e.TagId).ValueGeneratedNever();
        });

        builder.Entity<NotifyingPerson>()
            .HasOne(e => e.Husband).WithOne(e => e.Wife)
            .HasForeignKey<NotifyingPerson>(e => e.HusbandId);

        return builder.Model.FinalizeModel();
    }

    private class CategoryWithChanged : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public virtual ICollection<ProductWithChanged> Products { get; set; } = new ObservableCollection<ProductWithChanged>();

        // Actual implementation not needed for tests
#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }

    private class ProductWithChanged : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int? DependentId { get; set; }
        public string Name { get; set; } = null!;

        public virtual CategoryWithChanged Category { get; set; } = null!;

        // Actual implementation not needed for tests
#pragma warning disable 67
        public event PropertyChangedEventHandler? PropertyChanged;
#pragma warning restore 67
    }

    private static IModel BuildModelWithChanged()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder()
            .HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangedNotifications);

        builder.Entity<ProductWithChanged>();
        builder.Entity<CategoryWithChanged>()
            .HasMany(e => e.Products).WithOne(e => e.Category)
            .HasForeignKey(e => e.DependentId);

        return builder.Model.FinalizeModel();
    }

    private static InternalEntityEntry CreateInternalEntry<TEntity>(
        IServiceProvider contextServices,
        TEntity? entity = null)
        where TEntity : class, new()
        => contextServices.GetRequiredService<IStateManager>()
            .GetOrCreateEntry(
                entity ?? new TEntity(),
                contextServices.GetRequiredService<IModel>().FindEntityType(typeof(TEntity))!);

    private static IServiceProvider CreateContextServices(IModel? model = null)
        => InMemoryTestHelpers.Instance.CreateContextServices(
            new ServiceCollection()
                .AddScoped<TestRelationshipListener>()
                .AddScoped<IEntityGraphAttacher, TestAttacher>()
                .AddScoped<INavigationFixer>(p => p.GetRequiredService<TestRelationshipListener>()),
            model ?? BuildModel());

    private class TestAttacher(IEntityEntryGraphIterator graphIterator) : EntityGraphAttacher(graphIterator)
    {
        public Tuple<InternalEntityEntry, EntityState> Attached { get; set; } = null!;

        public override void AttachGraph(
            InternalEntityEntry rootEntry,
            EntityState targetState,
            EntityState storeGeneratedWithKeySetTargetState,
            bool forceStateWhenUnknownKey)
        {
            Attached = Tuple.Create(rootEntry, targetState);

            base.AttachGraph(rootEntry, targetState, storeGeneratedWithKeySetTargetState, forceStateWhenUnknownKey);
        }
    }

    private class TestRelationshipListener(IEntityGraphAttacher attacher) : NavigationFixer(
        attacher, new StructuralTypeMaterializerSource(
            new StructuralTypeMaterializerSourceDependencies([])))
    {
        public Tuple<InternalEntityEntry, IProperty, IEnumerable<IKey>, IEnumerable<IForeignKey>, object?, object?> KeyChange
        {
            get;
            set;
        } = null!;

        public Tuple<InternalEntityEntry, INavigationBase, object?, object?> ReferenceChange { get; set; } = null!;
        public Tuple<InternalEntityEntry, INavigationBase, IEnumerable<object>, IEnumerable<object>> CollectionChange { get; set; } = null!;

        public override void NavigationReferenceChanged(
            InternalEntityEntry entry,
            INavigationBase navigationBase,
            object? oldValue,
            object? newValue)
        {
            ReferenceChange = Tuple.Create(entry, navigationBase, oldValue, newValue);

            base.NavigationReferenceChanged(entry, navigationBase, oldValue, newValue);
        }

        public override void NavigationCollectionChanged(
            InternalEntityEntry entry,
            INavigationBase navigation,
            IEnumerable<object> added,
            IEnumerable<object> removed)
        {
            // ReSharper disable PossibleMultipleEnumeration
            CollectionChange = Tuple.Create(entry, navigation, added, removed);

            base.NavigationCollectionChanged(entry, navigation, added, removed);
            // ReSharper restore PossibleMultipleEnumeration
        }

        public override void KeyPropertyChanged(
            InternalEntityEntry entry,
            IProperty property,
            IEnumerable<IKey> containingPrincipalKeys,
            IEnumerable<IForeignKey> containingForeignKeys,
            object? oldValue,
            object? newValue)
        {
            KeyChange = Tuple.Create(entry, property, containingPrincipalKeys, containingForeignKeys, oldValue, newValue);

            base.KeyPropertyChanged(entry, property, containingPrincipalKeys, containingForeignKeys, oldValue, newValue);
        }
    }
}

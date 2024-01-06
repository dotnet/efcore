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
    [ConditionalFact]
    public void PropertyChanging_does_not_snapshot_if_eager_snapshots_are_in_use()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildModel());
        var entry = CreateInternalEntry<Product>(contextServices);

        Assert.True(entry.EntityType.UseEagerSnapshots());
        Assert.False(entry.HasRelationshipSnapshot);

        contextServices
            .GetRequiredService<IChangeDetector>()
            .PropertyChanging(entry, entry.EntityType.FindProperty("DependentId"));

        Assert.False(entry.HasRelationshipSnapshot);
    }

    [ConditionalFact]
    public void PropertyChanging_snapshots_original_and_FK_value_if_lazy_snapshots_are_in_use()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildNotifyingModel());
        var entity = new NotifyingProduct { DependentId = 77 };
        var entry = CreateInternalEntry(contextServices, entity);
        entry.SetEntityState(EntityState.Unchanged);

        Assert.False(entry.EntityType.UseEagerSnapshots());
        Assert.False(entry.HasRelationshipSnapshot);

        var property = entry.EntityType.FindProperty("DependentId");

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

    [ConditionalFact]
    public void PropertyChanging_does_not_snapshot_original_values_for_properties_with_no_original_value_tracking()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildNotifyingModel());
        var entity = new NotifyingProduct { Name = "Cheese" };
        var entry = CreateInternalEntry(contextServices, entity);

        Assert.False(entry.EntityType.UseEagerSnapshots());

        var property = entry.EntityType.FindProperty("Name");

        contextServices
            .GetRequiredService<IChangeDetector>()
            .PropertyChanging(entry, property);

        Assert.Equal("Cheese", entry.GetRelationshipSnapshotValue(property));
        Assert.Equal("Cheese", entry.GetCurrentValue(property));

        entity.Name = "Pickle";

        Assert.Equal("Pickle", entry.GetRelationshipSnapshotValue(property));
        Assert.Equal("Pickle", entry.GetCurrentValue(property));
    }

    [ConditionalFact]
    public void PropertyChanging_snapshots_reference_navigations_if_lazy_snapshots_are_in_use()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildNotifyingModel());
        var category = new NotifyingCategory();
        var entity = new NotifyingProduct { Category = category };
        var entry = CreateInternalEntry(contextServices, entity);
        entry.SetEntityState(EntityState.Added);

        Assert.False(entry.EntityType.UseEagerSnapshots());
        Assert.False(entry.HasRelationshipSnapshot);

        var navigation = entry.EntityType.FindNavigation("Category");

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

    [ConditionalFact]
    public void PropertyChanging_snapshots_PK_for_relationships_if_lazy_snapshots_are_in_use()
    {
        var contextServices = InMemoryTestHelpers.Instance.CreateContextServices(BuildNotifyingModel());
        var id = Guid.NewGuid();
        var entity = new NotifyingProduct { Id = id };
        var entry = CreateInternalEntry(contextServices, entity);
        entry.SetEntityState(EntityState.Added);

        Assert.False(entry.EntityType.UseEagerSnapshots());
        Assert.False(entry.HasRelationshipSnapshot);

        var property = entry.EntityType.FindProperty("Id");

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

    [ConditionalFact]
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
        Assert.True(entry.IsModified(entry.EntityType.FindProperty("Name")));
    }

    [ConditionalTheory]
    [InlineData(true, true, true)]
    [InlineData(false, true, true)]
    [InlineData(true, false, true)]
    [InlineData(false, false, true)]
    [InlineData(true, true, false)]
    [InlineData(false, true, false)]
    [InlineData(true, false, false)]
    [InlineData(false, false, false)]
    public void Can_insert_with_array_comparer(bool useTypeMapping, bool useStateChange, bool nullValue)
    {
        using var context = useTypeMapping ? new BaxterWithMappingContext() : new BaxterContext();
        var value = nullValue ? null : new[] { 1, 2, 3, 4 };

        var baxter = new Baxter { Id = Guid.NewGuid(), Demands = value };

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
            baxter.Demands[1] = 767;
        }

        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entityEntry.State);
        Assert.Equal(new[] { 1, 767, 3, 4 }, entityEntry.Property(e => e.Demands).CurrentValue);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Detects_scalar_property_change_with_custom_comparer(bool useTypeMapping)
    {
        using var context = useTypeMapping ? new BaxterWithMappingContext() : new BaxterContext();
        var baxter = context.Attach(
            new Baxter { Id = Guid.NewGuid(), Demands = [1, 2, 3, 4] }).Entity;

        baxter.Demands[2] = 33;

        var entityEntry = context.Entry(baxter);
        AssertDetected(entityEntry, entityEntry.Property(e => e.Demands));

        context.SaveChanges();

        Assert.Equal(EntityState.Unchanged, entityEntry.State);

        baxter.Demands[1] = 767;

        context.ChangeTracker.DetectChanges();

        Assert.Equal(EntityState.Modified, entityEntry.State);
        Assert.Equal(new[] { 1, 767, 3, 4 }, entityEntry.Property(e => e.Demands).CurrentValue);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
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
        public int[] Demands { get; set; }
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
            ValueConverter converter,
            ValueComparer comparer = null,
            ValueComparer keyComparer = null,
            CoreTypeMapping elementMapping = null,
            JsonValueReaderWriter jsonValueReaderWriter = null)
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
                v => v == null ? null : v.ToArray());

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

    [ConditionalFact]
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
        Assert.False(entry.IsModified(entry.EntityType.FindProperty("Name")));
    }

    [ConditionalFact]
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
        Assert.False(entry.IsModified(entry.EntityType.FindProperty("Name")));
    }

    [ConditionalFact]
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

        Assert.Equal(78, entry.GetRelationshipSnapshotValue(entry.EntityType.FindProperty("PrincipalId")));

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("PrincipalId"), testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(77, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindProperty("PrincipalId"), testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(78, testListener.KeyChange.Item5);
        Assert.Equal(77, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
    public void Reacts_to_principal_key_change_in_sidecar()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new Category { Id = -1, PrincipalId = 77 };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);

        var property = entry.EntityType.FindProperty("PrincipalId");
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

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindProperty("Id"), testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(-1, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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

        var property = entry.EntityType.FindProperty("Id");
        entry.PrepareToSave();

        entry[property] = 78;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.KeyChange.Item1);
        Assert.Same(entry.EntityType.FindProperty("Id"), testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(-1, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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

    [ConditionalFact]
    public void Ignores_no_change_to_principal_key_in_sidecar()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var category = new Category { Id = -1, PrincipalId = 77 };
        var entry = stateManager.GetOrCreateEntry(category);
        entry.SetEntityState(EntityState.Added);
        entry.PrepareToSave();

        var property = entry.EntityType.FindProperty("PrincipalId");

        entry[property] = 77;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindProperty("DependentId"), testListener.KeyChange.Item2);
        Assert.Empty(testListener.KeyChange.Item3);
        Assert.Single(testListener.KeyChange.Item4);
        Assert.Equal(77, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindProperty("DependentId"), testListener.KeyChange.Item2);
        Assert.Empty(testListener.KeyChange.Item3);
        Assert.Single(testListener.KeyChange.Item4);
        Assert.Equal(78, testListener.KeyChange.Item5);
        Assert.Equal(77, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
    public void Reacts_to_foreign_key_change_in_sidecar()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);
        entry.PrepareToSave();

        var property = entry.EntityType.FindProperty("DependentId");
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

    [ConditionalFact]
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

    [ConditionalFact]
    public void Ignores_no_change_to_foreign_key_in_sidecar()
    {
        var contextServices = CreateContextServices();

        var changeDetector = contextServices.GetRequiredService<IChangeDetector>();
        var stateManager = contextServices.GetRequiredService<IStateManager>();

        var product = new Product { Id = Guid.NewGuid(), DependentId = 77 };
        var entry = stateManager.GetOrCreateEntry(product);
        entry.SetEntityState(EntityState.Unchanged);
        entry.PrepareToSave();

        var property = entry.EntityType.FindProperty("DependentId");
        entry[property] = 77;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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
        product.Category = newCategory;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.ReferenceChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Category"), testListener.ReferenceChange.Item2);
        Assert.Equal(originalCategory, testListener.ReferenceChange.Item3);
        Assert.Equal(newCategory, testListener.ReferenceChange.Item4);

        Assert.Null(testListener.CollectionChange);

        Assert.Equal("DependentId", testListener.KeyChange.Item2.Name);
        Assert.Equal(1, testListener.KeyChange.Item5);
        Assert.Equal(2, testListener.KeyChange.Item6);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
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
        product.Category = newCategory;

        changeDetector.DetectChanges(entry);

        product.Category = originalCategory;

        changeDetector.DetectChanges(entry);

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.ReferenceChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Category"), testListener.ReferenceChange.Item2);
        Assert.Equal(newCategory, testListener.ReferenceChange.Item3);
        Assert.Equal(originalCategory, testListener.ReferenceChange.Item4);

        Assert.Equal("DependentId", testListener.KeyChange.Item2.Name);
        Assert.Equal(useNull ? null : 2, testListener.KeyChange.Item5);
        Assert.Equal(1, testListener.KeyChange.Item6);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindNavigation("Products"), testListener.CollectionChange.Item2);
        Assert.Equal(new[] { product3 }, testListener.CollectionChange.Item3);
        Assert.Empty(testListener.CollectionChange.Item4);

        var productEntry = stateManager.GetOrCreateEntry(product3);
        Assert.Same(productEntry, testListener.ReferenceChange.Item1);
        Assert.Same(productEntry.EntityType.FindNavigation("Category"), testListener.ReferenceChange.Item2);
        Assert.Null(testListener.ReferenceChange.Item3);
        Assert.Equal(category, testListener.ReferenceChange.Item4);

        Assert.Null(testListener.KeyChange);
    }

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindNavigation("Products"), testListener.CollectionChange.Item2);
        Assert.Empty(testListener.CollectionChange.Item3);
        Assert.Equal(new[] { product1 }, testListener.CollectionChange.Item4);

        Assert.Null(testListener.KeyChange);
        Assert.Null(testListener.ReferenceChange);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindNavigation("Products"), testListener.CollectionChange.Item2);
        Assert.Equal(new[] { product3 }, testListener.CollectionChange.Item3);
        Assert.Empty(testListener.CollectionChange.Item4);

        var productEntry = stateManager.GetOrCreateEntry(product3);
        Assert.Same(productEntry, testListener.ReferenceChange.Item1);
        Assert.Same(productEntry.EntityType.FindNavigation("Category"), testListener.ReferenceChange.Item2);
        Assert.Null(testListener.ReferenceChange.Item3);
        Assert.Equal(category, testListener.ReferenceChange.Item4);

        Assert.Null(testListener.KeyChange);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindProperty("PrincipalId"), testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(77, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindProperty("PrincipalId"), testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(78, testListener.KeyChange.Item5);
        Assert.Equal(77, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindProperty("Id"), testListener.KeyChange.Item2);
        Assert.Single(testListener.KeyChange.Item3);
        Assert.Empty(testListener.KeyChange.Item4);
        Assert.Equal(-1, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindProperty("DependentId"), testListener.KeyChange.Item2);
        Assert.Empty(testListener.KeyChange.Item3);
        Assert.Single(testListener.KeyChange.Item4);
        Assert.Equal(77, testListener.KeyChange.Item5);
        Assert.Equal(78, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindProperty("DependentId"), testListener.KeyChange.Item2);
        Assert.Empty(testListener.KeyChange.Item3);
        Assert.Single(testListener.KeyChange.Item4);
        Assert.Equal(78, testListener.KeyChange.Item5);
        Assert.Equal(77, testListener.KeyChange.Item6);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.CollectionChange);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindNavigation("Category"), testListener.ReferenceChange.Item2);
        Assert.Equal(originalCategory, testListener.ReferenceChange.Item3);
        Assert.Equal(newCategory, testListener.ReferenceChange.Item4);

        Assert.Equal("DependentId", testListener.KeyChange.Item2.Name);
        Assert.Equal(1, testListener.KeyChange.Item5);
        Assert.Equal(2, testListener.KeyChange.Item6);
    }

    [ConditionalTheory]
    [InlineData(false)]
    [InlineData(true)]
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

        product.Category = newCategory;
        product.Category = originalCategory;

        var testListener = contextServices.GetRequiredService<TestRelationshipListener>();

        Assert.Same(entry, testListener.ReferenceChange.Item1);
        Assert.Same(entry.EntityType.FindNavigation("Category"), testListener.ReferenceChange.Item2);
        Assert.Equal(newCategory, testListener.ReferenceChange.Item3);
        Assert.Equal(originalCategory, testListener.ReferenceChange.Item4);

        Assert.Equal("DependentId", testListener.KeyChange.Item2.Name);
        Assert.Equal(useNull ? null : 2, testListener.KeyChange.Item5);
        Assert.Equal(1, testListener.KeyChange.Item6);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindNavigation("Products"), testListener.CollectionChange.Item2);
        Assert.Equal(new[] { product3 }, testListener.CollectionChange.Item3);
        Assert.Empty(testListener.CollectionChange.Item4);

        var productEntry = stateManager.GetOrCreateEntry(product3);
        Assert.Same(productEntry, testListener.ReferenceChange.Item1);
        Assert.Same(productEntry.EntityType.FindNavigation("Category"), testListener.ReferenceChange.Item2);
        Assert.Null(testListener.ReferenceChange.Item3);
        Assert.Equal(category, testListener.ReferenceChange.Item4);

        Assert.Null(testListener.KeyChange);
    }

    [ConditionalFact]
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
        Assert.Same(entry.EntityType.FindNavigation("Products"), testListener.CollectionChange.Item2);
        Assert.Empty(testListener.CollectionChange.Item3);
        Assert.Equal(new[] { product1 }, testListener.CollectionChange.Item4);

        Assert.Null(testListener.ReferenceChange);
        Assert.Null(testListener.KeyChange);
    }

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    [ConditionalFact]
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

    private class Category
    {
        public int Id { get; set; }
        public int? PrincipalId { get; set; }
        public string Name { get; set; }

        public virtual ICollection<Product> Products { get; } = new List<Product>();

        public int TagId { get; set; }
        public CategoryTag Tag { get; set; }
    }

    private class CategoryTag
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; }
    }

    private class Product
    {
        public Guid Id { get; set; }
        public int? DependentId { get; set; }
        public string Name { get; set; }

        public virtual Category Category { get; set; }

        public int TagId { get; set; }
        public ProductTag Tag { get; set; }
    }

    private class ProductTag
    {
        public int Id { get; set; }

        public int ProductId { get; set; }
        public Product Product { get; set; }
    }

    private class Person
    {
        public int Id { get; set; }

        public int HusbandId { get; set; }
        public Person Husband { get; set; }
        public Person Wife { get; set; }
    }

    private static IModel BuildModel()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<Product>(
            b =>
            {
                b.HasOne(e => e.Tag).WithOne(e => e.Product)
                    .HasPrincipalKey<Product>(e => e.TagId)
                    .HasForeignKey<ProductTag>(e => e.ProductId);
                b.Property(e => e.TagId).ValueGeneratedNever();
            });

        builder.Entity<Category>(
            b =>
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
        private string _name;
        private int _tagId;
        private NotifyingCategoryTag _tag;

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
        private NotifyingCategory _category;

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
        private string _name;
        private NotifyingCategory _category;
        private int _tagId;
        private NotifyingProductTag _tag;

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
        private NotifyingProduct _product;

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
        private NotifyingPerson _husband;
        private NotifyingPerson _wife;

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

        public event PropertyChangingEventHandler PropertyChanging;
        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyChanged(string propertyName)
            => PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        private void NotifyChanging(string propertyName)
            => PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(propertyName));
    }

    private static IModel BuildNotifyingModel()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder()
            .HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);

        builder.Entity<NotifyingProduct>(
            b =>
            {
                b.HasOne(e => e.Tag).WithOne(e => e.Product)
                    .HasPrincipalKey<NotifyingProduct>(e => e.TagId)
                    .HasForeignKey<NotifyingProductTag>(e => e.ProductId);
                b.Property(e => e.TagId).ValueGeneratedNever();
            });

        builder.Entity<NotifyingCategory>(
            b =>
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
        public string Name { get; set; }

        public virtual ICollection<ProductWithChanged> Products { get; set; } = new ObservableCollection<ProductWithChanged>();

        // Actual implementation not needed for tests
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
#pragma warning restore 67
    }

    private class ProductWithChanged : INotifyPropertyChanged
    {
        public int Id { get; set; }
        public int? DependentId { get; set; }
        public string Name { get; set; }

        public virtual CategoryWithChanged Category { get; set; }

        // Actual implementation not needed for tests
#pragma warning disable 67
        public event PropertyChangedEventHandler PropertyChanged;
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
        TEntity entity = null)
        where TEntity : class, new()
        => contextServices.GetRequiredService<IStateManager>()
            .GetOrCreateEntry(
                entity ?? new TEntity(),
                contextServices.GetRequiredService<IModel>().FindEntityType(typeof(TEntity)));

    private static IServiceProvider CreateContextServices(IModel model = null)
        => InMemoryTestHelpers.Instance.CreateContextServices(
            new ServiceCollection()
                .AddScoped<TestRelationshipListener>()
                .AddScoped<IEntityGraphAttacher, TestAttacher>()
                .AddScoped<INavigationFixer>(p => p.GetRequiredService<TestRelationshipListener>()),
            model ?? BuildModel());

    private class TestAttacher(IEntityEntryGraphIterator graphIterator) : EntityGraphAttacher(graphIterator)
    {
        public Tuple<InternalEntityEntry, EntityState> Attached { get; set; }

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
            attacher, new EntityMaterializerSource(
                    new EntityMaterializerSourceDependencies(Enumerable.Empty<ISingletonInterceptor>())))
    {
        public Tuple<InternalEntityEntry, IProperty, IEnumerable<IKey>, IEnumerable<IForeignKey>, object, object> KeyChange
        {
            get;
            set;
        }

        public Tuple<InternalEntityEntry, INavigationBase, object, object> ReferenceChange { get; set; }
        public Tuple<InternalEntityEntry, INavigationBase, IEnumerable<object>, IEnumerable<object>> CollectionChange { get; set; }

        public override void NavigationReferenceChanged(
            InternalEntityEntry entry,
            INavigationBase navigationBase,
            object oldValue,
            object newValue)
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
            object oldValue,
            object newValue)
        {
            KeyChange = Tuple.Create(entry, property, containingPrincipalKeys, containingForeignKeys, oldValue, newValue);

            base.KeyPropertyChanged(entry, property, containingPrincipalKeys, containingForeignKeys, oldValue, newValue);
        }
    }
}

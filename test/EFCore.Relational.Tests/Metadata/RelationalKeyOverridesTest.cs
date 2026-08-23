// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata;

public class RelationalKeyOverridesTest
{
    [Fact]
    public void Key_overrides_are_stored_per_store_object_with_configuration_sources()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Customer>().ToTable("Customers");

        var key = (IMutableKey)modelBuilder.Model.FindEntityType(typeof(Customer))!.FindPrimaryKey()!;
        var customers = StoreObjectIdentifier.Table("Customers");
        var details = StoreObjectIdentifier.Table("CustomerDetails");

        Assert.Null(RelationalKeyOverrides.Find(key, customers));

        var overrides = RelationalKeyOverrides.GetOrCreate(key, customers, ConfigurationSource.Explicit);
        overrides.SetName("pk_customers", ConfigurationSource.Explicit);

        Assert.Equal("pk_customers", RelationalKeyOverrides.Find(key, customers)!.Name);
        Assert.True(RelationalKeyOverrides.Find(key, customers)!.IsNameOverridden);
        Assert.Null(RelationalKeyOverrides.Find(key, details));

        // A convention-source write must not clobber an explicit one. Precedence is enforced by
        // the builder, not by the metadata setter — see the note below this test.
        Assert.Null(overrides.Builder.HasName("pk_convention", ConfigurationSource.Convention));
        Assert.Equal("pk_customers", RelationalKeyOverrides.Find(key, customers)!.Name);

        // And the metadata setter, called directly, does *not* protect the explicit value. Pin
        // that too, so a later reader does not assume a guarantee the precedent never made.
        overrides.SetName("pk_clobbered", ConfigurationSource.Convention);
        Assert.Equal("pk_clobbered", overrides.Name);
        overrides.SetName("pk_customers", ConfigurationSource.Explicit);
    }

    [Fact]
    public void Explicit_null_name_override_is_distinguishable_from_unset()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Customer>().ToTable("Customers");

        var key = (IMutableKey)modelBuilder.Model.FindEntityType(typeof(Customer))!.FindPrimaryKey()!;
        var customers = StoreObjectIdentifier.Table("Customers");

        var overrides = RelationalKeyOverrides.GetOrCreate(key, customers, ConfigurationSource.Explicit);
        overrides.SetName(null, ConfigurationSource.Explicit);

        // Set-to-null means "suppress the name for this store object", not "no override configured".
        Assert.True(overrides.IsNameOverridden);
        Assert.Null(overrides.Name);
    }

    [Fact]
    public void Removing_an_override_detaches_it_from_the_model()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Customer>().ToTable("Customers");

        var key = (IMutableKey)modelBuilder.Model.FindEntityType(typeof(Customer))!.FindPrimaryKey()!;
        var customers = StoreObjectIdentifier.Table("Customers");

        var overrides = RelationalKeyOverrides.GetOrCreate(key, customers, ConfigurationSource.Explicit);
        Assert.True(overrides.IsInModel);

        Assert.Same(overrides, RelationalKeyOverrides.Remove(key, customers));
        Assert.False(overrides.IsInModel);
        Assert.Null(RelationalKeyOverrides.Find(key, customers));
    }

    [Fact]
    public void Key_name_override_beats_the_global_rewritten_name_per_table()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers");
            b.SplitToTable("CustomerDetails", s => s.Property(c => c.Name));
        });

        var key = (IMutableKey)modelBuilder.Model.FindEntityType(typeof(Customer))!.FindPrimaryKey()!;
        var customers = StoreObjectIdentifier.Table("Customers");
        var details = StoreObjectIdentifier.Table("CustomerDetails");

        // The NamingConventions #396 collision shape: one global rewritten name for two tables.
        key.SetName("pk_global_rewrite");
        Assert.Equal("pk_global_rewrite", key.GetName(customers));
        Assert.Equal("pk_global_rewrite", key.GetName(details));

        RelationalKeyOverrides.GetOrCreate(key, customers, ConfigurationSource.Explicit)
            .SetName("pk_customers", ConfigurationSource.Explicit);
        RelationalKeyOverrides.GetOrCreate(key, details, ConfigurationSource.Explicit)
            .SetName("pk_customer_details", ConfigurationSource.Explicit);

        Assert.Equal("pk_customers", key.GetName(customers));
        Assert.Equal("pk_customer_details", key.GetName(details));
    }

    [Fact]
    public void Explicit_null_key_name_override_falls_back_to_the_default_name()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers");
            b.SplitToTable("CustomerDetails", s => s.Property(c => c.Name));
        });

        var key = (IMutableKey)modelBuilder.Model.FindEntityType(typeof(Customer))!.FindPrimaryKey()!;
        var details = StoreObjectIdentifier.Table("CustomerDetails");

        key.SetName("pk_global_rewrite");
        RelationalKeyOverrides.GetOrCreate(key, details, ConfigurationSource.Explicit)
            .SetName(null, ConfigurationSource.Explicit);

        Assert.Equal("PK_CustomerDetails", key.GetName(details));
    }

    [Fact]
    public void Explicit_null_key_name_override_survives_the_in_memory_runtime_model()
    {
        // Spike/B7: RelationalRuntimeModelConvention converts the design-time KeyOverrides
        // annotation into RuntimeRelationalKeyOverrides when the model is initialized for runtime
        // use (IModelRuntimeInitializer.Initialize(..., designTime: false)) -- the path every
        // regular (non-compiled-model) DbContext goes through the first time DbContext.Model is
        // accessed. This is a *different* code path from the NativeAOT compiled-model source
        // generator (RelationalCSharpRuntimeAnnotationCodeGenerator, covered separately by
        // CompiledModelRelationalTestBase), and from the design-time resolution proven by
        // Explicit_null_key_name_override_falls_back_to_the_default_name above -- both must
        // independently preserve IsNameOverridden: true, Name: null rather than collapsing it into
        // "no override at all", or the override silently reverts to the global/default name once
        // the app actually runs.
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers");
            b.SplitToTable("CustomerDetails", s => s.Property(c => c.Name));
            b.HasKey(c => c.Id)
                .HasName("pk_global")
                .HasName(null, StoreObjectIdentifier.Table("CustomerDetails"));
        });

        var modelRuntimeInitializer = FakeRelationalTestHelpers.Instance.CreateContextServices()
            .GetRequiredService<IModelRuntimeInitializer>();
        var runtimeModel = modelRuntimeInitializer.Initialize((IModel)modelBuilder.Model, designTime: false);

        var key = runtimeModel.FindEntityType(typeof(Customer))!.FindPrimaryKey()!;
        var customers = StoreObjectIdentifier.Table("Customers");
        var details = StoreObjectIdentifier.Table("CustomerDetails");

        // Proves the annotation was actually *converted* by RelationalRuntimeModelConvention,
        // rather than the design-time RelationalKeyOverrides object merely being carried through
        // untouched (which the covariant IReadOnlyStoreObjectDictionary<out T> cast would allow --
        // reads through GetName/GetOverrides below would keep working either way, so this type
        // check is the only assertion that actually distinguishes "converted" from "never
        // touched").
        var detailsOverrides = key.GetOverrides().Single(o => o.StoreObject == details);
        Assert.IsType<RuntimeRelationalKeyOverrides>(detailsOverrides);

        // The main table has no override, so it resolves through to the global name.
        Assert.Equal("pk_global", key.GetName(customers));

        // The explicit-null override on the fragment must resolve to the *default* name, not the
        // global "pk_global" annotation -- proving IsNameOverridden: true, Name: null survived
        // the conversion rather than collapsing into "no override at all".
        Assert.Equal(key.GetDefaultName(details), key.GetName(details));
    }

    [Fact]
    public void Per_table_key_names_flow_into_the_relational_model()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<Customer>(b =>
        {
            b.ToTable("Customers");
            b.SplitToTable("CustomerDetails", s => s.Property(c => c.Name));
        });

        var key = (IMutableKey)modelBuilder.Model.FindEntityType(typeof(Customer))!.FindPrimaryKey()!;
        RelationalKeyOverrides.GetOrCreate(key, StoreObjectIdentifier.Table("Customers"), ConfigurationSource.Explicit)
            .SetName("pk_customers", ConfigurationSource.Explicit);
        RelationalKeyOverrides.GetOrCreate(key, StoreObjectIdentifier.Table("CustomerDetails"), ConfigurationSource.Explicit)
            .SetName("pk_customer_details", ConfigurationSource.Explicit);

        var relationalModel = modelBuilder.FinalizeModel().GetRelationalModel();

        Assert.Equal(
            "pk_customers",
            relationalModel.Tables.Single(t => t.Name == "Customers").UniqueConstraints.Single().Name);
        Assert.Equal(
            "pk_customer_details",
            relationalModel.Tables.Single(t => t.Name == "CustomerDetails").UniqueConstraints.Single().Name);
    }

    [Fact]
    public void Key_overrides_survive_key_re_creation_on_base_type_assignment()
    {
        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();

        // Add and fully process the base type first, so its properties (in particular "Id") are
        // discovered before anything tries to re-point a foreign/primary key at it. Adding
        // SpecialCustomer would otherwise auto-link it to Customer via BaseTypeDiscoveryConvention
        // as soon as both types co-exist, racing property discovery on whichever type is added
        // second and silently dropping the reattachment this test means to exercise.
        modelBuilder.Entity<Customer>();

        // Add SpecialCustomer (auto-linked to Customer immediately by BaseTypeDiscoveryConvention,
        // before it has any properties of its own), then explicitly detach it so it becomes a
        // standalone root again and gets its own freshly-discovered primary key to configure an
        // override on.
        modelBuilder.Entity<SpecialCustomer>();
        modelBuilder.Entity<SpecialCustomer>().HasBaseType((Type?)null);
        modelBuilder.Entity<SpecialCustomer>(b =>
        {
            b.ToTable("Customers");
            b.SplitToTable("CustomerDetails", s => s.Property(c => c.Name));
        });

        var derived = modelBuilder.Model.FindEntityType(typeof(SpecialCustomer))!;
        var key = (IMutableKey)derived.FindPrimaryKey()!;
        var customers = StoreObjectIdentifier.Table("Customers");

        RelationalKeyOverrides.GetOrCreate(key, customers, ConfigurationSource.Explicit)
            .SetName("pk_customers", ConfigurationSource.Explicit);

        // Assigning a base type detaches the derived key and re-creates it on the root type.
        modelBuilder.Entity<SpecialCustomer>().HasBaseType<Customer>();

        var newKey = modelBuilder.Model.FindEntityType(typeof(Customer))!.FindPrimaryKey()!;

        // The guard that makes this test meaningful: without it the assertions below could pass
        // simply because nothing was ever detached.
        Assert.NotSame(key, newKey);
        Assert.False(((IConventionKey)key).IsInModel);

        Assert.Equal("pk_customers", newKey.GetName(customers));
        Assert.Same(
            newKey,
            ((IConventionRelationalKeyOverrides)RelationalKeyOverrides.Find(newKey, customers)!).Key);
    }

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        // Split-table tests move Name to a secondary table; this stays on the main table so the
        // main store object keeps at least one non-key property, as required by validation.
        public string Address { get; set; } = null!;
    }

    private class SpecialCustomer : Customer
    {
        public string Tier { get; set; } = null!;
    }
}

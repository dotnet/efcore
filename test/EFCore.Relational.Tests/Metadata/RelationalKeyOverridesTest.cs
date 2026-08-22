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

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        // Split-table tests move Name to a secondary table; this stays on the main table so the
        // main store object keeps at least one non-key property, as required by validation.
        public string Address { get; set; } = null!;
    }
}

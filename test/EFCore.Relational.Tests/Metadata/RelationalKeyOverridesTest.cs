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

    private class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
    }
}

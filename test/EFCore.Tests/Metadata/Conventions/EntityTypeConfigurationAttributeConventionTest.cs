// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class EntityTypeConfigurationAttributeConventionTest
{
    [Fact]
    public void EntityTypeConfigurationAttribute_should_apply_configuration_to_EntityType()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<Customer>();

        var entityType = builder.Model.FindEntityType(typeof(Customer))!;
        Assert.Equal(1000, entityType.FindProperty(nameof(Customer.Name))!.GetMaxLength());
    }

    [Fact]
    public void EntityTypeConfigurationAttribute_should_apply_configuration_to_EntityType_Generic()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<CustomerGeneric>();

        var entityType = builder.Model.FindEntityType(typeof(CustomerGeneric))!;
        Assert.Equal(1000, entityType.FindProperty(nameof(CustomerGeneric.Name))!.GetMaxLength());
    }

    [Fact]
    public void EntityTypeConfigurationAttribute_should_apply_multiple_configurations_to_EntityType()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<MultiCustomer>();

        var entityType = builder.Model.FindEntityType(typeof(MultiCustomer))!;
        Assert.Equal(500, entityType.FindProperty(nameof(MultiCustomer.Name))!.GetMaxLength());
        Assert.Equal(250, entityType.FindProperty(nameof(MultiCustomer.Address))!.GetMaxLength());
    }

    [Fact]
    public void EntityTypeConfigurationAttribute_should_apply_multiple_configurations_to_EntityType_Generic()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<MultiCustomerGeneric>();

        var entityType = builder.Model.FindEntityType(typeof(MultiCustomerGeneric))!;
        Assert.Equal(500, entityType.FindProperty(nameof(MultiCustomerGeneric.Name))!.GetMaxLength());
        Assert.Equal(250, entityType.FindProperty(nameof(MultiCustomerGeneric.Address))!.GetMaxLength());
    }

    [Fact]
    public void EntityTypeConfigurationAttribute_should_apply_mixed_configurations_to_EntityType()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<MixedCustomer>();

        var entityType = builder.Model.FindEntityType(typeof(MixedCustomer))!;
        Assert.Equal(500, entityType.FindProperty(nameof(MixedCustomer.Name))!.GetMaxLength());
        Assert.Equal(250, entityType.FindProperty(nameof(MixedCustomer.Address))!.GetMaxLength());
    }

    [Fact]
    public void EntityTypeConfigurationAttribute_should_apply_configurations_across_partial_classes()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<PartialCustomer>();

        var entityType = builder.Model.FindEntityType(typeof(PartialCustomer))!;
        Assert.Equal(100, entityType.FindProperty(nameof(PartialCustomer.FirstName))!.GetMaxLength());
        Assert.Equal(200, entityType.FindProperty(nameof(PartialCustomer.LastName))!.GetMaxLength());
    }

    [Fact]
    public void EntityTypeConfigurationAttribute_should_ignore_duplicate_configurations()
    {
        CountingConfiguration.ExecutionCount = 0;
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<DuplicateCustomer>();

        var entityType = builder.Model.FindEntityType(typeof(DuplicateCustomer))!;
        Assert.Equal(100, entityType.FindProperty(nameof(DuplicateCustomer.Name))!.GetMaxLength());
        Assert.Equal(1, CountingConfiguration.ExecutionCount);
    }

    [Fact]
    public void EntityTypeConfigurationAttribute_does_not_inherit_to_derived_types()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<BaseCustomer>();
        builder.Entity<DerivedCustomer>();
        builder.Entity<DerivedCustomerWithoutAttribute>();

        var baseType = builder.Model.FindEntityType(typeof(BaseCustomer))!;
        var derivedType = builder.Model.FindEntityType(typeof(DerivedCustomer))!;
        var derivedWithoutAttrType = builder.Model.FindEntityType(typeof(DerivedCustomerWithoutAttribute))!;

        Assert.Equal(100, baseType.FindProperty(nameof(BaseCustomer.BaseName))!.GetMaxLength());
        Assert.Equal(200, derivedType.FindProperty(nameof(DerivedCustomer.DerivedName))!.GetMaxLength());
        Assert.NotNull(derivedWithoutAttrType);
    }

    [Fact]
    public void EntityTypeConfigurationAttribute_should_throw_when_configuration_is_wrong_type()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        Assert.Equal(
            CoreStrings.InvalidEntityTypeConfigurationAttribute(nameof(UserConfiguration), nameof(User)),
            Assert.Throws<InvalidOperationException>(() => builder.Entity<User>()).Message);
    }

    [Fact]
    public void EntityTypeConfigurationAttribute_should_throw_when_configuration_is_for_wrong_entity_type()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        Assert.Equal(
            CoreStrings.InvalidEntityTypeConfigurationAttribute(nameof(CustomerConfiguration), nameof(InvalidCustomer)),
            Assert.Throws<InvalidOperationException>(() => builder.Entity<InvalidCustomer>()).Message);
    }

    [Fact]
    public void EntityTypeConfigurationAttribute_should_throw_when_any_configuration_is_invalid()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        Assert.Equal(
            CoreStrings.InvalidEntityTypeConfigurationAttribute(nameof(UserConfiguration), nameof(PartiallyInvalidCustomer)),
            Assert.Throws<InvalidOperationException>(() => builder.Entity<PartiallyInvalidCustomer>()).Message);
    }

    private static IMutableModel CreateModel()
        => new Model();

    private class UserConfiguration;

    [EntityTypeConfiguration(typeof(UserConfiguration))]
    private class User
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }

    private class CustomerConfiguration : IEntityTypeConfiguration<Customer>
    {
        public void Configure(EntityTypeBuilder<Customer> builder)
            => builder.Property(c => c.Name).HasMaxLength(1000);
    }

    private class CustomerGenericConfiguration : IEntityTypeConfiguration<CustomerGeneric>
    {
        public void Configure(EntityTypeBuilder<CustomerGeneric> builder)
            => builder.Property(c => c.Name).HasMaxLength(1000);
    }

    [EntityTypeConfiguration(typeof(CustomerConfiguration))]
    private class Customer
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }

    [EntityTypeConfigurationAttribute<CustomerGenericConfiguration, CustomerGeneric>]
    private class CustomerGeneric
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }

    [EntityTypeConfiguration(typeof(CustomerConfiguration))]
    private class InvalidCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }

    private class MultiCustomerNameConfiguration : IEntityTypeConfiguration<MultiCustomer>
    {
        public void Configure(EntityTypeBuilder<MultiCustomer> builder)
            => builder.Property(c => c.Name).HasMaxLength(500);
    }

    private class MultiCustomerAddressConfiguration : IEntityTypeConfiguration<MultiCustomer>
    {
        public void Configure(EntityTypeBuilder<MultiCustomer> builder)
            => builder.Property(c => c.Address).HasMaxLength(250);
    }

    [EntityTypeConfiguration(typeof(MultiCustomerNameConfiguration))]
    [EntityTypeConfiguration(typeof(MultiCustomerAddressConfiguration))]
    private class MultiCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;
    }

    private class MultiCustomerGenericNameConfiguration : IEntityTypeConfiguration<MultiCustomerGeneric>
    {
        public void Configure(EntityTypeBuilder<MultiCustomerGeneric> builder)
            => builder.Property(c => c.Name).HasMaxLength(500);
    }

    private class MultiCustomerGenericAddressConfiguration : IEntityTypeConfiguration<MultiCustomerGeneric>
    {
        public void Configure(EntityTypeBuilder<MultiCustomerGeneric> builder)
            => builder.Property(c => c.Address).HasMaxLength(250);
    }

    [EntityTypeConfigurationAttribute<MultiCustomerGenericNameConfiguration, MultiCustomerGeneric>]
    [EntityTypeConfigurationAttribute<MultiCustomerGenericAddressConfiguration, MultiCustomerGeneric>]
    private class MultiCustomerGeneric
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;
    }

    private class MixedCustomerNameConfiguration : IEntityTypeConfiguration<MixedCustomer>
    {
        public void Configure(EntityTypeBuilder<MixedCustomer> builder)
            => builder.Property(c => c.Name).HasMaxLength(500);
    }

    private class MixedCustomerAddressConfiguration : IEntityTypeConfiguration<MixedCustomer>
    {
        public void Configure(EntityTypeBuilder<MixedCustomer> builder)
            => builder.Property(c => c.Address).HasMaxLength(250);
    }

    [EntityTypeConfiguration(typeof(MixedCustomerNameConfiguration))]
    [EntityTypeConfigurationAttribute<MixedCustomerAddressConfiguration, MixedCustomer>]
    private class MixedCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;

        public string Address { get; set; } = null!;
    }

    private class PartialCustomerFirstNameConfiguration : IEntityTypeConfiguration<PartialCustomer>
    {
        public void Configure(EntityTypeBuilder<PartialCustomer> builder)
            => builder.Property(c => c.FirstName).HasMaxLength(100);
    }

    private class PartialCustomerLastNameConfiguration : IEntityTypeConfiguration<PartialCustomer>
    {
        public void Configure(EntityTypeBuilder<PartialCustomer> builder)
            => builder.Property(c => c.LastName).HasMaxLength(200);
    }

    [EntityTypeConfiguration(typeof(PartialCustomerFirstNameConfiguration))]
    private partial class PartialCustomer
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;
    }

    [EntityTypeConfiguration(typeof(PartialCustomerLastNameConfiguration))]
    private partial class PartialCustomer
    {
        public string LastName { get; set; } = null!;
    }

    private class CountingConfiguration : IEntityTypeConfiguration<DuplicateCustomer>
    {
        public static int ExecutionCount;

        public void Configure(EntityTypeBuilder<DuplicateCustomer> builder)
        {
            ExecutionCount++;
            builder.Property(c => c.Name).HasMaxLength(100);
        }
    }

    [EntityTypeConfiguration(typeof(CountingConfiguration))]
    [EntityTypeConfiguration(typeof(CountingConfiguration))]
    [EntityTypeConfigurationAttribute<CountingConfiguration, DuplicateCustomer>]
    private class DuplicateCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }

    private class BaseCustomerConfiguration : IEntityTypeConfiguration<BaseCustomer>
    {
        public void Configure(EntityTypeBuilder<BaseCustomer> builder)
            => builder.Property(c => c.BaseName).HasMaxLength(100);
    }

    private class DerivedCustomerConfiguration : IEntityTypeConfiguration<DerivedCustomer>
    {
        public void Configure(EntityTypeBuilder<DerivedCustomer> builder)
            => builder.Property(c => c.DerivedName).HasMaxLength(200);
    }

    [EntityTypeConfiguration(typeof(BaseCustomerConfiguration))]
    private class BaseCustomer
    {
        public int Id { get; set; }

        public string BaseName { get; set; } = null!;
    }

    [EntityTypeConfiguration(typeof(DerivedCustomerConfiguration))]
    private class DerivedCustomer : BaseCustomer
    {
        public string DerivedName { get; set; } = null!;
    }

    private class DerivedCustomerWithoutAttribute : BaseCustomer
    {
        public string Extra { get; set; } = null!;
    }

    private class PartiallyInvalidCustomerValidConfiguration : IEntityTypeConfiguration<PartiallyInvalidCustomer>
    {
        public void Configure(EntityTypeBuilder<PartiallyInvalidCustomer> builder)
            => builder.Property(c => c.Name).HasMaxLength(500);
    }

    [EntityTypeConfiguration(typeof(PartiallyInvalidCustomerValidConfiguration))]
    [EntityTypeConfiguration(typeof(UserConfiguration))]
    private class PartiallyInvalidCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; } = null!;
    }
}

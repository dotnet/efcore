// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class EntityTypeConfigurationAttributeConventionTest
{
    [ConditionalFact]
    public void EntityTypeConfigurationAttribute_should_apply_configuration_to_EntityType()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<Customer>();

        var entityType = builder.Model.FindEntityType(typeof(Customer));
        Assert.Equal(1000, entityType.FindProperty(nameof(Customer.Name)).GetMaxLength());
    }

    [ConditionalFact]
    public void EntityTypeConfigurationAttribute_should_apply_configuration_to_EntityType_Generic()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        builder.Entity<CustomerGeneric>();

        var entityType = builder.Model.FindEntityType(typeof(CustomerGeneric));
        Assert.Equal(1000, entityType.FindProperty(nameof(CustomerGeneric.Name)).GetMaxLength());
    }

    [ConditionalFact]
    public void EntityTypeConfigurationAttribute_should_throw_when_configuration_is_wrong_type()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        Assert.Equal(
            CoreStrings.InvalidEntityTypeConfigurationAttribute(nameof(UserConfiguration), nameof(User)),
            Assert.Throws<InvalidOperationException>(() => builder.Entity<User>()).Message);
    }

    [ConditionalFact]
    public void EntityTypeConfigurationAttribute_should_throw_when_configuration_is_for_wrong_entity_type()
    {
        var builder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        Assert.Equal(
            CoreStrings.InvalidEntityTypeConfigurationAttribute(nameof(CustomerConfiguration), nameof(InvalidCustomer)),
            Assert.Throws<InvalidOperationException>(() => builder.Entity<InvalidCustomer>()).Message);
    }

    private static IMutableModel CreateModel()
        => new Model();

    private class UserConfiguration;

    [EntityTypeConfiguration(typeof(UserConfiguration))]
    private class User
    {
        public int Id { get; set; }

        public string Name { get; set; }
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

        public string Name { get; set; }
    }

    [EntityTypeConfigurationAttribute<CustomerGenericConfiguration, CustomerGeneric>]
    private class CustomerGeneric
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    [EntityTypeConfiguration(typeof(CustomerConfiguration))]
    private class InvalidCustomer
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}

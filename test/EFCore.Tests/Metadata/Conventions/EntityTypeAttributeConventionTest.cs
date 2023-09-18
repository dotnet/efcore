// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable UnusedMember.Local
// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class EntityTypeAttributeConventionTest
{
    #region NotMappedAttribute

    [ConditionalFact]
    public void NotMappedAttribute_overrides_configuration_from_convention_source()
    {
        var modelBuilder = new InternalModelBuilder(new Model());

        var entityBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Convention);

        RunConvention(entityBuilder);

        Assert.Empty(modelBuilder.Metadata.GetEntityTypes());
    }

    [ConditionalFact]
    public void NotMappedAttribute_does_not_override_configuration_from_explicit_source()
    {
        var modelBuilder = new InternalModelBuilder(new Model());

        var entityBuilder = modelBuilder.Entity(typeof(A), ConfigurationSource.Explicit);

        RunConvention(entityBuilder);

        Assert.Single(modelBuilder.Metadata.GetEntityTypes());
    }

    [ConditionalFact]
    public void NotMappedAttribute_ignores_entityTypes_with_conventional_builder()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<B>();

        Assert.Single(modelBuilder.Model.GetEntityTypes());
    }

    #endregion

    #region OwnedAttribute

    [ConditionalFact]
    public void OwnedAttribute_configures_entity_as_owned()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity<Customer>();

        Assert.Equal(2, modelBuilder.Model.GetEntityTypes().Count());
        Assert.True(
            modelBuilder.Model.FindEntityType(typeof(Customer)).FindNavigation(nameof(Customer.Address)).ForeignKey.IsOwnership);
    }

    [ConditionalFact]
    public void Entity_marked_with_OwnedAttribute_cannot_be_configured_as_regular_entity()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        Assert.Equal(
            CoreStrings.ClashingOwnedEntityType(nameof(Address)),
            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<Customer>().HasOne(e => e.Address).WithOne(e => e.Customer)).Message);
    }

    #endregion

    #region KeylessAttribute

    [ConditionalFact]
    public void KeylessAttribute_overrides_configuration_from_convention()
    {
        var modelBuilder = new InternalModelBuilder(new Model());

        var entityBuilder = modelBuilder.Entity(typeof(KeylessEntity), ConfigurationSource.Convention);
        entityBuilder.Property("Id", ConfigurationSource.Convention);
        entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

        Assert.NotNull(entityBuilder.Metadata.FindPrimaryKey());

        RunConvention(entityBuilder);

        Assert.Null(entityBuilder.Metadata.FindPrimaryKey());
        Assert.True(entityBuilder.Metadata.IsKeyless);
    }

    [ConditionalFact]
    public void KeylessAttribute_can_be_overriden_using_explicit_configuration()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        var entityBuilder = modelBuilder.Entity<KeylessEntity>();

        Assert.True(entityBuilder.Metadata.IsKeyless);

        entityBuilder.HasKey(e => e.Id);

        Assert.False(entityBuilder.Metadata.IsKeyless);
        Assert.NotNull(entityBuilder.Metadata.FindPrimaryKey());
    }

    [ConditionalFact]
    public void KeyAttribute_does_not_override_keyless_attribute()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        var entityBuilder = modelBuilder.Entity<KeyClash>();

        Assert.True(entityBuilder.Metadata.IsKeyless);
        Assert.Null(entityBuilder.Metadata.FindPrimaryKey());
    }

    #endregion

    private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var context = new ConventionContext<IConventionEntityTypeBuilder>(entityTypeBuilder.Metadata.Model.ConventionDispatcher);

        new NotMappedTypeAttributeConvention(CreateDependencies())
            .ProcessEntityTypeAdded(entityTypeBuilder, context);

        new OwnedAttributeConvention(CreateDependencies())
            .ProcessEntityTypeAdded(entityTypeBuilder, context);

        new KeylessAttributeConvention(CreateDependencies())
            .ProcessEntityTypeAdded(entityTypeBuilder, context);
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    [NotMapped]
    private class A
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }

    private class B
    {
        public int Id { get; set; }

        public virtual A NavToA { get; set; }
    }

    private class Customer
    {
        public int Id { get; set; }
        public Address Address { get; set; }
    }

    [Owned]
    private class Address
    {
        public int Id { get; set; }
        public Customer Customer { get; }
    }

    [Keyless]
    private class KeylessEntity
    {
        public int Id { get; set; }
    }

    [Keyless]
    private class KeyClash
    {
        [Key]
        public int MyId { get; set; }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

#nullable enable

public class PrimaryKeyAttributeConventionTest
{
    [ConditionalFact]
    public void PrimaryKeyAttribute_overrides_configuration_from_convention()
    {
        var modelBuilder = new InternalModelBuilder(new Model());

        var entityBuilder = modelBuilder.Entity(typeof(EntityWithPrimaryKey), ConfigurationSource.Convention)!;
        entityBuilder.Property("Id", ConfigurationSource.Convention);
        var propABuilder = entityBuilder.Property("A", ConfigurationSource.Convention)!;
        var propBBuilder = entityBuilder.Property("B", ConfigurationSource.Convention)!;
        entityBuilder.PrimaryKey(new List<string> { "Id" }, ConfigurationSource.Convention);

        var primaryKeyProperties = new List<string> { propABuilder.Metadata.Name, propBBuilder.Metadata.Name };
        entityBuilder.PrimaryKey(primaryKeyProperties, ConfigurationSource.Convention);

        RunConvention(entityBuilder);
        RunConvention(modelBuilder);

        var primaryKey = entityBuilder.Metadata.FindPrimaryKey()!;
        Assert.Same(primaryKey, entityBuilder.Metadata.GetKeys().Single());
        Assert.Equal(ConfigurationSource.DataAnnotation, primaryKey.GetConfigurationSource());
        Assert.Collection(
            primaryKey.Properties,
            prop0 => Assert.Equal("A", prop0.Name),
            prop1 => Assert.Equal("B", prop1.Name));
    }

    [ConditionalFact]
    public void PrimaryKeyAttribute_can_be_overriden_using_explicit_configuration()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var entityBuilder = modelBuilder.Entity<EntityWithPrimaryKey>();

        entityBuilder.HasKey(e => e.A);

        modelBuilder.Model.FinalizeModel();

        var primaryKey = (Key)entityBuilder.Metadata.FindPrimaryKey()!;
        Assert.Equal(ConfigurationSource.Explicit, primaryKey.GetConfigurationSource());
        Assert.Collection(
            primaryKey.Properties,
            prop0 => Assert.Equal("A", prop0.Name));
    }

    [ConditionalFact]
    public void PrimaryKeyAttribute_with_null_properties_array_throws()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        Assert.Throws<ArgumentNullException>(() => modelBuilder.Entity<EntityWithInvalidNullAdditionalProperties>());
    }

    [InlineData(typeof(EntityWithInvalidNullAdditionalProperty))]
    [InlineData(typeof(EntityWithInvalidEmptyPrimaryKeyProperty))]
    [InlineData(typeof(EntityWithInvalidWhiteSpacePrimaryKeyProperty))]
    [ConditionalTheory]
    public void PrimaryKeyAttribute_properties_cannot_include_whitespace(Type entityTypeWithInvalidPrimaryKey)
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        Assert.Equal(
            AbstractionsStrings.CollectionArgumentHasEmptyElements("additionalPropertyNames"),
            Assert.Throws<ArgumentException>(
                () => modelBuilder.Entity(entityTypeWithInvalidPrimaryKey)).Message);
    }

    [ConditionalFact]
    public void PrimaryKeyAttribute_can_be_inherited_from_base_entity_type()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var entityBuilder = modelBuilder.Entity<EntityWithPrimaryKeyFromBaseType>();
        modelBuilder.Model.FinalizeModel();

        // assert that the base type is not part of the model
        Assert.Empty(
            modelBuilder.Model.GetEntityTypes()
                .Where(e => e.ClrType == typeof(BaseUnmappedEntityWithPrimaryKey)));

        // assert that we see the primaryKey anyway
        var primaryKey = (Key)entityBuilder.Metadata.FindPrimaryKey()!;
        Assert.Equal(ConfigurationSource.DataAnnotation, primaryKey.GetConfigurationSource());
        Assert.Collection(
            primaryKey.Properties,
            prop0 => Assert.Equal("A", prop0.Name),
            prop1 => Assert.Equal("B", prop1.Name));
    }

    [ConditionalFact]
    public virtual void PrimaryKeyAttribute_on_ignored_property_causes_error()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<EntityPrimaryKeyWithIgnoredProperty>();

        Assert.Equal(
            CoreStrings.PrimaryKeyDefinedOnIgnoredProperty(nameof(EntityPrimaryKeyWithIgnoredProperty), "B"),
            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Model.FinalizeModel()).Message);
    }

    [ConditionalFact]
    public virtual void PrimaryKeyAttribute_with_non_existent_property_causes_error()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        modelBuilder.Entity<EntityPrimaryKeyWithNonExistentProperty>();

        Assert.Equal(
            CoreStrings.PrimaryKeyDefinedOnNonExistentProperty(
                nameof(EntityPrimaryKeyWithNonExistentProperty),
                "{'A', 'DoesNotExist'}",
                "DoesNotExist"),
            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Model.FinalizeModel()).Message);
    }

    [ConditionalFact]
    public virtual void PrimaryKeyAttribute_and_KeylessAttribute_on_same_type()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();

        Assert.Equal(
            CoreStrings.ConflictingKeylessAndPrimaryKeyAttributes(nameof(EntityPrimaryKeyAndKeyless)),
            Assert.Throws<InvalidOperationException>(
                () => modelBuilder.Entity<EntityPrimaryKeyAndKeyless>()).Message);
    }

    [ConditionalFact]
    public void PrimaryKeyAttribute_primaryKey_replicated_to_derived_type_when_base_type_changes()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var grandparentEntityBuilder = modelBuilder.Entity(typeof(GrandparentEntityWithPrimaryKey));
        var parentEntityBuilder = modelBuilder.Entity<ParentEntity>();
        var childEntityBuilder = modelBuilder.Entity<ChildEntity>();

        Assert.NotNull(parentEntityBuilder.Metadata.BaseType);
        Assert.NotNull(childEntityBuilder.Metadata.BaseType);
        Assert.Single(grandparentEntityBuilder.Metadata.GetDeclaredKeys());
        Assert.Empty(parentEntityBuilder.Metadata.GetDeclaredKeys());
        Assert.Empty(childEntityBuilder.Metadata.GetDeclaredKeys());

        parentEntityBuilder.HasBaseType((string)null!);

        Assert.Null(parentEntityBuilder.Metadata.BaseType);
        Assert.NotNull(childEntityBuilder.Metadata.BaseType);

        var basePrimaryKey = (Key)grandparentEntityBuilder.Metadata.FindPrimaryKey()!;
        Assert.Equal(ConfigurationSource.DataAnnotation, basePrimaryKey.GetConfigurationSource());
        var basePrimaryKeyProperty = Assert.Single(basePrimaryKey.Properties);
        Assert.Equal("B", basePrimaryKeyProperty.Name);

        var primaryKey = (Key)parentEntityBuilder.Metadata.FindPrimaryKey()!;
        Assert.Equal(ConfigurationSource.DataAnnotation, primaryKey.GetConfigurationSource());
        var primaryKeyProperty = Assert.Single(primaryKey.Properties);
        Assert.Equal("A", primaryKeyProperty.Name);

        var childPrimaryKey = (Key)childEntityBuilder.Metadata.FindPrimaryKey()!;
        Assert.Equal(ConfigurationSource.DataAnnotation, childPrimaryKey.GetConfigurationSource());
        var childPrimaryKeyProperty = Assert.Single(childPrimaryKey.Properties);
        Assert.Equal("A", childPrimaryKeyProperty.Name);

        // Check there are no errors.
        modelBuilder.Model.FinalizeModel();
    }

    [ConditionalFact]
    public void PrimaryKeyAttribute_primaryKey_is_created_when_missing_property_added()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(EntityWithPrimaryKeyOnShadowProperty));

        Assert.Equal("Id", entityBuilder.Metadata.FindPrimaryKey()!.Properties.Single().Name);

        entityBuilder.Property<int>("Y");
        modelBuilder.Model.FinalizeModel();

        var primaryKey = (Key)entityBuilder.Metadata.FindPrimaryKey()!;
        Assert.Equal(ConfigurationSource.DataAnnotation, primaryKey.GetConfigurationSource());
        Assert.Collection(
            primaryKey.Properties,
            prop0 => Assert.Equal("X", prop0.Name),
            prop1 => Assert.Equal("Y", prop1.Name));
    }

    [ConditionalFact]
    public void PrimaryKeyAttribute_primaryKey_is_created_when_primaryKey_on_private_property()
    {
        var modelBuilder = InMemoryTestHelpers.Instance.CreateConventionBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(EntityWithPrimaryKeyOnPrivateProperty));
        modelBuilder.Model.FinalizeModel();

        var primaryKey = (Key)entityBuilder.Metadata.FindPrimaryKey()!;
        Assert.Equal(ConfigurationSource.DataAnnotation, primaryKey.GetConfigurationSource());
        Assert.Collection(
            primaryKey.Properties,
            prop0 => Assert.Equal("X", prop0.Name),
            prop1 => Assert.Equal("Y", prop1.Name));
    }

    private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var context = new ConventionContext<IConventionEntityTypeBuilder>(
            entityTypeBuilder.Metadata.Model.ConventionDispatcher);

        CreatePrimaryKeyAttributeConvention().ProcessEntityTypeAdded(entityTypeBuilder, context);
    }

    private void RunConvention(InternalModelBuilder modelBuilder)
    {
        var context = new ConventionContext<IConventionModelBuilder>(modelBuilder.Metadata.ConventionDispatcher);

        CreatePrimaryKeyAttributeConvention().ProcessModelFinalizing(modelBuilder, context);
    }

    private KeyAttributeConvention CreatePrimaryKeyAttributeConvention()
        => new(CreateDependencies());

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    [PrimaryKey(nameof(A), nameof(B))]
    private class EntityWithPrimaryKey
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
    }

    [PrimaryKey(nameof(A), nameof(B))]
    [NotMapped]
    private class BaseUnmappedEntityWithPrimaryKey
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
    }

    private class EntityWithPrimaryKeyFromBaseType : BaseUnmappedEntityWithPrimaryKey
    {
        public int C { get; set; }
        public int D { get; set; }
    }

    [PrimaryKey(nameof(A), null!)]
    private class EntityWithInvalidNullAdditionalProperties
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
    }

    [PrimaryKey(nameof(A), nameof(B), null!)]
    private class EntityWithInvalidNullAdditionalProperty
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
    }

    [PrimaryKey(nameof(A), "")]
    private class EntityWithInvalidEmptyPrimaryKeyProperty
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
    }

    [PrimaryKey(nameof(A), " \r\n\t")]
    private class EntityWithInvalidWhiteSpacePrimaryKeyProperty
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
    }

    [PrimaryKey(nameof(A), nameof(B))]
    private class EntityPrimaryKeyWithIgnoredProperty
    {
        public int Id { get; set; }
        public int A { get; set; }

        [NotMapped]
        public int B { get; set; }
    }

    [PrimaryKey(nameof(A), "DoesNotExist")]
    private class EntityPrimaryKeyWithNonExistentProperty
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
    }

    [PrimaryKey(nameof(B))]
    private class GrandparentEntityWithPrimaryKey
    {
        public int Id { get; set; }
        public int A { get; set; }
        public int B { get; set; }
    }

    [PrimaryKey(nameof(A))]
    private class ParentEntity : GrandparentEntityWithPrimaryKey
    {
        public int C { get; set; }
        public int D { get; set; }
    }

    private class ChildEntity : ParentEntity
    {
        public int E { get; set; }
        public int F { get; set; }
    }

    [PrimaryKey(nameof(X), "Y")]
    private class EntityWithPrimaryKeyOnShadowProperty
    {
        public int Id { get; set; }
        public int X { get; set; }
    }

    [PrimaryKey(nameof(X), nameof(Y))]
    private class EntityWithPrimaryKeyOnPrivateProperty
    {
        public int Id { get; set; }
        public int X { get; set; }
        private int Y { get; set; }
    }

    [PrimaryKey("Id")]
    [Keyless]
    private class EntityPrimaryKeyAndKeyless
    {
        public int Id { get; set; }
    }
}

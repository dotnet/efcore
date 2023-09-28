// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
// ReSharper disable UnusedMember.Local
namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class ValueGeneratorConventionTest
{
    private class SampleEntity
    {
        public int Id { get; set; }
        public int Number { get; set; }
        public string Name { get; set; }
    }

    private class ReferencedEntity
    {
        public int Id { get; set; }
        public int SampleEntityId { get; set; }
    }

    private enum Eenom
    {
        E,
        Nom
    }

    #region RequiresValueGenerator

    [ConditionalFact]
    public void RequiresValueGenerator_flag_is_set_for_key_properties_that_use_value_generation()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

        var properties = new List<string> { "Id", "Name" };

        entityBuilder.Property(properties[0], ConfigurationSource.Convention)
            .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

        var keyBuilder = entityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

        RunConvention(entityBuilder);

        var keyProperties = keyBuilder.Metadata.Properties;

        Assert.True(keyProperties[0].RequiresValueGenerator());
        Assert.False(keyProperties[1].RequiresValueGenerator());

        Assert.Equal(ValueGenerated.OnAdd, keyProperties[0].ValueGenerated);
        Assert.Equal(ValueGenerated.Never, keyProperties[1].ValueGenerated);
    }

    [ConditionalFact]
    public void RequiresValueGenerator_flag_is_not_set_for_foreign_key()
    {
        var modelBuilder = CreateInternalModelBuilder();

        var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

        var properties = new List<string> { "SampleEntityId" };

        referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention);

        referencedEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
            ConfigurationSource.Convention);

        var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

        RunConvention(referencedEntityBuilder);

        var keyProperties = keyBuilder.Metadata.Properties;

        Assert.False(keyProperties[0].RequiresValueGenerator());
        Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
    }

    [ConditionalFact]
    public void RequiresValueGenerator_flag_is_set_for_property_which_are_not_part_of_any_foreign_key()
    {
        var modelBuilder = CreateInternalModelBuilder();

        var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

        var properties = new List<string> { "Id", "SampleEntityId" };
        referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention)
            .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);
        referencedEntityBuilder.Property(properties[1], ConfigurationSource.Convention)
            .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

        referencedEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            referencedEntityBuilder.GetOrCreateProperties(new[] { properties[1] }, ConfigurationSource.Convention),
            ConfigurationSource.Convention);

        var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

        RunConvention(referencedEntityBuilder);

        var keyProperties = keyBuilder.Metadata.Properties;

        Assert.True(keyProperties[0].RequiresValueGenerator());
        Assert.False(keyProperties[1].RequiresValueGenerator());
    }

    [ConditionalFact]
    public void RequiresValueGenerator_flag_is_not_set_for_properties_which_are_part_of_a_foreign_key()
    {
        var modelBuilder = CreateInternalModelBuilder();

        var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

        var properties = new List<string> { "Id", "SampleEntityId" };

        referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention)
            .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

        referencedEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
            ConfigurationSource.Convention);

        var keyBuilder = referencedEntityBuilder.PrimaryKey(new[] { properties[1] }, ConfigurationSource.Convention);

        RunConvention(referencedEntityBuilder);

        var keyProperties = keyBuilder.Metadata.Properties;

        Assert.False(keyProperties[0].RequiresValueGenerator());
        Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
    }

    [ConditionalFact]
    public void KeyConvention_does_not_override_ValueGenerated_when_configured_explicitly()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

        var properties = new List<string> { "Id" };

        entityBuilder.Property(properties[0], ConfigurationSource.Convention)
            .ValueGenerated(ValueGenerated.OnAdd, ConfigurationSource.Explicit);

        var keyBuilder = entityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

        RunConvention(entityBuilder);

        var keyProperties = keyBuilder.Metadata.Properties;

        Assert.Equal(ValueGenerated.OnAdd, keyProperties[0].ValueGenerated);
    }

    [ConditionalFact]
    public void RequiresValueGenerator_flag_is_turned_off_when_foreign_key_is_added()
    {
        var modelBuilder = CreateInternalModelBuilder();

        var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

        var properties = new List<string> { "SampleEntityId" };

        referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention);

        var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

        RunConvention(referencedEntityBuilder);

        var keyProperties = keyBuilder.Metadata.Properties;

        Assert.True(keyProperties[0].RequiresValueGenerator());

        var foreignKeyBuilder = referencedEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
            ConfigurationSource.Convention);

        RunConvention(foreignKeyBuilder);

        Assert.False(keyProperties[0].RequiresValueGenerator());
        Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
    }

    [ConditionalFact]
    public void RequiresValueGenerator_flag_is_set_when_foreign_key_is_removed()
    {
        var modelBuilder = CreateInternalModelBuilder();

        var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

        var properties = new List<string> { "SampleEntityId" };

        referencedEntityBuilder.Property(properties[0], ConfigurationSource.Convention);

        var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

        RunConvention(referencedEntityBuilder);

        var keyProperties = keyBuilder.Metadata.Properties;

        Assert.True(keyProperties[0].RequiresValueGenerator());

        var relationshipBuilder = referencedEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
            ConfigurationSource.Convention);

        RunConvention(relationshipBuilder);

        Assert.False(keyProperties[0].RequiresValueGenerator());
        Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);

        referencedEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.Convention);

        RunConvention(referencedEntityBuilder, relationshipBuilder.Metadata);

        Assert.True(keyProperties[0].RequiresValueGenerator());
        Assert.Equal(ValueGenerated.OnAdd, keyProperties[0].ValueGenerated);
    }

    #endregion

    #region Identity

    [ConditionalFact]
    public void Identity_is_set_for_primary_key()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

        var keyBuilder = entityBuilder.PrimaryKey(
            new List<string> { "Id" }, ConfigurationSource.Convention);

        RunConvention(entityBuilder);

        var property = keyBuilder.Metadata.Properties.First();

        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Identity_is_not_set_for_non_primary_key()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

        var keyBuilder = entityBuilder.HasKey(
            new List<string> { "Number" }, ConfigurationSource.Convention);

        RunConvention(entityBuilder);

        var property = keyBuilder.Metadata.Properties.First();

        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Identity_not_set_when_composite_primary_key()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

        var keyBuilder = entityBuilder.PrimaryKey(
            new List<string> { "Id", "Number" }, ConfigurationSource.Convention);

        RunConvention(entityBuilder);

        var keyProperties = keyBuilder.Metadata.Properties;

        Assert.Equal(ValueGenerated.Never, keyProperties[0].ValueGenerated);
        Assert.Equal(ValueGenerated.Never, keyProperties[1].ValueGenerated);
    }

    [ConditionalFact]
    public void Identity_not_set_when_primary_key_property_is_string()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

        var keyBuilder = entityBuilder.PrimaryKey(
            new List<string> { "Name" }, ConfigurationSource.Convention);

        var property = keyBuilder.Metadata.Properties.First();

        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.False(property.RequiresValueGenerator());
    }

    [ConditionalFact]
    public void Identity_not_set_when_primary_key_property_is_byte_array()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        entityBuilder.Property(typeof(byte[]), "binaryKey", ConfigurationSource.Explicit);

        var keyBuilder = entityBuilder.PrimaryKey(new[] { "binaryKey" }, ConfigurationSource.Convention);

        var property = keyBuilder.Metadata.Properties.First();

        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.False(property.RequiresValueGenerator());
    }

    [ConditionalFact]
    public void Identity_not_set_when_primary_key_property_is_enum()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        entityBuilder.Property(typeof(Eenom), "enumKey", ConfigurationSource.Explicit);

        var keyBuilder = entityBuilder.PrimaryKey(new[] { "enumKey" }, ConfigurationSource.Convention);

        var property = keyBuilder.Metadata.Properties.First();

        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
        Assert.False(property.RequiresValueGenerator());
    }

    [ConditionalFact]
    public void Identity_is_recomputed_when_primary_key_is_changed()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

        var idProperty = entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention).Metadata;
        var numberProperty = entityBuilder.Property(typeof(int), "Number", ConfigurationSource.Convention).Metadata;

        Assert.Same(idProperty, entityBuilder.Metadata.FindProperty("Id"));
        Assert.Same(numberProperty, entityBuilder.Metadata.FindProperty("Number"));

        Assert.Equal(ValueGenerated.OnAdd, idProperty.ValueGenerated);
        Assert.Equal(ValueGenerated.Never, numberProperty.ValueGenerated);

        var keyBuilder = entityBuilder.PrimaryKey(
            new List<string> { "Number" }, ConfigurationSource.Convention);
        Assert.NotNull(keyBuilder);

        Assert.Same(idProperty, entityBuilder.Metadata.FindProperty("Id"));
        Assert.Same(numberProperty, entityBuilder.Metadata.FindProperty("Number"));

        RunConvention(entityBuilder);

        Assert.Same(idProperty, entityBuilder.Metadata.FindProperty("Id"));
        Assert.Same(numberProperty, entityBuilder.Metadata.FindProperty("Number"));

        Assert.Equal(ValueGenerated.Never, ((IReadOnlyProperty)idProperty).ValueGenerated);
        Assert.Equal(ValueGenerated.OnAdd, ((IReadOnlyProperty)numberProperty).ValueGenerated);
    }

    [ConditionalFact]
    public void Convention_does_not_override_None_when_configured_explicitly()
    {
        var modelBuilder = CreateInternalModelBuilder();
        var entityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);

        entityBuilder.Property(typeof(int), "Id", ConfigurationSource.Convention)
            .ValueGenerated(ValueGenerated.Never, ConfigurationSource.Explicit);

        var keyBuilder = entityBuilder.PrimaryKey(
            new List<string> { "Id" }, ConfigurationSource.Convention);

        RunConvention(entityBuilder);

        var property = keyBuilder.Metadata.Properties.First();

        Assert.Equal(ValueGenerated.Never, property.ValueGenerated);
    }

    [ConditionalFact]
    public void Identity_is_removed_when_foreign_key_is_added()
    {
        var modelBuilder = CreateInternalModelBuilder();

        var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

        var properties = new List<string> { "Id" };
        var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

        RunConvention(referencedEntityBuilder);

        var property = keyBuilder.Metadata.Properties.First();

        Assert.Equal(ValueGenerated.OnAdd, ((IReadOnlyProperty)property).ValueGenerated);

        var relationshipBuilder = referencedEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
            ConfigurationSource.Convention);

        RunConvention(relationshipBuilder);

        Assert.Equal(ValueGenerated.Never, ((IReadOnlyProperty)property).ValueGenerated);
    }

    [ConditionalFact]
    public void Identity_is_added_when_foreign_key_is_removed_and_key_is_primary_key()
    {
        var modelBuilder = CreateInternalModelBuilder();

        var principalEntityBuilder = modelBuilder.Entity(typeof(SampleEntity), ConfigurationSource.Convention);
        var referencedEntityBuilder = modelBuilder.Entity(typeof(ReferencedEntity), ConfigurationSource.Convention);

        var properties = new List<string> { "Id" };
        var keyBuilder = referencedEntityBuilder.PrimaryKey(properties, ConfigurationSource.Convention);

        RunConvention(referencedEntityBuilder);

        var property = keyBuilder.Metadata.Properties.First();

        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);

        var relationshipBuilder = referencedEntityBuilder.HasRelationship(
            principalEntityBuilder.Metadata,
            referencedEntityBuilder.GetOrCreateProperties(properties, ConfigurationSource.Convention),
            ConfigurationSource.Convention);

        RunConvention(relationshipBuilder);

        Assert.Equal(ValueGenerated.Never, ((IReadOnlyProperty)property).ValueGenerated);

        referencedEntityBuilder.HasNoRelationship(relationshipBuilder.Metadata, ConfigurationSource.Convention);

        RunConvention(referencedEntityBuilder);

        Assert.Equal(ValueGenerated.OnAdd, property.ValueGenerated);
    }

    #endregion

    private static void RunConvention(InternalEntityTypeBuilder entityBuilder)
        => new ValueGenerationConvention(CreateDependencies())
            .ProcessEntityTypePrimaryKeyChanged(
                entityBuilder, entityBuilder.Metadata.FindPrimaryKey(), null,
                new ConventionContext<IConventionKey>(entityBuilder.Metadata.Model.ConventionDispatcher));

    private static void RunConvention(InternalForeignKeyBuilder foreignKeyBuilder)
        => new ValueGenerationConvention(CreateDependencies())
            .ProcessForeignKeyAdded(
                foreignKeyBuilder,
                new ConventionContext<IConventionForeignKeyBuilder>(
                    foreignKeyBuilder.Metadata.DeclaringEntityType.Model.ConventionDispatcher));

    private static void RunConvention(InternalEntityTypeBuilder entityBuilder, ForeignKey foreignKey)
        => new ValueGenerationConvention(CreateDependencies())
            .ProcessForeignKeyRemoved(
                entityBuilder, foreignKey,
                new ConventionContext<IConventionForeignKey>(entityBuilder.Metadata.Model.ConventionDispatcher));

    private static ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    private static InternalModelBuilder CreateInternalModelBuilder()
    {
        var serviceProvider = InMemoryTestHelpers.Instance.CreateContextServices();
        var conventionSet = new ConventionSet();
        var dependencies = serviceProvider.GetRequiredService<ProviderConventionSetBuilderDependencies>();

        // Use public API to add conventions, issue #214
        conventionSet.EntityTypeAddedConventions.Add(new PropertyDiscoveryConvention(dependencies));
        conventionSet.EntityTypeAddedConventions.Add(new KeyDiscoveryConvention(dependencies));

        var keyConvention = new ValueGenerationConvention(dependencies);

        conventionSet.ForeignKeyAddedConventions.Add(keyConvention);
        conventionSet.ForeignKeyRemovedConventions.Add(keyConvention);
        conventionSet.EntityTypePrimaryKeyChangedConventions.Add(keyConvention);

        return new Model(conventionSet, serviceProvider.GetRequiredService<ModelDependencies>()).Builder;
    }
}

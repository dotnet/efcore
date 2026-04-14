// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable CollectionNeverUpdated.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable InconsistentNaming

namespace Microsoft.EntityFrameworkCore.Metadata.Builders;

public class MetadataBuilderTest
{
    [ConditionalFact]
    public void Can_write_model_builder_extension()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .ModelBuilderExtension("V1")
            .ModelBuilderExtension("V2");

        Assert.IsAssignableFrom<ModelBuilder>(returnedBuilder);

        var model = builder.Model;

        Assert.Equal("V2.Annotation", model["Annotation"]);
        Assert.Equal("V2.Metadata", model["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_entity_builder_extension()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity(typeof(Gunter))
            .EntityBuilderExtension("V1")
            .EntityBuilderExtension("V2");

        Assert.IsType<EntityTypeBuilder>(returnedBuilder);

        var model = builder.Model;
        var entityType = model.FindEntityType(typeof(Gunter));

        Assert.Equal("V2.Annotation", entityType["Annotation"]);
        Assert.Equal("V2.Metadata", entityType["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_entity_builder_extension_and_use_with_generic_builder()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity<Gunter>()
            .EntityBuilderExtension("V1")
            .EntityBuilderExtension("V2");

        Assert.IsType<EntityTypeBuilder<Gunter>>(returnedBuilder);

        var model = builder.Model;
        var entityType = model.FindEntityType(typeof(Gunter));

        Assert.Equal("V2.Annotation", entityType["Annotation"]);
        Assert.Equal("V2.Metadata", entityType["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_generic_entity_builder_extension()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity<Gunter>()
            .GenericEntityBuilderExtension("V1")
            .GenericEntityBuilderExtension("V2");

        Assert.IsType<EntityTypeBuilder<Gunter>>(returnedBuilder);

        var model = builder.Model;
        var entityType = model.FindEntityType(typeof(Gunter));

        Assert.Equal("V2.Annotation", entityType["Annotation"]);
        Assert.Equal("V2.Metadata", entityType["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_key_builder_extension()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity<Gunter>()
            .HasKey(e => e.Id)
            .KeyBuilderExtension("V1")
            .KeyBuilderExtension("V2");

        Assert.IsType<KeyBuilder<Gunter>>(returnedBuilder);

        var model = builder.Model;
        var key = model.FindEntityType(typeof(Gunter)).FindPrimaryKey();

        Assert.Equal("V2.Annotation", key["Annotation"]);
        Assert.Equal("V2.Metadata", key["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_property_builder_extension()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity<Gunter>()
            .Property(e => e.Id)
            .PropertyBuilderExtension("V1")
            .PropertyBuilderExtension("V2");

        Assert.IsType<PropertyBuilder<int>>(returnedBuilder);

        var model = builder.Model;
        var property = model.FindEntityType(typeof(Gunter)).FindProperty("Id");

        Assert.Equal("V2.Annotation", property["Annotation"]);
        Assert.Equal("V2.Metadata", property["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_index_builder_extension()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity<Gunter>()
            .HasIndex(e => e.Id)
            .IndexBuilderExtension("V1")
            .IndexBuilderExtension("V2");

        Assert.IsType<IndexBuilder<Gunter>>(returnedBuilder);

        var model = builder.Model;
        var index = model.FindEntityType(typeof(Gunter)).GetIndexes().Single(i => i.Properties.All(p => p.Name == nameof(Gunter.Id)));

        Assert.Equal("V2.Annotation", index["Annotation"]);
        Assert.Equal("V2.Metadata", index["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_one_to_many_builder_extension()
    {
        var builder = CreateModelBuilder();

        var relationshipBuilder = builder
            .Entity<Gunter>().HasMany(e => e.Gates).WithOne(e => e.Gunter);

        var returnedBuilder = relationshipBuilder
            .OneToManyBuilderExtension("V1")
            .OneToManyBuilderExtension("V2");

        Assert.IsType(relationshipBuilder.GetType(), returnedBuilder);

        var model = builder.Model;
        var foreignKey = model.FindEntityType(typeof(Gate)).GetForeignKeys().Single();

        Assert.Equal("V2.Annotation", foreignKey["Annotation"]);
        Assert.Equal("V2.Metadata", foreignKey["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_many_to_one_builder_extension()
    {
        var builder = CreateModelBuilder();

        var relationshipBuilder = builder
            .Entity<Gate>().HasOne(e => e.Gunter).WithMany(e => e.Gates);

        var returnedBuilder = relationshipBuilder
            .ManyToOneBuilderExtension("V1")
            .ManyToOneBuilderExtension("V2");

        Assert.IsType(relationshipBuilder.GetType(), returnedBuilder);

        var model = builder.Model;
        var foreignKey = model.FindEntityType(typeof(Gate)).GetForeignKeys().Single();

        Assert.Equal("V2.Annotation", foreignKey["Annotation"]);
        Assert.Equal("V2.Metadata", foreignKey["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_one_to_one_builder_extension()
    {
        var builder = CreateModelBuilder();

        var relationshipBuilder = builder
            .Entity<Avatar>().HasOne(e => e.Gunter).WithOne(e => e.Avatar)
            .HasPrincipalKey<Gunter>(e => e.Id);

        var returnedBuilder = relationshipBuilder
            .OneToOneBuilderExtension("V1")
            .OneToOneBuilderExtension("V2");

        Assert.IsType(relationshipBuilder.GetType(), returnedBuilder);

        var model = builder.Model;
        var foreignKey = model.FindEntityType(typeof(Avatar)).GetForeignKeys().Single();

        Assert.Equal("V2.Annotation", foreignKey["Annotation"]);
        Assert.Equal("V2.Metadata", foreignKey["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_model_builder_extension_with_common_name()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .SharedNameExtension("V1")
            .SharedNameExtension("V2");

        Assert.IsAssignableFrom<ModelBuilder>(returnedBuilder);

        var model = builder.Model;

        Assert.Equal("V2.Annotation", model["Annotation"]);
        Assert.Equal("V2.Metadata", model["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_entity_builder_extension_with_common_name()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity(typeof(Gunter))
            .SharedNameExtension("V1")
            .SharedNameExtension("V2");

        Assert.IsType<EntityTypeBuilder>(returnedBuilder);

        var model = builder.Model;
        var entityType = model.FindEntityType(typeof(Gunter));

        Assert.Equal("V2.Annotation", entityType["Annotation"]);
        Assert.Equal("V2.Metadata", entityType["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_entity_builder_extension_and_use_with_generic_builder_with_common_name()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity<Gunter>()
            .SharedNameExtension("V1")
            .SharedNameExtension("V2");

        Assert.IsType<EntityTypeBuilder<Gunter>>(returnedBuilder);

        var model = builder.Model;
        var entityType = model.FindEntityType(typeof(Gunter));

        Assert.Equal("V2.Annotation", entityType["Annotation"]);
        Assert.Equal("V2.Metadata", entityType["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_generic_entity_builder_extension_with_common_name()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity<Gunter>()
            .SharedNameExtension("V1")
            .SharedNameExtension("V2");

        Assert.IsType<EntityTypeBuilder<Gunter>>(returnedBuilder);

        var model = builder.Model;
        var entityType = model.FindEntityType(typeof(Gunter));

        Assert.Equal("V2.Annotation", entityType["Annotation"]);
        Assert.Equal("V2.Metadata", entityType["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_key_builder_extension_with_common_name()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity<Gunter>()
            .HasKey(e => e.Id)
            .SharedNameExtension("V1")
            .SharedNameExtension("V2");

        Assert.IsType<KeyBuilder<Gunter>>(returnedBuilder);

        var model = builder.Model;
        var key = model.FindEntityType(typeof(Gunter)).FindPrimaryKey();

        Assert.Equal("V2.Annotation", key["Annotation"]);
        Assert.Equal("V2.Metadata", key["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_property_builder_extension_with_common_name()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity<Gunter>()
            .Property(e => e.Id)
            .SharedNameExtension("V1")
            .SharedNameExtension("V2");

        Assert.IsType<PropertyBuilder<int>>(returnedBuilder);

        var model = builder.Model;
        var property = model.FindEntityType(typeof(Gunter)).FindProperty("Id");

        Assert.Equal("V2.Annotation", property["Annotation"]);
        Assert.Equal("V2.Metadata", property["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_index_builder_extension_with_common_name()
    {
        var builder = CreateModelBuilder();

        var returnedBuilder = builder
            .Entity<Gunter>()
            .HasIndex(e => e.Id)
            .SharedNameExtension("V1")
            .SharedNameExtension("V2");

        Assert.IsType<IndexBuilder<Gunter>>(returnedBuilder);

        var model = builder.Model;
        var index = model.FindEntityType(typeof(Gunter)).GetIndexes().Single(i => i.Properties.All(p => p.Name == nameof(Gunter.Id)));

        Assert.Equal("V2.Annotation", index["Annotation"]);
        Assert.Equal("V2.Metadata", index["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_one_to_many_builder_extension_with_common_name()
    {
        var builder = CreateModelBuilder();

        var relationshipBuilder = builder
            .Entity<Gunter>().HasMany(e => e.Gates).WithOne(e => e.Gunter);

        var returnedBuilder = relationshipBuilder
            .SharedNameExtension("V1")
            .SharedNameExtension("V2");

        Assert.IsType(relationshipBuilder.GetType(), returnedBuilder);

        var model = builder.Model;
        var foreignKey = model.FindEntityType(typeof(Gate)).GetForeignKeys().Single();

        Assert.Equal("V2.Annotation", foreignKey["Annotation"]);
        Assert.Equal("V2.Metadata", foreignKey["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_many_to_one_builder_extension_with_common_name()
    {
        var builder = CreateModelBuilder();

        var relationshipBuilder = builder
            .Entity<Gate>().HasOne(e => e.Gunter).WithMany(e => e.Gates);

        var returnedBuilder = relationshipBuilder
            .SharedNameExtension("V1")
            .SharedNameExtension("V2");

        Assert.IsType(relationshipBuilder.GetType(), returnedBuilder);

        var model = builder.Model;
        var foreignKey = model.FindEntityType(typeof(Gate)).GetForeignKeys().Single();

        Assert.Equal("V2.Annotation", foreignKey["Annotation"]);
        Assert.Equal("V2.Metadata", foreignKey["Metadata"]);
    }

    [ConditionalFact]
    public void Can_write_one_to_one_builder_extension_with_common_name()
    {
        var builder = CreateModelBuilder();

        var relationshipBuilder = builder
            .Entity<Avatar>().HasOne(e => e.Gunter).WithOne(e => e.Avatar)
            .HasPrincipalKey<Gunter>(e => e.Id);

        var returnedBuilder = relationshipBuilder
            .SharedNameExtension("V1")
            .SharedNameExtension("V2");

        Assert.IsType(relationshipBuilder.GetType(), returnedBuilder);

        var model = builder.Model;
        var foreignKey = model.FindEntityType(typeof(Avatar)).GetForeignKeys().Single();

        Assert.Equal("V2.Annotation", foreignKey["Annotation"]);
        Assert.Equal("V2.Metadata", foreignKey["Metadata"]);
    }

    protected virtual ModelBuilder CreateModelBuilder()
        => InMemoryTestHelpers.Instance.CreateConventionBuilder();

    private class Gunter
    {
        public int Id { get; set; }

        public ICollection<Gate> Gates { get; set; }

        public Avatar Avatar { get; set; }
    }

    private class Gate
    {
        public int Id { get; set; }

        public int GunterId { get; set; }
        public Gunter Gunter { get; set; }
    }

    private class Avatar
    {
        public int Id { get; set; }

        public Gunter Gunter { get; set; }
    }
}

internal static class TestExtensions
{
    public static ModelBuilder ModelBuilderExtension(this ModelBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Model["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static EntityTypeBuilder EntityBuilderExtension(this EntityTypeBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static EntityTypeBuilder<TEntity> GenericEntityBuilderExtension<TEntity>(
        this EntityTypeBuilder<TEntity> builder,
        string value)
        where TEntity : class
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static KeyBuilder KeyBuilderExtension(this KeyBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static PropertyBuilder PropertyBuilderExtension(this PropertyBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static IndexBuilder IndexBuilderExtension(this IndexBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static ReferenceCollectionBuilder OneToManyBuilderExtension(this ReferenceCollectionBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static ReferenceCollectionBuilder ManyToOneBuilderExtension(this ReferenceCollectionBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static ReferenceReferenceBuilder OneToOneBuilderExtension(this ReferenceReferenceBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static ModelBuilder SharedNameExtension(this ModelBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Model["Metadata"] = value + ".Metadata";
        builder.Model["Model"] = value + ".Model";

        return builder;
    }

    public static EntityTypeBuilder SharedNameExtension(this EntityTypeBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static EntityTypeBuilder<TEntity> SharedNameExtension<TEntity, TBuilder>(
        this EntityTypeBuilder<TEntity> builder,
        string value)
        where TEntity : class
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static KeyBuilder SharedNameExtension(this KeyBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static PropertyBuilder SharedNameExtension(this PropertyBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static IndexBuilder SharedNameExtension(this IndexBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static ReferenceCollectionBuilder SharedNameExtension(this ReferenceCollectionBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }

    public static ReferenceReferenceBuilder SharedNameExtension(this ReferenceReferenceBuilder builder, string value)
    {
        builder.HasAnnotation("Annotation", value + ".Annotation");
        builder.Metadata["Metadata"] = value + ".Metadata";

        return builder;
    }
}

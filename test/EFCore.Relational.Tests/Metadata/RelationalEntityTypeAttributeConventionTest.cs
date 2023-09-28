// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Metadata;

public class RelationalEntityTypeAttributeConventionTest
{
    [ConditionalFact]
    public void TableAttribute_sets_table_name_and_schema_with_conventional_builder()
    {
        var modelBuilder = CreateConventionalModelBuilder();

        var entityBuilder = modelBuilder.Entity<A>();

        Assert.Equal("MyTable", entityBuilder.Metadata.GetTableName());
        Assert.Equal("MySchema", entityBuilder.Metadata.GetSchema());
    }

    [ConditionalFact]
    public void CommentAttribute_sets_table_comment_with_conventional_builder()
    {
        var modelBuilder = CreateConventionalModelBuilder();

        var entityBuilder = modelBuilder.Entity<A>();

        Assert.Equal("Test table comment", entityBuilder.Metadata.GetComment());
    }

    [ConditionalFact]
    public void TableAttribute_overrides_configuration_from_convention_source()
    {
        var entityBuilder = CreateInternalEntityTypeBuilder<A>();

        entityBuilder.HasAnnotation(RelationalAnnotationNames.TableName, "ConventionalName", ConfigurationSource.Convention);
        entityBuilder.HasAnnotation(RelationalAnnotationNames.Schema, "ConventionalSchema", ConfigurationSource.Convention);

        RunConvention(entityBuilder);

        Assert.Equal("MyTable", entityBuilder.Metadata.GetTableName());
        Assert.Equal("MySchema", entityBuilder.Metadata.GetSchema());
    }

    [ConditionalFact]
    public void TableAttribute_does_not_override_configuration_from_explicit_source()
    {
        var entityBuilder = CreateInternalEntityTypeBuilder<A>();

        entityBuilder.HasAnnotation(RelationalAnnotationNames.TableName, "ExplicitName", ConfigurationSource.Explicit);
        entityBuilder.HasAnnotation(RelationalAnnotationNames.Schema, "ExplicitName", ConfigurationSource.Explicit);

        RunConvention(entityBuilder);

        Assert.Equal("ExplicitName", entityBuilder.Metadata.GetTableName());
        Assert.Equal("ExplicitName", entityBuilder.Metadata.GetSchema());
    }

    [ConditionalFact]
    public void CommentAttribute_overrides_configuration_from_convention_source()
    {
        var entityBuilder = CreateInternalEntityTypeBuilder<A>();

        entityBuilder.HasAnnotation(RelationalAnnotationNames.Comment, "ConventionalComment", ConfigurationSource.Convention);

        RunConvention(entityBuilder);

        Assert.Equal("Test table comment", entityBuilder.Metadata.GetComment());
    }

    [ConditionalFact]
    public void CommentAttribute_does_not_override_configuration_from_explicit_source()
    {
        var entityBuilder = CreateInternalEntityTypeBuilder<A>();

        entityBuilder.HasAnnotation(RelationalAnnotationNames.Comment, "ExplicitName", ConfigurationSource.Explicit);

        RunConvention(entityBuilder);

        Assert.Equal("ExplicitName", entityBuilder.Metadata.GetComment());
    }

    private void RunConvention(InternalEntityTypeBuilder entityTypeBuilder)
    {
        var context = new ConventionContext<IConventionEntityTypeBuilder>(entityTypeBuilder.Metadata.Model.ConventionDispatcher);

        new RelationalTableAttributeConvention(CreateDependencies(), CreateRelationalDependencies())
            .ProcessEntityTypeAdded(entityTypeBuilder, context);

        new RelationalTableCommentAttributeConvention(CreateDependencies(), CreateRelationalDependencies())
            .ProcessEntityTypeAdded(entityTypeBuilder, context);
    }

    private InternalEntityTypeBuilder CreateInternalEntityTypeBuilder<T>()
    {
        var conventionSet = new ConventionSet();
        conventionSet.EntityTypeAddedConventions.Add(
            new PropertyDiscoveryConvention(CreateDependencies()));

        var modelBuilder = new InternalModelBuilder(new Model(conventionSet));

        return modelBuilder.Entity(typeof(T), ConfigurationSource.Explicit);
    }

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => FakeRelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    private RelationalConventionSetBuilderDependencies CreateRelationalDependencies()
        => FakeRelationalTestHelpers.Instance.CreateContextServices().GetRequiredService<RelationalConventionSetBuilderDependencies>();

    protected virtual ModelBuilder CreateConventionalModelBuilder()
        => FakeRelationalTestHelpers.Instance.CreateConventionBuilder();

    [Table("MyTable", Schema = "MySchema")]
    [Comment("Test table comment")]
    private class A
    {
        public int Id { get; set; }

        public string Name { get; set; }
    }
}

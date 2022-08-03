// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

namespace Microsoft.EntityFrameworkCore.Design;

public class AnnotationCodeGeneratorTest
{
    [ConditionalFact]
    public void IsTableExcludedFromMigrations_false_is_handled_by_convention()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity("foo").ToTable("foo");
        var entityType = modelBuilder.Model.GetEntityTypes().Single();

        var annotations = entityType.GetAnnotations().ToDictionary(a => a.Name, a => a);
        CreateGenerator().RemoveAnnotationsHandledByConventions((IEntityType)entityType, annotations);

        Assert.DoesNotContain(RelationalAnnotationNames.IsTableExcludedFromMigrations, annotations.Keys);
    }

    [ConditionalFact]
    public void GenerateFluentApi_IModel_works_with_collation()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.UseCollation("foo");
        var annotations = modelBuilder.Model.GetAnnotations().ToDictionary(a => a.Name, a => a);
        var result = CreateGenerator().GenerateFluentApiCalls((IModel)modelBuilder.Model, annotations).Single();

        Assert.Equal("UseCollation", result.Method);
        Assert.Equal("foo", Assert.Single(result.Arguments));
    }

    [ConditionalFact]
    public void GenerateFluentApi_IProperty_works_with_collation()
    {
        var modelBuilder = CreateModelBuilder();
        modelBuilder.Entity("Blog", x => x.Property<string>("Name").UseCollation("foo"));
        var property = modelBuilder.Model.FindEntityType("Blog").FindProperty("Name");

        var annotations = property.GetAnnotations().ToDictionary(a => a.Name, a => a);
        var result = CreateGenerator().GenerateFluentApiCalls((IProperty)property, annotations).Single();

        Assert.Equal("UseCollation", result.Method);
        Assert.Equal("foo", Assert.Single(result.Arguments));
    }

    private ModelBuilder CreateModelBuilder()
        => FakeRelationalTestHelpers.Instance.CreateConventionBuilder();

    private AnnotationCodeGenerator CreateGenerator()
        => new(
            new AnnotationCodeGeneratorDependencies(
                new TestRelationalTypeMappingSource(
                    TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
                    TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>())));
}

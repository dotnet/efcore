// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Data;
using Microsoft.EntityFrameworkCore.Storage.Internal;

namespace Microsoft.EntityFrameworkCore.Storage;

public class RelationalParameterBuilderTest
{
    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Can_add_type_mapped_parameter_by_type(bool nullable)
    {
        var typeMapper = (IRelationalTypeMappingSource)new TestRelationalTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());
        var typeMapping = typeMapper.FindMapping(nullable ? typeof(int?) : typeof(int));

        var parameterBuilder = new RelationalCommandBuilder(
            new RelationalCommandBuilderDependencies(
                typeMapper,
                new ExceptionDetector()));

        parameterBuilder.AddParameter(
            "InvariantName",
            "Name",
            typeMapping,
            nullable);

        Assert.Equal(1, parameterBuilder.Parameters.Count);

        var parameter = parameterBuilder.Parameters[0] as TypeMappedRelationalParameter;

        Assert.NotNull(parameter);
        Assert.Equal("InvariantName", parameter.InvariantName);
        Assert.Equal("Name", parameter.Name);
        Assert.Equal(typeMapping, parameter.RelationalTypeMapping);
        Assert.Equal(nullable, parameter.IsNullable);
    }

    [ConditionalTheory]
    [InlineData(true)]
    [InlineData(false)]
    public void Can_add_type_mapped_parameter_by_property(bool nullable)
    {
        var typeMapper = new TestRelationalTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        var modelBuilder = FakeRelationalTestHelpers.Instance.CreateConventionBuilder();

        modelBuilder.Entity("MyType").Property<string>("MyProp").IsRequired(!nullable);

        var model = modelBuilder.FinalizeModel(designTime: false, skipValidation: true);

        var property = model.GetEntityTypes().Single().FindProperty("MyProp");

        var parameterBuilder = new RelationalCommandBuilder(
            new RelationalCommandBuilderDependencies(typeMapper, new ExceptionDetector()));

        parameterBuilder.AddParameter(
            "InvariantName",
            "Name",
            property.GetRelationalTypeMapping(),
            property.IsNullable);

        Assert.Equal(1, parameterBuilder.Parameters.Count);

        var parameter = parameterBuilder.Parameters[0] as TypeMappedRelationalParameter;

        Assert.NotNull(parameter);
        Assert.Equal("InvariantName", parameter.InvariantName);
        Assert.Equal("Name", parameter.Name);
        Assert.Equal(property.GetTypeMapping(), parameter.RelationalTypeMapping);
        Assert.Equal(nullable, parameter.IsNullable);
    }

    [ConditionalFact]
    public void Can_add_composite_parameter()
    {
        var typeMapper = new TestRelationalTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        var parameterBuilder = new RelationalCommandBuilder(
            new RelationalCommandBuilderDependencies(
                typeMapper,
                new ExceptionDetector()));

        parameterBuilder.AddCompositeParameter(
            "CompositeInvariant",
            new List<IRelationalParameter>
            {
                new TypeMappedRelationalParameter(
                    "FirstInvariant",
                    "FirstName",
                    new IntTypeMapping("int", DbType.Int32),
                    nullable: false),
                new TypeMappedRelationalParameter(
                    "SecondInvariant",
                    "SecondName",
                    new StringTypeMapping("nvarchar(max)", DbType.String),
                    nullable: true)
            });

        Assert.Equal(1, parameterBuilder.Parameters.Count);

        var parameter = parameterBuilder.Parameters[0] as CompositeRelationalParameter;

        Assert.NotNull(parameter);
        Assert.Equal("CompositeInvariant", parameter.InvariantName);
        Assert.Equal(2, parameter.RelationalParameters.Count);
    }

    [ConditionalFact]
    public void Does_not_add_empty_composite_parameter()
    {
        var typeMapper = new TestRelationalTypeMappingSource(
            TestServiceFactory.Instance.Create<TypeMappingSourceDependencies>(),
            TestServiceFactory.Instance.Create<RelationalTypeMappingSourceDependencies>());

        var parameterBuilder = new RelationalCommandBuilder(
            new RelationalCommandBuilderDependencies(
                typeMapper,
                new ExceptionDetector()));

        parameterBuilder.AddCompositeParameter(
            "CompositeInvariant",
            new List<IRelationalParameter>());

        Assert.Equal(0, parameterBuilder.Parameters.Count);
    }

    public static RelationalTypeMapping GetMapping(
        IRelationalTypeMappingSource typeMappingSource,
        IProperty property)
        => typeMappingSource.FindMapping(property);
}

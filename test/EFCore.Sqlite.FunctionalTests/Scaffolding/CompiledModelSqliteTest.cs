// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Sqlite.Design.Internal;
using NetTopologySuite.Geometries;
using NetTopologySuite;

namespace Microsoft.EntityFrameworkCore.Scaffolding;

[SpatialiteRequired]
public class CompiledModelSqliteTest : CompiledModelRelationalTestBase
{
    protected override void BuildBigModel(ModelBuilder modelBuilder, bool jsonColumns)
    {
        base.BuildBigModel(modelBuilder, jsonColumns);

        modelBuilder.Entity<Data>(
            eb =>
            {
                eb.Property<int>("Id");
                eb.HasKey("Id");

                eb.Property<Point>("Point")
                    .HasSrid(1101);
            });

        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                eb.Property<Point>("Point")
                    .HasColumnType("geometry")
                    .HasDefaultValue(
                        NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0).CreatePoint(new CoordinateZM(0, 0, 0, 0)))
                    .HasConversion<CastingConverter<Point, Point>, CustomValueComparer<Point>, CustomValueComparer<Point>>();
            });
    }

    protected override void AssertBigModel(IModel model, bool jsonColumns)
    {
        base.AssertBigModel(model, jsonColumns);
        var dataEntity = model.FindEntityType(typeof(Data))!;

        Assert.Equal(typeof(Data).FullName, dataEntity.Name);
        Assert.False(dataEntity.HasSharedClrType);
        Assert.False(dataEntity.IsPropertyBag);
        Assert.False(dataEntity.IsOwned());
        Assert.IsType<ConstructorBinding>(dataEntity.ConstructorBinding);
        Assert.Null(dataEntity.FindIndexerPropertyInfo());
        Assert.Equal(ChangeTrackingStrategy.Snapshot, dataEntity.GetChangeTrackingStrategy());
        Assert.Equal("Data", dataEntity.GetTableName());
        Assert.Null(dataEntity.GetSchema());

        var point = dataEntity.FindProperty("Point")!;
        Assert.Equal(typeof(Point), point.ClrType);
        Assert.True(point.IsNullable);
        Assert.Equal(ValueGenerated.Never, point.ValueGenerated);
        Assert.Equal("Point", point.GetColumnName());
        Assert.Equal("POINT", point.GetColumnType());
        Assert.Null(point.GetValueConverter());
        Assert.IsType<GeometryValueComparer<Point>>(point.GetValueComparer());
        Assert.IsType<GeometryValueComparer<Point>>(point.GetKeyValueComparer());
        Assert.Null(point.GetSrid());

        var principalBase = model.FindEntityType(typeof(PrincipalBase))!;
        var pointProperty = principalBase.FindProperty("Point")!;
        Assert.Equal(typeof(Point), pointProperty.ClrType);
        Assert.True(pointProperty.IsNullable);
        Assert.Equal(ValueGenerated.OnAdd, pointProperty.ValueGenerated);
        Assert.Equal("Point", pointProperty.GetColumnName());
        Assert.Equal("geometry", pointProperty.GetColumnType());
        Assert.Equal(0, ((Point)pointProperty.GetDefaultValue()!).SRID);
        Assert.IsType<CastingConverter<Point, Point>>(pointProperty.GetValueConverter());
        Assert.IsType<CustomValueComparer<Point>>(pointProperty.GetValueComparer());
        Assert.IsType<CustomValueComparer<Point>>(pointProperty.GetKeyValueComparer());
        Assert.IsType<CustomValueComparer<Point>>(pointProperty.GetProviderValueComparer());
        Assert.Null(pointProperty[CoreAnnotationNames.PropertyAccessMode]);

    }

    //Sprocs not supported
    public override void ComplexTypes()
    {
    }

    //Not supported
    public override void Sequences()
    {
    }

    //Sprocs not supported
    public override void Tpc_Sprocs()
    {
    }

    protected override TestHelpers TestHelpers => SqliteTestHelpers.Instance;
    protected override ITestStoreFactory TestStoreFactory => SqliteTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        builder = base.AddOptions(builder)
            .ConfigureWarnings(w => w
                .Ignore(SqliteEventId.SchemaConfiguredWarning)
                .Ignore(SqliteEventId.CompositeKeyWithValueGeneration));
        new SqliteDbContextOptionsBuilder(builder).UseNetTopologySuite();
        return builder;
    }

    protected override void AddDesignTimeServices(IServiceCollection services)
        => new SqliteNetTopologySuiteDesignTimeServices().ConfigureDesignTimeServices(services);

    protected override BuildSource AddReferences(BuildSource build, [CallerFilePath] string filePath = "")
    {
        base.AddReferences(build);
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Sqlite"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Sqlite.NetTopologySuite"));
        build.References.Add(BuildReference.ByName("NetTopologySuite"));
        return build;
    }
}

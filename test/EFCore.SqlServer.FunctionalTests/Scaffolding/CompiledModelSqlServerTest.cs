// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Metadata.Internal;
using Microsoft.EntityFrameworkCore.SqlServer.Design.Internal;
using NetTopologySuite.Geometries;
using NetTopologySuite;

namespace Microsoft.EntityFrameworkCore.Scaffolding;

public class CompiledModelSqlServerTest : CompiledModelRelationalTestBase
{
    protected override void BuildBigModel(ModelBuilder modelBuilder, bool jsonColumns)
    {
        base.BuildBigModel(modelBuilder, jsonColumns);

        modelBuilder
            .UseCollation("Latin1_General_CS_AS")
            .UseIdentityColumns(3, 2);

        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                if (!jsonColumns)
                {
                    eb.Property(e => e.Id).UseIdentityColumn(2, 3).ValueGeneratedOnAdd();
                }

                eb.HasKey(e => new { e.Id, e.AlternateId })
                    .IsClustered();

                eb.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.Property(e => e.Details)
                            .IsSparse()
                            .UseCollation("Latin1_General_CI_AI");

                        if (!jsonColumns)
                        {
                            ob.ToTable(
                                "PrincipalBase", "mySchema",
                                t => t.Property("PrincipalBaseId").UseIdentityColumn(2, 3));
                        }
                    });
            });

        modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
            eb =>
            {
                eb.HasMany(e => e.Principals).WithMany(e => (ICollection<PrincipalDerived<DependentBase<byte?>>>)e.Deriveds)
                    .UsingEntity(
                        jb =>
                        {
                            jb.ToTable(tb => tb.IsMemoryOptimized());
                        });
            });

        modelBuilder.Entity<ManyTypes>(
            eb =>
            {
                eb.Property(m => m.CharToStringConverterProperty)
                    .IsFixedLength(true);
            });
    }

    protected override void AssertBigModel(IModel model, bool jsonColumns)
    {
        base.AssertBigModel(model, jsonColumns);
        Assert.Equal(
            new[] { RelationalAnnotationNames.MaxIdentifierLength, SqlServerAnnotationNames.ValueGenerationStrategy },
            model.GetAnnotations().Select(a => a.Name));
        Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, model.GetValueGenerationStrategy());
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => model.GetPropertyAccessMode()).Message);
        Assert.Null(model[SqlServerAnnotationNames.IdentitySeed]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => model.GetIdentitySeed()).Message);
        Assert.Null(model[SqlServerAnnotationNames.IdentityIncrement]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => model.GetIdentityIncrement()).Message);

        var principalBase = model.FindEntityType(typeof(PrincipalBase))!;
        var principalId = principalBase.FindProperty(nameof(PrincipalBase.Id))!;
        Assert.Equal("bigint", principalId.GetColumnType());
        if (jsonColumns)
        {
            Assert.Equal(
                new[] { SqlServerAnnotationNames.ValueGenerationStrategy },
                principalId.GetAnnotations().Select(a => a.Name));
            Assert.Equal(SqlServerValueGenerationStrategy.None, principalId.GetValueGenerationStrategy());
        }
        else
        {
            Assert.Equal(
                new[] { RelationalAnnotationNames.RelationalOverrides, SqlServerAnnotationNames.ValueGenerationStrategy },
                principalId.GetAnnotations().Select(a => a.Name));
            Assert.Equal(SqlServerValueGenerationStrategy.IdentityColumn, principalId.GetValueGenerationStrategy());
        }
        Assert.Null(principalId[SqlServerAnnotationNames.IdentitySeed]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => principalId.GetIdentitySeed()).Message);
        Assert.Null(principalId[SqlServerAnnotationNames.IdentityIncrement]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => principalId.GetIdentityIncrement()).Message);

        var principalKey = principalBase.GetKeys().Last();
        Assert.Null(principalKey[SqlServerAnnotationNames.Clustered]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => principalKey.IsClustered()).Message);

        var referenceOwnedNavigation = principalBase.GetNavigations().Single();
        var referenceOwnedType = referenceOwnedNavigation.TargetEntityType;
        Assert.False(referenceOwnedType.IsMemoryOptimized());

        var principalTable = StoreObjectIdentifier.Create(referenceOwnedType, StoreObjectType.Table)!.Value;
        if (jsonColumns)
        {
            Assert.Equal(
                SqlServerValueGenerationStrategy.None,
                principalId.GetValueGenerationStrategy(principalTable));
        }
        else
        {
            Assert.Equal(
                SqlServerValueGenerationStrategy.IdentityColumn,
                principalId.GetValueGenerationStrategy(principalTable));
        }

        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => principalId.GetIdentityIncrement(principalTable)).Message);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => principalId.GetIdentitySeed(principalTable)).Message);

        var detailsProperty = referenceOwnedType.FindProperty(nameof(OwnedType.Details))!;
        Assert.Null(detailsProperty[SqlServerAnnotationNames.Sparse]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => detailsProperty.IsSparse()).Message);

        var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>))!;
        var ownedCollectionNavigation = principalDerived.GetDeclaredNavigations().Last();
        var collectionOwnedType = ownedCollectionNavigation.TargetEntityType;
        Assert.False(collectionOwnedType.IsMemoryOptimized());

        var derivedSkipNavigation = principalDerived.GetDeclaredSkipNavigations().Single();
        var joinType = derivedSkipNavigation.JoinEntityType;
        Assert.True(joinType.IsMemoryOptimized());

        var rowid = joinType.GetProperties().Single(p => !p.IsForeignKey());
        Assert.Equal("rowversion", rowid.GetColumnType());
        Assert.Equal(SqlServerValueGenerationStrategy.None, rowid.GetValueGenerationStrategy());

        var manyTypesType = model.FindEntityType(typeof(ManyTypes))!;
        var stringProperty = manyTypesType.FindProperty(nameof(ManyTypes.String))!;
        Assert.True(stringProperty.FindRelationalTypeMapping()!.IsUnicode);
        Assert.False(stringProperty.FindRelationalTypeMapping()!.IsFixedLength);
        var charToStringConverterProperty = manyTypesType.FindProperty(nameof(ManyTypes.CharToStringConverterProperty))!;
        Assert.True(charToStringConverterProperty.FindRelationalTypeMapping()!.IsUnicode);
        Assert.True(charToStringConverterProperty.FindRelationalTypeMapping()!.IsFixedLength);

        var dependentNavigation = principalDerived.GetDeclaredNavigations().First();
        var dependentBase = dependentNavigation.TargetEntityType;
        var dependentDerived = dependentBase.GetDerivedTypes().Single();

        var dependentData = dependentDerived.GetDeclaredProperties().First();
        Assert.Equal("char(20)", dependentData.GetColumnType());

        var dependentMoney = dependentDerived.GetDeclaredProperties().Last();
        Assert.Equal("decimal(9,3)", dependentMoney.GetColumnType());
        Assert.Equal(
        new[]
            {
                dependentBase,
                dependentDerived,
                manyTypesType,
                principalBase,
                referenceOwnedType,
                principalDerived,
                collectionOwnedType,
                joinType
            },
            model.GetEntityTypes());
    }

    protected override bool UseSprocReturnValue => true;

    protected override void BuildTpcSprocsModel(ModelBuilder modelBuilder)
    {
        base.BuildTpcSprocsModel(modelBuilder);

        modelBuilder
            .HasDatabaseMaxSize("20TB")
            .HasPerformanceLevel("High")
            .HasServiceTier("AB");

        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                eb.ToTable("PrincipalBase");

                eb.HasIndex(["PrincipalBaseId"], "PrincipalIndex")
                    .IsUnique()
                    .IsClustered()
                    .IsCreatedOnline()
                    .HasFillFactor(40)
                    .IncludeProperties(e => e.Id)
                    .SortInTempDb()
                    .UseDataCompression(DataCompressionType.Page);
            });
    }

    protected override void AssertTpcSprocs(IModel model)
    {
        base.AssertTpcSprocs(model);

        Assert.Null(model[SqlServerAnnotationNames.MaxDatabaseSize]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(model.GetDatabaseMaxSize).Message);
        Assert.Null(model[SqlServerAnnotationNames.PerformanceLevelSql]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(model.GetPerformanceLevelSql).Message);
        Assert.Null(model[SqlServerAnnotationNames.ServiceTierSql]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(model.GetServiceTierSql).Message);

        var principalBase = model.FindEntityType(typeof(PrincipalBase))!;

        var id = principalBase.FindProperty("Id")!;

        Assert.Equal("Id", id.GetColumnName());
        Assert.Equal("PrincipalBase", principalBase.GetTableName());
        Assert.Equal("TPC", principalBase.GetSchema());
        Assert.Equal("Id", id.GetColumnName(StoreObjectIdentifier.Create(principalBase, StoreObjectType.Table)!.Value));
        Assert.Null(id.FindOverrides(StoreObjectIdentifier.Create(principalBase, StoreObjectType.Table)!.Value));

        var principalBaseId = principalBase.FindProperty("PrincipalBaseId")!;

        var alternateIndex = principalBase.GetIndexes().Last();
        Assert.Same(principalBaseId, alternateIndex.Properties.Single());
        Assert.True(alternateIndex.IsUnique);
        Assert.Null(alternateIndex[SqlServerAnnotationNames.Clustered]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => alternateIndex.IsClustered()).Message);
        Assert.Null(alternateIndex[SqlServerAnnotationNames.CreatedOnline]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => alternateIndex.IsCreatedOnline()).Message);
        Assert.Null(alternateIndex[SqlServerAnnotationNames.FillFactor]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => alternateIndex.GetFillFactor()).Message);
        Assert.Null(alternateIndex[SqlServerAnnotationNames.Include]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => alternateIndex.GetIncludeProperties()).Message);
        Assert.Null(alternateIndex[SqlServerAnnotationNames.SortInTempDb]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => alternateIndex.GetSortInTempDb()).Message);
        Assert.Null(alternateIndex[SqlServerAnnotationNames.DataCompression]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => alternateIndex.GetDataCompression()).Message);

        Assert.Equal(new[] { alternateIndex }, principalBaseId.GetContainingIndexes());
    }

    protected override void BuildComplexTypesModel(ModelBuilder modelBuilder)
    {
        base.BuildComplexTypesModel(modelBuilder);

        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                eb.ComplexProperty(
                    e => e.Owned, eb =>
                    {
                        eb.Ignore(c => c.Context);

                        eb.Property(c => c.Details)
                            .IsSparse()
                            .UseCollation("Latin1_General_CI_AI");
                    });
            });
    }

    protected override void AssertComplexTypes(IModel model)
    {
        base.AssertComplexTypes(model);

        var principalBase = model.FindEntityType(typeof(PrincipalBase))!;
        var complexProperty = principalBase.GetComplexProperties().Single();
        var complexType = complexProperty.ComplexType;
        var detailsProperty = complexType.FindProperty(nameof(OwnedType.Details))!;
        Assert.Equal(
            new[]
            {
                CoreAnnotationNames.MaxLength,
                CoreAnnotationNames.Precision,
                RelationalAnnotationNames.ColumnName,
                RelationalAnnotationNames.ColumnType,
                CoreAnnotationNames.Scale,
                SqlServerAnnotationNames.ValueGenerationStrategy,
                CoreAnnotationNames.Unicode,
                "foo"
            },
            detailsProperty.GetAnnotations().Select(a => a.Name));

        var dbFunction = model.FindDbFunction("PrincipalBaseTvf")!;
        Assert.Equal("dbo", dbFunction.Schema);

        Assert.Equal(SqlServerValueGenerationStrategy.None, detailsProperty.GetValueGenerationStrategy());
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => detailsProperty.GetIdentitySeed()).Message);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => detailsProperty.GetIdentityIncrement()).Message);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => detailsProperty.IsSparse()).Message);
    }

    [ConditionalFact]
    public virtual void Key_HiLo_sequence()
        => Test(
            modelBuilder => {
                modelBuilder.Entity<Data>(
                    eb =>
                    {
                        eb.Property<int>("Id").UseHiLo("HL", "S");
                        eb.HasKey("Id");
                    });
            },
            model =>
            {
                Assert.Equal(1, model.GetSequences().Count());

                var hiLo = model.FindSequence("HL", "S")!;
                Assert.Same(model, ((IReadOnlySequence)hiLo).Model);
                Assert.Equal("HL", hiLo.Name);
                Assert.Equal("S", hiLo.Schema);
                Assert.False(hiLo.IsCyclic);
                Assert.Equal(1, hiLo.StartValue);
                Assert.Null(hiLo.MinValue);
                Assert.Null(hiLo.MaxValue);
                Assert.Equal(10, hiLo.IncrementBy);
                Assert.NotNull(hiLo.ToString());

                Assert.Single(model.GetEntityTypes());
                var dataEntity = model.FindEntityType(typeof(Data))!;
                Assert.Same(hiLo, dataEntity.FindPrimaryKey()!.Properties.Single().FindHiLoSequence());
            });

    [ConditionalFact]
    public virtual void Key_sequence()
        => Test(
            modelBuilder => modelBuilder.Entity<Data>(
                eb =>
                {
                    eb.Property<int>("Id").UseSequence("KeySeq", "KeySeqSchema");
                    eb.HasKey("Id");
                }),
            model =>
            {
                Assert.Single(model.GetSequences());

                var keySequence = model.FindSequence("KeySeq", "KeySeqSchema")!;
                Assert.Same(model, ((IReadOnlySequence)keySequence).Model);
                Assert.Equal("KeySeq", keySequence.Name);
                Assert.Equal("KeySeqSchema", keySequence.Schema);
                Assert.False(keySequence.IsCyclic);
                Assert.Equal(1, keySequence.StartValue);
                Assert.Null(keySequence.MinValue);
                Assert.Null(keySequence.MaxValue);
                Assert.Equal(1, keySequence.IncrementBy);
                Assert.NotNull(keySequence.ToString());

                Assert.Single(model.GetEntityTypes());
                var dataEntity = model.FindEntityType(typeof(Data));
                Assert.Same(keySequence, dataEntity!.FindPrimaryKey()!.Properties.Single().FindSequence());
            });

    [ConditionalFact]
    [SqlServerCondition(SqlServerCondition.SupportsSqlClr)]
    public virtual void SpatialTypesTest()
        => Test(
            modelBuilder => modelBuilder.Entity<SpatialTypes>(
                eb =>
                {
                    eb.Property<Point>("Point")
                        .HasColumnType("geometry")
                        .HasDefaultValue(
                            NtsGeometryServices.Instance.CreateGeometryFactory(srid: 0).CreatePoint(new CoordinateZM(0, 0, 0, 0)))
                        .HasConversion<CastingConverter<Point, Point>, CustomValueComparer<Point>, CustomValueComparer<Point>>();
                }),
            model =>
            {
                var entityType = model.FindEntityType(typeof(SpatialTypes))!;
                var pointProperty = entityType.FindProperty("Point")!;
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
                Assert.Equal(SqlServerValueGenerationStrategy.None, pointProperty.GetValueGenerationStrategy());
                Assert.Null(pointProperty[CoreAnnotationNames.PropertyAccessMode]);
            },
            options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true });

    protected override TestHelpers TestHelpers => SqlServerTestHelpers.Instance;
    protected override ITestStoreFactory TestStoreFactory => SqlServerTestStoreFactory.Instance;

    protected override DbContextOptionsBuilder AddOptions(DbContextOptionsBuilder builder)
    {
        builder = base.AddOptions(builder)
            .ConfigureWarnings(w => w.Ignore(SqlServerEventId.DecimalTypeDefaultWarning));
        new SqlServerDbContextOptionsBuilder(builder).UseNetTopologySuite();
        return builder;
    }

    protected override void AddDesignTimeServices(IServiceCollection services)
        => new SqlServerNetTopologySuiteDesignTimeServices().ConfigureDesignTimeServices(services);

    protected override BuildSource AddReferences(BuildSource build, [CallerFilePath] string filePath = "")
    {
        base.AddReferences(build);
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.SqlServer.NetTopologySuite"));
        build.References.Add(BuildReference.ByName("NetTopologySuite"));
        return build;
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Scaffolding;

public class CompiledModelCosmosTest : CompiledModelTestBase
{
    [ConditionalFact]
    public virtual Task Basic_cosmos_model()
        => Test(
            modelBuilder =>
            {
                modelBuilder.Model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);

                modelBuilder.HasDefaultContainer("Default");

                modelBuilder.Entity<Data>(
                    eb =>
                    {
                        eb.Property<int>("Id");
                        eb.Property<long?>("PartitionId").HasConversion<string>();
                        eb.HasPartitionKey("PartitionId");
                        eb.HasKey("Id", "PartitionId");
                        eb.ToContainer("DataContainer");
                        eb.Property<Dictionary<string, string[]>>("Map");
                        eb.Property<List<Dictionary<string, int>>>("List");
                        eb.Property<ReadOnlyMemory<byte>>("Bytes");
                        eb.UseETagConcurrency();
                        eb.HasNoDiscriminator();
                        eb.Property(d => d.Blob).ToJsonProperty("JsonBlob");
                    });
            },
            model =>
            {
                Assert.Single(model.GetEntityTypes());
                var dataEntity = model.FindEntityType(typeof(Data))!;
                Assert.Equal(typeof(Data).FullName, dataEntity.Name);
                Assert.False(dataEntity.HasSharedClrType);
                Assert.False(dataEntity.IsPropertyBag);
                Assert.False(dataEntity.IsOwned());
                Assert.IsType<ConstructorBinding>(dataEntity.ConstructorBinding);
                Assert.Null(dataEntity.FindIndexerPropertyInfo());
                Assert.Equal(ChangeTrackingStrategy.Snapshot, dataEntity.GetChangeTrackingStrategy());
                Assert.Equal("DataContainer", dataEntity.GetContainer());
                Assert.Null(dataEntity.FindDiscriminatorProperty());

                var id = dataEntity.FindProperty("Id")!;
                Assert.Equal(typeof(int), id.ClrType);
                Assert.Null(id.PropertyInfo);
                Assert.Null(id.FieldInfo);
                Assert.False(id.IsNullable);
                Assert.False(id.IsConcurrencyToken);
                Assert.Equal(ValueGenerated.Never, id.ValueGenerated);
                Assert.Equal(PropertySaveBehavior.Throw, id.GetAfterSaveBehavior());
                Assert.Equal(PropertySaveBehavior.Save, id.GetBeforeSaveBehavior());
                Assert.Equal("Id", id.GetJsonPropertyName());
                Assert.Null(id.GetValueGeneratorFactory());
                Assert.Null(id.GetValueConverter());
                Assert.NotNull(id.GetValueComparer());
                Assert.NotNull(id.GetKeyValueComparer());

                var storeId = dataEntity.FindProperty("__id")!;
                Assert.Equal(typeof(string), storeId.ClrType);
                Assert.Null(storeId.PropertyInfo);
                Assert.Null(storeId.FieldInfo);
                Assert.False(storeId.IsNullable);
                Assert.False(storeId.IsConcurrencyToken);
                Assert.Equal(ValueGenerated.Never, storeId.ValueGenerated);
                Assert.Equal(PropertySaveBehavior.Throw, storeId.GetAfterSaveBehavior());
                Assert.Equal(PropertySaveBehavior.Save, storeId.GetBeforeSaveBehavior());
                Assert.Equal("id", storeId.GetJsonPropertyName());
                Assert.IsType<IdValueGenerator>(storeId.GetValueGeneratorFactory()!(storeId, dataEntity));
                Assert.Null(storeId.GetValueConverter());
                Assert.NotNull(storeId.GetValueComparer());
                Assert.NotNull(storeId.GetKeyValueComparer());

                var partitionId = dataEntity.FindProperty("PartitionId")!;
                Assert.Equal(typeof(long?), partitionId.ClrType);
                Assert.Null(partitionId.PropertyInfo);
                Assert.Null(partitionId.FieldInfo);
                Assert.False(partitionId.IsNullable);
                Assert.False(partitionId.IsConcurrencyToken);
                Assert.Equal(ValueGenerated.Never, partitionId.ValueGenerated);
                Assert.Equal(PropertySaveBehavior.Throw, partitionId.GetAfterSaveBehavior());
                Assert.Equal(PropertySaveBehavior.Save, partitionId.GetBeforeSaveBehavior());
                Assert.Equal("PartitionId", partitionId.GetJsonPropertyName());
                Assert.Null(partitionId.GetValueGeneratorFactory());
                Assert.Null(partitionId.GetValueConverter());
                Assert.Equal("1", partitionId.FindTypeMapping()!.Converter!.ConvertToProvider(1));
                Assert.NotNull(partitionId.GetValueComparer());
                Assert.NotNull(partitionId.GetKeyValueComparer());

                var map = dataEntity.FindProperty("Map")!;
                Assert.Equal(typeof(Dictionary<string, string[]>), map.ClrType);
                Assert.Null(map.PropertyInfo);
                Assert.Null(map.FieldInfo);
                Assert.True(map.IsNullable);
                Assert.False(map.IsConcurrencyToken);
                Assert.False(map.IsPrimitiveCollection);
                Assert.Equal(ValueGenerated.Never, map.ValueGenerated);
                Assert.Equal(PropertySaveBehavior.Save, map.GetAfterSaveBehavior());
                Assert.Equal(PropertySaveBehavior.Save, map.GetBeforeSaveBehavior());
                Assert.Equal("Map", map.GetJsonPropertyName());
                Assert.Null(map.GetValueGeneratorFactory());
                Assert.Null(map.GetValueConverter());
                Assert.NotNull(map.GetValueComparer());
                Assert.NotNull(map.GetKeyValueComparer());

                var list = dataEntity.FindProperty("List")!;
                Assert.Equal(typeof(List<Dictionary<string, int>>), list.ClrType);
                Assert.Null(list.PropertyInfo);
                Assert.Null(list.FieldInfo);
                Assert.True(list.IsNullable);
                Assert.False(list.IsConcurrencyToken);
                Assert.False(list.IsPrimitiveCollection);
                Assert.Equal(ValueGenerated.Never, list.ValueGenerated);
                Assert.Equal(PropertySaveBehavior.Save, list.GetAfterSaveBehavior());
                Assert.Equal(PropertySaveBehavior.Save, list.GetBeforeSaveBehavior());
                Assert.Equal("List", list.GetJsonPropertyName());
                Assert.Null(list.GetValueGeneratorFactory());
                Assert.Null(list.GetValueConverter());
                Assert.NotNull(list.GetValueComparer());
                Assert.NotNull(list.GetKeyValueComparer());

                var bytes = dataEntity.FindProperty("Bytes")!;
                Assert.Equal(typeof(ReadOnlyMemory<byte>), bytes.ClrType);
                Assert.Null(bytes.PropertyInfo);
                Assert.Null(bytes.FieldInfo);
                Assert.False(bytes.IsNullable);
                Assert.False(bytes.IsConcurrencyToken);
                Assert.False(bytes.IsPrimitiveCollection);
                Assert.Equal(ValueGenerated.Never, bytes.ValueGenerated);
                Assert.Equal(PropertySaveBehavior.Save, bytes.GetAfterSaveBehavior());
                Assert.Equal(PropertySaveBehavior.Save, bytes.GetBeforeSaveBehavior());
                Assert.Equal("Bytes", bytes.GetJsonPropertyName());
                Assert.Null(bytes.GetValueGeneratorFactory());
                Assert.Null(bytes.GetValueConverter());
                Assert.NotNull(bytes.GetValueComparer());
                Assert.NotNull(bytes.GetKeyValueComparer());

                var eTag = dataEntity.FindProperty("_etag")!;
                Assert.Equal(typeof(string), eTag.ClrType);
                Assert.Null(eTag.PropertyInfo);
                Assert.Null(eTag.FieldInfo);
                Assert.True(eTag.IsNullable);
                Assert.True(eTag.IsConcurrencyToken);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, eTag.ValueGenerated);
                Assert.Equal(PropertySaveBehavior.Ignore, eTag.GetAfterSaveBehavior());
                Assert.Equal(PropertySaveBehavior.Ignore, eTag.GetBeforeSaveBehavior());
                Assert.Equal("_etag", eTag.GetJsonPropertyName());
                Assert.Null(eTag.GetValueGeneratorFactory());
                Assert.Null(eTag.GetValueConverter());
                Assert.NotNull(eTag.GetValueComparer());
                Assert.NotNull(eTag.GetKeyValueComparer());
                Assert.Equal("_etag", dataEntity.GetETagPropertyName());
                Assert.Same(eTag, dataEntity.GetETagProperty());

                var blob = dataEntity.FindProperty(nameof(Data.Blob))!;
                Assert.Equal(typeof(byte[]), blob.ClrType);
                Assert.Equal(nameof(Data.Blob), blob.PropertyInfo!.Name);
                Assert.Equal("<Blob>k__BackingField", blob.FieldInfo!.Name);
                Assert.True(blob.IsNullable);
                Assert.False(blob.IsConcurrencyToken);
                Assert.Equal(ValueGenerated.Never, blob.ValueGenerated);
                Assert.Equal(PropertySaveBehavior.Save, blob.GetAfterSaveBehavior());
                Assert.Equal(PropertySaveBehavior.Save, blob.GetBeforeSaveBehavior());
                Assert.Equal("JsonBlob", blob.GetJsonPropertyName());
                Assert.Null(blob.GetValueGeneratorFactory());
                Assert.Null(blob.GetValueConverter());
                Assert.NotNull(blob.GetValueComparer());
                Assert.NotNull(blob.GetKeyValueComparer());

                var jObject = dataEntity.FindProperty("__jObject")!;
                Assert.Equal(typeof(JObject), jObject.ClrType);
                Assert.Null(jObject.PropertyInfo);
                Assert.Null(jObject.FieldInfo);
                Assert.True(jObject.IsNullable);
                Assert.False(jObject.IsConcurrencyToken);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, jObject.ValueGenerated);
                Assert.Equal(PropertySaveBehavior.Ignore, jObject.GetAfterSaveBehavior());
                Assert.Equal(PropertySaveBehavior.Ignore, jObject.GetBeforeSaveBehavior());
                Assert.Equal("", jObject.GetJsonPropertyName());
                Assert.Null(jObject.GetValueGeneratorFactory());
                Assert.Null(jObject.GetValueConverter());
                Assert.NotNull(jObject.GetValueComparer());
                Assert.NotNull(jObject.GetKeyValueComparer());

                Assert.Equal(1, dataEntity.GetKeys().Count());

                Assert.Equal([id, partitionId, blob, bytes, list, map, storeId, jObject, eTag], dataEntity.GetProperties());
            });

    protected override void BuildBigModel(ModelBuilder modelBuilder, bool jsonColumns)
    {
        base.BuildBigModel(modelBuilder, jsonColumns);

        modelBuilder.Entity<DependentBase<byte?>>(
            eb => eb.ToContainer("Dependents"));
        modelBuilder.Entity<DependentDerived<byte?>>(
            eb => eb.HasDiscriminator().IsComplete(false));

        modelBuilder.Entity<PrincipalBase>(
            b =>
            {
                // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
                b.Ignore(e => e.RefTypeList);
                b.Ignore(e => e.RefTypeArray);
                b.OwnsOne(
                    e => e.Owned, b =>
                    {
                        b.Ignore(e => e.RefTypeArray);
                        b.Ignore(e => e.RefTypeList);
                    });
            });

        modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
            b =>
            {
                // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
                b.OwnsMany(
                    typeof(OwnedType).FullName!, "ManyOwned", b =>
                    {
                        b.Ignore("RefTypeArray");
                        b.Ignore("RefTypeList");
                    });
            });

        modelBuilder.Entity<ManyTypes>(
            b =>
            {
                // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
                b.Ignore(e => e.GuidArray);
                b.Ignore(e => e.DateTimeArray);
                b.Ignore(e => e.DateOnlyArray);
                b.Ignore(e => e.TimeOnlyArray);
                b.Ignore(e => e.TimeSpanArray);
                b.Ignore(e => e.BytesArray);
                b.Ignore(e => e.UriArray);
                b.Ignore(e => e.IPAddressArray);
                b.Ignore(e => e.PhysicalAddressArray);
                b.Ignore(e => e.NullableGuidArray);
                b.Ignore(e => e.NullableDateTimeArray);
                b.Ignore(e => e.NullableDateOnlyArray);
                b.Ignore(e => e.NullableTimeOnlyArray);
                b.Ignore(e => e.NullableTimeSpanArray);
                b.Ignore(e => e.NullableBytesArray);
                b.Ignore(e => e.NullableUriArray);
                b.Ignore(e => e.NullableIPAddressArray);
                b.Ignore(e => e.NullablePhysicalAddressArray);
                b.Ignore(e => e.Enum8Collection);
                b.Ignore(e => e.Enum16Collection);
                b.Ignore(e => e.Enum32Collection);
                b.Ignore(e => e.Enum64Collection);
                b.Ignore(e => e.EnumU8Collection);
                b.Ignore(e => e.EnumU16Collection);
                b.Ignore(e => e.EnumU32Collection);
                b.Ignore(e => e.EnumU64Collection);
                b.Ignore(e => e.Enum8AsStringCollection);
                b.Ignore(e => e.Enum16AsStringCollection);
                b.Ignore(e => e.Enum32AsStringCollection);
                b.Ignore(e => e.Enum64AsStringCollection);
                b.Ignore(e => e.EnumU8AsStringCollection);
                b.Ignore(e => e.EnumU16AsStringCollection);
                b.Ignore(e => e.EnumU32AsStringCollection);
                b.Ignore(e => e.EnumU64AsStringCollection);
                b.Ignore(e => e.NullableEnum8Collection);
                b.Ignore(e => e.NullableEnum16Collection);
                b.Ignore(e => e.NullableEnum32Collection);
                b.Ignore(e => e.NullableEnum64Collection);
                b.Ignore(e => e.NullableEnumU8Collection);
                b.Ignore(e => e.NullableEnumU16Collection);
                b.Ignore(e => e.NullableEnumU32Collection);
                b.Ignore(e => e.NullableEnumU64Collection);
                b.Ignore(e => e.NullableEnum8AsStringCollection);
                b.Ignore(e => e.NullableEnum16AsStringCollection);
                b.Ignore(e => e.NullableEnum32AsStringCollection);
                b.Ignore(e => e.NullableEnum64AsStringCollection);
                b.Ignore(e => e.NullableEnumU8AsStringCollection);
                b.Ignore(e => e.NullableEnumU16AsStringCollection);
                b.Ignore(e => e.NullableEnumU32AsStringCollection);
                b.Ignore(e => e.NullableEnumU64AsStringCollection);
                b.Ignore(e => e.Enum8Array);
                b.Ignore(e => e.Enum16Array);
                b.Ignore(e => e.Enum32Array);
                b.Ignore(e => e.Enum64Array);
                b.Ignore(e => e.EnumU8Array);
                b.Ignore(e => e.EnumU16Array);
                b.Ignore(e => e.EnumU32Array);
                b.Ignore(e => e.EnumU64Array);
                b.Ignore(e => e.Enum8AsStringArray);
                b.Ignore(e => e.Enum16AsStringArray);
                b.Ignore(e => e.Enum32AsStringArray);
                b.Ignore(e => e.Enum64AsStringArray);
                b.Ignore(e => e.EnumU8AsStringArray);
                b.Ignore(e => e.EnumU16AsStringArray);
                b.Ignore(e => e.EnumU32AsStringArray);
                b.Ignore(e => e.EnumU64AsStringArray);
                b.Ignore(e => e.NullableEnum8Array);
                b.Ignore(e => e.NullableEnum16Array);
                b.Ignore(e => e.NullableEnum32Array);
                b.Ignore(e => e.NullableEnum64Array);
                b.Ignore(e => e.NullableEnumU8Array);
                b.Ignore(e => e.NullableEnumU16Array);
                b.Ignore(e => e.NullableEnumU32Array);
                b.Ignore(e => e.NullableEnumU64Array);
                b.Ignore(e => e.NullableEnum8AsStringArray);
                b.Ignore(e => e.NullableEnum16AsStringArray);
                b.Ignore(e => e.NullableEnum32AsStringArray);
                b.Ignore(e => e.NullableEnum64AsStringArray);
                b.Ignore(e => e.NullableEnumU8AsStringArray);
                b.Ignore(e => e.NullableEnumU16AsStringArray);
                b.Ignore(e => e.NullableEnumU32AsStringArray);
                b.Ignore(e => e.NullableEnumU64AsStringArray);
                b.Ignore(e => e.BytesNestedCollection);
                b.Ignore(e => e.NullableBytesNestedCollection);
                b.Ignore(e => e.Enum8NestedCollection);
                b.Ignore(e => e.Enum32NestedCollection);
                b.Ignore(e => e.EnumU64NestedCollection);
                b.Ignore(e => e.NullableEnum8NestedCollection);
                b.Ignore(e => e.NullableEnum32NestedCollection);
                b.Ignore(e => e.NullableEnumU64NestedCollection);
                b.Ignore(e => e.NullablePhysicalAddressNestedCollection);
                b.Ignore(e => e.GuidNestedCollection);
                b.Ignore(e => e.NullableGuidNestedCollection);
                b.Ignore(e => e.UInt8NestedCollection);
                b.Ignore(e => e.NullableUInt8NestedCollection);
                b.Ignore(e => e.IPAddressReadOnlyCollection);
            });
    }

    protected override void BuildComplexTypesModel(ModelBuilder modelBuilder)
    {
        base.BuildComplexTypesModel(modelBuilder);

        modelBuilder.Entity<PrincipalBase>(
            b =>
            {
                // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
                b.Ignore(e => e.RefTypeList);
                b.Ignore(e => e.RefTypeArray);
                b.ComplexProperty(
                    c => c.Owned, b =>
                    {
                        b.Ignore(e => e.RefTypeArray);
                        b.Ignore(e => e.RefTypeList);
                        b.ComplexProperty(
                            c => c.Principal, b =>
                            {
                                b.Ignore(e => e.RefTypeList);
                                b.Ignore(e => e.RefTypeArray);
                            });
                    });
            });
    }

    protected override void AssertBigModel(IModel model, bool jsonColumns)
    {
        base.AssertBigModel(model, jsonColumns);

        var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>))!;
        Assert.Equal("PrincipalDerived", principalDerived.GetDiscriminatorValue());
    }

    protected override int ExpectedComplexTypeProperties
        => 12;

    protected override TestHelpers TestHelpers
        => CosmosTestHelpers.Instance;

    protected override ITestStoreFactory TestStoreFactory
        => CosmosTestStoreFactory.Instance;

    protected override BuildSource AddReferences(BuildSource build, [CallerFilePath] string filePath = "")
    {
        base.AddReferences(build);
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Cosmos"));
        build.References.Add(BuildReference.ByName("Newtonsoft.Json"));
        return build;
    }

    protected override Task<(TContext?, IModel?)> Test<TContext>(
        Action<ModelBuilder>? onModelCreating = null,
        Action<IModel>? assertModel = null,
        Func<TContext, Task>? useContext = null,
        Action<DbContextOptionsBuilder>? onConfiguring = null,
        CompiledModelCodeGenerationOptions? options = null,
        Func<IServiceCollection, IServiceCollection>? addServices = null,
        Func<IServiceCollection, IServiceCollection>? addDesignTimeServices = null,
        IEnumerable<ScaffoldedFile>? additionalSourceFiles = null,
        Action<Assembly>? assertAssembly = null,
        string? expectedExceptionMessage = null,
        [CallerMemberName] string testName = "")
        where TContext : class
        => base.Test(
            onModelCreating,
            assertModel,
            useContext,
            b =>
            {
                onConfiguring?.Invoke(b);
                b.ConfigureWarnings(w => w.Ignore(CosmosEventId.NoPartitionKeyDefined));
            },
            options,
            addServices,
            addDesignTimeServices,
            additionalSourceFiles,
            assertAssembly,
            expectedExceptionMessage,
            testName);
}

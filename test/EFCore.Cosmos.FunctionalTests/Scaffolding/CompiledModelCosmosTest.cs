// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using Microsoft.EntityFrameworkCore.Cosmos.ValueGeneration.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Newtonsoft.Json.Linq;

namespace Microsoft.EntityFrameworkCore.Scaffolding;

public class CompiledModelCosmosTest(NonSharedFixture fixture) : CompiledModelTestBase(fixture)
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
                Assert.True(list.IsPrimitiveCollection);
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
                b.Property(e => e.Id).HasConversion<ManyTypesIdConverter>().ValueGeneratedNever();
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

    protected override async Task UseBigModel(DbContext context, bool jsonColumns)
    {
        var principalDerived = new PrincipalDerived<DependentBase<byte?>>
        {
            AlternateId = new Guid(),
            Dependent = new DependentDerived<byte?>(1, "one"),
            Owned = new OwnedType(context)
        };

        var principalId = context.Model.FindEntityType(typeof(PrincipalBase))!.FindProperty(nameof(PrincipalBase.Id))!;
        if (principalId.ValueGenerated == ValueGenerated.Never)
        {
            principalDerived.Id = 10;
        }

        context.Add(principalDerived);

        var types = new ManyTypes()
        {
            Bool = true,
            UInt8 = 1,
            Int16 = 2,
            Int32 = 3,
            Int64 = 4,
            UInt16 = 5,
            UInt32 = 6,
            UInt64 = 7,
            Char = 'a',
            Float = 8.0f,
            Double = 9.0,
            Decimal = 10.0m,
            String = "11",
            Guid = Guid.NewGuid(),
            DateTime = new DateTime(2023, 10, 10, 10, 10, 10),
            DateOnly = new DateOnly(2023, 10, 10),
            TimeOnly = new TimeOnly(10, 10),
            TimeSpan = new TimeSpan(1),
            Bytes = [1, 2, 3],
            Uri = new Uri("https://www.example.com"),
            PhysicalAddress = PhysicalAddress.Parse("00-00-00-00-00-01"),
            IPAddress = IPAddress.Parse("127.0.0.1"),

            NullableBool = true,
            NullableUInt8 = 1,
            NullableInt16 = 2,
            NullableInt32 = 3,
            NullableInt64 = 4,
            NullableUInt16 = 5,
            NullableUInt32 = 6,
            NullableUInt64 = 7,
            NullableChar = 'a',
            NullableFloat = 8.0f,
            NullableDouble = 9.0,
            NullableDecimal = 10.0m,
            NullableString = "11",
            NullableGuid = Guid.NewGuid(),
            NullableDateTime = new DateTime(2023, 10, 10, 10, 10, 10),
            NullableDateOnly = new DateOnly(2023, 10, 10),
            NullableTimeOnly = new TimeOnly(10, 10),
            NullableTimeSpan = new TimeSpan(1),
            NullableBytes = [1, 2, 3],
            NullableUri = new Uri("https://www.example.com"),

            BoolArray = [true],
            Int8Array = [1],
            Int16Array = [2],
            Int32Array = [3],
            Int64Array = [4],
            UInt8Array = [1],
            UInt16Array = [5],
            UInt32Array = [6],
            UInt64Array = [7],
            CharArray = ['a'],
            FloatArray = [8.0f],
            DoubleArray = [9.0],
            DecimalArray = [10.0m],
            StringArray = ["11"],
            GuidArray = [Guid.NewGuid()],
            DateTimeArray = [new DateTime(2023, 10, 10, 10, 10, 10)],
            DateOnlyArray = [new DateOnly(2023, 10, 10)],
            TimeOnlyArray = [new TimeOnly(10, 10)],
            TimeSpanArray = [new TimeSpan(1)],
            BytesArray = [[1, 2, 3]],
            UriArray = [new Uri("https://www.example.com")],
            IPAddressArray = [IPAddress.Parse("127.0.0.1")],
            PhysicalAddressArray = [PhysicalAddress.Parse("00-00-00-00-00-01")],

            NullableBoolArray = [true],
            NullableInt8Array = [1],
            NullableInt16Array = [2],
            NullableInt32Array = [3],
            NullableInt64Array = [4],
            NullableUInt8Array = [1],
            NullableUInt16Array = [5],
            NullableUInt32Array = [6],
            NullableUInt64Array = [7],
            NullableCharArray = ['a'],
            NullableFloatArray = [8.0f],
            NullableDoubleArray = [9.0],
            NullableDecimalArray = [10.0m],
            NullableStringArray = ["11"],
            NullableGuidArray = [Guid.NewGuid()],
            NullableDateTimeArray = [new DateTime(2023, 10, 10, 10, 10, 10)],
            NullableDateOnlyArray = [new DateOnly(2023, 10, 10)],
            NullableTimeOnlyArray = [new TimeOnly(10, 10)],
            NullableTimeSpanArray = [new TimeSpan(1)],
            NullableBytesArray = [[1, 2, 3]],
            NullableUriArray = [new Uri("https://www.example.com")],
            NullableIPAddressArray = [IPAddress.Parse("127.0.0.1")],
            NullablePhysicalAddressArray = [PhysicalAddress.Parse("00-00-00-00-00-01")],

            BoolReadOnlyCollection = [true],
            UInt8ReadOnlyCollection = [1],
            Int32ReadOnlyCollection = [2],
            StringReadOnlyCollection = ["3"],
            IPAddressReadOnlyCollection = [IPAddress.Parse("127.0.0.1")],

            Enum8 = Enum8.One,
            Enum16 = Enum16.One,
            Enum32 = Enum32.One,
            Enum64 = Enum64.One,
            EnumU8 = EnumU8.One,
            EnumU16 = EnumU16.One,
            EnumU32 = EnumU32.One,
            EnumU64 = EnumU64.One,

            Enum8AsString = Enum8.One,
            Enum16AsString = Enum16.One,
            Enum32AsString = Enum32.One,
            Enum64AsString = Enum64.One,
            EnumU8AsString = EnumU8.One,
            EnumU16AsString = EnumU16.One,
            EnumU32AsString = EnumU32.One,
            EnumU64AsString = EnumU64.One,

            Enum8Collection = [Enum8.One],
            Enum16Collection = [Enum16.One],
            Enum32Collection = [Enum32.One],
            Enum64Collection = [Enum64.One],
            EnumU8Collection = [EnumU8.One],
            EnumU16Collection = [EnumU16.One],
            EnumU32Collection = [EnumU32.One],
            EnumU64Collection = [EnumU64.One],

            Enum8AsStringCollection = [Enum8.One],
            Enum16AsStringCollection = [Enum16.One],
            Enum32AsStringCollection = [Enum32.One],
            Enum64AsStringCollection = [Enum64.One],
            EnumU8AsStringCollection = [EnumU8.One],
            EnumU16AsStringCollection = [EnumU16.One],
            EnumU32AsStringCollection = [EnumU32.One],
            EnumU64AsStringCollection = [EnumU64.One],

            NullableEnum8Collection = [Enum8.One],
            NullableEnum16Collection = [Enum16.One],
            NullableEnum32Collection = [Enum32.One],
            NullableEnum64Collection = [Enum64.One],
            NullableEnumU8Collection = [EnumU8.One],
            NullableEnumU16Collection = [EnumU16.One],
            NullableEnumU32Collection = [EnumU32.One],
            NullableEnumU64Collection = [EnumU64.One],

            NullableEnum8AsStringCollection = [Enum8.One],
            NullableEnum16AsStringCollection = [Enum16.One],
            NullableEnum32AsStringCollection = [Enum32.One],
            NullableEnum64AsStringCollection = [Enum64.One],
            NullableEnumU8AsStringCollection = [EnumU8.One],
            NullableEnumU16AsStringCollection = [EnumU16.One],
            NullableEnumU32AsStringCollection = [EnumU32.One],
            NullableEnumU64AsStringCollection = [EnumU64.One],

            Enum8Array = [Enum8.One],
            Enum16Array = [Enum16.One],
            Enum32Array = [Enum32.One],
            Enum64Array = [Enum64.One],
            EnumU8Array = [EnumU8.One],
            EnumU16Array = [EnumU16.One],
            EnumU32Array = [EnumU32.One],
            EnumU64Array = [EnumU64.One],

            Enum8AsStringArray = [Enum8.One],
            Enum16AsStringArray = [Enum16.One],
            Enum32AsStringArray = [Enum32.One],
            Enum64AsStringArray = [Enum64.One],
            EnumU8AsStringArray = [EnumU8.One],
            EnumU16AsStringArray = [EnumU16.One],
            EnumU32AsStringArray = [EnumU32.One],
            EnumU64AsStringArray = [EnumU64.One],

            NullableEnum8Array = [Enum8.One],
            NullableEnum16Array = [Enum16.One],
            NullableEnum32Array = [Enum32.One],
            NullableEnum64Array = [Enum64.One],
            NullableEnumU8Array = [EnumU8.One],
            NullableEnumU16Array = [EnumU16.One],
            NullableEnumU32Array = [EnumU32.One],
            NullableEnumU64Array = [EnumU64.One],

            NullableEnum8AsStringArray = [Enum8.One],
            NullableEnum16AsStringArray = [Enum16.One],
            NullableEnum32AsStringArray = [Enum32.One],
            NullableEnum64AsStringArray = [Enum64.One],
            NullableEnumU8AsStringArray = [EnumU8.One],
            NullableEnumU16AsStringArray = [EnumU16.One],
            NullableEnumU32AsStringArray = [EnumU32.One],
            NullableEnumU64AsStringArray = [EnumU64.One],

            BoolNestedCollection = [[true]],
            UInt8NestedCollection = [[9]],
            Int8NestedCollection = [[[9]]],
            Int32NestedCollection = [[9]],
            Int64NestedCollection = [[[9L]]],
            CharNestedCollection = [['a']],
            StringNestedCollection = [["11"]],
            GuidNestedCollection = [[[Guid.NewGuid()]]],
            BytesNestedCollection = [[[1, 2, 3]]],
            NullableUInt8NestedCollection = [[9]],
            NullableInt32NestedCollection = [[9]],
            NullableInt64NestedCollection = [[[9L]]],
            NullableStringNestedCollection = [["11"]],
            NullableGuidNestedCollection = [[Guid.NewGuid()]],
            NullableBytesNestedCollection = [[[1, 2, 3]]],
            NullablePhysicalAddressNestedCollection = [[[PhysicalAddress.Parse("00-00-00-00-00-01")]]],

            Enum8NestedCollection = [[Enum8.One]],
            Enum32NestedCollection = [[[Enum32.One]]],
            EnumU64NestedCollection = [[EnumU64.One]],
            NullableEnum8NestedCollection = [[Enum8.One]],
            NullableEnum32NestedCollection = [[[Enum32.One]]],
            NullableEnumU64NestedCollection = [[EnumU64.One]],

            BoolToStringConverterProperty = true,
            BoolToTwoValuesConverterProperty = true,
            BoolToZeroOneConverterProperty = true,
            BytesToStringConverterProperty = [1, 2, 3],
            CastingConverterProperty = 1,
            CharToStringConverterProperty = 'a',
            DateOnlyToStringConverterProperty = new DateOnly(2023, 10, 10),
            DateTimeOffsetToBinaryConverterProperty = new DateTimeOffset(2023, 10, 10, 10, 10, 10, TimeSpan.Zero),
            DateTimeOffsetToBytesConverterProperty = new DateTimeOffset(2023, 10, 10, 10, 10, 10, TimeSpan.Zero),
            DateTimeOffsetToStringConverterProperty = new DateTimeOffset(2023, 10, 10, 10, 10, 10, TimeSpan.Zero),
            DateTimeToBinaryConverterProperty = new DateTime(2023, 10, 10, 10, 10, 10),
            DateTimeToStringConverterProperty = new DateTime(2023, 10, 10, 10, 10, 10),
            EnumToNumberConverterProperty = Enum32.One,
            EnumToStringConverterProperty = Enum32.One,
            GuidToBytesConverterProperty = Guid.NewGuid(),
            GuidToStringConverterProperty = Guid.NewGuid(),
            IPAddressToBytesConverterProperty = IPAddress.Parse("127.0.0.1"),
            IPAddressToStringConverterProperty = IPAddress.Parse("127.0.0.1"),
            IntNumberToBytesConverterProperty = 1,
            DecimalNumberToBytesConverterProperty = 1.0m,
            DoubleNumberToBytesConverterProperty = 1.0,
            IntNumberToStringConverterProperty = 1,
            DecimalNumberToStringConverterProperty = 1.0m,
            DoubleNumberToStringConverterProperty = 1.0,
            PhysicalAddressToBytesConverterProperty = PhysicalAddress.Parse("00-00-00-00-00-01"),
            PhysicalAddressToStringConverterProperty = PhysicalAddress.Parse("00-00-00-00-00-01"),
            StringToBoolConverterProperty = "true",
            StringToBytesConverterProperty = "1",
            StringToCharConverterProperty = "a",
            StringToDateOnlyConverterProperty = new DateOnly(2023, 10, 10).ToString(@"yyyy\-MM\-dd"),
            StringToDateTimeConverterProperty = new DateTime(2023, 10, 10, 10, 10, 10).ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFF"),
            StringToDateTimeOffsetConverterProperty = new DateTimeOffset(2023, 10, 10, 10, 10, 10, TimeSpan.FromHours(1))
                .ToString(@"yyyy\-MM\-dd HH\:mm\:ss.FFFFFFFzzz"),
            StringToEnumConverterProperty = "One",
            StringToGuidConverterProperty = Guid.NewGuid().ToString(),
            StringToIntNumberConverterProperty = "1",
            StringToDecimalNumberConverterProperty = "1.0",
            StringToDoubleNumberConverterProperty = "1.0",
            StringToTimeOnlyConverterProperty = new TimeOnly(10, 10).ToString("o"),
            StringToTimeSpanConverterProperty = new TimeSpan(1).ToString("c"),
            StringToUriConverterProperty = "https://www.example.com/",
            TimeOnlyToStringConverterProperty = new TimeOnly(10, 10),
            TimeOnlyToTicksConverterProperty = new TimeOnly(10, 10),
            TimeSpanToStringConverterProperty = new TimeSpan(1),
            TimeSpanToTicksConverterProperty = new TimeSpan(1),
            UriToStringConverterProperty = new Uri("https://www.example.com/"),
            NullIntToNullStringConverterProperty = null
        };

        var manyTypesId = context.Model.FindEntityType(typeof(ManyTypes))!.FindProperty(nameof(ManyTypes.Id))!;
        if (manyTypesId.ValueGenerated == ValueGenerated.Never)
        {
            types.Id = new ManyTypesId(17);
        }

        context.Add(types);

        await context.SaveChangesAsync();

        var principalDerivedFromStore = await context.Set<PrincipalDerived<DependentBase<byte?>>>().IgnoreAutoIncludes().SingleAsync();
        Assert.Equal(principalDerived.AlternateId, principalDerivedFromStore.AlternateId);

        var typesFromStore = await context.Set<ManyTypes>().OrderBy(m => m.Id).FirstAsync();
        AssertEqual(types, typesFromStore, jsonColumns);
    }

    protected override void BuildComplexTypesModel(ModelBuilder modelBuilder)
    {
        base.BuildComplexTypesModel(modelBuilder);

        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                // Cosmos provider cannot map collections of elements with converters. See Issue #34026.
                eb.Ignore(e => e.RefTypeList);
                eb.Ignore(e => e.RefTypeArray);
                eb.ComplexProperty(
                    c => c.Owned, ob =>
                    {
                        ob.Ignore(e => e.RefTypeArray);
                        ob.Ignore(e => e.RefTypeList);
                        ob.ComplexProperty(
                            c => c.Principal, cb =>
                            {
                                cb.Ignore(e => e.RefTypeList);
                                cb.Ignore(e => e.RefTypeArray);
                            });
                    });
            });

        // TODO: Complex collections not supported. Issue #31253
        modelBuilder.Ignore<PrincipalDerived<DependentBase<byte?>>>();

        //modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
        //    eb =>
        //    {
        //        eb.ComplexCollection<IList<OwnedType>, OwnedType>(
        //            "ManyOwned", "OwnedCollection", ob =>
        //            {
        //                ob.Ignore(e => e.RefTypeArray);
        //                ob.Ignore(e => e.RefTypeList);
        //                ob.ComplexProperty(
        //                    o => o.Principal, cb =>
        //                    {
        //                        cb.Ignore(e => e.RefTypeList);
        //                        cb.Ignore(e => e.RefTypeArray);
        //                    });
        //            });
        //        eb.Ignore(p => p.Dependent);
        //        eb.Ignore(p => p.Principals);
        //    });
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

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
    public virtual void Basic_cosmos_model()
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
                Assert.Equal("Id", CosmosPropertyExtensions.GetJsonPropertyName(id));
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
                Assert.Equal("id", CosmosPropertyExtensions.GetJsonPropertyName(storeId));
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
                Assert.Equal("PartitionId", CosmosPropertyExtensions.GetJsonPropertyName(partitionId));
                Assert.Null(partitionId.GetValueGeneratorFactory());
                Assert.Null(partitionId.GetValueConverter());
                Assert.Equal("1", partitionId.FindTypeMapping()!.Converter!.ConvertToProvider(1));
                Assert.NotNull(partitionId.GetValueComparer());
                Assert.NotNull(partitionId.GetKeyValueComparer());

                var eTag = dataEntity.FindProperty("_etag")!;
                Assert.Equal(typeof(string), eTag.ClrType);
                Assert.Null(eTag.PropertyInfo);
                Assert.Null(eTag.FieldInfo);
                Assert.True(eTag.IsNullable);
                Assert.True(eTag.IsConcurrencyToken);
                Assert.Equal(ValueGenerated.OnAddOrUpdate, eTag.ValueGenerated);
                Assert.Equal(PropertySaveBehavior.Ignore, eTag.GetAfterSaveBehavior());
                Assert.Equal(PropertySaveBehavior.Ignore, eTag.GetBeforeSaveBehavior());
                Assert.Equal("_etag", CosmosPropertyExtensions.GetJsonPropertyName(eTag));
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
                Assert.Equal("JsonBlob", CosmosPropertyExtensions.GetJsonPropertyName(blob));
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
                Assert.Equal("", CosmosPropertyExtensions.GetJsonPropertyName(jObject));
                Assert.Null(jObject.GetValueGeneratorFactory());
                Assert.Null(jObject.GetValueConverter());
                Assert.NotNull(jObject.GetValueComparer());
                Assert.NotNull(jObject.GetKeyValueComparer());

                Assert.Equal(2, dataEntity.GetKeys().Count());

                Assert.Equal(new[] { id, partitionId, blob, storeId, jObject, eTag }, dataEntity.GetProperties());
            });

    // Primitive collections not supported yet
    public override void BigModel()
    {
    }

    // Primitive collections not supported yet
    public override void ComplexTypes()
    {
    }

    protected override TestHelpers TestHelpers => CosmosTestHelpers.Instance;
    protected override ITestStoreFactory TestStoreFactory => CosmosTestStoreFactory.Instance;

    protected override BuildSource AddReferences(BuildSource build, [CallerFilePath] string filePath = "")
    {
        base.AddReferences(build);
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Cosmos"));
        build.References.Add(BuildReference.ByName("Newtonsoft.Json"));
        return build;
    }
}

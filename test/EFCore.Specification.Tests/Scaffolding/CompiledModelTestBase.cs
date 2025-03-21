// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming

using System.ComponentModel;
using System.ComponentModel.DataAnnotations.Schema;
using System.Net;
using System.Net.NetworkInformation;
using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.Storage.Json;
using Microsoft.EntityFrameworkCore.ValueGeneration.Internal;

namespace Microsoft.EntityFrameworkCore.Scaffolding;

public abstract class CompiledModelTestBase(NonSharedFixture fixture) : NonSharedModelTestBase(fixture), IClassFixture<NonSharedFixture>
{
    [ConditionalFact]
    public virtual Task SimpleModel()
        => Test(
            modelBuilder =>
            {
                modelBuilder.Ignore<DependentBase<int>>();
                modelBuilder.Entity<DependentDerived<int>>(
                    b =>
                    {
                        b.Ignore(e => e.Principal);
                        b.Property(e => e.Id).ValueGeneratedNever();
                        b.Property<string>("Data");
                    });
            },
            model => Assert.Single(model.GetEntityTypes()),
            async c =>
            {
                c.Add(new DependentDerived<int>(1, "one"));

                await c.SaveChangesAsync();

                var stored = await c.Set<DependentDerived<int>>().SingleAsync();
                Assert.Equal(0, stored.Id);
                Assert.Equal(1, stored.GetId());
                Assert.Equal("one", stored.GetData());
            },
            options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true, ForNativeAot = true },
            additionalSourceFiles:
            [
                new ScaffoldedFile(
                    "DbContextModelStub.cs",
                    """
using Microsoft.EntityFrameworkCore.Metadata;
using static TestNamespace.DbContextModel.Dummy;

namespace TestNamespace
{
    public partial class DbContextModel
    {
        public static IModel GetModel()
            => Instance;

        public static class Dummy
        {
            public static IModel Instance => null!;
        }
    }
}
""")
            ],
            assertAssembly: assembly =>
            {
                var instanceProperty = assembly.GetType("TestNamespace.DbContextModel")!
                    .GetMethod("GetModel", BindingFlags.Public | BindingFlags.Static)!;

                var model = (IModel)instanceProperty.Invoke(null, [])!;
                Assert.NotNull(model);
            });

    [ConditionalFact]
    public virtual Task No_NativeAOT()
        => BigModel(false);

    [ConditionalFact]
    public virtual Task BigModel()
        => BigModel(true);

    protected virtual Task BigModel(bool forNativeAot, [CallerMemberName] string testName = "")
        => Test(
            modelBuilder => BuildBigModel(modelBuilder, jsonColumns: false),
            model => AssertBigModel(model, jsonColumns: false),
            context => UseBigModel(context, jsonColumns: false),
            options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true, ForNativeAot = forNativeAot },
            testName: testName);

    protected virtual void BuildBigModel(ModelBuilder modelBuilder, bool jsonColumns)
    {
        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                eb.Property("FlagsEnum2")
                    .UsePropertyAccessMode(PropertyAccessMode.Property)
                    .HasSentinel(AFlagsEnum.C | AFlagsEnum.B);

                eb.Property(e => e.AlternateId)
                    .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);

                eb.HasKey(e => new { e.Id, e.AlternateId });

                eb.Property(e => e.Id).ValueGeneratedNever();
                eb.HasAlternateKey(e => e.Id);

                eb.Property(e => e.AlternateId).Metadata.SetJsonValueReaderWriterType(
                    jsonColumns
                        ? typeof(MyJsonGuidReaderWriter)
                        : typeof(JsonGuidReaderWriter));

                eb.OwnsOne(
                    e => e.Owned, ob =>
                    {
                        ob.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues);
                        ob.UsePropertyAccessMode(PropertyAccessMode.Field);

                        if (!jsonColumns)
                        {
                            ob.HasData(
                                new
                                {
                                    Number = 10,
                                    PrincipalBaseId = 1L,
                                    PrincipalBaseAlternateId = new Guid()
                                });
                        }
                    });

                eb.Navigation(e => e.Owned).IsRequired().HasField("_ownedField")
                    .UsePropertyAccessMode(PropertyAccessMode.Field);

                if (!jsonColumns)
                {
                    eb.HasData(new PrincipalBase { Id = 1, AlternateId = new Guid() });
                }
            });

        modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
            eb =>
            {
                eb.HasOne(e => e.Dependent).WithOne(e => e.Principal)
                    .HasForeignKey<DependentBase<byte?>>()
                    .OnDelete(DeleteBehavior.ClientNoAction);

                eb.Navigation(e => e.Dependent).AutoInclude().EnableLazyLoading(false);

                eb.OwnsMany(typeof(OwnedType).FullName!, "ManyOwned");

                eb.HasMany(e => e.Principals).WithMany(e => (ICollection<PrincipalDerived<DependentBase<byte?>>>)e.Deriveds)
                    .UsingEntity(
                        jb =>
                        {
                            jb.Property<byte[]>("rowid");
                        });
            });

        modelBuilder.Entity<DependentBase<byte?>>(
            eb =>
            {
                eb.Property<byte?>("Id");

                eb.HasKey("PrincipalId", "PrincipalAlternateId");

                eb.HasOne<PrincipalBase>().WithOne()
                    .HasForeignKey<DependentBase<byte?>>("PrincipalId")
                    .HasPrincipalKey<PrincipalBase>(e => e.Id);

                eb.HasDiscriminator<Enum1>("EnumDiscriminator")
                    .HasValue(Enum1.One)
                    .HasValue<DependentDerived<byte?>>(Enum1.Two)
                    .IsComplete(false);
            });

        modelBuilder.Entity<DependentDerived<byte?>>(
            eb =>
            {
                eb.Property<string>("Data")
                    .HasMaxLength(20)
                    .IsUnicode(false);

                eb.Property<decimal>("Money")
                    .HasPrecision(9, 3);
            });

        modelBuilder.Entity<ManyTypes>(
            b =>
            {
                b.Property(e => e.Id).HasConversion<ManyTypesIdConverter>().ValueGeneratedOnAdd();
                b.HasKey(e => e.Id);

                b.Property(e => e.Enum8AsString).HasConversion<string>();
                b.Property(e => e.Enum16AsString).HasConversion<string>();
                b.Property(e => e.Enum32AsString).HasConversion<string>();
                b.Property(e => e.Enum64AsString).HasConversion<string>();
                b.Property(e => e.EnumU8AsString).HasConversion<string>();
                b.Property(e => e.EnumU16AsString).HasConversion<string>();
                b.Property(e => e.EnumU32AsString).HasConversion<string>();
                b.Property(e => e.EnumU64AsString).HasConversion<string>();

                b.PrimitiveCollection(e => e.Enum8AsStringCollection).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.Enum16AsStringCollection).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.Enum32AsStringCollection).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.Enum64AsStringCollection).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.EnumU8AsStringCollection).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.EnumU16AsStringCollection).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.EnumU32AsStringCollection).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.EnumU64AsStringCollection).ElementType(b => b.HasConversion<string>());

                b.PrimitiveCollection(e => e.Enum8AsStringArray).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.Enum16AsStringArray).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.Enum32AsStringArray).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.Enum64AsStringArray).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.EnumU8AsStringArray).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.EnumU16AsStringArray).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.EnumU32AsStringArray).ElementType(b => b.HasConversion<string>());
                b.PrimitiveCollection(e => e.EnumU64AsStringArray).ElementType(b => b.HasConversion<string>());

                b.Property(e => e.BoolReadOnlyCollection);
                b.Property(e => e.UInt8ReadOnlyCollection).HasField("_uInt8ReadOnlyCollection");
                b.Property(e => e.Int32ReadOnlyCollection);
                b.Property(e => e.StringReadOnlyCollection).HasField("_stringReadOnlyCollection");

                b.PrimitiveCollection(e => e.IPAddressReadOnlyCollection)
                    .ElementType(b => b.HasConversion<string>())
                    .HasField("_ipAddressReadOnlyCollection");

                b.Property(e => e.BoolToStringConverterProperty).HasConversion(new BoolToStringConverter("A", "B"));
                b.Property(e => e.BoolToTwoValuesConverterProperty).HasConversion(new BoolToTwoValuesConverter<byte>(0, 1));
                b.Property(e => e.BoolToZeroOneConverterProperty).HasConversion<BoolToZeroOneConverter<short>>();
                b.Property(e => e.BytesToStringConverterProperty).HasConversion<BytesToStringConverter, ArrayStructuralComparer<byte>>();
                b.Property(e => e.CastingConverterProperty).HasConversion<CastingConverter<int, decimal>>();
                b.Property(e => e.CharToStringConverterProperty).HasConversion<CharToStringConverter>();
                b.Property(e => e.DateOnlyToStringConverterProperty).HasConversion<DateOnlyToStringConverter>();
                b.Property(e => e.DateTimeOffsetToBinaryConverterProperty).HasConversion<DateTimeOffsetToBinaryConverter>();
                b.Property(e => e.DateTimeOffsetToBytesConverterProperty).HasConversion<DateTimeOffsetToBytesConverter>();
                b.Property(e => e.DateTimeOffsetToStringConverterProperty).HasConversion<DateTimeOffsetToStringConverter>();
                b.Property(e => e.DateTimeToBinaryConverterProperty).HasConversion<DateTimeToBinaryConverter>();
                b.Property(e => e.DateTimeToStringConverterProperty).HasConversion<DateTimeToStringConverter>();
                b.Property(e => e.EnumToNumberConverterProperty).HasConversion<EnumToNumberConverter<Enum32, int>>();
                b.Property(e => e.EnumToStringConverterProperty).HasConversion<EnumToStringConverter<Enum32>>();
                b.Property(e => e.GuidToBytesConverterProperty).HasConversion<GuidToBytesConverter>();
                b.Property(e => e.GuidToStringConverterProperty).HasConversion<GuidToStringConverter>();
                b.Property(e => e.IPAddressToBytesConverterProperty).HasConversion<IPAddressToBytesConverter>();
                b.Property(e => e.IPAddressToStringConverterProperty).HasConversion<IPAddressToStringConverter>();
                b.Property(e => e.IntNumberToBytesConverterProperty).HasConversion<NumberToBytesConverter<int>>();
                b.Property(e => e.DecimalNumberToBytesConverterProperty).HasConversion<NumberToBytesConverter<decimal>>();
                b.Property(e => e.DoubleNumberToBytesConverterProperty).HasConversion<NumberToBytesConverter<double>>();
                b.Property(e => e.IntNumberToStringConverterProperty).HasConversion<NumberToStringConverter<int>>();
                b.Property(e => e.DecimalNumberToStringConverterProperty).HasConversion<NumberToStringConverter<decimal>>();
                b.Property(e => e.DoubleNumberToStringConverterProperty).HasConversion<NumberToStringConverter<double>>();
                b.Property(e => e.PhysicalAddressToBytesConverterProperty).HasConversion<PhysicalAddressToBytesConverter>();
                b.Property(e => e.PhysicalAddressToStringConverterProperty).HasConversion<PhysicalAddressToStringConverter>();
                b.Property(e => e.StringToBoolConverterProperty).HasConversion<StringToBoolConverter>();
                b.Property(e => e.StringToBytesConverterProperty).HasConversion(new StringToBytesConverter(Encoding.UTF32));
                b.Property(e => e.StringToCharConverterProperty).HasConversion<StringToCharConverter>();
                b.Property(e => e.StringToDateOnlyConverterProperty).HasConversion<StringToDateOnlyConverter>();
                b.Property(e => e.StringToDateTimeConverterProperty).HasConversion<StringToDateTimeConverter>();
                b.Property(e => e.StringToDateTimeOffsetConverterProperty).HasConversion<StringToDateTimeOffsetConverter>();
                b.Property(e => e.StringToEnumConverterProperty).HasConversion<StringToEnumConverter<EnumU32>>();
                b.Property(e => e.StringToIntNumberConverterProperty).HasConversion<StringToNumberConverter<int>>();
                b.Property(e => e.StringToDecimalNumberConverterProperty).HasConversion<StringToNumberConverter<decimal>>();
                b.Property(e => e.StringToDoubleNumberConverterProperty).HasConversion<StringToNumberConverter<double>>();
                b.Property(e => e.StringToTimeOnlyConverterProperty).HasConversion<StringToTimeOnlyConverter>();
                b.Property(e => e.StringToTimeSpanConverterProperty).HasConversion<StringToTimeSpanConverter>();
                b.Property(e => e.StringToUriConverterProperty).HasConversion<StringToUriConverter>();
                b.Property(e => e.TimeOnlyToStringConverterProperty).HasConversion<TimeOnlyToStringConverter>();
                b.Property(e => e.TimeOnlyToTicksConverterProperty).HasConversion<TimeOnlyToTicksConverter>();
                b.Property(e => e.TimeSpanToStringConverterProperty).HasConversion<TimeSpanToStringConverter>();
                b.Property(e => e.TimeSpanToTicksConverterProperty).HasConversion<TimeSpanToTicksConverter>();
                b.Property(e => e.UriToStringConverterProperty).HasConversion<UriToStringConverter>();
                b.Property(e => e.NullIntToNullStringConverterProperty).HasConversion<NullIntToNullStringConverter>();
            });
    }

    protected virtual void AssertBigModel(IModel model, bool jsonColumns)
    {
        var manyTypesType = model.FindEntityType(typeof(ManyTypes))!;

        Assert.Equal(typeof(ManyTypes).FullName, manyTypesType.Name);
        Assert.False(manyTypesType.HasSharedClrType);
        Assert.False(manyTypesType.IsPropertyBag);
        Assert.False(manyTypesType.IsOwned());
        Assert.IsType<ConstructorBinding>(manyTypesType.ConstructorBinding);
        Assert.Null(manyTypesType.FindIndexerPropertyInfo());
        Assert.Equal(ChangeTrackingStrategy.Snapshot, manyTypesType.GetChangeTrackingStrategy());

        var ipAddressCollection = manyTypesType.FindProperty(nameof(ManyTypes.IPAddressReadOnlyCollection));
        if (ipAddressCollection != null)
        {
            Assert.True(ipAddressCollection.IsPrimitiveCollection);
            var ipAddressElementType = ipAddressCollection.GetElementType()!;
            Assert.Equal(typeof(IPAddress), ipAddressElementType.ClrType);
            Assert.Same(ipAddressCollection, ipAddressElementType.CollectionProperty);
            Assert.Equal(typeof(string), ipAddressElementType.GetProviderClrType());
            Assert.Null(ipAddressElementType.GetMaxLength());
            Assert.Null(ipAddressElementType.GetPrecision());
            Assert.Null(ipAddressElementType.GetScale());
            Assert.Null(ipAddressElementType.IsUnicode());
            Assert.Equal(ipAddressCollection.GetTypeMapping().ElementTypeMapping?.GetType(), ipAddressElementType.GetTypeMapping().GetType());
            Assert.NotNull(ipAddressElementType.GetTypeMapping().Comparer);
            Assert.NotNull(ipAddressElementType.GetTypeMapping().Converter);
            Assert.NotNull(ipAddressElementType.GetTypeMapping().JsonValueReaderWriter);
            Assert.NotNull(ipAddressElementType.GetValueComparer());
            Assert.Null(ipAddressElementType.GetValueConverter());
            Assert.Null(ipAddressElementType.GetJsonValueReaderWriter());
        }

        Assert.Null(model.FindEntityType(typeof(AbstractBase)));
        var principalBase = model.FindEntityType(typeof(PrincipalBase))!;
        Assert.Equal(typeof(PrincipalBase).FullName, principalBase.Name);
        Assert.False(principalBase.HasSharedClrType);
        Assert.False(principalBase.IsPropertyBag);
        Assert.False(principalBase.IsOwned());
        Assert.Null(principalBase.BaseType);
        Assert.IsType<ConstructorBinding>(principalBase.ConstructorBinding);
        Assert.Null(principalBase.FindIndexerPropertyInfo());
        Assert.Equal(ChangeTrackingStrategy.Snapshot, principalBase.GetChangeTrackingStrategy());
        Assert.Null(principalBase.GetQueryFilter());
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => principalBase.GetSeedData()).Message);

        var principalId = principalBase.FindProperty(nameof(PrincipalBase.Id))!;
        Assert.Equal(typeof(long?), principalId.ClrType);
        Assert.Equal(typeof(long?), principalId.PropertyInfo!.PropertyType);
        Assert.Equal(typeof(long?), principalId.FieldInfo!.FieldType);
        Assert.False(principalId.IsNullable);
        Assert.Equal(PropertySaveBehavior.Throw, principalId.GetAfterSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Save, principalId.GetBeforeSaveBehavior());
        Assert.Null(principalId[CoreAnnotationNames.BeforeSaveBehavior]);
        Assert.Null(principalId[CoreAnnotationNames.AfterSaveBehavior]);
        Assert.Null(principalId.GetValueConverter());
        Assert.NotNull(principalId.GetValueComparer());
        Assert.NotNull(principalId.GetKeyValueComparer());

        var principalAlternateId = principalBase.FindProperty(nameof(PrincipalBase.AlternateId))!;

        var principalAlternateKey = principalBase.GetKeys().Single(k => k.Properties.Count == 1 && principalId == k.Properties.Single());
        Assert.False(principalAlternateKey.IsPrimaryKey());

        var principalKey = principalBase.GetKeys()
            .Single(k => k.Properties.Count == 2 && k.Properties.SequenceEqual([principalId, principalAlternateId]));
        Assert.True(principalKey.IsPrimaryKey());

        Assert.Equal([principalAlternateKey, principalKey], principalId.GetContainingKeys());

        var referenceOwnedNavigation = principalBase.GetNavigations().Single();
        Assert.Equal(
            [CoreAnnotationNames.EagerLoaded],
            referenceOwnedNavigation.GetAnnotations().Select(a => a.Name));
        Assert.Equal(nameof(PrincipalBase.Owned), referenceOwnedNavigation.Name);
        Assert.False(referenceOwnedNavigation.IsCollection);
        Assert.True(referenceOwnedNavigation.IsEagerLoaded);
        Assert.False(referenceOwnedNavigation.IsOnDependent);
        Assert.Equal(typeof(OwnedType), referenceOwnedNavigation.ClrType);
        Assert.Equal("_ownedField", referenceOwnedNavigation.FieldInfo!.Name);
        Assert.Equal(nameof(PrincipalBase.Owned), referenceOwnedNavigation.PropertyInfo!.Name);
        Assert.Null(referenceOwnedNavigation.Inverse);
        Assert.Equal(principalBase, referenceOwnedNavigation.DeclaringEntityType);
        Assert.Equal(PropertyAccessMode.Field, referenceOwnedNavigation.GetPropertyAccessMode());
        Assert.Null(referenceOwnedNavigation[CoreAnnotationNames.PropertyAccessMode]);

        var referenceOwnedType = referenceOwnedNavigation.TargetEntityType;
        Assert.Equal(typeof(PrincipalBase).FullName + ".Owned#OwnedType", referenceOwnedType.Name);
        Assert.Equal(typeof(OwnedType), referenceOwnedType.ClrType);
        Assert.True(referenceOwnedType.HasSharedClrType);
        Assert.False(referenceOwnedType.IsPropertyBag);
        Assert.True(referenceOwnedType.IsOwned());
        Assert.Null(referenceOwnedType.BaseType);
        Assert.IsType<ConstructorBinding>(referenceOwnedType.ConstructorBinding);
        Assert.Null(referenceOwnedType.FindIndexerPropertyInfo());
        Assert.Equal(
            ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues,
            referenceOwnedType.GetChangeTrackingStrategy());
        Assert.Null(referenceOwnedType.GetQueryFilter());
        Assert.Null(referenceOwnedType[CoreAnnotationNames.PropertyAccessMode]);
        Assert.Null(referenceOwnedType[CoreAnnotationNames.NavigationAccessMode]);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => referenceOwnedType.GetPropertyAccessMode()).Message);
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => referenceOwnedType.GetNavigationAccessMode()).Message);

        var ownedId = referenceOwnedType.FindProperty("PrincipalBaseId")!;
        Assert.True(ownedId.IsPrimaryKey());

        var referenceOwnership = referenceOwnedNavigation.ForeignKey;
        Assert.Empty(referenceOwnership.GetAnnotations());
        Assert.Same(referenceOwnership, referenceOwnedType.FindOwnership());
        Assert.True(referenceOwnership.IsOwnership);
        Assert.True(referenceOwnership.IsRequired);
        Assert.True(referenceOwnership.IsRequiredDependent);
        Assert.True(referenceOwnership.IsUnique);
        Assert.Null(referenceOwnership.DependentToPrincipal);
        Assert.Same(referenceOwnedNavigation, referenceOwnership.PrincipalToDependent);
        Assert.Equal(DeleteBehavior.Cascade, referenceOwnership.DeleteBehavior);
        Assert.Equal(2, referenceOwnership.Properties.Count());
        Assert.Same(principalKey, referenceOwnership.PrincipalKey);

        var ownedServiceProperty = referenceOwnedType.GetServiceProperties().Single();
        Assert.Empty(ownedServiceProperty.GetAnnotations());
        Assert.Equal(typeof(DbContext), ownedServiceProperty.ClrType);
        Assert.Equal(typeof(DbContext), ownedServiceProperty.PropertyInfo!.PropertyType);
        Assert.Null(ownedServiceProperty.FieldInfo);
        Assert.Same(referenceOwnedType, ownedServiceProperty.DeclaringEntityType);
        var ownedServicePropertyBinding = ownedServiceProperty.ParameterBinding;
        Assert.IsType<ContextParameterBinding>(ownedServicePropertyBinding);
        Assert.Equal(typeof(DbContext), ownedServicePropertyBinding.ServiceType);
        Assert.Equal(ownedServiceProperty, ownedServicePropertyBinding.ConsumedProperties.Single());
        Assert.Equal(PropertyAccessMode.PreferField, ownedServiceProperty.GetPropertyAccessMode());
        Assert.Null(ownedServiceProperty[CoreAnnotationNames.PropertyAccessMode]);

        var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>))!;
        Assert.Equal(principalBase, principalDerived.BaseType);
        Assert.Equal(
            "Microsoft.EntityFrameworkCore.Scaffolding.CompiledModelTestBase+"
            + "PrincipalDerived<Microsoft.EntityFrameworkCore.Scaffolding.CompiledModelTestBase+DependentBase<byte?>>",
            principalDerived.Name);
        Assert.False(principalDerived.IsOwned());
        Assert.IsType<ConstructorBinding>(principalDerived.ConstructorBinding);
        Assert.Equal(ChangeTrackingStrategy.Snapshot, principalDerived.GetChangeTrackingStrategy());

        Assert.Equal(2, principalDerived.GetDeclaredNavigations().Count());
        var dependentNavigation = principalDerived.GetDeclaredNavigations().First();
        Assert.Equal("Dependent", dependentNavigation.Name);
        Assert.Equal("Dependent", dependentNavigation.PropertyInfo!.Name);
        Assert.Equal("<Dependent>k__BackingField", dependentNavigation.FieldInfo!.Name);
        Assert.False(dependentNavigation.IsCollection);
        Assert.True(dependentNavigation.IsEagerLoaded);
        Assert.False(dependentNavigation.LazyLoadingEnabled);
        Assert.False(dependentNavigation.IsOnDependent);
        Assert.Equal(principalDerived, dependentNavigation.DeclaringEntityType);
        Assert.Equal("Principal", dependentNavigation.Inverse!.Name);

        var ownedCollectionNavigation = principalDerived.GetDeclaredNavigations().Last();
        Assert.Equal("ManyOwned", ownedCollectionNavigation.Name);
        Assert.Null(ownedCollectionNavigation.PropertyInfo);
        Assert.Equal("ManyOwned", ownedCollectionNavigation.FieldInfo!.Name);
        Assert.Equal(typeof(ICollection<OwnedType>), ownedCollectionNavigation.ClrType);
        Assert.True(ownedCollectionNavigation.IsCollection);
        Assert.True(ownedCollectionNavigation.IsEagerLoaded);
        Assert.False(ownedCollectionNavigation.IsOnDependent);
        Assert.Null(ownedCollectionNavigation.Inverse);
        Assert.Equal(principalDerived, ownedCollectionNavigation.DeclaringEntityType);

        var collectionOwnedType = ownedCollectionNavigation.TargetEntityType;
        Assert.Equal(principalDerived.Name + ".ManyOwned#OwnedType", collectionOwnedType.Name);
        Assert.Equal(typeof(OwnedType), collectionOwnedType.ClrType);
        Assert.True(collectionOwnedType.HasSharedClrType);
        Assert.False(collectionOwnedType.IsPropertyBag);
        Assert.True(collectionOwnedType.IsOwned());
        Assert.Null(collectionOwnedType.BaseType);
        Assert.IsType<ConstructorBinding>(collectionOwnedType.ConstructorBinding);
        Assert.Equal(ChangeTrackingStrategy.Snapshot, collectionOwnedType.GetChangeTrackingStrategy());

        var collectionOwnership = ownedCollectionNavigation.ForeignKey;
        Assert.Same(collectionOwnership, collectionOwnedType.FindOwnership());
        Assert.True(collectionOwnership.IsOwnership);
        Assert.True(collectionOwnership.IsRequired);
        Assert.False(collectionOwnership.IsRequiredDependent);
        Assert.False(collectionOwnership.IsUnique);
        Assert.Null(collectionOwnership.DependentToPrincipal);
        Assert.Same(ownedCollectionNavigation, collectionOwnership.PrincipalToDependent);
        Assert.Equal(DeleteBehavior.Cascade, collectionOwnership.DeleteBehavior);
        Assert.Equal(2, collectionOwnership.Properties.Count());

        var derivedSkipNavigation = principalDerived.GetDeclaredSkipNavigations().Single();
        Assert.Equal("Principals", derivedSkipNavigation.Name);
        Assert.Equal("Principals", derivedSkipNavigation.PropertyInfo!.Name);
        Assert.Equal("<Principals>k__BackingField", derivedSkipNavigation.FieldInfo!.Name);
        Assert.Equal(typeof(ICollection<PrincipalBase>), derivedSkipNavigation.ClrType);
        Assert.True(derivedSkipNavigation.IsCollection);
        Assert.False(derivedSkipNavigation.IsEagerLoaded);
        Assert.True(derivedSkipNavigation.LazyLoadingEnabled);
        Assert.False(derivedSkipNavigation.IsOnDependent);
        Assert.Equal(principalDerived, derivedSkipNavigation.DeclaringEntityType);
        Assert.Equal("Deriveds", derivedSkipNavigation.Inverse.Name);
        Assert.Same(principalBase.GetSkipNavigations().Single(), derivedSkipNavigation.Inverse);

        Assert.Same(derivedSkipNavigation, derivedSkipNavigation.ForeignKey.GetReferencingSkipNavigations().Single());
        Assert.Same(
            derivedSkipNavigation.Inverse, derivedSkipNavigation.Inverse.ForeignKey.GetReferencingSkipNavigations().Single());

        Assert.Equal([derivedSkipNavigation.Inverse, derivedSkipNavigation], principalDerived.GetSkipNavigations());

        var joinType = derivedSkipNavigation.JoinEntityType;

        Assert.Equal("PrincipalBasePrincipalDerived<DependentBase<byte?>>", joinType.Name);
        Assert.Equal(typeof(Dictionary<string, object>), joinType.ClrType);
        Assert.True(joinType.HasSharedClrType);
        Assert.True(joinType.IsPropertyBag);
        Assert.False(joinType.IsOwned());
        Assert.Null(joinType.BaseType);
        Assert.IsType<ConstructorBinding>(joinType.ConstructorBinding);
        Assert.Equal("Item", joinType.FindIndexerPropertyInfo()!.Name);
        Assert.Equal(ChangeTrackingStrategy.Snapshot, joinType.GetChangeTrackingStrategy());
        Assert.Null(joinType.GetQueryFilter());

        var rowid = joinType.FindProperty("rowid")!;
        Assert.Equal(typeof(byte[]), rowid.ClrType);
        Assert.True(rowid.IsIndexerProperty());
        Assert.Same(joinType.FindIndexerPropertyInfo(), rowid.PropertyInfo);
        Assert.Null(rowid.FieldInfo);
        Assert.True(rowid.IsNullable);
        Assert.False(rowid.IsForeignKey());
        Assert.False(rowid.IsShadowProperty());
        Assert.Null(rowid.GetValueConverter());
        Assert.NotNull(rowid.GetValueComparer());
        Assert.NotNull(rowid.GetKeyValueComparer());

        var dependentForeignKey = dependentNavigation.ForeignKey;
        Assert.False(dependentForeignKey.IsOwnership);
        Assert.True(dependentForeignKey.IsRequired);
        Assert.False(dependentForeignKey.IsRequiredDependent);
        Assert.True(dependentForeignKey.IsUnique);
        Assert.Same(dependentNavigation.Inverse, dependentForeignKey.DependentToPrincipal);
        Assert.Same(dependentNavigation, dependentForeignKey.PrincipalToDependent);
        Assert.Equal(DeleteBehavior.ClientNoAction, dependentForeignKey.DeleteBehavior);
        Assert.Equal(["PrincipalId", "PrincipalAlternateId"], dependentForeignKey.Properties.Select(p => p.Name));
        Assert.Same(principalKey, dependentForeignKey.PrincipalKey);

        var dependentBase = dependentNavigation.TargetEntityType;

        Assert.False(dependentBase.GetIsDiscriminatorMappingComplete());
        var principalDiscriminator = dependentBase.FindDiscriminatorProperty()!;
        Assert.IsType<DiscriminatorValueGenerator>(
            principalDiscriminator.GetValueGeneratorFactory()!(principalDiscriminator, dependentBase));
        Assert.Equal(Enum1.One, dependentBase.GetDiscriminatorValue());

        var dependentBaseForeignKey = dependentBase.GetForeignKeys().Single(fk => fk != dependentForeignKey);
        var dependentForeignKeyProperty = dependentBaseForeignKey.Properties.Single();

        Assert.Equal(
            [dependentBaseForeignKey, dependentForeignKey], dependentForeignKeyProperty.GetContainingForeignKeys());

        var dependentDerived = dependentBase.GetDerivedTypes().Single();
        Assert.Equal(Enum1.Two, dependentDerived.GetDiscriminatorValue());

        Assert.Equal(2, dependentDerived.GetDeclaredProperties().Count());

        var dependentData = dependentDerived.GetDeclaredProperties().First();
        Assert.Equal(typeof(string), dependentData.ClrType);
        Assert.Equal("Data", dependentData.Name);
        Assert.Equal("Data", dependentData.PropertyInfo!.Name);
        Assert.Equal("<Data>k__BackingField", dependentData.FieldInfo!.Name);
        Assert.True(dependentData.IsNullable);
        Assert.False(dependentData.IsShadowProperty());
        Assert.False(dependentData.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.Never, dependentData.ValueGenerated);
        Assert.Equal(20, dependentData.GetMaxLength());
        Assert.False(dependentData.IsUnicode());
        Assert.Null(dependentData.GetPrecision());
        Assert.Null(dependentData.GetScale());

        var dependentMoney = dependentDerived.GetDeclaredProperties().Last();
        Assert.Equal(typeof(decimal), dependentMoney.ClrType);
        Assert.Equal("Money", dependentMoney.Name);
        Assert.Null(dependentMoney.PropertyInfo);
        Assert.Null(dependentMoney.FieldInfo);
        Assert.False(dependentMoney.IsNullable);
        Assert.True(dependentMoney.IsShadowProperty());
        Assert.False(dependentMoney.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.Never, dependentMoney.ValueGenerated);
        Assert.Null(dependentMoney.GetMaxLength());
        Assert.Null(dependentMoney.IsUnicode());
        Assert.Equal(9, dependentMoney.GetPrecision());
        Assert.Equal(3, dependentMoney.GetScale());

        Assert.Equal(
            [derivedSkipNavigation.ForeignKey, collectionOwnership, dependentForeignKey],
            principalDerived.GetDeclaredReferencingForeignKeys());
    }

    protected virtual async Task UseBigModel(DbContext context, bool jsonColumns)
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

        var dependent = context.Set<PrincipalDerived<DependentBase<byte?>>>().Include(p => p.Dependent).Single().Dependent!;
        Assert.Equal("one", ((DependentDerived<byte?>)dependent).GetData());

        var typesFromStore = context.Set<ManyTypes>().OrderBy(m => m.Id).First();
        AssertEqual(types, typesFromStore, jsonColumns);
    }

    protected virtual void AssertEqual(ManyTypes types, ManyTypes otherTypes, bool jsonColumns)
    {
        Assert.Equal(types.Id.Id, otherTypes.Id.Id);
        Assert.Equal(types.Bool, otherTypes.Bool);
        Assert.Equal(types.UInt8, otherTypes.UInt8);
        Assert.Equal(types.UInt16, otherTypes.UInt16);
        Assert.Equal(types.UInt32, otherTypes.UInt32);
        Assert.Equal(types.UInt64, otherTypes.UInt64);
        Assert.Equal(types.Int8, otherTypes.Int8);
        Assert.Equal(types.Int16, otherTypes.Int16);
        Assert.Equal(types.Int32, otherTypes.Int32);
        Assert.Equal(types.Int64, otherTypes.Int64);
        Assert.Equal(types.Char, otherTypes.Char);
        Assert.Equal(types.Decimal, otherTypes.Decimal);
        Assert.Equal(types.Double, otherTypes.Double);
        Assert.Equal(types.Float, otherTypes.Float);
        Assert.Equal(types.Guid, otherTypes.Guid);
        Assert.Equal(types.DateTime, otherTypes.DateTime);
        Assert.Equal(types.DateOnly, otherTypes.DateOnly);
        Assert.Equal(types.TimeOnly, otherTypes.TimeOnly);
        Assert.Equal(types.TimeSpan, otherTypes.TimeSpan);
        Assert.Equal(types.String, otherTypes.String);
        Assert.Equal(types.Bytes, otherTypes.Bytes);
        Assert.Equal(types.Uri, otherTypes.Uri);
        Assert.Equal(types.IPAddress, otherTypes.IPAddress);
        Assert.Equal(types.PhysicalAddress, otherTypes.PhysicalAddress);

        Assert.Equal(types.NullableBool, otherTypes.NullableBool);
        Assert.Equal(types.NullableUInt8, otherTypes.NullableUInt8);
        Assert.Equal(types.NullableUInt16, otherTypes.NullableUInt16);
        Assert.Equal(types.NullableUInt32, otherTypes.NullableUInt32);
        Assert.Equal(types.NullableUInt64, otherTypes.NullableUInt64);
        Assert.Equal(types.NullableInt8, otherTypes.NullableInt8);
        Assert.Equal(types.NullableInt16, otherTypes.NullableInt16);
        Assert.Equal(types.NullableInt32, otherTypes.NullableInt32);
        Assert.Equal(types.NullableInt64, otherTypes.NullableInt64);
        Assert.Equal(types.NullableChar, otherTypes.NullableChar);
        Assert.Equal(types.NullableDecimal, otherTypes.NullableDecimal);
        Assert.Equal(types.NullableDouble, otherTypes.NullableDouble);
        Assert.Equal(types.NullableFloat, otherTypes.NullableFloat);
        Assert.Equal(types.NullableGuid, otherTypes.NullableGuid);
        Assert.Equal(types.NullableDateTime, otherTypes.NullableDateTime);
        Assert.Equal(types.NullableDateOnly, otherTypes.NullableDateOnly);
        Assert.Equal(types.NullableTimeOnly, otherTypes.NullableTimeOnly);
        Assert.Equal(types.NullableTimeSpan, otherTypes.NullableTimeSpan);
        Assert.Equal(types.NullableString, otherTypes.NullableString);
        Assert.Equal(types.NullableBytes, otherTypes.NullableBytes);
        Assert.Equal(types.NullableUri, otherTypes.NullableUri);
        Assert.Equal(types.NullableIPAddress, otherTypes.NullableIPAddress);
        Assert.Equal(types.NullablePhysicalAddress, otherTypes.NullablePhysicalAddress);

        Assert.Equal(types.BoolArray, otherTypes.BoolArray);
        Assert.Equal(types.UInt8Array, otherTypes.UInt8Array);
        Assert.Equal(types.UInt16Array, otherTypes.UInt16Array);
        Assert.Equal(types.UInt32Array, otherTypes.UInt32Array);
        Assert.Equal(types.UInt64Array, otherTypes.UInt64Array);
        Assert.Equal(types.Int8Array, otherTypes.Int8Array);
        Assert.Equal(types.Int16Array, otherTypes.Int16Array);
        Assert.Equal(types.Int32Array, otherTypes.Int32Array);
        Assert.Equal(types.Int64Array, otherTypes.Int64Array);
        Assert.Equal(types.CharArray, otherTypes.CharArray);
        Assert.Equal(types.DecimalArray, otherTypes.DecimalArray);
        Assert.Equal(types.DoubleArray, otherTypes.DoubleArray);
        Assert.Equal(types.FloatArray, otherTypes.FloatArray);
        Assert.Equal(types.GuidArray, otherTypes.GuidArray);
        Assert.Equal(types.DateTimeArray, otherTypes.DateTimeArray);
        Assert.Equal(types.DateOnlyArray, otherTypes.DateOnlyArray);
        Assert.Equal(types.TimeOnlyArray, otherTypes.TimeOnlyArray);
        Assert.Equal(types.TimeSpanArray, otherTypes.TimeSpanArray);
        Assert.Equal(types.StringArray, otherTypes.StringArray);
        Assert.Equal(types.BytesArray, otherTypes.BytesArray);
        Assert.Equal(types.UriArray, otherTypes.UriArray);
        Assert.Equal(types.IPAddressArray, otherTypes.IPAddressArray);
        Assert.Equal(types.PhysicalAddressArray, otherTypes.PhysicalAddressArray);

        Assert.Equal(types.NullableBoolArray, otherTypes.NullableBoolArray);
        Assert.Equal(types.NullableUInt8Array, otherTypes.NullableUInt8Array);
        Assert.Equal(types.NullableUInt16Array, otherTypes.NullableUInt16Array);
        Assert.Equal(types.NullableUInt32Array, otherTypes.NullableUInt32Array);
        Assert.Equal(types.NullableUInt64Array, otherTypes.NullableUInt64Array);
        Assert.Equal(types.NullableInt8Array, otherTypes.NullableInt8Array);
        Assert.Equal(types.NullableInt16Array, otherTypes.NullableInt16Array);
        Assert.Equal(types.NullableInt32Array, otherTypes.NullableInt32Array);
        Assert.Equal(types.NullableInt64Array, otherTypes.NullableInt64Array);
        Assert.Equal(types.NullableCharArray, otherTypes.NullableCharArray);
        Assert.Equal(types.NullableDecimalArray, otherTypes.NullableDecimalArray);
        Assert.Equal(types.NullableDoubleArray, otherTypes.NullableDoubleArray);
        Assert.Equal(types.NullableFloatArray, otherTypes.NullableFloatArray);
        Assert.Equal(types.NullableGuidArray, otherTypes.NullableGuidArray);
        Assert.Equal(types.NullableDateTimeArray, otherTypes.NullableDateTimeArray);
        Assert.Equal(types.NullableDateOnlyArray, otherTypes.NullableDateOnlyArray);
        Assert.Equal(types.NullableTimeOnlyArray, otherTypes.NullableTimeOnlyArray);
        Assert.Equal(types.NullableTimeSpanArray, otherTypes.NullableTimeSpanArray);
        Assert.Equal(types.NullableStringArray, otherTypes.NullableStringArray);
        Assert.Equal(types.NullableBytesArray, otherTypes.NullableBytesArray);
        Assert.Equal(types.NullableUriArray, otherTypes.NullableUriArray);
        Assert.Equal(types.NullableIPAddressArray, otherTypes.NullableIPAddressArray);
        Assert.Equal(types.NullablePhysicalAddressArray, otherTypes.NullablePhysicalAddressArray);

        Assert.Equal(types.BoolReadOnlyCollection, otherTypes.BoolReadOnlyCollection);
        Assert.Equal(types.UInt8ReadOnlyCollection, otherTypes.UInt8ReadOnlyCollection);
        Assert.Equal(types.Int32ReadOnlyCollection, otherTypes.Int32ReadOnlyCollection);
        Assert.Equal(types.StringReadOnlyCollection, otherTypes.StringReadOnlyCollection);
        Assert.Equal(types.IPAddressReadOnlyCollection, otherTypes.IPAddressReadOnlyCollection);

        Assert.Equal(types.Enum8, otherTypes.Enum8);
        Assert.Equal(types.Enum16, otherTypes.Enum16);
        Assert.Equal(types.Enum32, otherTypes.Enum32);
        Assert.Equal(types.Enum64, otherTypes.Enum64);
        Assert.Equal(types.EnumU8, otherTypes.EnumU8);
        Assert.Equal(types.EnumU16, otherTypes.EnumU16);
        Assert.Equal(types.EnumU32, otherTypes.EnumU32);
        Assert.Equal(types.EnumU64, otherTypes.EnumU64);

        Assert.Equal(types.Enum8AsString, otherTypes.Enum8AsString);
        Assert.Equal(types.Enum16AsString, otherTypes.Enum16AsString);
        Assert.Equal(types.Enum32AsString, otherTypes.Enum32AsString);
        Assert.Equal(types.Enum64AsString, otherTypes.Enum64AsString);
        Assert.Equal(types.EnumU8AsString, otherTypes.EnumU8AsString);
        Assert.Equal(types.EnumU16AsString, otherTypes.EnumU16AsString);
        Assert.Equal(types.EnumU32AsString, otherTypes.EnumU32AsString);
        Assert.Equal(types.EnumU64AsString, otherTypes.EnumU64AsString);

        Assert.Equal(types.NullableEnum8, otherTypes.NullableEnum8);
        Assert.Equal(types.NullableEnum16, otherTypes.NullableEnum16);
        Assert.Equal(types.NullableEnum32, otherTypes.NullableEnum32);
        Assert.Equal(types.NullableEnum64, otherTypes.NullableEnum64);
        Assert.Equal(types.NullableEnumU8, otherTypes.NullableEnumU8);
        Assert.Equal(types.NullableEnumU16, otherTypes.NullableEnumU16);
        Assert.Equal(types.NullableEnumU32, otherTypes.NullableEnumU32);
        Assert.Equal(types.NullableEnumU64, otherTypes.NullableEnumU64);

        Assert.Equal(types.NullableEnum8AsString, otherTypes.NullableEnum8AsString);
        Assert.Equal(types.NullableEnum16AsString, otherTypes.NullableEnum16AsString);
        Assert.Equal(types.NullableEnum32AsString, otherTypes.NullableEnum32AsString);
        Assert.Equal(types.NullableEnum64AsString, otherTypes.NullableEnum64AsString);
        Assert.Equal(types.NullableEnumU8AsString, otherTypes.NullableEnumU8AsString);
        Assert.Equal(types.NullableEnumU16AsString, otherTypes.NullableEnumU16AsString);
        Assert.Equal(types.NullableEnumU32AsString, otherTypes.NullableEnumU32AsString);
        Assert.Equal(types.NullableEnumU64AsString, otherTypes.NullableEnumU64AsString);

        Assert.Equal(types.Enum8Collection, otherTypes.Enum8Collection);
        Assert.Equal(types.Enum16Collection, otherTypes.Enum16Collection);
        Assert.Equal(types.Enum32Collection, otherTypes.Enum32Collection);
        Assert.Equal(types.Enum64Collection, otherTypes.Enum64Collection);
        Assert.Equal(types.EnumU8Collection, otherTypes.EnumU8Collection);
        Assert.Equal(types.EnumU16Collection, otherTypes.EnumU16Collection);
        Assert.Equal(types.EnumU32Collection, otherTypes.EnumU32Collection);
        Assert.Equal(types.EnumU64Collection, otherTypes.EnumU64Collection);

        Assert.Equal(types.Enum8AsStringCollection, otherTypes.Enum8AsStringCollection);
        Assert.Equal(types.Enum16AsStringCollection, otherTypes.Enum16AsStringCollection);
        Assert.Equal(types.Enum32AsStringCollection, otherTypes.Enum32AsStringCollection);
        Assert.Equal(types.Enum64AsStringCollection, otherTypes.Enum64AsStringCollection);
        Assert.Equal(types.EnumU8AsStringCollection, otherTypes.EnumU8AsStringCollection);
        Assert.Equal(types.EnumU16AsStringCollection, otherTypes.EnumU16AsStringCollection);
        Assert.Equal(types.EnumU32AsStringCollection, otherTypes.EnumU32AsStringCollection);
        Assert.Equal(types.EnumU64AsStringCollection, otherTypes.EnumU64AsStringCollection);

        Assert.Equal(types.NullableEnum8Collection, otherTypes.NullableEnum8Collection);
        Assert.Equal(types.NullableEnum16Collection, otherTypes.NullableEnum16Collection);
        Assert.Equal(types.NullableEnum32Collection, otherTypes.NullableEnum32Collection);
        Assert.Equal(types.NullableEnum64Collection, otherTypes.NullableEnum64Collection);
        Assert.Equal(types.NullableEnumU8Collection, otherTypes.NullableEnumU8Collection);
        Assert.Equal(types.NullableEnumU16Collection, otherTypes.NullableEnumU16Collection);
        Assert.Equal(types.NullableEnumU32Collection, otherTypes.NullableEnumU32Collection);
        Assert.Equal(types.NullableEnumU64Collection, otherTypes.NullableEnumU64Collection);

        Assert.Equal(types.NullableEnum8AsStringCollection, otherTypes.NullableEnum8AsStringCollection);
        Assert.Equal(types.NullableEnum16AsStringCollection, otherTypes.NullableEnum16AsStringCollection);
        Assert.Equal(types.NullableEnum32AsStringCollection, otherTypes.NullableEnum32AsStringCollection);
        Assert.Equal(types.NullableEnum64AsStringCollection, otherTypes.NullableEnum64AsStringCollection);
        Assert.Equal(types.NullableEnumU8AsStringCollection, otherTypes.NullableEnumU8AsStringCollection);
        Assert.Equal(types.NullableEnumU16AsStringCollection, otherTypes.NullableEnumU16AsStringCollection);
        Assert.Equal(types.NullableEnumU32AsStringCollection, otherTypes.NullableEnumU32AsStringCollection);
        Assert.Equal(types.NullableEnumU64AsStringCollection, otherTypes.NullableEnumU64AsStringCollection);

        Assert.Equal(types.Enum8Array, otherTypes.Enum8Array);
        Assert.Equal(types.Enum16Array, otherTypes.Enum16Array);
        Assert.Equal(types.Enum32Array, otherTypes.Enum32Array);
        Assert.Equal(types.Enum64Array, otherTypes.Enum64Array);
        Assert.Equal(types.EnumU8Array, otherTypes.EnumU8Array);
        Assert.Equal(types.EnumU16Array, otherTypes.EnumU16Array);
        Assert.Equal(types.EnumU32Array, otherTypes.EnumU32Array);
        Assert.Equal(types.EnumU64Array, otherTypes.EnumU64Array);

        Assert.Equal(types.Enum8AsStringArray, otherTypes.Enum8AsStringArray);
        Assert.Equal(types.Enum16AsStringArray, otherTypes.Enum16AsStringArray);
        Assert.Equal(types.Enum32AsStringArray, otherTypes.Enum32AsStringArray);
        Assert.Equal(types.Enum64AsStringArray, otherTypes.Enum64AsStringArray);
        Assert.Equal(types.EnumU8AsStringArray, otherTypes.EnumU8AsStringArray);
        Assert.Equal(types.EnumU16AsStringArray, otherTypes.EnumU16AsStringArray);
        Assert.Equal(types.EnumU32AsStringArray, otherTypes.EnumU32AsStringArray);
        Assert.Equal(types.EnumU64AsStringArray, otherTypes.EnumU64AsStringArray);

        Assert.Equal(types.NullableEnum8Array, otherTypes.NullableEnum8Array);
        Assert.Equal(types.NullableEnum16Array, otherTypes.NullableEnum16Array);
        Assert.Equal(types.NullableEnum32Array, otherTypes.NullableEnum32Array);
        Assert.Equal(types.NullableEnum64Array, otherTypes.NullableEnum64Array);
        Assert.Equal(types.NullableEnumU8Array, otherTypes.NullableEnumU8Array);
        Assert.Equal(types.NullableEnumU16Array, otherTypes.NullableEnumU16Array);
        Assert.Equal(types.NullableEnumU32Array, otherTypes.NullableEnumU32Array);
        Assert.Equal(types.NullableEnumU64Array, otherTypes.NullableEnumU64Array);

        Assert.Equal(types.NullableEnum8AsStringArray, otherTypes.NullableEnum8AsStringArray);
        Assert.Equal(types.NullableEnum16AsStringArray, otherTypes.NullableEnum16AsStringArray);
        Assert.Equal(types.NullableEnum32AsStringArray, otherTypes.NullableEnum32AsStringArray);
        Assert.Equal(types.NullableEnum64AsStringArray, otherTypes.NullableEnum64AsStringArray);
        Assert.Equal(types.NullableEnumU8AsStringArray, otherTypes.NullableEnumU8AsStringArray);
        Assert.Equal(types.NullableEnumU16AsStringArray, otherTypes.NullableEnumU16AsStringArray);
        Assert.Equal(types.NullableEnumU32AsStringArray, otherTypes.NullableEnumU32AsStringArray);
        Assert.Equal(types.NullableEnumU64AsStringArray, otherTypes.NullableEnumU64AsStringArray);

        Assert.Equal(types.BoolNestedCollection, otherTypes.BoolNestedCollection);
        Assert.Equal(types.UInt8NestedCollection, otherTypes.UInt8NestedCollection);
        Assert.Equal(types.Int8NestedCollection, otherTypes.Int8NestedCollection);
        Assert.Equal(types.Int32NestedCollection, otherTypes.Int32NestedCollection);
        Assert.Equal(types.Int64NestedCollection, otherTypes.Int64NestedCollection);
        Assert.Equal(types.CharNestedCollection, otherTypes.CharNestedCollection);
        Assert.Equal(types.GuidNestedCollection, otherTypes.GuidNestedCollection);
        Assert.Equal(types.StringNestedCollection, otherTypes.StringNestedCollection);
        Assert.Equal(types.BytesNestedCollection, otherTypes.BytesNestedCollection);

        Assert.Equal(types.NullableUInt8NestedCollection, otherTypes.NullableUInt8NestedCollection);
        Assert.Equal(types.NullableInt32NestedCollection, otherTypes.NullableInt32NestedCollection);
        Assert.Equal(types.NullableInt64NestedCollection, otherTypes.NullableInt64NestedCollection);
        Assert.Equal(types.NullableGuidNestedCollection, otherTypes.NullableGuidNestedCollection);
        Assert.Equal(types.NullableStringNestedCollection, otherTypes.NullableStringNestedCollection);
        Assert.Equal(types.NullableBytesNestedCollection, otherTypes.NullableBytesNestedCollection);
        Assert.Equal(types.NullablePhysicalAddressNestedCollection, otherTypes.NullablePhysicalAddressNestedCollection);

        Assert.Equal(types.Enum8NestedCollection, otherTypes.Enum8NestedCollection);
        Assert.Equal(types.Enum32NestedCollection, otherTypes.Enum32NestedCollection);
        Assert.Equal(types.EnumU64NestedCollection, otherTypes.EnumU64NestedCollection);
        Assert.Equal(types.NullableEnum8NestedCollection, otherTypes.NullableEnum8NestedCollection);
        Assert.Equal(types.NullableEnum32NestedCollection, otherTypes.NullableEnum32NestedCollection);
        Assert.Equal(types.NullableEnumU64NestedCollection, otherTypes.NullableEnumU64NestedCollection);

        Assert.Equal(types.BoolToStringConverterProperty, otherTypes.BoolToStringConverterProperty);
        Assert.Equal(types.BoolToTwoValuesConverterProperty, otherTypes.BoolToTwoValuesConverterProperty);
        Assert.Equal(types.BoolToZeroOneConverterProperty, otherTypes.BoolToZeroOneConverterProperty);
        Assert.Equal(types.BytesToStringConverterProperty, otherTypes.BytesToStringConverterProperty);
        Assert.Equal(types.CastingConverterProperty, otherTypes.CastingConverterProperty);
        Assert.Equal(types.CharToStringConverterProperty, otherTypes.CharToStringConverterProperty);
        Assert.Equal(types.DateOnlyToStringConverterProperty, otherTypes.DateOnlyToStringConverterProperty);
        Assert.Equal(types.DateTimeOffsetToBinaryConverterProperty, otherTypes.DateTimeOffsetToBinaryConverterProperty);
        Assert.Equal(types.DateTimeOffsetToBytesConverterProperty, otherTypes.DateTimeOffsetToBytesConverterProperty);
        Assert.Equal(types.DateTimeOffsetToStringConverterProperty, otherTypes.DateTimeOffsetToStringConverterProperty);
        Assert.Equal(types.DateTimeToBinaryConverterProperty, otherTypes.DateTimeToBinaryConverterProperty);
        Assert.Equal(types.DateTimeToStringConverterProperty, otherTypes.DateTimeToStringConverterProperty);
        Assert.Equal(types.EnumToNumberConverterProperty, otherTypes.EnumToNumberConverterProperty);
        Assert.Equal(types.EnumToStringConverterProperty, otherTypes.EnumToStringConverterProperty);
        Assert.Equal(types.GuidToBytesConverterProperty, otherTypes.GuidToBytesConverterProperty);
        Assert.Equal(types.GuidToStringConverterProperty, otherTypes.GuidToStringConverterProperty);
        Assert.Equal(types.IPAddressToBytesConverterProperty, otherTypes.IPAddressToBytesConverterProperty);
        Assert.Equal(types.IPAddressToStringConverterProperty, otherTypes.IPAddressToStringConverterProperty);
        Assert.Equal(types.IntNumberToBytesConverterProperty, otherTypes.IntNumberToBytesConverterProperty);
        Assert.Equal(types.DecimalNumberToBytesConverterProperty, otherTypes.DecimalNumberToBytesConverterProperty);
        Assert.Equal(types.DoubleNumberToBytesConverterProperty, otherTypes.DoubleNumberToBytesConverterProperty);
        Assert.Equal(types.IntNumberToStringConverterProperty, otherTypes.IntNumberToStringConverterProperty);
        Assert.Equal(types.DecimalNumberToStringConverterProperty, otherTypes.DecimalNumberToStringConverterProperty);
        Assert.Equal(types.DoubleNumberToStringConverterProperty, otherTypes.DoubleNumberToStringConverterProperty);
        Assert.Equal(types.PhysicalAddressToBytesConverterProperty, otherTypes.PhysicalAddressToBytesConverterProperty);
        Assert.Equal(types.PhysicalAddressToStringConverterProperty, otherTypes.PhysicalAddressToStringConverterProperty);
        Assert.Equal(types.StringToBoolConverterProperty, otherTypes.StringToBoolConverterProperty);
        Assert.Equal(types.StringToBytesConverterProperty, otherTypes.StringToBytesConverterProperty);
        Assert.Equal(types.StringToCharConverterProperty, otherTypes.StringToCharConverterProperty);
        Assert.Equal(types.StringToDateOnlyConverterProperty, otherTypes.StringToDateOnlyConverterProperty);
        Assert.Equal(types.StringToDateTimeConverterProperty, otherTypes.StringToDateTimeConverterProperty);
        Assert.Equal(types.StringToDateTimeOffsetConverterProperty, otherTypes.StringToDateTimeOffsetConverterProperty);
        Assert.Equal(types.StringToEnumConverterProperty, otherTypes.StringToEnumConverterProperty);
        Assert.Equal(types.StringToIntNumberConverterProperty, otherTypes.StringToIntNumberConverterProperty);
        Assert.Equal(types.StringToDecimalNumberConverterProperty, otherTypes.StringToDecimalNumberConverterProperty);
        Assert.Equal(types.StringToDoubleNumberConverterProperty, otherTypes.StringToDoubleNumberConverterProperty);
        Assert.Equal(types.StringToTimeOnlyConverterProperty, otherTypes.StringToTimeOnlyConverterProperty);
        Assert.Equal(types.StringToTimeSpanConverterProperty, otherTypes.StringToTimeSpanConverterProperty);
        Assert.Equal(types.StringToUriConverterProperty, otherTypes.StringToUriConverterProperty);
        Assert.Equal(types.TimeOnlyToStringConverterProperty, otherTypes.TimeOnlyToStringConverterProperty);
        Assert.Equal(types.TimeOnlyToTicksConverterProperty, otherTypes.TimeOnlyToTicksConverterProperty);
        Assert.Equal(types.TimeSpanToStringConverterProperty, otherTypes.TimeSpanToStringConverterProperty);
        Assert.Equal(types.UriToStringConverterProperty, otherTypes.UriToStringConverterProperty);
        Assert.Equal(types.NullIntToNullStringConverterProperty, otherTypes.NullIntToNullStringConverterProperty);
    }

    [ConditionalFact]
    public virtual Task ComplexTypes()
        => Test(
            BuildComplexTypesModel,
            AssertComplexTypes,
            async c =>
            {
                c.Set<PrincipalDerived<DependentBase<byte?>>>().Add(
                    new PrincipalDerived<DependentBase<byte?>>
                    {
                        Id = 1,
                        AlternateId = new Guid(),
                        Dependent = new DependentBase<byte?>(1),
                        Owned = new OwnedType(c) { Principal = new PrincipalBase() }
                    });

                await c.SaveChangesAsync();
            },
            options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true, ForNativeAot = true });

    protected virtual void BuildComplexTypesModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                eb.Ignore(e => e.Owned);

                eb.Property("FlagsEnum2");

                eb.ComplexProperty(
                    e => e.Owned, eb =>
                    {
                        eb.HasField("_ownedField")
                            .UsePropertyAccessMode(PropertyAccessMode.Field)
                            .HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues)
                            .HasPropertyAnnotation("goo", "ber")
                            .HasTypeAnnotation("go", "brr");
                        eb.Property(c => c.Details)
                            .IsUnicode(false)
                            .IsRequired(false)
                            .HasField("_details")
                            .HasSentinel("")
                            .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction)
                            .HasMaxLength(64)
                            .HasPrecision(3, 2)
                            .HasAnnotation("foo", "bar");
                        eb.Ignore(e => e.Context);
                        eb.ComplexProperty(
                            o => o.Principal, cb =>
                            {
                                cb.Property("FlagsEnum2");
                            });
                    });
            });

        modelBuilder.Entity<PrincipalDerived<DependentBase<byte?>>>(
            eb =>
            {
                //eb.ComplexCollection(typeof(OwnedType).Name, "ManyOwned");
                eb.Ignore(p => p.Dependent);
                eb.Ignore(p => p.Principals);
            });
    }

    protected virtual void AssertComplexTypes(IModel model)
    {
        var principalBase = model.FindEntityType(typeof(PrincipalBase))!;

        var complexProperty = principalBase.GetComplexProperties().Single();
        Assert.Equal(
            ["goo"],
            complexProperty.GetAnnotations().Select(a => a.Name));
        Assert.Equal(nameof(PrincipalBase.Owned), complexProperty.Name);
        Assert.False(complexProperty.IsCollection);
        Assert.False(complexProperty.IsNullable);
        Assert.Equal(typeof(OwnedType), complexProperty.ClrType);
        Assert.Equal("_ownedField", complexProperty.FieldInfo!.Name);
        Assert.Equal(nameof(PrincipalBase.Owned), complexProperty.PropertyInfo!.Name);
        Assert.Equal(principalBase, complexProperty.DeclaringType);
        Assert.Equal(PropertyAccessMode.Field, complexProperty.GetPropertyAccessMode());
        Assert.Equal("ber", complexProperty["goo"]);

        var complexType = complexProperty.ComplexType;
        Assert.Equal(typeof(PrincipalBase).FullName + ".Owned#OwnedType", complexType.Name);
        Assert.Equal(typeof(OwnedType), complexType.ClrType);
        Assert.True(complexType.HasSharedClrType);
        Assert.False(complexType.IsPropertyBag);
        Assert.IsType<ConstructorBinding>(complexType.ConstructorBinding);
        Assert.Null(complexType.FindIndexerPropertyInfo());
        Assert.Equal(
            ChangeTrackingStrategy.ChangingAndChangedNotificationsWithOriginalValues,
            complexType.GetChangeTrackingStrategy());
        Assert.Equal(
            CoreStrings.RuntimeModelMissingData,
            Assert.Throws<InvalidOperationException>(() => complexType.GetPropertyAccessMode()).Message);
        Assert.Equal("brr", complexType["go"]);

        var detailsProperty = complexType.FindProperty(nameof(OwnedType.Details))!;
        Assert.Equal(typeof(string), detailsProperty.ClrType);
        Assert.Equal(typeof(string), detailsProperty.PropertyInfo!.PropertyType);
        Assert.Equal(typeof(string), detailsProperty.FieldInfo!.FieldType);
        Assert.Equal("_details", detailsProperty.FieldInfo.Name);
        Assert.True(detailsProperty.IsNullable);
        Assert.False(detailsProperty.IsUnicode());
        Assert.Equal(64, detailsProperty.GetMaxLength());
        Assert.Equal(3, detailsProperty.GetPrecision());
        Assert.Equal(2, detailsProperty.GetScale());
        Assert.Equal("", detailsProperty.Sentinel);
        Assert.Equal(PropertyAccessMode.FieldDuringConstruction, detailsProperty.GetPropertyAccessMode());
        Assert.Null(detailsProperty.GetValueConverter());
        Assert.NotNull(detailsProperty.GetValueComparer());
        Assert.NotNull(detailsProperty.GetKeyValueComparer());

        var nestedComplexProperty = complexType.FindComplexProperty(nameof(OwnedType.Principal))!;
        Assert.True(nestedComplexProperty.IsNullable);

        var nestedComplexType = nestedComplexProperty.ComplexType;

        Assert.Equal(ExpectedComplexTypeProperties, nestedComplexType.GetProperties().Count());

        var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>))!;
        Assert.Equal(principalBase, principalDerived.BaseType);

        Assert.Equal(
            [principalBase, principalDerived],
            model.GetEntityTypes());
    }

    protected virtual int ExpectedComplexTypeProperties
        => 14;

    public class CustomValueComparer<T>() : ValueComparer<T>(false);

    public class ManyTypesIdConverter() : ValueConverter<ManyTypesId, int>(v => v.Id, v => new ManyTypesId(v));

    public class NullIntToNullStringConverter() : ValueConverter<int?, string?>(
        v => v == null ? null : v.ToString()!, v => v == null || v == "<null>" ? null : int.Parse(v), convertsNulls: true);

    public abstract class AbstractBase
    {
        public int Id { get; set; }
    }

    public enum AnEnum
    {
        A = 1,
        B,
    }

    [Flags]
    public enum AFlagsEnum
    {
        A = 1,
        B = 2,
        C = 4,
    }

    [Flags]
    public enum Enum1
    {
        Default = 0,
        One = 1,
        Two = 2
    }

    public enum Enum8 : sbyte
    {
        Min = sbyte.MinValue,
        Default = 0,
        One = 1,
        Max = sbyte.MaxValue
    }

    public enum Enum16 : short
    {
        Min = short.MinValue,
        Default = 0,
        One = 1,
        Max = short.MaxValue
    }

    public enum Enum32
    {
        Min = int.MinValue,
        Default = 0,
        One = 1,
        Max = int.MaxValue
    }

    public enum Enum64 : long
    {
        Min = long.MinValue,
        Default = 0,
        One = 1,
        Max = long.MaxValue
    }

    public enum EnumU8 : byte
    {
        Min = byte.MinValue,
        Default = 0,
        One = 1,
        Max = byte.MaxValue
    }

    public enum EnumU16 : ushort
    {
        Min = ushort.MinValue,
        Default = 0,
        One = 1,
        Max = ushort.MaxValue
    }

    public enum EnumU32 : uint
    {
        Min = uint.MinValue,
        Default = 0,
        One = 1,
        Max = uint.MaxValue
    }

    public enum EnumU64 : ulong
    {
        Min = ulong.MinValue,
        Default = 0,
        One = 1,
        Max = ulong.MaxValue
    }

    public sealed class MyJsonGuidReaderWriter : JsonValueReaderWriter<Guid>
    {
        public override Guid FromJsonTyped(ref Utf8JsonReaderManager manager, object? existingObject = null)
            => manager.CurrentReader.GetGuid();

        public override void ToJsonTyped(Utf8JsonWriter writer, Guid value)
            => writer.WriteStringValue(value);

        private readonly Expression<Func<MyJsonGuidReaderWriter>> _ctorLambda = () => new MyJsonGuidReaderWriter();

        /// <inheritdoc />
        public override Expression ConstructorExpression
            => _ctorLambda.Body;
    }

    public class ManyTypes
    {
        public ManyTypesId Id { get; set; }
        public bool Bool { get; set; }
        public byte UInt8 { get; set; }
        public ushort UInt16 { get; set; }
        public uint UInt32 { get; set; }
        public ulong UInt64 { get; set; }
        public sbyte Int8 { get; set; }
        public short Int16 { get; set; }
        public int Int32 { get; set; }
        public long Int64 { get; set; }
        public char Char { get; set; }
        public decimal Decimal { get; set; }
        public double Double { get; set; }
        public float Float { get; set; }
        public Guid Guid { get; set; }
        public DateTime DateTime { get; set; }
        public DateOnly DateOnly { get; set; }
        public TimeOnly TimeOnly { get; set; }
        public TimeSpan TimeSpan { get; set; }
        public string String { get; set; } = null!;
        public byte[] Bytes { get; set; } = null!;
        public Uri Uri { get; set; } = null!;
        public IPAddress IPAddress { get; set; } = null!;
        public PhysicalAddress PhysicalAddress { get; set; } = null!;

        public bool? NullableBool { get; set; }
        public byte? NullableUInt8 { get; set; }
        public ushort? NullableUInt16 { get; set; }
        public uint? NullableUInt32 { get; set; }
        public ulong? NullableUInt64 { get; set; }
        public sbyte? NullableInt8 { get; set; }
        public short? NullableInt16 { get; set; }
        public int? NullableInt32 { get; set; }
        public long? NullableInt64 { get; set; }
        public char? NullableChar { get; set; }
        public decimal? NullableDecimal { get; set; }
        public double? NullableDouble { get; set; }
        public float? NullableFloat { get; set; }
        public Guid? NullableGuid { get; set; }
        public DateTime? NullableDateTime { get; set; }
        public DateOnly? NullableDateOnly { get; set; }
        public TimeOnly? NullableTimeOnly { get; set; }
        public TimeSpan? NullableTimeSpan { get; set; }
        public string? NullableString { get; set; }
        public byte[]? NullableBytes { get; set; }
        public Uri? NullableUri { get; set; }
        public IPAddress? NullableIPAddress { get; set; }
        public PhysicalAddress? NullablePhysicalAddress { get; set; }

        public bool[] BoolArray { get; set; } = null!;
        public byte[] UInt8Array { get; set; } = null!;
        public ushort[] UInt16Array { get; set; } = null!;
        public uint[] UInt32Array { get; set; } = null!;
        public ulong[] UInt64Array { get; set; } = null!;
        public sbyte[] Int8Array { get; set; } = null!;
        public short[] Int16Array { get; set; } = null!;
        public int[] Int32Array { get; set; } = null!;
        public long[] Int64Array { get; set; } = null!;
        public char[] CharArray { get; set; } = null!;
        public decimal[] DecimalArray { get; set; } = null!;
        public double[] DoubleArray { get; set; } = null!;
        public float[] FloatArray { get; set; } = null!;
        public Guid[] GuidArray { get; set; } = null!;
        public DateTime[] DateTimeArray { get; set; } = null!;
        public DateOnly[] DateOnlyArray { get; set; } = null!;
        public TimeOnly[] TimeOnlyArray { get; set; } = null!;
        public TimeSpan[] TimeSpanArray { get; set; } = null!;
        public string[] StringArray { get; set; } = null!;
        public byte[][] BytesArray { get; set; } = null!;
        public Uri[] UriArray { get; set; } = null!;
        public IPAddress[] IPAddressArray { get; set; } = null!;
        public PhysicalAddress[] PhysicalAddressArray { get; set; } = null!;

        public bool?[] NullableBoolArray { get; set; } = null!;
        public byte?[] NullableUInt8Array { get; set; } = null!;
        public ushort?[] NullableUInt16Array { get; set; } = null!;
        public uint?[] NullableUInt32Array { get; set; } = null!;
        public ulong?[] NullableUInt64Array { get; set; } = null!;
        public sbyte?[] NullableInt8Array { get; set; } = null!;
        public short?[] NullableInt16Array { get; set; } = null!;
        public int?[] NullableInt32Array { get; set; } = null!;
        public long?[] NullableInt64Array { get; set; } = null!;
        public char?[] NullableCharArray { get; set; } = null!;
        public decimal?[] NullableDecimalArray { get; set; } = null!;
        public double?[] NullableDoubleArray { get; set; } = null!;
        public float?[] NullableFloatArray { get; set; } = null!;
        public Guid?[] NullableGuidArray { get; set; } = null!;
        public DateTime?[] NullableDateTimeArray { get; set; } = null!;
        public DateOnly?[] NullableDateOnlyArray { get; set; } = null!;
        public TimeOnly?[] NullableTimeOnlyArray { get; set; } = null!;
        public TimeSpan?[] NullableTimeSpanArray { get; set; } = null!;
        public string?[] NullableStringArray { get; set; } = null!;
        public byte[]?[] NullableBytesArray { get; set; } = null!;
        public Uri?[] NullableUriArray { get; set; } = null!;
        public IPAddress?[] NullableIPAddressArray { get; set; } = null!;
        public PhysicalAddress?[] NullablePhysicalAddressArray { get; set; } = null!;

        private List<bool> _boolReadOnlyCollection = [];
        private List<byte> _uInt8ReadOnlyCollection = [];
        private List<int> _int32ReadOnlyCollection = [];
        private List<string> _stringReadOnlyCollection = [];
        private List<IPAddress> _ipAddressReadOnlyCollection = [];

        public IReadOnlyCollection<bool> BoolReadOnlyCollection
        {
            get => _boolReadOnlyCollection.ToList();
            set => _boolReadOnlyCollection = value.ToList();
        }

        public IReadOnlyCollection<byte> UInt8ReadOnlyCollection
        {
            get => _uInt8ReadOnlyCollection.ToList();
            set => _uInt8ReadOnlyCollection = value.ToList();
        }

        public IReadOnlyCollection<int> Int32ReadOnlyCollection
        {
            get => _int32ReadOnlyCollection.ToList();
            set => _int32ReadOnlyCollection = value.ToList();
        }

        public IReadOnlyCollection<string> StringReadOnlyCollection
        {
            get => _stringReadOnlyCollection.ToList();
            set => _stringReadOnlyCollection = value.ToList();
        }

        public IReadOnlyCollection<IPAddress> IPAddressReadOnlyCollection
        {
            get => _ipAddressReadOnlyCollection.ToList();
            set => _ipAddressReadOnlyCollection = value.ToList();
        }

        public Enum8 Enum8 { get; set; }
        public Enum16 Enum16 { get; set; }
        public Enum32 Enum32 { get; set; }
        public Enum64 Enum64 { get; set; }
        public EnumU8 EnumU8 { get; set; }
        public EnumU16 EnumU16 { get; set; }
        public EnumU32 EnumU32 { get; set; }
        public EnumU64 EnumU64 { get; set; }

        public Enum8 Enum8AsString { get; set; }
        public Enum16 Enum16AsString { get; set; }
        public Enum32 Enum32AsString { get; set; }
        public Enum64 Enum64AsString { get; set; }
        public EnumU8 EnumU8AsString { get; set; }
        public EnumU16 EnumU16AsString { get; set; }
        public EnumU32 EnumU32AsString { get; set; }
        public EnumU64 EnumU64AsString { get; set; }

        public Enum8? NullableEnum8 { get; set; }
        public Enum16? NullableEnum16 { get; set; }
        public Enum32? NullableEnum32 { get; set; }
        public Enum64? NullableEnum64 { get; set; }
        public EnumU8? NullableEnumU8 { get; set; }
        public EnumU16? NullableEnumU16 { get; set; }
        public EnumU32? NullableEnumU32 { get; set; }
        public EnumU64? NullableEnumU64 { get; set; }

        public Enum8? NullableEnum8AsString { get; set; }
        public Enum16? NullableEnum16AsString { get; set; }
        public Enum32? NullableEnum32AsString { get; set; }
        public Enum64? NullableEnum64AsString { get; set; }
        public EnumU8? NullableEnumU8AsString { get; set; }
        public EnumU16? NullableEnumU16AsString { get; set; }
        public EnumU32? NullableEnumU32AsString { get; set; }
        public EnumU64? NullableEnumU64AsString { get; set; }

        public List<Enum8> Enum8Collection { get; set; } = null!;
        public List<Enum16> Enum16Collection { get; set; } = null!;
        public List<Enum32> Enum32Collection { get; set; } = null!;
        public List<Enum64> Enum64Collection { get; set; } = null!;
        public List<EnumU8> EnumU8Collection { get; set; } = null!;
        public List<EnumU16> EnumU16Collection { get; set; } = null!;
        public List<EnumU32> EnumU32Collection { get; set; } = null!;
        public List<EnumU64> EnumU64Collection { get; set; } = null!;

        public List<Enum8> Enum8AsStringCollection { get; set; } = null!;
        public List<Enum16> Enum16AsStringCollection { get; set; } = null!;
        public List<Enum32> Enum32AsStringCollection { get; set; } = null!;
        public List<Enum64> Enum64AsStringCollection { get; set; } = null!;
        public List<EnumU8> EnumU8AsStringCollection { get; set; } = null!;
        public List<EnumU16> EnumU16AsStringCollection { get; set; } = null!;
        public List<EnumU32> EnumU32AsStringCollection { get; set; } = null!;
        public List<EnumU64> EnumU64AsStringCollection { get; set; } = null!;

        public List<Enum8?> NullableEnum8Collection { get; set; } = null!;
        public List<Enum16?> NullableEnum16Collection { get; set; } = null!;
        public List<Enum32?> NullableEnum32Collection { get; set; } = null!;
        public List<Enum64?> NullableEnum64Collection { get; set; } = null!;
        public List<EnumU8?> NullableEnumU8Collection { get; set; } = null!;
        public List<EnumU16?> NullableEnumU16Collection { get; set; } = null!;
        public List<EnumU32?> NullableEnumU32Collection { get; set; } = null!;
        public List<EnumU64?> NullableEnumU64Collection { get; set; } = null!;

        public List<Enum8?> NullableEnum8AsStringCollection { get; set; } = null!;
        public List<Enum16?> NullableEnum16AsStringCollection { get; set; } = null!;
        public List<Enum32?> NullableEnum32AsStringCollection { get; set; } = null!;
        public List<Enum64?> NullableEnum64AsStringCollection { get; set; } = null!;
        public List<EnumU8?> NullableEnumU8AsStringCollection { get; set; } = null!;
        public List<EnumU16?> NullableEnumU16AsStringCollection { get; set; } = null!;
        public List<EnumU32?> NullableEnumU32AsStringCollection { get; set; } = null!;
        public List<EnumU64?> NullableEnumU64AsStringCollection { get; set; } = null!;

        public Enum8[] Enum8Array { get; set; } = null!;
        public Enum16[] Enum16Array { get; set; } = null!;
        public Enum32[] Enum32Array { get; set; } = null!;
        public Enum64[] Enum64Array { get; set; } = null!;
        public EnumU8[] EnumU8Array { get; set; } = null!;
        public EnumU16[] EnumU16Array { get; set; } = null!;
        public EnumU32[] EnumU32Array { get; set; } = null!;
        public EnumU64[] EnumU64Array { get; set; } = null!;

        public Enum8[] Enum8AsStringArray { get; set; } = null!;
        public Enum16[] Enum16AsStringArray { get; set; } = null!;
        public Enum32[] Enum32AsStringArray { get; set; } = null!;
        public Enum64[] Enum64AsStringArray { get; set; } = null!;
        public EnumU8[] EnumU8AsStringArray { get; set; } = null!;
        public EnumU16[] EnumU16AsStringArray { get; set; } = null!;
        public EnumU32[] EnumU32AsStringArray { get; set; } = null!;
        public EnumU64[] EnumU64AsStringArray { get; set; } = null!;

        public Enum8?[] NullableEnum8Array { get; set; } = null!;
        public Enum16?[] NullableEnum16Array { get; set; } = null!;
        public Enum32?[] NullableEnum32Array { get; set; } = null!;
        public Enum64?[] NullableEnum64Array { get; set; } = null!;
        public EnumU8?[] NullableEnumU8Array { get; set; } = null!;
        public EnumU16?[] NullableEnumU16Array { get; set; } = null!;
        public EnumU32?[] NullableEnumU32Array { get; set; } = null!;
        public EnumU64?[] NullableEnumU64Array { get; set; } = null!;

        public Enum8?[] NullableEnum8AsStringArray { get; set; } = null!;
        public Enum16?[] NullableEnum16AsStringArray { get; set; } = null!;
        public Enum32?[] NullableEnum32AsStringArray { get; set; } = null!;
        public Enum64?[] NullableEnum64AsStringArray { get; set; } = null!;
        public EnumU8?[] NullableEnumU8AsStringArray { get; set; } = null!;
        public EnumU16?[] NullableEnumU16AsStringArray { get; set; } = null!;
        public EnumU32?[] NullableEnumU32AsStringArray { get; set; } = null!;
        public EnumU64?[] NullableEnumU64AsStringArray { get; set; } = null!;

        public bool[][] BoolNestedCollection { get; set; } = null!;
        public List<byte[]> UInt8NestedCollection { get; set; } = null!;
        public sbyte[][][] Int8NestedCollection { get; set; } = null!;
        public int[][] Int32NestedCollection { get; set; } = null!;
        public IList<long[]>[] Int64NestedCollection { get; set; } = null!;
        public char[][] CharNestedCollection { get; set; } = null!;
        public ICollection<Guid[][]> GuidNestedCollection { get; set; } = null!;
        public string[][] StringNestedCollection { get; set; } = null!;
        public byte[][][] BytesNestedCollection { get; set; } = null!;

        public byte?[][] NullableUInt8NestedCollection { get; set; } = null!;
        public int?[][] NullableInt32NestedCollection { get; set; } = null!;
        public List<long?[][]> NullableInt64NestedCollection { get; set; } = null!;
        public Guid?[][] NullableGuidNestedCollection { get; set; } = null!;
        public string?[][] NullableStringNestedCollection { get; set; } = null!;
        public byte[]?[][] NullableBytesNestedCollection { get; set; } = null!;
        public IEnumerable<PhysicalAddress?[][]> NullablePhysicalAddressNestedCollection { get; set; } = null!;

        public Enum8[][] Enum8NestedCollection { get; set; } = null!;
        public List<Enum32>[][] Enum32NestedCollection { get; set; } = null!;
        public EnumU64[][] EnumU64NestedCollection { get; set; } = null!;

        public Enum8?[][] NullableEnum8NestedCollection { get; set; } = null!;
        public Enum32?[][][] NullableEnum32NestedCollection { get; set; } = null!;
        public EnumU64?[][] NullableEnumU64NestedCollection { get; set; } = null!;

        public bool BoolToStringConverterProperty { get; set; }
        public bool BoolToTwoValuesConverterProperty { get; set; }
        public bool BoolToZeroOneConverterProperty { get; set; }
        public byte[] BytesToStringConverterProperty { get; set; } = null!;
        public int CastingConverterProperty { get; set; }
        public char CharToStringConverterProperty { get; set; }
        public DateOnly DateOnlyToStringConverterProperty { get; set; }
        public DateTimeOffset DateTimeOffsetToBinaryConverterProperty { get; set; }
        public DateTimeOffset DateTimeOffsetToBytesConverterProperty { get; set; }
        public DateTimeOffset DateTimeOffsetToStringConverterProperty { get; set; }
        public DateTime DateTimeToBinaryConverterProperty { get; set; }
        public DateTime DateTimeToStringConverterProperty { get; set; }
        public DateTime DateTimeToTicksConverterProperty { get; set; }
        public Enum32 EnumToNumberConverterProperty { get; set; }
        public Enum32 EnumToStringConverterProperty { get; set; }
        public Guid GuidToBytesConverterProperty { get; set; }
        public Guid GuidToStringConverterProperty { get; set; }
        public IPAddress IPAddressToBytesConverterProperty { get; set; } = null!;
        public IPAddress IPAddressToStringConverterProperty { get; set; } = null!;
        public int IntNumberToBytesConverterProperty { get; set; }
        public decimal DecimalNumberToBytesConverterProperty { get; set; }
        public double DoubleNumberToBytesConverterProperty { get; set; }
        public int IntNumberToStringConverterProperty { get; set; }
        public decimal DecimalNumberToStringConverterProperty { get; set; }
        public double DoubleNumberToStringConverterProperty { get; set; }
        public PhysicalAddress PhysicalAddressToBytesConverterProperty { get; set; } = null!;
        public PhysicalAddress PhysicalAddressToStringConverterProperty { get; set; } = null!;
        public string StringToBoolConverterProperty { get; set; } = null!;
        public string? StringToBytesConverterProperty { get; set; }
        public string StringToCharConverterProperty { get; set; } = null!;
        public string StringToDateOnlyConverterProperty { get; set; } = null!;
        public string StringToDateTimeConverterProperty { get; set; } = null!;
        public string StringToDateTimeOffsetConverterProperty { get; set; } = null!;
        public string StringToEnumConverterProperty { get; set; } = null!;
        public string StringToGuidConverterProperty { get; set; } = null!;
        public string StringToIntNumberConverterProperty { get; set; } = null!;
        public string StringToDecimalNumberConverterProperty { get; set; } = null!;
        public string StringToDoubleNumberConverterProperty { get; set; } = null!;
        public string StringToTimeOnlyConverterProperty { get; set; } = null!;
        public string StringToTimeSpanConverterProperty { get; set; } = null!;
        public string StringToUriConverterProperty { get; set; } = null!;
        public TimeOnly TimeOnlyToStringConverterProperty { get; set; }
        public TimeOnly TimeOnlyToTicksConverterProperty { get; set; }
        public TimeSpan TimeSpanToStringConverterProperty { get; set; }
        public TimeSpan TimeSpanToTicksConverterProperty { get; set; }
        public Uri UriToStringConverterProperty { get; set; } = null!;
        public int? NullIntToNullStringConverterProperty { get; set; }
    }

    public readonly record struct ManyTypesId(int Id);

    public class Data
    {
        public byte[]? Blob { get; set; }
    }

    public class PrincipalBase : AbstractBase
    {
        public new long? Id { get; set; }
        public Guid AlternateId;

        public AnEnum Enum1 { get; set; }
        public AnEnum? Enum2 { get; set; }
        public AFlagsEnum FlagsEnum1 { get; set; }
        private AFlagsEnum FlagsEnum2 { get; set; }

        public List<short>? ValueTypeList { get; set; }
        public IList<byte>? ValueTypeIList { get; set; }
        public DateTime[]? ValueTypeArray { get; set; }
        public IEnumerable<byte>? ValueTypeEnumerable { get; set; }

        public List<IPAddress>? RefTypeList { get; set; }
        public IList<string>? RefTypeIList { get; set; }
        public IPAddress[]? RefTypeArray { get; set; }
        public IEnumerable<string>? RefTypeEnumerable { get; set; }

        private OwnedType _ownedField = null!;
        public OwnedType Owned { get => _ownedField; set => _ownedField = value; }
        public ICollection<PrincipalBase> Deriveds { get; set; } = null!;
    }

    public class PrincipalDerived<TDependent> : PrincipalBase
        where TDependent : class
    {
        public TDependent? Dependent { get; set; }
        protected ICollection<OwnedType> ManyOwned = null!;
        public ICollection<PrincipalBase> Principals { get; set; } = null!;
    }

    public class DependentBase<TKey>(TKey id) : AbstractBase
    {
        private new TKey Id { get; } = id;

        public TKey GetId()
            => Id;

        public PrincipalDerived<DependentBase<TKey>>? Principal { get; set; }
    }

    public class DependentDerived<TKey>(TKey id, string data) : DependentBase<TKey>(id)
    {
        private string? Data { get; set; } = data;

        public string? GetData()
            => Data;
    }

    public class OwnedType : INotifyPropertyChanged, INotifyPropertyChanging
    {
        private DbContext? _context;

        public OwnedType()
        {
        }

        public OwnedType(DbContext context)
            => Context = context;

        public DbContext? Context
        {
            get => _context;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs("Context"));
                _context = value;
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs("Context"));
            }
        }

        public int Number { get; set; }

        [NotMapped]
        public PrincipalBase? Principal { get; set; }

        private string? _details;
        private List<short>? _valueTypeList;
        private DateTime[]? _valueTypeArray;
        private IEnumerable<byte>? _valueTypeEnumerable;
        private List<IPAddress>? _refTypeList;
        private IList<string>? _refTypeIList;
        private IPAddress[]? _refTypeArray;
        private IEnumerable<string>? _refTypeEnumerable;

        public string? Details
        {
            get => _details;
            set => _details = value;
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        public event PropertyChangingEventHandler? PropertyChanging;

        public List<short>? ValueTypeList
        {
            get => _valueTypeList;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueTypeList)));
                _valueTypeList = value;
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(ValueTypeList)));
            }
        }

        public IList<byte>? ValueTypeIList { get; set; }

        public DateTime[]? ValueTypeArray
        {
            get => _valueTypeArray;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueTypeArray)));
                _valueTypeArray = value;
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(ValueTypeArray)));
            }
        }

        public IEnumerable<byte>? ValueTypeEnumerable
        {
            get => _valueTypeEnumerable;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(ValueTypeEnumerable)));
                _valueTypeEnumerable = value;
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(ValueTypeEnumerable)));
            }
        }

        public List<IPAddress>? RefTypeList
        {
            get => _refTypeList;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefTypeList)));
                _refTypeList = value;
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(RefTypeList)));
            }
        }

        public IList<string>? RefTypeIList
        {
            get => _refTypeIList;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefTypeIList)));
                _refTypeIList = value;
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(RefTypeIList)));
            }
        }

        public IPAddress[]? RefTypeArray
        {
            get => _refTypeArray;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefTypeArray)));
                _refTypeArray = value;
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(RefTypeArray)));
            }
        }

        public IEnumerable<string>? RefTypeEnumerable
        {
            get => _refTypeEnumerable;
            set
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(RefTypeEnumerable)));
                _refTypeEnumerable = value;
                PropertyChanging?.Invoke(this, new PropertyChangingEventArgs(nameof(RefTypeEnumerable)));
            }
        }
    }

    protected abstract TestHelpers TestHelpers { get; }

    protected override string StoreName
        => "CompiledModelTest";

    private string _filePath = "";

    protected virtual BuildSource AddReferences(
        BuildSource build,
        [CallerFilePath] string filePath = "")
    {
        _filePath = filePath;
        build.References.Add(BuildReference.ByName("System.Linq"));
        build.References.Add(BuildReference.ByName("System.Net.Primitives"));
        build.References.Add(BuildReference.ByName("System.Net.NetworkInformation"));
        build.References.Add(BuildReference.ByName("System.Text.Json"));
        build.References.Add(BuildReference.ByName("System.Threading.Thread"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Abstractions"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Proxies"));
        build.References.Add(BuildReference.ByName("Microsoft.EntityFrameworkCore.Specification.Tests"));
        build.References.Add(BuildReference.ByName(typeof(CompiledModelTestBase).Assembly.GetName().Name!));
        build.References.Add(BuildReference.ByName(GetType().Assembly.GetName().Name!));
        return build;
    }

    protected virtual void AddDesignTimeServices(IServiceCollection services)
    {
    }

    protected virtual Task Test(
        Action<ModelBuilder> onModelCreating,
        Action<IModel>? assertModel = null,
        Func<DbContext, Task>? useContext = null,
        Action<DbContextOptionsBuilder>? onConfiguring = null,
        CompiledModelCodeGenerationOptions? options = null,
        Func<IServiceCollection, IServiceCollection>? addServices = null,
        Func<IServiceCollection, IServiceCollection>? addDesignTimeServices = null,
        IEnumerable<ScaffoldedFile>? additionalSourceFiles = null,
        Action<Assembly>? assertAssembly = null,
        string? expectedExceptionMessage = null,
        [CallerMemberName] string testName = "")
        => Test<DbContext>(
            onModelCreating,
            assertModel,
            useContext,
            onConfiguring,
            options,
            addServices,
            addDesignTimeServices,
            additionalSourceFiles,
            assertAssembly,
            expectedExceptionMessage,
            testName);

    protected virtual async Task<(TContext?, IModel?)> Test<TContext>(
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
        where TContext : DbContext
    {
        var contextFactory = CreateContextFactory<TContext>(
            modelBuilder =>
            {
                var model = modelBuilder.Model;
                ((Model)model).ModelId = new Guid();
                model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
                onModelCreating?.Invoke(modelBuilder);
            },
            onConfiguring,
            addServices);
        using var context = contextFactory.CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;

        options ??= new CompiledModelCodeGenerationOptions { ForNativeAot = true };
        options.ModelNamespace ??= "TestNamespace";
        options.ContextType ??= context.GetType();

        var generator = TestHelpers.CreateDesignServiceProvider(
                context.GetService<IDatabaseProvider>().Name,
                addDesignTimeServices: services =>
                {
                    AddDesignTimeServices(services);
                    addDesignTimeServices?.Invoke(services);
                })
            .GetRequiredService<ICompiledModelCodeGeneratorSelector>()
            .Select(options);

        if (expectedExceptionMessage != null)
        {
            Assert.Equal(
                expectedExceptionMessage,
                Assert.Throws<InvalidOperationException>(
                    () => generator.GenerateModel(
                        model,
                        options)).Message);
            return (null, null);
        }

        var scaffoldedFiles = generator.GenerateModel(
            model,
            options);

        var filesToCompile = scaffoldedFiles;
        if (additionalSourceFiles != null)
        {
            filesToCompile = scaffoldedFiles.Concat(additionalSourceFiles).ToArray();
        }

        var compiledModel = CompileModel(filesToCompile, options, context, assertAssembly);
        assertModel?.Invoke(compiledModel);

        if (additionalSourceFiles == null)
        {
            TestHelpers.ModelAsserter.AssertEqual(context.Model, compiledModel);
        }

        if (useContext != null)
        {
            await TestStore.InitializeAsync(ServiceProvider, contextFactory.CreateContext);
            ListLoggerFactory.Clear();

            using var compiledModelContext = CreateContextFactory<TContext>(
                    onConfiguring: options =>
                    {
                        onConfiguring?.Invoke(options);
                        options.UseModel(compiledModel);
                    },
                    addServices: addServices)
                .CreateContext();
            await useContext(compiledModelContext);
        }

        AssertBaseline(scaffoldedFiles, testName);

        return (context, compiledModel);
    }

    private IModel CompileModel(
        IReadOnlyCollection<ScaffoldedFile> scaffoldedFiles,
        CompiledModelCodeGenerationOptions options,
        DbContext context,
        Action<Assembly>? assertAssembly = null)
    {
        var build = new BuildSource
        {
            Sources = scaffoldedFiles.ToDictionary(f => f.Path, f => f.Code), NullableReferenceTypes = options.UseNullableReferenceTypes
        };
        AddReferences(build);

        var assembly = build.BuildInMemory();

        var modelTypeName = options.ContextType.Name + "Model";
        var modelType = assembly.GetType(
            string.IsNullOrEmpty(options.ModelNamespace)
                ? modelTypeName
                : options.ModelNamespace + "." + modelTypeName)!;
        var instancePropertyInfo = modelType.GetProperty("Instance", BindingFlags.Public | BindingFlags.Static)!;
        var compiledModel = (IModel)instancePropertyInfo.GetValue(null)!;

        var modelRuntimeInitializer = context.GetService<IModelRuntimeInitializer>();
        compiledModel = modelRuntimeInitializer.Initialize(compiledModel, designTime: false);

        assertAssembly?.Invoke(assembly);
        return compiledModel;
    }

    private void AssertBaseline(
        IReadOnlyCollection<ScaffoldedFile> scaffoldedFiles,
        string testName)
    {
        var testDirectory = Path.GetDirectoryName(_filePath);
        if (string.IsNullOrEmpty(testDirectory))
        {
            return; // cannot look for the baseline
        }

        var prefix = Path.DirectorySeparatorChar + "_" + Path.DirectorySeparatorChar;
        if (testDirectory.StartsWith(prefix))
        {
            // assumes a current directory like /path/to/efcore/artifacts/bin/EFCore.Sqlite.FunctionalTests/Release/net9.0
            // so that a path mangled by DeterministicSourcePaths such as /_/test/EFCore.Sqlite.FunctionalTests/Scaffolding becomes
            // /path/to/efcore/test/EFCore.Sqlite.FunctionalTests/Scaffolding
            testDirectory = string.Join(Path.DirectorySeparatorChar, Enumerable.Repeat("..", 5)) + testDirectory[2..];
        }

        if (!Directory.Exists(testDirectory))
        {
            // Source files not available
            return;
        }

        var baselinesDirectory = Path.Combine(testDirectory, "Baselines", testName);
        Directory.CreateDirectory(baselinesDirectory);

        var shouldRewrite = Environment.GetEnvironmentVariable("EF_TEST_REWRITE_BASELINES")?.ToUpper() is "1" or "TRUE";
        List<Exception> exceptions = [];
        foreach (var file in scaffoldedFiles)
        {
            var fullFilePath = Path.Combine(baselinesDirectory, file.Path);
            try
            {
                Assert.Equal(File.ReadAllText(fullFilePath), file.Code, ignoreLineEndingDifferences: true);
            }
            catch (Exception ex)
            {
                if (shouldRewrite)
                {
                    File.WriteAllText(fullFilePath, file.Code);
                }
                else
                {
                    exceptions.Add(new Exception($"Difference found in {file.Path}", ex));
                }
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException($"Differences found in {exceptions.Count} files", exceptions);
        }
    }
}

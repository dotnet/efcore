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

public abstract class CompiledModelTestBase : NonSharedModelTestBase
{
    [ConditionalFact]
    public virtual void SimpleModel()
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
            // Blocked by dotnet/runtime/issues/89439
            //c =>
            //{
            //    c.Add(new DependentDerived<int>(1, "one"));

            //    c.SaveChanges();

            //    var stored = c.Set<DependentDerived<int>>().Single();
            //    Assert.Equal(0, stored.Id);
            //    Assert.Equal(1, stored.GetId());
            //    Assert.Equal("one", stored.GetData());
            //},
            options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true },
            additionalSourceFiles:
            [
                new()
                {
                    Path = "DbContextModelStub.cs",
                    Code = """
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
"""
                }
            ],
            assertAssembly: assembly =>
            {
                var instanceProperty = assembly.GetType("TestNamespace.DbContextModel")!
                    .GetMethod("GetModel", BindingFlags.Public | BindingFlags.Static)!;

                var model = (IModel)instanceProperty.Invoke(null, [])!;
                Assert.NotNull(model);
            });

    [ConditionalFact]
    public virtual void BigModel()
        => Test(
            modelBuilder => BuildBigModel(modelBuilder, jsonColumns: false),
            model => AssertBigModel(model, jsonColumns: false),
            // Blocked by dotnet/runtime/issues/89439
            //c =>
            //{
            //    var principalDerived = new PrincipalDerived<DependentBase<byte?>>
            //    {
            //        AlternateId = new Guid(),
            //        Dependent = new DependentBase<byte?>(1),
            //        Owned = new OwnedType(c)
            //    };

            //    var principalBase = c.Model.FindEntityType(typeof(PrincipalBase))!;
            //    var principalId = principalBase.FindProperty(nameof(PrincipalBase.Id))!;
            //    if (principalId.ValueGenerated == ValueGenerated.Never)
            //    {
            //        principalDerived.Id = 10;
            //    }

            //    c.Add(principalDerived);

            //    c.SaveChanges();
            //},
            options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true });

    protected virtual void BuildBigModel(ModelBuilder modelBuilder, bool jsonColumns)
    {
        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                eb.Property(e => e.FlagsEnum2)
                    .HasSentinel(AFlagsEnum.C | AFlagsEnum.B);

                eb.Property(e => e.AlternateId)
                    .UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);

                eb.HasIndex(e => new { e.AlternateId, e.Id });

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
                            jb.Property<byte[]>("rowid")
                                .IsRowVersion();
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
        var compositeIndex = principalBase.GetIndexes().Single();
        Assert.Equal(PropertyAccessMode.FieldDuringConstruction, principalAlternateId.GetPropertyAccessMode());
        Assert.Empty(compositeIndex.GetAnnotations());
        Assert.Equal(new[] { principalAlternateId, principalId }, compositeIndex.Properties);
        Assert.False(compositeIndex.IsUnique);
        Assert.Null(compositeIndex.Name);

        Assert.Equal(new[] { compositeIndex }, principalAlternateId.GetContainingIndexes());

        Assert.Equal(2, principalBase.GetKeys().Count());

        var principalAlternateKey = principalBase.GetKeys().First();
        Assert.Same(principalId, principalAlternateKey.Properties.Single());
        Assert.False(principalAlternateKey.IsPrimaryKey());

        var principalKey = principalBase.GetKeys().Last();
        Assert.Equal(new[] { principalId, principalAlternateId }, principalKey.Properties);
        Assert.True(principalKey.IsPrimaryKey());

        Assert.Equal(new[] { principalAlternateKey, principalKey }, principalId.GetContainingKeys());

        var referenceOwnedNavigation = principalBase.GetNavigations().Single();
        Assert.Equal(
            new[] { CoreAnnotationNames.EagerLoaded },
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
        Assert.Equal("PrincipalDerived<DependentBase<byte?>>", principalDerived.GetDiscriminatorValue());

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

        Assert.Equal(new[] { derivedSkipNavigation.Inverse, derivedSkipNavigation }, principalDerived.GetSkipNavigations());

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

        var rowid = joinType.GetProperties().Single(p => !p.IsForeignKey());
        Assert.Equal(typeof(byte[]), rowid.ClrType);
        Assert.True(rowid.IsIndexerProperty());
        Assert.Same(joinType.FindIndexerPropertyInfo(), rowid.PropertyInfo);
        Assert.Null(rowid.FieldInfo);
        Assert.True(rowid.IsNullable);
        Assert.False(rowid.IsShadowProperty());
        Assert.True(rowid.IsConcurrencyToken);
        Assert.Equal(ValueGenerated.OnAddOrUpdate, rowid.ValueGenerated);
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
        Assert.Equal(new[] { "PrincipalId", "PrincipalAlternateId" }, dependentForeignKey.Properties.Select(p => p.Name));
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
            new[] { dependentBaseForeignKey, dependentForeignKey }, dependentForeignKeyProperty.GetContainingForeignKeys());

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
            new[] { derivedSkipNavigation.ForeignKey, collectionOwnership, dependentForeignKey },
            principalDerived.GetDeclaredReferencingForeignKeys());
    }

    [ConditionalFact]
    public virtual void ComplexTypes()
        => Test(
            BuildComplexTypesModel,
            AssertComplexTypes,
            c =>
            {
                c.Set<PrincipalDerived<DependentBase<byte?>>>().Add(
                    new PrincipalDerived<DependentBase<byte?>>
                    {
                        Id = 1,
                        AlternateId = new Guid(),
                        Dependent = new DependentBase<byte?>(1),
                        Owned = new OwnedType(c) { Principal = new PrincipalBase() }
                    });

                //c.SaveChanges();

                return Task.CompletedTask;
            },
            options: new CompiledModelCodeGenerationOptions { UseNullableReferenceTypes = true });

    protected virtual void BuildComplexTypesModel(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<PrincipalBase>(
            eb =>
            {
                eb.Ignore(e => e.Owned);
                eb.ComplexProperty(
                    e => e.Owned, eb =>
                    {
                        eb.IsRequired()
                            .HasField("_ownedField")
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
                            .IsRowVersion()
                            .HasAnnotation("foo", "bar");
                        eb.Ignore(e => e.Context);
                        eb.ComplexProperty(o => o.Principal).IsRequired();
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
            new[] { "goo" },
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
        Assert.Equal(ValueGenerated.OnAddOrUpdate, detailsProperty.ValueGenerated);
        Assert.Equal(PropertySaveBehavior.Ignore, detailsProperty.GetAfterSaveBehavior());
        Assert.Equal(PropertySaveBehavior.Ignore, detailsProperty.GetBeforeSaveBehavior());
        Assert.False(detailsProperty.IsUnicode());
        Assert.True(detailsProperty.IsConcurrencyToken);
        Assert.Equal(64, detailsProperty.GetMaxLength());
        Assert.Equal(3, detailsProperty.GetPrecision());
        Assert.Equal(2, detailsProperty.GetScale());
        Assert.Equal("", detailsProperty.Sentinel);
        Assert.Equal(PropertyAccessMode.FieldDuringConstruction, detailsProperty.GetPropertyAccessMode());
        Assert.Null(detailsProperty.GetValueConverter());
        Assert.NotNull(detailsProperty.GetValueComparer());
        Assert.NotNull(detailsProperty.GetKeyValueComparer());

        var nestedComplexType = complexType.FindComplexProperty(nameof(OwnedType.Principal))!.ComplexType;

        Assert.Equal(14, nestedComplexType.GetProperties().Count());

        var principalDerived = model.FindEntityType(typeof(PrincipalDerived<DependentBase<byte?>>))!;
        Assert.Equal(principalBase, principalDerived.BaseType);

        Assert.Equal(
            new[] { principalBase, principalDerived },
            model.GetEntityTypes());
    }

    public class CustomValueComparer<T> : ValueComparer<T>
    {
        public CustomValueComparer()
            : base(false)
        {
        }
    }

    public class ManyTypesIdConverter : ValueConverter<ManyTypesId, int>
    {
        public ManyTypesIdConverter()
            : base(v => v.Id, v => new ManyTypesId(v))
        {
        }
    }

    public class NullIntToNullStringConverter : ValueConverter<int?, string?>
    {
        public NullIntToNullStringConverter()
            : base(v => v == null ? null : v.ToString()!, v => v == null || v == "<null>" ? null : int.Parse(v), convertsNulls: true)
        {
        }
    }

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
        public AFlagsEnum FlagsEnum2 { get; set; }

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

    public class DependentDerived<TKey> : DependentBase<TKey>
    {
        public DependentDerived(TKey id, string data)
            : base(id)
        {
            Data = data;
        }

        private string? Data { get; set; }

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
        {
            Context = context;
        }

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
        using var context = (await CreateContextFactory<TContext>(
            modelBuilder =>
            {
                var model = modelBuilder.Model;
                ((Model)model).ModelId = new Guid();
                model.RemoveAnnotation(CoreAnnotationNames.ProductVersion);
                onModelCreating?.Invoke(modelBuilder);
            },
            onConfiguring,
            addServices)).CreateContext();
        var model = context.GetService<IDesignTimeModel>().Model;

        options ??= new CompiledModelCodeGenerationOptions();
        options.ModelNamespace ??= "TestNamespace";
        options.ContextType ??= context.GetType();
        // options.UseNullableReferenceTypes = false;

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
            var contextFactory = await CreateContextFactory<TContext>(
                onConfiguring: options =>
                {
                    onConfiguring?.Invoke(options);
                    options.UseModel(compiledModel);
                },
                addServices: addServices);
            ListLoggerFactory.Clear();
            await TestStore.InitializeAsync(ServiceProvider, contextFactory.CreateContext, c => useContext((TContext)c));
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
        if (string.IsNullOrEmpty(testDirectory)
            || !Directory.Exists(testDirectory))
        {
            return;
        }

        var baselinesDirectory = Path.Combine(testDirectory, "Baselines", testName);
        try
        {
            Directory.CreateDirectory(baselinesDirectory);
        }
        catch
        {
            return;
        }

        var shouldRewrite = Environment.GetEnvironmentVariable("EF_TEST_REWRITE_BASELINES")?.ToUpper() is "1" or "TRUE";
        foreach (var file in scaffoldedFiles)
        {
            var fullFilePath = Path.Combine(baselinesDirectory, file.Path);
            if (!File.Exists(fullFilePath)
                || shouldRewrite)
            {
                File.WriteAllText(fullFilePath, file.Code);
            }
            else
            {
                try
                {
                    Assert.Equal(File.ReadAllText(fullFilePath), file.Code, ignoreLineEndingDifferences: true);
                }
                catch (Exception ex)
                {
                    throw new Exception($"Difference found in {file.Path}", ex);
                }
            }
        }
    }
}

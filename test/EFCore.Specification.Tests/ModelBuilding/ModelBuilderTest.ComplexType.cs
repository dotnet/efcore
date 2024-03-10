// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using System.Collections.ObjectModel;
using System.Dynamic;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.ModelBuilding;

#nullable disable

public abstract partial class ModelBuilderTest
{
    public abstract class ComplexTypeTestBase(ModelBuilderFixtureBase fixture) : ModelBuilderTestBase(fixture)
    {
        [ConditionalFact]
        public virtual void Can_set_complex_property_annotation()
        {
            var modelBuilder = CreateModelBuilder();

            var complexPropertyBuilder = modelBuilder
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer)
                .HasTypeAnnotation("foo", "bar")
                .HasPropertyAnnotation("foo2", "bar2")
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders);

            var model = modelBuilder.FinalizeModel();
            var complexProperty = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single();

            Assert.Equal("bar", complexProperty.ComplexType["foo"]);
            Assert.Equal("bar2", complexProperty["foo2"]);
            Assert.Equal(typeof(Customer).Name, complexProperty.Name);
            Assert.Equal(
                @"Customer (Customer) Required
  ComplexType: ComplexProperties.Customer#Customer
    Properties: "
                + @"
      AlternateKey (Guid) Required
      Id (int) Required
      Name (string)
      Notes (List<string>) Element type: string Required", complexProperty.ToDebugString(), ignoreLineEndingDifferences: true);
        }

        [ConditionalFact]
        public virtual void Can_set_property_annotation()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Product>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer)
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders)
                .Property(c => c.Name).HasAnnotation("foo", "bar");

            var model = modelBuilder.FinalizeModel();
            var complexProperty = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single();
            var property = complexProperty.ComplexType.FindProperty(nameof(Customer.Name));

            Assert.Equal("bar", property["foo"]);
        }

        [ConditionalFact]
        public virtual void Can_set_property_annotation_when_no_clr_property()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Product>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer)
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders)
                .Property<string>(Customer.NameProperty.Name).HasAnnotation("foo", "bar");

            var model = modelBuilder.FinalizeModel();
            var complexProperty = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single();
            var property = complexProperty.ComplexType.FindProperty(nameof(Customer.Name));

            Assert.Equal("bar", property["foo"]);
        }

        [ConditionalFact]
        public virtual void Can_set_property_annotation_by_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.Properties<string>().HaveAnnotation("foo", "bar"));

            modelBuilder
                .Ignore<Product>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer)
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders)
                .Property(c => c.Name);

            var model = modelBuilder.FinalizeModel();
            var complexProperty = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single();
            var property = complexProperty.ComplexType.FindProperty(nameof(Customer.Name));

            Assert.Equal("bar", property["foo"]);
        }

        [ConditionalFact]
        public virtual void Properties_are_required_by_default_only_if_CLR_type_is_nullable()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down);
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.False(complexType.FindProperty("Up").IsNullable);
            Assert.True(complexType.FindProperty("Down").IsNullable);
            Assert.False(complexType.FindProperty("Charm").IsNullable);
            Assert.True(complexType.FindProperty("Strange").IsNullable);
            Assert.False(complexType.FindProperty("Top").IsNullable);
            Assert.True(complexType.FindProperty("Bottom").IsNullable);
        }

        [ConditionalFact]
        public virtual void Properties_can_be_ignored()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Ignore(e => e.Up);
                        b.Ignore(e => e.Down);
                        b.Ignore("Charm");
                        b.Ignore("Strange");
                        b.Ignore("Top");
                        b.Ignore("Bottom");
                        b.Ignore("Shadow");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Contains(nameof(Quarks.Id), complexType.GetProperties().Select(p => p.Name));
            Assert.DoesNotContain(nameof(Quarks.Up), complexType.GetProperties().Select(p => p.Name));
            Assert.DoesNotContain(nameof(Quarks.Down), complexType.GetProperties().Select(p => p.Name));
        }

        [ConditionalFact]
        public virtual void Properties_can_be_ignored_by_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.IgnoreAny<Guid>());

            modelBuilder
                .Ignore<Product>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer, b => b.Ignore(c => c.Details).Ignore(c => c.Orders));

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;
            Assert.Null(complexType.FindProperty(nameof(Customer.AlternateKey)));
        }

        [ConditionalFact]
        public virtual void Can_ignore_shadow_properties_when_they_have_been_added_explicitly()
        {
            var modelBuilder = CreateModelBuilder();

            var complexPropertyBuilder = modelBuilder
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer, b => b.Ignore(c => c.Details).Ignore(c => c.Orders));
            complexPropertyBuilder.Property<string>("Shadow");
            complexPropertyBuilder.Ignore("Shadow");

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;
            Assert.Null(complexType.FindProperty("Shadow"));
        }

        [ConditionalFact]
        public virtual void Can_add_shadow_properties_when_they_have_been_ignored()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Product>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Customer,
                    b =>
                    {
                        b.Ignore(c => c.Details);
                        b.Ignore(c => c.Orders);
                        b.Ignore("Shadow");
                        b.Property<string>("Shadow");
                    });

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;
            Assert.NotNull(complexType.FindProperty("Shadow"));
        }

        [ConditionalFact]
        public virtual void Properties_can_be_made_required()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).IsRequired();
                        b.Property(e => e.Down).IsRequired();
                        b.Property<int>("Charm").IsRequired();
                        b.Property<string>("Strange").IsRequired();
                        b.Property<int>("Top").IsRequired();
                        b.Property<string>("Bottom").IsRequired();
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.False(complexType.FindProperty("Up").IsNullable);
            Assert.False(complexType.FindProperty("Down").IsNullable);
            Assert.False(complexType.FindProperty("Charm").IsNullable);
            Assert.False(complexType.FindProperty("Strange").IsNullable);
            Assert.False(complexType.FindProperty("Top").IsNullable);
            Assert.False(complexType.FindProperty("Bottom").IsNullable);
        }

        [ConditionalFact]
        public virtual void Properties_can_be_made_optional()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Down).IsRequired(false);
                        b.Property<string>("Strange").IsRequired(false);
                        b.Property<string>("Bottom").IsRequired(false);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.True(complexType.FindProperty("Down").IsNullable);
            Assert.True(complexType.FindProperty("Strange").IsNullable);
            Assert.True(complexType.FindProperty("Bottom").IsNullable);
        }

        [ConditionalFact]
        public virtual void Non_nullable_properties_cannot_be_made_optional()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.CannotBeNullable("Up", "ComplexProperties.Quarks#Quarks", "int"),
                            Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Up).IsRequired(false)).Message);

                        Assert.Equal(
                            CoreStrings.CannotBeNullable("Charm", "ComplexProperties.Quarks#Quarks", "int"),
                            Assert.Throws<InvalidOperationException>(() => b.Property<int>("Charm").IsRequired(false)).Message);

                        Assert.Equal(
                            CoreStrings.CannotBeNullable("Top", "ComplexProperties.Quarks#Quarks", "int"),
                            Assert.Throws<InvalidOperationException>(() => b.Property<int>("Top").IsRequired(false)).Message);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.False(complexType.FindProperty("Up").IsNullable);
            Assert.False(complexType.FindProperty("Charm").IsNullable);
            Assert.False(complexType.FindProperty("Top").IsNullable);
        }

        [ConditionalFact]
        public virtual void Properties_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property<int>("Up");
                        b.Property<int>("Gluon");
                        b.Property<string>("Down");
                        b.Property<string>("Photon");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.False(complexType.FindProperty("Up").IsShadowProperty());
            Assert.False(complexType.FindProperty("Down").IsShadowProperty());
            Assert.True(complexType.FindProperty("Gluon").IsShadowProperty());
            Assert.True(complexType.FindProperty("Photon").IsShadowProperty());

            Assert.Equal(-1, complexType.FindProperty("Up").GetShadowIndex());
            Assert.Equal(-1, complexType.FindProperty("Down").GetShadowIndex());
            Assert.NotEqual(-1, complexType.FindProperty("Gluon").GetShadowIndex());
            Assert.NotEqual(-1, complexType.FindProperty("Photon").GetShadowIndex());
            Assert.NotEqual(complexType.FindProperty("Gluon").GetShadowIndex(), complexType.FindProperty("Photon").GetShadowIndex());
        }

        [ConditionalFact]
        public virtual void Properties_can_be_made_concurrency_tokens()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).IsConcurrencyToken();
                        b.Property(e => e.Down).IsConcurrencyToken(false);
                        b.Property<int>("Charm").IsConcurrencyToken();
                        b.Property<string>("Strange").IsConcurrencyToken(false);
                        b.Property<int>("Top").IsConcurrencyToken();
                        b.Property<string>("Bottom").IsConcurrencyToken(false);
                        b.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.False(complexType.FindProperty(Customer.IdProperty.Name).IsConcurrencyToken);
            Assert.True(complexType.FindProperty("Up").IsConcurrencyToken);
            Assert.False(complexType.FindProperty("Down").IsConcurrencyToken);
            Assert.True(complexType.FindProperty("Charm").IsConcurrencyToken);
            Assert.False(complexType.FindProperty("Strange").IsConcurrencyToken);
            Assert.True(complexType.FindProperty("Top").IsConcurrencyToken);
            Assert.False(complexType.FindProperty("Bottom").IsConcurrencyToken);

            Assert.Equal(-1, complexType.FindProperty(Customer.IdProperty.Name).GetOriginalValueIndex());
            Assert.Equal(6, complexType.FindProperty("Up").GetOriginalValueIndex());
            Assert.Equal(-1, complexType.FindProperty("Down").GetOriginalValueIndex());
            Assert.Equal(4, complexType.FindProperty("Charm").GetOriginalValueIndex());
            Assert.Equal(-1, complexType.FindProperty("Strange").GetOriginalValueIndex());
            Assert.Equal(5, complexType.FindProperty("Top").GetOriginalValueIndex());
            Assert.Equal(-1, complexType.FindProperty("Bottom").GetOriginalValueIndex());

            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, complexType.GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_access_mode_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down).HasField("_forDown").UsePropertyAccessMode(PropertyAccessMode.Field);
                        b.Property<int>("Charm").UsePropertyAccessMode(PropertyAccessMode.Property);
                        b.Property<string>("Strange").UsePropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                    })
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
                        b.UseDefaultPropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                        b.PrimitiveCollection(e => e.Up).UsePropertyAccessMode(PropertyAccessMode.Property);
                        b.PrimitiveCollection(e => e.Down).HasField("_forDown");
                    });

            var model = modelBuilder.FinalizeModel();

            var quarksType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties()
                .Single(p => p.Name == nameof(Quarks)).ComplexType;

            Assert.Equal(PropertyAccessMode.PreferField, quarksType.FindProperty("Up")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Field, quarksType.FindProperty("Down")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, quarksType.FindProperty("Charm")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, quarksType.FindProperty("Strange")!.GetPropertyAccessMode());

            quarksType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties()
                .Single(p => p.Name == nameof(CollectionQuarks)).ComplexType;

            Assert.Equal(PropertyAccessMode.Property, quarksType.FindProperty("Up")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, quarksType.FindProperty("Down")!.GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Access_mode_can_be_overridden_at_entity_and_property_levels()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder.UsePropertyAccessMode(PropertyAccessMode.Field);

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer, b => b.Ignore(c => c.Details).Ignore(c => c.Orders));

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
                        b.UseDefaultPropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                        b.Property(e => e.Up).UsePropertyAccessMode(PropertyAccessMode.Property);
                        b.Property(e => e.Down).HasField("_forDown");
                    })
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.UsePropertyAccessMode(PropertyAccessMode.PreferFieldDuringConstruction);
                        b.UseDefaultPropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
                        b.PrimitiveCollection(e => e.Up).UsePropertyAccessMode(PropertyAccessMode.Property);
                        b.PrimitiveCollection(e => e.Down).HasField("_forDown");
                    });

            var model = modelBuilder.FinalizeModel();
            AssertEqual(modelBuilder.Model, model);

            var entityType = model.FindEntityType(typeof(ComplexProperties))!;
            Assert.Equal(PropertyAccessMode.Field, model.GetPropertyAccessMode());

            var quarksProperty = entityType.FindComplexProperty(nameof(ComplexProperties.Quarks))!;
            var quarksType = quarksProperty.ComplexType;
            Assert.Equal(PropertyAccessMode.PreferFieldDuringConstruction, quarksProperty.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, quarksType.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, quarksType.FindProperty("Down")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, quarksType.FindProperty("Up")!.GetPropertyAccessMode());

            quarksProperty = entityType.FindComplexProperty(nameof(ComplexProperties.CollectionQuarks))!;
            quarksType = quarksProperty.ComplexType;
            Assert.Equal(PropertyAccessMode.PreferFieldDuringConstruction, quarksProperty.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, quarksType.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, quarksType.FindProperty("Down")!.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, quarksType.FindProperty("Up")!.GetPropertyAccessMode());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_provider_type_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down).HasConversion<byte[]>();
                        b.Property<int>("Charm").HasConversion<long, CustomValueComparer<int>>();
                        b.Property<string>("Strange").HasConversion<byte[]>(
                            new CustomValueComparer<string>(), new CustomValueComparer<byte[]>());
                        b.Property<string>("Strange").HasConversion(null);
                        b.Property<string>("Top").HasConversion<string>(new CustomValueComparer<string>());
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            var up = complexType.FindProperty("Up");
            Assert.Null(up.GetProviderClrType());
            Assert.True(up.GetValueComparer().IsDefault());

            var down = complexType.FindProperty("Down");
            Assert.Same(typeof(byte[]), down.GetProviderClrType());
            Assert.True(down.GetValueComparer().IsDefault());
            Assert.IsType<ValueComparer<byte[]>>(down.GetProviderValueComparer());

            var charm = complexType.FindProperty("Charm");
            Assert.Same(typeof(long), charm.GetProviderClrType());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());
            Assert.True(charm.GetProviderValueComparer().IsDefault());

            var strange = complexType.FindProperty("Strange");
            Assert.Null(strange.GetProviderClrType());
            Assert.True(strange.GetValueComparer().IsDefault());
            Assert.True(strange.GetProviderValueComparer().IsDefault());

            var top = complexType.FindProperty("Top");
            Assert.Same(typeof(string), top.GetProviderClrType());
            Assert.IsType<CustomValueComparer<string>>(top.GetValueComparer());
            Assert.IsType<CustomValueComparer<string>>(top.GetProviderValueComparer());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_provider_type_set_for_type()
        {
            var modelBuilder = CreateModelBuilder(c => c.Properties<string>().HaveConversion<byte[]>());

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down);
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Null(complexType.FindProperty("Up").GetProviderClrType());
            Assert.Same(typeof(byte[]), complexType.FindProperty("Down").GetProviderClrType());
            Assert.Null(complexType.FindProperty("Charm").GetProviderClrType());
            Assert.Same(typeof(byte[]), complexType.FindProperty("Strange").GetProviderClrType());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_non_generic_value_converter_set()
        {
            var modelBuilder = CreateModelBuilder();

            ValueConverter stringConverter = new StringToBytesConverter(Encoding.UTF8);
            ValueConverter intConverter = new CastingConverter<int, long>();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down).HasConversion(stringConverter);
                        b.Property<int>("Charm").HasConversion(intConverter, null, new CustomValueComparer<long>());
                        b.Property<string>("Strange").HasConversion(stringConverter);
                        b.Property<string>("Strange").HasConversion(null);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Null(complexType.FindProperty("Up").GetValueConverter());

            var down = complexType.FindProperty("Down");
            Assert.Same(stringConverter, down.GetValueConverter());
            Assert.True(down.GetValueComparer().IsDefault());
            Assert.IsType<ValueComparer<byte[]>>(down.GetProviderValueComparer());

            var charm = complexType.FindProperty("Charm");
            Assert.Same(intConverter, charm.GetValueConverter());
            Assert.True(charm.GetValueComparer().IsDefault());
            Assert.IsType<CustomValueComparer<long>>(charm.GetProviderValueComparer());

            Assert.Null(complexType.FindProperty("Strange").GetValueConverter());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_custom_type_value_converter_type_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).HasConversion<int, CustomValueComparer<int>>();
                        b.Property(e => e.Down)
                            .HasConversion<UTF8StringToBytesConverter, CustomValueComparer<string>, CustomValueComparer<byte[]>>();
                        b.Property<int>("Charm").HasConversion<CastingConverter<int, long>, CustomValueComparer<int>>();
                        b.Property<string>("Strange").HasConversion<UTF8StringToBytesConverter, CustomValueComparer<string>>();
                        b.Property<string>("Strange").HasConversion(null, null);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            var up = complexType.FindProperty("Up");
            Assert.Equal(typeof(int), up.GetProviderClrType());
            Assert.Null(up.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(up.GetValueComparer());
            Assert.IsType<CustomValueComparer<int>>(up.GetProviderValueComparer());

            var down = complexType.FindProperty("Down");
            Assert.IsType<UTF8StringToBytesConverter>(down.GetValueConverter());
            Assert.IsType<CustomValueComparer<string>>(down.GetValueComparer());
            Assert.IsType<CustomValueComparer<byte[]>>(down.GetProviderValueComparer());

            var charm = complexType.FindProperty("Charm");
            Assert.IsType<CastingConverter<int, long>>(charm.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());
            Assert.True(charm.GetProviderValueComparer().IsDefault());

            var strange = complexType.FindProperty("Strange");
            Assert.Null(strange.GetValueConverter());
            Assert.True(strange.GetValueComparer().IsDefault());
            Assert.True(strange.GetProviderValueComparer().IsDefault());
        }

        private class UTF8StringToBytesConverter : StringToBytesConverter
        {
            public UTF8StringToBytesConverter()
                : base(Encoding.UTF8)
            {
            }
        }

        private class CustomValueComparer<T> : ValueComparer<T>
        {
            public CustomValueComparer()
                : base(false)
            {
            }
        }

        [ConditionalFact]
        public virtual void Properties_can_have_value_converter_set_inline()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down).HasConversion(v => int.Parse(v), v => v.ToString());
                        b.Property<int>("Charm").HasConversion(v => (long)v, v => (int)v, new CustomValueComparer<int>());
                        b.Property<float>("Strange").HasConversion(
                            v => (double)v, v => (float)v, new CustomValueComparer<float>(), new CustomValueComparer<double>());
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            var up = complexType.FindProperty("Up");
            Assert.Null(up.GetProviderClrType());
            Assert.Null(up.GetValueConverter());
            Assert.True(up.GetValueComparer().IsDefault());
            Assert.True(up.GetProviderValueComparer().IsDefault());

            var down = complexType.FindProperty("Down");
            Assert.IsType<ValueConverter<string, int>>(down.GetValueConverter());
            Assert.True(down.GetValueComparer().IsDefault());
            Assert.True(down.GetProviderValueComparer().IsDefault());

            var charm = complexType.FindProperty("Charm");
            Assert.IsType<ValueConverter<int, long>>(charm.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());
            Assert.True(charm.GetProviderValueComparer().IsDefault());

            var strange = complexType.FindProperty("Strange");
            Assert.IsType<ValueConverter<float, double>>(strange.GetValueConverter());
            Assert.IsType<CustomValueComparer<float>>(strange.GetValueComparer());
            Assert.IsType<CustomValueComparer<double>>(strange.GetProviderValueComparer());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_value_converter_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up);
                        b.Property(e => e.Down).HasConversion(
                            new ValueConverter<string, int>(v => int.Parse(v), v => v.ToString()));
                        b.Property<int>("Charm").HasConversion(
                            new ValueConverter<int, long>(v => v, v => (int)v), new CustomValueComparer<int>());
                        b.Property<float>("Strange").HasConversion(
                            new ValueConverter<float, double>(v => v, v => (float)v), new CustomValueComparer<float>(),
                            new CustomValueComparer<double>());
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            var up = complexType.FindProperty("Up");
            Assert.Null(up.GetProviderClrType());
            Assert.Null(up.GetValueConverter());
            Assert.True(up.GetValueComparer().IsDefault());
            Assert.True(up.GetProviderValueComparer().IsDefault());

            var down = complexType.FindProperty("Down");
            Assert.IsType<ValueConverter<string, int>>(down.GetValueConverter());
            Assert.True(down.GetValueComparer().IsDefault());
            Assert.True(down.GetProviderValueComparer().IsDefault());

            var charm = complexType.FindProperty("Charm");
            Assert.IsType<ValueConverter<int, long>>(charm.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(charm.GetValueComparer());
            Assert.True(charm.GetProviderValueComparer().IsDefault());

            var strange = complexType.FindProperty("Strange");
            Assert.IsType<ValueConverter<float, double>>(strange.GetValueConverter());
            Assert.IsType<CustomValueComparer<float>>(strange.GetValueComparer());
            Assert.IsType<CustomValueComparer<double>>(strange.GetProviderValueComparer());
        }

        [ConditionalFact]
        public virtual void IEnumerable_properties_with_value_converter_set_are_not_discovered_as_complex_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.DynamicProperty,
                    b =>
                    {
                        b.Property(e => e.ExpandoObject).HasConversion(
                            v => (string)((IDictionary<string, object>)v)["Value"], v => DeserializeExpandoObject(v));

                        var comparer = new ValueComparer<ExpandoObject>(
                            (v1, v2) => v1.SequenceEqual(v2),
                            v => v.GetHashCode());

                        b.Property(e => e.ExpandoObject).Metadata.SetValueComparer(comparer);
                    });

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.NotNull(complexType.FindProperty(nameof(DynamicProperty.ExpandoObject)).GetValueConverter());
            Assert.NotNull(complexType.FindProperty(nameof(DynamicProperty.ExpandoObject)).GetValueComparer());
        }

        private static ExpandoObject DeserializeExpandoObject(string value)
        {
            dynamic obj = new ExpandoObject();
            obj.Value = value;

            return obj;
        }

        private class ExpandoObjectConverter : ValueConverter<ExpandoObject, string>
        {
            public ExpandoObjectConverter()
                : base(v => (string)((IDictionary<string, object>)v)["Value"], v => DeserializeExpandoObject(v))
            {
            }
        }

        private class ExpandoObjectComparer : ValueComparer<ExpandoObject>
        {
            public ExpandoObjectComparer()
                : base((v1, v2) => v1.SequenceEqual(v2), v => v.GetHashCode())
            {
            }
        }

        [ConditionalFact]
        public virtual void Properties_can_have_value_converter_configured_by_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties(typeof(IWrapped<>)).AreUnicode(false);
                    c.Properties<WrappedStringBase>().HaveMaxLength(20);
                    c.Properties<WrappedString>().HaveConversion(typeof(WrappedStringToStringConverter));
                });

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.WrappedStringEntity);

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;
            var wrappedProperty = complexType.FindProperty(nameof(WrappedStringEntity.WrappedString));
            Assert.False(wrappedProperty.IsUnicode());
            Assert.Equal(20, wrappedProperty.GetMaxLength());
            Assert.IsType<WrappedStringToStringConverter>(wrappedProperty.GetValueConverter());
            Assert.IsType<ValueComparer<WrappedString>>(wrappedProperty.GetValueComparer());
        }

        [ConditionalFact]
        public virtual void Value_converter_configured_on_non_nullable_type_is_applied()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().HaveConversion<NumberToStringConverter<int>, CustomValueComparer<int>>();
                });

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property<int?>("Wierd");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            var id = complexType.FindProperty("Id");
            Assert.IsType<NumberToStringConverter<int>>(id.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(id.GetValueComparer());

            var wierd = complexType.FindProperty("Wierd");
            Assert.IsType<NumberToStringConverter<int>>(wierd.GetValueConverter());
            Assert.IsType<ValueComparer<int?>>(wierd.GetValueComparer());
        }

        [ConditionalFact]
        public virtual void Value_converter_configured_on_nullable_type_overrides_non_nullable()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int?>().HaveConversion<NumberToStringConverter<int?>, CustomValueComparer<int?>>();
                    c.Properties<int>()
                        .HaveConversion<NumberToStringConverter<int>, CustomValueComparer<int>, CustomValueComparer<string>>();
                });

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property<int?>("Wierd");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            var id = complexType.FindProperty("Id");
            Assert.IsType<NumberToStringConverter<int>>(id.GetValueConverter());
            Assert.IsType<CustomValueComparer<int>>(id.GetValueComparer());
            Assert.IsType<CustomValueComparer<string>>(id.GetProviderValueComparer());

            var wierd = complexType.FindProperty("Wierd");
            Assert.IsType<NumberToStringConverter<int?>>(wierd.GetValueConverter());
            Assert.IsType<CustomValueComparer<int?>>(wierd.GetValueComparer());
            Assert.IsType<CustomValueComparer<string>>(wierd.GetProviderValueComparer());
        }

        private class WrappedStringToStringConverter : ValueConverter<WrappedString, string>
        {
            public WrappedStringToStringConverter()
                : base(v => v.Value, v => new WrappedString { Value = v })
            {
            }
        }

        [ConditionalFact]
        public virtual void Value_converter_type_is_checked()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.ConverterPropertyMismatch("string", "ComplexProperties.Quarks#Quarks", "Up", "int"),
                            Assert.Throws<InvalidOperationException>(
                                () => b.Property(e => e.Up).HasConversion(
                                    new StringToBytesConverter(Encoding.UTF8))).Message);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;
            Assert.Null(complexType.FindProperty("Up").GetValueConverter());
        }

        [ConditionalFact]
        public virtual void Properties_can_have_field_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property<int>("Up").HasField("_forUp");
                        b.Property(e => e.Down).HasField("_forDown");
                        b.Property<int?>("_forWierd").HasField("_forWierd");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Equal("_forUp", complexType.FindProperty("Up").GetFieldName());
            Assert.Equal("_forDown", complexType.FindProperty("Down").GetFieldName());
            Assert.Equal("_forWierd", complexType.FindProperty("_forWierd").GetFieldName());
        }

        [ConditionalFact]
        public virtual void HasField_throws_if_field_is_not_found()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.MissingBackingField("_notFound", nameof(Quarks.Down), "ComplexProperties.Quarks#Quarks"),
                            Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Down).HasField("_notFound")).Message);
                    });
        }

        [ConditionalFact]
        public virtual void HasField_throws_if_field_is_wrong_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.BadBackingFieldType("_forUp", "int", nameof(Quarks), nameof(Quarks.Down), "string"),
                            Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Down).HasField("_forUp")).Message);
                    });
        }

        [ConditionalFact]
        public virtual void Properties_can_be_set_to_generate_values_on_Add()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).ValueGeneratedOnAddOrUpdate();
                        b.Property(e => e.Down).ValueGeneratedNever();
                        b.Property<int>("Charm").Metadata.ValueGenerated = ValueGenerated.OnUpdateSometimes;
                        b.Property<string>("Strange").ValueGeneratedNever();
                        b.Property<int>("Top").ValueGeneratedOnAddOrUpdate();
                        b.Property<string>("Bottom").ValueGeneratedOnUpdate();
                    });

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;
            Assert.Equal(ValueGenerated.Never, complexType.FindProperty(Customer.IdProperty.Name).ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, complexType.FindProperty("Up").ValueGenerated);
            Assert.Equal(ValueGenerated.Never, complexType.FindProperty("Down").ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdateSometimes, complexType.FindProperty("Charm").ValueGenerated);
            Assert.Equal(ValueGenerated.Never, complexType.FindProperty("Strange").ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, complexType.FindProperty("Top").ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdate, complexType.FindProperty("Bottom").ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Properties_can_set_row_version()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).IsRowVersion();
                        b.Property(e => e.Down).ValueGeneratedNever();
                        b.Property<int>("Charm").IsRowVersion();
                    });

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Equal(ValueGenerated.OnAddOrUpdate, complexType.FindProperty("Up").ValueGenerated);
            Assert.Equal(ValueGenerated.Never, complexType.FindProperty("Down").ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, complexType.FindProperty("Charm").ValueGenerated);

            Assert.True(complexType.FindProperty("Up").IsConcurrencyToken);
            Assert.False(complexType.FindProperty("Down").IsConcurrencyToken);
            Assert.True(complexType.FindProperty("Charm").IsConcurrencyToken);
        }

        [ConditionalFact]
        public virtual void Can_set_max_length_for_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).HasMaxLength(0);
                        b.Property(e => e.Down).HasMaxLength(100);
                        b.Property<int>("Charm").HasMaxLength(0);
                        b.Property<string>("Strange").HasMaxLength(-1);
                        b.Property<int>("Top").HasMaxLength(0);
                        b.Property<string>("Bottom").HasMaxLength(100);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Null(complexType.FindProperty(Customer.IdProperty.Name).GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Up").GetMaxLength());
            Assert.Equal(100, complexType.FindProperty("Down").GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Charm").GetMaxLength());
            Assert.Equal(-1, complexType.FindProperty("Strange").GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Top").GetMaxLength());
            Assert.Equal(100, complexType.FindProperty("Bottom").GetMaxLength());
        }

        [ConditionalFact]
        public virtual void Can_set_max_length_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().HaveMaxLength(0);
                    c.Properties<string>().HaveMaxLength(100);
                });

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Equal(0, complexType.FindProperty(Customer.IdProperty.Name).GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Up").GetMaxLength());
            Assert.Equal(100, complexType.FindProperty("Down").GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Charm").GetMaxLength());
            Assert.Equal(100, complexType.FindProperty("Strange").GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Top").GetMaxLength());
            Assert.Equal(100, complexType.FindProperty("Bottom").GetMaxLength());
        }

        [ConditionalFact]
        public virtual void Can_set_sentinel_for_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).HasSentinel(1);
                        b.Property(e => e.Down).HasSentinel("100");
                        b.Property<int>("Charm").HasSentinel(-1);
                        b.Property<string>("Strange").HasSentinel("-1");
                        b.Property<int>("Top").HasSentinel(77);
                        b.Property<string>("Bottom").HasSentinel("100");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Equal(0, complexType.FindProperty(Customer.IdProperty.Name)!.Sentinel);
            Assert.Equal(1, complexType.FindProperty("Up")!.Sentinel);
            Assert.Equal("100", complexType.FindProperty("Down")!.Sentinel);
            Assert.Equal(-1, complexType.FindProperty("Charm")!.Sentinel);
            Assert.Equal("-1", complexType.FindProperty("Strange")!.Sentinel);
            Assert.Equal(77, complexType.FindProperty("Top")!.Sentinel);
            Assert.Equal("100", complexType.FindProperty("Bottom")!.Sentinel);
        }

        [ConditionalFact]
        public virtual void Can_set_sentinel_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().HaveSentinel(-1);
                    c.Properties<string>().HaveSentinel("100");
                });

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Equal(-1, complexType.FindProperty(Customer.IdProperty.Name)!.Sentinel);
            Assert.Equal(-1, complexType.FindProperty("Up")!.Sentinel);
            Assert.Equal("100", complexType.FindProperty("Down")!.Sentinel);
            Assert.Equal(-1, complexType.FindProperty("Charm")!.Sentinel);
            Assert.Equal("100", complexType.FindProperty("Strange")!.Sentinel);
            Assert.Equal(-1, complexType.FindProperty("Top")!.Sentinel);
            Assert.Equal("100", complexType.FindProperty("Bottom")!.Sentinel);
        }

        [ConditionalFact]
        public virtual void Can_set_unbounded_max_length_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().HaveMaxLength(0);
                    c.Properties<string>().HaveMaxLength(-1);
                });

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Equal(0, complexType.FindProperty(Customer.IdProperty.Name).GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Up").GetMaxLength());
            Assert.Equal(-1, complexType.FindProperty("Down").GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Charm").GetMaxLength());
            Assert.Equal(-1, complexType.FindProperty("Strange").GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Top").GetMaxLength());
            Assert.Equal(-1, complexType.FindProperty("Bottom").GetMaxLength());
        }

        [ConditionalFact]
        public virtual void Can_set_precision_and_scale_for_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).HasPrecision(1, 0);
                        b.Property(e => e.Down).HasPrecision(100, 10);
                        b.Property<int>("Charm").HasPrecision(1, 0);
                        b.Property<string>("Strange").HasPrecision(100, 10);
                        b.Property<int>("Top").HasPrecision(1, 0);
                        b.Property<string>("Bottom").HasPrecision(100, 10);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Null(complexType.FindProperty(Customer.IdProperty.Name).GetPrecision());
            Assert.Null(complexType.FindProperty(Customer.IdProperty.Name).GetScale());
            Assert.Equal(1, complexType.FindProperty("Up").GetPrecision());
            Assert.Equal(0, complexType.FindProperty("Up").GetScale());
            Assert.Equal(100, complexType.FindProperty("Down").GetPrecision());
            Assert.Equal(10, complexType.FindProperty("Down").GetScale());
            Assert.Equal(1, complexType.FindProperty("Charm").GetPrecision());
            Assert.Equal(0, complexType.FindProperty("Charm").GetScale());
            Assert.Equal(100, complexType.FindProperty("Strange").GetPrecision());
            Assert.Equal(10, complexType.FindProperty("Strange").GetScale());
            Assert.Equal(1, complexType.FindProperty("Top").GetPrecision());
            Assert.Equal(0, complexType.FindProperty("Top").GetScale());
            Assert.Equal(100, complexType.FindProperty("Bottom").GetPrecision());
            Assert.Equal(10, complexType.FindProperty("Bottom").GetScale());
        }

        [ConditionalFact]
        public virtual void Can_set_precision_and_scale_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().HavePrecision(1, 0);
                    c.Properties<string>().HavePrecision(100, 10);
                });

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Equal(1, complexType.FindProperty(Customer.IdProperty.Name).GetPrecision());
            Assert.Equal(0, complexType.FindProperty(Customer.IdProperty.Name).GetScale());
            Assert.Equal(1, complexType.FindProperty("Up").GetPrecision());
            Assert.Equal(0, complexType.FindProperty("Up").GetScale());
            Assert.Equal(100, complexType.FindProperty("Down").GetPrecision());
            Assert.Equal(10, complexType.FindProperty("Down").GetScale());
            Assert.Equal(1, complexType.FindProperty("Charm").GetPrecision());
            Assert.Equal(0, complexType.FindProperty("Charm").GetScale());
            Assert.Equal(100, complexType.FindProperty("Strange").GetPrecision());
            Assert.Equal(10, complexType.FindProperty("Strange").GetScale());
            Assert.Equal(1, complexType.FindProperty("Top").GetPrecision());
            Assert.Equal(0, complexType.FindProperty("Top").GetScale());
            Assert.Equal(100, complexType.FindProperty("Bottom").GetPrecision());
            Assert.Equal(10, complexType.FindProperty("Bottom").GetScale());
        }

        [ConditionalFact]
        public virtual void Can_set_custom_value_generator_for_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).HasValueGenerator<CustomValueGenerator>();
                        b.Property(e => e.Down).HasValueGenerator(typeof(CustomValueGenerator));
                        b.Property<string>("Strange").HasValueGenerator<CustomValueGenerator>();
                        b.Property<int>("Top").HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory));
                        b.Property<string>("Bottom").HasValueGeneratorFactory<CustomValueGeneratorFactory>();
                    });

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Null(complexType.FindProperty(Customer.IdProperty.Name).GetValueGeneratorFactory());
            Assert.IsType<CustomValueGenerator>(complexType.FindProperty("Up").GetValueGeneratorFactory()(null, null));
            Assert.IsType<CustomValueGenerator>(complexType.FindProperty("Down").GetValueGeneratorFactory()(null, null));
            Assert.IsType<CustomValueGenerator>(complexType.FindProperty("Strange").GetValueGeneratorFactory()(null, null));
            Assert.IsType<CustomValueGenerator>(complexType.FindProperty("Top").GetValueGeneratorFactory()(null, null));
            Assert.IsType<CustomValueGenerator>(complexType.FindProperty("Bottom").GetValueGeneratorFactory()(null, null));
        }

        private class CustomValueGenerator : ValueGenerator<int>
        {
            public override int Next(EntityEntry entry)
                => throw new NotImplementedException();

            public override bool GeneratesTemporaryValues
                => false;
        }

        private class CustomValueGeneratorFactory : ValueGeneratorFactory
        {
            public override ValueGenerator Create(IProperty property, ITypeBase entityType)
                => new CustomValueGenerator();
        }

        [ConditionalFact]
        public virtual void Throws_for_bad_value_generator_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.BadValueGeneratorType(nameof(Random), nameof(ValueGenerator)),
                            Assert.Throws<ArgumentException>(() => b.Property(e => e.Down).HasValueGenerator(typeof(Random))).Message);
                    });
        }

        [ConditionalFact]
        public virtual void Throws_for_value_generator_that_cannot_be_constructed()
        {
            var modelBuilder = CreateModelBuilder();
            var model = modelBuilder.Model;

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).HasValueGenerator<BadCustomValueGenerator1>();
                        b.Property(e => e.Down).HasValueGenerator<BadCustomValueGenerator2>();
                    });

            var complexType = model.FindEntityType(typeof(ComplexProperties))
                .FindComplexProperty(nameof(ComplexProperties.Quarks))!.ComplexType;

            Assert.Equal(
                CoreStrings.CannotCreateValueGenerator(nameof(BadCustomValueGenerator1), "HasValueGenerator"),
                Assert.Throws<InvalidOperationException>(
                    () => complexType.FindProperty("Up").GetValueGeneratorFactory()(null, null)).Message);

            Assert.Equal(
                CoreStrings.CannotCreateValueGenerator(nameof(BadCustomValueGenerator2), "HasValueGenerator"),
                Assert.Throws<InvalidOperationException>(
                    () => complexType.FindProperty("Down").GetValueGeneratorFactory()(null, null)).Message);
        }

#pragma warning disable CS9113 // Parameter 'foo' is unread
        private class BadCustomValueGenerator1(string foo) : CustomValueGenerator
#pragma warning restore CS9113
        {
        }

        private abstract class BadCustomValueGenerator2 : CustomValueGenerator;

        protected class StringCollectionEntity
        {
            public ICollection<string> Property { get; set; }
        }

        [ConditionalFact]
        public virtual void Can_set_unicode_for_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property(e => e.Up).IsUnicode();
                        b.Property(e => e.Down).IsUnicode(false);
                        b.Property<int>("Charm").IsUnicode();
                        b.Property<string>("Strange").IsUnicode(false);
                        b.Property<int>("Top").IsUnicode();
                        b.Property<string>("Bottom").IsUnicode(false);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.Null(complexType.FindProperty(Customer.IdProperty.Name).IsUnicode());
            Assert.True(complexType.FindProperty("Up").IsUnicode());
            Assert.False(complexType.FindProperty("Down").IsUnicode());
            Assert.True(complexType.FindProperty("Charm").IsUnicode());
            Assert.False(complexType.FindProperty("Strange").IsUnicode());
            Assert.True(complexType.FindProperty("Top").IsUnicode());
            Assert.False(complexType.FindProperty("Bottom").IsUnicode());
        }

        [ConditionalFact]
        public virtual void Can_set_unicode_for_property_type()
        {
            var modelBuilder = CreateModelBuilder(
                c =>
                {
                    c.Properties<int>().AreUnicode();
                    c.Properties<string>().AreUnicode(false);
                });

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Quarks,
                    b =>
                    {
                        b.Property<int>("Charm");
                        b.Property<string>("Strange");
                        b.Property<int>("Top");
                        b.Property<string>("Bottom");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;

            Assert.True(complexType.FindProperty(Customer.IdProperty.Name).IsUnicode());
            Assert.True(complexType.FindProperty("Up").IsUnicode());
            Assert.False(complexType.FindProperty("Down").IsUnicode());
            Assert.True(complexType.FindProperty("Charm").IsUnicode());
            Assert.False(complexType.FindProperty("Strange").IsUnicode());
            Assert.True(complexType.FindProperty("Top").IsUnicode());
            Assert.False(complexType.FindProperty("Bottom").IsUnicode());
        }

        [ConditionalFact]
        public virtual void PropertyBuilder_methods_can_be_chained()
            => CreateModelBuilder()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Quarks)
                .Property(e => e.Up)
                .IsRequired()
                .HasAnnotation("A", "V")
                .IsConcurrencyToken()
                .ValueGeneratedNever()
                .ValueGeneratedOnAdd()
                .ValueGeneratedOnAddOrUpdate()
                .ValueGeneratedOnUpdate()
                .IsUnicode()
                .HasMaxLength(100)
                .HasSentinel(1)
                .HasPrecision(10, 1)
                .HasValueGenerator<CustomValueGenerator>()
                .HasValueGenerator(typeof(CustomValueGenerator))
                .HasValueGeneratorFactory<CustomValueGeneratorFactory>()
                .HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory))
                .IsRequired();

        [ConditionalFact]
        public virtual void Can_call_Property_on_a_field()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.EntityWithFields).Property(e => e.Id);

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;
            Assert.Equal(6, complexType.GetProperties().Count());
            var property = complexType.FindProperty(nameof(EntityWithFields.Id));
            Assert.Null(property.PropertyInfo);
            Assert.NotNull(property.FieldInfo);
        }

        [ConditionalFact]
        public virtual void Can_ignore_a_field()
        {
            var modelBuilder = CreateModelBuilder(c => c.ComplexProperties<KeylessEntityWithFields>());

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.EntityWithFields,
                    b => b.Ignore(e => e.CompanyId));

            var model = modelBuilder.FinalizeModel();
            var complexProperty = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single();
            Assert.Equal(5, complexProperty.ComplexType.GetProperties().Count());
            Assert.Single(complexProperty.ComplexType.GetComplexProperties());
        }

        [ConditionalFact]
        public virtual void Complex_properties_not_discovered_by_convention()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Product>()
                .Ignore<CustomerDetails>()
                .Ignore<Order>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer);

            modelBuilder
                .Entity<ValueComplexProperties>(
                    b =>
                    {
                        b.Ignore(e => e.Tuple);
                        b.ComplexProperty(e => e.Label, b => b.ComplexProperty(e => e.Customer));
                        b.ComplexProperty(e => e.OldLabel, b => b.ComplexProperty(e => e.Customer));
                    });

            var model = modelBuilder.FinalizeModel();
            AssertEqual(modelBuilder.Model, model);

            var customerType = model.FindEntityType(typeof(ComplexProperties))!
                .FindComplexProperty(nameof(ComplexProperties.Customer))!.ComplexType;
            var indexedType = model.FindEntityType(typeof(ComplexProperties))!
                .FindComplexProperty(nameof(ComplexProperties.IndexedClass))!.ComplexType;

            var nestedProperty = indexedType.FindComplexProperty(nameof(IndexedClass.Nested))!;
            Assert.False(nestedProperty.IsNullable);
            Assert.Equal(typeof(NestedComplexType), nestedProperty.ClrType);
            var nestedType = nestedProperty.ComplexType;
            Assert.Equal(typeof(NestedComplexType), nestedType.ClrType);

            var doubleNestedProperty = nestedType.FindComplexProperty(nameof(NestedComplexType.DoubleNested))!;
            Assert.False(doubleNestedProperty.IsNullable);
            Assert.Equal(typeof(DoubleNestedComplexType), doubleNestedProperty.ClrType);
            var doubleNestedType = doubleNestedProperty.ComplexType;
            Assert.Equal(typeof(DoubleNestedComplexType), doubleNestedType.ClrType);

            var valueType = model.FindEntityType(typeof(ValueComplexProperties))!;
            var labelProperty = valueType.FindComplexProperty(nameof(ValueComplexProperties.Label))!;
            Assert.False(labelProperty.IsNullable);
            Assert.Equal(typeof(ProductLabel), labelProperty.ClrType);
            var labelType = labelProperty.ComplexType;
            Assert.Equal(typeof(ProductLabel), labelType.ClrType);

            var labelCustomerProperty = labelType.FindComplexProperty(nameof(ProductLabel.Customer))!;
            Assert.False(labelCustomerProperty.IsNullable);
            Assert.Equal(typeof(Customer), labelCustomerProperty.ClrType);

            var oldLabelProperty = valueType.FindComplexProperty(nameof(ValueComplexProperties.OldLabel))!;
            Assert.False(oldLabelProperty.IsNullable);
            Assert.Equal(typeof(ProductLabel), oldLabelProperty.ClrType);
            var oldLabelType = oldLabelProperty.ComplexType;
            Assert.Equal(typeof(ProductLabel), oldLabelType.ClrType);

            var oldLabelCustomerProperty = labelType.FindComplexProperty(nameof(ProductLabel.Customer))!;
            Assert.False(oldLabelCustomerProperty.IsNullable);
            Assert.Equal(typeof(Customer), oldLabelCustomerProperty.ClrType);
        }

        [ConditionalFact]
        public virtual void Complex_properties_can_be_configured_by_type()
        {
            var modelBuilder = CreateModelBuilder(m => m.ComplexProperties<Customer>());

            modelBuilder
                .Ignore<Product>()
                .Ignore<CustomerDetails>()
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>();

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties)).GetComplexProperties().Single().ComplexType;
            Assert.Equal(typeof(Customer), complexType.ClrType);
        }

        [ConditionalFact]
        public virtual void Throws_for_optional_complex_property()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer).IsRequired(false);

            Assert.Equal(
                CoreStrings.ComplexPropertyOptional(
                    nameof(ComplexProperties), nameof(ComplexProperties.Customer)),
                Assert.Throws<InvalidOperationException>(modelBuilder.FinalizeModel).Message);
        }

        [ConditionalFact]
        public virtual void Throws_for_tuple()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<ValueComplexProperties>()
                .Ignore(e => e.Label)
                .Ignore(e => e.OldLabel)
                .ComplexProperty(e => e.Tuple);

            var model = modelBuilder.FinalizeModel();

            var valueType = model.FindEntityType(typeof(ValueComplexProperties))!;
            var tupleProperty = valueType.FindComplexProperty(nameof(ValueComplexProperties.Tuple))!;
            Assert.False(tupleProperty.IsNullable);
            Assert.Equal(typeof((string, int)), tupleProperty.ClrType);
            var tupleType = tupleProperty.ComplexType;
            Assert.Equal(typeof((string, int)), tupleType.ClrType);
            Assert.Equal("ValueComplexProperties.Tuple#ValueTuple<string, int>", tupleType.DisplayName());

            Assert.Equal(2, tupleType.GetProperties().Count());
        }

        [ConditionalFact]
        protected virtual void Mapping_throws_for_non_ignored_navigations_on_complex_types()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer);

            Assert.Equal(
                CoreStrings.NavigationNotAddedComplexType(
                    "ComplexProperties.Customer#Customer", nameof(Customer.Details), typeof(CustomerDetails).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(modelBuilder.FinalizeModel).Message);
        }

        [ConditionalFact]
        protected virtual void Mapping_throws_for_empty_complex_types()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer)
                .Ignore(c => c.Notes)
                .Ignore(c => c.Name)
                .Ignore(c => c.Id)
                .Ignore(c => c.AlternateKey);

            Assert.Equal(
                CoreStrings.EmptyComplexType(
                    "ComplexProperties.Customer#Customer"),
                Assert.Throws<InvalidOperationException>(modelBuilder.FinalizeModel).Message);
        }

        [ConditionalFact]
        public virtual void Can_set_primitive_collection_annotation_when_no_clr_property()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Product>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer)
                .Ignore(c => c.Details)
                .Ignore(c => c.Orders)
                .PrimitiveCollection<int[]>("Ints").HasAnnotation("foo", "bar");

            var model = modelBuilder.FinalizeModel();
            var complexProperty = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single();
            var property = complexProperty.ComplexType.FindProperty("Ints")!;

            Assert.Equal("bar", property["foo"]);
        }

        [ConditionalFact]
        public virtual void Primitive_collections_are_required_by_default_only_if_CLR_type_is_nullable()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection(e => e.Up);
                        b.PrimitiveCollection(e => e.Down);
                        b.PrimitiveCollection<List<int>>("Charm");
                        b.PrimitiveCollection<List<string>>("Strange");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;

            Assert.False(complexType.FindProperty("Up")!.IsNullable);
            Assert.True(complexType.FindProperty("Down")!.IsNullable);
            Assert.True(complexType.FindProperty("Charm")!.IsNullable); // Because we can't detect the non-nullable reference type
            Assert.True(complexType.FindProperty("Strange")!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Can_ignore_shadow_primitive_collections_when_they_have_been_added_explicitly()
        {
            var modelBuilder = CreateModelBuilder();

            var complexPropertyBuilder = modelBuilder
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.Customer, b => b.Ignore(c => c.Details).Ignore(c => c.Orders));
            complexPropertyBuilder.PrimitiveCollection<string[]>("Shadow");
            complexPropertyBuilder.Ignore("Shadow");

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;
            Assert.Null(complexType.FindProperty("Shadow"));
        }

        [ConditionalFact]
        public virtual void Can_add_shadow_primitive_collections_when_they_have_been_ignored()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Product>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.Customer,
                    b =>
                    {
                        b.Ignore(c => c.Details);
                        b.Ignore(c => c.Orders);
                        b.Ignore("Shadow");
                        b.PrimitiveCollection<string[]>("Shadow");
                    });

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;
            Assert.NotNull(complexType.FindProperty("Shadow"));
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_be_made_required()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection(e => e.Up).IsRequired();
                        b.PrimitiveCollection(e => e.Down).IsRequired();
                        b.PrimitiveCollection<List<int>>("Charm").IsRequired();
                        b.PrimitiveCollection<List<string>>("Strange").IsRequired();
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;

            Assert.False(complexType.FindProperty("Up")!.IsNullable);
            Assert.False(complexType.FindProperty("Down")!.IsNullable);
            Assert.False(complexType.FindProperty("Charm")!.IsNullable);
            Assert.False(complexType.FindProperty("Strange")!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_be_made_optional()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection(e => e.Up).IsRequired(false);
                        b.PrimitiveCollection(e => e.Down).IsRequired(false);
                        b.PrimitiveCollection<List<int>>("Charm").IsRequired(false);
                        b.PrimitiveCollection<List<string>>("Strange").IsRequired(false);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;

            Assert.True(complexType.FindProperty("Up")!.IsNullable);
            Assert.True(complexType.FindProperty("Down")!.IsNullable);
            Assert.True(complexType.FindProperty("Charm")!.IsNullable);
            Assert.True(complexType.FindProperty("Strange")!.IsNullable);
        }

        [ConditionalFact]
        public virtual void Primitive_collections_specified_by_string_are_shadow_properties_unless_already_known_to_be_CLR_properties()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection<ObservableCollection<int>>("Up");
                        b.PrimitiveCollection<ObservableCollection<string>>("Down");
                        b.PrimitiveCollection<ObservableCollection<int>>("Charm");
                        b.PrimitiveCollection<ObservableCollection<string>>("Strange");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;

            Assert.False(complexType.FindProperty("Up")!.IsShadowProperty());
            Assert.False(complexType.FindProperty("Down")!.IsShadowProperty());
            Assert.True(complexType.FindProperty("Charm")!.IsShadowProperty());
            Assert.True(complexType.FindProperty("Strange")!.IsShadowProperty());

            Assert.Equal(-1, complexType.FindProperty("Up")!.GetShadowIndex());
            Assert.Equal(-1, complexType.FindProperty("Down")!.GetShadowIndex());
            Assert.NotEqual(-1, complexType.FindProperty("Charm")!.GetShadowIndex());
            Assert.NotEqual(-1, complexType.FindProperty("Strange")!.GetShadowIndex());
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_be_made_concurrency_tokens()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection(e => e.Up).IsConcurrencyToken();
                        b.PrimitiveCollection(e => e.Down).IsConcurrencyToken(false);
                        b.PrimitiveCollection<List<int>>("Charm").IsConcurrencyToken();
                        b.PrimitiveCollection<List<string>>("Strange").IsConcurrencyToken(false);
                        b.HasChangeTrackingStrategy(ChangeTrackingStrategy.ChangingAndChangedNotifications);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties()!.Single().ComplexType;

            Assert.True(complexType.FindProperty("Up")!.IsConcurrencyToken);
            Assert.False(complexType.FindProperty("Down")!.IsConcurrencyToken);
            Assert.True(complexType.FindProperty("Charm")!.IsConcurrencyToken);
            Assert.False(complexType.FindProperty("Strange")!.IsConcurrencyToken);

            Assert.Equal(ChangeTrackingStrategy.ChangingAndChangedNotifications, complexType.GetChangeTrackingStrategy());
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_have_field_set()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection<ObservableCollection<int>>("Up").HasField("_forUp");
                        b.PrimitiveCollection(e => e.Down).HasField("_forDown");
                        b.PrimitiveCollection<ObservableCollection<string>>("_forWierd").HasField("_forWierd");
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;

            Assert.Equal("_forUp", complexType.FindProperty("Up")!.GetFieldName());
            Assert.Equal("_forDown", complexType.FindProperty("Down")!.GetFieldName());
            Assert.Equal("_forWierd", complexType.FindProperty("_forWierd")!.GetFieldName());
        }

        [ConditionalFact]
        public virtual void HasField_for_primitive_collection_throws_if_field_is_not_found()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.MissingBackingField(
                                "_notFound", nameof(CollectionQuarks.Down), "ComplexProperties.CollectionQuarks#CollectionQuarks"),
                            Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Down).HasField("_notFound")).Message);
                    });
        }

        [ConditionalFact]
        public virtual void HasField_for_primitive_collection_throws_if_field_is_wrong_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.BadBackingFieldType(
                                "_forUp", "ObservableCollection<int>", nameof(CollectionQuarks), nameof(CollectionQuarks.Down),
                                "ObservableCollection<string>"),
                            Assert.Throws<InvalidOperationException>(() => b.Property(e => e.Down).HasField("_forUp")).Message);
                    });
        }

        [ConditionalFact]
        public virtual void Primitive_collections_can_be_set_to_generate_values_on_Add()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection(e => e.Up).ValueGeneratedOnAddOrUpdate();
                        b.PrimitiveCollection(e => e.Down).ValueGeneratedNever();
                        b.PrimitiveCollection<List<int>>("Charm").Metadata.ValueGenerated = ValueGenerated.OnUpdateSometimes;
                        b.PrimitiveCollection<List<string>>("Strange").ValueGeneratedNever();
                        b.PrimitiveCollection<List<int>>("Top").ValueGeneratedOnAddOrUpdate();
                        b.PrimitiveCollection<List<string>>("Bottom").ValueGeneratedOnUpdate();
                    });

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;
            Assert.Equal(ValueGenerated.OnAddOrUpdate, complexType.FindProperty("Up")!.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, complexType.FindProperty("Down")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdateSometimes, complexType.FindProperty("Charm")!.ValueGenerated);
            Assert.Equal(ValueGenerated.Never, complexType.FindProperty("Strange")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, complexType.FindProperty("Top")!.ValueGenerated);
            Assert.Equal(ValueGenerated.OnUpdate, complexType.FindProperty("Bottom")!.ValueGenerated);
        }

        [ConditionalFact]
        public virtual void Can_set_max_length_for_primitive_collections()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection(e => e.Up).HasMaxLength(0);
                        b.PrimitiveCollection(e => e.Down).HasMaxLength(100);
                        b.PrimitiveCollection<List<int>>("Charm").HasMaxLength(0);
                        b.PrimitiveCollection<List<string>>("Strange").HasMaxLength(-1);
                        b.PrimitiveCollection<int[]>("Top").HasMaxLength(0);
                        b.PrimitiveCollection<string[]>("Bottom").HasMaxLength(100);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;

            Assert.Equal(0, complexType.FindProperty("Up")!.GetMaxLength());
            Assert.Equal(100, complexType.FindProperty("Down")!.GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Charm")!.GetMaxLength());
            Assert.Equal(-1, complexType.FindProperty("Strange")!.GetMaxLength());
            Assert.Equal(0, complexType.FindProperty("Top")!.GetMaxLength());
            Assert.Equal(100, complexType.FindProperty("Bottom")!.GetMaxLength());
        }

        [ConditionalFact]
        public virtual void Can_set_sentinel_for_primitive_collections()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection(e => e.Up).HasSentinel(null);
                        b.PrimitiveCollection(e => e.Down).HasSentinel([]);
                        b.PrimitiveCollection<int[]>("Charm").HasSentinel([]);
                        b.PrimitiveCollection<List<string>>("Strange").HasSentinel([]);
                        b.PrimitiveCollection<int[]>("Top").HasSentinel([77]);
                        b.PrimitiveCollection<List<string>>("Bottom").HasSentinel([""]);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;

            Assert.Equal(0, complexType.FindProperty(nameof(CollectionQuarks.Id))!.Sentinel);
            Assert.Null(complexType.FindProperty("Up")!.Sentinel);
            Assert.Equal(new ObservableCollection<string>(), complexType.FindProperty("Down")!.Sentinel);
            Assert.Equal(Array.Empty<int>(), complexType.FindProperty("Charm")!.Sentinel);
            Assert.Equal(new List<string> { }, complexType.FindProperty("Strange")!.Sentinel);
            Assert.Equal(new int[] { 77 }, complexType.FindProperty("Top")!.Sentinel);
            Assert.Equal(new List<string> { "" }, complexType.FindProperty("Bottom")!.Sentinel);
        }

        [ConditionalFact]
        public virtual void Can_set_custom_value_generator_for_primitive_collections()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection(e => e.Up).HasValueGenerator<CustomValueGenerator>();
                        b.PrimitiveCollection(e => e.Down).HasValueGenerator(typeof(CustomValueGenerator));
                        b.PrimitiveCollection<List<string>>("Strange").HasValueGenerator<CustomValueGenerator>();
                        b.PrimitiveCollection<int[]>("Top").HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory));
                        b.PrimitiveCollection<List<string>>("Bottom").HasValueGeneratorFactory<CustomValueGeneratorFactory>();
                    });

            var model = modelBuilder.FinalizeModel();

            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;

            Assert.IsType<CustomValueGenerator>(complexType.FindProperty("Up")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(complexType.FindProperty("Down")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(complexType.FindProperty("Strange")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(complexType.FindProperty("Top")!.GetValueGeneratorFactory()!(null!, null!));
            Assert.IsType<CustomValueGenerator>(complexType.FindProperty("Bottom")!.GetValueGeneratorFactory()!(null!, null!));
        }

        [ConditionalFact]
        public virtual void Throws_for_primitive_collection_with_bad_value_generator_type()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        Assert.Equal(
                            CoreStrings.BadValueGeneratorType(nameof(Random), nameof(ValueGenerator)),
                            Assert.Throws<ArgumentException>(() => b.Property(e => e.Down).HasValueGenerator(typeof(Random))).Message);
                    });
        }

        [ConditionalFact]
        public virtual void Can_set_unicode_for_primitive_collections()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(
                    e => e.CollectionQuarks,
                    b =>
                    {
                        b.PrimitiveCollection(e => e.Up).IsUnicode();
                        b.PrimitiveCollection(e => e.Down).IsUnicode(false);
                        b.PrimitiveCollection<int[]>("Charm").IsUnicode();
                        b.PrimitiveCollection<List<string>>("Strange").IsUnicode(false);
                        b.PrimitiveCollection<int[]>("Top").IsUnicode();
                        b.PrimitiveCollection<List<string>>("Bottom").IsUnicode(false);
                    });

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;

            Assert.True(complexType.FindProperty("Up")!.IsUnicode());
            Assert.False(complexType.FindProperty("Down")!.IsUnicode());
            Assert.True(complexType.FindProperty("Charm")!.IsUnicode());
            Assert.False(complexType.FindProperty("Strange")!.IsUnicode());
            Assert.True(complexType.FindProperty("Top")!.IsUnicode());
            Assert.False(complexType.FindProperty("Bottom")!.IsUnicode());
        }

        [ConditionalFact]
        public virtual void PrimitiveCollectionBuilder_methods_can_be_chained()
            => CreateModelBuilder()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.CollectionQuarks)
                .PrimitiveCollection(e => e.Up)
                .ElementType(t => t
                    .HasAnnotation("B", "C")
                    .HasConversion(typeof(long))
                    .HasConversion(new CastingConverter<int, long>())
                    .HasConversion(typeof(long), typeof(CustomValueComparer<int>))
                    .HasConversion(typeof(long), new CustomValueComparer<int>())
                    .HasConversion(new CastingConverter<int, long>())
                    .HasConversion(new CastingConverter<int, long>(), new CustomValueComparer<int>())
                    .HasConversion<long>()
                    .HasConversion<long>(new CustomValueComparer<int>())
                    .HasConversion<long, CustomValueComparer<int>>()
                    .HasMaxLength(2)
                    .HasPrecision(1)
                    .HasPrecision(1, 2)
                    .IsRequired()
                    .IsUnicode())
                .IsRequired()
                .HasAnnotation("A", "V")
                .IsConcurrencyToken()
                .ValueGeneratedNever()
                .ValueGeneratedOnAdd()
                .ValueGeneratedOnAddOrUpdate()
                .ValueGeneratedOnUpdate()
                .IsUnicode()
                .HasMaxLength(100)
                .HasSentinel(null)
                .HasValueGenerator<CustomValueGenerator>()
                .HasValueGenerator(typeof(CustomValueGenerator))
                .HasValueGeneratorFactory<CustomValueGeneratorFactory>()
                .HasValueGeneratorFactory(typeof(CustomValueGeneratorFactory))
                .IsRequired();

        [ConditionalFact]
        public virtual void Can_call_PrimitiveCollection_on_a_field()
        {
            var modelBuilder = CreateModelBuilder();

            modelBuilder
                .Ignore<Order>()
                .Ignore<IndexedClass>()
                .Entity<ComplexProperties>()
                .ComplexProperty(e => e.EntityWithFields).PrimitiveCollection(e => e.CollectionId);

            var model = modelBuilder.FinalizeModel();
            var complexType = model.FindEntityType(typeof(ComplexProperties))!.GetComplexProperties().Single().ComplexType;
            Assert.Equal(6, complexType.GetProperties().Count());
            var property = complexType.FindProperty(nameof(EntityWithFields.CollectionId))!;
            Assert.Null(property.PropertyInfo);
            Assert.NotNull(property.FieldInfo);
        }
    }
}

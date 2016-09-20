// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Internal
{
    public class ComplexTypeDefinitionTest
    {
        [Fact]
        public void Can_add_properties_to_complex_type()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));

            var prop1 = complex1.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"));
            var prop2 = complex1.AddPropertyDefinition("Prop2");
            var prop3 = complex1.AddPropertyDefinition("Prop3", typeof(byte));
            var prop4 = complex1.AddPropertyDefinition("_field1");

            AssertProperties(complex1, prop1, prop2, prop3, prop4, ConfigurationSource.Explicit);
            AssertProperties(complex1, prop1, prop2, prop3, prop4, false);
            AssertProperties((IComplexTypeDefinition)complex1, prop1, prop2, prop3, prop4, false);
            AssertProperties((ITypeBase)complex1, prop1, prop2, prop3, prop4, false);
        }

        [Fact]
        public void Can_add_properties_with_config_source_to_complex_type()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));

            var prop1 = complex1.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"), ConfigurationSource.Convention);
            var prop2 = complex1.AddPropertyDefinition("Prop2", configurationSource: ConfigurationSource.Convention);
            var prop3 = complex1.AddPropertyDefinition("Prop3", typeof(byte), configurationSource: ConfigurationSource.Convention);
            var prop4 = complex1.AddPropertyDefinition("_field1", configurationSource: ConfigurationSource.Convention);

            AssertProperties(complex1, prop1, prop2, prop3, prop4, ConfigurationSource.Convention);
            AssertProperties(complex1, prop1, prop2, prop3, prop4, false);
            AssertProperties((IComplexTypeDefinition)complex1, prop1, prop2, prop3, prop4, false);
            AssertProperties((ITypeBase)complex1, prop1, prop2, prop3, prop4, false);
        }

        private static void AssertProperties(
            ComplexTypeDefinition complex1,
            ComplexPropertyDefinition prop1,
            ComplexPropertyDefinition prop2,
            ComplexPropertyDefinition prop3,
            ComplexPropertyDefinition prop4,
            ConfigurationSource configurationSource)
        {
            Assert.Equal(
                new[] { prop1, prop2, prop3, prop4 },
                complex1.GetPropertyDefinitions().ToArray());

            Assert.Same(prop1, complex1.FindPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1")));
            Assert.Same(prop1, complex1.FindPropertyDefinition("Prop1"));
            Assert.Same(prop2, complex1.FindPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop2")));
            Assert.Same(prop2, complex1.FindPropertyDefinition("Prop2"));
            Assert.Same(prop3, complex1.FindPropertyDefinition("Prop3"));
            Assert.Same(prop4, complex1.FindPropertyDefinition("_field1"));
            Assert.Null(complex1.FindPropertyDefinition("NotFound"));
            Assert.Null(complex1.FindPropertyDefinition(typeof(Complex1).GetAnyProperty("NotFound")));

            Assert.Equal("Prop1", prop1.Name);
            Assert.False(prop1.IsShadowProperty);
            Assert.Same(typeof(int), prop1.ClrType);
            Assert.Same(complex1, prop1.DeclaringType);
            Assert.Null(prop1.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Prop1"), prop1.PropertyInfo);
            Assert.Equal(configurationSource, prop1.GetConfigurationSource());

            Assert.Equal("Prop2", prop2.Name);
            Assert.False(prop2.IsShadowProperty);
            Assert.Same(typeof(string), prop2.ClrType);
            Assert.Same(complex1, prop2.DeclaringType);
            Assert.Null(prop2.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Prop2"), prop2.PropertyInfo);
            Assert.Equal(configurationSource, prop2.GetConfigurationSource());

            Assert.Equal("Prop3", prop3.Name);
            Assert.True(prop3.IsShadowProperty);
            Assert.Same(typeof(byte), prop3.ClrType);
            Assert.Same(complex1, prop3.DeclaringType);
            Assert.Null(prop3.FieldInfo);
            Assert.Null(prop3.PropertyInfo);
            Assert.Equal(configurationSource, prop3.GetConfigurationSource());

            Assert.Equal("_field1", prop4.Name);
            Assert.False(prop4.IsShadowProperty);
            Assert.Same(typeof(DateTime), prop4.ClrType);
            Assert.Same(complex1, prop4.DeclaringType);
            Assert.Equal("_field1", prop4.FieldInfo?.Name);
            Assert.Null(prop4.PropertyInfo);
            Assert.Equal(configurationSource, prop4.GetConfigurationSource());
        }

        private static void AssertProperties(
            IMutableComplexTypeDefinition complex1,
            IMutableComplexPropertyDefinition prop1,
            IMutableComplexPropertyDefinition prop2,
            IMutableComplexPropertyDefinition prop3,
            IMutableComplexPropertyDefinition prop4,
            bool forceShadow)
        {
            Assert.Equal(
                new[] { prop1, prop2, prop3, prop4 },
                complex1.GetPropertyDefinitions().ToArray());

            Assert.Same(prop1, complex1.FindPropertyDefinition("Prop1"));
            Assert.Same(prop2, complex1.FindPropertyDefinition("Prop2"));
            Assert.Same(prop3, complex1.FindPropertyDefinition("Prop3"));
            Assert.Same(prop4, complex1.FindPropertyDefinition("_field1"));
            Assert.Null(complex1.FindPropertyDefinition("NotFound"));

            Assert.Equal("Prop1", prop1.Name);
            Assert.Equal(forceShadow, prop1.IsShadowProperty);
            Assert.Same(typeof(int), prop1.ClrType);
            Assert.Same(complex1, prop1.DeclaringType);
            Assert.Null(prop1.FieldInfo);
            Assert.Same(forceShadow ? null : typeof(Complex1).GetAnyProperty("Prop1"), prop1.PropertyInfo);

            Assert.Equal("Prop2", prop2.Name);
            Assert.Equal(forceShadow, prop2.IsShadowProperty);
            Assert.Same(typeof(string), prop2.ClrType);
            Assert.Same(complex1, prop2.DeclaringType);
            Assert.Null(prop2.FieldInfo);
            Assert.Same(forceShadow ? null : typeof(Complex1).GetAnyProperty("Prop2"), prop2.PropertyInfo);

            Assert.Equal("Prop3", prop3.Name);
            Assert.True(prop3.IsShadowProperty);
            Assert.Same(typeof(byte), prop3.ClrType);
            Assert.Same(complex1, prop3.DeclaringType);
            Assert.Null(prop3.FieldInfo);
            Assert.Null(prop3.PropertyInfo);

            Assert.Equal("_field1", prop4.Name);
            Assert.Equal(forceShadow, prop4.IsShadowProperty);
            Assert.Same(typeof(DateTime), prop4.ClrType);
            Assert.Same(complex1, prop4.DeclaringType);
            Assert.Equal(forceShadow ? null : "_field1", prop4.FieldInfo?.Name);
            Assert.Null(prop4.PropertyInfo);
        }

        private static void AssertProperties(
            IComplexTypeDefinition complex1,
            IComplexPropertyDefinition prop1,
            IComplexPropertyDefinition prop2,
            IComplexPropertyDefinition prop3,
            IComplexPropertyDefinition prop4,
            bool forceShadow)
        {
            Assert.Equal(
                new[] { prop1, prop2, prop3, prop4 },
                complex1.GetPropertyDefinitions().ToArray());

            Assert.Same(prop1, complex1.FindPropertyDefinition("Prop1"));
            Assert.Same(prop2, complex1.FindPropertyDefinition("Prop2"));
            Assert.Same(prop3, complex1.FindPropertyDefinition("Prop3"));
            Assert.Same(prop4, complex1.FindPropertyDefinition("_field1"));
            Assert.Null(complex1.FindPropertyDefinition("NotFound"));

            Assert.Equal("Prop1", prop1.Name);
            Assert.Equal(forceShadow, prop1.IsShadowProperty);
            Assert.Same(typeof(int), prop1.ClrType);
            Assert.Same(complex1, prop1.DeclaringType);
            Assert.Null(prop1.FieldInfo);
            Assert.Same(forceShadow ? null : typeof(Complex1).GetAnyProperty("Prop1"), prop1.PropertyInfo);

            Assert.Equal("Prop2", prop2.Name);
            Assert.Equal(forceShadow, prop2.IsShadowProperty);
            Assert.Same(typeof(string), prop2.ClrType);
            Assert.Same(complex1, prop2.DeclaringType);
            Assert.Null(prop2.FieldInfo);
            Assert.Same(forceShadow ? null : typeof(Complex1).GetAnyProperty("Prop2"), prop2.PropertyInfo);

            Assert.Equal("Prop3", prop3.Name);
            Assert.True(prop3.IsShadowProperty);
            Assert.Same(typeof(byte), prop3.ClrType);
            Assert.Same(complex1, prop3.DeclaringType);
            Assert.Null(prop3.FieldInfo);
            Assert.Null(prop3.PropertyInfo);

            Assert.Equal("_field1", prop4.Name);
            Assert.Equal(forceShadow, prop4.IsShadowProperty);
            Assert.Same(typeof(DateTime), prop4.ClrType);
            Assert.Same(complex1, prop4.DeclaringType);
            Assert.Equal(forceShadow ? null : "_field1", prop4.FieldInfo?.Name);
            Assert.Null(prop4.PropertyInfo);
        }

        private static void AssertProperties(
            ITypeBase complex1,
            IPropertyBase prop1,
            IPropertyBase prop2,
            IPropertyBase prop3,
            IPropertyBase prop4,
            bool forceShadow)
        {
            Assert.Equal("Prop1", prop1.Name);
            Assert.Same(typeof(int), prop1.ClrType);
            Assert.Same(complex1, prop1.DeclaringType);
            Assert.Null(prop1.FieldInfo);
            Assert.Same(forceShadow ? null : typeof(Complex1).GetAnyProperty("Prop1"), prop1.PropertyInfo);

            Assert.Equal("Prop2", prop2.Name);
            Assert.Same(typeof(string), prop2.ClrType);
            Assert.Same(complex1, prop2.DeclaringType);
            Assert.Null(prop2.FieldInfo);
            Assert.Same(forceShadow ? null : typeof(Complex1).GetAnyProperty("Prop2"), prop2.PropertyInfo);

            Assert.Equal("Prop3", prop3.Name);
            Assert.Same(typeof(byte), prop3.ClrType);
            Assert.Same(complex1, prop3.DeclaringType);
            Assert.Null(prop3.FieldInfo);
            Assert.Null(prop3.PropertyInfo);

            Assert.Equal("_field1", prop4.Name);
            Assert.Same(typeof(DateTime), prop4.ClrType);
            Assert.Same(complex1, prop4.DeclaringType);
            Assert.Equal(forceShadow ? null : "_field1", prop4.FieldInfo?.Name);
            Assert.Null(prop4.PropertyInfo);
        }

        [Fact]
        public void Can_add_properties_to_shadow_complex_type()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition("ShadowComplex1");

            var prop1 = complex1.AddPropertyDefinition("Prop1", typeof(int));
            var prop2 = complex1.AddPropertyDefinition("Prop2", typeof(string), configurationSource: ConfigurationSource.DataAnnotation);
            var prop3 = complex1.AddPropertyDefinition("Prop3", typeof(byte));

            Assert.Equal(
                new[] { prop1, prop2, prop3 },
                complex1.GetPropertyDefinitions().ToArray());

            Assert.Equal("Prop1", prop1.Name);
            Assert.True(prop1.IsShadowProperty);
            Assert.Same(typeof(int), prop1.ClrType);
            Assert.Same(complex1, prop1.DeclaringType);
            Assert.Null(prop1.FieldInfo);
            Assert.Null(prop1.PropertyInfo);
            Assert.Equal(ConfigurationSource.Explicit, prop1.GetConfigurationSource());

            Assert.Equal("Prop2", prop2.Name);
            Assert.True(prop2.IsShadowProperty);
            Assert.Same(typeof(string), prop2.ClrType);
            Assert.Same(complex1, prop2.DeclaringType);
            Assert.Null(prop2.FieldInfo);
            Assert.Null(prop2.PropertyInfo);
            Assert.Equal(ConfigurationSource.DataAnnotation, prop2.GetConfigurationSource());

            Assert.Equal("Prop3", prop3.Name);
            Assert.True(prop3.IsShadowProperty);
            Assert.Same(typeof(byte), prop3.ClrType);
            Assert.Same(complex1, prop3.DeclaringType);
            Assert.Null(prop3.FieldInfo);
            Assert.Null(prop3.PropertyInfo);
            Assert.Equal(ConfigurationSource.Explicit, prop3.GetConfigurationSource());
        }

        [Fact]
        public void Can_remove_properties_of_complex_type()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));

            var prop1 = complex1.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"));
            var prop2 = complex1.AddPropertyDefinition("Prop2");
            var prop3 = complex1.AddPropertyDefinition("Prop3", typeof(byte));
            var prop4 = complex1.AddPropertyDefinition("_field1");

            Assert.Equal(
                new[] { prop1, prop2, prop3, prop4 },
                complex1.GetPropertyDefinitions().ToArray());

            complex1.RemovePropertyDefinition("Prop1");

            Assert.Equal(
                new[] { prop2, prop3, prop4 },
                complex1.GetPropertyDefinitions().ToArray());

            complex1.RemovePropertyDefinition("Prop2");

            Assert.Equal(
                new[] { prop3, prop4 },
                complex1.GetPropertyDefinitions().ToArray());

            ((IMutableComplexTypeDefinition)complex1).RemovePropertyDefinition("_field1");

            Assert.Equal(
                new[] { prop3 },
                complex1.GetPropertyDefinitions().ToArray());

            ((IMutableComplexTypeDefinition)complex1).RemovePropertyDefinition("Prop3");

            Assert.Empty(complex1.GetPropertyDefinitions().ToArray());
        }

        [Fact]
        public void Can_nest_complex_types()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complex2 = model.AddComplexTypeDefinition(typeof(Complex2));

            var reference1 = complex1.AddComplexTypeReferenceDefinition("Nested", complex2);
            var reference2 = complex1.AddComplexTypeReferenceDefinition("_fieldNested", complex2);
            var reference3 = complex1.AddComplexTypeReferenceDefinition("ShadowNested", complex2);

            AssertReference(complex1, complex2, reference1, reference2, reference3, ConfigurationSource.Explicit);
            AssertReference(complex1, complex2, reference1, reference2, reference3);
            AssertReference((IComplexTypeDefinition)complex1, complex2, reference1, reference2, reference3);
            AssertReference((ITypeBase)complex1, complex2, reference1, reference2, reference3);
        }

        [Fact]
        public void Can_nest_complex_types_with_config_source()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complex2 = model.AddComplexTypeDefinition(typeof(Complex2));

            var reference1 = complex1.AddComplexTypeReferenceDefinition("Nested", complex2, ConfigurationSource.DataAnnotation);
            var reference2 = complex1.AddComplexTypeReferenceDefinition("_fieldNested", complex2, ConfigurationSource.DataAnnotation);
            var reference3 = complex1.AddComplexTypeReferenceDefinition("ShadowNested", complex2, ConfigurationSource.DataAnnotation);

            AssertReference(complex1, complex2, reference1, reference2, reference3, ConfigurationSource.DataAnnotation);
            AssertReference(complex1, complex2, reference1, reference2, reference3);
            AssertReference((IComplexTypeDefinition)complex1, complex2, reference1, reference2, reference3);
            AssertReference((ITypeBase)complex1, complex2, reference1, reference2, reference3);
        }

        [Fact]
        public void Can_nest_complex_types_using_interface()
        {
            var model = new Model();

            IMutableComplexTypeDefinition complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            IMutableComplexTypeDefinition complex2 = model.AddComplexTypeDefinition(typeof(Complex2));

            var reference1 = (ComplexTypeReferenceDefinition)complex1.AddComplexTypeReferenceDefinition("Nested", complex2);
            var reference2 = (ComplexTypeReferenceDefinition)complex1.AddComplexTypeReferenceDefinition("_fieldNested", complex2);
            var reference3 = (ComplexTypeReferenceDefinition)complex1.AddComplexTypeReferenceDefinition("ShadowNested", complex2);

            AssertReference((ComplexTypeDefinition)complex1, (ComplexTypeDefinition)complex2, reference1, reference2, reference3, ConfigurationSource.Explicit);
            AssertReference(complex1, complex2, reference1, reference2, reference3);
            AssertReference((IComplexTypeDefinition)complex1, complex2, reference1, reference2, reference3);
            AssertReference((ITypeBase)complex1, complex2, reference1, reference2, reference3);
        }

        private static void AssertReference(
            ComplexTypeDefinition complex1,
            ComplexTypeDefinition complex2,
            ComplexTypeReferenceDefinition reference1,
            ComplexTypeReferenceDefinition reference2,
            ComplexTypeReferenceDefinition reference3,
            ConfigurationSource configurationSource,
            bool shadowComplexType = false)
        {
            Assert.Equal(
                new[] { reference1, reference3, reference2 },
                complex1.GetComplexTypeReferenceDefinitions().ToArray());

            Assert.Same(reference1, complex1.FindComplexTypeReferenceDefinition("Nested"));
            Assert.Same(reference2, complex1.FindComplexTypeReferenceDefinition("_fieldNested"));
            Assert.Same(reference3, complex1.FindComplexTypeReferenceDefinition("ShadowNested"));
            Assert.Null(complex1.FindComplexTypeReferenceDefinition("NotFound"));

            Assert.Equal("Nested", reference1.Name);
            Assert.Equal(shadowComplexType, reference1.IsShadowProperty);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference1.ClrType);
            Assert.Same(complex1, reference1.DeclaringType);
            Assert.Same(complex2, reference1.ReferencedComplexTypeDefinition);
            Assert.True(reference1.IsRequired);
            Assert.Null(reference1.FieldInfo);
            Assert.Same(shadowComplexType ? null : typeof(Complex1).GetAnyProperty("Nested"), reference1.PropertyInfo);
            Assert.Equal(configurationSource, reference1.GetConfigurationSource());

            Assert.Equal("_fieldNested", reference2.Name);
            Assert.Equal(shadowComplexType, reference1.IsShadowProperty);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference2.ClrType);
            Assert.Same(complex1, reference2.DeclaringType);
            Assert.Same(complex2, reference2.ReferencedComplexTypeDefinition);
            Assert.True(reference2.IsRequired);
            Assert.Equal(shadowComplexType ? null : "_fieldNested", reference2.FieldInfo?.Name);
            Assert.Null(reference2.PropertyInfo);
            Assert.Equal(configurationSource, reference2.GetConfigurationSource());

            Assert.Equal("ShadowNested", reference3.Name);
            Assert.True(reference3.IsShadowProperty);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference3.ClrType);
            Assert.Same(complex1, reference3.DeclaringType);
            Assert.Same(complex2, reference3.ReferencedComplexTypeDefinition);
            Assert.True(reference3.IsRequired);
            Assert.Null(reference3.FieldInfo);
            Assert.Null(reference3.PropertyInfo);
            Assert.Equal(configurationSource, reference3.GetConfigurationSource());
        }

        private static void AssertReference(
            IMutableComplexTypeDefinition complex1,
            IMutableComplexTypeDefinition complex2,
            IMutableComplexTypeReferenceDefinition reference1,
            IMutableComplexTypeReferenceDefinition reference2,
            IMutableComplexTypeReferenceDefinition reference3,
            bool shadowComplexType = false)
        {
            Assert.Equal(
                new[] { reference1, reference3, reference2 },
                complex1.GetComplexTypeReferenceDefinitions().ToArray());

            Assert.Same(reference1, complex1.FindComplexTypeReferenceDefinition("Nested"));
            Assert.Same(reference2, complex1.FindComplexTypeReferenceDefinition("_fieldNested"));
            Assert.Same(reference3, complex1.FindComplexTypeReferenceDefinition("ShadowNested"));
            Assert.Null(complex1.FindComplexTypeReferenceDefinition("NotFound"));

            Assert.Equal("Nested", reference1.Name);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference1.ClrType);
            Assert.Same(complex1, reference1.DeclaringType);
            Assert.Same(complex2, reference1.ReferencedComplexTypeDefinition);
            Assert.True(reference1.IsRequired);
            Assert.Null(reference1.FieldInfo);
            Assert.Same(shadowComplexType ? null : typeof(Complex1).GetAnyProperty("Nested"), reference1.PropertyInfo);

            Assert.Equal("_fieldNested", reference2.Name);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference2.ClrType);
            Assert.Same(complex1, reference2.DeclaringType);
            Assert.Same(complex2, reference2.ReferencedComplexTypeDefinition);
            Assert.True(reference2.IsRequired);
            Assert.Equal(shadowComplexType ? null : "_fieldNested", reference2.FieldInfo?.Name);
            Assert.Null(reference2.PropertyInfo);

            Assert.Equal("ShadowNested", reference3.Name);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference3.ClrType);
            Assert.Same(complex1, reference3.DeclaringType);
            Assert.Same(complex2, reference3.ReferencedComplexTypeDefinition);
            Assert.True(reference3.IsRequired);
            Assert.Null(reference3.FieldInfo);
            Assert.Null(reference3.PropertyInfo);
        }

        private static void AssertReference(
            IComplexTypeDefinition complex1,
            IComplexTypeDefinition complex2,
            IComplexTypeReferenceDefinition reference1,
            IComplexTypeReferenceDefinition reference2,
            IComplexTypeReferenceDefinition reference3,
            bool shadowComplexType = false)
        {
            Assert.Equal(
                new[] { reference1, reference3, reference2 },
                complex1.GetComplexTypeReferenceDefinitions().ToArray());

            Assert.Same(reference1, complex1.FindComplexTypeReferenceDefinition("Nested"));
            Assert.Same(reference2, complex1.FindComplexTypeReferenceDefinition("_fieldNested"));
            Assert.Same(reference3, complex1.FindComplexTypeReferenceDefinition("ShadowNested"));
            Assert.Null(complex1.FindComplexTypeReferenceDefinition("NotFound"));

            Assert.Equal("Nested", reference1.Name);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference1.ClrType);
            Assert.Same(complex1, reference1.DeclaringType);
            Assert.Same(complex2, reference1.ReferencedComplexTypeDefinition);
            Assert.True(reference1.IsRequired);
            Assert.Null(reference1.FieldInfo);
            Assert.Same(shadowComplexType ? null : typeof(Complex1).GetAnyProperty("Nested"), reference1.PropertyInfo);

            Assert.Equal("_fieldNested", reference2.Name);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference2.ClrType);
            Assert.Same(complex1, reference2.DeclaringType);
            Assert.Same(complex2, reference2.ReferencedComplexTypeDefinition);
            Assert.True(reference2.IsRequired);
            Assert.Equal(shadowComplexType ? null : "_fieldNested", reference2.FieldInfo?.Name);
            Assert.Null(reference2.PropertyInfo);

            Assert.Equal("ShadowNested", reference3.Name);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference3.ClrType);
            Assert.Same(complex1, reference3.DeclaringType);
            Assert.Same(complex2, reference3.ReferencedComplexTypeDefinition);
            Assert.True(reference3.IsRequired);
            Assert.Null(reference3.FieldInfo);
            Assert.Null(reference3.PropertyInfo);
        }

        private static void AssertReference(
            ITypeBase complex1,
            ITypeBase complex2,
            IPropertyBase reference1,
            IPropertyBase reference2,
            IPropertyBase reference3,
            bool shadowComplexType = false)
        {
            Assert.Equal("Nested", reference1.Name);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference1.ClrType);
            Assert.Same(complex1, reference1.DeclaringType);
            Assert.Null(reference1.FieldInfo);
            Assert.Same(shadowComplexType ? null : typeof(Complex1).GetAnyProperty("Nested"), reference1.PropertyInfo);

            Assert.Equal("_fieldNested", reference2.Name);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference2.ClrType);
            Assert.Same(complex1, reference2.DeclaringType);
            Assert.Equal(shadowComplexType ? null : "_fieldNested", reference2.FieldInfo?.Name);
            Assert.Null(reference2.PropertyInfo);

            Assert.Equal("ShadowNested", reference3.Name);
            Assert.Same(shadowComplexType ? null : typeof(Complex2), reference3.ClrType);
            Assert.Same(complex1, reference3.DeclaringType);
            Assert.Null(reference3.FieldInfo);
            Assert.Null(reference3.PropertyInfo);
        }

        [Fact]
        public void Can_nest_complex_types_on_shadow_complex_type()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition("Complex1");
            var complex2 = model.AddComplexTypeDefinition("Complex2");

            var reference1 = complex1.AddComplexTypeReferenceDefinition("Nested", complex2);
            var reference2 = complex1.AddComplexTypeReferenceDefinition("_fieldNested", complex2);
            var reference3 = complex1.AddComplexTypeReferenceDefinition("ShadowNested", complex2);

            AssertReference(complex1, complex2, reference1, reference2, reference3, ConfigurationSource.Explicit, shadowComplexType: true);
            AssertReference(complex1, complex2, reference1, reference2, reference3, shadowComplexType: true);
            AssertReference((IComplexTypeDefinition)complex1, complex2, reference1, reference2, reference3, shadowComplexType: true);
            AssertReference((ITypeBase)complex1, complex2, reference1, reference2, reference3, shadowComplexType: true);
        }

        [Fact]
        public void Throws_adding_complex_type_reference_with_same_name_as_property()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complex2 = model.AddComplexTypeDefinition(typeof(Complex2));

            complex1.AddPropertyDefinition("Nested", typeof(Complex2));

            Assert.Equal(
                CoreStrings.ConflictingPropertyToReference("Nested", nameof(Complex1), nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => complex1.AddComplexTypeReferenceDefinition("Nested", complex2)).Message);
        }

        [Fact]
        public void Throws_adding_complex_type_reference_twice()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complex2 = model.AddComplexTypeDefinition(typeof(Complex2));

            complex1.AddComplexTypeReferenceDefinition("Nested", complex2);

            Assert.Equal(
                CoreStrings.DuplicateComplexReference("Nested", nameof(Complex1), nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => complex1.AddComplexTypeReferenceDefinition("Nested", complex2)).Message);
        }

        [Fact]
        public void Throws_adding_property_with_same_name_as_complex_reference()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complex2 = model.AddComplexTypeDefinition(typeof(Complex2));

            complex1.AddComplexTypeReferenceDefinition("Nested", complex2);

            Assert.Equal(
                CoreStrings.ConflictingComplexReference("Nested", nameof(Complex1), nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => complex1.AddPropertyDefinition("Nested", typeof(Complex2))).Message);
        }

        [Fact]
        public void Throws_adding_property_twice()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));

            complex1.AddPropertyDefinition("Nested", typeof(Complex2));

            Assert.Equal(
                CoreStrings.DuplicateProperty("Nested", nameof(Complex1), nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => complex1.AddPropertyDefinition("Nested", typeof(Complex2))).Message);
        }

        private class Complex1
        {
#pragma warning disable 169
            private DateTime _field1;
            private Complex2 _fieldNested;
#pragma warning restore 169

            public int Prop1 { get; set; }
            public string Prop2 { get; set; }
            public long NotFound { get; set; }

            public Complex2 Nested { get; set; }
        }

        private class Complex2
        {
            public int Prop1 { get; set; }
            public string Prop2 { get; set; }
        }
    }
}

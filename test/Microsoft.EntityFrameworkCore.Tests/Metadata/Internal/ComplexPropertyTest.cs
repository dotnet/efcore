// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Microsoft.EntityFrameworkCore.ValueGeneration;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Internal
{
    public class ComplexPropertyTest
    {
        [Fact]
        public void Complex_property_nullable_facet_makes_use_of_definition()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            AssertNullable(complexUsage, false, true, false, true);

            propDef11.IsNullableDefault = true;
            propDef12.IsNullableDefault = true;
            propDef21.IsNullableDefault = false;
            propDef22.IsNullableDefault = false;

            AssertNullable(complexUsage, false, true, false, false);

            complexUsage.FindProperty("StringProp").IsNullable = false;
            complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsNullable = true;

            AssertNullable(complexUsage, false, false, false, true);
        }

        [Fact]
        public void Cannot_set_non_nullable_CLR_property_to_nullable()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            Assert.Equal(
                CoreStrings.CannotBeNullable("IntProp", nameof(Complex1), typeof(int).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(
                    () => complexUsage.FindProperty("IntProp").IsNullable = true).Message);

            Assert.Equal(
                CoreStrings.CannotBeNullable("IntProp", nameof(Complex2), typeof(int).ShortDisplayName()),
                Assert.Throws<InvalidOperationException>(
                    () => complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsNullable = true).Message);

            AssertNullable(complexUsage, false, true, false, true);
        }

        [Fact]
        public void Cannot_set_key_property_to_nullable()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            propDef12.IsNullableDefault = false;

            var keyProperty = complexUsage.FindProperty("StringProp");
            complexUsage.DeclaringEntityType.SetPrimaryKey(keyProperty);

            Assert.Equal(
                CoreStrings.CannotBeNullablePK("StringProp", nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(
                    () => keyProperty.IsNullable = true).Message);

            AssertNullable(complexUsage, false, false, false, true);

            propDef12.IsNullableDefault = true;

            AssertNullable(complexUsage, false, false, false, true);
        }

        private static void AssertNullable(ComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsNullable);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsNullable);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsNullable);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsNullable);

            AssertNullable((IMutableComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertNullable(IMutableComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsNullable);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsNullable);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsNullable);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsNullable);

            AssertNullable((IComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertNullable(IComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsNullable);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsNullable);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsNullable);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsNullable);
        }

        [Fact]
        public void Complex_property_concurrency_facet_makes_use_of_definition()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            AssertConcurrency(complexUsage, false, false, false, false);

            propDef11.IsConcurrencyTokenDefault = true;
            propDef12.IsConcurrencyTokenDefault = false;
            propDef21.IsConcurrencyTokenDefault = true;
            propDef22.IsConcurrencyTokenDefault = false;

            AssertConcurrency(complexUsage, true, false, true, false);

            complexUsage.FindProperty(propDef11.Name).IsConcurrencyToken = false;
            complexUsage.FindComplexTypeUsage("Nested").FindProperty(propDef22.Name).IsConcurrencyToken = true;

            AssertConcurrency(complexUsage, false, false, true, true);
        }

        private static void AssertConcurrency(ComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsConcurrencyToken);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsConcurrencyToken);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsConcurrencyToken);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsConcurrencyToken);

            AssertConcurrency((IMutableComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertConcurrency(IMutableComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsConcurrencyToken);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsConcurrencyToken);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsConcurrencyToken);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsConcurrencyToken);

            AssertConcurrency((IComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertConcurrency(IComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsConcurrencyToken);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsConcurrencyToken);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsConcurrencyToken);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsConcurrencyToken);
        }

        [Fact]
        public void Complex_property_read_only_after_save_facet_makes_use_of_definition()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            AssertReadOnlyAfter(complexUsage, false, false, false, false);

            var keyProperty = complexUsage.FindProperty("StringProp");
            keyProperty.IsNullable = false;
            complexUsage.DeclaringEntityType.SetPrimaryKey(keyProperty);

            complexUsage.FindProperty("IntProp").ValueGenerated = ValueGenerated.OnAddOrUpdate;
            complexUsage.FindProperty("IntProp").IsStoreGeneratedAlways = true;
            complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").ValueGenerated = ValueGenerated.OnAddOrUpdate;

            AssertReadOnlyAfter(complexUsage, false, true, true, false);

            propDef11.IsReadOnlyAfterSaveDefault = true;
            propDef12.IsReadOnlyAfterSaveDefault = false;
            propDef21.IsReadOnlyAfterSaveDefault = false;
            propDef22.IsReadOnlyAfterSaveDefault = false;

            AssertReadOnlyAfter(complexUsage, true, true, true, false);

            complexUsage.FindProperty("IntProp").IsReadOnlyAfterSave = false;
            complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsReadOnlyAfterSave = true;

            AssertReadOnlyAfter(complexUsage, false, true, true, true);
        }

        private static void AssertReadOnlyAfter(ComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsReadOnlyAfterSave);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsReadOnlyAfterSave);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsReadOnlyAfterSave);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsReadOnlyAfterSave);

            AssertReadOnlyAfter((IMutableComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertReadOnlyAfter(IMutableComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsReadOnlyAfterSave);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsReadOnlyAfterSave);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsReadOnlyAfterSave);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsReadOnlyAfterSave);

            AssertReadOnlyAfter((IComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertReadOnlyAfter(IComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsReadOnlyAfterSave);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsReadOnlyAfterSave);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsReadOnlyAfterSave);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsReadOnlyAfterSave);
        }

        [Fact]
        public void Complex_property_read_only_before_save_facet_makes_use_of_definition()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            AssertReadOnlyBefore(complexUsage, false, false, false, false);

            var keyProperty = complexUsage.FindProperty("StringProp");
            keyProperty.IsNullable = false;
            complexUsage.DeclaringEntityType.SetPrimaryKey(keyProperty);

            complexUsage.FindProperty("IntProp").ValueGenerated = ValueGenerated.OnAddOrUpdate;
            complexUsage.FindProperty("IntProp").IsStoreGeneratedAlways = true;
            complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").ValueGenerated = ValueGenerated.OnAddOrUpdate;

            AssertReadOnlyBefore(complexUsage, false, false, true, false);

            propDef11.IsReadOnlyBeforeSaveDefault = true;
            propDef12.IsReadOnlyBeforeSaveDefault = false;
            propDef21.IsReadOnlyBeforeSaveDefault = false;
            propDef22.IsReadOnlyBeforeSaveDefault = false;

            AssertReadOnlyBefore(complexUsage, true, false, true, false);

            complexUsage.FindProperty("IntProp").IsReadOnlyBeforeSave = false;
            complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsReadOnlyBeforeSave = true;

            AssertReadOnlyBefore(complexUsage, false, false, true, true);
        }

        private static void AssertReadOnlyBefore(ComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsReadOnlyBeforeSave);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsReadOnlyBeforeSave);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsReadOnlyBeforeSave);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsReadOnlyBeforeSave);

            AssertReadOnlyBefore((IMutableComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertReadOnlyBefore(IMutableComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsReadOnlyBeforeSave);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsReadOnlyBeforeSave);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsReadOnlyBeforeSave);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsReadOnlyBeforeSave);

            AssertReadOnlyBefore((IComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertReadOnlyBefore(IComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsReadOnlyBeforeSave);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsReadOnlyBeforeSave);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsReadOnlyBeforeSave);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsReadOnlyBeforeSave);
        }

        [Fact]
        public void Complex_property_requires_value_generator_facet_makes_use_of_definition()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            AssertRequiresValueGenerator(complexUsage, false, false, false, false);

            var keyProperty1 = complexUsage.FindProperty("StringProp");
            keyProperty1.IsNullable = false;

            var keyProperty2 = complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp");
            keyProperty2.ValueGenerated = ValueGenerated.OnAdd;

            complexUsage.DeclaringEntityType.SetPrimaryKey(new[] { keyProperty1, keyProperty2 });

            AssertRequiresValueGenerator(complexUsage, false, false, true, false);

            propDef11.RequiresValueGeneratorDefault = true;
            propDef12.RequiresValueGeneratorDefault = false;
            propDef21.RequiresValueGeneratorDefault = false;
            propDef22.RequiresValueGeneratorDefault = true;

            AssertRequiresValueGenerator(complexUsage, true, false, true, true);

            complexUsage.FindProperty("IntProp").RequiresValueGenerator = false;
            keyProperty2.RequiresValueGenerator = false;

            AssertRequiresValueGenerator(complexUsage, false, false, false, true);
        }

        private static void AssertRequiresValueGenerator(ComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").RequiresValueGenerator);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").RequiresValueGenerator);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").RequiresValueGenerator);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").RequiresValueGenerator);

            AssertRequiresValueGenerator((IMutableComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertRequiresValueGenerator(IMutableComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").RequiresValueGenerator);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").RequiresValueGenerator);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").RequiresValueGenerator);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").RequiresValueGenerator);

            AssertRequiresValueGenerator((IComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertRequiresValueGenerator(IComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").RequiresValueGenerator);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").RequiresValueGenerator);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").RequiresValueGenerator);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").RequiresValueGenerator);
        }

        [Fact]
        public void Complex_property_store_generated_always_facet_makes_use_of_definition()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            AssertIsStoreGeneratedAlways(complexUsage, false, false, false, false);

            var conToken = complexUsage.FindProperty("StringProp");
            conToken.IsConcurrencyToken = true;
            conToken.ValueGenerated = ValueGenerated.OnAddOrUpdate;

            complexUsage.FindProperty("IntProp").ValueGenerated = ValueGenerated.OnAddOrUpdate;
            complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsConcurrencyToken = true;

            AssertIsStoreGeneratedAlways(complexUsage, false, true, false, false);

            propDef11.IsStoreGeneratedAlwaysDefault = true;
            propDef12.IsStoreGeneratedAlwaysDefault = false;
            propDef21.IsStoreGeneratedAlwaysDefault = false;
            propDef22.IsStoreGeneratedAlwaysDefault = true;

            AssertIsStoreGeneratedAlways(complexUsage, true, true, false, true);

            complexUsage.FindProperty("IntProp").IsStoreGeneratedAlways = false;
            complexUsage.FindProperty("StringProp").IsStoreGeneratedAlways = false;

            AssertIsStoreGeneratedAlways(complexUsage, false, false, false, true);
        }

        private static void AssertIsStoreGeneratedAlways(ComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsStoreGeneratedAlways);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsStoreGeneratedAlways);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsStoreGeneratedAlways);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsStoreGeneratedAlways);

            AssertIsStoreGeneratedAlways((IMutableComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertIsStoreGeneratedAlways(IMutableComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsStoreGeneratedAlways);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsStoreGeneratedAlways);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsStoreGeneratedAlways);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsStoreGeneratedAlways);

            AssertIsStoreGeneratedAlways((IComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertIsStoreGeneratedAlways(IComplexTypeUsage complexUsage, bool facet11, bool facet12, bool facet21, bool facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").IsStoreGeneratedAlways);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").IsStoreGeneratedAlways);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").IsStoreGeneratedAlways);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").IsStoreGeneratedAlways);
        }

        [Fact]
        public void Complex_property_value_generated_facet_makes_use_of_definition()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            AssertValueGenerated(complexUsage, ValueGenerated.Never, ValueGenerated.Never, ValueGenerated.Never, ValueGenerated.Never);

            propDef11.ValueGeneratedDefault = ValueGenerated.OnAddOrUpdate;
            propDef12.ValueGeneratedDefault = ValueGenerated.Never;
            propDef21.ValueGeneratedDefault = ValueGenerated.Never;
            propDef22.ValueGeneratedDefault = ValueGenerated.OnAdd;

            AssertValueGenerated(complexUsage, ValueGenerated.OnAddOrUpdate, ValueGenerated.Never, ValueGenerated.Never, ValueGenerated.OnAdd);

            complexUsage.FindProperty("IntProp").ValueGenerated = ValueGenerated.OnAdd;
            complexUsage.FindProperty("StringProp").ValueGenerated = ValueGenerated.OnAdd;

            AssertValueGenerated(complexUsage, ValueGenerated.OnAdd, ValueGenerated.OnAdd, ValueGenerated.Never, ValueGenerated.OnAdd);
        }

        private static void AssertValueGenerated(ComplexTypeUsage complexUsage, ValueGenerated facet11, ValueGenerated facet12, ValueGenerated facet21, ValueGenerated facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").ValueGenerated);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").ValueGenerated);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").ValueGenerated);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").ValueGenerated);

            AssertValueGenerated((IMutableComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertValueGenerated(IMutableComplexTypeUsage complexUsage, ValueGenerated facet11, ValueGenerated facet12, ValueGenerated facet21, ValueGenerated facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").ValueGenerated);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").ValueGenerated);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").ValueGenerated);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").ValueGenerated);

            AssertValueGenerated((IComplexTypeUsage)complexUsage, facet11, facet12, facet21, facet22);
        }

        private static void AssertValueGenerated(IComplexTypeUsage complexUsage, ValueGenerated facet11, ValueGenerated facet12, ValueGenerated facet21, ValueGenerated facet22)
        {
            Assert.Equal(facet11, complexUsage.FindProperty("IntProp").ValueGenerated);
            Assert.Equal(facet12, complexUsage.FindProperty("StringProp").ValueGenerated);
            Assert.Equal(facet21, complexUsage.FindComplexTypeUsage("Nested").FindProperty("IntProp").ValueGenerated);
            Assert.Equal(facet22, complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp").ValueGenerated);
        }

        [Fact]
        public void Can_set_max_length_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.GetMaxLength());
            Assert.Null(nestedProperty.GetMaxLength());
            Assert.Null(propDef12.GetMaxLengthDefault());
            Assert.Null(propDef22.GetMaxLengthDefault());

            propDef12.SetMaxLengthDefault(77);
            propDef22.SetAnnotation(CoreAnnotationNames.MaxLengthAnnotation, 88, ConfigurationSource.DataAnnotation);

            Assert.Equal(77, propDef12.GetMaxLengthDefault());
            Assert.Equal(88, propDef22.GetMaxLengthDefault());

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(CoreAnnotationNames.MaxLengthAnnotation).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(CoreAnnotationNames.MaxLengthAnnotation).GetConfigurationSource());

            Assert.Equal(77, property.GetMaxLength());
            Assert.Equal(88, nestedProperty.GetMaxLength());

            Assert.Null(property.FindAnnotation(CoreAnnotationNames.MaxLengthAnnotation));
            Assert.Null(nestedProperty.FindAnnotation(CoreAnnotationNames.MaxLengthAnnotation));

            property.SetMaxLength(100);
            nestedProperty.SetAnnotation(CoreAnnotationNames.MaxLengthAnnotation, 200, ConfigurationSource.Convention);

            Assert.Equal(100, property.GetMaxLength());
            Assert.Equal(200, nestedProperty.GetMaxLength());

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(CoreAnnotationNames.MaxLengthAnnotation).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(CoreAnnotationNames.MaxLengthAnnotation).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_unicode_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.IsUnicode());
            Assert.Null(nestedProperty.IsUnicode());
            Assert.Null(propDef12.IsUnicodeDefault());
            Assert.Null(propDef22.IsUnicodeDefault());

            propDef12.IsUnicodeDefault(true);
            propDef22.SetAnnotation(CoreAnnotationNames.UnicodeAnnotation, false, ConfigurationSource.DataAnnotation);

            Assert.Equal(true, propDef12.IsUnicodeDefault());
            Assert.Equal(false, propDef22.IsUnicodeDefault());

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(CoreAnnotationNames.UnicodeAnnotation).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(CoreAnnotationNames.UnicodeAnnotation).GetConfigurationSource());

            Assert.Equal(true, property.IsUnicode());
            Assert.Equal(false, nestedProperty.IsUnicode());

            Assert.Null(property.FindAnnotation(CoreAnnotationNames.UnicodeAnnotation));
            Assert.Null(nestedProperty.FindAnnotation(CoreAnnotationNames.UnicodeAnnotation));

            property.IsUnicode(false);
            nestedProperty.SetAnnotation(CoreAnnotationNames.UnicodeAnnotation, true, ConfigurationSource.Convention);

            Assert.Equal(false, property.IsUnicode());
            Assert.Equal(true, nestedProperty.IsUnicode());

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(CoreAnnotationNames.UnicodeAnnotation).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(CoreAnnotationNames.UnicodeAnnotation).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_value_generation_factory_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.GetValueGeneratorFactory());
            Assert.Null(nestedProperty.GetValueGeneratorFactory());
            Assert.Null(propDef12.GetValueGeneratorFactoryDefault());
            Assert.Null(propDef22.GetValueGeneratorFactoryDefault());

            Func<IProperty, IEntityType, ValueGenerator> factory1 = (p, e) => null;
            Func<IProperty, IEntityType, ValueGenerator> factory2 = (p, e) => null;

            propDef12.SetValueGeneratorFactoryDefault(factory1);
            propDef22.SetAnnotation(CoreAnnotationNames.ValueGeneratorFactoryAnnotation, factory2, ConfigurationSource.DataAnnotation);

            Assert.Same(factory1, propDef12.GetValueGeneratorFactoryDefault());
            Assert.Same(factory2, propDef22.GetValueGeneratorFactoryDefault());

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(CoreAnnotationNames.ValueGeneratorFactoryAnnotation).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(CoreAnnotationNames.ValueGeneratorFactoryAnnotation).GetConfigurationSource());

            Assert.Same(factory1, property.GetValueGeneratorFactory());
            Assert.Same(factory2, nestedProperty.GetValueGeneratorFactory());

            Assert.Null(property.FindAnnotation(CoreAnnotationNames.ValueGeneratorFactoryAnnotation));
            Assert.Null(nestedProperty.FindAnnotation(CoreAnnotationNames.ValueGeneratorFactoryAnnotation));

            Func<IProperty, IEntityType, ValueGenerator> factory3 = (p, e) => null;
            Func<IProperty, IEntityType, ValueGenerator> factory4 = (p, e) => null;

            property.SetValueGeneratorFactory(factory3);
            nestedProperty.SetAnnotation(CoreAnnotationNames.ValueGeneratorFactoryAnnotation, factory4, ConfigurationSource.Convention);

            Assert.Same(factory3, property.GetValueGeneratorFactory());
            Assert.Same(factory4, nestedProperty.GetValueGeneratorFactory());

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(CoreAnnotationNames.ValueGeneratorFactoryAnnotation).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(CoreAnnotationNames.ValueGeneratorFactoryAnnotation).GetConfigurationSource());
        }

        [Fact]
        public void Can_set_property_access_mode_for_complex_property()
        {
            ComplexPropertyDefinition propDef11, propDef12, propDef21, propDef22;

            var complexUsage = BuildUsage(out propDef11, out propDef12, out propDef21, out propDef22);

            var property = complexUsage.FindProperty("StringProp");
            var nestedProperty = complexUsage.FindComplexTypeUsage("Nested").FindProperty("StringProp");

            Assert.Null(property.GetPropertyAccessMode());
            Assert.Null(nestedProperty.GetPropertyAccessMode());
            Assert.Null(propDef12.GetPropertyAccessMode());
            Assert.Null(propDef22.GetPropertyAccessMode());

            propDef12.SetPropertyAccessMode(PropertyAccessMode.Field);
            propDef22.SetAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation, PropertyAccessMode.Property, ConfigurationSource.DataAnnotation);

            Assert.Equal(PropertyAccessMode.Field, propDef12.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, propDef22.GetPropertyAccessMode());

            Assert.Equal(ConfigurationSource.Explicit, propDef12.FindAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.DataAnnotation, propDef22.FindAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation).GetConfigurationSource());

            Assert.Equal(PropertyAccessMode.Field, property.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, nestedProperty.GetPropertyAccessMode());

            Assert.Null(property.FindAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation));
            Assert.Null(nestedProperty.FindAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation));

            property.SetPropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
            nestedProperty.SetAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation, PropertyAccessMode.Field, ConfigurationSource.Convention);

            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, property.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Field, nestedProperty.GetPropertyAccessMode());

            Assert.Equal(ConfigurationSource.Explicit, property.FindAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation).GetConfigurationSource());
            Assert.Equal(ConfigurationSource.Convention, nestedProperty.FindAnnotation(CoreAnnotationNames.PropertyAccessModeAnnotation).GetConfigurationSource());
        }

        private static ComplexTypeUsage BuildUsage(
            out ComplexPropertyDefinition propDef11,
            out ComplexPropertyDefinition propDef12,
            out ComplexPropertyDefinition propDef21,
            out ComplexPropertyDefinition propDef22)
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));
            propDef11 = complexDef1.AddPropertyDefinition("IntProp");
            propDef12 = complexDef1.AddPropertyDefinition("StringProp");

            var complexDef2 = model.AddComplexTypeDefinition(typeof(Complex2));
            propDef21 = complexDef2.AddPropertyDefinition("IntProp");
            propDef22 = complexDef2.AddPropertyDefinition("StringProp");

            var entityType = model.AddEntityType(typeof(Entity1));

            var usage1 = entityType.AddComplexTypeUsage("Usage1", complexDef1);

            var nested1 = usage1.AddComplexTypeUsage(complexDef1.AddComplexTypeReferenceDefinition("Nested", complexDef2));

            usage1.AddProperty(propDef11);
            usage1.AddProperty(propDef12);

            nested1.AddProperty(propDef21);
            nested1.AddProperty(propDef22);

            return usage1;
        }

        private class Entity1
        {
            public int Id { get; set; }
            public Complex1 Usage { get; set; }
        }

        private class Complex1
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }

            public Complex2 Nested { get; set; }
        }

        private class Complex2
        {
            public int IntProp { get; set; }
            public string StringProp { get; set; }
        }
    }
}

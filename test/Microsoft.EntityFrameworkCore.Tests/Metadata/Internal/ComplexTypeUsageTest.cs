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
    public class ComplexTypeUsageTest
    {
        [Fact]
        public void Can_add_complex_type_usages_to_entity()
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complexDef2 = model.AddComplexTypeDefinition(typeof(Complex2));

            var reference1 = complexDef1.AddComplexTypeReferenceDefinition("Nested", complexDef2);
            var reference2 = complexDef1.AddComplexTypeReferenceDefinition("_fieldNested", complexDef2);
            var reference3 = complexDef1.AddComplexTypeReferenceDefinition("ShadowNested", complexDef2);

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage", complexDef1);
            var usage2 = entity.AddComplexTypeUsage("_fieldUsage", complexDef1);
            var usage3 = entity.AddComplexTypeUsage("Usage2", complexDef2);
            var usage4 = entity.AddComplexTypeUsage("ShadowUsage", complexDef2);

            var nested1 = usage1.AddComplexTypeUsage(reference1);
            var nested2 = usage1.AddComplexTypeUsage(reference2);
            var nested3 = usage1.AddComplexTypeUsage(reference3);

            AssertReferences(
                entity, complexDef1, complexDef2, reference1, reference2, reference3,
                usage1, usage2, usage3, usage4, nested1, nested2, nested3, ConfigurationSource.Explicit);
        }

        [Fact]
        public void Can_add_complex_type_usages_to_entity_using_interfaces()
        {
            IMutableModel model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complexDef2 = model.AddComplexTypeDefinition(typeof(Complex2));

            var reference1 = complexDef1.AddComplexTypeReferenceDefinition("Nested", complexDef2);
            var reference2 = complexDef1.AddComplexTypeReferenceDefinition("_fieldNested", complexDef2);
            var reference3 = complexDef1.AddComplexTypeReferenceDefinition("ShadowNested", complexDef2);

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage", complexDef1);
            var usage2 = entity.AddComplexTypeUsage("_fieldUsage", complexDef1);
            var usage3 = entity.AddComplexTypeUsage("Usage2", complexDef2);
            var usage4 = entity.AddComplexTypeUsage("ShadowUsage", complexDef2);

            var nested1 = usage1.AddComplexTypeUsage(reference1);
            var nested2 = usage1.AddComplexTypeUsage(reference2);
            var nested3 = usage1.AddComplexTypeUsage(reference3);

            AssertReferences(
                (EntityType)entity, (ComplexTypeDefinition)complexDef1, (ComplexTypeDefinition)complexDef2,
                (ComplexTypeReferenceDefinition)reference1, (ComplexTypeReferenceDefinition)reference2, (ComplexTypeReferenceDefinition)reference3,
                (ComplexTypeUsage)usage1, (ComplexTypeUsage)usage2, (ComplexTypeUsage)usage3, (ComplexTypeUsage)usage4,
                (ComplexTypeUsage)nested1, (ComplexTypeUsage)nested2, (ComplexTypeUsage)nested3, ConfigurationSource.Explicit);
        }

        [Fact]
        public void Can_add_complex_type_usages_to_entity_with_config_source()
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complexDef2 = model.AddComplexTypeDefinition(typeof(Complex2));

            var reference1 = complexDef1.AddComplexTypeReferenceDefinition("Nested", complexDef2);
            var reference2 = complexDef1.AddComplexTypeReferenceDefinition("_fieldNested", complexDef2);
            var reference3 = complexDef1.AddComplexTypeReferenceDefinition("ShadowNested", complexDef2);

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage", complexDef1, ConfigurationSource.Convention);
            var usage2 = entity.AddComplexTypeUsage("_fieldUsage", complexDef1, ConfigurationSource.Convention);
            var usage3 = entity.AddComplexTypeUsage("Usage2", complexDef2, ConfigurationSource.Convention);
            var usage4 = entity.AddComplexTypeUsage("ShadowUsage", complexDef2, ConfigurationSource.Convention);

            var nested1 = usage1.AddComplexTypeUsage(reference1, ConfigurationSource.Convention);
            var nested2 = usage1.AddComplexTypeUsage(reference2, ConfigurationSource.Convention);
            var nested3 = usage1.AddComplexTypeUsage(reference3, ConfigurationSource.Convention);

            AssertReferences(
                entity, complexDef1, complexDef2, reference1, reference2, reference3,
                usage1, usage2, usage3, usage4, nested1, nested2, nested3, ConfigurationSource.Convention);
        }

        private static void AssertReferences(
            EntityType entity,
            ComplexTypeDefinition complexDef1,
            ComplexTypeDefinition complexDef2,
            ComplexTypeReferenceDefinition reference1,
            ComplexTypeReferenceDefinition reference2,
            ComplexTypeReferenceDefinition reference3,
            ComplexTypeUsage usage1,
            ComplexTypeUsage usage2,
            ComplexTypeUsage usage3,
            ComplexTypeUsage usage4,
            ComplexTypeUsage nested1,
            ComplexTypeUsage nested2,
            ComplexTypeUsage nested3,
            ConfigurationSource configurationSource)
        {
            Assert.Equal(
                new[] { usage4, usage1, usage3, usage2 },
                entity.GetComplexTypeUsages().ToArray());

            Assert.Same(usage1, entity.FindComplexTypeUsage("Usage"));
            Assert.Same(usage2, entity.FindComplexTypeUsage("_fieldUsage"));
            Assert.Same(usage3, entity.FindComplexTypeUsage("Usage2"));
            Assert.Same(usage4, entity.FindComplexTypeUsage("ShadowUsage"));
            Assert.Null(entity.FindComplexTypeUsage("NotFound"));

            Assert.Equal(
                new[] { nested1, nested3, nested2 },
                usage1.GetComplexTypeUsages().ToArray());

            Assert.Same(nested1, usage1.FindComplexTypeUsage("Nested"));
            Assert.Same(nested2, usage1.FindComplexTypeUsage("_fieldNested"));
            Assert.Same(nested3, usage1.FindComplexTypeUsage("ShadowNested"));
            Assert.Null(usage1.FindComplexTypeUsage("NotFound"));

            Assert.Equal("Usage", usage1.Name);
            Assert.False(usage1.IsShadowProperty);
            Assert.Same(typeof(Complex1), usage1.ClrType);
            Assert.Same(entity, usage1.DeclaringType);
            Assert.Same(entity, usage1.DeclaringEntityType);
            Assert.Same(complexDef1, usage1.Definition);
            Assert.True(usage1.IsRequired);
            Assert.Null(usage1.FieldInfo);
            Assert.Same(typeof(Entity1).GetAnyProperty("Usage"), usage1.PropertyInfo);
            Assert.Equal(configurationSource, usage1.GetConfigurationSource());

            Assert.Equal("_fieldUsage", usage2.Name);
            Assert.False(usage2.IsShadowProperty);
            Assert.Same(typeof(Complex1), usage2.ClrType);
            Assert.Same(entity, usage2.DeclaringType);
            Assert.Same(entity, usage2.DeclaringEntityType);
            Assert.Same(complexDef1, usage2.Definition);
            Assert.True(usage2.IsRequired);
            Assert.Equal("_fieldUsage", usage2.FieldInfo?.Name);
            Assert.Null(usage2.PropertyInfo);
            Assert.Equal(configurationSource, usage2.GetConfigurationSource());

            Assert.Equal("Usage2", usage3.Name);
            Assert.False(usage3.IsShadowProperty);
            Assert.Same(typeof(Complex2), usage3.ClrType);
            Assert.Same(entity, usage3.DeclaringType);
            Assert.Same(entity, usage3.DeclaringEntityType);
            Assert.Same(complexDef2, usage3.Definition);
            Assert.True(usage3.IsRequired);
            Assert.Null(usage3.FieldInfo);
            Assert.Same(typeof(Entity1).GetAnyProperty("Usage2"), usage3.PropertyInfo);
            Assert.Equal(configurationSource, usage3.GetConfigurationSource());

            Assert.Equal("ShadowUsage", usage4.Name);
            Assert.True(usage4.IsShadowProperty);
            Assert.Same(typeof(Complex2), usage4.ClrType);
            Assert.Same(entity, usage4.DeclaringType);
            Assert.Same(entity, usage4.DeclaringEntityType);
            Assert.Same(complexDef2, usage4.Definition);
            Assert.True(usage4.IsRequired);
            Assert.Null(usage4.FieldInfo);
            Assert.Null(usage4.PropertyInfo);
            Assert.Equal(configurationSource, usage4.GetConfigurationSource());

            Assert.Equal("Nested", nested1.Name);
            Assert.False(nested1.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested1.ClrType);
            Assert.Same(usage1, nested1.DeclaringType);
            Assert.Same(entity, nested1.DeclaringEntityType);
            Assert.Same(complexDef2, nested1.Definition);
            Assert.True(nested1.IsRequired);
            Assert.Null(nested1.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Nested"), nested1.PropertyInfo);
            Assert.Equal(configurationSource, nested1.GetConfigurationSource());

            Assert.Equal("_fieldNested", nested2.Name);
            Assert.False(nested2.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested2.ClrType);
            Assert.Same(usage1, nested2.DeclaringType);
            Assert.Same(entity, nested2.DeclaringEntityType);
            Assert.Same(complexDef2, nested2.Definition);
            Assert.True(nested2.IsRequired);
            Assert.Equal("_fieldNested", nested2.FieldInfo?.Name);
            Assert.Null(nested2.PropertyInfo);
            Assert.Equal(configurationSource, nested2.GetConfigurationSource());

            Assert.Equal("ShadowNested", nested3.Name);
            Assert.True(nested3.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested3.ClrType);
            Assert.Same(usage1, nested3.DeclaringType);
            Assert.Same(entity, nested3.DeclaringEntityType);
            Assert.Same(complexDef2, nested3.Definition);
            Assert.True(nested3.IsRequired);
            Assert.Null(nested3.FieldInfo);
            Assert.Null(nested3.PropertyInfo);
            Assert.Equal(configurationSource, nested3.GetConfigurationSource());

            AssertReferences(
                entity, complexDef1, complexDef2, reference1, reference2, reference3,
                usage1, usage2, usage3, usage4, nested1, nested2, nested3);
        }

        private static void AssertReferences(
            IMutableEntityType entity,
            IMutableComplexTypeDefinition complexDef1,
            IMutableComplexTypeDefinition complexDef2,
            IMutableComplexTypeReferenceDefinition reference1,
            IMutableComplexTypeReferenceDefinition reference2,
            IMutableComplexTypeReferenceDefinition reference3,
            IMutableComplexTypeUsage usage1,
            IMutableComplexTypeUsage usage2,
            IMutableComplexTypeUsage usage3,
            IMutableComplexTypeUsage usage4,
            IMutableComplexTypeUsage nested1,
            IMutableComplexTypeUsage nested2,
            IMutableComplexTypeUsage nested3)
        {
            Assert.Equal(
                new[] { usage4, usage1, usage3, usage2 },
                entity.GetComplexTypeUsages().ToArray());

            Assert.Same(usage1, entity.FindComplexTypeUsage("Usage"));
            Assert.Same(usage2, entity.FindComplexTypeUsage("_fieldUsage"));
            Assert.Same(usage3, entity.FindComplexTypeUsage("Usage2"));
            Assert.Same(usage4, entity.FindComplexTypeUsage("ShadowUsage"));
            Assert.Null(entity.FindComplexTypeUsage("NotFound"));

            Assert.Equal(
                new[] { nested1, nested3, nested2 },
                usage1.GetComplexTypeUsages().ToArray());

            Assert.Same(nested1, usage1.FindComplexTypeUsage("Nested"));
            Assert.Same(nested2, usage1.FindComplexTypeUsage("_fieldNested"));
            Assert.Same(nested3, usage1.FindComplexTypeUsage("ShadowNested"));
            Assert.Null(usage1.FindComplexTypeUsage("NotFound"));

            Assert.Equal("Usage", usage1.Name);
            Assert.False(usage1.IsShadowProperty);
            Assert.Same(typeof(Complex1), usage1.ClrType);
            Assert.Same(entity, usage1.DeclaringType);
            Assert.Same(entity, usage1.DeclaringEntityType);
            Assert.Same(complexDef1, usage1.Definition);
            Assert.True(usage1.IsRequired);
            Assert.Null(usage1.FieldInfo);
            Assert.Same(typeof(Entity1).GetAnyProperty("Usage"), usage1.PropertyInfo);

            Assert.Equal("_fieldUsage", usage2.Name);
            Assert.False(usage2.IsShadowProperty);
            Assert.Same(typeof(Complex1), usage2.ClrType);
            Assert.Same(entity, usage2.DeclaringType);
            Assert.Same(entity, usage2.DeclaringEntityType);
            Assert.Same(complexDef1, usage2.Definition);
            Assert.True(usage2.IsRequired);
            Assert.Equal("_fieldUsage", usage2.FieldInfo?.Name);
            Assert.Null(usage2.PropertyInfo);

            Assert.Equal("Usage2", usage3.Name);
            Assert.False(usage3.IsShadowProperty);
            Assert.Same(typeof(Complex2), usage3.ClrType);
            Assert.Same(entity, usage3.DeclaringType);
            Assert.Same(entity, usage3.DeclaringEntityType);
            Assert.Same(complexDef2, usage3.Definition);
            Assert.True(usage3.IsRequired);
            Assert.Null(usage3.FieldInfo);
            Assert.Same(typeof(Entity1).GetAnyProperty("Usage2"), usage3.PropertyInfo);

            Assert.Equal("ShadowUsage", usage4.Name);
            Assert.True(usage4.IsShadowProperty);
            Assert.Same(typeof(Complex2), usage4.ClrType);
            Assert.Same(entity, usage4.DeclaringType);
            Assert.Same(entity, usage4.DeclaringEntityType);
            Assert.Same(complexDef2, usage4.Definition);
            Assert.True(usage4.IsRequired);
            Assert.Null(usage4.FieldInfo);
            Assert.Null(usage4.PropertyInfo);

            Assert.Equal("Nested", nested1.Name);
            Assert.False(nested1.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested1.ClrType);
            Assert.Same(usage1, nested1.DeclaringType);
            Assert.Same(entity, nested1.DeclaringEntityType);
            Assert.Same(complexDef2, nested1.Definition);
            Assert.True(nested1.IsRequired);
            Assert.Null(nested1.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Nested"), nested1.PropertyInfo);

            Assert.Equal("_fieldNested", nested2.Name);
            Assert.False(nested2.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested2.ClrType);
            Assert.Same(usage1, nested2.DeclaringType);
            Assert.Same(entity, nested2.DeclaringEntityType);
            Assert.Same(complexDef2, nested2.Definition);
            Assert.True(nested2.IsRequired);
            Assert.Equal("_fieldNested", nested2.FieldInfo?.Name);
            Assert.Null(nested2.PropertyInfo);

            Assert.Equal("ShadowNested", nested3.Name);
            Assert.True(nested3.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested3.ClrType);
            Assert.Same(usage1, nested3.DeclaringType);
            Assert.Same(entity, nested3.DeclaringEntityType);
            Assert.Same(complexDef2, nested3.Definition);
            Assert.True(nested3.IsRequired);
            Assert.Null(nested3.FieldInfo);
            Assert.Null(nested3.PropertyInfo);

            AssertReferences(
                entity, complexDef1, complexDef2, reference1, reference2, reference3,
                (IComplexTypeUsage)usage1, usage2, usage3, usage4, nested1, nested2, nested3);
        }

        private static void AssertReferences(
            IEntityType entity,
            IComplexTypeDefinition complexDef1,
            IComplexTypeDefinition complexDef2,
            IComplexTypeReferenceDefinition reference1,
            IComplexTypeReferenceDefinition reference2,
            IComplexTypeReferenceDefinition reference3,
            IComplexTypeUsage usage1,
            IComplexTypeUsage usage2,
            IComplexTypeUsage usage3,
            IComplexTypeUsage usage4,
            IComplexTypeUsage nested1,
            IComplexTypeUsage nested2,
            IComplexTypeUsage nested3)
        {
            Assert.Equal(
                new[] { usage4, usage1, usage3, usage2 },
                entity.GetComplexTypeUsages().ToArray());

            Assert.Same(usage1, entity.FindComplexTypeUsage("Usage"));
            Assert.Same(usage2, entity.FindComplexTypeUsage("_fieldUsage"));
            Assert.Same(usage3, entity.FindComplexTypeUsage("Usage2"));
            Assert.Same(usage4, entity.FindComplexTypeUsage("ShadowUsage"));
            Assert.Null(entity.FindComplexTypeUsage("NotFound"));

            Assert.Equal(
                new[] { nested1, nested3, nested2 },
                usage1.GetComplexTypeUsages().ToArray());

            Assert.Same(nested1, usage1.FindComplexTypeUsage("Nested"));
            Assert.Same(nested2, usage1.FindComplexTypeUsage("_fieldNested"));
            Assert.Same(nested3, usage1.FindComplexTypeUsage("ShadowNested"));
            Assert.Null(usage1.FindComplexTypeUsage("NotFound"));

            Assert.Equal("Usage", usage1.Name);
            Assert.False(usage1.IsShadowProperty);
            Assert.Same(typeof(Complex1), usage1.ClrType);
            Assert.Same(entity, usage1.DeclaringType);
            Assert.Same(entity, usage1.DeclaringEntityType);
            Assert.Same(complexDef1, usage1.Definition);
            Assert.True(usage1.IsRequired);
            Assert.Null(usage1.FieldInfo);
            Assert.Same(typeof(Entity1).GetAnyProperty("Usage"), usage1.PropertyInfo);

            Assert.Equal("_fieldUsage", usage2.Name);
            Assert.False(usage2.IsShadowProperty);
            Assert.Same(typeof(Complex1), usage2.ClrType);
            Assert.Same(entity, usage2.DeclaringType);
            Assert.Same(entity, usage2.DeclaringEntityType);
            Assert.Same(complexDef1, usage2.Definition);
            Assert.True(usage2.IsRequired);
            Assert.Equal("_fieldUsage", usage2.FieldInfo?.Name);
            Assert.Null(usage2.PropertyInfo);

            Assert.Equal("Usage2", usage3.Name);
            Assert.False(usage3.IsShadowProperty);
            Assert.Same(typeof(Complex2), usage3.ClrType);
            Assert.Same(entity, usage3.DeclaringType);
            Assert.Same(entity, usage3.DeclaringEntityType);
            Assert.Same(complexDef2, usage3.Definition);
            Assert.True(usage3.IsRequired);
            Assert.Null(usage3.FieldInfo);
            Assert.Same(typeof(Entity1).GetAnyProperty("Usage2"), usage3.PropertyInfo);

            Assert.Equal("ShadowUsage", usage4.Name);
            Assert.True(usage4.IsShadowProperty);
            Assert.Same(typeof(Complex2), usage4.ClrType);
            Assert.Same(entity, usage4.DeclaringType);
            Assert.Same(entity, usage4.DeclaringEntityType);
            Assert.Same(complexDef2, usage4.Definition);
            Assert.True(usage4.IsRequired);
            Assert.Null(usage4.FieldInfo);
            Assert.Null(usage4.PropertyInfo);

            Assert.Equal("Nested", nested1.Name);
            Assert.False(nested1.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested1.ClrType);
            Assert.Same(usage1, nested1.DeclaringType);
            Assert.Same(entity, nested1.DeclaringEntityType);
            Assert.Same(complexDef2, nested1.Definition);
            Assert.True(nested1.IsRequired);
            Assert.Null(nested1.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Nested"), nested1.PropertyInfo);

            Assert.Equal("_fieldNested", nested2.Name);
            Assert.False(nested2.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested2.ClrType);
            Assert.Same(usage1, nested2.DeclaringType);
            Assert.Same(entity, nested2.DeclaringEntityType);
            Assert.Same(complexDef2, nested2.Definition);
            Assert.True(nested2.IsRequired);
            Assert.Equal("_fieldNested", nested2.FieldInfo?.Name);
            Assert.Null(nested2.PropertyInfo);

            Assert.Equal("ShadowNested", nested3.Name);
            Assert.True(nested3.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested3.ClrType);
            Assert.Same(usage1, nested3.DeclaringType);
            Assert.Same(entity, nested3.DeclaringEntityType);
            Assert.Same(complexDef2, nested3.Definition);
            Assert.True(nested3.IsRequired);
            Assert.Null(nested3.FieldInfo);
            Assert.Null(nested3.PropertyInfo);

            AssertReferences(
                entity, complexDef1, complexDef2, reference1, reference2, reference3,
                (IPropertyBase)usage1, usage2, usage3, usage4, nested1, nested2, nested3);

            AssertReferences(
                entity, complexDef1, complexDef2, reference1, reference2, reference3,
                (ITypeBase)usage1, usage2, usage3, usage4, nested1, nested2, nested3);
        }

        private static void AssertReferences(
            IEntityType entity,
            IComplexTypeDefinition complexDef1,
            IComplexTypeDefinition complexDef2,
            IComplexTypeReferenceDefinition reference1,
            IComplexTypeReferenceDefinition reference2,
            IComplexTypeReferenceDefinition reference3,
            IPropertyBase usage1,
            IPropertyBase usage2,
            IPropertyBase usage3,
            IPropertyBase usage4,
            IPropertyBase nested1,
            IPropertyBase nested2,
            IPropertyBase nested3)
        {
            Assert.Equal(
                new[] { usage4, usage1, usage3, usage2 },
                entity.GetComplexTypeUsages().Cast<IPropertyBase>().ToArray());

            Assert.Same(usage1, entity.FindComplexTypeUsage("Usage"));
            Assert.Same(usage2, entity.FindComplexTypeUsage("_fieldUsage"));
            Assert.Same(usage3, entity.FindComplexTypeUsage("Usage2"));
            Assert.Same(usage4, entity.FindComplexTypeUsage("ShadowUsage"));
            Assert.Null(entity.FindComplexTypeUsage("NotFound"));

            Assert.Equal("Usage", usage1.Name);
            Assert.False(usage1.IsShadowProperty);
            Assert.Same(typeof(Complex1), usage1.ClrType);
            Assert.Same(entity, usage1.DeclaringType);
            Assert.Null(usage1.FieldInfo);
            Assert.Same(typeof(Entity1).GetAnyProperty("Usage"), usage1.PropertyInfo);

            Assert.Equal("_fieldUsage", usage2.Name);
            Assert.False(usage2.IsShadowProperty);
            Assert.Same(typeof(Complex1), usage2.ClrType);
            Assert.Same(entity, usage2.DeclaringType);
            Assert.Equal("_fieldUsage", usage2.FieldInfo?.Name);
            Assert.Null(usage2.PropertyInfo);

            Assert.Equal("Usage2", usage3.Name);
            Assert.False(usage3.IsShadowProperty);
            Assert.Same(typeof(Complex2), usage3.ClrType);
            Assert.Same(entity, usage3.DeclaringType);
            Assert.Null(usage3.FieldInfo);
            Assert.Same(typeof(Entity1).GetAnyProperty("Usage2"), usage3.PropertyInfo);

            Assert.Equal("ShadowUsage", usage4.Name);
            Assert.True(usage4.IsShadowProperty);
            Assert.Same(typeof(Complex2), usage4.ClrType);
            Assert.Same(entity, usage4.DeclaringType);
            Assert.Null(usage4.FieldInfo);
            Assert.Null(usage4.PropertyInfo);

            Assert.Equal("Nested", nested1.Name);
            Assert.False(nested1.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested1.ClrType);
            Assert.Same(usage1, nested1.DeclaringType);
            Assert.Null(nested1.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Nested"), nested1.PropertyInfo);

            Assert.Equal("_fieldNested", nested2.Name);
            Assert.False(nested2.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested2.ClrType);
            Assert.Same(usage1, nested2.DeclaringType);
            Assert.Equal("_fieldNested", nested2.FieldInfo?.Name);
            Assert.Null(nested2.PropertyInfo);

            Assert.Equal("ShadowNested", nested3.Name);
            Assert.True(nested3.IsShadowProperty);
            Assert.Same(typeof(Complex2), nested3.ClrType);
            Assert.Same(usage1, nested3.DeclaringType);
            Assert.Null(nested3.FieldInfo);
            Assert.Null(nested3.PropertyInfo);
        }

        private static void AssertReferences(
            IEntityType entity,
            IComplexTypeDefinition complexDef1,
            IComplexTypeDefinition complexDef2,
            IComplexTypeReferenceDefinition reference1,
            IComplexTypeReferenceDefinition reference2,
            IComplexTypeReferenceDefinition reference3,
            ITypeBase usage1,
            ITypeBase usage2,
            ITypeBase usage3,
            ITypeBase usage4,
            ITypeBase nested1,
            ITypeBase nested2,
            ITypeBase nested3)
        {
            Assert.Equal(
                new[] { usage4, usage1, usage3, usage2 },
                entity.GetComplexTypeUsages().Cast<ITypeBase>().ToArray());

            Assert.Same(usage1, entity.FindComplexTypeUsage("Usage"));
            Assert.Same(usage2, entity.FindComplexTypeUsage("_fieldUsage"));
            Assert.Same(usage3, entity.FindComplexTypeUsage("Usage2"));
            Assert.Same(usage4, entity.FindComplexTypeUsage("ShadowUsage"));
            Assert.Null(entity.FindComplexTypeUsage("NotFound"));

            Assert.Equal("Usage", usage1.Name);
            Assert.Same(typeof(Complex1), usage1.ClrType);

            Assert.Equal("_fieldUsage", usage2.Name);
            Assert.Same(typeof(Complex1), usage2.ClrType);

            Assert.Equal("Usage2", usage3.Name);
            Assert.Same(typeof(Complex2), usage3.ClrType);

            Assert.Equal("ShadowUsage", usage4.Name);
            Assert.Same(typeof(Complex2), usage4.ClrType);

            Assert.Equal("Nested", nested1.Name);
            Assert.Same(typeof(Complex2), nested1.ClrType);

            Assert.Equal("_fieldNested", nested2.Name);
            Assert.Same(typeof(Complex2), nested2.ClrType);

            Assert.Equal("ShadowNested", nested3.Name);
            Assert.Same(typeof(Complex2), nested3.ClrType);
        }

        [Fact]
        public void Can_remove_complex_type_usages()
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complexDef2 = model.AddComplexTypeDefinition(typeof(Complex2));

            var reference1 = complexDef1.AddComplexTypeReferenceDefinition("Nested", complexDef2);
            var reference2 = complexDef1.AddComplexTypeReferenceDefinition("_fieldNested", complexDef2);
            var reference3 = complexDef1.AddComplexTypeReferenceDefinition("ShadowNested", complexDef2);

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage", complexDef1);
            var usage2 = entity.AddComplexTypeUsage("_fieldUsage", complexDef1);
            var usage3 = entity.AddComplexTypeUsage("Usage2", complexDef2);
            var usage4 = entity.AddComplexTypeUsage("ShadowUsage", complexDef2);

            var nested1 = usage1.AddComplexTypeUsage(reference1);
            var nested2 = usage1.AddComplexTypeUsage(reference2);
            var nested3 = usage1.AddComplexTypeUsage(reference3);

            Assert.Equal(
                new[] { usage4, usage1, usage3, usage2 },
                entity.GetComplexTypeUsages().ToArray());

            Assert.Equal(
                new[] { nested1, nested3, nested2 },
                usage1.GetComplexTypeUsages().ToArray());

            usage1.RemoveComplexTypeUsage(reference2.Name);
            entity.RemoveComplexTypeUsage("ShadowUsage");

            Assert.Equal(
                new[] { usage1, usage3, usage2 },
                entity.GetComplexTypeUsages().ToArray());

            Assert.Equal(
                new[] { nested1, nested3 },
                usage1.GetComplexTypeUsages().ToArray());

            ((IMutableComplexTypeUsage)usage1).RemoveComplexTypeUsage(reference1.Name);
            ((IMutableEntityType)entity).RemoveComplexTypeUsage("_fieldUsage");

            Assert.Equal(
                new[] { usage1, usage3 },
                entity.GetComplexTypeUsages().ToArray());

            Assert.Equal(
                new[] { nested3 },
                usage1.GetComplexTypeUsages().ToArray());
        }

        [Fact]
        public void Throws_adding_usage_for_wrong_definition()
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complexDef2 = model.AddComplexTypeDefinition(typeof(Complex2));
            var complexDef3 = model.AddComplexTypeDefinition(typeof(Complex3));

            var reference1 = complexDef2.AddComplexTypeReferenceDefinition("NestedStruct", complexDef3);

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage", complexDef1);

            Assert.Equal(
                CoreStrings.ComplexReferenceWrongType("NestedStruct", nameof(Complex3), nameof(Complex1), nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => usage1.AddComplexTypeUsage(reference1)).Message);
        }

        [Fact]
        public void Can_add_properties_to_complex_type_usage()
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));

            var propDef1 = complexDef1.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"));
            var propDef2 = complexDef1.AddPropertyDefinition("Prop2");
            var propDef3 = complexDef1.AddPropertyDefinition("Prop3", typeof(byte));
            var propDef4 = complexDef1.AddPropertyDefinition("_field1");

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage1", complexDef1);

            var prop1 = usage1.AddProperty(propDef1);
            var prop2 = usage1.AddProperty(propDef2);
            var prop3 = usage1.AddProperty(propDef3);
            var prop4 = usage1.AddProperty(propDef4);

            AssertProperties(usage1, propDef1, propDef2, propDef3, propDef4, prop1, prop2, prop3, prop4, ConfigurationSource.Explicit);
        }

        [Fact]
        public void Can_add_properties_to_complex_type_usage_using_interfaces()
        {
            IMutableModel model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));

            var propDef1 = complexDef1.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"));
            var propDef2 = complexDef1.AddPropertyDefinition("Prop2", null);
            var propDef3 = complexDef1.AddPropertyDefinition("Prop3", typeof(byte));
            var propDef4 = complexDef1.AddPropertyDefinition("_field1", null);

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage1", complexDef1);

            var prop1 = usage1.AddProperty(propDef1);
            var prop2 = usage1.AddProperty(propDef2);
            var prop3 = usage1.AddProperty(propDef3);
            var prop4 = usage1.AddProperty(propDef4);

            AssertProperties(
                (ComplexTypeUsage)usage1,
                (ComplexPropertyDefinition)propDef1, (ComplexPropertyDefinition)propDef2, (ComplexPropertyDefinition)propDef3, (ComplexPropertyDefinition)propDef4,
                (ComplexProperty)prop1, (ComplexProperty)prop2, (ComplexProperty)prop3, (ComplexProperty)prop4,
                ConfigurationSource.Explicit);
        }

        [Fact]
        public void Can_add_properties_to_complex_type_usage_with_config_source()
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));

            var propDef1 = complexDef1.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"));
            var propDef2 = complexDef1.AddPropertyDefinition("Prop2");
            var propDef3 = complexDef1.AddPropertyDefinition("Prop3", typeof(byte));
            var propDef4 = complexDef1.AddPropertyDefinition("_field1");

            var entity = model.AddEntityType(typeof(Entity1));

            var complex1 = entity.AddComplexTypeUsage("Usage1", complexDef1, ConfigurationSource.Convention);

            var prop1 = complex1.AddProperty(propDef1, ConfigurationSource.Convention);
            var prop2 = complex1.AddProperty(propDef2, ConfigurationSource.Convention);
            var prop3 = complex1.AddProperty(propDef3, ConfigurationSource.Convention);
            var prop4 = complex1.AddProperty(propDef4, ConfigurationSource.Convention);

            AssertProperties(complex1, propDef1, propDef2, propDef3, propDef4, prop1, prop2, prop3, prop4, ConfigurationSource.Convention);
        }

        private static void AssertProperties(
            ComplexTypeUsage complex1,
            ComplexPropertyDefinition propDef1,
            ComplexPropertyDefinition propDef2,
            ComplexPropertyDefinition propDef3,
            ComplexPropertyDefinition propDef4,
            ComplexProperty prop1,
            ComplexProperty prop2,
            ComplexProperty prop3,
            ComplexProperty prop4,
            ConfigurationSource configurationSource)
        {
            Assert.Equal(
                new[] { prop1, prop2, prop3, prop4 },
                complex1.GetProperties().ToArray());

            Assert.Same(prop1, complex1.FindProperty(typeof(Complex1).GetAnyProperty("Prop1")));
            Assert.Same(prop1, complex1.FindProperty("Prop1"));
            Assert.Same(prop2, complex1.FindProperty(typeof(Complex1).GetAnyProperty("Prop2")));
            Assert.Same(prop2, complex1.FindProperty("Prop2"));
            Assert.Same(prop3, complex1.FindProperty("Prop3"));
            Assert.Same(prop4, complex1.FindProperty("_field1"));
            Assert.Null(complex1.FindProperty("NotFound"));
            Assert.Null(complex1.FindProperty(typeof(Complex1).GetAnyProperty("NotFound")));

            Assert.Equal("Prop1", prop1.Name);
            Assert.False(prop1.IsShadowProperty);
            Assert.Same(typeof(int), prop1.ClrType);
            Assert.Same(complex1, prop1.DeclaringType);
            Assert.Null(prop1.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Prop1"), prop1.PropertyInfo);
            Assert.Equal(configurationSource, prop1.GetConfigurationSource());
            Assert.Same(propDef1, prop1.Definition);

            Assert.Equal("Prop2", prop2.Name);
            Assert.False(prop2.IsShadowProperty);
            Assert.Same(typeof(string), prop2.ClrType);
            Assert.Same(complex1, prop2.DeclaringType);
            Assert.Null(prop2.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Prop2"), prop2.PropertyInfo);
            Assert.Equal(configurationSource, prop2.GetConfigurationSource());
            Assert.Same(propDef2, prop2.Definition);

            Assert.Equal("Prop3", prop3.Name);
            Assert.True(prop3.IsShadowProperty);
            Assert.Same(typeof(byte), prop3.ClrType);
            Assert.Same(complex1, prop3.DeclaringType);
            Assert.Null(prop3.FieldInfo);
            Assert.Null(prop3.PropertyInfo);
            Assert.Equal(configurationSource, prop3.GetConfigurationSource());
            Assert.Same(propDef3, prop3.Definition);

            Assert.Equal("_field1", prop4.Name);
            Assert.False(prop4.IsShadowProperty);
            Assert.Same(typeof(DateTime), prop4.ClrType);
            Assert.Same(complex1, prop4.DeclaringType);
            Assert.Equal("_field1", prop4.FieldInfo?.Name);
            Assert.Null(prop4.PropertyInfo);
            Assert.Equal(configurationSource, prop4.GetConfigurationSource());
            Assert.Same(propDef4, prop4.Definition);

            AssertProperties(complex1, propDef1, propDef2, propDef3, propDef4, prop1, prop2, prop3, prop4);
        }

        private static void AssertProperties(
            IMutableComplexTypeUsage complex1,
            IMutableComplexPropertyDefinition propDef1,
            IMutableComplexPropertyDefinition propDef2,
            IMutableComplexPropertyDefinition propDef3,
            IMutableComplexPropertyDefinition propDef4,
            IMutableComplexProperty prop1,
            IMutableComplexProperty prop2,
            IMutableComplexProperty prop3,
            IMutableComplexProperty prop4)
        {
            Assert.Equal(
                new[] { prop1, prop2, prop3, prop4 },
                complex1.GetProperties().ToArray());

            Assert.Same(prop1, complex1.FindProperty("Prop1"));
            Assert.Same(prop2, complex1.FindProperty("Prop2"));
            Assert.Same(prop3, complex1.FindProperty("Prop3"));
            Assert.Same(prop4, complex1.FindProperty("_field1"));
            Assert.Null(complex1.FindProperty("NotFound"));

            Assert.Equal("Prop1", prop1.Name);
            Assert.False(prop1.IsShadowProperty);
            Assert.Same(typeof(int), prop1.ClrType);
            Assert.Same(complex1, prop1.DeclaringType);
            Assert.Null(prop1.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Prop1"), prop1.PropertyInfo);
            Assert.Same(propDef1, prop1.Definition);

            Assert.Equal("Prop2", prop2.Name);
            Assert.False(prop2.IsShadowProperty);
            Assert.Same(typeof(string), prop2.ClrType);
            Assert.Same(complex1, prop2.DeclaringType);
            Assert.Null(prop2.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Prop2"), prop2.PropertyInfo);
            Assert.Same(propDef2, prop2.Definition);

            Assert.Equal("Prop3", prop3.Name);
            Assert.True(prop3.IsShadowProperty);
            Assert.Same(typeof(byte), prop3.ClrType);
            Assert.Same(complex1, prop3.DeclaringType);
            Assert.Null(prop3.FieldInfo);
            Assert.Null(prop3.PropertyInfo);
            Assert.Same(propDef3, prop3.Definition);

            Assert.Equal("_field1", prop4.Name);
            Assert.False(prop4.IsShadowProperty);
            Assert.Same(typeof(DateTime), prop4.ClrType);
            Assert.Same(complex1, prop4.DeclaringType);
            Assert.Equal("_field1", prop4.FieldInfo?.Name);
            Assert.Null(prop4.PropertyInfo);
            Assert.Same(propDef4, prop4.Definition);

            AssertProperties((IComplexTypeUsage)complex1, propDef1, propDef2, propDef3, propDef4, prop1, prop2, prop3, prop4);
        }

        private static void AssertProperties(
            IComplexTypeUsage complex1,
            IComplexPropertyDefinition propDef1,
            IComplexPropertyDefinition propDef2,
            IComplexPropertyDefinition propDef3,
            IComplexPropertyDefinition propDef4,
            IComplexProperty prop1,
            IComplexProperty prop2,
            IComplexProperty prop3,
            IComplexProperty prop4)
        {
            Assert.Equal(
                new[] { prop1, prop2, prop3, prop4 },
                complex1.GetProperties().ToArray());

            Assert.Same(prop1, complex1.FindProperty("Prop1"));
            Assert.Same(prop2, complex1.FindProperty("Prop2"));
            Assert.Same(prop3, complex1.FindProperty("Prop3"));
            Assert.Same(prop4, complex1.FindProperty("_field1"));
            Assert.Null(complex1.FindProperty("NotFound"));

            Assert.Equal("Prop1", prop1.Name);
            Assert.False(prop1.IsShadowProperty);
            Assert.Same(typeof(int), prop1.ClrType);
            Assert.Same(complex1, prop1.DeclaringType);
            Assert.Null(prop1.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Prop1"), prop1.PropertyInfo);
            Assert.Same(propDef1, prop1.Definition);

            Assert.Equal("Prop2", prop2.Name);
            Assert.False(prop2.IsShadowProperty);
            Assert.Same(typeof(string), prop2.ClrType);
            Assert.Same(complex1, prop2.DeclaringType);
            Assert.Null(prop2.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Prop2"), prop2.PropertyInfo);
            Assert.Same(propDef2, prop2.Definition);

            Assert.Equal("Prop3", prop3.Name);
            Assert.True(prop3.IsShadowProperty);
            Assert.Same(typeof(byte), prop3.ClrType);
            Assert.Same(complex1, prop3.DeclaringType);
            Assert.Null(prop3.FieldInfo);
            Assert.Null(prop3.PropertyInfo);
            Assert.Same(propDef3, prop3.Definition);

            Assert.Equal("_field1", prop4.Name);
            Assert.False(prop4.IsShadowProperty);
            Assert.Same(typeof(DateTime), prop4.ClrType);
            Assert.Same(complex1, prop4.DeclaringType);
            Assert.Equal("_field1", prop4.FieldInfo?.Name);
            Assert.Null(prop4.PropertyInfo);
            Assert.Same(propDef4, prop4.Definition);

            AssertProperties(complex1, prop1, prop2, prop3, prop4);
        }

        private static void AssertProperties(
            ITypeBase complex1,
            IPropertyBase prop1,
            IPropertyBase prop2,
            IPropertyBase prop3,
            IPropertyBase prop4)
        {
            Assert.Equal("Prop1", prop1.Name);
            Assert.Same(typeof(int), prop1.ClrType);
            Assert.Same(complex1, prop1.DeclaringType);
            Assert.Null(prop1.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Prop1"), prop1.PropertyInfo);

            Assert.Equal("Prop2", prop2.Name);
            Assert.Same(typeof(string), prop2.ClrType);
            Assert.Same(complex1, prop2.DeclaringType);
            Assert.Null(prop2.FieldInfo);
            Assert.Same(typeof(Complex1).GetAnyProperty("Prop2"), prop2.PropertyInfo);

            Assert.Equal("Prop3", prop3.Name);
            Assert.Same(typeof(byte), prop3.ClrType);
            Assert.Same(complex1, prop3.DeclaringType);
            Assert.Null(prop3.FieldInfo);
            Assert.Null(prop3.PropertyInfo);

            Assert.Equal("_field1", prop4.Name);
            Assert.Same(typeof(DateTime), prop4.ClrType);
            Assert.Same(complex1, prop4.DeclaringType);
            Assert.Equal("_field1", prop4.FieldInfo?.Name);
            Assert.Null(prop4.PropertyInfo);
        }

        [Fact]
        public void Can_remove_properties_from_complex_type_usage()
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));

            var propDef1 = complexDef1.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"));
            var propDef2 = complexDef1.AddPropertyDefinition("Prop2");
            var propDef3 = complexDef1.AddPropertyDefinition("Prop3", typeof(byte));
            var propDef4 = complexDef1.AddPropertyDefinition("_field1");

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage1", complexDef1);

            var prop1 = usage1.AddProperty(propDef1);
            var prop2 = usage1.AddProperty(propDef2);
            var prop3 = usage1.AddProperty(propDef3);
            var prop4 = usage1.AddProperty(propDef4);

            Assert.Equal(
                new[] { prop1, prop2, prop3, prop4 },
                usage1.GetProperties().ToArray());

            usage1.RemoveProperty(prop1.Name);

            Assert.Equal(
                new[] { prop2, prop3, prop4 },
                usage1.GetProperties().ToArray());

            ((IMutableComplexTypeUsage)usage1).RemoveProperty(prop3.Name);

            Assert.Equal(
                new[] { prop2, prop4 },
                usage1.GetProperties().ToArray());
        }

        [Fact]
        public void Throws_when_adding_property_definition_from_wrong_type()
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complexDef2 = model.AddComplexTypeDefinition(typeof(Complex2));

            var propDef2 = complexDef2.AddPropertyDefinition("Prop1");

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage1", complexDef1);

            Assert.Equal(
                CoreStrings.ComplexPropertyWrongType("Prop1", nameof(Complex1), nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => usage1.AddProperty(propDef2)).Message);
        }

        [Fact]
        public void Can_change_requiredness_for_class_complex_type()
        {
            var model = new Model();

            var complexDef1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complexDef2 = model.AddComplexTypeDefinition(typeof(Complex2));

            var reference1 = complexDef1.AddComplexTypeReferenceDefinition("Nested", complexDef2);

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage", complexDef1);
            var usage2 = entity.AddComplexTypeUsage("_fieldUsage", complexDef1);
            var usage3 = entity.AddComplexTypeUsage("Usage2", complexDef2);

            var nested1 = usage1.AddComplexTypeUsage(reference1);

            Assert.True(usage1.IsRequired);
            Assert.True(usage2.IsRequired);
            Assert.True(usage3.IsRequired);
            Assert.True(nested1.IsRequired);

            Assert.Null(usage1.IsRequiredConfigurationSource);
            Assert.Null(usage2.IsRequiredConfigurationSource);
            Assert.Null(usage3.IsRequiredConfigurationSource);
            Assert.Null(nested1.IsRequiredConfigurationSource);

            reference1.IsRequired = false;

            Assert.True(usage1.IsRequired);
            Assert.True(usage2.IsRequired);
            Assert.True(usage3.IsRequired);
            Assert.False(nested1.IsRequired);

            Assert.Null(usage1.IsRequiredConfigurationSource);
            Assert.Null(usage2.IsRequiredConfigurationSource);
            Assert.Null(usage3.IsRequiredConfigurationSource);
            Assert.Null(nested1.IsRequiredConfigurationSource);

            usage1.SetIsRequired(false, ConfigurationSource.Convention);
            usage2.IsRequired = false;
            ((IMutableComplexTypeUsage)usage3).IsRequired = false;
            nested1.SetIsRequired(true, ConfigurationSource.DataAnnotation);

            Assert.False(usage1.IsRequired);
            Assert.False(((IComplexTypeUsage)usage2).IsRequired);
            Assert.False(((IMutableComplexTypeUsage)usage3).IsRequired);
            Assert.True(nested1.IsRequired);

            Assert.Equal(ConfigurationSource.Convention, usage1.IsRequiredConfigurationSource);
            Assert.Equal(ConfigurationSource.Explicit, usage2.IsRequiredConfigurationSource);
            Assert.Equal(ConfigurationSource.Explicit, usage3.IsRequiredConfigurationSource);
            Assert.Equal(ConfigurationSource.DataAnnotation, nested1.IsRequiredConfigurationSource);
        }

        [Fact]
        public void Cannot_change_requiredness_for_struct_complex_type()
        {
            var model = new Model();

            var complexDef2 = model.AddComplexTypeDefinition(typeof(Complex2));
            var complexDef3 = model.AddComplexTypeDefinition(typeof(Complex3));

            var reference1 = complexDef2.AddComplexTypeReferenceDefinition("NestedStruct", complexDef3);

            var entity = model.AddEntityType(typeof(Entity1));

            var usage1 = entity.AddComplexTypeUsage("Usage2", complexDef2);
            var usage2 = entity.AddComplexTypeUsage("StructUsage", complexDef3);

            var nested1 = usage1.AddComplexTypeUsage(reference1);

            Assert.True(usage1.IsRequired);
            Assert.True(nested1.IsRequired);

            Assert.Equal(
                CoreStrings.ComplexTypeStructIsRequired("StructUsage", nameof(Complex3), nameof(Entity1)),
                Assert.Throws<InvalidOperationException>(() => usage2.SetIsRequired(false, ConfigurationSource.Convention)).Message);

            Assert.Equal(
                CoreStrings.ComplexTypeStructIsRequired("NestedStruct", nameof(Complex3), nameof(Complex2)),
                Assert.Throws<InvalidOperationException>(() => nested1.IsRequired = false).Message);

            Assert.True(usage1.IsRequired);
            Assert.True(nested1.IsRequired);
        }

        private class Entity1
        {
            public int Id { get; set; }
            public Complex1 Usage { get; set; }
#pragma warning disable 169
            private Complex1 _fieldUsage;
#pragma warning restore 169
            public Complex2 Usage2 { get; set; }
            public Complex3 StructUsage { get; set; }
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
            public Complex3 NestedStruct { get; set; }
        }

        private struct Complex3
        {
        }
    }
}

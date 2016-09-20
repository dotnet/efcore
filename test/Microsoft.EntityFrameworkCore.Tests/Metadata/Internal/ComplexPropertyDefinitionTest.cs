// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Internal
{
    public class ComplexPropertyDefinitionTest
    {
        [Fact]
        public void Can_change_nullability_defaults()
        {
            var complexType = new Model().AddComplexTypeDefinition(typeof(Complex1));
            var prop = complexType.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"));

            Assert.Null(prop.IsNullableDefault);
            Assert.Null(((IMutableComplexPropertyDefinition)prop).IsNullableDefault);
            Assert.Null(((IComplexPropertyDefinition)prop).IsNullableDefault);
            Assert.Null(prop.IsNullableConfigurationSource);

            prop.SetIsNullableDefault(true, ConfigurationSource.DataAnnotation);

            Assert.Equal(true, prop.IsNullableDefault);
            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).IsNullableDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).IsNullableDefault);
            Assert.Equal(ConfigurationSource.DataAnnotation, prop.IsNullableConfigurationSource);

            prop.IsNullableDefault = false;

            Assert.Equal(false, prop.IsNullableDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).IsNullableDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).IsNullableDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsNullableConfigurationSource);

            prop.SetIsNullableDefault(true, ConfigurationSource.Convention);

            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).IsNullableDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).IsNullableDefault);
            Assert.Equal(true, prop.IsNullableDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsNullableConfigurationSource);

            ((IMutableComplexPropertyDefinition)prop).IsNullableDefault = false;

            Assert.Equal(false, prop.IsNullableDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).IsNullableDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).IsNullableDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsNullableConfigurationSource);
        }

        [Fact]
        public void Can_change_ReadOnlyBeforeSave_defaults()
        {
            var complexType = new Model().AddComplexTypeDefinition(typeof(Complex1));
            var prop = complexType.AddPropertyDefinition("Prop2");

            Assert.Null(prop.IsReadOnlyBeforeSaveDefault);
            Assert.Null(((IMutableComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault);
            Assert.Null(((IComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault);
            Assert.Null(prop.IsReadOnlyBeforeSaveConfigurationSource);

            prop.SetIsReadOnlyBeforeSaveDefault(true, ConfigurationSource.DataAnnotation);

            Assert.Equal(true, prop.IsReadOnlyBeforeSaveDefault);
            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault);
            Assert.Equal(ConfigurationSource.DataAnnotation, prop.IsReadOnlyBeforeSaveConfigurationSource);

            prop.IsReadOnlyBeforeSaveDefault = false;

            Assert.Equal(false, prop.IsReadOnlyBeforeSaveDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsReadOnlyBeforeSaveConfigurationSource);

            prop.SetIsReadOnlyBeforeSaveDefault(true, ConfigurationSource.Convention);

            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault);
            Assert.Equal(true, prop.IsReadOnlyBeforeSaveDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsReadOnlyBeforeSaveConfigurationSource);

            ((IMutableComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault = false;

            Assert.Equal(false, prop.IsReadOnlyBeforeSaveDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).IsReadOnlyBeforeSaveDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsReadOnlyBeforeSaveConfigurationSource);
        }

        [Fact]
        public void Can_change_ReadOnlyAfterSave_defaults()
        {
            var complexType = new Model().AddComplexTypeDefinition(typeof(Complex1));
            var prop = complexType.AddPropertyDefinition("Prop3", typeof(byte));

            Assert.Null(prop.IsReadOnlyAfterSaveDefault);
            Assert.Null(((IMutableComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault);
            Assert.Null(((IComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault);
            Assert.Null(prop.IsReadOnlyAfterSaveConfigurationSource);

            prop.SetIsReadOnlyAfterSaveDefault(true, ConfigurationSource.DataAnnotation);

            Assert.Equal(true, prop.IsReadOnlyAfterSaveDefault);
            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault);
            Assert.Equal(ConfigurationSource.DataAnnotation, prop.IsReadOnlyAfterSaveConfigurationSource);

            prop.IsReadOnlyAfterSaveDefault = false;

            Assert.Equal(false, prop.IsReadOnlyAfterSaveDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsReadOnlyAfterSaveConfigurationSource);

            prop.SetIsReadOnlyAfterSaveDefault(true, ConfigurationSource.Convention);

            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault);
            Assert.Equal(true, prop.IsReadOnlyAfterSaveDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsReadOnlyAfterSaveConfigurationSource);

            ((IMutableComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault = false;

            Assert.Equal(false, prop.IsReadOnlyAfterSaveDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).IsReadOnlyAfterSaveDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsReadOnlyAfterSaveConfigurationSource);
        }

        [Fact]
        public void Can_change_RequiresValueGenerator_defaults()
        {
            var complexType = new Model().AddComplexTypeDefinition(typeof(Complex1));
            var prop = complexType.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"));

            Assert.Null(prop.RequiresValueGeneratorDefault);
            Assert.Null(((IMutableComplexPropertyDefinition)prop).RequiresValueGeneratorDefault);
            Assert.Null(((IComplexPropertyDefinition)prop).RequiresValueGeneratorDefault);
            Assert.Null(prop.RequiresValueGeneratorConfigurationSource);

            prop.SetRequiresValueGeneratorDefault(true, ConfigurationSource.DataAnnotation);

            Assert.Equal(true, prop.RequiresValueGeneratorDefault);
            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).RequiresValueGeneratorDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).RequiresValueGeneratorDefault);
            Assert.Equal(ConfigurationSource.DataAnnotation, prop.RequiresValueGeneratorConfigurationSource);

            prop.RequiresValueGeneratorDefault = false;

            Assert.Equal(false, prop.RequiresValueGeneratorDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).RequiresValueGeneratorDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).RequiresValueGeneratorDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.RequiresValueGeneratorConfigurationSource);

            prop.SetRequiresValueGeneratorDefault(true, ConfigurationSource.Convention);

            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).RequiresValueGeneratorDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).RequiresValueGeneratorDefault);
            Assert.Equal(true, prop.RequiresValueGeneratorDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.RequiresValueGeneratorConfigurationSource);

            ((IMutableComplexPropertyDefinition)prop).RequiresValueGeneratorDefault = false;

            Assert.Equal(false, prop.RequiresValueGeneratorDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).RequiresValueGeneratorDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).RequiresValueGeneratorDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.RequiresValueGeneratorConfigurationSource);
        }

        [Fact]
        public void Can_change_ConcurrencyToken_defaults()
        {
            var complexType = new Model().AddComplexTypeDefinition(typeof(Complex1));
            var prop = complexType.AddPropertyDefinition("_field1");

            Assert.Null(prop.IsConcurrencyTokenDefault);
            Assert.Null(((IMutableComplexPropertyDefinition)prop).IsConcurrencyTokenDefault);
            Assert.Null(((IComplexPropertyDefinition)prop).IsConcurrencyTokenDefault);
            Assert.Null(prop.IsConcurrencyTokenConfigurationSource);

            prop.SetIsConcurrencyTokenDefault(true, ConfigurationSource.DataAnnotation);

            Assert.Equal(true, prop.IsConcurrencyTokenDefault);
            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).IsConcurrencyTokenDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).IsConcurrencyTokenDefault);
            Assert.Equal(ConfigurationSource.DataAnnotation, prop.IsConcurrencyTokenConfigurationSource);

            prop.IsConcurrencyTokenDefault = false;

            Assert.Equal(false, prop.IsConcurrencyTokenDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).IsConcurrencyTokenDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).IsConcurrencyTokenDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsConcurrencyTokenConfigurationSource);

            prop.SetIsConcurrencyTokenDefault(true, ConfigurationSource.Convention);

            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).IsConcurrencyTokenDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).IsConcurrencyTokenDefault);
            Assert.Equal(true, prop.IsConcurrencyTokenDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsConcurrencyTokenConfigurationSource);

            ((IMutableComplexPropertyDefinition)prop).IsConcurrencyTokenDefault = false;

            Assert.Equal(false, prop.IsConcurrencyTokenDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).IsConcurrencyTokenDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).IsConcurrencyTokenDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsConcurrencyTokenConfigurationSource);
        }

        [Fact]
        public void Can_change_StoreGeneratedAlways_defaults()
        {
            var complexType = new Model().AddComplexTypeDefinition(typeof(Complex1));
            var prop = complexType.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"));

            Assert.Null(prop.IsStoreGeneratedAlwaysDefault);
            Assert.Null(((IMutableComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault);
            Assert.Null(((IComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault);
            Assert.Null(prop.IsStoreGeneratedAlwaysConfigurationSource);

            prop.SetIsStoreGeneratedAlwaysDefault(true, ConfigurationSource.DataAnnotation);

            Assert.Equal(true, prop.IsStoreGeneratedAlwaysDefault);
            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault);
            Assert.Equal(ConfigurationSource.DataAnnotation, prop.IsStoreGeneratedAlwaysConfigurationSource);

            prop.IsStoreGeneratedAlwaysDefault = false;

            Assert.Equal(false, prop.IsStoreGeneratedAlwaysDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsStoreGeneratedAlwaysConfigurationSource);

            prop.SetIsStoreGeneratedAlwaysDefault(true, ConfigurationSource.Convention);

            Assert.Equal(true, ((IMutableComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault);
            Assert.Equal(true, ((IComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault);
            Assert.Equal(true, prop.IsStoreGeneratedAlwaysDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsStoreGeneratedAlwaysConfigurationSource);

            ((IMutableComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault = false;

            Assert.Equal(false, prop.IsStoreGeneratedAlwaysDefault);
            Assert.Equal(false, ((IMutableComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault);
            Assert.Equal(false, ((IComplexPropertyDefinition)prop).IsStoreGeneratedAlwaysDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.IsStoreGeneratedAlwaysConfigurationSource);
        }

        [Fact]
        public void Can_change_ValueGenerated_defaults()
        {
            var complexType = new Model().AddComplexTypeDefinition(typeof(Complex1));
            var prop = complexType.AddPropertyDefinition(typeof(Complex1).GetAnyProperty("Prop1"));

            Assert.Null(prop.ValueGeneratedDefault);
            Assert.Null(((IMutableComplexPropertyDefinition)prop).ValueGeneratedDefault);
            Assert.Null(((IComplexPropertyDefinition)prop).ValueGeneratedDefault);
            Assert.Null(prop.ValueGeneratedConfigurationSource);

            prop.SetValueGeneratedDefault(ValueGenerated.Never, ConfigurationSource.DataAnnotation);

            Assert.Equal(ValueGenerated.Never, prop.ValueGeneratedDefault);
            Assert.Equal(ValueGenerated.Never, ((IMutableComplexPropertyDefinition)prop).ValueGeneratedDefault);
            Assert.Equal(ValueGenerated.Never, ((IComplexPropertyDefinition)prop).ValueGeneratedDefault);
            Assert.Equal(ConfigurationSource.DataAnnotation, prop.ValueGeneratedConfigurationSource);

            prop.ValueGeneratedDefault = ValueGenerated.OnAdd;

            Assert.Equal(ValueGenerated.OnAdd, prop.ValueGeneratedDefault);
            Assert.Equal(ValueGenerated.OnAdd, ((IMutableComplexPropertyDefinition)prop).ValueGeneratedDefault);
            Assert.Equal(ValueGenerated.OnAdd, ((IComplexPropertyDefinition)prop).ValueGeneratedDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.ValueGeneratedConfigurationSource);

            prop.SetValueGeneratedDefault(ValueGenerated.OnAddOrUpdate, ConfigurationSource.Convention);

            Assert.Equal(ValueGenerated.OnAddOrUpdate, ((IMutableComplexPropertyDefinition)prop).ValueGeneratedDefault);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, ((IComplexPropertyDefinition)prop).ValueGeneratedDefault);
            Assert.Equal(ValueGenerated.OnAddOrUpdate, prop.ValueGeneratedDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.ValueGeneratedConfigurationSource);

            ((IMutableComplexPropertyDefinition)prop).ValueGeneratedDefault = ValueGenerated.OnAdd;

            Assert.Equal(ValueGenerated.OnAdd, prop.ValueGeneratedDefault);
            Assert.Equal(ValueGenerated.OnAdd, ((IMutableComplexPropertyDefinition)prop).ValueGeneratedDefault);
            Assert.Equal(ValueGenerated.OnAdd, ((IComplexPropertyDefinition)prop).ValueGeneratedDefault);
            Assert.Equal(ConfigurationSource.Explicit, prop.ValueGeneratedConfigurationSource);
        }

        private class Complex1
        {
#pragma warning disable 169
            private DateTime _field1;
#pragma warning restore 169

            public int Prop1 { get; set; }
            public string Prop2 { get; set; }
        }
    }
}

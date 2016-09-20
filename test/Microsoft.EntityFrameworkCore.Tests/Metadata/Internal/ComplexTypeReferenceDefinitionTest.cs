// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Internal
{
    public class ComplexTypeReferenceDefinitionTest
    {
        [Fact]
        public void Can_change_requiredness_for_class_complex_type()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complex2 = model.AddComplexTypeDefinition(typeof(Complex2));

            var reference1 = complex1.AddComplexTypeReferenceDefinition("Nested", complex2);
            var reference2 = complex1.AddComplexTypeReferenceDefinition("_fieldNested", complex2);
            var reference3 = complex1.AddComplexTypeReferenceDefinition("ShadowNested", complex2);

            Assert.True(reference1.IsRequired);
            Assert.True(((IMutableComplexTypeReferenceDefinition)reference2).IsRequired);
            Assert.True(((IComplexTypeReferenceDefinition)reference3).IsRequired);
            Assert.Null(reference1.IsRequiredConfigurationSource);
            Assert.Null(reference2.IsRequiredConfigurationSource);
            Assert.Null(reference3.IsRequiredConfigurationSource);

            reference1.SetIsRequired(false, ConfigurationSource.Convention);
            reference2.IsRequired = false;
            ((IMutableComplexTypeReferenceDefinition)reference3).IsRequired = false;

            Assert.False(reference1.IsRequired);
            Assert.False(((IMutableComplexTypeReferenceDefinition)reference2).IsRequired);
            Assert.False(((IComplexTypeReferenceDefinition)reference3).IsRequired);
            Assert.Equal(ConfigurationSource.Convention, reference1.IsRequiredConfigurationSource);
            Assert.Equal(ConfigurationSource.Explicit, reference2.IsRequiredConfigurationSource);
            Assert.Equal(ConfigurationSource.Explicit, reference3.IsRequiredConfigurationSource);
        }

        [Fact]
        public void Cannot_change_requiredness_for_struct_complex_type()
        {
            var model = new Model();

            var complex1 = model.AddComplexTypeDefinition(typeof(Complex1));
            var complex3 = model.AddComplexTypeDefinition(typeof(Complex3));

            var reference1 = complex1.AddComplexTypeReferenceDefinition("NestedStruct", complex3);
            var reference2 = complex1.AddComplexTypeReferenceDefinition("_fieldNestedStruct", complex3);
            var reference3 = complex1.AddComplexTypeReferenceDefinition("ShadowNestedStruct", complex3);

            Assert.True(reference1.IsRequired);
            Assert.True(((IMutableComplexTypeReferenceDefinition)reference2).IsRequired);
            Assert.True(((IComplexTypeReferenceDefinition)reference3).IsRequired);
            Assert.Null(reference1.IsRequiredConfigurationSource);
            Assert.Null(reference2.IsRequiredConfigurationSource);
            Assert.Null(reference3.IsRequiredConfigurationSource);

            Assert.Equal(
                CoreStrings.ComplexTypeStructIsRequired("NestedStruct", nameof(Complex3), nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => reference1.SetIsRequired(false, ConfigurationSource.Convention)).Message);

            Assert.Equal(
                CoreStrings.ComplexTypeStructIsRequired("_fieldNestedStruct", nameof(Complex3), nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => reference2.IsRequired = false).Message);

            Assert.Equal(
                CoreStrings.ComplexTypeStructIsRequired("ShadowNestedStruct", nameof(Complex3), nameof(Complex1)),
                Assert.Throws<InvalidOperationException>(() => ((IMutableComplexTypeReferenceDefinition)reference3).IsRequired = false).Message);

            Assert.True(reference1.IsRequired);
            Assert.True(((IMutableComplexTypeReferenceDefinition)reference2).IsRequired);
            Assert.True(((IComplexTypeReferenceDefinition)reference3).IsRequired);
            Assert.Null(reference1.IsRequiredConfigurationSource);
            Assert.Null(reference2.IsRequiredConfigurationSource);
            Assert.Null(reference3.IsRequiredConfigurationSource);

            reference1.SetIsRequired(true, ConfigurationSource.Convention);
            reference2.IsRequired = true;
            ((IMutableComplexTypeReferenceDefinition)reference3).IsRequired = true;

            Assert.True(reference1.IsRequired);
            Assert.True(((IMutableComplexTypeReferenceDefinition)reference2).IsRequired);
            Assert.True(((IComplexTypeReferenceDefinition)reference3).IsRequired);
            Assert.Equal(ConfigurationSource.Convention, reference1.IsRequiredConfigurationSource);
            Assert.Equal(ConfigurationSource.Explicit, reference2.IsRequiredConfigurationSource);
            Assert.Equal(ConfigurationSource.Explicit, reference3.IsRequiredConfigurationSource);
        }

        private class Complex1
        {
#pragma warning disable 169
            private Complex2 _fieldNested;
            private Complex3 _fieldNestedStruct;
#pragma warning restore 169

            public Complex2 Nested { get; set; }

            public Complex3 NestedStruct { get; set; }
        }

        private class Complex2
        {
        }

        private struct Complex3
        {
        }
    }
}

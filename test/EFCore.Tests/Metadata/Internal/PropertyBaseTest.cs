// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.Extensions;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.TestUtilities;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable ConvertToAutoProperty
namespace Microsoft.EntityFrameworkCore.Metadata.Internal
{
    public class PropertyBaseTest
    {
        private const string Property = "Foo";
        private const string Reference = "Reference";
        private const string Collection = "Collection";

        [Fact]
        public void Get_MemberInfos_for_auto_props()
        {
            const string field = "<Foo>k__BackingField";
            var property = CreateProperty<AutoProp>(field);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, field, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.Property, Property, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, field, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, Property, Property, Property);
        }

        [Fact]
        public void Get_MemberInfos_for_full_props()
        {
            const string field = "_foo";
            var property = CreateProperty<FullProp>(field);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, field, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.Property, Property, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, field, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, Property, Property, Property);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_props()
        {
            const string field = "_foo";
            var property = CreateProperty<ReadOnlyProp>(field);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, field, field, Property);
            MemberInfoTest(property, PropertyAccessMode.Property, NoSetter<ReadOnlyProp>(), NoSetter<ReadOnlyProp>(), Property);
            MemberInfoTest(property, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, field, field, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, field, field, Property);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_auto_props()
        {
            const string field = "<Foo>k__BackingField";
            var property = CreateProperty<ReadOnlyAutoProp>(field);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, field, field, Property);
            MemberInfoTest(property, PropertyAccessMode.Property, NoSetter<ReadOnlyAutoProp>(), NoSetter<ReadOnlyAutoProp>(), Property);
            MemberInfoTest(property, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, field, field, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, field, field, Property);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_field_props()
        {
            const string field = "_foo";
            var property = CreateProperty<ReadOnlyFieldProp>(field);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, field, field, Property);
            MemberInfoTest(property, PropertyAccessMode.Property, NoSetter<ReadOnlyFieldProp>(), NoSetter<ReadOnlyFieldProp>(), Property);
            MemberInfoTest(property, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, field, field, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, field, field, Property);
        }

        [Fact]
        public void Get_MemberInfos_for_write_only_props()
        {
            const string field = "_foo";
            var property = CreateProperty<WriteOnlyProp>(field);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, field, Property, field);
            MemberInfoTest(property, PropertyAccessMode.Property, Property, Property, NoGetter<WriteOnlyProp>());
            MemberInfoTest(property, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, field, Property, field);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, Property, Property, field);
        }

        [Fact]
        public void Get_MemberInfos_for_field_only_props()
        {
            const string field = "_foo";
            var property = CreateProperty<FieldOnly>(field, field);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, field, field, field);
            MemberInfoTest(
                property, PropertyAccessMode.Property, NoProperty<FieldOnly>(field), NoProperty<FieldOnly>(field),
                NoProperty<FieldOnly>(field));
            MemberInfoTest(property, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, field, field, field);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_field_only_props()
        {
            const string field = "_foo";
            var property = CreateProperty<ReadOnlyFieldOnly>(field, field);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, field, field, field);
            MemberInfoTest(
                property, PropertyAccessMode.Property, NoProperty<ReadOnlyFieldOnly>(field), NoProperty<ReadOnlyFieldOnly>(field),
                NoProperty<ReadOnlyFieldOnly>(field));
            MemberInfoTest(property, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, field, field, field);
        }

        [Fact]
        public void Get_MemberInfos_for_full_props_with_field_not_found()
        {
            var property = CreateProperty<FullPropNoField>(null);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, Property, Property, Property);
            MemberInfoTest(
                property, PropertyAccessMode.Field, NoField<FullPropNoField>(), NoField<FullPropNoField>(), NoField<FullPropNoField>());
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, NoField<FullPropNoField>(), Property, Property);
            MemberInfoTest(property, PropertyAccessMode.Property, Property, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferField, Property, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, Property, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, Property, Property, Property);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_props_with_field_not_found()
        {
            var property = CreateProperty<ReadOnlyPropNoField>(null);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, NoFieldOrSetter<ReadOnlyPropNoField>(), NoFieldOrSetter<ReadOnlyPropNoField>(), Property);
            MemberInfoTest(
                property, PropertyAccessMode.Field, NoField<ReadOnlyPropNoField>(), NoField<ReadOnlyPropNoField>(),
                NoField<ReadOnlyPropNoField>());
            MemberInfoTest(
                property, PropertyAccessMode.FieldDuringConstruction, NoField<ReadOnlyPropNoField>(),
                NoFieldOrSetter<ReadOnlyPropNoField>(), Property);
            MemberInfoTest(
                property, PropertyAccessMode.Property, NoSetter<ReadOnlyPropNoField>(), NoSetter<ReadOnlyPropNoField>(), Property);
            MemberInfoTest(
                property, PropertyAccessMode.PreferField, NoFieldOrSetter<ReadOnlyPropNoField>(), NoFieldOrSetter<ReadOnlyPropNoField>(),
                Property);
            MemberInfoTest(
                property, PropertyAccessMode.PreferFieldDuringConstruction, NoFieldOrSetter<ReadOnlyPropNoField>(),
                NoFieldOrSetter<ReadOnlyPropNoField>(), Property);
            MemberInfoTest(
                property, PropertyAccessMode.PreferProperty, NoFieldOrSetter<ReadOnlyPropNoField>(), NoFieldOrSetter<ReadOnlyPropNoField>(),
                Property);
        }

        [Fact]
        public void Get_MemberInfos_for_write_only_props_with_field_not_found()
        {
            var property = CreateProperty<WriteOnlyPropNoField>(null);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, Property, Property, NoFieldOrGetter<WriteOnlyPropNoField>());
            MemberInfoTest(
                property, PropertyAccessMode.Field, NoField<WriteOnlyPropNoField>(), NoField<WriteOnlyPropNoField>(),
                NoField<WriteOnlyPropNoField>());
            MemberInfoTest(
                property, PropertyAccessMode.FieldDuringConstruction, NoField<WriteOnlyPropNoField>(), Property,
                NoFieldOrGetter<WriteOnlyPropNoField>());
            MemberInfoTest(property, PropertyAccessMode.Property, Property, Property, NoGetter<WriteOnlyPropNoField>());
            MemberInfoTest(property, PropertyAccessMode.PreferField, Property, Property, NoFieldOrGetter<WriteOnlyPropNoField>());
            MemberInfoTest(
                property, PropertyAccessMode.PreferFieldDuringConstruction, Property, Property, NoFieldOrGetter<WriteOnlyPropNoField>());
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, Property, Property, NoFieldOrGetter<WriteOnlyPropNoField>());
        }

        [Fact]
        public void Get_MemberInfos_for_full_props_private_setter_in_base()
        {
            const string field = "_foo";
            var property = CreateProperty<PrivateSetterInBase>(field);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, field, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.Property, Property, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, field, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, Property, Property, Property);
        }

        [Fact]
        public void Get_MemberInfos_for_full_props_private_getter_in_base()
        {
            const string field = "_foo";
            var property = CreateProperty<PrivateGetterInBase>(field);
            Assert.False(property.IsShadowProperty());

            MemberInfoTest(property, null, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.FieldDuringConstruction, field, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.Property, Property, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(property, PropertyAccessMode.PreferFieldDuringConstruction, field, Property, Property);
            MemberInfoTest(property, PropertyAccessMode.PreferProperty, Property, Property, Property);
        }

        [Fact]
        public void Get_MemberInfos_for_auto_prop_navigations()
        {
            const string field = "<Reference>k__BackingField";
            var navigation = CreateReferenceNavigation<AutoProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Reference, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Reference, Reference, Reference);
        }

        [Fact]
        public void Get_MemberInfos_for_full_prop_navigations()
        {
            const string field = "_reference";
            var navigation = CreateReferenceNavigation<FullProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Reference, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Reference, Reference, Reference);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_prop_navigations()
        {
            const string field = "_reference";
            var navigation = CreateReferenceNavigation<ReadOnlyProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, field, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.Property, NoSetterRef<ReadOnlyProp>(), NoSetterRef<ReadOnlyProp>(), Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, field, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, field, field, Reference);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_auto_prop_navigations()
        {
            const string field = "<Reference>k__BackingField";
            var navigation = CreateReferenceNavigation<ReadOnlyAutoProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, field, Reference);
            MemberInfoTest(
                navigation, PropertyAccessMode.Property, NoSetterRef<ReadOnlyAutoProp>(), NoSetterRef<ReadOnlyAutoProp>(), Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, field, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, field, field, Reference);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_field_prop_navigations()
        {
            const string field = "_reference";
            var navigation = CreateReferenceNavigation<ReadOnlyFieldProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, field, Reference);
            MemberInfoTest(
                navigation, PropertyAccessMode.Property, NoSetterRef<ReadOnlyFieldProp>(), NoSetterRef<ReadOnlyFieldProp>(), Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, field, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, field, field, Reference);
        }

        [Fact]
        public void Get_MemberInfos_for_write_only_prop_navigations()
        {
            const string field = "_reference";
            var navigation = CreateReferenceNavigation<WriteOnlyProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, Reference, field);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Reference, Reference, NoGetterRef<WriteOnlyProp>());
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, Reference, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Reference, Reference, field);
        }

        [Fact]
        public void Get_MemberInfos_for_full_prop_navigations_with_field_not_found()
        {
            var navigation = CreateReferenceNavigation<FullPropNoField>(null);

            MemberInfoTest(navigation, null, Reference, Reference, Reference);
            MemberInfoTest(
                navigation, PropertyAccessMode.Field, NoFieldRef<FullPropNoField>(), NoFieldRef<FullPropNoField>(),
                NoFieldRef<FullPropNoField>());
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, NoFieldRef<FullPropNoField>(), Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Reference, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, Reference, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, Reference, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Reference, Reference, Reference);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_prop_navigations_with_field_not_found()
        {
            var navigation = CreateReferenceNavigation<ReadOnlyPropNoField>(null);

            MemberInfoTest(
                navigation, null, NoFieldOrSetterRef<ReadOnlyPropNoField>(), NoFieldOrSetterRef<ReadOnlyPropNoField>(), Reference);
            MemberInfoTest(
                navigation, PropertyAccessMode.Field, NoFieldRef<ReadOnlyPropNoField>(), NoFieldRef<ReadOnlyPropNoField>(),
                NoFieldRef<ReadOnlyPropNoField>());
            MemberInfoTest(
                navigation, PropertyAccessMode.FieldDuringConstruction, NoFieldRef<ReadOnlyPropNoField>(),
                NoFieldOrSetterRef<ReadOnlyPropNoField>(), Reference);
            MemberInfoTest(
                navigation, PropertyAccessMode.Property, NoSetterRef<ReadOnlyPropNoField>(), NoSetterRef<ReadOnlyPropNoField>(), Reference);
            MemberInfoTest(
                navigation, PropertyAccessMode.PreferField, NoFieldOrSetterRef<ReadOnlyPropNoField>(),
                NoFieldOrSetterRef<ReadOnlyPropNoField>(), Reference);
            MemberInfoTest(
                navigation, PropertyAccessMode.PreferFieldDuringConstruction, NoFieldOrSetterRef<ReadOnlyPropNoField>(),
                NoFieldOrSetterRef<ReadOnlyPropNoField>(), Reference);
            MemberInfoTest(
                navigation, PropertyAccessMode.PreferProperty, NoFieldOrSetterRef<ReadOnlyPropNoField>(),
                NoFieldOrSetterRef<ReadOnlyPropNoField>(), Reference);
        }

        [Fact]
        public void Get_MemberInfos_for_write_only_prop_navigations_with_field_not_found()
        {
            var navigation = CreateReferenceNavigation<WriteOnlyPropNoField>(null);

            MemberInfoTest(navigation, null, Reference, Reference, NoFieldOrGetterRef<WriteOnlyPropNoField>());
            MemberInfoTest(
                navigation, PropertyAccessMode.Field, NoFieldRef<WriteOnlyPropNoField>(), NoFieldRef<WriteOnlyPropNoField>(),
                NoFieldRef<WriteOnlyPropNoField>());
            MemberInfoTest(
                navigation, PropertyAccessMode.FieldDuringConstruction, NoFieldRef<WriteOnlyPropNoField>(), Reference,
                NoFieldOrGetterRef<WriteOnlyPropNoField>());
            MemberInfoTest(navigation, PropertyAccessMode.Property, Reference, Reference, NoGetterRef<WriteOnlyPropNoField>());
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, Reference, Reference, NoFieldOrGetterRef<WriteOnlyPropNoField>());
            MemberInfoTest(
                navigation, PropertyAccessMode.PreferFieldDuringConstruction, Reference, Reference,
                NoFieldOrGetterRef<WriteOnlyPropNoField>());
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Reference, Reference, NoFieldOrGetterRef<WriteOnlyPropNoField>());
        }

        [Fact]
        public void Get_MemberInfos_for_full_prop_navigations_private_setter_in_base()
        {
            const string field = "_reference";
            var navigation = CreateReferenceNavigation<PrivateSetterInBase>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Reference, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Reference, Reference, Reference);
        }

        [Fact]
        public void Get_MemberInfos_for_full_prop_navigations_private_getter_in_base()
        {
            const string field = "_reference";
            var navigation = CreateReferenceNavigation<PrivateGetterInBase>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Reference, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, Reference, Reference);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Reference, Reference, Reference);
        }

        [Fact]
        public void Get_MemberInfos_for_auto_prop_collection_navigations()
        {
            const string field = "<Collection>k__BackingField";
            var navigation = CreateCollectionNavigation<AutoProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Collection, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Collection, Collection, Collection);
        }

        [Fact]
        public void Get_MemberInfos_for_full_prop_collection_navigations()
        {
            const string field = "_collection";
            var navigation = CreateCollectionNavigation<FullProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Collection, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Collection, Collection, Collection);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_prop_collection_navigations()
        {
            const string field = "_collection";
            var navigation = CreateCollectionNavigation<ReadOnlyProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, field, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Property, null, null, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, field, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, field, field, Collection);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_auto_prop_collection_navigations()
        {
            const string field = "<Collection>k__BackingField";
            var navigation = CreateCollectionNavigation<ReadOnlyAutoProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, field, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Property, null, null, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, field, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, field, field, Collection);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_field_prop_collection_navigations()
        {
            const string field = "_collection";
            var navigation = CreateCollectionNavigation<ReadOnlyFieldProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, field, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Property, null, null, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, field, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, field, field, Collection);
        }

        [Fact]
        public void Get_MemberInfos_for_write_only_prop_collection_navigations()
        {
            const string field = "_collection";
            var navigation = CreateCollectionNavigation<WriteOnlyProp>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, Collection, field);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Collection, Collection, NoGetterColl<WriteOnlyProp>());
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, field, Collection, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Collection, Collection, field);
        }

        [Fact]
        public void Get_MemberInfos_for_full_prop_collection_navigations_with_field_not_found()
        {
            var navigation = CreateCollectionNavigation<FullPropNoField>(null);

            MemberInfoTest(navigation, null, Collection, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Field, null, null, NoFieldColl<FullPropNoField>());
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, null, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Collection, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, Collection, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, Collection, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Collection, Collection, Collection);
        }

        [Fact]
        public void Get_MemberInfos_for_read_only_prop_collection_navigations_with_field_not_found()
        {
            var navigation = CreateCollectionNavigation<ReadOnlyPropNoField>(null);

            MemberInfoTest(navigation, null, null, null, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Field, null, null, NoFieldColl<ReadOnlyPropNoField>());
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, null, null, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Property, null, null, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, null, null, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferFieldDuringConstruction, null, null, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, null, null, Collection);
        }

        [Fact]
        public void Get_MemberInfos_for_write_only_prop_collection_navigations_with_field_not_found()
        {
            var navigation = CreateCollectionNavigation<WriteOnlyPropNoField>(null);

            MemberInfoTest(navigation, null, Collection, Collection, NoFieldOrGetterColl<WriteOnlyPropNoField>());
            MemberInfoTest(navigation, PropertyAccessMode.Field, null, null, NoFieldColl<WriteOnlyPropNoField>());
            MemberInfoTest(
                navigation, PropertyAccessMode.FieldDuringConstruction, null, Collection, NoFieldOrGetterColl<WriteOnlyPropNoField>());
            MemberInfoTest(navigation, PropertyAccessMode.Property, Collection, Collection, NoGetterColl<WriteOnlyPropNoField>());
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, Collection, Collection, NoFieldOrGetterColl<WriteOnlyPropNoField>());
            MemberInfoTest(
                navigation, PropertyAccessMode.PreferProperty, Collection, Collection, NoFieldOrGetterColl<WriteOnlyPropNoField>());
        }

        [Fact]
        public void Get_MemberInfos_for_full_prop_collection_navigations_private_setter_in_base()
        {
            const string field = "_collection";
            var navigation = CreateCollectionNavigation<PrivateSetterInBase>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Collection, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Collection, Collection, Collection);
        }

        [Fact]
        public void Get_MemberInfos_for_full_prop_collection_navigations_private_getter_in_base()
        {
            const string field = "_collection";
            var navigation = CreateCollectionNavigation<PrivateGetterInBase>(field);

            MemberInfoTest(navigation, null, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.Field, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.FieldDuringConstruction, field, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.Property, Collection, Collection, Collection);
            MemberInfoTest(navigation, PropertyAccessMode.PreferField, field, field, field);
            MemberInfoTest(navigation, PropertyAccessMode.PreferProperty, Collection, Collection, Collection);
        }

        private static string NoProperty<TEntity>(string fieldName)
            => CoreStrings.NoProperty(fieldName, typeof(TEntity).Name, nameof(PropertyAccessMode));

        private static string NoField<TEntity>()
            => CoreStrings.NoBackingField(Property, typeof(TEntity).Name, nameof(PropertyAccessMode));

        private static string NoSetter<TEntity>()
            => CoreStrings.NoSetter(Property, typeof(TEntity).Name, nameof(PropertyAccessMode));

        private static string NoFieldOrSetter<TEntity>()
            => CoreStrings.NoFieldOrSetter(Property, typeof(TEntity).Name);

        private static string NoFieldOrGetter<TEntity>()
            => CoreStrings.NoFieldOrGetter(Property, typeof(TEntity).Name);

        private static string NoGetter<TEntity>()
            => CoreStrings.NoGetter(Property, typeof(TEntity).Name, nameof(PropertyAccessMode));

        private static string NoFieldRef<TEntity>()
            => CoreStrings.NoBackingField(Reference, typeof(TEntity).Name, nameof(PropertyAccessMode));

        private static string NoSetterRef<TEntity>()
            => CoreStrings.NoSetter(Reference, typeof(TEntity).Name, nameof(PropertyAccessMode));

        private static string NoFieldOrSetterRef<TEntity>()
            => CoreStrings.NoFieldOrSetter(Reference, typeof(TEntity).Name);

        private static string NoFieldOrGetterRef<TEntity>()
            => CoreStrings.NoFieldOrGetter(Reference, typeof(TEntity).Name);

        private static string NoGetterRef<TEntity>()
            => CoreStrings.NoGetter(Reference, typeof(TEntity).Name, nameof(PropertyAccessMode));

        private static string NoFieldColl<TEntity>()
            => CoreStrings.NoBackingField(Collection, typeof(TEntity).Name, nameof(PropertyAccessMode));

        private static string NoFieldOrGetterColl<TEntity>()
            => CoreStrings.NoFieldOrGetter(Collection, typeof(TEntity).Name);

        private static string NoGetterColl<TEntity>()
            => CoreStrings.NoGetter(Collection, typeof(TEntity).Name, nameof(PropertyAccessMode));

        private static Property CreateProperty<TEntity>(string fieldName, string propertyName = Property)
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(TEntity));
            entityType.SetPrimaryKey(entityType.AddProperty("Id", typeof(int)));
            var property = entityType.AddProperty(propertyName, typeof(int));
            property.SetField(fieldName);
            return property;
        }

        private static Navigation CreateReferenceNavigation<TEntity>(
            string fieldName, string navigationName = Reference)
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(TEntity));
            var property = entityType.AddProperty("Id", typeof(int));
            var key = entityType.SetPrimaryKey(property);
            var foreignKey = entityType.AddForeignKey(property, key, entityType);
            var navigation = foreignKey.HasDependentToPrincipal(typeof(TEntity).GetProperty(navigationName));
            navigation.SetField(fieldName);
            return navigation;
        }

        private static Navigation CreateCollectionNavigation<TEntity>(
            string fieldName, string navigationName = Collection)
        {
            var model = new Model();
            var entityType = model.AddEntityType(typeof(TEntity));
            var property = entityType.AddProperty("Id", typeof(int));
            var key = entityType.SetPrimaryKey(property);
            var foreignKey = entityType.AddForeignKey(property, key, entityType);
            var navigation = foreignKey.HasPrincipalToDependent(typeof(TEntity).GetProperty(navigationName));
            navigation.SetField(fieldName);
            return navigation;
        }

        private void MemberInfoTest(
            IMutableProperty property, PropertyAccessMode? accessMode, string forConstruction, string forSet, string forGet)
        {
            property.SetPropertyAccessMode(accessMode);

            MemberInfoTestCommon(property, accessMode, forConstruction, forSet, forGet);
        }

        private void MemberInfoTest(
            IMutableNavigation navigation, PropertyAccessMode? accessMode, string forConstruction, string forSet, string forGet)
        {
            navigation.SetPropertyAccessMode(accessMode);

            MemberInfoTestCommon(navigation, accessMode, forConstruction, forSet, forGet);
        }

        private void MemberInfoTestCommon(
            IPropertyBase propertyBase, PropertyAccessMode? accessMode, string forConstruction, string forSet, string forGet)
        {
            string failMessage = null;
            try
            {
                var memberInfo = propertyBase.GetMemberInfo(forConstruction: true, forSet: true);
                Assert.Equal(forConstruction, memberInfo?.Name);

                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo != null)
                {
                    Assert.NotNull(propertyInfo.SetMethod);
                }
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equal(forConstruction, ex.Message);
                failMessage = ex.Message;
            }

            try
            {
                var memberInfo = propertyBase.GetMemberInfo(forConstruction: false, forSet: true);
                Assert.Equal(forSet, memberInfo?.Name);

                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo != null)
                {
                    Assert.NotNull(propertyInfo.SetMethod);
                }
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equal(forSet, ex.Message);
                failMessage = failMessage ?? ex.Message;
            }

            try
            {
                var memberInfo = propertyBase.GetMemberInfo(forConstruction: false, forSet: false);
                Assert.Equal(forGet, memberInfo?.Name);

                var propertyInfo = memberInfo as PropertyInfo;
                if (propertyInfo != null)
                {
                    Assert.NotNull(propertyInfo.GetMethod);
                }
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equal(forGet, ex.Message);
                failMessage = failMessage ?? ex.Message;
            }

            try
            {
                InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<IModelValidator>()
                    .Validate(propertyBase.DeclaringType.Model,
                        new DiagnosticsLoggers(
                            new TestLogger<DbLoggerCategory.Model, LoggingDefinitions>(),
                            new TestLogger<DbLoggerCategory.Model.Validation, LoggingDefinitions>()));

                Assert.Null(failMessage);
            }
            catch (InvalidOperationException ex)
            {
                Assert.Equal(failMessage, ex.Message);
            }
        }

        [Fact]
        public virtual void Access_mode_can_be_overriden_at_entity_and_property_levels()
        {
            IMutableModel model = new Model();

            var entityType1 = model.AddEntityType(typeof(FullProp));
            var e1p1 = entityType1.AddProperty("Id", typeof(int));
            var e1p2 = entityType1.AddProperty("Foo", typeof(int));

            var entityType2 = model.AddEntityType(typeof(ReadOnlyProp));
            var e2p1 = entityType2.AddProperty("Id", typeof(int));
            var e2p2 = entityType2.AddProperty("Foo", typeof(int));

            model.SetPropertyAccessMode(PropertyAccessMode.Field);
            entityType2.SetPropertyAccessMode(PropertyAccessMode.FieldDuringConstruction);
            e2p2.SetPropertyAccessMode(PropertyAccessMode.Property);

            Assert.Equal(PropertyAccessMode.Field, model.GetPropertyAccessMode());

            Assert.Equal(PropertyAccessMode.Field, entityType1.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Field, e1p1.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Field, e1p2.GetPropertyAccessMode());

            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, entityType2.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.FieldDuringConstruction, e2p1.GetPropertyAccessMode());
            Assert.Equal(PropertyAccessMode.Property, e2p2.GetPropertyAccessMode());
        }

        [Fact]
        public virtual void Properties_can_have_field_cleared()
        {
            var propertyInfo = typeof(FullProp).GetAnyProperty("Foo");

            Properties_can_have_field_cleared_test(
                new Model().AddEntityType(typeof(FullProp)).AddProperty(propertyInfo), propertyInfo, "_foo");
        }

        [Fact]
        public virtual void Field_only_properties_throws_when_field_cleared()
        {
            var propertyBase = new Model().AddEntityType(typeof(FieldOnly)).AddProperty("_foo", typeof(int));

            Assert.Equal(CoreStrings.FieldNameMismatch(null, nameof(FieldOnly), "_foo"),
                Assert.Throws<InvalidOperationException>(() => propertyBase.SetField(null)).Message);
        }

        [Fact]
        public virtual void Navigations_can_have_field_cleared()
        {
            var entityType = new Model().AddEntityType(typeof(FullProp));
            var property = entityType.AddProperty("Id", typeof(int));
            var key = entityType.SetPrimaryKey(property);
            var foreignKey = entityType.AddForeignKey(property, key, entityType);

            var propertyInfo = typeof(FullProp).GetAnyProperty("Reference");

            Properties_can_have_field_cleared_test(
                foreignKey.HasDependentToPrincipal(propertyInfo), propertyInfo, "_reference");
        }

        private void Properties_can_have_field_cleared_test(PropertyBase propertyBase, PropertyInfo propertyInfo, string fieldName)
        {
            Assert.Null(propertyBase.GetFieldName());
            Assert.Null(propertyBase.FieldInfo);
            Assert.Same(propertyInfo, propertyBase.GetIdentifyingMemberInfo());

            propertyBase.SetField(fieldName, ConfigurationSource.Explicit);

            Assert.Equal(fieldName, propertyBase.GetFieldName());
            var fieldInfo = propertyBase.FieldInfo;
            Assert.Equal(fieldName, fieldInfo.Name);
            Assert.Same(propertyInfo ?? (MemberInfo)fieldInfo, propertyBase.GetIdentifyingMemberInfo());

            propertyBase.SetField((string)null, ConfigurationSource.Explicit);

            Assert.Null(propertyBase.GetFieldName());
            Assert.Null(propertyBase.FieldInfo);
            Assert.Same(propertyInfo, propertyBase.GetIdentifyingMemberInfo());

            propertyBase.SetField(fieldInfo, ConfigurationSource.Explicit);

            Assert.Equal(fieldName, propertyBase.GetFieldName());
            Assert.Same(propertyInfo ?? (MemberInfo)fieldInfo, propertyBase.GetIdentifyingMemberInfo());

            propertyBase.SetField((FieldInfo)null, ConfigurationSource.Explicit);

            Assert.Null(propertyBase.GetFieldName());
            Assert.Null(propertyBase.FieldInfo);
            Assert.Same(propertyInfo, propertyBase.GetIdentifyingMemberInfo());
        }

        [Fact]
        public virtual void Setting_fieldInfo_for_shadow_property_throws()
        {
            IMutableModel model = new Model();

            var entityType = model.AddEntityType(typeof(FullProp));
            var property = entityType.AddProperty("shadow", typeof(int));

            Assert.Equal(CoreStrings.FieldNameMismatch("_foo", nameof(FullProp), "shadow"),
                Assert.Throws<InvalidOperationException>(() => property.SetField("_foo")).Message);
        }

        private class AutoProp
        {
            public int Id { get; set; }
            public int Foo { get; set; }
            public AutoProp Reference { get; set; }
            public IEnumerable<AutoProp> Collection { get; set; }
        }

        private class FullProp
        {
            private int _foo;
            private FullProp _reference;
            private IEnumerable<FullProp> _collection;

            public int Id { get; set; }

            public int Foo
            {
                get => _foo;
                set => _foo = value;
            }

            public FullProp Reference
            {
                get => _reference;
                set => _reference = value;
            }

            public IEnumerable<FullProp> Collection
            {
                get => _collection;
                set => _collection = value;
            }
        }

        private class ReadOnlyProp
        {
            private readonly int _foo;
            private readonly ReadOnlyProp _reference;
            private readonly IEnumerable<ReadOnlyProp> _collection;

            public int Id { get; set; }

            public ReadOnlyProp(int id, ReadOnlyProp reference, IEnumerable<ReadOnlyProp> collection)
            {
                _foo = id;
                _reference = reference;
                _collection = collection;
            }

            public int Foo => _foo;
            public ReadOnlyProp Reference => _reference;
            public IEnumerable<ReadOnlyProp> Collection => _collection;
        }

        private class ReadOnlyAutoProp
        {
            public ReadOnlyAutoProp(int id, ReadOnlyAutoProp reference, IEnumerable<ReadOnlyAutoProp> collection)
            {
                Foo = id;
                Reference = reference;
                Collection = collection;
            }

            public int Id { get; set; }
            public int Foo { get; }
            public ReadOnlyAutoProp Reference { get; }
            public IEnumerable<ReadOnlyAutoProp> Collection { get; }
        }

        private class ReadOnlyFieldProp
        {
            private readonly int _foo;
            private readonly ReadOnlyFieldProp _reference;
            private readonly IEnumerable<ReadOnlyFieldProp> _collection;

            public ReadOnlyFieldProp(int id, ReadOnlyFieldProp reference, IEnumerable<ReadOnlyFieldProp> collection)
            {
                _foo = id;
                _reference = reference;
                _collection = collection;
            }

            public int Id { get; set; }
            public int Foo => _foo;
            public ReadOnlyFieldProp Reference => _reference;
            public IEnumerable<ReadOnlyFieldProp> Collection => _collection;
        }

        private class WriteOnlyProp
        {
            private int _foo;
            private WriteOnlyProp _reference;
            private IEnumerable<WriteOnlyProp> _collection;

            public int Id { get; set; }

            public int Foo
            {
                set => _foo = value;
            }

            public WriteOnlyProp Reference
            {
                set => _reference = value;
            }

            public IEnumerable<WriteOnlyProp> Collection
            {
                set => _collection = value;
            }
        }

        private class FieldOnly
        {
            private readonly int _foo;

            public FieldOnly(int id)
            {
                _foo = id;
            }

            public int Id { get; set; }
        }

        private class ReadOnlyFieldOnly
        {
            private readonly int _foo;

            public ReadOnlyFieldOnly(int id)
            {
                _foo = id;
            }

            public int Id { get; set; }
        }

        private class FullPropNoField
        {
            private int _notFound;
            private FullPropNoField _notFoundRef;
            private IEnumerable<FullPropNoField> _notFoundColl;

            public int Id { get; set; }

            public int Foo
            {
                get => _notFound;
                set => _notFound = value;
            }

            public FullPropNoField Reference
            {
                get => _notFoundRef;
                set => _notFoundRef = value;
            }

            public IEnumerable<FullPropNoField> Collection
            {
                get => _notFoundColl;
                set => _notFoundColl = value;
            }
        }

        private class ReadOnlyPropNoField
        {
            private readonly int _notFound;
            private readonly ReadOnlyPropNoField _notFoundRef;
            private readonly IEnumerable<ReadOnlyPropNoField> _notFoundColl;

            public ReadOnlyPropNoField(int id, ReadOnlyPropNoField notFoundRef, IEnumerable<ReadOnlyPropNoField> notFoundColl)
            {
                _notFound = id;
                _notFoundRef = notFoundRef;
                _notFoundColl = notFoundColl;
            }

            public int Id { get; set; }
            public int Foo => _notFound;
            public ReadOnlyPropNoField Reference => _notFoundRef;
            public IEnumerable<ReadOnlyPropNoField> Collection => _notFoundColl;
        }

        private class WriteOnlyPropNoField
        {
            private int _notFound;
            private WriteOnlyPropNoField _notFoundRef;
            private IEnumerable<WriteOnlyPropNoField> _notFoundColl;

            public int Id { get; set; }

            public int Foo
            {
                set => _notFound = value;
            }

            public WriteOnlyPropNoField Reference
            {
                set => _notFoundRef = value;
            }

            public IEnumerable<WriteOnlyPropNoField> Collection
            {
                set => _notFoundColl = value;
            }
        }

        private class PrivateSetterInBase : PrivateSetterBase
        {
            public override int Foo => _foo;
            public override PrivateSetterInBase Reference => _reference;
            public override IEnumerable<PrivateSetterInBase> Collection => _collection;
        }

        private class PrivateSetterBase
        {
            public int Id { get; set; }
            protected int _foo;
            protected PrivateSetterInBase _reference;
            protected IEnumerable<PrivateSetterInBase> _collection;

            public virtual int Foo
            {
                get => _foo;
                private set => _foo = value;
            }

            public virtual PrivateSetterInBase Reference
            {
                get => _reference;
                private set => _reference = value;
            }

            public virtual IEnumerable<PrivateSetterInBase> Collection
            {
                get => _collection;
                private set => _collection = value;
            }
        }

        private class PrivateGetterInBase : PrivateGetterBase
        {
            public override int Foo
            {
                set => _foo = value;
            }

            public override PrivateGetterInBase Reference
            {
                set => _reference = value;
            }

            public override IEnumerable<PrivateGetterInBase> Collection
            {
                set => _collection = value;
            }
        }

        private class PrivateGetterBase
        {
            public int Id { get; set; }
            protected int _foo;
            protected PrivateGetterInBase _reference;
            protected IEnumerable<PrivateGetterInBase> _collection;

            public virtual int Foo
            {
                private get => _foo;
                set => _foo = value;
            }

            public virtual PrivateGetterInBase Reference
            {
                private get => _reference;
                set => _reference = value;
            }

            public virtual IEnumerable<PrivateGetterInBase> Collection
            {
                private get => _collection;
                set => _collection = value;
            }
        }
    }
}

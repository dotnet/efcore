// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

// ReSharper disable InconsistentNaming
// ReSharper disable ClassNeverInstantiated.Local
// ReSharper disable MemberHidesStaticFromOuterClass
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable UnusedAutoPropertyAccessor.Local
// ReSharper disable FieldCanBeMadeReadOnly.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable ConvertToAutoProperty

namespace Microsoft.EntityFrameworkCore.Metadata.Internal;

public class PropertyBaseTest
{
    private const string Property = "Foo";
    private const string Reference = "Reference";
    private const string Collection = "Collection";
    private const string SkipCollection = "SkipCollection";

    [ConditionalFact]
    public void Get_MemberInfos_for_auto_props()
    {
        const string field = "<Foo>k__BackingField";

        MemberInfoTest(CreateProperty<AutoProp>(field), null, field, field, field);
        MemberInfoTest(CreateProperty<AutoProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(CreateProperty<AutoProp>(field), PropertyAccessMode.FieldDuringConstruction, field, Property, Property);
        MemberInfoTest(CreateProperty<AutoProp>(field), PropertyAccessMode.Property, Property, Property, Property);
        MemberInfoTest(CreateProperty<AutoProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(CreateProperty<AutoProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Property, Property);
        MemberInfoTest(CreateProperty<AutoProp>(field), PropertyAccessMode.PreferProperty, Property, Property, Property);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_props()
    {
        const string field = "_foo";

        MemberInfoTest(CreateProperty<FullProp>(field), null, field, field, field);
        MemberInfoTest(CreateProperty<FullProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(CreateProperty<FullProp>(field), PropertyAccessMode.FieldDuringConstruction, field, Property, Property);
        MemberInfoTest(CreateProperty<FullProp>(field), PropertyAccessMode.Property, Property, Property, Property);
        MemberInfoTest(CreateProperty<FullProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(CreateProperty<FullProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Property, Property);
        MemberInfoTest(CreateProperty<FullProp>(field), PropertyAccessMode.PreferProperty, Property, Property, Property);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_props()
    {
        const string field = "_foo";

        MemberInfoTest(CreateProperty<ReadOnlyProp>(field), null, field, field, field);
        MemberInfoTest(CreateProperty<ReadOnlyProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(CreateProperty<ReadOnlyProp>(field), PropertyAccessMode.FieldDuringConstruction, field, field, Property);
        MemberInfoTest(
            CreateProperty<ReadOnlyProp>(field), PropertyAccessMode.Property, NoSetter<ReadOnlyProp>(), NoSetter<ReadOnlyProp>(),
            Property);
        MemberInfoTest(CreateProperty<ReadOnlyProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(CreateProperty<ReadOnlyProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, field, Property);
        MemberInfoTest(CreateProperty<ReadOnlyProp>(field), PropertyAccessMode.PreferProperty, field, field, Property);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_auto_props()
    {
        const string field = "<Foo>k__BackingField";

        MemberInfoTest(CreateProperty<ReadOnlyAutoProp>(field), null, field, field, field);
        MemberInfoTest(CreateProperty<ReadOnlyAutoProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(CreateProperty<ReadOnlyAutoProp>(field), PropertyAccessMode.FieldDuringConstruction, field, field, Property);
        MemberInfoTest(
            CreateProperty<ReadOnlyAutoProp>(field), PropertyAccessMode.Property, NoSetter<ReadOnlyAutoProp>(),
            NoSetter<ReadOnlyAutoProp>(), Property);
        MemberInfoTest(CreateProperty<ReadOnlyAutoProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateProperty<ReadOnlyAutoProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, field, Property);
        MemberInfoTest(CreateProperty<ReadOnlyAutoProp>(field), PropertyAccessMode.PreferProperty, field, field, Property);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_field_props()
    {
        const string field = "_foo";

        MemberInfoTest(CreateProperty<ReadOnlyFieldProp>(field), null, field, field, field);
        MemberInfoTest(CreateProperty<ReadOnlyFieldProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(CreateProperty<ReadOnlyFieldProp>(field), PropertyAccessMode.FieldDuringConstruction, field, field, Property);
        MemberInfoTest(
            CreateProperty<ReadOnlyFieldProp>(field), PropertyAccessMode.Property, NoSetter<ReadOnlyFieldProp>(),
            NoSetter<ReadOnlyFieldProp>(), Property);
        MemberInfoTest(CreateProperty<ReadOnlyFieldProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateProperty<ReadOnlyFieldProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, field, Property);
        MemberInfoTest(CreateProperty<ReadOnlyFieldProp>(field), PropertyAccessMode.PreferProperty, field, field, Property);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_write_only_props()
    {
        const string field = "_foo";

        MemberInfoTest(CreateProperty<WriteOnlyProp>(field), null, field, field, field);
        MemberInfoTest(CreateProperty<WriteOnlyProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(CreateProperty<WriteOnlyProp>(field), PropertyAccessMode.FieldDuringConstruction, field, Property, field);
        MemberInfoTest(
            CreateProperty<WriteOnlyProp>(field), PropertyAccessMode.Property, Property, Property, NoGetter<WriteOnlyProp>());
        MemberInfoTest(CreateProperty<WriteOnlyProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(CreateProperty<WriteOnlyProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Property, field);
        MemberInfoTest(CreateProperty<WriteOnlyProp>(field), PropertyAccessMode.PreferProperty, Property, Property, field);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_field_only_props()
    {
        const string field = "_foo";

        MemberInfoTest(CreateProperty<FieldOnly>(field, field), null, field, field, field);
        MemberInfoTest(CreateProperty<FieldOnly>(field, field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(CreateProperty<FieldOnly>(field, field), PropertyAccessMode.FieldDuringConstruction, field, field, field);
        MemberInfoTest(
            CreateProperty<FieldOnly>(field, field), PropertyAccessMode.Property, NoProperty<FieldOnly>(field),
            NoProperty<FieldOnly>(field),
            NoProperty<FieldOnly>(field));
        MemberInfoTest(CreateProperty<FieldOnly>(field, field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(CreateProperty<FieldOnly>(field, field), PropertyAccessMode.PreferFieldDuringConstruction, field, field, field);
        MemberInfoTest(CreateProperty<FieldOnly>(field, field), PropertyAccessMode.PreferProperty, field, field, field);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_field_only_props()
    {
        const string field = "_foo";

        MemberInfoTest(CreateProperty<ReadOnlyFieldOnly>(field, field), null, field, field, field);
        MemberInfoTest(CreateProperty<ReadOnlyFieldOnly>(field, field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateProperty<ReadOnlyFieldOnly>(field, field), PropertyAccessMode.FieldDuringConstruction, field, field, field);
        MemberInfoTest(
            CreateProperty<ReadOnlyFieldOnly>(field, field), PropertyAccessMode.Property, NoProperty<ReadOnlyFieldOnly>(field),
            NoProperty<ReadOnlyFieldOnly>(field), NoProperty<ReadOnlyFieldOnly>(field));
        MemberInfoTest(CreateProperty<ReadOnlyFieldOnly>(field, field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateProperty<ReadOnlyFieldOnly>(field, field), PropertyAccessMode.PreferFieldDuringConstruction, field, field, field);
        MemberInfoTest(CreateProperty<ReadOnlyFieldOnly>(field, field), PropertyAccessMode.PreferProperty, field, field, field);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_props_with_field_not_found()
    {
        MemberInfoTest(CreateProperty<FullPropNoField>(null), null, Property, Property, Property);
        MemberInfoTest(
            CreateProperty<FullPropNoField>(null), PropertyAccessMode.Field, NoField<FullPropNoField>(),
            NoField<FullPropNoField>(), NoField<FullPropNoField>());
        MemberInfoTest(
            CreateProperty<FullPropNoField>(null), PropertyAccessMode.FieldDuringConstruction,
            NoField<FullPropNoField>(), Property, Property);
        MemberInfoTest(CreateProperty<FullPropNoField>(null), PropertyAccessMode.Property, Property, Property, Property);
        MemberInfoTest(CreateProperty<FullPropNoField>(null), PropertyAccessMode.PreferField, Property, Property, Property);
        MemberInfoTest(
            CreateProperty<FullPropNoField>(null), PropertyAccessMode.PreferFieldDuringConstruction, Property, Property, Property);
        MemberInfoTest(CreateProperty<FullPropNoField>(null), PropertyAccessMode.PreferProperty, Property, Property, Property);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_props_with_field_not_found()
    {
        MemberInfoTest(
            CreateProperty<ReadOnlyPropNoField>(null), null,
            NoFieldOrSetter<ReadOnlyPropNoField>(), NoFieldOrSetter<ReadOnlyPropNoField>(), Property);
        MemberInfoTest(
            CreateProperty<ReadOnlyPropNoField>(null), PropertyAccessMode.Field,
            NoField<ReadOnlyPropNoField>(), NoField<ReadOnlyPropNoField>(), NoField<ReadOnlyPropNoField>());
        MemberInfoTest(
            CreateProperty<ReadOnlyPropNoField>(null), PropertyAccessMode.FieldDuringConstruction,
            NoField<ReadOnlyPropNoField>(), NoFieldOrSetter<ReadOnlyPropNoField>(), Property);
        MemberInfoTest(
            CreateProperty<ReadOnlyPropNoField>(null), PropertyAccessMode.Property,
            NoSetter<ReadOnlyPropNoField>(), NoSetter<ReadOnlyPropNoField>(), Property);
        MemberInfoTest(
            CreateProperty<ReadOnlyPropNoField>(null), PropertyAccessMode.PreferField,
            NoFieldOrSetter<ReadOnlyPropNoField>(), NoFieldOrSetter<ReadOnlyPropNoField>(), Property);
        MemberInfoTest(
            CreateProperty<ReadOnlyPropNoField>(null), PropertyAccessMode.PreferFieldDuringConstruction,
            NoFieldOrSetter<ReadOnlyPropNoField>(), NoFieldOrSetter<ReadOnlyPropNoField>(), Property);
        MemberInfoTest(
            CreateProperty<ReadOnlyPropNoField>(null), PropertyAccessMode.PreferProperty,
            NoFieldOrSetter<ReadOnlyPropNoField>(), NoFieldOrSetter<ReadOnlyPropNoField>(), Property);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_write_only_props_with_field_not_found()
    {
        MemberInfoTest(CreateProperty<WriteOnlyPropNoField>(null), null, Property, Property, NoFieldOrGetter<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateProperty<WriteOnlyPropNoField>(null), PropertyAccessMode.Field,
            NoField<WriteOnlyPropNoField>(), NoField<WriteOnlyPropNoField>(), NoField<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateProperty<WriteOnlyPropNoField>(null), PropertyAccessMode.FieldDuringConstruction,
            NoField<WriteOnlyPropNoField>(), Property, NoFieldOrGetter<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateProperty<WriteOnlyPropNoField>(null), PropertyAccessMode.Property,
            Property, Property, NoGetter<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateProperty<WriteOnlyPropNoField>(null), PropertyAccessMode.PreferField,
            Property, Property, NoFieldOrGetter<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateProperty<WriteOnlyPropNoField>(null), PropertyAccessMode.PreferFieldDuringConstruction,
            Property, Property, NoFieldOrGetter<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateProperty<WriteOnlyPropNoField>(null), PropertyAccessMode.PreferProperty,
            Property, Property, NoFieldOrGetter<WriteOnlyPropNoField>());
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_props_private_setter_in_base()
    {
        const string field = "_foo";

        MemberInfoTest(CreateProperty<PrivateSetterInBase>(field), null, field, field, field);
        MemberInfoTest(CreateProperty<PrivateSetterInBase>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateProperty<PrivateSetterInBase>(field), PropertyAccessMode.FieldDuringConstruction, field, Property, Property);
        MemberInfoTest(CreateProperty<PrivateSetterInBase>(field), PropertyAccessMode.Property, Property, Property, Property);
        MemberInfoTest(CreateProperty<PrivateSetterInBase>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateProperty<PrivateSetterInBase>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Property, Property);
        MemberInfoTest(CreateProperty<PrivateSetterInBase>(field), PropertyAccessMode.PreferProperty, Property, Property, Property);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_props_private_getter_in_base()
    {
        const string field = "_foo";

        MemberInfoTest(CreateProperty<PrivateGetterInBase>(field), null, field, field, field);
        MemberInfoTest(CreateProperty<PrivateGetterInBase>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateProperty<PrivateGetterInBase>(field), PropertyAccessMode.FieldDuringConstruction, field, Property, Property);
        MemberInfoTest(CreateProperty<PrivateGetterInBase>(field), PropertyAccessMode.Property, Property, Property, Property);
        MemberInfoTest(CreateProperty<PrivateGetterInBase>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateProperty<PrivateGetterInBase>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Property, Property);
        MemberInfoTest(CreateProperty<PrivateGetterInBase>(field), PropertyAccessMode.PreferProperty, Property, Property, Property);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_auto_prop_navigations()
    {
        const string field = "<Reference>k__BackingField";

        MemberInfoTest(CreateReferenceNavigation<AutoProp>(field), null, field, field, field);
        MemberInfoTest(CreateReferenceNavigation<AutoProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<AutoProp>(field), PropertyAccessMode.FieldDuringConstruction, field, Reference, Reference);
        MemberInfoTest(CreateReferenceNavigation<AutoProp>(field), PropertyAccessMode.Property, Reference, Reference, Reference);
        MemberInfoTest(CreateReferenceNavigation<AutoProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<AutoProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Reference, Reference);
        MemberInfoTest(CreateReferenceNavigation<AutoProp>(field), PropertyAccessMode.PreferProperty, Reference, Reference, Reference);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_navigations()
    {
        const string field = "_reference";

        MemberInfoTest(CreateReferenceNavigation<FullProp>(field), null, field, field, field);
        MemberInfoTest(CreateReferenceNavigation<FullProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<FullProp>(field), PropertyAccessMode.FieldDuringConstruction, field, Reference, Reference);
        MemberInfoTest(CreateReferenceNavigation<FullProp>(field), PropertyAccessMode.Property, Reference, Reference, Reference);
        MemberInfoTest(CreateReferenceNavigation<FullProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<FullProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Reference, Reference);
        MemberInfoTest(CreateReferenceNavigation<FullProp>(field), PropertyAccessMode.PreferProperty, Reference, Reference, Reference);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_prop_navigations()
    {
        const string field = "_reference";

        MemberInfoTest(CreateReferenceNavigation<ReadOnlyProp>(field), null, field, field, field);
        MemberInfoTest(CreateReferenceNavigation<ReadOnlyProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyProp>(field), PropertyAccessMode.FieldDuringConstruction, field, field, Reference);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyProp>(field), PropertyAccessMode.Property, NoSetterRef<ReadOnlyProp>(),
            NoSetterRef<ReadOnlyProp>(), Reference);
        MemberInfoTest(CreateReferenceNavigation<ReadOnlyProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, field, Reference);
        MemberInfoTest(CreateReferenceNavigation<ReadOnlyProp>(field), PropertyAccessMode.PreferProperty, field, field, Reference);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_auto_prop_navigations()
    {
        const string field = "<Reference>k__BackingField";

        MemberInfoTest(CreateReferenceNavigation<ReadOnlyAutoProp>(field), null, field, field, field);
        MemberInfoTest(CreateReferenceNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.FieldDuringConstruction, field, field, Reference);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.Property, NoSetterRef<ReadOnlyAutoProp>(),
            NoSetterRef<ReadOnlyAutoProp>(), Reference);
        MemberInfoTest(CreateReferenceNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, field,
            Reference);
        MemberInfoTest(CreateReferenceNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.PreferProperty, field, field, Reference);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_field_prop_navigations()
    {
        const string field = "_reference";

        MemberInfoTest(CreateReferenceNavigation<ReadOnlyFieldProp>(field), null, field, field, field);
        MemberInfoTest(CreateReferenceNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.FieldDuringConstruction, field, field, Reference);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.Property, NoSetterRef<ReadOnlyFieldProp>(),
            NoSetterRef<ReadOnlyFieldProp>(), Reference);
        MemberInfoTest(CreateReferenceNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, field,
            Reference);
        MemberInfoTest(CreateReferenceNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.PreferProperty, field, field, Reference);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_write_only_prop_navigations()
    {
        const string field = "_reference";

        MemberInfoTest(CreateReferenceNavigation<WriteOnlyProp>(field), null, field, field, field);
        MemberInfoTest(CreateReferenceNavigation<WriteOnlyProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<WriteOnlyProp>(field), PropertyAccessMode.FieldDuringConstruction, field, Reference, field);
        MemberInfoTest(
            CreateReferenceNavigation<WriteOnlyProp>(field), PropertyAccessMode.Property, Reference, Reference,
            NoGetterRef<WriteOnlyProp>());
        MemberInfoTest(CreateReferenceNavigation<WriteOnlyProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<WriteOnlyProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Reference, field);
        MemberInfoTest(CreateReferenceNavigation<WriteOnlyProp>(field), PropertyAccessMode.PreferProperty, Reference, Reference, field);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_navigations_with_field_not_found()
    {
        MemberInfoTest(CreateReferenceNavigation<FullPropNoField>(null), null, Reference, Reference, Reference);
        MemberInfoTest(
            CreateReferenceNavigation<FullPropNoField>(null), PropertyAccessMode.Field, NoFieldRef<FullPropNoField>(),
            NoFieldRef<FullPropNoField>(),
            NoFieldRef<FullPropNoField>());
        MemberInfoTest(
            CreateReferenceNavigation<FullPropNoField>(null), PropertyAccessMode.FieldDuringConstruction, NoFieldRef<FullPropNoField>(),
            Reference, Reference);
        MemberInfoTest(CreateReferenceNavigation<FullPropNoField>(null), PropertyAccessMode.Property, Reference, Reference, Reference);
        MemberInfoTest(
            CreateReferenceNavigation<FullPropNoField>(null), PropertyAccessMode.PreferField, Reference, Reference, Reference);
        MemberInfoTest(
            CreateReferenceNavigation<FullPropNoField>(null), PropertyAccessMode.PreferFieldDuringConstruction, Reference, Reference,
            Reference);
        MemberInfoTest(
            CreateReferenceNavigation<FullPropNoField>(null), PropertyAccessMode.PreferProperty, Reference, Reference, Reference);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_prop_navigations_with_field_not_found()
    {
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyPropNoField>(null), null, NoFieldOrSetterRef<ReadOnlyPropNoField>(),
            NoFieldOrSetterRef<ReadOnlyPropNoField>(), Reference);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.Field, NoFieldRef<ReadOnlyPropNoField>(),
            NoFieldRef<ReadOnlyPropNoField>(),
            NoFieldRef<ReadOnlyPropNoField>());
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.FieldDuringConstruction,
            NoFieldRef<ReadOnlyPropNoField>(),
            NoFieldOrSetterRef<ReadOnlyPropNoField>(), Reference);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.Property, NoSetterRef<ReadOnlyPropNoField>(),
            NoSetterRef<ReadOnlyPropNoField>(), Reference);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.PreferField,
            NoFieldOrSetterRef<ReadOnlyPropNoField>(),
            NoFieldOrSetterRef<ReadOnlyPropNoField>(), Reference);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.PreferFieldDuringConstruction,
            NoFieldOrSetterRef<ReadOnlyPropNoField>(),
            NoFieldOrSetterRef<ReadOnlyPropNoField>(), Reference);
        MemberInfoTest(
            CreateReferenceNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.PreferProperty,
            NoFieldOrSetterRef<ReadOnlyPropNoField>(),
            NoFieldOrSetterRef<ReadOnlyPropNoField>(), Reference);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_write_only_prop_navigations_with_field_not_found()
    {
        MemberInfoTest(
            CreateReferenceNavigation<WriteOnlyPropNoField>(null), null,
            Reference, Reference, NoFieldOrGetterRef<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateReferenceNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.Field,
            NoFieldRef<WriteOnlyPropNoField>(), NoFieldRef<WriteOnlyPropNoField>(), NoFieldRef<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateReferenceNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.FieldDuringConstruction,
            NoFieldRef<WriteOnlyPropNoField>(), Reference, NoFieldOrGetterRef<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateReferenceNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.Property,
            Reference, Reference, NoGetterRef<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateReferenceNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.PreferField,
            Reference, Reference, NoFieldOrGetterRef<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateReferenceNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.PreferFieldDuringConstruction,
            Reference, Reference, NoFieldOrGetterRef<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateReferenceNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.PreferProperty,
            Reference, Reference, NoFieldOrGetterRef<WriteOnlyPropNoField>());
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_navigations_private_setter_in_base()
    {
        const string field = "_reference";

        MemberInfoTest(CreateReferenceNavigation<PrivateSetterInBase>(field), null, field, field, field);
        MemberInfoTest(CreateReferenceNavigation<PrivateSetterInBase>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<PrivateSetterInBase>(field), PropertyAccessMode.FieldDuringConstruction, field, Reference,
            Reference);
        MemberInfoTest(
            CreateReferenceNavigation<PrivateSetterInBase>(field), PropertyAccessMode.Property, Reference, Reference, Reference);
        MemberInfoTest(CreateReferenceNavigation<PrivateSetterInBase>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<PrivateSetterInBase>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Reference,
            Reference);
        MemberInfoTest(
            CreateReferenceNavigation<PrivateSetterInBase>(field), PropertyAccessMode.PreferProperty, Reference, Reference, Reference);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_navigations_private_getter_in_base()
    {
        const string field = "_reference";

        MemberInfoTest(CreateReferenceNavigation<PrivateGetterInBase>(field), null, field, field, field);
        MemberInfoTest(CreateReferenceNavigation<PrivateGetterInBase>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<PrivateGetterInBase>(field), PropertyAccessMode.FieldDuringConstruction, field,
            Reference, Reference);
        MemberInfoTest(
            CreateReferenceNavigation<PrivateGetterInBase>(field), PropertyAccessMode.Property, Reference, Reference, Reference);
        MemberInfoTest(CreateReferenceNavigation<PrivateGetterInBase>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateReferenceNavigation<PrivateGetterInBase>(field), PropertyAccessMode.PreferFieldDuringConstruction, field,
            Reference, Reference);
        MemberInfoTest(
            CreateReferenceNavigation<PrivateGetterInBase>(field), PropertyAccessMode.PreferProperty, Reference, Reference, Reference);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_auto_prop_collection_navigations()
    {
        const string field = "<Collection>k__BackingField";

        MemberInfoTest(CreateCollectionNavigation<AutoProp>(field), null, field, field, field);
        MemberInfoTest(CreateCollectionNavigation<AutoProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<AutoProp>(field), PropertyAccessMode.FieldDuringConstruction, field, Collection, Collection);
        MemberInfoTest(CreateCollectionNavigation<AutoProp>(field), PropertyAccessMode.Property, Collection, Collection, Collection);
        MemberInfoTest(CreateCollectionNavigation<AutoProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<AutoProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Collection,
            Collection);
        MemberInfoTest(
            CreateCollectionNavigation<AutoProp>(field), PropertyAccessMode.PreferProperty, Collection, Collection, Collection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_collection_navigations()
    {
        const string field = "_collection";

        MemberInfoTest(CreateCollectionNavigation<FullProp>(field), null, field, field, field);
        MemberInfoTest(CreateCollectionNavigation<FullProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<FullProp>(field), PropertyAccessMode.FieldDuringConstruction, field, Collection, Collection);
        MemberInfoTest(CreateCollectionNavigation<FullProp>(field), PropertyAccessMode.Property, Collection, Collection, Collection);
        MemberInfoTest(CreateCollectionNavigation<FullProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<FullProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Collection,
            Collection);
        MemberInfoTest(
            CreateCollectionNavigation<FullProp>(field), PropertyAccessMode.PreferProperty, Collection, Collection, Collection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_prop_collection_navigations()
    {
        const string field = "_collection";

        MemberInfoTest(CreateCollectionNavigation<ReadOnlyProp>(field), null, field, field, field);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyProp>(field), PropertyAccessMode.FieldDuringConstruction, field, field, Collection);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyProp>(field), PropertyAccessMode.Property, null, null, Collection);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, field,
            Collection);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyProp>(field), PropertyAccessMode.PreferProperty, field, field, Collection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_auto_prop_collection_navigations()
    {
        const string field = "<Collection>k__BackingField";

        MemberInfoTest(CreateCollectionNavigation<ReadOnlyAutoProp>(field), null, field, field, field);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.FieldDuringConstruction, field, field, Collection);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.Property, null, null, Collection);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.PreferFieldDuringConstruction,
            field, field, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyAutoProp>(field), PropertyAccessMode.PreferProperty, field, field, Collection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_field_prop_collection_navigations()
    {
        const string field = "_collection";

        MemberInfoTest(CreateCollectionNavigation<ReadOnlyFieldProp>(field), null, field, field, field);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.FieldDuringConstruction, field, field, Collection);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.Property, null, null, Collection);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.PreferFieldDuringConstruction,
            field, field, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyFieldProp>(field), PropertyAccessMode.PreferProperty, field, field, Collection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_write_only_prop_collection_navigations()
    {
        const string field = "_collection";

        MemberInfoTest(CreateCollectionNavigation<WriteOnlyProp>(field), null, field, field, field);
        MemberInfoTest(CreateCollectionNavigation<WriteOnlyProp>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<WriteOnlyProp>(field), PropertyAccessMode.FieldDuringConstruction, field, Collection, field);
        MemberInfoTest(
            CreateCollectionNavigation<WriteOnlyProp>(field), PropertyAccessMode.Property,
            Collection, Collection, NoGetterColl<WriteOnlyProp>());
        MemberInfoTest(CreateCollectionNavigation<WriteOnlyProp>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<WriteOnlyProp>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, Collection,
            field);
        MemberInfoTest(
            CreateCollectionNavigation<WriteOnlyProp>(field), PropertyAccessMode.PreferProperty, Collection, Collection, field);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_collection_navigations_with_field_not_found()
    {
        MemberInfoTest(CreateCollectionNavigation<FullPropNoField>(null), null, Collection, Collection, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<FullPropNoField>(null), PropertyAccessMode.Field, null, null, NoFieldColl<FullPropNoField>());
        MemberInfoTest(
            CreateCollectionNavigation<FullPropNoField>(null), PropertyAccessMode.FieldDuringConstruction, null, Collection,
            Collection);
        MemberInfoTest(
            CreateCollectionNavigation<FullPropNoField>(null), PropertyAccessMode.Property, Collection, Collection, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<FullPropNoField>(null), PropertyAccessMode.PreferField, Collection, Collection, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<FullPropNoField>(null), PropertyAccessMode.PreferFieldDuringConstruction,
            Collection, Collection, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<FullPropNoField>(null), PropertyAccessMode.PreferProperty, Collection, Collection, Collection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_prop_collection_navigations_with_field_not_found()
    {
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyPropNoField>(null), null, null, null, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.Field,
            null, null, NoFieldColl<ReadOnlyPropNoField>());
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.FieldDuringConstruction, null, null, Collection);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.Property, null, null, Collection);
        MemberInfoTest(CreateCollectionNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.PreferField, null, null, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.PreferFieldDuringConstruction,
            null, null, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<ReadOnlyPropNoField>(null), PropertyAccessMode.PreferProperty, null, null, Collection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_write_only_prop_collection_navigations_with_field_not_found()
    {
        MemberInfoTest(
            CreateCollectionNavigation<WriteOnlyPropNoField>(null), null,
            Collection, Collection, NoFieldOrGetterColl<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateCollectionNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.Field,
            null, null, NoFieldColl<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateCollectionNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.FieldDuringConstruction,
            null, Collection, NoFieldOrGetterColl<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateCollectionNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.Property,
            Collection, Collection, NoGetterColl<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateCollectionNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.PreferField,
            Collection, Collection, NoFieldOrGetterColl<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateCollectionNavigation<WriteOnlyPropNoField>(null), PropertyAccessMode.PreferProperty,
            Collection, Collection, NoFieldOrGetterColl<WriteOnlyPropNoField>());
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_collection_navigations_private_setter_in_base()
    {
        const string field = "_collection";

        MemberInfoTest(CreateCollectionNavigation<PrivateSetterInBase>(field), null, field, field, field);
        MemberInfoTest(CreateCollectionNavigation<PrivateSetterInBase>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<PrivateSetterInBase>(field), PropertyAccessMode.FieldDuringConstruction, field,
            Collection, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<PrivateSetterInBase>(field), PropertyAccessMode.Property, Collection, Collection, Collection);
        MemberInfoTest(CreateCollectionNavigation<PrivateSetterInBase>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<PrivateSetterInBase>(field), PropertyAccessMode.PreferProperty, Collection, Collection,
            Collection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_collection_navigations_private_getter_in_base()
    {
        const string field = "_collection";

        MemberInfoTest(CreateCollectionNavigation<PrivateGetterInBase>(field), null, field, field, field);
        MemberInfoTest(CreateCollectionNavigation<PrivateGetterInBase>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<PrivateGetterInBase>(field), PropertyAccessMode.FieldDuringConstruction,
            field, Collection, Collection);
        MemberInfoTest(
            CreateCollectionNavigation<PrivateGetterInBase>(field), PropertyAccessMode.Property, Collection, Collection, Collection);
        MemberInfoTest(CreateCollectionNavigation<PrivateGetterInBase>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateCollectionNavigation<PrivateGetterInBase>(field), PropertyAccessMode.PreferProperty, Collection, Collection,
            Collection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_auto_prop_skip_collection_navigations()
    {
        const string field = "<SkipCollection>k__BackingField";

        MemberInfoTest(CreateSkipCollectionNavigation<AutoProp, AutoPropOther>(field), null, field, field, field);
        MemberInfoTest(CreateSkipCollectionNavigation<AutoProp, AutoPropOther>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<AutoProp, AutoPropOther>(field), PropertyAccessMode.FieldDuringConstruction, field, SkipCollection, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<AutoProp, AutoPropOther>(field), PropertyAccessMode.Property, SkipCollection, SkipCollection, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<AutoProp, AutoPropOther>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<AutoProp, AutoPropOther>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, SkipCollection,
            SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<AutoProp, AutoPropOther>(field), PropertyAccessMode.PreferProperty, SkipCollection, SkipCollection, SkipCollection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_skip_collection_navigations()
    {
        const string field = "_skipCollection";

        MemberInfoTest(CreateSkipCollectionNavigation<FullProp, FullPropOther>(field), null, field, field, field);
        MemberInfoTest(CreateSkipCollectionNavigation<FullProp, FullPropOther>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<FullProp, FullPropOther>(field), PropertyAccessMode.FieldDuringConstruction, field, SkipCollection, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<FullProp, FullPropOther>(field), PropertyAccessMode.Property, SkipCollection, SkipCollection, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<FullProp, FullPropOther>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<FullProp, FullPropOther>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, SkipCollection,
            SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<FullProp, FullPropOther>(field), PropertyAccessMode.PreferProperty, SkipCollection, SkipCollection, SkipCollection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_prop_skip_collection_navigations()
    {
        const string field = "_skipCollection";

        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyProp, ReadOnlyPropOther>(field), null, field, field, field);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyProp, ReadOnlyPropOther>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyProp, ReadOnlyPropOther>(field), PropertyAccessMode.FieldDuringConstruction, field, field, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyProp, ReadOnlyPropOther>(field), PropertyAccessMode.Property, null, null, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyProp, ReadOnlyPropOther>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyProp, ReadOnlyPropOther>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, field,
            SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyProp, ReadOnlyPropOther>(field), PropertyAccessMode.PreferProperty, field, field, SkipCollection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_auto_prop_skip_collection_navigations()
    {
        const string field = "<SkipCollection>k__BackingField";

        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyAutoProp, ReadOnlyAutoPropOther>(field), null, field, field, field);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyAutoProp, ReadOnlyAutoPropOther>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyAutoProp, ReadOnlyAutoPropOther>(field), PropertyAccessMode.FieldDuringConstruction, field, field, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyAutoProp, ReadOnlyAutoPropOther>(field), PropertyAccessMode.Property, null, null, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyAutoProp, ReadOnlyAutoPropOther>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyAutoProp, ReadOnlyAutoPropOther>(field), PropertyAccessMode.PreferFieldDuringConstruction,
            field, field, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyAutoProp, ReadOnlyAutoPropOther>(field), PropertyAccessMode.PreferProperty, field, field, SkipCollection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_field_prop_skip_collection_navigations()
    {
        const string field = "_skipCollection";

        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyFieldProp, ReadOnlyFieldPropOther>(field), null, field, field, field);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyFieldProp, ReadOnlyFieldPropOther>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyFieldProp, ReadOnlyFieldPropOther>(field), PropertyAccessMode.FieldDuringConstruction, field, field, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyFieldProp, ReadOnlyFieldPropOther>(field), PropertyAccessMode.Property, null, null, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyFieldProp, ReadOnlyFieldPropOther>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyFieldProp, ReadOnlyFieldPropOther>(field), PropertyAccessMode.PreferFieldDuringConstruction,
            field, field, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyFieldProp, ReadOnlyFieldPropOther>(field), PropertyAccessMode.PreferProperty, field, field, SkipCollection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_write_only_prop_skip_collection_navigations()
    {
        const string field = "_skipCollection";

        MemberInfoTest(CreateSkipCollectionNavigation<WriteOnlyProp, WriteOnlyPropOther>(field), null, field, field, field);
        MemberInfoTest(CreateSkipCollectionNavigation<WriteOnlyProp, WriteOnlyPropOther>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<WriteOnlyProp, WriteOnlyPropOther>(field), PropertyAccessMode.FieldDuringConstruction, field, SkipCollection, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<WriteOnlyProp, WriteOnlyPropOther>(field), PropertyAccessMode.Property,
            SkipCollection, SkipCollection, NoGetterSkipColl<WriteOnlyProp>());
        MemberInfoTest(CreateSkipCollectionNavigation<WriteOnlyProp, WriteOnlyPropOther>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<WriteOnlyProp, WriteOnlyPropOther>(field), PropertyAccessMode.PreferFieldDuringConstruction, field, SkipCollection,
            field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<WriteOnlyProp, WriteOnlyPropOther>(field), PropertyAccessMode.PreferProperty, SkipCollection, SkipCollection, field);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_skip_collection_navigations_with_field_not_found()
    {
        MemberInfoTest(CreateSkipCollectionNavigation<FullPropNoField, FullPropNoFieldOther>(null), null, SkipCollection, SkipCollection, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<FullPropNoField, FullPropNoFieldOther>(null), PropertyAccessMode.Field, null, null, NoFieldSkipColl<FullPropNoField>());
        MemberInfoTest(
            CreateSkipCollectionNavigation<FullPropNoField, FullPropNoFieldOther>(null), PropertyAccessMode.FieldDuringConstruction, null, SkipCollection,
            SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<FullPropNoField, FullPropNoFieldOther>(null), PropertyAccessMode.Property, SkipCollection, SkipCollection, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<FullPropNoField, FullPropNoFieldOther>(null), PropertyAccessMode.PreferField, SkipCollection, SkipCollection, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<FullPropNoField, FullPropNoFieldOther>(null), PropertyAccessMode.PreferFieldDuringConstruction,
            SkipCollection, SkipCollection, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<FullPropNoField, FullPropNoFieldOther>(null), PropertyAccessMode.PreferProperty, SkipCollection, SkipCollection, SkipCollection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_read_only_prop_skip_collection_navigations_with_field_not_found()
    {
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyPropNoField, ReadOnlyPropNoFieldOther>(null), null, null, null, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyPropNoField, ReadOnlyPropNoFieldOther>(null), PropertyAccessMode.Field,
            null, null, NoFieldSkipColl<ReadOnlyPropNoField>());
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyPropNoField, ReadOnlyPropNoFieldOther>(null), PropertyAccessMode.FieldDuringConstruction, null, null, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyPropNoField, ReadOnlyPropNoFieldOther>(null), PropertyAccessMode.Property, null, null, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<ReadOnlyPropNoField, ReadOnlyPropNoFieldOther>(null), PropertyAccessMode.PreferField, null, null, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyPropNoField, ReadOnlyPropNoFieldOther>(null), PropertyAccessMode.PreferFieldDuringConstruction,
            null, null, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<ReadOnlyPropNoField, ReadOnlyPropNoFieldOther>(null), PropertyAccessMode.PreferProperty, null, null, SkipCollection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_write_only_prop_skip_collection_navigations_with_field_not_found()
    {
        MemberInfoTest(
            CreateSkipCollectionNavigation<WriteOnlyPropNoField, WriteOnlyPropNoFieldOther>(null), null,
            SkipCollection, SkipCollection, NoFieldOrGetterSkipColl<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateSkipCollectionNavigation<WriteOnlyPropNoField, WriteOnlyPropNoFieldOther>(null), PropertyAccessMode.Field,
            null, null, NoFieldSkipColl<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateSkipCollectionNavigation<WriteOnlyPropNoField, WriteOnlyPropNoFieldOther>(null), PropertyAccessMode.FieldDuringConstruction,
            null, SkipCollection, NoFieldOrGetterSkipColl<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateSkipCollectionNavigation<WriteOnlyPropNoField, WriteOnlyPropNoFieldOther>(null), PropertyAccessMode.Property,
            SkipCollection, SkipCollection, NoGetterSkipColl<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateSkipCollectionNavigation<WriteOnlyPropNoField, WriteOnlyPropNoFieldOther>(null), PropertyAccessMode.PreferField,
            SkipCollection, SkipCollection, NoFieldOrGetterSkipColl<WriteOnlyPropNoField>());
        MemberInfoTest(
            CreateSkipCollectionNavigation<WriteOnlyPropNoField, WriteOnlyPropNoFieldOther>(null), PropertyAccessMode.PreferProperty,
            SkipCollection, SkipCollection, NoFieldOrGetterSkipColl<WriteOnlyPropNoField>());
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_skip_collection_navigations_private_setter_in_base()
    {
        const string field = "_skipCollection";

        MemberInfoTest(CreateSkipCollectionNavigation<PrivateSetterInBase, PrivateSetterBaseOther>(field), null, field, field, field);
        MemberInfoTest(CreateSkipCollectionNavigation<PrivateSetterInBase, PrivateSetterBaseOther>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<PrivateSetterInBase, PrivateSetterBaseOther>(field), PropertyAccessMode.FieldDuringConstruction, field,
            SkipCollection, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<PrivateSetterInBase, PrivateSetterBaseOther>(field), PropertyAccessMode.Property, SkipCollection, SkipCollection, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<PrivateSetterInBase, PrivateSetterBaseOther>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<PrivateSetterInBase, PrivateSetterBaseOther>(field), PropertyAccessMode.PreferProperty, SkipCollection, SkipCollection,
            SkipCollection);
    }

    [ConditionalFact]
    public void Get_MemberInfos_for_full_prop_skip_collection_navigations_private_getter_in_base()
    {
        const string field = "_skipCollection";

        MemberInfoTest(CreateSkipCollectionNavigation<PrivateGetterInBase, PrivateGetterBaseOther>(field), null, field, field, field);
        MemberInfoTest(CreateSkipCollectionNavigation<PrivateGetterInBase, PrivateGetterBaseOther>(field), PropertyAccessMode.Field, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<PrivateGetterInBase, PrivateGetterBaseOther>(field), PropertyAccessMode.FieldDuringConstruction,
            field, SkipCollection, SkipCollection);
        MemberInfoTest(
            CreateSkipCollectionNavigation<PrivateGetterInBase, PrivateGetterBaseOther>(field), PropertyAccessMode.Property, SkipCollection, SkipCollection, SkipCollection);
        MemberInfoTest(CreateSkipCollectionNavigation<PrivateGetterInBase, PrivateGetterBaseOther>(field), PropertyAccessMode.PreferField, field, field, field);
        MemberInfoTest(
            CreateSkipCollectionNavigation<PrivateGetterInBase, PrivateGetterBaseOther>(field), PropertyAccessMode.PreferProperty, SkipCollection, SkipCollection,
            SkipCollection);
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

    private static string NoFieldSkipColl<TEntity>()
        => CoreStrings.NoBackingField(SkipCollection, typeof(TEntity).Name, nameof(PropertyAccessMode));

    private static string NoFieldOrGetterSkipColl<TEntity>()
        => CoreStrings.NoFieldOrGetter(SkipCollection, typeof(TEntity).Name);

    private static string NoGetterSkipColl<TEntity>()
        => CoreStrings.NoGetter(SkipCollection, typeof(TEntity).Name, nameof(PropertyAccessMode));

    private static IMutableProperty CreateProperty<TEntity>(string fieldName, string propertyName = Property)
        where TEntity : class
    {
        var model = CreateModelBuilder();
        var property = model.Entity<TEntity>()
            .Ignore("Reference")
            .Ignore("Collection")
            .Ignore("SkipCollection")
            .Property<int>(propertyName)
            .Metadata;

        property.SetField(fieldName);
        Assert.False(property.IsShadowProperty());

        return property;
    }

    private static IMutableNavigation CreateReferenceNavigation<TEntity>(
        string fieldName,
        string navigationName = Reference)
        where TEntity : class
    {
        var model = CreateModelBuilder();
        var relationship = model.Entity<TEntity>()
            .Ignore("Foo")
            .Ignore("Collection")
            .Ignore("SkipCollection")
            .HasOne(typeof(TEntity), navigationName)
            .WithMany();

        var navigation = relationship.Metadata.DependentToPrincipal;
        navigation.SetField(fieldName);

        return navigation;
    }

    private static IMutableNavigation CreateCollectionNavigation<TEntity>(
        string fieldName,
        string navigationName = Collection)
        where TEntity : class
    {
        var model = CreateModelBuilder();
        var relationship = model.Entity<TEntity>()
            .Ignore("Foo")
            .Ignore("Reference")
            .Ignore("SkipCollection")
            .HasMany(typeof(TEntity), navigationName)
            .WithOne();

        var navigation = relationship.Metadata.PrincipalToDependent;
        navigation.SetField(fieldName);

        return navigation;
    }

    private static IMutableSkipNavigation CreateSkipCollectionNavigation<TEntity, TOtherEntity>(
        string fieldName,
        string navigationName = SkipCollection)
        where TEntity : class
        where TOtherEntity : class
    {
        var model = CreateModelBuilder();
        var relationship = model.Entity<TEntity>()
            .Ignore("Foo")
            .Ignore("Reference")
            .Ignore("Collection")
            .HasMany(typeof(TOtherEntity), navigationName)
            .WithMany();

        var navigation = model.Entity<TEntity>().Navigation(navigationName);
        navigation.HasField(fieldName);

        return (IMutableSkipNavigation)navigation.Metadata;
    }

    private static ModelBuilder CreateModelBuilder()
        => InMemoryTestHelpers.Instance.CreateConventionBuilder();

    private void MemberInfoTest(
        IMutableProperty property,
        PropertyAccessMode? accessMode,
        string forConstruction,
        string forSet,
        string forGet)
    {
        property.SetPropertyAccessMode(accessMode);

        MemberInfoTestCommon((IPropertyBase)property, accessMode, forConstruction, forSet, forGet);
    }

    private void MemberInfoTest(
        IMutableNavigationBase navigation,
        PropertyAccessMode? accessMode,
        string forConstruction,
        string forSet,
        string forGet)
    {
        navigation.SetPropertyAccessMode(accessMode);

        MemberInfoTestCommon((IPropertyBase)navigation, accessMode, forConstruction, forSet, forGet);
    }

    private void MemberInfoTestCommon(
        IPropertyBase propertyBase,
        PropertyAccessMode? accessMode,
        string forConstruction,
        string forSet,
        string forGet)
    {
        string failMessage = null;
        try
        {
            var memberInfo = propertyBase.GetMemberInfo(forMaterialization: true, forSet: true);
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
            var memberInfo = propertyBase.GetMemberInfo(forMaterialization: false, forSet: true);
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
            failMessage ??= ex.Message;
        }

        try
        {
            var memberInfo = propertyBase.GetMemberInfo(forMaterialization: false, forSet: false);
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
            failMessage ??= ex.Message;
        }

        try
        {
            var model = propertyBase.DeclaringType.Model;
            var contextServices = InMemoryTestHelpers.Instance.CreateContextServices();
            var modelRuntimeInitializer = contextServices.GetRequiredService<IModelRuntimeInitializer>();

            model = modelRuntimeInitializer.Initialize(
                model, designTime: false, new TestLogger<DbLoggerCategory.Model.Validation, TestLoggingDefinitions>());

            Assert.Null(failMessage);
        }
        catch (InvalidOperationException ex)
        {
            Assert.Equal(failMessage, ex.Message);
        }
    }

    [ConditionalFact]
    public virtual void Access_mode_can_be_overridden_at_entity_and_property_levels()
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

    [ConditionalFact]
    public virtual void Properties_can_have_field_cleared()
    {
        var propertyInfo = typeof(FullProp).GetAnyProperty("Foo");

        Properties_can_have_field_cleared_test(
            ((IMutableModel)new Model()).AddEntityType(typeof(FullProp)).AddProperty(propertyInfo), propertyInfo, "_foo");
    }

    [ConditionalFact]
    public virtual void Field_only_properties_throws_when_field_cleared()
    {
        var propertyBase = ((IMutableModel)new Model()).AddEntityType(typeof(FieldOnly)).AddProperty("_foo", typeof(int));

        Assert.Equal(
            CoreStrings.FieldNameMismatch(null, nameof(FieldOnly), "_foo"),
            Assert.Throws<InvalidOperationException>(() => propertyBase.SetField(null)).Message);
    }

    [ConditionalFact]
    public virtual void Navigations_can_have_field_cleared()
    {
        var entityType = ((IMutableModel)new Model()).AddEntityType(typeof(FullProp));
        var property = entityType.AddProperty("Id", typeof(int));
        var key = entityType.SetPrimaryKey(property);
        var foreignKey = entityType.AddForeignKey(property, key, entityType);

        var propertyInfo = typeof(FullProp).GetAnyProperty("Reference");

        Properties_can_have_field_cleared_test(
            foreignKey.SetDependentToPrincipal(propertyInfo), propertyInfo, "_reference");
    }

    private void Properties_can_have_field_cleared_test(IMutablePropertyBase propertyBase, PropertyInfo propertyInfo, string fieldName)
    {
        Assert.Null(propertyBase.GetFieldName());
        Assert.Null(propertyBase.FieldInfo);
        Assert.Same(propertyInfo, propertyBase.GetIdentifyingMemberInfo());

        propertyBase.SetField(fieldName);

        Assert.Equal(fieldName, propertyBase.GetFieldName());
        var fieldInfo = propertyBase.FieldInfo;
        Assert.Equal(fieldName, fieldInfo.Name);
        Assert.Same(propertyInfo ?? (MemberInfo)fieldInfo, propertyBase.GetIdentifyingMemberInfo());

        propertyBase.SetField(null);

        Assert.Null(propertyBase.GetFieldName());
        Assert.Null(propertyBase.FieldInfo);
        Assert.Same(propertyInfo, propertyBase.GetIdentifyingMemberInfo());

        propertyBase.FieldInfo = fieldInfo;

        Assert.Equal(fieldName, propertyBase.GetFieldName());
        Assert.Same(propertyInfo ?? (MemberInfo)fieldInfo, propertyBase.GetIdentifyingMemberInfo());

        propertyBase.FieldInfo = null;

        Assert.Null(propertyBase.GetFieldName());
        Assert.Null(propertyBase.FieldInfo);
        Assert.Same(propertyInfo, propertyBase.GetIdentifyingMemberInfo());
    }

    [ConditionalFact]
    public virtual void Setting_fieldInfo_for_shadow_property_throws()
    {
        IMutableModel model = new Model();

        var entityType = model.AddEntityType(typeof(FullProp));
        var property = entityType.AddProperty("shadow", typeof(int));

        Assert.Equal(
            CoreStrings.FieldNameMismatch("_foo", nameof(FullProp), "shadow"),
            Assert.Throws<InvalidOperationException>(() => property.SetField("_foo")).Message);
    }

    private class AutoProp
    {
        public int Id { get; set; }
        public int Foo { get; set; }
        public AutoProp Reference { get; set; }
        public IEnumerable<AutoProp> Collection { get; set; }
        public IEnumerable<AutoPropOther> SkipCollection { get; set; }
    }

    private class AutoPropOther
    {
        public int Id { get; set; }
    }

    private class FullProp
    {
        private int _foo;
        private FullProp _reference;
        private IEnumerable<FullProp> _collection;
        private IEnumerable<FullPropOther> _skipCollection;

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

        public IEnumerable<FullPropOther> SkipCollection
        {
            get => _skipCollection;
            set => _skipCollection = value;
        }
    }

    private class FullPropOther
    {
        public int Id { get; set; }
    }

    private class ReadOnlyProp
    {
        private readonly int _foo;
        private readonly ReadOnlyProp _reference;
        private readonly IEnumerable<ReadOnlyProp> _collection;
        private readonly IEnumerable<ReadOnlyPropOther> _skipCollection;

        public int Id { get; set; }

        public ReadOnlyProp()
        {
        }

        public ReadOnlyProp(
            int id,
            ReadOnlyProp reference,
            IEnumerable<ReadOnlyProp> collection,
            IEnumerable<ReadOnlyPropOther> skipCollection)
        {
            _foo = id;
            _reference = reference;
            _collection = collection;
            _skipCollection = skipCollection;
        }

        public int Foo
            => _foo;

        public ReadOnlyProp Reference
            => _reference;

        public IEnumerable<ReadOnlyProp> Collection
            => _collection;

        public IEnumerable<ReadOnlyPropOther> SkipCollection
            => _skipCollection;
    }

    private class ReadOnlyPropOther
    {
        public int Id { get; set; }
    }

    private class ReadOnlyAutoProp
    {
        public ReadOnlyAutoProp()
        {
        }

        public ReadOnlyAutoProp(
            int id,
            ReadOnlyAutoProp reference,
            IEnumerable<ReadOnlyAutoProp> collection,
            IEnumerable<ReadOnlyAutoPropOther> skipCollection)
        {
            Foo = id;
            Reference = reference;
            Collection = collection;
            SkipCollection = skipCollection;
        }

        public int Id { get; set; }
        public int Foo { get; }
        public ReadOnlyAutoProp Reference { get; }
        public IEnumerable<ReadOnlyAutoProp> Collection { get; }
        public IEnumerable<ReadOnlyAutoPropOther> SkipCollection { get; }
    }

    private class ReadOnlyAutoPropOther
    {
        public int Id { get; set; }
    }

    private class ReadOnlyFieldProp
    {
        private readonly int _foo;
        private readonly ReadOnlyFieldProp _reference;
        private readonly IEnumerable<ReadOnlyFieldProp> _collection;
        private readonly IEnumerable<ReadOnlyFieldPropOther> _skipCollection;

        public ReadOnlyFieldProp()
        {
        }

        public ReadOnlyFieldProp(
            int id,
            ReadOnlyFieldProp reference,
            IEnumerable<ReadOnlyFieldProp> collection,
            IEnumerable<ReadOnlyFieldPropOther> skipCollection)
        {
            _foo = id;
            _reference = reference;
            _collection = collection;
            _skipCollection = skipCollection;
        }

        public int Id { get; set; }

        public int Foo
            => _foo;

        public ReadOnlyFieldProp Reference
            => _reference;

        public IEnumerable<ReadOnlyFieldProp> Collection
            => _collection;

        public IEnumerable<ReadOnlyFieldPropOther> SkipCollection
            => _skipCollection;
    }

    private class ReadOnlyFieldPropOther
    {
        public int Id { get; set; }
    }

    private class WriteOnlyProp
    {
        private int _foo;
        private WriteOnlyProp _reference;
        private IEnumerable<WriteOnlyProp> _collection;
        private IEnumerable<WriteOnlyPropOther> _skipCollection;

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

        public IEnumerable<WriteOnlyPropOther> SkipCollection
        {
            set => _skipCollection = value;
        }
    }

    private class WriteOnlyPropOther
    {
        public int Id { get; set; }
    }

    private class FieldOnly(int id)
    {
        private readonly int _foo = id;

        public int Id { get; set; }
    }

    private class ReadOnlyFieldOnly(int id)
    {
        private readonly int _foo = id;

        public int Id { get; set; }
    }

    private class FullPropNoField
    {
        private int _notFound;
        private FullPropNoField _notFoundRef;
        private IEnumerable<FullPropNoField> _notFoundColl;
        private IEnumerable<FullPropNoFieldOther> _notFoundSkipColl;

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

        public IEnumerable<FullPropNoFieldOther> SkipCollection
        {
            get => _notFoundSkipColl;
            set => _notFoundSkipColl = value;
        }
    }

    private class FullPropNoFieldOther
    {
        public int Id { get; set; }
    }

    private class ReadOnlyPropNoField
    {
        private readonly int _notFound;
        private readonly ReadOnlyPropNoField _notFoundRef;
        private readonly IEnumerable<ReadOnlyPropNoField> _notFoundColl;
        private readonly IEnumerable<ReadOnlyPropNoFieldOther> _notFoundSkipColl;

        public ReadOnlyPropNoField()
        {
        }

        public ReadOnlyPropNoField(
            int id,
            ReadOnlyPropNoField notFoundRef,
            IEnumerable<ReadOnlyPropNoField> notFoundColl,
            IEnumerable<ReadOnlyPropNoFieldOther> notFoundSkipColl)
        {
            _notFound = id;
            _notFoundRef = notFoundRef;
            _notFoundColl = notFoundColl;
            _notFoundSkipColl = notFoundSkipColl;
        }

        public int Id { get; set; }

        public int Foo
            => _notFound;

        public ReadOnlyPropNoField Reference
            => _notFoundRef;

        public IEnumerable<ReadOnlyPropNoField> Collection
            => _notFoundColl;

        public IEnumerable<ReadOnlyPropNoFieldOther> SkipCollection
            => _notFoundSkipColl;
    }

    private class ReadOnlyPropNoFieldOther
    {
        public int Id { get; set; }
    }

    private class WriteOnlyPropNoField
    {
        private int _notFound;
        private WriteOnlyPropNoField _notFoundRef;
        private IEnumerable<WriteOnlyPropNoField> _notFoundColl;
        private IEnumerable<WriteOnlyPropNoFieldOther> _notFoundSkipColl;

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

        public IEnumerable<WriteOnlyPropNoFieldOther> SkipCollection
        {
            set => _notFoundSkipColl = value;
        }
    }

    private class WriteOnlyPropNoFieldOther
    {
        public int Id { get; set; }
    }

    private class PrivateSetterInBase : PrivateSetterBase
    {
        public override int Foo
            => _foo;

        public override PrivateSetterInBase Reference
            => _reference;

        public override IEnumerable<PrivateSetterInBase> Collection
            => _collection;

        public override IEnumerable<PrivateSetterBaseOther> SkipCollection
            => _skipCollection;
    }

    private class PrivateSetterBase
    {
        public int Id { get; set; }
        protected int _foo;
        protected PrivateSetterInBase _reference;
        protected IEnumerable<PrivateSetterInBase> _collection;
        protected IEnumerable<PrivateSetterBaseOther> _skipCollection;

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

        public virtual IEnumerable<PrivateSetterBaseOther> SkipCollection
        {
            get => _skipCollection;
            private set => _skipCollection = value;
        }
    }

    private class PrivateSetterBaseOther
    {
        public int Id { get; set; }
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

        public override IEnumerable<PrivateGetterBaseOther> SkipCollection
        {
            set => _skipCollection = value;
        }
    }

    private class PrivateGetterBase
    {
        public int Id { get; set; }
        protected int _foo;
        protected PrivateGetterInBase _reference;
        protected IEnumerable<PrivateGetterInBase> _collection;
        protected IEnumerable<PrivateGetterBaseOther> _skipCollection;

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

        public virtual IEnumerable<PrivateGetterBaseOther> SkipCollection
        {
            private get => _skipCollection;
            set => _skipCollection = value;
        }
    }

    private class PrivateGetterBaseOther
    {
        public int Id { get; set; }
    }
}

// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

using Microsoft.EntityFrameworkCore.Metadata.Conventions.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

// ReSharper disable ArrangeAccessorOwnerBody
// ReSharper disable MemberCanBePrivate.Local
// ReSharper disable UnusedMember.Local
// ReSharper disable PossibleInvalidOperationException
// ReSharper disable ParameterOnlyUsedForPreconditionCheck.Local
// ReSharper disable InconsistentNaming
// ReSharper disable ConvertToAutoProperty

#pragma warning disable RCS1213 // Remove unused member declaration.

namespace Microsoft.EntityFrameworkCore.Metadata.Conventions;

public class BackingFieldConventionTest
{
    [ConditionalFact]
    public void Auto_property_name_matching_field_is_used_as_first_preference()
        => FieldMatchTest<TheDarkSideOfTheMoon>("ComfortablyNumb", "<ComfortablyNumb>k__BackingField");

    [ConditionalFact]
    public void Property_name_matching_field_is_used_as_next_preference()
        => FieldMatchTest<TheDarkSideOfTheMoon>("IsThereAnybodyOutThere", "IsThereAnybodyOutThere");

    [ConditionalFact]
    public void Camel_case_matching_field_is_used_as_next_preference()
        => FieldMatchTest<TheDarkSideOfTheMoon>("Breathe", "breathe");

    [ConditionalFact]
    public void Camel_case_matching_field_is_not_used_if_type_is_not_compatible()
        => FieldMatchTest<TheDarkSideOfTheMoon>("OnTheRun", "_onTheRun");

    [ConditionalFact]
    public void Underscore_camel_case_matching_field_is_used_as_next_preference()
        => FieldMatchTest<TheDarkSideOfTheMoon>("Time", "_time");

    [ConditionalFact]
    public void Underscore_suffix_camel_case_matching_field_is_used_as_next_preference()
        => FieldMatchTest<TheDarkSideOfTheMoon>("Time", "_time");

    [ConditionalFact]
    public void Underscore_camel_case_matching_field_is_not_used_if_type_is_not_compatible()
        => FieldMatchTest<TheDarkSideOfTheMoon>("TheGreatGigInTheSky", "_TheGreatGigInTheSky");

    [ConditionalFact]
    public void Underscore_matching_field_is_used_as_next_preference()
        => FieldMatchTest<TheDarkSideOfTheMoon>("Money", "_Money");

    [ConditionalFact]
    public void Underscore_matching_field_is_not_used_if_type_is_not_compatible()
        => FieldMatchTest<TheDarkSideOfTheMoon>("UsAndThem", "m_usAndThem");

    [ConditionalFact]
    public void M_Underscore_camel_case_matching_field_is_used_as_next_preference()
        => FieldMatchTest<TheDarkSideOfTheMoon>("AnyColourYouLike", "m_anyColourYouLike");

    [ConditionalFact]
    public void M_underscore_camel_case_matching_field_is_not_used_if_type_is_not_compatible()
        => FieldMatchTest<TheDarkSideOfTheMoon>("BrainDamage", "m_BrainDamage");

    [ConditionalFact]
    public void M_underscore_matching_field_is_used_as_next_preference()
        => FieldMatchTest<TheDarkSideOfTheMoon>("Eclipse", "m_Eclipse");

    [ConditionalFact]
    public void M_underscore_matching_field_is_not_used_if_type_is_not_compatible()
    {
        var entityType = CreateModel().AddEntityType(typeof(TheDarkSideOfTheMoon));
        var property = entityType.AddProperty("SpeakToMe", typeof(int));

        RunConvention(property);
        Validate(property);

        Assert.Null(property.GetFieldName());
    }

    [ConditionalFact]
    public void Property_name_matching_field_is_used_as_first_preference_for_field_only()
        => FieldMatchTest<TheDarkerSideOfTheMoon>("IsThereAnybodyOutThere", "IsThereAnybodyOutThere");

    [ConditionalFact]
    public void Camel_case_matching_field_is_not_used_as_next_preference_for_field_only()
        => FieldMatchTest<TheDarkerSideOfTheMoon>("Breathe", null);

    [ConditionalFact]
    public void Camel_case_matching_field_is_not_used_if_type_is_not_compatible_for_field_only()
        => FieldMatchTest<TheDarkerSideOfTheMoon>("OnTheRun", null);

    [ConditionalFact]
    public void Underscore_camel_case_matching_field_is_not_used_as_next_preference_for_field_only()
        => FieldMatchTest<TheDarkerSideOfTheMoon>("Time", null);

    [ConditionalFact]
    public void Underscore_camel_case_matching_field_is_not_used_if_type_is_not_compatible_for_field_only()
        => FieldMatchTest<TheDarkerSideOfTheMoon>("TheGreatGigInTheSky", null);

    [ConditionalFact]
    public void Underscore_matching_field_is_not_used_as_next_preference_for_field_only()
        => FieldMatchTest<TheDarkerSideOfTheMoon>("Money", null);

    [ConditionalFact]
    public void Underscore_matching_field_is_not_used_if_type_is_not_compatible_for_field_only()
        => FieldMatchTest<TheDarkerSideOfTheMoon>("UsAndThem", null);

    [ConditionalFact]
    public void M_underscore_camel_case_matching_field_is_not_used_as_next_preference_for_field_only()
        => FieldMatchTest<TheDarkerSideOfTheMoon>("AnyColourYouLike", null);

    [ConditionalFact]
    public void M_underscore_camel_case_matching_field_is_not_used_if_type_is_not_compatible_for_field_only()
        => FieldMatchTest<TheDarkerSideOfTheMoon>("BrainDamage", null);

    [ConditionalFact]
    public void M_underscore_matching_field_is_not_used_as_next_preference_for_field_only()
        => FieldMatchTest<TheDarkerSideOfTheMoon>("Eclipse", null);

    [ConditionalFact]
    public void M_underscore_matching_field_is_not_used_if_type_is_not_compatible_for_field_only()
    {
        var entityType = CreateModel().AddEntityType(typeof(TheDarkerSideOfTheMoon));
        var property = entityType.AddProperty("SpeakToMe", typeof(int));

        RunConvention(property);
        Validate(property);

        Assert.Null(property.GetFieldName());
    }

    private void FieldMatchTest<TEntity>(string propertyName, string fieldName)
    {
        var entityType = CreateModel().AddEntityType(typeof(TEntity));
        var property = entityType.AddProperty(propertyName, typeof(int));

        RunConvention(property);
        Validate(property);

        Assert.Equal(fieldName, property.GetFieldName());
    }

    [ConditionalFact]
    public void Field_in_base_type_is_matched()
    {
        var entityType = CreateModel().AddEntityType(typeof(TheDarkSide));
        var property = entityType.AddProperty(OfTheMoon.TheGreatGigInTheSkyProperty);

        RunConvention(property);
        Validate(property);

        Assert.Equal("_theGreatGigInTheSky", property.GetFieldName());
    }

    [ConditionalFact]
    public void Matched_field_on_base_class_is_found()
    {
        var entityType = CreateModel().AddEntityType(typeof(TheDarkSide));
        var property = entityType.AddProperty(TheDarkSide.OnBaseProperty);

        RunConvention(property);
        Validate(property);

        Assert.Equal("_onBase", property.GetFieldName());
    }

    [ConditionalFact]
    public void Multiple_matches_throws()
    {
        var entityType = CreateModel().AddEntityType(typeof(AlwaysLookOnTheBrightSideOfLife));
        var property = entityType.AddProperty("OnTheRun", typeof(int));

        RunConvention(property);
        Assert.Equal(
            CoreStrings.ConflictingBackingFields(
                "OnTheRun", nameof(AlwaysLookOnTheBrightSideOfLife), "_onTheRun", "m_onTheRun"),
            Assert.Throws<InvalidOperationException>(
                () => Validate(property)).Message);
    }

    [ConditionalFact]
    public void Object_field_non_object_property_matches_and_throws_ambiguous()
    {
        var entityType = CreateModel().AddEntityType(typeof(HesNotTheMessiah));
        var property = entityType.AddProperty("OnTheRun", typeof(int));

        RunConvention(property);
        Assert.Equal(
            CoreStrings.ConflictingBackingFields(
                "OnTheRun", nameof(HesNotTheMessiah), "_onTheRun", "m_onTheRun"),
            Assert.Throws<InvalidOperationException>(
                () => Validate(property)).Message);
    }

    [ConditionalFact]
    public void Object_property_non_object_field_matches_and_throws_ambiguous()
    {
        var entityType = CreateModel().AddEntityType(typeof(HesAVeryNaughtyBoy));
        var property = entityType.AddProperty("OnTheRun", typeof(object));

        RunConvention(property);
        Assert.Equal(
            CoreStrings.ConflictingBackingFields(
                "OnTheRun", nameof(HesAVeryNaughtyBoy), "_onTheRun", "m_onTheRun"),
            Assert.Throws<InvalidOperationException>(
                () => Validate(property)).Message);
    }

    [ConditionalFact]
    public void Explicitly_set_FieldInfo_is_used()
    {
        var entityType = CreateModel().AddEntityType(typeof(AlwaysLookOnTheBrightSideOfLife));
        var property = entityType.AddProperty("OnTheRun", typeof(int));

        RunConvention(property);

        property.SetField("m_onTheRun");

        Validate(property);

        Assert.Equal("m_onTheRun", property.GetFieldName());
    }

    [ConditionalFact]
    public void FieldInfo_set_by_annotation_is_used()
    {
        var entityType = ((IConventionModel)CreateModel()).AddEntityType(typeof(AlwaysLookOnTheBrightSideOfLife));
        var property = entityType.AddProperty("OnTheRun", typeof(int));

        RunConvention((IMutableProperty)property);

        property.SetField("m_onTheRun", fromDataAnnotation: true);

        Validate((IMutableProperty)property);

        Assert.Equal("m_onTheRun", property.GetFieldName());
    }

    [ConditionalFact]
    public void Backing_field_is_not_discovered_for_indexer_property()
    {
        var entityType = CreateModel().AddEntityType(typeof(IndexedClass));
        var property = entityType.AddIndexerProperty("Nation", typeof(string));

        RunConvention(property);
        Validate(property);

        Assert.Null(property.GetFieldName());
    }

    [ConditionalFact]
    public void Setting_field_on_indexer_property_throws()
    {
        var entityType = CreateModel().AddEntityType(typeof(IndexedClass));
        var property = entityType.AddIndexerProperty("Nation", typeof(string));

        Assert.Equal(
            CoreStrings.BackingFieldOnIndexer("nation", entityType.DisplayName(), "Nation"),
            Assert.Throws<InvalidOperationException>(() => property.SetField("nation")).Message);
    }

    private void RunConvention(IMutableProperty property)
        => new BackingFieldConvention(CreateDependencies())
            .ProcessPropertyAdded(
                ((Property)property).Builder,
                new ConventionContext<IConventionPropertyBuilder>(((Model)property.DeclaringType.Model).ConventionDispatcher));

    private void Validate(IMutableProperty property)
        => new BackingFieldConvention(CreateDependencies())
            .ProcessModelFinalizing(
                ((Property)property).DeclaringType.Model.Builder,
                new ConventionContext<IConventionModelBuilder>(((Model)property.DeclaringType.Model).ConventionDispatcher));

    private ProviderConventionSetBuilderDependencies CreateDependencies()
        => InMemoryTestHelpers.Instance.CreateContextServices().GetRequiredService<ProviderConventionSetBuilderDependencies>();

    private static IMutableModel CreateModel()
        => new Model();

#pragma warning disable RCS1222 // Merge preprocessor directives.
#pragma warning disable 649, 169
#pragma warning disable IDE0027 // Use expression body for accessors
#pragma warning disable IDE1006 // Naming Styles
#pragma warning disable IDE0044 // Add readonly modifier
    private class TheDarkSideOfTheMoon
#pragma warning restore RCS1222 // Merge preprocessor directives.
    {
        private readonly string m_SpeakToMe;
        private int _notSpeakToMe;

        public int SpeakToMe
        {
            get { return _notSpeakToMe; }
            set { _notSpeakToMe = value; }
        }

        private int? comfortablyNumb;

        public int ComfortablyNumb { get; set; }

        private readonly int IsThereAnybodyOutThere;

        private int? breathe;

        public int Breathe
        {
            get { return (int)breathe; }
            set { breathe = value; }
        }

        private readonly string onTheRun;
        private int? _onTheRun;

        public int OnTheRun
        {
            get { return (int)_onTheRun; }
            set { _onTheRun = value; }
        }

        private int? _time;

        public int Time
        {
            get { return (int)_time; }
            set { _time = value; }
        }

        private int? time2_;

        public int Time2
        {
            get { return (int)_time; }
            set { _time = value; }
        }

        private readonly string _theGreatGigInTheSky;
        private int? _TheGreatGigInTheSky;

        public int TheGreatGigInTheSky
        {
            get { return (int)_TheGreatGigInTheSky; }
            set { _TheGreatGigInTheSky = value; }
        }

        private int? _Money;

        public int Money
        {
            get { return (int)_Money; }
            set { _Money = value; }
        }

        private readonly string _UsAndThem;
        private int? m_usAndThem;

        public int UsAndThem
        {
            get { return (int)m_usAndThem; }
            set { m_usAndThem = value; }
        }

        private int? m_anyColourYouLike;

        public int AnyColourYouLike
        {
            get { return (int)m_anyColourYouLike; }
            set { m_anyColourYouLike = value; }
        }

        private readonly string m_brainDamage;
        private int? m_BrainDamage;

        public int BrainDamage
        {
            get { return (int)m_BrainDamage; }
            set { m_BrainDamage = value; }
        }

        private int? m_Eclipse;

        public int Eclipse
        {
            get { return (int)m_Eclipse; }
            set { m_Eclipse = value; }
        }
    }

    private class TheDarkerSideOfTheMoon
    {
        private readonly string m_SpeakToMe;

        private readonly int IsThereAnybodyOutThere;

        private int? breathe;

        private readonly string onTheRun;
        private int? _onTheRun;

        private int? _time;

        private readonly string _theGreatGigInTheSky;
        private int? _TheGreatGigInTheSky;

        private int? _Money;

        private readonly string _UsAndThem;
        private int? m_usAndThem;

        private int? m_anyColourYouLike;

        private readonly string m_brainDamage;
        private int? m_BrainDamage;

        private int? m_Eclipse;
    }

    private class TheDarkSide : OfTheMoon
    {
        public static readonly PropertyInfo OnBaseProperty
            = typeof(TheDarkSide).GetProperty(nameof(OnBase));

        public int OnBase
        {
            get { return _onBase; }
            set { _onBase = value; }
        }

        // ReSharper disable once UnusedMember.Global
#pragma warning disable 414
        public new int Unrelated = 1;
    }

    private class OfTheMoon
    {
        public static readonly PropertyInfo TheGreatGigInTheSkyProperty =
            typeof(OfTheMoon).GetProperty(nameof(TheGreatGigInTheSky));

        private int? _theGreatGigInTheSky;

        public int TheGreatGigInTheSky
        {
            get { return (int)_theGreatGigInTheSky; }
            set { _theGreatGigInTheSky = value; }
        }

        protected int _onBase;

        // ReSharper disable once UnusedMember.Global
        public int Unrelated = 2;
    }

    private class AlwaysLookOnTheBrightSideOfLife
    {
        private readonly string onTheRun;
        private int? _onTheRun;
        private int? m_onTheRun;

        public int OnTheRun
        {
            get { return (int)m_onTheRun; }
            set { m_onTheRun = value; }
        }
    }

    private class HesNotTheMessiah
    {
        private object _onTheRun;
        private int m_onTheRun;

        public int OnTheRun
        {
            get { return m_onTheRun; }
            set { m_onTheRun = value; }
        }
    }

    private class HesAVeryNaughtyBoy
    {
        private object _onTheRun;
        private int m_onTheRun;

        public object OnTheRun
        {
            get { return m_onTheRun; }
            set { m_onTheRun = (int)value; }
        }
    }

    private class IndexedClass
    {
        private string nation;
        private string _nation;
        private string _Nation;
        private string m_nation;
        private string m_Nation;

        public object this[string name]
        {
            get => null;
            set { }
        }
    }

#pragma warning disable RCS1222 // Merge preprocessor directives.
#pragma warning restore 649, 169
#pragma warning restore IDE0027 // Use expression body for accessors
#pragma warning restore IDE1006 // Naming Styles
#pragma warning restore IDE0044 // Add readonly modifier
}
#pragma warning restore RCS1222 // Merge preprocessor directives.

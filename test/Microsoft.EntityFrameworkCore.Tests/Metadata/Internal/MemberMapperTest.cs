// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Metadata.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Metadata.Internal
{
    public class MemberMapperTest
    {
        [Fact]
        public void Annotated_field_name_is_used_if_present()
        {
            var entityType = new Model().AddEntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty(TheDarkSide.SpeakToMeProperty);
            property["BackingField"] = "fieldForSpeak";

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.Equal("fieldForSpeak", mapping.Item2.Name);
        }

        [Fact]
        public void Throws_if_annotated_field_name_is_not_found()
        {
            var entityType = new Model().AddEntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty(TheDarkSide.SpeakToMeProperty);
            property["BackingField"] = "_speakToMe";

            Assert.Equal(
                CoreStrings.MissingBackingField(typeof(TheDarkSide).FullName, "SpeakToMe", "_speakToMe"),
                Assert.Throws<InvalidOperationException>(
                    () => new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType)).Message);
        }

        [Fact]
        public void Throws_if_annotated_field_if_types_not_compatible()
        {
            var entityType = new Model().AddEntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty(TheDarkSide.SpeakToMeProperty);
            property["BackingField"] = "_badFieldForSpeak";

            Assert.Equal(
                CoreStrings.BadBackingFieldType("_badFieldForSpeak", typeof(string).Name, typeof(TheDarkSide).FullName, "SpeakToMe", typeof(int).Name),
                Assert.Throws<InvalidOperationException>(
                    () => new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType)).Message);
        }

        [Fact]
        public void Field_name_match_is_used_in_preference_to_property_setter()
        {
            var entityType = new Model().AddEntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty(TheDarkSide.BreatheProperty);

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.IsAssignableFrom<FieldInfo>(mapping.Item2);
            Assert.Equal("_breathe", mapping.Item2.Name);
        }

        [Fact]
        public void Property_setter_is_used_if_no_matching_field_is_found()
        {
            var entityType = new Model().AddEntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty(TheDarkSide.OnTheRunProperty);

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.IsAssignableFrom<PropertyInfo>(mapping.Item2);
            Assert.Equal("OnTheRun", mapping.Item2.Name);
        }

        [Fact]
        public void Throws_if_no_match_found_and_no_property_setter()
        {
            var entityType = new Model().AddEntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty(TheDarkSide.TimeProperty);

            Assert.Equal(
                CoreStrings.NoFieldOrSetter(typeof(TheDarkSide).FullName, "Time"),
                Assert.Throws<InvalidOperationException>(
                    () => new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType)).Message);
        }

        [Fact]
        public void Property_and_field_in_base_type_is_matched()
        {
            var entityType = new Model().AddEntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty(OfTheMoon.TheGreatGigInTheSkyProperty);

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.IsAssignableFrom<FieldInfo>(mapping.Item2);
            Assert.Equal("_theGreatGigInTheSky", mapping.Item2.Name);
        }

        [Fact]
        public void Property_and_field_in_base_type_is_matched_even_when_overridden()
        {
            var entityType = new Model().AddEntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty(TheDarkSide.MoneyProperty);

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.IsAssignableFrom<FieldInfo>(mapping.Item2);
            Assert.Equal("_money", mapping.Item2.Name);
        }

        [Fact]
        public void Private_setter_in_base_class_of_overridden_property_is_used()
        {
            var entityType = new Model().AddEntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty(TheDarkSide.UsAndThemProperty);

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.IsAssignableFrom<PropertyInfo>(mapping.Item2);
            Assert.Equal("UsAndThem", mapping.Item2.Name);
        }

        private class TheDarkSide : OfTheMoon
        {
            public static readonly PropertyInfo SpeakToMeProperty = typeof(TheDarkSide).GetProperty("SpeakToMe");
            public static readonly PropertyInfo BreatheProperty = typeof(TheDarkSide).GetProperty("Breathe");
            public static readonly PropertyInfo OnTheRunProperty = typeof(TheDarkSide).GetProperty("OnTheRun");
            public static readonly PropertyInfo TimeProperty = typeof(TheDarkSide).GetProperty("Time");
            public static readonly PropertyInfo MoneyProperty = typeof(TheDarkSide).GetProperty("Money");
            public static readonly PropertyInfo UsAndThemProperty = typeof(TheDarkSide).GetProperty("UsAndThem");

            private int? fieldForSpeak;
#pragma warning disable 169
            private string _badFieldForSpeak;
#pragma warning restore 169

            public int SpeakToMe
            {
                get { return (int)fieldForSpeak; }
                set { fieldForSpeak = value; }
            }

            private int? _breathe;

            public int Breathe
            {
                get { return (int)_breathe; }
                set { _breathe = value; }
            }

            public int OnTheRun { get; set; }

            public int Time => 0;

            public override int Money => 0;

            public override int UsAndThem => 0;
        }

        private class OfTheMoon
        {
            public static readonly PropertyInfo TheGreatGigInTheSkyProperty = typeof(OfTheMoon).GetProperty("TheGreatGigInTheSky");

            private int? _theGreatGigInTheSky;

            public int TheGreatGigInTheSky
            {
                get { return (int)_theGreatGigInTheSky; }
                set { _theGreatGigInTheSky = value; }
            }

            private int? _money;

            public virtual int Money
            {
                get { return (int)_money; }
                private set { _money = value; }
            }

            public virtual int UsAndThem { get; private set; }
        }
    }
}

// Copyright (c) Microsoft Open Technologies, Inc.
// All Rights Reserved
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
// http://www.apache.org/licenses/LICENSE-2.0
//
// THIS CODE IS PROVIDED *AS IS* BASIS, WITHOUT WARRANTIES OR
// CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING
// WITHOUT LIMITATION ANY IMPLIED WARRANTIES OR CONDITIONS OF
// TITLE, FITNESS FOR A PARTICULAR PURPOSE, MERCHANTABLITY OR
// NON-INFRINGEMENT.
// See the Apache 2 License for the specific language governing
// permissions and limitations under the License.

using System;
using System.Linq;
using System.Reflection;
using Microsoft.Data.Entity.Metadata;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Metadata
{
    public class MemberMapperTest
    {
        [Fact]
        public void Annotated_field_name_is_used_if_present()
        {
            var entityType = new EntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty("SpeakToMe", typeof(int));
            property["BackingField"] = "fieldForSpeak";

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.Equal("fieldForSpeak", mapping.Item2.Name);
        }

        [Fact]
        public void Throws_if_annotated_field_name_is_not_found()
        {
            var entityType = new EntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty("SpeakToMe", typeof(int));
            property["BackingField"] = "_speakToMe";

            Assert.Equal(
                Strings.FormatMissingBackingField("TheDarkSide", "SpeakToMe", "_speakToMe"),
                Assert.Throws<InvalidOperationException>(
                    () => new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType)).Message);
        }

        [Fact]
        public void Throws_if_annotated_field_if_types_not_compatible()
        {
            var entityType = new EntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty("SpeakToMe", typeof(string));
            property["BackingField"] = "fieldForSpeak";

            Assert.Equal(
                Strings.FormatBadBackingFieldType("fieldForSpeak", typeof(int?).Name, "TheDarkSide", "SpeakToMe", typeof(string).Name),
                Assert.Throws<InvalidOperationException>(
                    () => new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType)).Message);
        }

        [Fact]
        public void Field_name_match_is_used_in_preference_to_property_setter()
        {
            var entityType = new EntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty("Breathe", typeof(int));

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.IsAssignableFrom<FieldInfo>(mapping.Item2);
            Assert.Equal("_breathe", mapping.Item2.Name);
        }

        [Fact]
        public void Property_setter_is_used_if_no_matching_field_is_found()
        {
            var entityType = new EntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty("OnTheRun", typeof(int));

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.IsAssignableFrom<PropertyInfo>(mapping.Item2);
            Assert.Equal("OnTheRun", mapping.Item2.Name);
        }

        [Fact]
        public void Throws_if_no_match_found_and_no_property_setter()
        {
            var entityType = new EntityType(typeof(TheDarkSide));
            entityType.AddProperty("Time", typeof(string));

            Assert.Equal(
                Strings.FormatNoFieldOrSetter("TheDarkSide", "Time"),
                Assert.Throws<InvalidOperationException>(
                    () => new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType)).Message);
        }

        [Fact]
        public void Property_and_field_in_base_type_is_matched()
        {
            var entityType = new EntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty("TheGreatGigInTheSky", typeof(int));

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.IsAssignableFrom<FieldInfo>(mapping.Item2);
            Assert.Equal("_theGreatGigInTheSky", mapping.Item2.Name);
        }

        [Fact]
        public void Property_and_field_in_base_type_is_matched_even_when_overridden()
        {
            var entityType = new EntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty("Money", typeof(int));

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.IsAssignableFrom<FieldInfo>(mapping.Item2);
            Assert.Equal("_money", mapping.Item2.Name);
        }

        [Fact]
        public void Private_setter_in_base_class_of_overridden_property_is_used()
        {
            var entityType = new EntityType(typeof(TheDarkSide));
            var property = entityType.AddProperty("UsAndThem", typeof(int));

            var mapping = new MemberMapper(new FieldMatcher()).MapPropertiesToMembers(entityType).Single();

            Assert.Same(property, mapping.Item1);
            Assert.IsAssignableFrom<PropertyInfo>(mapping.Item2);
            Assert.Equal("UsAndThem", mapping.Item2.Name);
        }

        private class TheDarkSide : OfTheMoon
        {
            private int? fieldForSpeak;

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

            public int Time
            {
                get { return 0; }
            }

            public override int Money
            {
                get { return 0; }
            }

            public override int UsAndThem
            {
                get { return 0; }
            }
        }

        private class OfTheMoon
        {
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

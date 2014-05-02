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
    public class FieldMatcherTest
    {
        [Fact]
        public void Camel_case_matching_field_is_used_as_first_preference()
        {
            FieldMatchTest("Breathe", "breathe");
        }

        [Fact]
        public void Camel_case_matching_field_is_not_used_if_type_is_not_compatible()
        {
            FieldMatchTest("OnTheRun", "_onTheRun");
        }

        [Fact]
        public void Underscore_camel_case_matching_field_is_used_as_second_preference()
        {
            FieldMatchTest("Time", "_time");
        }

        [Fact]
        public void Underscpre_camel_case_matching_field_is_not_used_if_type_is_not_compatible()
        {
            FieldMatchTest("TheGreatGigInTheSky", "_TheGreatGigInTheSky");
        }

        [Fact]
        public void Underscpre_matching_field_is_used_as_third_preference()
        {
            FieldMatchTest("Money", "_Money");
        }

        [Fact]
        public void Underscore_matching_field_is_not_used_if_type_is_not_compatible()
        {
            FieldMatchTest("UsAndThem", "m_usAndThem");
        }

        [Fact]
        public void M_underscpre_camel_case_matching_field_is_used_as_fourth_preference()
        {
            FieldMatchTest("AnyColourYouLike", "m_anyColourYouLike");
        }

        [Fact]
        public void M_underscore_camel_case_matching_field_is_not_used_if_type_is_not_compatible()
        {
            FieldMatchTest("BrainDamage", "m_BrainDamage");
        }

        [Fact]
        public void M_underscpre_matching_field_is_used_as_fifth_preference()
        {
            FieldMatchTest("Eclipse", "m_Eclipse");
        }

        private static void FieldMatchTest(string propertyName, string fieldName)
        {
            var entityType = new EntityType(typeof(TheDarkSideOfTheMoon));
            var property = entityType.AddProperty(propertyName, typeof(int));
            var propertyInfo = entityType.Type.GetAnyProperty(propertyName);
            var fields = propertyInfo.DeclaringType.GetRuntimeFields().ToDictionary(f => f.Name);

            var matchedField = new FieldMatcher().TryMatchFieldName(property, propertyInfo, fields);

            Assert.Equal(fieldName, matchedField.Name);
        }

        [Fact]
        public void M_underscore_matching_field_is_not_used_if_type_is_not_compatible()
        {
            var entityType = new EntityType(typeof(TheDarkSideOfTheMoon));
            var property = entityType.AddProperty("SpeakToMe", typeof(int));
            var propertyInfo = entityType.Type.GetAnyProperty("SpeakToMe");
            var fields = propertyInfo.DeclaringType.GetRuntimeFields().ToDictionary(f => f.Name);

            Assert.Null(new FieldMatcher().TryMatchFieldName(property, propertyInfo, fields));
        }

        private class TheDarkSideOfTheMoon
        {
#pragma warning disable 649
#pragma warning disable 169
            private string m_SpeakToMe;

            public int SpeakToMe { get; set; }

            private int? breathe;
            private int? _breathe;
            private int? _Breathe;
            private int? m_breathe;
            private int? m_Breathe;

            public int Breathe
            {
                get { return (int)breathe; }
                set { breathe = value; }
            }

            private string onTheRun;
            private int? _onTheRun;
            private int? _OnTheRun;
            private int? m_onTheRun;
            private int? m_OnTheRun;

            public int OnTheRun
            {
                get { return (int)_onTheRun; }
                set { _onTheRun = value; }
            }

            private int? _time;
            private int? _Time;
            private int? m_time;
            private int? m_Time;

            public int Time
            {
                get { return (int)_time; }
                set { _time = value; }
            }

            private string _theGreatGigInTheSky;
            private int? _TheGreatGigInTheSky;
            private int? m_theGreatGigInTheSky;
            private int? m_TheGreatGigInTheSky;

            public int TheGreatGigInTheSky
            {
                get { return (int)_TheGreatGigInTheSky; }
                set { _TheGreatGigInTheSky = value; }
            }

            private int? _Money;
            private int? m_money;
            private int? m_Money;

            public int Money
            {
                get { return (int)_Money; }
                set { _Money = value; }
            }

            private string _UsAndThem;
            private int? m_usAndThem;
            private int? m_UsAndThem;

            public int UsAndThem
            {
                get { return (int)m_usAndThem; }
                set { m_usAndThem = value; }
            }

            private int? m_anyColourYouLike;
            private int? m_AnyColourYouLike;

            public int AnyColourYouLike
            {
                get { return (int)m_anyColourYouLike; }
                set { m_anyColourYouLike = value; }
            }

            private string m_brainDamage;
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
#pragma warning restore 649
#pragma warning restore 169
        }
    }
}

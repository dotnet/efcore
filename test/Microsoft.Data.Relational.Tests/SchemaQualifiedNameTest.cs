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
using Xunit;

namespace Microsoft.Data.Relational.Tests
{
    public class SchemaQualifiedNameTest
    {
        [Fact]
        public void To_string_returns_table_name_when_no_schema_specified()
        {
            var schemaQualifiedName = new SchemaQualifiedName("T");

            Assert.Equal("T", schemaQualifiedName.ToString());
        }

        [Fact]
        public void To_string_returns_schema_and_table_name_when_schema_specified()
        {
            var schemaQualifiedName = new SchemaQualifiedName("T", "S");

            Assert.Equal("S.T", schemaQualifiedName.ToString());
        }

        [Fact]
        public void Equals_returns_true_when_names_equal_and_no_schema_specified()
        {
            var schemaQualifiedName1 = new SchemaQualifiedName("T");
            var schemaQualifiedName2 = new SchemaQualifiedName("T");

            Assert.Equal(schemaQualifiedName1, schemaQualifiedName2);
        }

        [Fact]
        public void Equals_returns_false_when_names_equal_and_schemas_not_equal()
        {
            var schemaQualifiedName1 = new SchemaQualifiedName("T", "S1");
            var schemaQualifiedName2 = new SchemaQualifiedName("T", "S2");

            Assert.NotEqual(schemaQualifiedName1, schemaQualifiedName2);
        }

        [Fact]
        public void Parse_parses_table_name()
        {
            var schemaQualifiedName = SchemaQualifiedName.Parse("A");

            Assert.Equal(null, schemaQualifiedName.Schema);
            Assert.Equal("A", schemaQualifiedName.Name);
        }

        [Fact]
        public void Parse_parses_schema_dot_table_name()
        {
            var schemaQualifiedName = SchemaQualifiedName.Parse("S.A");

            Assert.Equal("S", schemaQualifiedName.Schema);
            Assert.Equal("A", schemaQualifiedName.Name);
            Assert.True(schemaQualifiedName.IsSchemaQualified);
        }

        [Fact]
        public void Parse_throws_when_too_many_parts()
        {
            Assert.Equal(
                Strings.FormatInvalidSchemaQualifiedName("S1.S2.A"),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse("S1.S2.A")).Message);
        }

        [Fact]
        public void Parse_throws_for_empty_table()
        {
            Assert.Equal(
                Strings.FormatInvalidSchemaQualifiedName("A."),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse("A.")).Message);
        }

        [Fact]
        public void Parse_throws_for_empty_schema()
        {
            Assert.Equal(
                Strings.FormatInvalidSchemaQualifiedName(".A"),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse(".A")).Message);
        }

        [Fact]
        public void Parse_throws_for_empty_table_and_schema()
        {
            Assert.Equal(
                Strings.FormatInvalidSchemaQualifiedName("."),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse(".")).Message);
        }

        [Fact]
        public void Parse_parses_name_with_delimeters()
        {
            var schemaQualifiedName = SchemaQualifiedName.Parse("[a.].[.b]");

            Assert.Equal("a.", schemaQualifiedName.Schema);
            Assert.Equal(".b", schemaQualifiedName.Name);

            schemaQualifiedName = SchemaQualifiedName.Parse("foo.[bar.baz]");

            Assert.Equal("foo", schemaQualifiedName.Schema);
            Assert.Equal("bar.baz", schemaQualifiedName.Name);

            schemaQualifiedName = SchemaQualifiedName.Parse("[foo.[bar].baz");

            Assert.Equal("foo.[bar", schemaQualifiedName.Schema);
            Assert.Equal("baz", schemaQualifiedName.Name);

            schemaQualifiedName = SchemaQualifiedName.Parse("[foo.[bar.baz]");

            Assert.Null(schemaQualifiedName.Schema);
            Assert.Equal("foo.[bar.baz", schemaQualifiedName.Name);
        }

        [Fact]
        public void Parse_parses_name_with_escaped_delimeters()
        {
            var schemaQualifiedName = SchemaQualifiedName.Parse("[a.]].]]].[.b.[c]]d]");

            Assert.Equal("a.].]", schemaQualifiedName.Schema);
            Assert.Equal(".b.[c]d", schemaQualifiedName.Name);
        }

        [Fact]
        public void To_string_should_escape_name_when_required()
        {
            var schemaQualifiedName = SchemaQualifiedName.Parse("[a.]].]]].[.b.[c]]d]");

            Assert.Equal("[a.]].]]].[.b.[c]]d]", schemaQualifiedName.ToString());

            schemaQualifiedName = SchemaQualifiedName.Parse("abc.[d.ef]");

            Assert.Equal("abc.[d.ef]", schemaQualifiedName.ToString());
        }
    }
}

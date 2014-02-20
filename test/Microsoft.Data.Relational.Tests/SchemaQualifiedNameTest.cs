// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Relational
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
                Strings.InvalidSchemaQualifiedName("S1.S2.A"),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse("S1.S2.A")).Message);
        }

        [Fact]
        public void Parse_throws_for_empty_table()
        {
            Assert.Equal(
                Strings.InvalidSchemaQualifiedName("A."),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse("A.")).Message);
        }

        [Fact]
        public void Parse_throws_for_empty_schema()
        {
            Assert.Equal(
                Strings.InvalidSchemaQualifiedName(".A"),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse(".A")).Message);
        }

        [Fact]
        public void Parse_throws_for_empty_table_and_schema()
        {
            Assert.Equal(
                Strings.InvalidSchemaQualifiedName("."),
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

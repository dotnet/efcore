// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Relational
{
    public class SchemaQualifiedNameTest
    {
        [Fact]
        public void ToStringReturnsTableNameWhenNoSchemaSpecified()
        {
            var schemaQualifiedName = new SchemaQualifiedName("T");

            Assert.Equal("T", schemaQualifiedName.ToString());
        }

        [Fact]
        public void ToStringReturnsSchemaAndTableNameWhenSchemaSpecified()
        {
            var schemaQualifiedName = new SchemaQualifiedName("T", "S");

            Assert.Equal("S.T", schemaQualifiedName.ToString());
        }

        [Fact]
        public void EqualsReturnsTrueWhenNamesEqualAndNoSchemaSpecified()
        {
            var schemaQualifiedName1 = new SchemaQualifiedName("T");
            var schemaQualifiedName2 = new SchemaQualifiedName("T");

            Assert.Equal(schemaQualifiedName1, schemaQualifiedName2);
        }

        [Fact]
        public void EqualsReturnsFalseWhenNamesEqualAndSchemasNotEqual()
        {
            var schemaQualifiedName1 = new SchemaQualifiedName("T", "S1");
            var schemaQualifiedName2 = new SchemaQualifiedName("T", "S2");

            Assert.NotEqual(schemaQualifiedName1, schemaQualifiedName2);
        }

        [Fact]
        public void ParseParsesTableName()
        {
            var schemaQualifiedName = SchemaQualifiedName.Parse("A");

            Assert.Equal(null, schemaQualifiedName.Schema);
            Assert.Equal("A", schemaQualifiedName.Name);
        }

        [Fact]
        public void ParseParsesSchemaDotTableName()
        {
            var schemaQualifiedName = SchemaQualifiedName.Parse("S.A");

            Assert.Equal("S", schemaQualifiedName.Schema);
            Assert.Equal("A", schemaQualifiedName.Name);
        }

        [Fact]
        public void ParseThrowsWhenTooManyParts()
        {
            Assert.Equal(
                Strings.InvalidSchemaQualifiedName("S1.S2.A"),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse("S1.S2.A")).Message);
        }

        [Fact]
        public void ParseThrowsForEmptyTable()
        {
            Assert.Equal(
                Strings.InvalidSchemaQualifiedName("A."),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse("A.")).Message);
        }

        [Fact]
        public void ParseThrowsForEmptySchema()
        {
            Assert.Equal(
                Strings.InvalidSchemaQualifiedName(".A"),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse(".A")).Message);
        }

        [Fact]
        public void ParseThrowsForEmptyTableAndSchema()
        {
            Assert.Equal(
                Strings.InvalidSchemaQualifiedName("."),
                Assert.Throws<ArgumentException>(() => SchemaQualifiedName.Parse(".")).Message);
        }

        [Fact]
        public void ParseParsesNameWithDelimeters()
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
        public void ParseParsesNameWithEscapedDelimeters()
        {
            var schemaQualifiedName = SchemaQualifiedName.Parse("[a.]].]]].[.b.[c]]d]");

            Assert.Equal("a.].]", schemaQualifiedName.Schema);
            Assert.Equal(".b.[c]d", schemaQualifiedName.Name);
        }

        [Fact]
        public void ToStringShouldEscapeNameWhenRequired()
        {
            var schemaQualifiedName = SchemaQualifiedName.Parse("[a.]].]]].[.b.[c]]d]");

            Assert.Equal("[a.]].]]].[.b.[c]]d]", schemaQualifiedName.ToString());

            schemaQualifiedName = SchemaQualifiedName.Parse("abc.[d.ef]");

            Assert.Equal("abc.[d.ef]", schemaQualifiedName.ToString());
        }
    }
}

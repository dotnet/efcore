// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Collections.Generic;
using Xunit;

namespace Microsoft.Data.Entity.Commands.Utilities
{
    public class CSharpHelperTest
    {
        [Theory]
        [InlineData(
            "single-line string with \"",
            "\"single-line string with \\\"\"")]
        [InlineData(
            true,
            "true")]
        [InlineData(
            false,
            "false")]
        [InlineData(
            (byte)42,
            "(byte)42")]
        [InlineData(
            'A',
            "'A'")]
        [InlineData(
            '\'',
            @"'\''")]
        [InlineData(
            4.2,
            "4.2")]
        [InlineData(
            4.2f,
            "4.2f")]
        [InlineData(
            42,
            "42")]
        [InlineData(
            42L,
            "42L")]
        [InlineData(
            (sbyte)42,
            "(sbyte)42")]
        [InlineData(
            (short)42,
            "(short)42")]
        [InlineData(
            42u,
            "42u")]
        [InlineData(
            42ul,
            "42ul")]
        [InlineData(
            (ushort)42,
            "(ushort)42")]
        public void Literal_works(dynamic value, string expected)
        {
            var literal = new CSharpHelper().Literal(value);
            Assert.Equal(expected, literal);
        }

        [Fact]
        public void Literal_works_when_empty_ByteArray() =>
            Literal_works(
                new byte[0],
                "new byte[] {  }");

        [Fact]
        public void Literal_works_when_single_ByteArray() =>
            Literal_works(
                new byte[] { 1 },
                "new byte[] { 1 }");

        [Fact]
        public void Literal_works_when_many_ByteArray() =>
            Literal_works(
                new byte[] { 1, 2 },
                "new byte[] { 1, 2 }");

        [Fact]
        public void Literal_works_when_multiline_string() =>
            Literal_works(
                "multi-line" + Environment.NewLine + "string with \"",
                "@\"multi-line" + Environment.NewLine + "string with \"\"\"");

        [Fact]
        public void Literal_works_when_DateTime() =>
            Literal_works(
                new DateTime(2015, 3, 12),
                "DateTime.Parse(\"3/12/2015 12:00:00 AM\")");

        [Fact]
        public void Literal_works_when_DateTimeOffset() =>
            Literal_works(
                new DateTimeOffset(new DateTime(2015, 3, 12), new TimeSpan(-7, 0, 0)),
                "DateTimeOffset.Parse(\"3/12/2015 12:00:00 AM -07:00\")");

        [Fact]
        public void Literal_works_when_decimal() =>
            Literal_works(
                4.2m,
                "4.2m");

        [Fact]
        public void Literal_works_when_Guid() =>
            Literal_works(
                new Guid("fad4f3c3-9501-4b3a-af99-afeb496f7664"),
                "new Guid(\"fad4f3c3-9501-4b3a-af99-afeb496f7664\")");

        [Fact]
        public void Literal_works_when_TimeSpan() =>
            Literal_works(
                new TimeSpan(2, 8, 31),
                "TimeSpan.Parse(\"02:08:31\")");

        [Fact]
        public void Literal_works_when_NullableInt() =>
            Literal_works(
                (int?)42,
                "42");

        [Fact]
        public void Literal_works_when_single_StringArray() =>
            Literal_works(
                new[] { "A" },
                "\"A\"");

        [Fact]
        public void Literal_works_when_many_StringArray() =>
            Literal_works(
                new[] { "A", "B" },
                "new[] { \"A\", \"B\" }");

        [Fact]
        public void Literal_works_when_empty_DictionaryStringString() =>
            Literal_works(
                new Dictionary<string, string>(),
                "new Dictionary<string, string> {  }");

        [Fact]
        public void Literal_works_when_single_DictionaryStringString() =>
            Literal_works(
                new Dictionary<string, string> { { "A", "a" } },
                "new Dictionary<string, string> { { \"A\", \"a\" } }");

        [Fact]
        public void Literal_works_when_many_DictionaryStringString() =>
            Literal_works(
                new Dictionary<string, string> { { "A", "a" }, { "B", "b" } },
                "new Dictionary<string, string> { { \"A\", \"a\" }, { \"B\", \"b\" } }");

        [Fact]
        public void Literal_throws_when_unknown()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new CSharpHelper().Literal((object)1));
            Assert.Equal(Strings.UnknownLiteral(typeof(int)), ex.Message);
        }
    }
}
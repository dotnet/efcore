// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Microsoft.EntityFrameworkCore.Migrations.Design;
using Microsoft.EntityFrameworkCore.Specification.Tests.TestUtilities.Xunit;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Design.Tests.Migrations.Design
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
             -1.7976931348623157E+308, // Double MinValue
             "-1.7976931348623157E+308")]
        [InlineData(
             1.7976931348623157E+308, // Double MaxValue
             "1.7976931348623157E+308")]
        [InlineData(
             4.2f,
             "4.2f")]
        [InlineData(
             -3.402823E+38f, // Single MinValue
             "-3.402823E+38f")]
        [InlineData(
             3.402823E+38f, // Single MaxValue
             "3.402823E+38f")]
        [InlineData(
             42,
             "42")]
        [InlineData(
             42L,
             "42L")]
        [InlineData(
             9000000000000000000L, // Ensure not printed as exponent
             "9000000000000000000L")]
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
             18000000000000000000ul, // Ensure not printed as exponent
             "18000000000000000000ul")]
        [InlineData(
             (ushort)42,
             "(ushort)42")]
        [InlineData(
             "",
             "\"\"")]
        [InlineData(
             SomeEnum.Default,
             "CSharpHelperTest.SomeEnum.Default")]
        public void Literal_works(object value, string expected)
        {
            var literal = new CSharpHelper().UnknownLiteral(value);
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
        [UseCulture("de-DE")]
        public void Literal_works_when_DateTime() =>
            Literal_works(
                new DateTime(2015, 3, 15, 20, 45, 17, 300, DateTimeKind.Local),
                "new DateTime(2015, 3, 15, 20, 45, 17, 300, DateTimeKind.Local)");

        [Fact]
        [UseCulture("de-DE")]
        public void Literal_works_when_DateTimeOffset() =>
            Literal_works(
                new DateTimeOffset(new DateTime(2015, 3, 15, 19, 43, 47, 500), new TimeSpan(-7, 0, 0)),
                "new DateTimeOffset(new DateTime(2015, 3, 15, 19, 43, 47, 500, DateTimeKind.Unspecified), new TimeSpan(0, -7, 0, 0, 0))");

        [Fact]
        public void Literal_works_when_decimal() =>
            Literal_works(
                4.2m,
                "4.2m");

        [Fact]
        public void Literal_works_when_decimal_max_value() =>
            Literal_works(
                79228162514264337593543950335m, // Decimal MaxValue
                "79228162514264337593543950335m");

        [Fact]
        public void Literal_works_when_decimal_min_value() =>
            Literal_works(
                -79228162514264337593543950335m, // Decimal MinValue
                "-79228162514264337593543950335m");

        [Fact]
        public void Literal_works_when_Guid() =>
            Literal_works(
                new Guid("fad4f3c3-9501-4b3a-af99-afeb496f7664"),
                "new Guid(\"fad4f3c3-9501-4b3a-af99-afeb496f7664\")");

        [Fact]
        public void Literal_works_when_TimeSpan() =>
            Literal_works(
                new TimeSpan(17, 21, 42, 37, 250),
                "new TimeSpan(17, 21, 42, 37, 250)");

        [Fact]
        public void Literal_works_when_NullableInt() =>
            Literal_works(
                (int?)42,
                "42");

        [Fact]
        public void Literal_works_when_single_StringArray()
        {
            var literal = new CSharpHelper().Literal(new[] { "A" });
            Assert.Equal("\"A\"", literal);
        }

        [Fact]
        public void Literal_works_when_many_StringArray()
        {
            var literal = new CSharpHelper().Literal(new[] { "A", "B" });
            Assert.Equal("new[] { \"A\", \"B\" }", literal);
        }

        [Fact]
        public void UnknownLiteral_throws_when_unknown()
        {
            var ex = Assert.Throws<InvalidOperationException>(
                () => new CSharpHelper().UnknownLiteral(new object()));
            Assert.Equal(DesignStrings.UnknownLiteral(typeof(object)), ex.Message);
        }

        [Theory]
        [InlineData(typeof(int), "int")]
        [InlineData(typeof(int?), "int?")]
        [InlineData(typeof(int[]), "int[]")]
        [InlineData(typeof(int[,]), "int[,]")]
        [InlineData(typeof(int[][]), "int[][]")]
        [InlineData(typeof(Generic<int>), "Generic<int>")]
        [InlineData(typeof(Nested), "CSharpHelperTest.Nested")]
        [InlineData(typeof(Generic<Generic<int>>), "Generic<Generic<int>>")]
        [InlineData(typeof(MultiGeneric<int, int>), "MultiGeneric<int, int>")]
        [InlineData(typeof(NestedGeneric<int>), "CSharpHelperTest.NestedGeneric<int>")]
        [InlineData(typeof(Nested.DoubleNested), "CSharpHelperTest.Nested.DoubleNested")]
        public void Reference_works(Type type, string expected)
            => Assert.Equal(expected, new CSharpHelper().Reference(type));

        private class Nested
        {
            public class DoubleNested
            {
            }
        }

        internal class NestedGeneric<T>
        {
        }

        private enum SomeEnum
        {
            Default
        }

        [Theory]
        [InlineData("dash-er", "dasher")]
        [InlineData("params", "@params")]
        [InlineData("true", "@true")]
        [InlineData("yield", "yield")]
        [InlineData("spac ed", "spaced")]
        [InlineData("1nders", "_1nders")]
        [InlineData("name.space", "@namespace")]
        [InlineData("$", "_")]
        public void Identifier_works(string input, string expected)
        {
            Assert.Equal(expected, new CSharpHelper().Identifier(input));
        }

        [Theory]
        [InlineData(new[] { "WebApplication1", "Migration" }, "WebApplication1.Migration")]
        [InlineData(new[] { "WebApplication1.Migration" }, "WebApplication1.Migration")]
        [InlineData(new[] { "ef-xplat.namespace" }, "efxplat.@namespace")]
        [InlineData(new[] { "#", "$" }, "_._")]
        [InlineData(new[] { "" }, "_")]
        [InlineData(new string[] { }, "_")]
        [InlineData(new string[] { null }, "_")]
        public void Namespace_works(string[] input, string excepted)
        {
            Assert.Equal(excepted, new CSharpHelper().Namespace(input));
        }
    }

    internal class Generic<T>
    {
    }

    internal class MultiGeneric<T1, T2>
    {
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.EntityFrameworkCore.Internal;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Tests.Utilities
{
    public class IndentedStringBuilderTest
    {
        private readonly string _nl = Environment.NewLine;

        [Fact]
        public void Append_at_start_with_indent()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.Append("Foo");
            }

            Assert.Equal("    Foo", indentedStringBuilder.ToString());
        }

        [Fact]
        public void Append_in_middle_when_no_new_line()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            indentedStringBuilder.Append("Foo");

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.Append("Foo");
            }

            Assert.Equal("FooFoo", indentedStringBuilder.ToString());
        }

        [Fact]
        public void Append_in_middle_when_new_line()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            indentedStringBuilder.AppendLine("Foo");

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.Append("Foo");
                indentedStringBuilder.AppendLine();
            }

            Assert.Equal($"Foo{_nl}    Foo{_nl}", indentedStringBuilder.ToString());
        }

        [Fact]
        public void Append_line_at_start_with_indent()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.AppendLine("Foo");
            }

            Assert.Equal("    Foo" + _nl, indentedStringBuilder.ToString());
        }

        [Fact]
        public void Append_line_in_middle_when_no_new_line()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            indentedStringBuilder.AppendLine("Foo");

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.AppendLine("Foo");
            }

            Assert.Equal($"Foo{_nl}    Foo{_nl}", indentedStringBuilder.ToString());
        }

        [Fact]
        public void Append_line_with_indent_only()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.AppendLine();
            }

            Assert.Equal(Environment.NewLine, indentedStringBuilder.ToString());
        }
    }
}

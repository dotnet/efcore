// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Xunit;

namespace Microsoft.Data.Entity.Utilities
{
    public class IndentedStringBuilderTest
    {
        [Fact]
        public void AppendAtStartWithIndent()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.Append("Foo");
            }

            Assert.Equal("    Foo", indentedStringBuilder.ToString());
        }

        [Fact]
        public void AppendInMiddleWhenNoNewLine()
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
        public void AppendInMiddleWhenNewLine()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            indentedStringBuilder.AppendLine("Foo");

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.Append("Foo");
                indentedStringBuilder.AppendLine();
            }

            Assert.Equal("Foo\r\n    Foo\r\n", indentedStringBuilder.ToString());
        }

        [Fact]
        public void AppendLineAtStartWithIndent()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.AppendLine("Foo");
            }

            Assert.Equal("    Foo\r\n", indentedStringBuilder.ToString());
        }

        [Fact]
        public void AppendLineInMiddleWhenNoNewLine()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            indentedStringBuilder.AppendLine("Foo");

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.AppendLine("Foo");
            }

            Assert.Equal("Foo\r\n    Foo\r\n", indentedStringBuilder.ToString());
        }
    }
}

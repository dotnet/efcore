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

using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class IndentedStringBuilderTest
    {
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

            Assert.Equal("Foo\r\n    Foo\r\n", indentedStringBuilder.ToString());
        }

        [Fact]
        public void Append_line_at_start_with_indent()
        {
            var indentedStringBuilder = new IndentedStringBuilder();

            using (indentedStringBuilder.Indent())
            {
                indentedStringBuilder.AppendLine("Foo");
            }

            Assert.Equal("    Foo\r\n", indentedStringBuilder.ToString());
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

            Assert.Equal("Foo\r\n    Foo\r\n", indentedStringBuilder.ToString());
        }
    }
}

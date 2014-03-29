// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class EnumerableExtensionsTest
    {
        [Fact]
        public void Order_by_ordinal_should_respect_case()
        {
            Assert.Equal(new[] { "A", "a", "b" }, new[] { "b", "A", "a" }.OrderByOrdinal(s => s));
        }

        [Fact]
        public void Join_empty_input_returns_empty_string()
        {
            Assert.Equal("", new object[] { }.Join());
        }

        [Fact]
        public void Join_single_element_does_not_use_separator()
        {
            Assert.Equal("42", new object[] { 42 }.Join());
        }

        [Fact]
        public void Join_should_use_comma_by_default()
        {
            Assert.Equal("42, bar", new object[] { 42, "bar" }.Join());
        }

        [Fact]
        public void Join_should_use_explicit_separator_when_provided()
        {
            Assert.Equal("42-bar", new object[] { 42, "bar" }.Join("-"));
        }
    }
}

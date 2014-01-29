// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Utilities
{
    using System;
    using Xunit;

    public class CheckFacts
    {
        [Fact]
        public void NotNull_throws_when_arg_is_null()
        {
// ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentNullException>(() => Check.NotNull(null, "foo"));
        }

        [Fact]
        public void NotNull_throws_when_arg_name_empty()
        {
            Assert.Throws<ArgumentException>(() => Check.NotNull(new object(), string.Empty));
        }

        [Fact]
        public void NotEmpty_throws_when_empty()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty("", string.Empty));
        }

        [Fact]
        public void NotEmpty_throws_when_whitespace()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty(" ", string.Empty));
        }

        [Fact]
        public void NotEmpty_throws_when_parameter_name_null()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty("42", string.Empty));
        }
    }
}

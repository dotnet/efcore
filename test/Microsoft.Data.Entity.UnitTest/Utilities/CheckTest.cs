// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using Xunit;

namespace Microsoft.Data.Entity.Utilities
{
    public class CheckTest
    {
        [Fact]
        public void NotNullThrowsWhenArgIsNull()
        {
// ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentNullException>(() => Check.NotNull<string>(null, "foo"));
        }

        [Fact]
        public void NotNullThrowsWhenArgNameEmpty()
        {
            Assert.Throws<ArgumentException>(() => Check.NotNull(new object(), string.Empty));
        }

        [Fact]
        public void NotEmptyThrowsWhenEmpty()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty("", string.Empty));
        }

        [Fact]
        public void NotEmptyThrowsWhenWhitespace()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty(" ", string.Empty));
        }

        [Fact]
        public void NotEmptyThrowsWhenParameterNameNull()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty("42", string.Empty));
        }

        [Fact]
        public void IsDefinedThrowsWhenEnumOutOfRange()
        {
// ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentException>(() => Check.IsDefined((EntityState)42, "foo"));
        }
    }
}

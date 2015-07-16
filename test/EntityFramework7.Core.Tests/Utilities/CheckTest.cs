// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class CheckTest
    {
        [Fact]
        public void Not_null_throws_when_arg_is_null()
        {
            // ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentNullException>(() => Check.NotNull<string>(null, "foo"));
        }

        [Fact]
        public void Not_null_throws_when_arg_name_empty()
        {
            Assert.Throws<ArgumentException>(() => Check.NotNull(null as object, string.Empty));
        }

        [Fact]
        public void Not_empty_throws_when_empty()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty("", string.Empty));
        }

        [Fact]
        public void Not_empty_throws_when_whitespace()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty(" ", string.Empty));
        }

        [Fact]
        public void Not_empty_throws_when_parameter_name_null()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => Check.NotEmpty(null, null));
        }

        [Fact]
        public void Generic_Not_empty_throws_when_arg_is_empty()
        {
            // ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentException>(() => Check.NotEmpty(new string[] { }, "foo"));
        }

        [Fact]
        public void Generic_Not_empty_throws_when_arg_is_null()
        {
            // ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentNullException>(() => Check.NotEmpty<object>(null, "foo"));
        }

        [Fact]
        public void Generic_Not_empty_throws_when_arg_name_empty()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty(null, string.Empty));
        }

        [Fact]
        public void Is_defined_throws_when_enum_out_of_range()
        {
            // ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentException>(() => Check.IsDefined((EntityState)42, "foo"));
        }
    }
}

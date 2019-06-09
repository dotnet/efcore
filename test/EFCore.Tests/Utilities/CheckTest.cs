// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using Xunit;

// ReSharper disable InconsistentNaming
namespace Microsoft.EntityFrameworkCore.Utilities
{
    public class CheckTest
    {
        [ConditionalFact]
        public void Not_null_throws_when_arg_is_null()
        {
            // ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentNullException>(() => Check.NotNull<string>(null, "foo"));
        }

        [ConditionalFact]
        public void Not_null_throws_when_arg_name_empty()
        {
            Assert.Throws<ArgumentException>(() => Check.NotNull(null as object, string.Empty));
        }

        [ConditionalFact]
        public void Not_empty_throws_when_empty()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty("", string.Empty));
        }

        [ConditionalFact]
        public void Not_empty_throws_when_whitespace()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty(" ", string.Empty));
        }

        [ConditionalFact]
        public void Not_empty_throws_when_parameter_name_null()
        {
            // ReSharper disable once AssignNullToNotNullAttribute
            Assert.Throws<ArgumentNullException>(() => Check.NotEmpty(null, null));
        }

        [ConditionalFact]
        public void Generic_Not_empty_throws_when_arg_is_empty()
        {
            // ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentException>(() => Check.NotEmpty(Array.Empty<string>(), "foo"));
        }

        [ConditionalFact]
        public void Generic_Not_empty_throws_when_arg_is_null()
        {
            // ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentNullException>(() => Check.NotEmpty<object>(null, "foo"));
        }

        [ConditionalFact]
        public void Generic_Not_empty_throws_when_arg_name_empty()
        {
            Assert.Throws<ArgumentException>(() => Check.NotEmpty(null, string.Empty));
        }
    }
}

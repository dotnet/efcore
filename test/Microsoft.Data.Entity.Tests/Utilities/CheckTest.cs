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
            Assert.Throws<ArgumentException>(() => Check.NotNull(new object(), string.Empty));
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
            Assert.Throws<ArgumentException>(() => Check.NotEmpty("42", string.Empty));
        }

        [Fact]
        public void Is_defined_throws_when_enum_out_of_range()
        {
            // ReSharper disable once NotResolvedInText
            Assert.Throws<ArgumentException>(() => Check.IsDefined((EntityState)42, "foo"));
        }
    }
}

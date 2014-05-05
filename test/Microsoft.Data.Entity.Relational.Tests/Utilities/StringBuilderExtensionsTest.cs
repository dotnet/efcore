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

using System.Text;
using Xunit;

namespace Microsoft.Data.Entity.Relational.Tests.Utilities
{
    public class StringBuilderExtensionsTest
    {
        [Fact]
        public void AppendJoin_joins_values()
        {
            Assert.Equal("a:b:c", new StringBuilder().AppendJoin(new[] { "a", "b", "c" }, ":").ToString());
            Assert.Equal("abc", new StringBuilder().AppendJoin(new[] { "a", "b", "c" }, string.Empty).ToString());
            Assert.Empty(new StringBuilder().AppendJoin(new string[0], ":").ToString());

            Assert.Equal(
                "11, 22, 33",
                new StringBuilder().AppendJoin(new[] { 1, 2, 3 }, (sb, v) => sb.Append(v).Append(v), ", ").ToString());
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Text;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Utilities
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

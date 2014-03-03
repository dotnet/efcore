// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System;
using System.Text;
using Xunit;

namespace Microsoft.Data.Relational.Utilities
{
    public class StringBuilderExtensionsTest
    {
        [Fact]
        public void AppendJoin_joins_values()
        {
            Assert.Equal("a:b:c", new StringBuilder().AppendJoin(new [] {"a", "b", "c"}, ":").ToString());
            Assert.Equal("abc", new StringBuilder().AppendJoin(new[] { "a", "b", "c" }, string.Empty).ToString());
            Assert.Empty(new StringBuilder().AppendJoin(new string[0], ":").ToString());

            Assert.Equal(
                "11, 22, 33", 
                new StringBuilder().AppendJoin(new [] {1, 2, 3}, (sb, v) => sb.Append(v).Append(v), ", ").ToString());
        }

        [Fact]
        public void AppendJoin_checks_parameters_not_null()
        {
            Assert.Equal(
                "stringBuilder",
                Assert.Throws<ArgumentNullException>(
                    () => StringBuilderExtensions.AppendJoin(null, new string[0], ",")).ParamName);

            Assert.Equal(
                "stringBuilder",
                Assert.Throws<ArgumentNullException>(
                    () => StringBuilderExtensions.AppendJoin(null, new string[0], (b, s) => { }, ",")).ParamName);

            var sb = new StringBuilder();

            Assert.Equal("values", Assert.Throws<ArgumentNullException>(() => sb.AppendJoin(null, ",")).ParamName);
            Assert.Equal("separator", Assert.Throws<ArgumentNullException>(() => sb.AppendJoin(new string[0], null)).ParamName);
            
            Assert.Equal("values", Assert.Throws<ArgumentNullException>(() => sb.AppendJoin((string[])null, (b, s) => { }, ",")).ParamName);
            Assert.Equal("joinAction", Assert.Throws<ArgumentNullException>(() => sb.AppendJoin(new string[0], null, ",")).ParamName);
            Assert.Equal("separator", Assert.Throws<ArgumentNullException>(() => sb.AppendJoin(new string[0], (b, s) => { }, null)).ParamName);

        }

    }
}

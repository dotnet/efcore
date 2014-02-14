// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Linq;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.Entity.Utilities
{
    public class EnumerableExtensionsTest
    {
        [Fact]
        public void OrderByOrdinalShouldRespectCase()
        {
            Assert.Equal(new[] { "A", "a", "b" }, new[] { "b", "A", "a" }.OrderByOrdinal(s => s));
        }

        [Fact]
        public void JoinEmptyInputReturnsEmptyString()
        {
            Assert.Equal("", new object[] { }.Join());
        }

        [Fact]
        public void JoinSingleElementDoesNotUseSeparator()
        {
            Assert.Equal("42", new object[] { 42 }.Join());
        }

        [Fact]
        public void JoinShouldUseCommaByDefault()
        {
            Assert.Equal("42, bar", new object[] { 42, "bar" }.Join());
        }

        [Fact]
        public void JoinShouldUseExplicitSeparatorWhenProvided()
        {
            Assert.Equal("42-bar", new object[] { 42, "bar" }.Join("-"));
        }

        [Fact]
        public async Task SelectAsyncShouldApplySelectorOverSequence()
        {
            Assert.Equal(
                new[] { "aa", "bb" },
                await new[] { "AA", "BB" }.SelectAsync(s => Task.FromResult(s.ToLower())));
        }

        [Fact]
        public async Task SelectManyAsyncShouldApplySelectorOverInnerSequences()
        {
            Assert.Equal(
                new[] { "a", "b", "c", "d" },
                await new[] { "ab", "cd" }
                    .SelectManyAsync(s => Task.FromResult(s.Select(c => c.ToString()))));
        }

        [Fact]
        public async Task WhereAsyncShouldApplyFilterOverSequence()
        {
            Assert.Equal(
                new[] { "BB" },
                await new[] { "AA", "BB" }.WhereAsync(s => Task.FromResult(s.StartsWith("B"))));
        }
    }
}

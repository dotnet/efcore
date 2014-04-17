// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Threading.Tasks;
using Microsoft.Data.Entity.Query;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Query
{
    public class CompletedAsyncEnumerableTest
    {
        [Fact]
        public async Task Can_async_wrap_enumerable()
        {
            var completedAsyncEnumerable
                = new CompletedAsyncEnumerable<int>(new[] { 1, 2, 3 });

            Assert.Equal(new[] { 1, 2, 3 }, await completedAsyncEnumerable.ToArrayAsync());
        }
    }
}

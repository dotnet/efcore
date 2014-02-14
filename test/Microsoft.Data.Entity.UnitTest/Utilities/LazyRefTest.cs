// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace Microsoft.Data.Entity.Utilities
{
    public class LazyRefTest
    {
        [Fact]
        public async Task CanInitializeFromMultipleThreadsAndInitializationHappensOnlyOnce()
        {
            var counter = 0;
            var safeLazy = new LazyRef<string>(() => counter++.ToString());
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => safeLazy.Value));
            }

            await Task.WhenAll(tasks);

            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task CanExchangeValue()
        {
            var safeLazy = new LazyRef<string>(() => "");
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => safeLazy.ExchangeValue(s => s + "s")));
            }

            await Task.WhenAll(tasks);

            Assert.Equal("ssssssssss", safeLazy.Value);
        }

        [Fact]
        public void HasValueIsFalseUntilValueAccessed()
        {
            var safeLazy = new LazyRef<string>(() => "s");

            Assert.False(safeLazy.HasValue);
            Assert.Equal("s", safeLazy.Value);
            Assert.True(safeLazy.HasValue);
        }
    }
}

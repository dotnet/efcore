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

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class ThreadSafeLazyRefTest
    {
        [Fact]
        public async Task Can_initialize_from_multiple_threads_and_initialization_happens_only_once()
        {
            var counter = 0;
            var safeLazy = new ThreadSafeLazyRef<string>(() => counter++.ToString());
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => safeLazy.Value));
            }

            await Task.WhenAll(tasks);

            Assert.Equal(1, counter);
        }

        [Fact]
        public async Task Can_exchange_value()
        {
            var safeLazy = new ThreadSafeLazyRef<string>(() => "");
            var tasks = new List<Task>();

            for (var i = 0; i < 10; i++)
            {
                tasks.Add(Task.Run(() => safeLazy.ExchangeValue(s => s + "s")));
            }

            await Task.WhenAll(tasks);

            Assert.Equal("ssssssssss", safeLazy.Value);
        }

        [Fact]
        public void Has_value_is_false_until_value_accessed()
        {
            var safeLazy = new ThreadSafeLazyRef<string>(() => "s");

            Assert.False(safeLazy.HasValue);
            Assert.Equal("s", safeLazy.Value);
            Assert.True(safeLazy.HasValue);
        }
    }
}

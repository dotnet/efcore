// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System.Threading;
using System.Threading.Tasks;
using Microsoft.Data.Entity.Query;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Query
{
    public class TaskResultAsyncEnumerableTest
    {
        [Fact]
        public async Task Can_async_wrap_task()
        {
            var taskAdapterAsyncEnumerable
                = new TaskResultAsyncEnumerable<int>(Task.Delay(100).ContinueWith(_ => 42));

            var enumerator = taskAdapterAsyncEnumerable.GetEnumerator();

            Assert.Equal(default(int), enumerator.Current);
            Assert.True(await enumerator.MoveNext(default(CancellationToken)));
            Assert.Equal(42, enumerator.Current);
            Assert.False(await enumerator.MoveNext(default(CancellationToken)));
        }

        [Fact]
        public async Task Can_async_wrap_completed_task()
        {
            var taskAdapterAsyncEnumerable
                = new TaskResultAsyncEnumerable<int>(Task.Run(() => 42));

            var enumerator = taskAdapterAsyncEnumerable.GetEnumerator();

            Assert.Equal(default(int), enumerator.Current);
            Assert.True(await enumerator.MoveNext(default(CancellationToken)));
            Assert.Equal(42, enumerator.Current);
            Assert.False(await enumerator.MoveNext(default(CancellationToken)));
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Internal;
using Microsoft.Data.Entity.Utilities;
using Xunit;

namespace Microsoft.Data.Entity.Tests.Utilities
{
    public class ThreadSafeDictionaryCacheTest
    {
        [Fact]
        public void Creates_new_instance_or_returns_cached_instance_as_appropriate()
        {
            var cache = new ThreadSafeDictionaryCache<int, string>();

            Assert.Equal("Cheese", cache.GetOrAdd(1, k => "Cheese"));
            Assert.Equal("Cheese", cache.GetOrAdd(1, k => "Pickle"));
            Assert.Equal("Pickle", cache.GetOrAdd(2, k => "Pickle"));
        }
    }
}

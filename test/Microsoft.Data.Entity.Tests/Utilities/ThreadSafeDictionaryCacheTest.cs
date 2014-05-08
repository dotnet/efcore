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

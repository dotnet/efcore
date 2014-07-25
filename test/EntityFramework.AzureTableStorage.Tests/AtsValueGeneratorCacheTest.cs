// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.Data.Entity.Identity;
using Microsoft.Data.Entity.Metadata;
using Moq;
using Xunit;

namespace Microsoft.Data.Entity.AzureTableStorage.Tests
{
    public class AtsValueGeneratorCacheTest
    {
        private readonly AtsValueGeneratorCache _cache
            = new AtsValueGeneratorCache(Mock.Of<ValueGeneratorSelector>(), Mock.Of<ForeignKeyValueGenerator>());

        [Fact]
        public void It_returns_null()
        {
            var generator = _cache.GetGenerator(Mock.Of<IProperty>());
            Assert.Null(generator);
        }
    }
}

// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using Microsoft.EntityFrameworkCore.Storage;
using Xunit;

namespace Microsoft.EntityFrameworkCore.Relational.Tests.Storage
{
    public class TypedRelationalValueBufferFactoryFactoryTest
    {
        [Fact]
        public void Cache_key_comparison_when_null_index_map()
        {
            var typedRelationalValueBufferFactoryFactory
                = new TypedRelationalValueBufferFactoryFactory();

            var factory1
                = typedRelationalValueBufferFactoryFactory.Create(new[] { typeof(string) }, new[] { 42 });

            Assert.NotNull(factory1);

            var factory2
                = typedRelationalValueBufferFactoryFactory.Create(new[] { typeof(string) }, null);

            Assert.NotSame(factory1, factory2);
        }
    }
}
